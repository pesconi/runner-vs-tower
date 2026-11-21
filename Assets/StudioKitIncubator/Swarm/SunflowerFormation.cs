using System;
using Unity.Mathematics;
using UnityEngine;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// How a swarm keeps its sunflower formation while it is steered: the shape of the disc, how fast the center
    /// follows the steering target and how fast each unit follows its slot (each unit at its own pace, so the crowd
    /// ripples instead of moving as one block). Defaults are the reference values of the studio crowd look.
    /// </summary>
    [Serializable]
    public struct SwarmFormationSettings
    {
        [Tooltip("Distance between rings of the sunflower disc (meters): radius of unit i = √(i + 0.5)·spacing.")]
        [Min(0.01f)] public float Spacing;
        [Tooltip("Depth of the disc relative to its width (1 = round; > 1 = longer along the run).")]
        [Min(0.01f)] public float DepthStretch;
        [Tooltip("How fast the center follows the steering target (1/s; the gap shrinks by 1 − e^(−sharpness·dt)).")]
        [Min(0f)] public float CenterSharpness;
        [Tooltip("How fast each unit follows its slot (1/s).")]
        [Min(0f)] public float FollowSharpness;
        [Tooltip("Spread of the per-unit pace: unit i follows at (1 − spread/2) + spread·((7i mod 10)/10).")]
        [Range(0f, 1f)] public float PaceSpread;
        [Tooltip("Distance kept between a unit's slot and the lane edge (meters).")]
        [Min(0f)] public float EdgeMargin;

        public static SwarmFormationSettings Default => new SwarmFormationSettings
        {
            Spacing = 0.25f,
            DepthStretch = 1.35f,
            CenterSharpness = 8f,
            FollowSharpness = 7f,
            PaceSpread = 0.4f,
            EdgeMargin = 0.2f,
        };
    }

    /// <summary>
    /// The sunflower (golden angle) formation and its follow formulas, pure and Burst-compatible (tested in EditMode
    /// against the reference). Offsets: x is across, y is along +Z (the run direction).
    /// </summary>
    public static class SunflowerFormation
    {
        /// <summary>Golden angle (radians) between consecutive units.</summary>
        public const double GoldenAngle = 2.39996;

        /// <summary>
        /// Offset of unit <paramref name="index"/> from the center: radius √(i + 0.5)·spacing, depth stretched. The
        /// angle i·golden angle grows past 1000 rad, so it is taken in double precision (as the reference does);
        /// in single precision the outer rings drift by millimeters. Computed once per unit, at load.
        /// </summary>
        public static float2 Offset(int index, float spacing, float depthStretch)
        {
            double radius = Math.Sqrt(index + 0.5) * spacing;
            double angle = index * GoldenAngle;
            return new float2((float)(Math.Cos(angle) * radius), (float)(Math.Sin(angle) * radius * depthStretch));
        }

        /// <summary>Radius of the disc that holds <paramref name="count"/> units (across, before the stretch).</summary>
        public static float Radius(int count, float spacing)
        {
            return count <= 0 ? 0f : math.sqrt(count - 0.5f) * spacing;
        }

        /// <summary>Fills <paramref name="offsets"/> with the offsets of units 0..Length−1 (done once, at load).</summary>
        public static void FillOffsets(Span<float2> offsets, float spacing, float depthStretch)
        {
            for (int i = 0; i < offsets.Length; i++)
            {
                offsets[i] = Offset(i, spacing, depthStretch);
            }
        }

        /// <summary>Share of the gap closed this frame by an exponential follow: 1 − e^(−sharpness·dt).</summary>
        public static float FollowFactor(float sharpness, float deltaTime)
        {
            return 1f - math.exp(-deltaTime * sharpness);
        }

        /// <summary>Pace of unit <paramref name="index"/>: (1 − spread/2) + spread·((7i mod 10)/10), 0.8–1.16 by default.</summary>
        public static float Pace(int index, float spread)
        {
            return 1f - 0.5f * spread + spread * ((index * 7) % 10) / 10f;
        }

        /// <summary>Across position of a unit's slot: center + offset, kept inside the lane minus the edge margin.</summary>
        public static float SlotX(float centerX, float offsetX, float laneCenter, float laneHalfWidth, float edgeMargin)
        {
            float limit = math.max(0f, laneHalfWidth - edgeMargin);
            return math.clamp(centerX + offsetX, laneCenter - limit, laneCenter + limit);
        }

        /// <summary>
        /// One frame of a unit following its slot. Across, the unit closes <paramref name="follow"/>·pace of the gap;
        /// along the run (relative to the center), <paramref name="follow"/> of it.
        /// </summary>
        public static float2 Follow(float2 position, float slotX, float offsetZ, float follow, float pace)
        {
            return new float2(position.x + (slotX - position.x) * follow * pace, position.y + (offsetZ - position.y) * follow);
        }
    }
}
