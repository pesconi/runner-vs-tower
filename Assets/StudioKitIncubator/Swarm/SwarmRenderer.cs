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
        [SerializeField] private SwarmFormationSettings formation = SwarmFormationSettings.Default;
        [Tooltip("Half width of the lane the formation is kept in, centered on this object across (0 = no lane).")]
        [Min(0f)] [SerializeField] private float laneHalfWidth;
        [Tooltip("Longest frame step of the simulation (s); longer frames (hitches, loading) are slowed down.")]
        [Min(0.001f)] [SerializeField] private float maxDeltaTime = 0.05f;
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
        public SwarmFormationSettings Formation => formation;
        public float LaneHalfWidth => laneHalfWidth;
        public int Count => simulation == null ? 0 : simulation.Count;

        /// <summary>Across position of the formation center now (it follows <see cref="TargetX"/> smoothly).</summary>
        public float CenterX => simulation == null ? transform.position.x : simulation.CenterX;

        /// <summary>Where the formation is being steered to, across.</summary>
        public float TargetX => simulation == null ? transform.position.x : simulation.TargetX;
        public Mesh UnitMesh => unitMesh;

        /// <summary>Body tint of every unit (where the mesh's vertex alpha is 255).</summary>
        public Color Tint => tint;
        public int UnitTriangles => unitMesh == null ? 0 : (int)(unitMesh.GetIndexCount(0) / 3);
        public bool DrawsShadows => shadowMaterial != null && shadowMesh != null;

        /// <summary>True without a graphics device (batchmode -nographics): the swarm simulates but draws nothing.</summary>
        public bool Headless => headless;

        /// <summary>Sets the live count (clamped to the capacity); new units appear in their slot of the formation.</summary>
        public void SetCount(int count)
        {
            if (simulation != null)
            {
                simulation.SetCount(count);
            }
            else
            {
                initialCount = Mathf.Max(0, count);
            }
        }

        /// <summary>
        /// Swaps the unit shape (a mesh with one submesh, like the one set in the Inspector); the next frame draws it.
        /// Nothing is reallocated: the instance buffer does not depend on the mesh.
        /// </summary>
        public void SetUnitMesh(Mesh mesh)
        {
            if (mesh == null || mesh.subMeshCount < 1)
            {
                Debug.LogError("[Swarm] the unit mesh must exist and have a submesh", this);
                return;
            }

            unitMesh = mesh;
        }

        /// <summary>Steers the formation center toward <paramref name="x"/> (world, across); it follows smoothly.</summary>
        public void SteerTo(float x)
        {
            simulation?.SteerTo(x);
        }

        /// <summary>Puts the formation center at <paramref name="x"/> at once (setup; the units walk to their slots).</summary>
        public void PlaceCenter(float x)
        {
            simulation?.PlaceCenter(x);
        }

        /// <summary>Where unit <paramref name="index"/> (below <see cref="Count"/>) stands now, on the ground.</summary>
        public Vector3 UnitGroundPosition(int index)
        {
            float2 p = simulation.GroundPosition(index);
            return new Vector3(p.x, 0f, p.y);
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

            Vector3 origin = transform.position;
            simulation = new SwarmSimulation(capacity, formation, seed);
            simulation.SetLane(origin.x, laneHalfWidth > 0f ? laneHalfWidth : SwarmSimulation.NoLane);
            simulation.PlaceCenter(origin.x);
            simulation.CenterZ = origin.z;
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
            simulation.CenterZ = transform.position.z;
            float deltaTime = Mathf.Min(Time.deltaTime, maxDeltaTime);
            Color linear = tint.linear;
            simulation.Schedule(deltaTime, Time.time, motion, new float3(linear.r, linear.g, linear.b));
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
