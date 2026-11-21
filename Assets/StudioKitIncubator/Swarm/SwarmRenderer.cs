using JungleDegree.StudioKit.Toon;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// Draws a swarm with no per-unit object: a <see cref="SwarmSimulation"/> animated by Burst every frame, uploaded
    /// to one structured buffer and drawn in one call for the bodies (<c>StudioKit/ToonLit</c> with
    /// <see cref="ToonShaderIds.InstancesKeyword"/>) plus one for the blob shadows (<c>StudioKit/BlobShadowInstanced</c>),
    /// both reading the same buffer. Everything is allocated when enabled (scene load) and released when disabled.
    /// </summary>
    public sealed class SwarmRenderer : MonoBehaviour
    {
        [SerializeField] private Mesh unitMesh;
        [Tooltip("StudioKit/ToonLit with the instances keyword on.")]
        [SerializeField] private Material unitMaterial;
        [SerializeField] private Mesh shadowMesh;
        [Tooltip("StudioKit/BlobShadowInstanced (optional: no shadows without it).")]
        [SerializeField] private Material shadowMaterial;
        [Min(1)] [SerializeField] private int capacity = 2048;
        [Min(0)] [SerializeField] private int initialCount = 100;
        [Min(0.01f)] [SerializeField] private float spacing = 0.25f;
        [Min(0.01f)] [SerializeField] private float depthStretch = 1.35f;
        [SerializeField] private Color tint = Color.white;
        [SerializeField] private SwarmMotionSettings motion = SwarmMotionSettings.Default;
        [SerializeField] private uint seed = 1;

        private SwarmSimulation simulation;
        private GraphicsBuffer buffer;
        private RenderParams unitParams;
        private RenderParams shadowParams;
        private bool drawShadows;
        private bool headless;

        public int Capacity => capacity;
        public float Spacing => spacing;
        public int Count => simulation == null ? 0 : simulation.Count;
        public int UnitTriangles => unitMesh == null ? 0 : (int)(unitMesh.GetIndexCount(0) / 3);
        public bool DrawsShadows => shadowMaterial != null && shadowMesh != null;

        /// <summary>True without a graphics device (batchmode -nographics): the swarm simulates but draws nothing.</summary>
        public bool Headless => headless;

        /// <summary>Sets the live count around the swarm's position (clamped to the capacity).</summary>
        public void SetCount(int count)
        {
            if (simulation != null)
            {
                Vector3 p = transform.position;
                simulation.SetCount(count, new float2(p.x, p.z));
            }
            else
            {
                initialCount = Mathf.Max(0, count);
            }
        }

        private void OnEnable()
        {
            string error = Validate();
            if (error != null)
            {
                Debug.LogError("[Swarm] " + error, this);
                enabled = false;
                return;
            }

            simulation = new SwarmSimulation(capacity, spacing, depthStretch, seed);
            headless = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
            if (headless)
            {
                Debug.Log("[Swarm] no graphics device: simulating without drawing", this);
                SetCount(initialCount);
                return;
            }

            buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, ToonInstance.Stride);
            unitParams = new RenderParams(unitMaterial)
            {
                matProps = new MaterialPropertyBlock(),
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
                layer = gameObject.layer,
            };
            unitParams.matProps.SetBuffer(ToonShaderIds.Instances, buffer);
            drawShadows = DrawsShadows;
            if (drawShadows)
            {
                shadowParams = new RenderParams(shadowMaterial)
                {
                    matProps = unitParams.matProps,
                    shadowCastingMode = ShadowCastingMode.Off,
                    receiveShadows = false,
                    layer = gameObject.layer,
                };
            }

            SetCount(initialCount);
        }

        private void OnDisable()
        {
            simulation?.Dispose();
            simulation = null;
            buffer?.Release();
            buffer = null;
        }

        private void Update()
        {
            if (simulation.Count == 0)
            {
                return;
            }

            Color linear = tint.linear;
            simulation.Schedule(Time.time, motion, new float3(linear.r, linear.g, linear.b));
        }

        private void LateUpdate()
        {
            int count = simulation.Count;
            if (count == 0)
            {
                return;
            }

            simulation.Complete();
            if (headless)
            {
                return;
            }

            buffer.SetData(simulation.Instances, 0, 0, count);
            Bounds bounds = simulation.GroundBounds(1.5f);
            unitParams.worldBounds = bounds;
            Graphics.RenderMeshPrimitives(unitParams, unitMesh, 0, count);
            if (drawShadows)
            {
                shadowParams.worldBounds = bounds;
                Graphics.RenderMeshPrimitives(shadowParams, shadowMesh, 0, count);
            }
        }

        private string Validate()
        {
            if (unitMesh == null || unitMaterial == null)
            {
                return "unit mesh and material are required";
            }

            if (!unitMaterial.IsKeywordEnabled(ToonShaderIds.InstancesKeyword))
            {
                return $"{unitMaterial.name} must have {ToonShaderIds.InstancesKeyword} on";
            }

            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null
                && (!SystemInfo.supportsInstancing || SystemInfo.maxComputeBufferInputsVertex < 1))
            {
                return $"this device cannot read a structured buffer in the vertex shader ({SystemInfo.graphicsDeviceType})";
            }

            return null;
        }
    }
}
