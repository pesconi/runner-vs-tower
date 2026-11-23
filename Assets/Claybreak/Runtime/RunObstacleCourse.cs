using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak
{
    /// <summary>
    /// The run's obstacles (RT-6t.18): the level's list (<see cref="ClaybreakLevel.Obstacles"/>), moved by
    /// <see cref="ObstacleMotion"/> on this course's clock, drawn instanced with <c>StudioKit/ToonLit</c> (one
    /// <see cref="ToonInstanceDraw"/> per kind, so one call per kind on both routes: at most
    /// <see cref="ClaybreakLevel.MaximumObstacles"/> instances, one block of matrices) and deadly to the units they touch.
    /// The saw's slot is a pit stretched across the track, drawn with the holes. Nothing is created while playing: the
    /// instance arrays and the draws are allocated when enabled (scene load), the level only re-places them.
    /// <para>
    /// Kills: the flow calls <see cref="Track"/> every frame of the run. For each deadly obstacle near the formation it tests
    /// the units' ground positions (no collider) and throws every unit inside out in the kit's ballistic death, from where it
    /// stood (<see cref="deaths"/>, a <see cref="LooseSwarmRenderer"/> with no live units), at most
    /// <see cref="maximumKillsPerFrame"/> a frame so the burst reads; the flow takes that many units off the army. A slot of the
    /// formation is hit once per obstacle (one bit each, so overlapping obstacles never hit a slot twice). The kit's swarm can only remove its outer ring, so the army loses outer units
    /// while the thrown ones fly from the obstacle (decision T-RT6t18-2; a kit delivery can remove the very unit hit).
    /// </para>
    /// </summary>
    // After the crowd moves (RunCrowd, -50) and before the flow tests the kills (-40): the clock is this frame's.
    [DefaultExecutionOrder(-45)]
    public sealed class RunObstacleCourse : MonoBehaviour
    {
        [Tooltip("One mesh per kind, in ObstacleKind order (ObstacleArt).")]
        [SerializeField] private Mesh[] meshes = new Mesh[ObstacleMotion.KindCount];
        [Tooltip("StudioKit/ToonLit with the instances keyword and GPU instancing on.")]
        [SerializeField] private Material material;
        [Tooltip("Throws the units an obstacle kills (the crowd's shape and material, no live units).")]
        [SerializeField] private LooseSwarmRenderer deaths;
        [Tooltip("Most units killed in one frame (the rest go the next frames), so a burst reads as a burst.")]
        [Min(1)] [SerializeField] private int maximumKillsPerFrame = 6;

        private readonly ObstaclePlacement[] placed = new ObstaclePlacement[ClaybreakLevel.MaximumObstacles];
        private ToonInstanceDraw[] draws;
        private NativeArray<ToonInstance>[] instances;
        private int[] counts;
        private byte[] hitBy;
        private int count;
        private float clock;
        private float centerX;
        private float halfWidth = 4f;
        private Color bodyTint = Color.white;
        private bool headless;
        private bool ready;

        /// <summary>Obstacles placed for the level.</summary>
        public int Count => count;

        /// <summary>Seconds on the obstacles' clock since the level was placed.</summary>
        public float Clock => clock;

        /// <summary>Instanced calls of the last frame (one per kind with an obstacle; the saw slots ride with the holes).</summary>
        public int DrawCalls { get; private set; }

        /// <summary>Kinds drawn in the last frame.</summary>
        public int KindsDrawn { get; private set; }

        /// <summary>Units the obstacles killed since the level was placed.</summary>
        public int Kills { get; private set; }

        /// <summary>Units on their way out after an obstacle hit.</summary>
        public int DyingCount => deaths == null ? 0 : deaths.DyingCount;

        /// <summary>The draw route in use (null while headless or disabled).</summary>
        public ToonInstanceRoute? Route => draws == null || draws[0] == null ? (ToonInstanceRoute?)null : draws[0].Route;

        /// <summary>The obstacles' body color (the world palette's defense color), drawn where the meshes' vertex alpha is 1.</summary>
        public Color BodyTint => bodyTint;

        public bool HasParts
        {
            get
            {
                if (material == null || deaths == null || meshes == null || meshes.Length != ObstacleMotion.KindCount)
                {
                    return false;
                }

                foreach (Mesh mesh in meshes)
                {
                    if (mesh == null)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>Checks the wiring and makes room to mark the crowd's slots (the flow calls it once, at start).</summary>
        public bool TryInitialize(int crowdCapacity, out string error)
        {
            if (!HasParts)
            {
                error = "the run obstacles need their 5 meshes, their material and the death thrower (rebuild the scene).";
                return false;
            }

            if (!ready)
            {
                error = "the run obstacles did not start (see the log above).";
                return false;
            }

            hitBy = new byte[Mathf.Max(1, crowdCapacity)]; // load time, once
            error = null;
            return true;
        }

        /// <summary>
        /// Places <paramref name="level"/>'s obstacles on a lane centered on <paramref name="trackCenterX"/>; the clock restarts and
        /// no slot is marked. False (nothing placed) when the level has more obstacles than the course holds.
        /// </summary>
        public bool Configure(ClaybreakLevel level, float trackCenterX, float laneHalfWidth)
        {
            count = 0;
            clock = 0f;
            Kills = 0;
            centerX = trackCenterX;
            halfWidth = Mathf.Max(0.5f, laneHalfWidth);
            ClearMarks();
            if (deaths != null)
            {
                deaths.Clear();
            }
            if (level == null || level.Obstacles.Count > placed.Length)
            {
                Debug.LogError($"RunObstacleCourse holds {placed.Length} obstacles; the level has more.", this);
                return false;
            }

            for (int i = 0; i < level.Obstacles.Count; i++)
            {
                placed[i] = level.Obstacles[i];
            }

            count = level.Obstacles.Count;
            return true;
        }

        /// <summary>The body color of every obstacle (the world palette's defense color).</summary>
        public void Dress(Color body)
        {
            bodyTint = body;
        }

        /// <summary>The shape the thrown units are drawn with (the crowd's).</summary>
        public void UseShape(Mesh unit)
        {
            if (deaths != null)
            {
                deaths.SetUnitMesh(unit);
            }
        }

        /// <summary>Forgets which slots were hit (the device gate loops the same crowd through the same obstacles).</summary>
        public void ClearMarks()
        {
            if (hitBy != null)
            {
                System.Array.Clear(hitBy, 0, hitBy.Length);
            }
        }

        /// <summary>Sets the clock (tests and captures: an obstacle at a known instant).</summary>
        public void SetClock(float seconds)
        {
            clock = Mathf.Max(0f, seconds);
        }

        public ObstaclePlacement PlacementOf(int index) => placed[index];

        /// <summary>Where obstacle <paramref name="index"/> is now and whether it kills now.</summary>
        public ObstacleMotion.Pose PoseOf(int index) => ObstacleMotion.PoseAt(placed[index], clock, centerX, halfWidth);

        /// <summary>
        /// Kills the units of <paramref name="crowd"/> the deadly obstacles touch this frame and returns how many (the flow
        /// takes them off the army). Reads only positions; allocates nothing.
        /// </summary>
        public int Track(RunCrowd crowd)
        {
            SwarmRenderer swarm = crowd.Swarm;
            int army = swarm.Count;
            if (count == 0 || hitBy == null || army == 0)
            {
                return 0;
            }

            // The formation's depth along the run, with a margin: obstacles outside it cannot touch anyone.
            Vector3 center = crowd.FormationCenter;
            SwarmFormationSettings formation = swarm.Formation;
            float depth = SunflowerFormation.Radius(army, formation.Spacing) * formation.DepthStretch + 1f;
            deaths.Area = new Bounds(new Vector3(centerX, 1f, center.z), new Vector3(halfWidth * 2f + 4f, 2f, depth * 2f + 4f));
            Color tint = swarm.Tint;
            int killed = 0;
            int units = Mathf.Min(army, hitBy.Length);
            for (int o = 0; o < count && killed < maximumKillsPerFrame; o++)
            {
                ObstaclePlacement obstacle = placed[o];
                ObstacleMotion.Reach(obstacle, out float near, out float far);
                if (far < center.z - depth || near > center.z + depth)
                {
                    continue;
                }

                ObstacleMotion.Pose pose = PoseOf(o);
                if (!pose.Deadly)
                {
                    continue;
                }

                // One bit per obstacle (at most 6 fit a byte): overlapping obstacles each hit a slot once.
                byte mark = (byte)(1 << o);
                for (int i = 0; i < units; i++)
                {
                    if ((hitBy[i] & mark) != 0)
                    {
                        continue;
                    }

                    Vector3 unit = swarm.UnitGroundPosition(i);
                    if (!ObstacleMotion.Hits(obstacle.kind, pose, unit.x, unit.z))
                    {
                        continue;
                    }

                    hitBy[i] |= mark;
                    deaths.Throw(unit.x, unit.z, 1f, tint, new Vector2(unit.x - pose.Hazard.x, unit.z - pose.Hazard.z));
                    killed++;
                    if (killed >= maximumKillsPerFrame)
                    {
                        break;
                    }
                }
            }

            Kills += killed;
            return killed;
        }

        private void OnEnable()
        {
            if (!HasParts)
            {
                Debug.LogError("[Obstacles] the course needs its 5 meshes, its material and the death thrower (rebuild the scene).", this);
                enabled = false;
                return;
            }

            if (!material.IsKeywordEnabled(ToonShaderIds.InstancesKeyword) || ToonInstanceDraw.ValidateMaterial(material) != null)
            {
                Debug.LogError($"[Obstacles] {material.name} must have {ToonShaderIds.InstancesKeyword} and GPU instancing on.", this);
                enabled = false;
                return;
            }

            int capacity = ClaybreakLevel.MaximumObstacles;
            instances = new NativeArray<ToonInstance>[ObstacleMotion.KindCount];
            counts = new int[ObstacleMotion.KindCount];
            for (int k = 0; k < instances.Length; k++)
            {
                instances[k] = new NativeArray<ToonInstance>(capacity, Allocator.Persistent);
            }

            headless = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
            if (!headless)
            {
                CreateDraws();
            }

            ready = true;
        }

        private void OnDisable()
        {
            ready = false;
            DisposeDraws();
            if (instances != null)
            {
                foreach (NativeArray<ToonInstance> array in instances)
                {
                    if (array.IsCreated)
                    {
                        array.Dispose();
                    }
                }

                instances = null;
            }

            DrawCalls = 0;
        }

        private void Update()
        {
            clock += Time.deltaTime;
        }

        private void LateUpdate()
        {
            DrawCalls = 0;
            KindsDrawn = 0;
            if (headless || count == 0)
            {
                return;
            }

            if (draws[0].Stale)
            {
                // A route was forced (tests, measurement builds): the other route's draws.
                DisposeDraws();
                CreateDraws();
            }

            System.Array.Clear(counts, 0, counts.Length);
            Color linear = bodyTint.linear;
            var tint = new float4(linear.r, linear.g, linear.b, 0f);
            float nearest = float.MaxValue;
            float farthest = float.MinValue;
            for (int i = 0; i < count; i++)
            {
                ObstaclePlacement obstacle = placed[i];
                ObstacleMotion.Pose pose = PoseOf(i);
                Write((int)obstacle.kind, Compose(obstacle.kind, pose, tint));
                if (obstacle.kind == ObstacleKind.Saw)
                {
                    // The blade's slot: a pit stretched across the track.
                    var slot = new Vector3((halfWidth + ObstacleMotion.SawSlotOvershoot) * 2f / (ObstacleMotion.HoleHalfWidth * 2f), 1f,
                        ObstacleMotion.SawSlotDepth / (ObstacleMotion.HoleHalfDepth * 2f));
                    Write((int)ObstacleKind.Hole, Instance(new Vector3(centerX, 0f, obstacle.z), Quaternion.identity, slot, tint));
                }

                ObstacleMotion.Reach(obstacle, out float near, out float far);
                nearest = Mathf.Min(nearest, near);
                farthest = Mathf.Max(farthest, far);
            }

            var bounds = new Bounds(new Vector3(centerX, ObstacleMotion.SwingPivotHeight * 0.5f + 2f, 0.5f * (nearest + farthest)),
                new Vector3(halfWidth * 2f + 12f, ObstacleMotion.SwingPivotHeight + 14f, farthest - nearest + 6f));
            int calls = 0;
            int kinds = 0;
            for (int k = 0; k < counts.Length; k++)
            {
                int n = counts[k];
                if (n == 0)
                {
                    continue;
                }

                draws[k].Upload(instances[k], n);
                draws[k].Draw(meshes[k], n, bounds);
                calls += ToonInstanceDraw.Calls(draws[k].Route, n);
                kinds++;
            }

            DrawCalls = calls;
            KindsDrawn = kinds;
        }

        private void Write(int kind, in ToonInstance instance)
        {
            NativeArray<ToonInstance> array = instances[kind];
            if (counts[kind] < array.Length)
            {
                array[counts[kind]] = instance;
                counts[kind]++;
            }
        }

        // The mesh of each kind is modeled around its anchor (ObstacleArt): the log hangs down from its pivot and turns about
        // z, the blade turns about z, the drum about x; the hammer and the pit are only placed (the pit at the hole's size).
        private static ToonInstance Compose(ObstacleKind kind, in ObstacleMotion.Pose pose, float4 tint)
        {
            switch (kind)
            {
                case ObstacleKind.SwingingLog:
                case ObstacleKind.Saw:
                    return Instance(pose.Anchor, Quaternion.Euler(0f, 0f, pose.Angle * Mathf.Rad2Deg), Vector3.one, tint);
                case ObstacleKind.Roller:
                    return Instance(pose.Anchor, Quaternion.Euler(pose.Angle * Mathf.Rad2Deg, 0f, 0f), Vector3.one, tint);
                default:
                    return Instance(pose.Anchor, Quaternion.identity, Vector3.one, tint);
            }
        }

        private static ToonInstance Instance(Vector3 position, Quaternion rotation, Vector3 scale, float4 tint)
        {
            Matrix4x4 m = Matrix4x4.TRS(position, rotation, scale);
            return new ToonInstance
            {
                Row0 = new float4(m.m00, m.m01, m.m02, m.m03),
                Row1 = new float4(m.m10, m.m11, m.m12, m.m13),
                Row2 = new float4(m.m20, m.m21, m.m22, m.m23),
                Tint = tint,
            };
        }

        private void CreateDraws()
        {
            draws = new ToonInstanceDraw[ObstacleMotion.KindCount];
            for (int k = 0; k < draws.Length; k++)
            {
                // The holes also draw the saws' slots: room for one per obstacle either way.
                draws[k] = new ToonInstanceDraw(material, null, ClaybreakLevel.MaximumObstacles, gameObject.layer);
            }
        }

        private void DisposeDraws()
        {
            if (draws == null)
            {
                return;
            }

            foreach (ToonInstanceDraw draw in draws)
            {
                draw?.Dispose();
            }

            draws = null;
        }
    }
}
