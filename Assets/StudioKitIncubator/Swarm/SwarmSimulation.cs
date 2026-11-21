using System;
using JungleDegree.StudioKit.Toon;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// State of a swarm as native arrays of a fixed capacity, allocated once (no per-unit objects): the sunflower
    /// offsets (computed at load), ground position (world x, z relative to the center) and hop phase per unit, plus the
    /// per-frame instance data. Every frame the center follows the steering target, each unit follows its slot
    /// (<see cref="SwarmFollowJob"/>) and the hop is written (<see cref="SwarmAnimateJob"/>), both Burst jobs on the
    /// worker threads. Slots are kept inside a lane. The owner disposes it.
    /// </summary>
    public sealed class SwarmSimulation : IDisposable
    {
        /// <summary>Lane half width meaning "no lane" (finite: the jobs use fast math, which assumes no infinities).</summary>
        public const float NoLane = 1e9f;

        private const int JobBatch = 64;

        private readonly SwarmFormationSettings formation;
        private NativeArray<float2> offsets;
        private NativeArray<float2> positions;
        private NativeArray<float> phases;
        private NativeArray<ToonInstance> instances;
        private JobHandle pending;
        private bool scheduled;

        public int Capacity { get; }
        public int Count { get; private set; }
        public SwarmFormationSettings Formation => formation;

        /// <summary>Across position of the formation center (follows <see cref="TargetX"/>).</summary>
        public float CenterX { get; private set; }

        /// <summary>Where the center is being steered to (across).</summary>
        public float TargetX { get; private set; }

        /// <summary>Position of the formation center along the run.</summary>
        public float CenterZ { get; set; }

        public float LaneCenter { get; private set; }

        /// <summary>Half width of the lane the slots are kept in (<see cref="NoLane"/> by default).</summary>
        public float LaneHalfWidth { get; private set; } = NoLane;

        public NativeArray<ToonInstance> Instances => instances;

        public SwarmSimulation(int capacity, in SwarmFormationSettings formation, uint seed)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            Capacity = capacity;
            this.formation = formation;
            offsets = new NativeArray<float2>(capacity, Allocator.Persistent);
            positions = new NativeArray<float2>(capacity, Allocator.Persistent);
            phases = new NativeArray<float>(capacity, Allocator.Persistent);
            instances = new NativeArray<ToonInstance>(capacity, Allocator.Persistent);
            SunflowerFormation.FillOffsets(offsets.AsSpan(), formation.Spacing, formation.DepthStretch);
            var random = new Unity.Mathematics.Random(seed == 0 ? 1u : seed);
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

        /// <summary>
        /// Sets the live count (clamped to the capacity). New units appear in their slot; removed ones are the last
        /// (outer ring) of the formation.
        /// </summary>
        public void SetCount(int count)
        {
            Complete();
            int next = math.clamp(count, 0, Capacity);
            for (int i = Count; i < next; i++)
            {
                float2 offset = offsets[i];
                positions[i] = new float2(SlotX(offset.x), offset.y);
            }

            Count = next;
        }

        /// <summary>Where unit <paramref name="index"/> stands now (world x, z), after the last completed frame.</summary>
        public float2 GroundPosition(int index)
        {
            Complete();
            float2 p = positions[index];
            return new float2(p.x, CenterZ + p.y);
        }

        /// <summary>Bounds of the live units on the ground plus the hop, for culling the draws.</summary>
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
            return new Bounds(new Vector3(0.5f * (minX + maxX), 0.5f * height, CenterZ), size);
        }

        /// <summary>
        /// Advances one frame: the center closes on the target, then the units follow and hop (Burst, worker threads;
        /// <see cref="Complete"/> before reading).
        /// </summary>
        public void Schedule(float deltaTime, float time, in SwarmMotionSettings motion, float3 tint)
        {
            Complete();
            CenterX += (TargetX - CenterX) * SunflowerFormation.FollowFactor(formation.CenterSharpness, deltaTime);
            var follow = new SwarmFollowJob
            {
                Offsets = offsets,
                Positions = positions,
                CenterX = CenterX,
                LaneCenter = LaneCenter,
                LaneHalfWidth = LaneHalfWidth,
                EdgeMargin = formation.EdgeMargin,
                Follow = SunflowerFormation.FollowFactor(formation.FollowSharpness, deltaTime),
                PaceSpread = formation.PaceSpread,
            };
            var animate = new SwarmAnimateJob
            {
                Positions = positions,
                Phases = phases,
                Instances = instances,
                Motion = motion,
                Tint = tint,
                Time = time,
                CenterZ = CenterZ,
            };
            JobHandle followed = follow.Schedule(Count, JobBatch);
            pending = animate.Schedule(Count, JobBatch, followed);
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
            if (offsets.IsCreated)
            {
                offsets.Dispose();
            }

            if (positions.IsCreated)
            {
                positions.Dispose();
            }

            if (phases.IsCreated)
            {
                phases.Dispose();
            }

            if (instances.IsCreated)
            {
                instances.Dispose();
            }
        }

        private float SlotX(float offsetX)
        {
            return SunflowerFormation.SlotX(CenterX, offsetX, LaneCenter, LaneHalfWidth, formation.EdgeMargin);
        }
    }
}
