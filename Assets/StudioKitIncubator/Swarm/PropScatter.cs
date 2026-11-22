using System;
using JungleDegree.StudioKit.Toon;
using Unity.Mathematics;
using UnityEngine;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// Where the side props stand: a window along Z that follows an anchor (the camera), from <see cref="Behind"/> meters
    /// behind it to <see cref="Length"/> meters further, and a band on each side of the lane, from <see cref="InnerX"/>
    /// to <see cref="InnerX"/> + <see cref="BandWidth"/> meters away from <see cref="CenterX"/>.
    /// </summary>
    [Serializable]
    public struct PropScatterSettings
    {
        [Tooltip("Meters behind the anchor where a prop leaves the window (and comes back at the far end).")]
        [Min(0f)] public float Behind;
        [Tooltip("Length of the window along Z (m).")]
        [Min(1f)] public float Length;
        [Tooltip("Across center of the lane the props stand beside.")]
        public float CenterX;
        [Tooltip("Nearest distance from the center across (m): keeps the props off the lane.")]
        [Min(0f)] public float InnerX;
        [Tooltip("Width of the band on each side (m).")]
        [Min(0f)] public float BandWidth;

        public static PropScatterSettings Default => new PropScatterSettings
        {
            Behind = 4f,
            Length = 90f,
            CenterX = 0f,
            InnerX = 8.5f,
            BandWidth = 22f,
        };

        /// <summary>Null when usable, otherwise what is wrong.</summary>
        public string Validate()
        {
            if (!(Length >= 1f) || !(Behind >= 0f) || !(InnerX >= 0f) || !(BandWidth >= 0f) || float.IsNaN(CenterX))
            {
                return "the scatter needs a window of 1 m or more and no negative distance";
            }

            return null;
        }
    }

    /// <summary>
    /// Placement of the recycled side props, as a pure function of the anchor: prop <c>index</c> of layer <c>layer</c> has a
    /// fixed phase along the window; it stands at the one Z of its sequence (phase + lap) × length that falls in the window,
    /// and each lap draws a new side, distance, scale and turn from a hash of (seed, layer, index, lap). When the anchor
    /// moves on, a prop left behind wraps to the far end somewhere else, like the reference prototype's recycling, but
    /// with no state: the same anchor always gives the same layout (forward, back to the start, or a jump).
    /// </summary>
    public static class PropScatter
    {
        /// <summary>Z where the window starts for <paramref name="anchorZ"/> (props behind it have wrapped).</summary>
        public static float WindowStart(in PropScatterSettings settings, float anchorZ) => anchorZ - settings.Behind;

        /// <summary>The lap of the prop in the window for <paramref name="anchorZ"/> (it grows as the anchor moves on).</summary>
        public static int Lap(in PropScatterSettings settings, uint seed, int layer, int index, float anchorZ)
        {
            float phase = Phase(seed, layer, index);
            return (int)math.ceil(WindowStart(settings, anchorZ) / settings.Length - phase);
        }

        /// <summary>
        /// The transform of the prop (yaw about Y, uniform scale within <paramref name="scaleRange"/>, on the ground) as a toon
        /// instance with a white tint and no tint mask use.
        /// </summary>
        public static ToonInstance Place(in PropScatterSettings settings, uint seed, int layer, int index, float anchorZ, float2 scaleRange)
        {
            int lap = Lap(settings, seed, layer, index, anchorZ);
            float z = (Phase(seed, layer, index) + lap) * settings.Length;
            var random = new Unity.Mathematics.Random(math.max(1u, math.hash(new uint4(seed, (uint)layer, (uint)index, (uint)lap))));
            float side = random.NextBool() ? 1f : -1f;
            float x = settings.CenterX + side * (settings.InnerX + random.NextFloat() * settings.BandWidth);
            float scale = math.lerp(scaleRange.x, scaleRange.y, random.NextFloat());
            math.sincos(random.NextFloat() * 2f * math.PI, out float sin, out float cos);
            return new ToonInstance
            {
                Row0 = new float4(scale * cos, 0f, scale * sin, x),
                Row1 = new float4(0f, scale, 0f, 0f),
                Row2 = new float4(-scale * sin, 0f, scale * cos, z),
                Tint = new float4(1f, 1f, 1f, 0f),
            };
        }

        private static float Phase(uint seed, int layer, int index)
        {
            uint hash = math.hash(new uint3(seed ^ 0x9E3779B9u, (uint)layer, (uint)index));
            return (hash >> 8) * (1f / 16777216f);
        }
    }
}
