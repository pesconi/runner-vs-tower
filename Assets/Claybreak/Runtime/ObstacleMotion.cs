using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The motion and the deadly area of every obstacle kind (RT-6t.18), pure: where an obstacle is at a time of its clock,
    /// whether it can kill right then and whether a unit standing at (x, z) is inside what it kills. The run course
    /// (<see cref="RunObstacleCourse"/>) draws and kills with it; the level check and the reaction window read
    /// <see cref="Reach"/>; the tests drive it directly. Sizes are in metres for the game's 8 m track (the meshes are built to
    /// them, ObstacleArt); positions across follow the lane half width given.
    /// </summary>
    public static class ObstacleMotion
    {
        public const int KindCount = 5;

        /// <summary>A unit's footprint radius and height (the crowd shapes are about 0.5 m wide and 1 m tall).</summary>
        public const float UnitRadius = 0.25f;
        public const float UnitHeight = 1f;

        /// <summary>No obstacle moves faster than one cycle in this many seconds (the eye must follow it).</summary>
        public const float MinimumPeriod = 1.2f;
        public const float MaximumPeriod = 12f;

        // Swinging log: a rod from a pivot high above the track, the log across its end (axis along the run).
        public const float SwingPivotHeight = 7.6f;
        public const float SwingArm = 6.8f;
        public const float LogRadius = 0.65f;
        public const float LogLength = 2f;
        /// <summary>How far past the lane edge the log swings, so the edges are not a safe place to wait.</summary>
        public const float SwingOvershoot = 0.6f;

        // Saw: a vertical blade (in the plane across the track) half sunk in a slot, running side to side.
        public const float SawRadius = 0.95f;
        public const float SawHubHeight = 0.3f;
        public const float SawHalfThickness = 0.1f;
        /// <summary>The blade's center stops this far inside the lane edges.</summary>
        public const float SawEdgeInset = 0.3f;
        /// <summary>Spin of the blade (radians per second).</summary>
        public const float SawSpin = 14f;
        /// <summary>The slot the blade runs in reaches this far past each lane edge.</summary>
        public const float SawSlotOvershoot = 0.3f;
        public const float SawSlotDepth = 0.32f;

        // Hammer: a block over half the track that rises, waits up and slams down.
        public const float HammerHalfWidth = 1.9f;
        public const float HammerHalfDepth = 0.7f;
        public const float HammerHeadHeight = 0.9f;
        public const float HammerLift = 3.2f;
        /// <summary>Shares of the cycle: rising until, waiting up until, slamming until; then down.</summary>
        public const float HammerRiseEnd = 0.4f;
        public const float HammerHoldEnd = 0.78f;
        public const float HammerSlamEnd = 0.86f;

        // Hole: a pit across part of the track.
        public const float HoleHalfWidth = 1.8f;
        public const float HoleHalfDepth = 1.2f;

        // Roller: a spiked drum across one lane, rolling toward the crowd along a run, rising and sinking at its ends.
        public const float RollerRadius = 0.6f;
        public const float RollerHalfLength = 1.2f;
        public const float RollerRun = 10f;
        /// <summary>Share of the cycle the roller takes to rise out of the track (and to sink back at the end).</summary>
        public const float RollerRiseShare = 0.12f;

        /// <summary>Where an obstacle is and what it can do at one instant.</summary>
        public struct Pose
        {
            /// <summary>Where its mesh is placed (the log's pivot, the blade's hub, the hammer's sole, the pit, the drum's axis).</summary>
            public Vector3 Anchor;
            /// <summary>Its turn (radians) about its own axis: z for the log and the saw, x for the roller; 0 otherwise.</summary>
            public float Angle;
            /// <summary>The center of what it kills, on the ground.</summary>
            public Vector3 Hazard;
            /// <summary>True when it kills now (the log low enough, the hammer down, the roller up).</summary>
            public bool Deadly;
        }

        /// <summary>The kind's cycle when a level leaves it at 0 (seconds).</summary>
        public static float DefaultPeriod(ObstacleKind kind)
        {
            switch (kind)
            {
                case ObstacleKind.SwingingLog: return 3.2f;
                case ObstacleKind.Saw: return 2.8f;
                case ObstacleKind.Hammer: return 2.4f;
                case ObstacleKind.Roller: return 2.6f;
                default: return 0f;
            }
        }

        /// <summary>The obstacle's cycle in seconds (its own, or the kind's default).</summary>
        public static float Period(in ObstaclePlacement obstacle)
        {
            return obstacle.period > 0f ? obstacle.period : DefaultPeriod(obstacle.kind);
        }

        /// <summary>True when the kind moves (has a cycle).</summary>
        public static bool Moves(ObstacleKind kind) => kind != ObstacleKind.Hole;

        /// <summary>
        /// The stretch of track (z) where the obstacle can kill: what the level check keeps away from the start, the gates and
        /// the firing line, and what the run course tests against the crowd.
        /// </summary>
        public static void Reach(in ObstaclePlacement obstacle, out float near, out float far)
        {
            float z = obstacle.z;
            switch (obstacle.kind)
            {
                case ObstacleKind.SwingingLog:
                    near = z - LogLength * 0.5f;
                    far = z + LogLength * 0.5f;
                    break;
                case ObstacleKind.Saw:
                    near = z - SawHalfThickness;
                    far = z + SawHalfThickness;
                    break;
                case ObstacleKind.Hammer:
                    near = z - HammerHalfDepth;
                    far = z + HammerHalfDepth;
                    break;
                case ObstacleKind.Hole:
                    near = z - HoleHalfDepth;
                    far = z + HoleHalfDepth;
                    break;
                default:
                    near = z - RollerRun - RollerRadius;
                    far = z + RollerRadius;
                    break;
            }

            near -= UnitRadius;
            far += UnitRadius;
        }

        /// <summary>The obstacle at <paramref name="time"/> (seconds on its course's clock) on a lane centered on <paramref name="centerX"/>.</summary>
        public static Pose PoseAt(in ObstaclePlacement obstacle, float time, float centerX, float halfWidth)
        {
            float period = Mathf.Max(MinimumPeriod, Period(obstacle));
            float cycle = time / period + obstacle.phase;
            float u = cycle - Mathf.Floor(cycle);
            float z = obstacle.z;
            switch (obstacle.kind)
            {
                case ObstacleKind.SwingingLog:
                {
                    float angle = SwingAmplitude(halfWidth) * Mathf.Sin(2f * Mathf.PI * cycle);
                    float headX = centerX + SwingArm * Mathf.Sin(angle);
                    float headY = SwingPivotHeight - SwingArm * Mathf.Cos(angle);
                    return new Pose
                    {
                        Anchor = new Vector3(centerX, SwingPivotHeight, z),
                        Angle = angle,
                        Hazard = new Vector3(headX, 0f, z),
                        Deadly = headY - LogRadius < UnitHeight
                    };
                }
                case ObstacleKind.Saw:
                {
                    float reach = Mathf.Max(0f, halfWidth - SawEdgeInset);
                    float x = centerX - reach * Mathf.Cos(2f * Mathf.PI * cycle);
                    return new Pose
                    {
                        Anchor = new Vector3(x, SawHubHeight, z),
                        Angle = -time * SawSpin,
                        Hazard = new Vector3(x, 0f, z),
                        Deadly = true
                    };
                }
                case ObstacleKind.Hammer:
                {
                    float x = HammerX(obstacle, centerX, halfWidth);
                    float height = HammerHeight(u);
                    return new Pose
                    {
                        Anchor = new Vector3(x, height, z),
                        Hazard = new Vector3(x, 0f, z),
                        Deadly = height < UnitHeight
                    };
                }
                case ObstacleKind.Hole:
                {
                    float x = centerX + Mathf.Clamp(obstacle.across, -1f, 1f) * Mathf.Max(0f, halfWidth - HoleHalfWidth);
                    return new Pose { Anchor = new Vector3(x, 0f, z), Hazard = new Vector3(x, 0f, z), Deadly = true };
                }
                default:
                {
                    float x = centerX + Mathf.Clamp(obstacle.across, -1f, 1f) * Mathf.Max(0f, halfWidth - RollerHalfLength);
                    float rolled = RollerRun * u;
                    float rise = RollerRise(u);
                    float rollerZ = z - rolled;
                    return new Pose
                    {
                        Anchor = new Vector3(x, RollerRadius * (2f * rise - 1f), rollerZ),
                        // Rolling toward -z: the top of the drum moves back, a negative turn about x.
                        Angle = -rolled / RollerRadius,
                        Hazard = new Vector3(x, 0f, rollerZ),
                        Deadly = rise > 0.6f
                    };
                }
            }
        }

        /// <summary>True when a unit standing at (<paramref name="x"/>, <paramref name="z"/>) is inside what the obstacle kills at <paramref name="pose"/>.</summary>
        public static bool Hits(ObstacleKind kind, in Pose pose, float x, float z)
        {
            if (!pose.Deadly)
            {
                return false;
            }

            float dx = Mathf.Abs(x - pose.Hazard.x);
            float dz = Mathf.Abs(z - pose.Hazard.z);
            switch (kind)
            {
                case ObstacleKind.SwingingLog:
                    return dx <= LogRadius + UnitRadius && dz <= LogLength * 0.5f + UnitRadius;
                case ObstacleKind.Saw:
                    // The blade's width at the units' waist (half the unit up).
                    return dx <= SawHalfChord(UnitHeight * 0.5f) + UnitRadius && dz <= SawHalfThickness + UnitRadius;
                case ObstacleKind.Hammer:
                    return dx <= HammerHalfWidth + UnitRadius * 0.5f && dz <= HammerHalfDepth + UnitRadius;
                case ObstacleKind.Hole:
                    // Only a unit whose center is over the pit falls.
                    return dx <= HoleHalfWidth && dz <= HoleHalfDepth;
                default:
                    return dx <= RollerHalfLength + UnitRadius && dz <= RollerRadius + UnitRadius;
            }
        }

        /// <summary>The largest swing of the log (radians): its head reaches <see cref="SwingOvershoot"/> past the lane edges.</summary>
        public static float SwingAmplitude(float halfWidth)
        {
            return Mathf.Asin(Mathf.Clamp01((halfWidth + SwingOvershoot) / SwingArm));
        }

        /// <summary>The hammer's center across: the middle of the half its <see cref="ObstaclePlacement.across"/> picks.</summary>
        public static float HammerX(in ObstaclePlacement obstacle, float centerX, float halfWidth)
        {
            return centerX + (obstacle.across < 0f ? -0.5f : 0.5f) * halfWidth;
        }

        /// <summary>
        /// The height of the hammer's sole at <paramref name="u"/> (share of its cycle): it rises smoothly, waits up, slams
        /// down (accelerating) and stays down until the cycle ends.
        /// </summary>
        public static float HammerHeight(float u)
        {
            if (u < HammerRiseEnd)
            {
                float s = u / HammerRiseEnd;
                return HammerLift * s * s * (3f - 2f * s);
            }

            if (u < HammerHoldEnd)
            {
                return HammerLift;
            }

            if (u < HammerSlamEnd)
            {
                float s = (u - HammerHoldEnd) / (HammerSlamEnd - HammerHoldEnd);
                return HammerLift * (1f - s * s);
            }

            return 0f;
        }

        /// <summary>How far the roller is out of the track at <paramref name="u"/> (0 sunk, 1 up).</summary>
        public static float RollerRise(float u)
        {
            float s = Mathf.Clamp01(Mathf.Min(u, 1f - u) / RollerRiseShare);
            return s * s * (3f - 2f * s);
        }

        /// <summary>Half the width of the saw blade at <paramref name="height"/> above the track.</summary>
        public static float SawHalfChord(float height)
        {
            float d = height - SawHubHeight;
            return Mathf.Sqrt(Mathf.Max(0f, SawRadius * SawRadius - d * d));
        }
    }
}
