using System;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The run's obstacles (RT-6t.18, the user on 2026-11-23: "tem que ter algo para atrapalhar e matar a crowd"). Each one is
    /// procedural (no collider, no animation clip), drawn instanced (one call per kind) and kills the units it touches.
    /// Values are stable: new kinds go at the end.
    /// </summary>
    public enum ObstacleKind
    {
        /// <summary>A spiked log on a long rod, swinging across the track like a pendulum (slow at the ends).</summary>
        SwingingLog,
        /// <summary>A spinning saw blade running from side to side in a slot across the track.</summary>
        Saw,
        /// <summary>A big hammer over one half of the track: it rises slowly and slams down.</summary>
        Hammer,
        /// <summary>A pit in part of the track: units over it fall.</summary>
        Hole,
        /// <summary>A spiked roller rolling down one lane toward the crowd, rising out of the track and sinking back.</summary>
        Roller
    }

    /// <summary>One obstacle of a level's run (level data, like the gates and the crates).</summary>
    [Serializable]
    public struct ObstaclePlacement
    {
        public ObstacleKind kind;

        [Tooltip("Distance along the track (m). The roller starts here and rolls back toward the crowd.")]
        [Min(1f)] public float z;

        [Tooltip("Where across the track, from -1 (left edge) to 1 (right edge) of the room the obstacle has: the hammer's half (below 0 left, else right), the hole's and the roller's lane. The log and the saw sweep the whole track and ignore it.")]
        [Range(-1f, 1f)] public float across;

        [Tooltip("Seconds per cycle (swing, saw run, hammer blow, roller run); 0 = the kind's default. The hole ignores it.")]
        [Min(0f)] public float period;

        [Tooltip("Where in its cycle the obstacle starts (0-1), so two of a kind do not move in step.")]
        [Range(0f, 1f)] public float phase;

        public ObstaclePlacement(ObstacleKind kind, float z, float across = 0f, float period = 0f, float phase = 0f)
        {
            this.kind = kind;
            this.z = z;
            this.across = across;
            this.period = period;
            this.phase = phase;
        }
    }
}
