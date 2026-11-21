using System;
using JungleDegree.StudioKit.Toon;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// State of a swarm as native arrays of a fixed capacity, allocated once (no per-unit objects): ground position
    /// (x, z) and hop phase per unit, plus the per-frame instance data written by <see cref="SwarmAnimateJob"/>.
    /// Units stand in a sunflower formation around <see cref="Center"/>. The owner disposes it.
    /// </summary>
    public sealed class SwarmSimulation : IDisposable
    {
        private const int JobBatch = 64;

        private NativeArray<float2> positions;
        private NativeArray<float> phases;
        private NativeArray<ToonInstance> instances;
        private JobHandle pending;
        private bool scheduled;

        public int Capacity { get; }
        public int Count { get; private set; }
        public float Spacing { get; }
        public float DepthStretch { get; }
        public float2 Center { get; private set; }
        public NativeArray<ToonInstance> Instances => instances;

        public SwarmSimulation(int capacity, float spacing, float depthStretch, uint seed)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            Capacity = capacity;
            Spacing = spacing;
            DepthStretch = depthStretch;
            positions = new NativeArray<float2>(capacity, Allocator.Persistent);
            phases = new NativeArray<float>(capacity, Allocator.Persistent);
            instances = new NativeArray<ToonInstance>(capacity, Allocator.Persistent);
            var random = new Unity.Mathematics.Random(seed == 0 ? 1u : seed);
            for (int i = 0; i < capacity; i++)
            {
                phases[i] = random.NextFloat(0f, 2f * math.PI);
            }
        }

        /// <summary>Sets the live count (clamped to the capacity) and places the units in formation.</summary>
        public void SetCount(int count, float2 center)
        {
            Complete();
            Count = math.clamp(count, 0, Capacity);
            Center = center;
            for (int i = 0; i < Count; i++)
            {
                positions[i] = center + SwarmMotion.SunflowerOffset(i, Spacing, DepthStretch);
            }
        }

        /// <summary>Bounds of the live units on the ground plus the hop, for culling the draws.</summary>
        public Bounds GroundBounds(float height)
        {
            float radius = SwarmMotion.SunflowerRadius(Count, Spacing) + Spacing;
            var size = new Vector3(2f * radius, height, 2f * radius * DepthStretch);
            return new Bounds(new Vector3(Center.x, 0.5f * height, Center.y), size);
        }

        /// <summary>Schedules the animation of this frame (Burst, worker threads).</summary>
        public void Schedule(float time, in SwarmMotionSettings motion, float3 tint)
        {
            Complete();
            var job = new SwarmAnimateJob
            {
                Positions = positions,
                Phases = phases,
                Instances = instances,
                Motion = motion,
                Tint = tint,
                Time = time,
            };
            pending = job.Schedule(Count, JobBatch);
            scheduled = true;
        }

        /// <summary>Waits for the scheduled animation; <see cref="Instances"/> is then readable.</summary>
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
    }
}
