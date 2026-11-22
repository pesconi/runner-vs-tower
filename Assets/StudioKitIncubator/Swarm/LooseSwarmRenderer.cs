using JungleDegree.StudioKit.Toon;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// Draws loose units, each placed by its owner (no formation): units sent off one by one, each with its own size and
    /// tint, that hop and lean like a <see cref="SwarmRenderer"/>'s and die the same ballistic death (a
    /// <see cref="SwarmDyingRing"/>). Same look and cost model as the swarm: no per-unit object, one structured buffer,
    /// one call for the bodies (live, then dying) and one for the blob shadows of the live ones. Units are kept in a dense
    /// array: a removal moves the last unit into the freed index (the owner mirrors it in its own arrays). Everything is
    /// allocated when enabled (scene load) and released when disabled; nothing is allocated while playing.
    /// </summary>
    public sealed class LooseSwarmRenderer : MonoBehaviour
    {
        private const int JobBatch = 64;

        [SerializeField] private Mesh unitMesh;
        [Tooltip("StudioKit/ToonLit with the instances keyword on.")]
        [SerializeField] private Material unitMaterial;
        [SerializeField] private Mesh shadowMesh;
        [Tooltip("StudioKit/BlobShadowInstanced (optional: no shadows without it).")]
        [SerializeField] private Material shadowMaterial;
        [Min(1)] [SerializeField] private int capacity = 300;
        [Tooltip("Most units on their way out at once (ballistic death); when full, the oldest goes first.")]
        [Min(0)] [SerializeField] private int dyingCapacity = 512;
        [Tooltip("Longest frame step of the animation (s), as the swarm.")]
        [Min(0.001f)] [SerializeField] private float maxDeltaTime = 0.05f;
        [SerializeField] private SwarmMotionSettings motion = SwarmMotionSettings.Default;
        [SerializeField] private SwarmAnimationSettings unitAnimation = SwarmAnimationSettings.Default;
        [SerializeField] private SwarmDeathSettings death = SwarmDeathSettings.Default;
        [SerializeField] private uint seed = 7;

        private NativeArray<float4> placements; // x, z (world), across speed (m/s), size
        private NativeArray<float4> looks;      // linear tint rgb, hop phase
        private NativeArray<ToonInstance> instances;
        private SwarmDyingRing dying;
        private Unity.Mathematics.Random random;
        private GraphicsBuffer buffer;
        private RenderParams unitParams;
        private RenderParams shadowParams;
        private Bounds area;
        private bool drawShadows;
        private bool headless;
        private bool ready;

        public int Capacity => capacity;
        public int DyingCapacity => dyingCapacity;

        /// <summary>Live units (indices 0 to Count − 1).</summary>
        public int Count { get; private set; }

        /// <summary>Units on their way out now.</summary>
        public int DyingCount => dying == null ? 0 : dying.Count;

        public Mesh UnitMesh => unitMesh;
        public int UnitTriangles => unitMesh == null ? 0 : (int)(unitMesh.GetIndexCount(0) / 3);

        /// <summary>True without a graphics device (batchmode -nographics): units are kept but nothing is drawn.</summary>
        public bool Headless => headless;

        /// <summary>
        /// Where the live units can be (the culling box of the draws; the dying grow it by how far they fly). Set it when
        /// the play area changes.
        /// </summary>
        public Bounds Area
        {
            get => area;
            set => area = value;
        }

        /// <summary>Adds a unit at (<paramref name="x"/>, <paramref name="z"/>); false when full (nothing changes).</summary>
        public bool TryAdd(float x, float z, float size, Color tint)
        {
            if (!ready || Count >= capacity)
            {
                return false;
            }

            Color linear = tint.linear;
            placements[Count] = new float4(x, z, 0f, size);
            looks[Count] = new float4(linear.r, linear.g, linear.b, random.NextFloat(0f, 2f * math.PI));
            Count++;
            return true;
        }

        /// <summary>Moves unit <paramref name="index"/>; <paramref name="acrossSpeed"/> (m/s) makes it lean into the move.</summary>
        public void Place(int index, float x, float z, float acrossSpeed)
        {
            float4 p = placements[index];
            placements[index] = new float4(x, z, acrossSpeed, p.w);
        }

        /// <summary>Where unit <paramref name="index"/> stands (world x, z).</summary>
        public Vector2 PositionOf(int index)
        {
            float4 p = placements[index];
            return new Vector2(p.x, p.y);
        }

        /// <summary>Drawn size of unit <paramref name="index"/> (1 = the mesh as modeled).</summary>
        public float SizeOf(int index)
        {
            return placements[index].w;
        }

        /// <summary>Removes unit <paramref name="index"/> at once; the last unit takes its index.</summary>
        public void RemoveAt(int index)
        {
            int last = Count - 1;
            placements[index] = placements[last];
            looks[index] = looks[last];
            Count = last;
        }

        /// <summary>
        /// Removes unit <paramref name="index"/> in the ballistic death, thrown along <paramref name="outward"/> (x across,
        /// y along the run); the last unit takes its index.
        /// </summary>
        public void KillAt(int index, Vector2 outward)
        {
            float4 p = placements[index];
            float4 look = looks[index];
            Push(p.x, 0f, p.y, p.w, look.xyz, outward);
            RemoveAt(index);
        }

        /// <summary>
        /// A unit of <paramref name="size"/> and <paramref name="tint"/> dies at (<paramref name="x"/>,
        /// <paramref name="z"/>) without a live one going (a unit that is reused at once, a burst of units).
        /// </summary>
        public void Throw(float x, float z, float size, Color tint, Vector2 outward)
        {
            Throw(x, 0f, z, size, tint, outward);
        }

        /// <summary>As <see cref="Throw(float, float, float, Color, Vector2)"/>, from <paramref name="height"/> above the ground.</summary>
        public void Throw(float x, float height, float z, float size, Color tint, Vector2 outward)
        {
            Color linear = tint.linear;
            Push(x, height, z, size, new float3(linear.r, linear.g, linear.b), outward);
        }

        /// <summary>Every live unit gone at once and no one dying (a restart).</summary>
        public void Clear()
        {
            Count = 0;
            dying?.Clear();
        }

        /// <summary>Swaps the unit shape (a mesh with one submesh); the next frame draws it, nothing is reallocated.</summary>
        public void SetUnitMesh(Mesh mesh)
        {
            if (mesh == null || mesh.subMeshCount < 1)
            {
                Debug.LogError("[LooseSwarm] the unit mesh must exist and have a submesh", this);
                return;
            }

            unitMesh = mesh;
        }

        private void Push(float x, float height, float z, float size, float3 tint, Vector2 outward)
        {
            if (dying == null)
            {
                return;
            }

            float2 direction = math.lengthsq(new float2(outward.x, outward.y)) > 1e-8f ? new float2(outward.x, outward.y) : new float2(0f, -1f);
            SwarmDyingUnit unit = SwarmDying.Launch(new float2(x, z), direction, Time.time, tint, size, death, ref random);
            unit.Position.y = math.max(0f, height);
            dying.Push(unit);
        }

        private void OnEnable()
        {
            string error = Validate();
            if (error != null)
            {
                Debug.LogError("[LooseSwarm] " + error, this);
                enabled = false;
                return;
            }

            placements = new NativeArray<float4>(capacity, Allocator.Persistent);
            looks = new NativeArray<float4>(capacity, Allocator.Persistent);
            instances = new NativeArray<ToonInstance>(capacity + dyingCapacity, Allocator.Persistent);
            dying = new SwarmDyingRing(dyingCapacity);
            random = new Unity.Mathematics.Random(seed == 0 ? 1u : seed);
            Count = 0;
            ready = true;
            headless = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
            if (headless)
            {
                Debug.Log("[LooseSwarm] no graphics device: keeping units without drawing", this);
                return;
            }

            buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity + dyingCapacity, ToonInstance.Stride);
            unitParams = new RenderParams(unitMaterial)
            {
                matProps = new MaterialPropertyBlock(),
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
                layer = gameObject.layer,
            };
            unitParams.matProps.SetBuffer(ToonShaderIds.Instances, buffer);
            drawShadows = shadowMaterial != null && shadowMesh != null;
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
        }

        private void OnDisable()
        {
            ready = false;
            Count = 0;
            dying?.Dispose();
            dying = null;
            if (placements.IsCreated)
            {
                placements.Dispose();
                looks.Dispose();
                instances.Dispose();
            }

            buffer?.Release();
            buffer = null;
        }

        // The owner moves its units in Update; they are animated and drawn after.
        private void LateUpdate()
        {
            float time = Time.time;
            float deltaTime = Mathf.Min(Time.deltaTime, maxDeltaTime);
            dying.DropExpired(time, death.Lifetime);
            int count = Count;
            int dyingCount = dying.Count;
            int drawn = count + dyingCount;
            if (headless || drawn == 0)
            {
                return;
            }

            var animate = new LooseSwarmAnimateJob
            {
                Placements = placements,
                Looks = looks,
                Instances = instances,
                Motion = motion,
                Animation = unitAnimation,
                Time = time,
            };
            JobHandle handle = animate.Schedule(count, JobBatch);
            handle = dying.ScheduleFly(instances.GetSubArray(count, dyingCount), deltaTime, time, 0f, death, handle);
            handle.Complete();

            buffer.SetData(instances, 0, 0, drawn);
            Bounds bounds = area;
            dying.Encapsulate(ref bounds, 0f, area.max.y, death);
            unitParams.worldBounds = bounds;
            Graphics.RenderMeshPrimitives(unitParams, unitMesh, 0, drawn);
            if (drawShadows && count > 0)
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

    /// <summary>
    /// Writes the drawn transform of every loose unit for this frame: its hop and squash (as the swarm's), the forward
    /// lean, the sideways lean into its across speed, all scaled by its size; tint alpha is the blob shadow scale.
    /// </summary>
    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct LooseSwarmAnimateJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float4> Placements;
        [ReadOnly] public NativeArray<float4> Looks;
        [WriteOnly] public NativeArray<ToonInstance> Instances;
        public SwarmMotionSettings Motion;
        public SwarmAnimationSettings Animation;
        public float Time;

        public void Execute(int index)
        {
            float4 p = Placements[index];
            float4 look = Looks[index];
            float size = p.w;
            float bounce = SwarmMotion.Bounce(Time, look.w, Motion.Frequency);
            float sy = SwarmMotion.SquashY(bounce, Motion);
            float sxz = SwarmMotion.SquashXZ(sy);
            var instance = new ToonInstance
            {
                Tint = new float4(look.xyz, SwarmAnimation.ShadowScale(bounce, 1f, Animation) * size),
            };
            SwarmAnimation.Compose(ref instance, new float3(p.x, bounce * Motion.HopHeight * size, p.y),
                Motion.ForwardLean, SwarmAnimation.Tilt(p.z, Animation), sxz * size, sy * size);
            Instances[index] = instance;
        }
    }
}
