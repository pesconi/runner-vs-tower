using System;
using JungleDegree.StudioKit.Toon;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>How units removed from a swarm leave it.</summary>
    public enum SwarmRemoval
    {
        /// <summary>Gone at once (a restart, a unit taken out for another use).</summary>
        Vanish,

        /// <summary>Thrown out in the ballistic death (<see cref="SwarmDying"/>), while room is left in the ring.</summary>
        Die,
    }

    /// <summary>
    /// State of a swarm as native arrays of a fixed capacity, allocated once (no per-unit objects): the sunflower
    /// offsets (computed at load), ground position (world x, z relative to the center), across speed, hop phase and
    /// birth time per unit, a ring of fixed capacity for the units on their way out, plus the per-frame instance data
    /// (the live units first, then the dying). Every frame the center follows the steering target, each unit follows
    /// its slot (<see cref="SwarmFollowJob"/>) and is animated (<see cref="SwarmAnimateJob"/>), and the dying fly
    /// (<see cref="SwarmDyingJob"/>), all Burst jobs on the worker threads. Slots are kept inside a lane. Units are
    /// removed from the outer ring of the formation inward. The owner disposes it.
    /// </summary>
    public sealed class SwarmSimulation : IDisposable
    {
        /// <summary>Lane half width meaning "no lane" (finite: the jobs use fast math, which assumes no infinities).</summary>
        public const float NoLane = 1e9f;

        // Birth time of units placed without a pop: far enough back to be past any pop, finite for fast math.
        private const float BornLongAgo = -1e6f;

        private const int JobBatch = 64;

        private readonly SwarmFormationSettings formation;
        private NativeArray<float2> offsets;
        private NativeArray<float2> positions;
        private NativeArray<float> acrossSpeeds;
        private NativeArray<float> phases;
        private NativeArray<float> bornAt;
        private readonly SwarmDyingRing dying;
        private NativeArray<ToonInstance> instances;
        private Unity.Mathematics.Random random;
        private JobHandle pending;
        private bool scheduled;
        private int writtenCount;
        private int writtenDying;

        public int Capacity { get; }

        /// <summary>Most units that can be on their way out at once; when full, the oldest goes first.</summary>
        public int DyingCapacity { get; }

        /// <summary>Live units (the formation).</summary>
        public int Count { get; private set; }

        /// <summary>Units on their way out (drawn after the live ones).</summary>
        public int DyingCount => dying.Count;

        /// <summary>
        /// Instances written by the last frame (<see cref="Schedule"/>): <see cref="WrittenCount"/> live, then the dying.
        /// Counts changed after a frame is scheduled show from the next frame on, so the draw never reads stale entries.
        /// </summary>
        public int DrawCount => writtenCount + writtenDying;

        /// <summary>Live units written by the last frame (the ones with a blob shadow).</summary>
        public int WrittenCount => writtenCount;

        public SwarmFormationSettings Formation => formation;

        /// <summary>Time of the last frame (<see cref="Schedule"/>); births and deaths start from it unless told otherwise.</summary>
        public float Clock { get; private set; }

        /// <summary>Across position of the formation center (follows <see cref="TargetX"/>).</summary>
        public float CenterX { get; private set; }

        /// <summary>Where the center is being steered to (across).</summary>
        public float TargetX { get; private set; }

        /// <summary>Position of the formation center along the run.</summary>
        public float CenterZ { get; set; }

        public float LaneCenter { get; private set; }

        /// <summary>Half width of the lane the slots are kept in (<see cref="NoLane"/> by default).</summary>
        public float LaneHalfWidth { get; private set; } = NoLane;

        /// <summary>Body tint given to the units that die from now on (linear color); the live ones get it per frame.</summary>
        public float3 Tint { get; set; } = new float3(1f);

        /// <summary>How the live units lean and pop (<see cref="SwarmAnimation"/>).</summary>
        public SwarmAnimationSettings Animation { get; set; } = SwarmAnimationSettings.Default;

        /// <summary>How removed units fly out (<see cref="SwarmDying"/>).</summary>
        public SwarmDeathSettings Death { get; set; } = SwarmDeathSettings.Default;

        public NativeArray<ToonInstance> Instances => instances;

        /// <summary>A swarm with no room for dying units (removals always vanish).</summary>
        public SwarmSimulation(int capacity, in SwarmFormationSettings formation, uint seed)
            : this(capacity, 0, formation, seed)
        {
        }

        public SwarmSimulation(int capacity, int dyingCapacity, in SwarmFormationSettings formation, uint seed)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (dyingCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dyingCapacity));
            }

            Capacity = capacity;
            DyingCapacity = dyingCapacity;
            this.formation = formation;
            offsets = new NativeArray<float2>(capacity, Allocator.Persistent);
            positions = new NativeArray<float2>(capacity, Allocator.Persistent);
            acrossSpeeds = new NativeArray<float>(capacity, Allocator.Persistent);
            phases = new NativeArray<float>(capacity, Allocator.Persistent);
            bornAt = new NativeArray<float>(capacity, Allocator.Persistent);
            dying = new SwarmDyingRing(dyingCapacity);
            instances = new NativeArray<ToonInstance>(capacity + dyingCapacity, Allocator.Persistent);
            SunflowerFormation.FillOffsets(offsets.AsSpan(), formation.Spacing, formation.DepthStretch);
            random = new Unity.Mathematics.Random(seed == 0 ? 1u : seed);
            for (int i = 0; i < capacity; i++)
            {
                phases[i] = random.NextFloat(0f, 2f * math.PI);
            }
        }

        /// <summary>Sets the lane the formation is kept in (centered on <paramref name="center"/> across).</summary>
        public void SetLane(float center, float halfWidth)
        {
            LaneCenter = center;
            LaneHalfWidth = math.clamp(halfWidth, 0f, NoLane);
        }

        /// <summary>Steers the center toward <paramref name="x"/>; it gets there smoothly, frame by frame.</summary>
        public void SteerTo(float x)
        {
            TargetX = x;
        }

        /// <summary>Puts the center (and its target) at <paramref name="x"/> at once; units keep their positions.</summary>
        public void PlaceCenter(float x)
        {
            CenterX = x;
            TargetX = x;
        }

        /// <summary>Sets the live count at the time of the last frame (<see cref="Clock"/>); removed units vanish.</summary>
        public void SetCount(int count)
        {
            SetCount(count, Clock, SwarmRemoval.Vanish);
        }

        /// <summary>
        /// Sets the live count (clamped to the capacity) at <paramref name="time"/>. New units appear in their slot and
        /// pop after a random delay; removed ones are the last (outer ring) of the formation and either
        /// vanish or start their death.
        /// </summary>
        public void SetCount(int count, float time, SwarmRemoval removal)
        {
            Complete();
            int next = math.clamp(count, 0, Capacity);
            for (int i = Count; i < next; i++)
            {
                float2 offset = offsets[i];
                positions[i] = new float2(SlotX(offset.x), offset.y);
                acrossSpeeds[i] = 0f;
                bornAt[i] = time + random.NextFloat() * Animation.BirthDelayMax;
            }

            if (removal == SwarmRemoval.Die)
            {
                // In slot order, as the reference does: when the ring overflows, the first thrown goes first.
                for (int i = next; i < Count; i++)
                {
                    dying.Push(SwarmDying.Launch(positions[i], offsets[i], time, Tint, Death, ref random));
                }
            }

            Count = next;
        }

        /// <summary>
        /// A restart: the units on their way out are cleared and <paramref name="count"/> units stand in their slots at
        /// once, already full size (no pop, no death).
        /// </summary>
        public void Reset(int count)
        {
            ClearDying();
            Count = 0;
            SetCount(count, Clock, SwarmRemoval.Vanish);
            for (int i = 0; i < Count; i++)
            {
                bornAt[i] = BornLongAgo;
            }
        }

        /// <summary>When unit <paramref name="index"/> starts (or started) its pop (tests and tools).</summary>
        public float BirthTime(int index)
        {
            Complete();
            return bornAt[index];
        }

        /// <summary>Clears the units on their way out at once (a restart).</summary>
        public void ClearDying()
        {
            Complete();
            dying.Clear();
        }

        /// <summary>Unit <paramref name="index"/> of the ring of dying units, oldest first (tests and tools).</summary>
        public SwarmDyingUnit DyingUnit(int index)
        {
            Complete();
            return dying[index];
        }

        /// <summary>Where unit <paramref name="index"/> stands now (world x, z), after the last completed frame.</summary>
        public float2 GroundPosition(int index)
        {
            Complete();
            float2 p = positions[index];
            return new float2(p.x, CenterZ + p.y);
        }

        /// <summary>
        /// Bounds of the live units on the ground plus the hop, for culling the draws; grown by how far the dying can fly
        /// while there are any.
        /// </summary>
        public Bounds GroundBounds(float height)
        {
            float radius = SunflowerFormation.Radius(Count, formation.Spacing) + formation.Spacing;
            float minX;
            float maxX;
            if (LaneHalfWidth >= NoLane)
            {
                minX = math.min(CenterX, TargetX) - radius;
                maxX = math.max(CenterX, TargetX) + radius;
            }
            else
            {
                minX = LaneCenter - LaneHalfWidth - formation.Spacing;
                maxX = LaneCenter + LaneHalfWidth + formation.Spacing;
            }

            var size = new Vector3(maxX - minX, height, 2f * radius * formation.DepthStretch);
            var bounds = new Bounds(new Vector3(0.5f * (minX + maxX), 0.5f * height, CenterZ), size);
            dying.Encapsulate(ref bounds, CenterZ, height, Death);
            return bounds;
        }

        /// <summary>
        /// Advances one frame to <paramref name="time"/>: the dying past their lifetime are dropped, the center closes on
        /// the target, then the units follow and are animated and the dying fly (Burst, worker threads;
        /// <see cref="Complete"/> before reading).
        /// </summary>
        public void Schedule(float deltaTime, float time, in SwarmMotionSettings motion, float3 tint)
        {
            Complete();
            Clock = time;
            Tint = tint;
            dying.DropExpired(time, Death.Lifetime);
            CenterX += (TargetX - CenterX) * SunflowerFormation.FollowFactor(formation.CenterSharpness, deltaTime);
            var follow = new SwarmFollowJob
            {
                Offsets = offsets,
                Positions = positions,
                AcrossSpeeds = acrossSpeeds,
                CenterX = CenterX,
                LaneCenter = LaneCenter,
                LaneHalfWidth = LaneHalfWidth,
                EdgeMargin = formation.EdgeMargin,
                Follow = SunflowerFormation.FollowFactor(formation.FollowSharpness, deltaTime),
                PaceSpread = formation.PaceSpread,
                DeltaTime = deltaTime,
            };
            var animate = new SwarmAnimateJob
            {
                Positions = positions,
                AcrossSpeeds = acrossSpeeds,
                Phases = phases,
                BornAt = bornAt,
                Instances = instances,
                Motion = motion,
                Animation = Animation,
                Tint = tint,
                Time = time,
                CenterZ = CenterZ,
            };
            JobHandle followed = follow.Schedule(Count, JobBatch);
            pending = animate.Schedule(Count, JobBatch, followed);
            pending = dying.ScheduleFly(instances.GetSubArray(Count, DyingCount), deltaTime, time, CenterZ, Death, pending);

            writtenCount = Count;
            writtenDying = DyingCount;
            scheduled = true;
        }

        /// <summary>Waits for the scheduled frame; <see cref="Instances"/> is then readable.</summary>
        public void Complete()
        {
            if (scheduled)
            {
                pending.Complete();
                scheduled = false;
            }
        }

        public void Dispose()
        {
            Complete();
            DisposeArray(ref offsets);
            DisposeArray(ref positions);
            DisposeArray(ref acrossSpeeds);
            DisposeArray(ref phases);
            DisposeArray(ref bornAt);
            dying.Dispose();
            DisposeArray(ref instances);
        }

        private static void DisposeArray<T>(ref NativeArray<T> array) where T : struct
        {
            if (array.IsCreated)
            {
                array.Dispose();
            }
        }

        private float SlotX(float offsetX)
        {
            return SunflowerFormation.SlotX(CenterX, offsetX, LaneCenter, LaneHalfWidth, formation.EdgeMargin);
        }
    }
}
