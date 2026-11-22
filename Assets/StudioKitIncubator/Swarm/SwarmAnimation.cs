using System;
using JungleDegree.StudioKit.Toon;
using Unity.Mathematics;
using UnityEngine;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// The procedural life of a live unit on top of its hop (no skeleton): the sideways lean into its across speed, the
    /// elastic pop when it is born (after a small random delay) and how much its blob shadow shrinks when it is up in the
    /// air. Defaults are the reference values of the studio crowd look.
    /// </summary>
    [Serializable]
    public struct SwarmAnimationSettings
    {
        [Tooltip("Sideways lean per m/s of across speed (radians); the unit leans into the direction it moves.")]
        [Min(0f)] public float TiltPerSpeed;
        [Tooltip("Largest sideways lean (radians).")]
        [Min(0f)] public float TiltLimit;
        [Tooltip("Length of the elastic pop of a new unit (s).")]
        [Min(0.001f)] public float BirthDuration;
        [Tooltip("New units start popping after a random delay of 0 up to this (s), so a group does not pop as one.")]
        [Min(0f)] public float BirthDelayMax;
        [Tooltip("Share of the blob shadow lost at the top of the hop (the shadow shrinks as the unit goes up).")]
        [Range(0f, 1f)] public float ShadowShrink;

        public static SwarmAnimationSettings Default => new SwarmAnimationSettings
        {
            TiltPerSpeed = 0.06f,
            TiltLimit = 0.5f,
            BirthDuration = 0.45f,
            BirthDelayMax = 0.25f,
            ShadowShrink = 0.35f,
        };
    }

    /// <summary>
    /// Formulas of the live unit animation, pure and Burst-compatible (tested in EditMode against the reference).
    /// Axes: x across, y up, z the front (+Z, the run direction).
    /// </summary>
    public static class SwarmAnimation
    {
        /// <summary>
        /// Sideways lean (rotation about Z, radians) for an across speed: −speed·gain kept within ±limit; moving right
        /// (+x) tips the top to the right.
        /// </summary>
        public static float Tilt(float acrossSpeed, in SwarmAnimationSettings settings)
        {
            return math.clamp(-acrossSpeed * settings.TiltPerSpeed, -settings.TiltLimit, settings.TiltLimit);
        }

        /// <summary>
        /// Scale of a unit born at <paramref name="bornAt"/> (its pop start), at <paramref name="time"/>: 0 before the
        /// start, then the elastic pop 1 − 2^(−10a)·cos(3.3πa) with a = age / duration (overshoots past 1 and settles),
        /// 1 once the pop is over.
        /// </summary>
        public static float BirthScale(float time, float bornAt, float duration)
        {
            float a = (time - bornAt) / duration;
            if (a <= 0f)
            {
                return 0f;
            }

            if (a >= 1f)
            {
                return 1f;
            }

            return 1f - math.exp2(-10f * a) * math.cos(a * math.PI * 3.3f);
        }

        /// <summary>Scale of the blob shadow (1 = the full disc) for a bounce 0..1 and the birth scale.</summary>
        public static float ShadowScale(float bounce, float birthScale, in SwarmAnimationSettings settings)
        {
            return (1f - settings.ShadowShrink * bounce) * birthScale;
        }

        /// <summary>
        /// The drawn transform M = T(position) · Rx(lean) · Rz(tilt) · S(scaleXZ, scaleY, scaleXZ) as the three rows of
        /// an instance (translation in w). Lean tips the top toward +Z; tilt tips it toward −x for a positive angle.
        /// </summary>
        public static void Compose(ref ToonInstance instance, float3 position, float lean, float tilt, float scaleXZ, float scaleY)
        {
            math.sincos(lean, out float sl, out float cl);
            math.sincos(tilt, out float st, out float ct);
            // Rx · Rz = [[ct, −st, 0], [cl·st, cl·ct, −sl], [sl·st, sl·ct, cl]]; S scales the columns.
            instance.Row0 = new float4(ct * scaleXZ, -st * scaleY, 0f, position.x);
            instance.Row1 = new float4(cl * st * scaleXZ, cl * ct * scaleY, -sl * scaleXZ, position.y);
            instance.Row2 = new float4(sl * st * scaleXZ, sl * ct * scaleY, cl * scaleXZ, position.z);
        }
    }
}
