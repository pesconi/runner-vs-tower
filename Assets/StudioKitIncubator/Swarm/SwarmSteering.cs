using System;
using Unity.Mathematics;
using UnityEngine;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>How a finger drag steers a swarm across its lane. Defaults are the reference values of the crowd look.</summary>
    [Serializable]
    public struct SwarmDragSettings
    {
        [Tooltip("Meters the target moves for a drag across the whole screen width.")]
        [Min(0f)] public float MetersPerScreenWidth;
        [Tooltip("Distance kept between the target and the lane edge (meters).")]
        [Min(0f)] public float EdgeMargin;
        [Tooltip("Meters the target moves per key press (keyboard, Editor).")]
        [Min(0f)] public float KeyStep;

        public static SwarmDragSettings Default => new SwarmDragSettings
        {
            MetersPerScreenWidth = 16f,
            EdgeMargin = 0.5f,
            KeyStep = 1.5f,
        };
    }

    /// <summary>
    /// Steering formulas, pure (tested in EditMode against the reference): the target is where it was when the finger
    /// went down plus the drag measured in screen widths, kept inside the lane minus a margin. Reading the pointer is
    /// the game's job.
    /// </summary>
    public static class SwarmSteering
    {
        /// <summary>Target across for a drag from <paramref name="pressX"/> to <paramref name="pointerX"/> (pixels).</summary>
        public static float DragTarget(float targetAtPress, float pressX, float pointerX, float screenWidth,
            in SwarmDragSettings settings, float laneCenter, float laneHalfWidth)
        {
            float moved = screenWidth > 0f ? (pointerX - pressX) / screenWidth * settings.MetersPerScreenWidth : 0f;
            return Clamp(targetAtPress + moved, settings, laneCenter, laneHalfWidth);
        }

        /// <summary>Target after a key step to the right (+1) or left (−1).</summary>
        public static float KeyTarget(float target, int direction, in SwarmDragSettings settings, float laneCenter, float laneHalfWidth)
        {
            return Clamp(target + math.sign(direction) * settings.KeyStep, settings, laneCenter, laneHalfWidth);
        }

        /// <summary>Keeps a target inside the lane minus the margin (no lane: unchanged).</summary>
        public static float Clamp(float target, in SwarmDragSettings settings, float laneCenter, float laneHalfWidth)
        {
            if (laneHalfWidth <= 0f)
            {
                return target;
            }

            float limit = math.max(0f, laneHalfWidth - settings.EdgeMargin);
            return math.clamp(target, laneCenter - limit, laneCenter + limit);
        }
    }
}
