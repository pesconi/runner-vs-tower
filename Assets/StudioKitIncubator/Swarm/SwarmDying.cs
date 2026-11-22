using System;
using JungleDegree.StudioKit.Toon;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// The ballistic death of a removed unit: thrown out of the formation (outward from its slot, plus a random push)
    /// and up, it falls under gravity, tumbles and shrinks to nothing. Defaults are the reference values of the studio
    /// crowd look.
    /// </summary>
    [Serializable]
    public struct SwarmDeathSettings
    {
        [Tooltip("Downward acceleration (m/s²).")]
        [Min(0f)] public float Gravity;
        [Tooltip("Time from the death to the unit gone (s); it shrinks linearly over it.")]
        [Min(0.001f)] public float Lifetime;
        [Tooltip("Outward speed across (m/s), along the direction of the unit's slot from the formation center.")]
        public float OutwardSpeedX;
        [Tooltip("Outward speed along the run (m/s).")]
        public float OutwardSpeedZ;
        [Tooltip("Random extra speed across, ±half of this (m/s).")]
        [Min(0f)] public float JitterX;
        [Tooltip("Random extra speed along the run, ±half of this (m/s).")]
        [Min(0f)] public float JitterZ;
        [Tooltip("Smallest upward speed (m/s).")]
        public float UpSpeedMin;
        [Tooltip("Random extra upward speed, 0 up to this (m/s).")]
        [Min(0f)] public float UpSpeedRange;
        [Tooltip("Largest tumble rate (rad/s); each unit draws its own between 0 and this.")]
        [Min(0f)] public float SpinMax;

        public static SwarmDeathSettings Default => new SwarmDeathSettings
        {
            Gravity = 22f,
            Lifetime = 1.1f,
            OutwardSpeedX = 3f,
            OutwardSpeedZ = 2f,
            JitterX = 3f,
            JitterZ = 2f,
            UpSpeedMin = 4f,
            UpSpeedRange = 4f,
            SpinMax = 6f,
        };
    }

    /// <summary>
    /// One unit on its way out: position (world x, height, z relative to the swarm center), speed, and its size when it
    /// died (1 = the mesh as modeled; it shrinks from there).
    /// </summary>
    public struct SwarmDyingUnit
    {
        public float3 Position;
        public float3 Velocity;
        public float DiedAt;
        public float Spin;
        public float3 Tint;
        public float Size;
    }

    /// <summary>Formulas of the ballistic death, pure and Burst-compatible (tested in EditMode against the reference).</summary>
    public static class SwarmDying
    {
        /// <summary>The tumble about Z turns at this share of the tumble about X.</summary>
        public const float SpinZShare = 0.7f;

        /// <summary>
        /// A unit thrown out at <paramref name="time"/> from <paramref name="ground"/> (world x, z relative to the
        /// center), whose formation slot is at <paramref name="slotOffset"/>: outward along the slot direction, up, with
        /// the random parts drawn from <paramref name="random"/> in a fixed order (across, up, along, spin).
        /// </summary>
        public static SwarmDyingUnit Launch(float2 ground, float2 slotOffset, float time, float3 tint,
            in SwarmDeathSettings settings, ref Unity.Mathematics.Random random)
        {
            return Launch(ground, slotOffset, time, tint, 1f, settings, ref random);
        }

        /// <summary>
        /// As the formation launch, for a unit of <paramref name="size"/> (1 = the mesh as modeled) thrown along
        /// <paramref name="outward"/> (any length; only its direction counts), for units that are not in a formation.
        /// </summary>
        public static SwarmDyingUnit Launch(float2 ground, float2 outward, float time, float3 tint, float size,
            in SwarmDeathSettings settings, ref Unity.Mathematics.Random random)
        {
            float angle = math.atan2(outward.y, outward.x);
            math.sincos(angle, out float sin, out float cos);
            float vx = cos * settings.OutwardSpeedX + (random.NextFloat() - 0.5f) * settings.JitterX;
            float vy = settings.UpSpeedMin + random.NextFloat() * settings.UpSpeedRange;
            float vz = sin * settings.OutwardSpeedZ + (random.NextFloat() - 0.5f) * settings.JitterZ;
            return new SwarmDyingUnit
            {
                Position = new float3(ground.x, 0f, ground.y),
                Velocity = new float3(vx, vy, vz),
                DiedAt = time,
                Spin = random.NextFloat() * settings.SpinMax,
                Tint = tint,
                Size = size,
            };
        }

        /// <summary>True while a unit that died at <paramref name="diedAt"/> is still on its way out at <paramref name="time"/>.</summary>
        public static bool Alive(float time, float diedAt, float lifetime)
        {
            return time - diedAt < lifetime;
        }

        /// <summary>
        /// One frame of the flight: gravity first, then the move; the height never goes below the ground (the unit
        /// slides along it).
        /// </summary>
        public static void Step(ref SwarmDyingUnit unit, float deltaTime, float gravity)
        {
            unit.Velocity.y -= gravity * deltaTime;
            unit.Position.x += unit.Velocity.x * deltaTime;
            unit.Position.y = math.max(0f, unit.Position.y + unit.Velocity.y * deltaTime);
            unit.Position.z += unit.Velocity.z * deltaTime;
        }

        /// <summary>Scale of a dying unit: 1 at the death, down to 0 at the end of its lifetime.</summary>
        public static float Scale(float time, float diedAt, float lifetime)
        {
            return math.max(0f, 1f - (time - diedAt) / lifetime);
        }

        /// <summary>
        /// The tumble angles at <paramref name="time"/> (x: about X, y: about Z). As in the reference they follow the
        /// clock, not the age; the X tumble is mirrored like the forward lean (the front is +Z here).
        /// </summary>
        public static float2 Tumble(float time, float spin)
        {
            return new float2(-spin * time, SpinZShare * spin * time);
        }

        /// <summary>How far a dying unit can get from where it died (x across, y up, z along), to grow the draw bounds.</summary>
        public static float3 Reach(in SwarmDeathSettings settings)
        {
            float up = settings.UpSpeedMin + settings.UpSpeedRange;
            float height = settings.Gravity > 0f ? up * up / (2f * settings.Gravity) : up * settings.Lifetime;
            return new float3(
                (math.abs(settings.OutwardSpeedX) + 0.5f * settings.JitterX) * settings.Lifetime,
                height,
                (math.abs(settings.OutwardSpeedZ) + 0.5f * settings.JitterZ) * settings.Lifetime);
        }
    }

    /// <summary>
    /// Advances every unit on its way out by one frame and writes its drawn transform after the live ones. The units
    /// live in a ring of fixed capacity; job index j is ring slot (Head + j) mod capacity. Shadow scale (tint.w) is 0:
    /// the dying are not given blob shadows.
    /// </summary>
    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct SwarmDyingJob : IJob
    {
        public NativeArray<SwarmDyingUnit> Ring;
        [WriteOnly] public NativeArray<ToonInstance> Instances;
        public int Head;
        public int Count;
        public float DeltaTime;
        public float Time;
        public float CenterZ;
        public SwarmDeathSettings Death;

        public void Execute()
        {
            int capacity = Ring.Length;
            for (int j = 0; j < Count; j++)
            {
                int slot = (Head + j) % capacity;
                SwarmDyingUnit unit = Ring[slot];
                SwarmDying.Step(ref unit, DeltaTime, Death.Gravity);
                Ring[slot] = unit;
                float scale = SwarmDying.Scale(Time, unit.DiedAt, Death.Lifetime) * unit.Size;
                float2 tumble = SwarmDying.Tumble(Time, unit.Spin);
                var instance = new ToonInstance { Tint = new float4(unit.Tint, 0f) };
                SwarmAnimation.Compose(ref instance, new float3(unit.Position.x, unit.Position.y, CenterZ + unit.Position.z),
                    tumble.x, tumble.y, scale, scale);
                Instances[j] = instance;
            }
        }
    }
}
