using System;
using JungleDegree.StudioKit.Toon;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>One kind of side prop: its mesh (vertex colors weigh the three slots), how many can and do stand, their scale.</summary>
    [Serializable]
    public struct PropLayer
    {
        [Tooltip("One mesh, one submesh; its vertex color RGB weigh the slot colors (StudioKit/ToonLit slots variant).")]
        public Mesh Mesh;
        [Tooltip("Most props of this kind (allocated once).")]
        [Min(0)] public int Capacity;
        [Tooltip("Props shown now (up to the capacity); SetLayer changes it.")]
        [Min(0)] public int Count;
        [Tooltip("Smallest and largest uniform scale.")]
        public Vector2 Scale;
        public Color Slot0;
        public Color Slot1;
        public Color Slot2;
    }

    /// <summary>
    /// Side scenery with no object per prop: each <see cref="PropLayer"/> is drawn by its own <see cref="ToonInstanceDraw"/>
    /// (<c>StudioKit/ToonLit</c> with the instances and slots keywords; one call from a structured buffer, or one per block of
    /// matrices on the fallback route of <see cref="ToonInstanceRouting"/>), its props scattered by
    /// <see cref="PropScatter"/> in a window that follows the anchor (the camera) and recycled to the far end when the
    /// anchor passes them — an endless roadside from a fixed number of instances. Buffers are allocated when enabled
    /// (scene load) and released when disabled; switching worlds is <see cref="SetLayer"/> (count and slot colors),
    /// nothing is created or destroyed. Transforms are rebuilt only on frames where the anchor moved or a layer changed.
    /// </summary>
    public sealed class InstancedProps : MonoBehaviour
    {
        [Tooltip("What the window follows along Z (usually the camera).")]
        [SerializeField] private Transform anchor;
        [Tooltip("StudioKit/ToonLit with the instances and slots keywords on.")]
        [SerializeField] private Material material;
        [SerializeField] private PropLayer[] layers = new PropLayer[0];
        [SerializeField] private PropScatterSettings scatter = PropScatterSettings.Default;
        [SerializeField] private uint seed = 1;
        [Tooltip("Height of the drawn bounds (m): the tallest prop at its largest scale fits under it.")]
        [Min(0.1f)] [SerializeField] private float boundsHeight = 8f;

        private NativeArray<ToonInstance>[] instances;
        private ToonInstanceDraw[] draws;
        private bool allocated;
        private bool headless;
        private bool dirty = true;
        private float builtAnchorZ;

        public int LayerCount => layers == null ? 0 : layers.Length;
        public PropScatterSettings Scatter => scatter;
        public Transform Anchor => anchor;

        /// <summary>
        /// Instanced draws per frame on the route in use: one per layer with props shown on the buffer route, one per block
        /// of <see cref="ToonInstanceDraw.ChunkSize"/> on the fallback route.
        /// </summary>
        public int DrawCalls
        {
            get
            {
                int calls = 0;
                for (int i = 0; i < LayerCount; i++)
                {
                    calls += ToonInstanceDraw.Calls(ToonInstanceRouting.Current, layers[i].Count);
                }

                return calls;
            }
        }

        public PropLayer Layer(int layer) => layers[layer];

        /// <summary>Null when the component is usable, otherwise what is wrong with it.</summary>
        public string Validate()
        {
            if (anchor == null || material == null)
            {
                return "the props need an anchor and a material";
            }

            if (!material.IsKeywordEnabled(ToonShaderIds.InstancesKeyword) || !material.IsKeywordEnabled(ToonShaderIds.SlotsKeyword))
            {
                return $"{material.name} must have {ToonShaderIds.InstancesKeyword} and {ToonShaderIds.SlotsKeyword} on";
            }

            if (layers == null || layers.Length == 0)
            {
                return "the props need at least one layer";
            }

            for (int i = 0; i < layers.Length; i++)
            {
                PropLayer layer = layers[i];
                if (layer.Mesh == null || layer.Mesh.subMeshCount < 1 || layer.Capacity < 1 || layer.Count < 0 || layer.Count > layer.Capacity
                    || !(layer.Scale.x > 0f) || layer.Scale.y < layer.Scale.x)
                {
                    return $"prop layer {i} needs a mesh, a capacity of 1 or more, a count within it and a scale range above 0";
                }
            }

            return ToonInstanceDraw.ValidateMaterial(material) ?? scatter.Validate();
        }

        /// <summary>
        /// Shows <paramref name="count"/> props of layer <paramref name="layer"/> (clamped to its capacity) with these slot
        /// colors (vertex red, green, blue). Nothing is allocated; the next draw uses them.
        /// </summary>
        public void SetLayer(int layer, int count, Color slot0, Color slot1, Color slot2)
        {
            if (layers == null || layer < 0 || layer >= layers.Length)
            {
                return;
            }

            PropLayer data = layers[layer];
            data.Count = Mathf.Clamp(count, 0, data.Capacity);
            data.Slot0 = slot0;
            data.Slot1 = slot1;
            data.Slot2 = slot2;
            layers[layer] = data;
            if (allocated && !headless)
            {
                SetSlots(draws[layer], data);
            }

            dirty = true;
        }

        /// <summary>Where prop <paramref name="index"/> of <paramref name="layer"/> stands for the anchor now (tools and tests).</summary>
        public Vector3 PropPosition(int layer, int index)
        {
            ToonInstance placed = PropScatter.Place(scatter, seed, layer, index, AnchorZ, (float2)(Vector2)layers[layer].Scale);
            float3 p = placed.Position;
            return new Vector3(p.x, p.y, p.z);
        }

        /// <summary>
        /// Places and draws every layer for <paramref name="camera"/> (null = every camera). Called each frame from
        /// <c>LateUpdate</c>; tools that render a scene in edit mode call it right before rendering (it allocates on first
        /// use there) and <see cref="Release"/> after.
        /// </summary>
        public void Render(Camera camera)
        {
            if (!allocated && !Allocate())
            {
                return;
            }

            if (!headless && draws[0].Stale)
            {
                // A route was forced (tests, measurement builds): the other route's draws, uploaded again below.
                CreateDraws();
                dirty = true;
            }

            float anchorZ = AnchorZ;
            if (dirty || anchorZ != builtAnchorZ)
            {
                Build(anchorZ);
            }

            if (headless)
            {
                return;
            }

            float start = PropScatter.WindowStart(scatter, anchorZ);
            float across = 2f * (scatter.InnerX + scatter.BandWidth) + 8f;
            var bounds = new Bounds(new Vector3(scatter.CenterX, boundsHeight * 0.5f, start + scatter.Length * 0.5f),
                new Vector3(across, boundsHeight, scatter.Length + 8f));
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].Count == 0)
                {
                    continue;
                }

                draws[i].Draw(layers[i].Mesh, layers[i].Count, bounds, camera);
            }
        }

        /// <summary>Frees the buffers (done when disabled; tools call it after an edit-mode <see cref="Render"/>).</summary>
        public void Release()
        {
            DisposeDraws();
            if (instances != null)
            {
                foreach (NativeArray<ToonInstance> placed in instances)
                {
                    if (placed.IsCreated)
                    {
                        placed.Dispose();
                    }
                }
            }

            instances = null;
            allocated = false;
            dirty = true;
        }

        private float AnchorZ => anchor == null ? transform.position.z : anchor.position.z;

        private void OnEnable()
        {
            Allocate();
        }

        private void OnDisable()
        {
            Release();
        }

        private void LateUpdate()
        {
            Render(null);
        }

        private bool Allocate()
        {
            string error = Validate();
            if (error != null)
            {
                Debug.LogError("[InstancedProps] " + error, this);
                enabled = false;
                return false;
            }

            headless = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
            instances = new NativeArray<ToonInstance>[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                instances[i] = new NativeArray<ToonInstance>(layers[i].Capacity, Allocator.Persistent);
            }

            if (!headless)
            {
                CreateDraws();
            }

            allocated = true;
            dirty = true;
            return true;
        }

        // Scene load, or the frame after a route was forced.
        private void CreateDraws()
        {
            DisposeDraws();
            draws = new ToonInstanceDraw[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                draws[i] = new ToonInstanceDraw(material, null, layers[i].Capacity, gameObject.layer);
                SetSlots(draws[i], layers[i]);
            }
        }

        private void DisposeDraws()
        {
            if (draws != null)
            {
                foreach (ToonInstanceDraw draw in draws)
                {
                    draw?.Dispose();
                }
            }

            draws = null;
        }

        private void Build(float anchorZ)
        {
            for (int l = 0; l < layers.Length; l++)
            {
                PropLayer layer = layers[l];
                NativeArray<ToonInstance> placed = instances[l];
                var scale = new float2(layer.Scale.x, layer.Scale.y);
                for (int i = 0; i < layer.Count; i++)
                {
                    placed[i] = PropScatter.Place(scatter, seed, l, i, anchorZ, scale);
                }

                if (!headless && layer.Count > 0)
                {
                    draws[l].Upload(placed, layer.Count);
                }
            }

            builtAnchorZ = anchorZ;
            dirty = false;
        }

        // The shader mixes the slots as linear colors (vertex weights are not colors, so nothing converts them there).
        private static void SetSlots(ToonInstanceDraw draw, PropLayer layer)
        {
            draw.SetVector(ToonShaderIds.Slot0, ToonShaderIds.ShaderColor(layer.Slot0));
            draw.SetVector(ToonShaderIds.Slot1, ToonShaderIds.ShaderColor(layer.Slot1));
            draw.SetVector(ToonShaderIds.Slot2, ToonShaderIds.ShaderColor(layer.Slot2));
        }
    }
}
