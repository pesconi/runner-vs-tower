using System;
using Unity.Mathematics;
using UnityEngine;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// Shape of the procedural hop of every unit (no skeleton): bounce h = |sin(t·frequency + phase)|, lift y = h·height,
    /// squash sy = squashBase + squashAmount·h with the volume kept (sx = sz = 1/√sy), and a constant lean toward the
    /// front (+Z). Defaults are the reference values of the studio crowd look.
    /// </summary>
    [Serializable]
    public struct SwarmMotionSettings
    {
        [Tooltip("Hop speed (radians of the bounce sine per second).")]
        public float Frequency;
        [Tooltip("Height of the hop at the top of the bounce (meters).")]
        public float HopHeight;
        [Tooltip("Vertical scale at the bottom of the bounce (squashed).")]
        public float SquashBase;
        [Tooltip("Vertical scale added at the top of the bounce (stretched).")]
        public float SquashAmount;
        [Tooltip("Lean of the top toward the front (+Z), radians.")]
        public float ForwardLean;

        public static SwarmMotionSettings Default => new SwarmMotionSettings
        {
            Frequency = 9.5f,
            HopHeight = 0.2f,
            SquashBase = 0.8f,
            SquashAmount = 0.32f,
            ForwardLean = 0.12f,
        };
    }

    /// <summary>The hop and formation formulas, pure and Burst-compatible (tested in EditMode).</summary>
    public static class SwarmMotion
    {
        /// <summary>Golden angle (radians) of the sunflower formation.</summary>
        public const float GoldenAngle = 2.39996f;

        /// <summary>Bounce 0..1: 0 on the ground, 1 at the top of the hop.</summary>
        public static float Bounce(float time, float phase, float frequency)
        {
            return math.abs(math.sin(time * frequency + phase));
        }

        /// <summary>Vertical scale of the squash for a bounce h.</summary>
        public static float SquashY(float bounce, in SwarmMotionSettings settings)
        {
            return settings.SquashBase + settings.SquashAmount * bounce;
        }

        /// <summary>Horizontal scale that keeps the volume for a vertical scale sy.</summary>
        public static float SquashXZ(float squashY)
        {
            return math.rsqrt(squashY);
        }

        /// <summary>
        /// Offset of unit <paramref name="index"/> in a sunflower (golden angle) disc: radius √(i + 0.5)·spacing,
        /// depth stretched by <paramref name="depthStretch"/>. x is across, y is along +Z.
        /// </summary>
        public static float2 SunflowerOffset(int index, float spacing, float depthStretch)
        {
            float radius = math.sqrt(index + 0.5f) * spacing;
            float angle = index * GoldenAngle;
            return new float2(math.cos(angle) * radius, math.sin(angle) * radius * depthStretch);
        }

        /// <summary>Radius of the sunflower disc that holds <paramref name="count"/> units (across, before the stretch).</summary>
        public static float SunflowerRadius(int count, float spacing)
        {
            return count <= 0 ? 0f : math.sqrt(count - 0.5f) * spacing;
        }
    }
}
