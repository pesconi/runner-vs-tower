using System;
using JungleDegree.StudioKit.Toon;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// The units on their way out (ballistic death, <see cref="SwarmDying"/>) of one swarm: a ring of fixed capacity
    /// allocated once. Units enter in time order and share one lifetime, so the expired are always at the head; when the
    /// ring is full the oldest makes room. Each frame <see cref="ScheduleFly"/> moves them all in one Burst job and writes
    /// their drawn transforms. Positions are world x, height and z relative to the owner's center along the run (0 for an
    /// owner that works in world z). The owner disposes it.
    /// </summary>
    public sealed class SwarmDyingRing : IDisposable
    {
        private NativeArray<SwarmDyingUnit> ring;
        private int head;
        private float4 origins; // min x, min z, max x, max z of where the units in the ring died

        /// <summary>A ring for up to <paramref name="capacity"/> units (0 = nothing is ever kept).</summary>
        public SwarmDyingRing(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            Capacity = capacity;
            // A ring of at least one slot keeps the job's array valid; with no capacity nothing is ever put in it.
            ring = new NativeArray<SwarmDyingUnit>(math.max(1, capacity), Allocator.Persistent);
        }

        public int Capacity { get; }

        /// <summary>Units on their way out now.</summary>
        public int Count { get; private set; }

        /// <summary>Puts a unit in the ring; when it is full the oldest goes first. Nothing with no capacity.</summary>
        public void Push(in SwarmDyingUnit unit)
        {
            if (Capacity == 0)
            {
                return;
            }

            if (Count == Capacity)
            {
                head = (head + 1) % Capacity;
                Count--;
            }

            ring[(head + Count) % Capacity] = unit;
            var origin = new float2(unit.Position.x, unit.Position.z);
            origins = Count == 0
                ? new float4(origin, origin)
                : new float4(math.min(origins.xy, origin), math.max(origins.zw, origin));
            Count++;
        }

        /// <summary>Unit <paramref name="index"/>, oldest first (tests and tools; the owner completes its jobs first).</summary>
        public SwarmDyingUnit this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return ring[(head + index) % Capacity];
            }
        }

        /// <summary>Empties the ring at once (a restart).</summary>
        public void Clear()
        {
            head = 0;
            Count = 0;
        }

        /// <summary>Drops the units whose lifetime is over at <paramref name="time"/>.</summary>
        public void DropExpired(float time, float lifetime)
        {
            while (Count > 0 && !SwarmDying.Alive(time, ring[head].DiedAt, lifetime))
            {
                head = (head + 1) % Capacity;
                Count--;
            }
        }

        /// <summary>
        /// Schedules one frame of the flight after <paramref name="dependsOn"/>: every unit moves and its transform is
        /// written to <paramref name="instances"/> (exactly <see cref="Count"/> long). Nothing is scheduled when empty.
        /// </summary>
        public JobHandle ScheduleFly(NativeArray<ToonInstance> instances, float deltaTime, float time, float centerZ,
            in SwarmDeathSettings death, JobHandle dependsOn)
        {
            if (Count == 0)
            {
                return dependsOn;
            }

            var fly = new SwarmDyingJob
            {
                Ring = ring,
                Instances = instances,
                Head = head,
                Count = Count,
                DeltaTime = deltaTime,
                Time = time,
                CenterZ = centerZ,
                Death = death,
            };
            return fly.Schedule(dependsOn);
        }

        /// <summary>Grows <paramref name="bounds"/> by how far the units in the ring can fly (nothing when empty).</summary>
        public void Encapsulate(ref Bounds bounds, float centerZ, float height, in SwarmDeathSettings death)
        {
            if (Count == 0)
            {
                return;
            }

            // The box of where the units in the ring died, grown by how far they can fly, holds them all.
            float3 reach = SwarmDying.Reach(death);
            bounds.Encapsulate(new Vector3(origins.x - reach.x, 0f, centerZ + origins.y - reach.z));
            bounds.Encapsulate(new Vector3(origins.z + reach.x, height + reach.y, centerZ + origins.w + reach.z));
        }

        public void Dispose()
        {
            if (ring.IsCreated)
            {
                ring.Dispose();
            }
        }
    }
}
