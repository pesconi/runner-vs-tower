using System.Collections.Generic;
using JungleDegree.StudioKit.Swarm;
using Unity.Mathematics;

namespace Claybreak.Editor
{
    /// <summary>
    /// RT-6t.15: the time a player has to read a run gate and steer to it, at the level's run speed (<see cref="RunPace"/>).
    /// Two windows, both from the real run camera (the rig's formulas at every crowd size up to the cap and every steering,
    /// fitted to the portrait screens the game checks, as <see cref="PropWrapGuard"/> does):
    /// <list type="bullet">
    /// <item>sight: from the moment a gate's number is legible (its capital height at least <see cref="LegibleScreenShare"/>
    /// of the screen height, about Android's 12 sp body text on the OPPO's 2354 px) to the crowd reaching it. The camera
    /// moves with the crowd, so that distance ahead is the same for every gate; the window is it over the run speed;</item>
    /// <item>gap: from the start or the previous gate pair to the next one, the time to steer across after a choice.</item>
    /// </list>
    /// Pure (no scene); the validator and the tests give it the run camera profile and the levels.
    /// </summary>
    public static class ReactionWindow
    {
        /// <summary>Shortest time from a legible gate number to the crowd at the gate (s).</summary>
        public const float MinimumSightSeconds = 3f;

        /// <summary>Shortest time from the start or a gate pair to the next pair (s).</summary>
        public const float MinimumGapSeconds = 1.6f;

        /// <summary>Capital height of the gate number as a share of the screen height that still reads (1% ≈ 23 px on the OPPO).</summary>
        public const float LegibleScreenShare = 0.01f;

        /// <summary>Capital height of Baloo 2 ExtraBold per em (the gate labels' font).</summary>
        public const float CapHeightPerEm = 0.7f;

        /// <summary>Height of the gate numbers above the track (the scene builder's pair labels).</summary>
        public const float LabelHeight = 1.3f;

        private static readonly int[] SampleArmies = { 1, 10, 25, 50, 100, 150, 200, 300, 500, 1000, 2048 };
        private const float SearchStep = 0.5f;
        private const float SearchFarthest = 200f;

        /// <summary>The window of one level (seconds), worst gate first.</summary>
        public readonly struct Result
        {
            public Result(float sightSeconds, float gapSeconds)
            {
                SightSeconds = sightSeconds;
                GapSeconds = gapSeconds;
            }

            public float SightSeconds { get; }
            public float GapSeconds { get; }
            public bool Passes => SightSeconds >= MinimumSightSeconds && GapSeconds >= MinimumGapSeconds;
        }

        /// <summary>
        /// The farthest distance ahead of the crowd at which a gate number of <paramref name="labelEm"/> reads, on every
        /// camera the run uses (crowd of up to <paramref name="crowdCap"/> on a lane of <paramref name="laneHalfWidth"/>).
        /// </summary>
        public static float LegibleDistance(in SwarmCameraSettings run, int crowdCap, float laneHalfWidth, float labelEm)
        {
            float capHeight = labelEm * CapHeightPerEm;
            float nearest = float.MaxValue;
            foreach (float aspect in PropWrapGuard.PortraitAspects)
            {
                foreach (int army in SampleArmies)
                {
                    if (army > crowdCap)
                    {
                        continue;
                    }

                    float distance = SwarmCameraMath.TargetDistance(army, run.Growth);
                    for (int side = -1; side <= 1; side++)
                    {
                        SwarmCameraPose pose = SwarmCameraMath.FitLane(run, aspect, 0f, laneHalfWidth, side * laneHalfWidth, 0f, distance);
                        // Both labels of a pair stand half a lane off the middle; the far side of the steering reads worst.
                        for (int label = -1; label <= 1; label += 2)
                        {
                            nearest = math.min(nearest, Farthest(pose, aspect, label * laneHalfWidth * 0.5f, capHeight));
                        }
                    }
                }
            }

            return nearest;
        }

        /// <summary>The level's windows given the <paramref name="legibleDistance"/> of its gate numbers.</summary>
        public static Result Of(float runSpeed, IReadOnlyList<RunGatePair> gates, float legibleDistance)
        {
            float speed = math.max(0.01f, runSpeed);
            float shortestGap = float.MaxValue;
            float previous = 0f;
            for (int i = 0; i < gates.Count; i++)
            {
                shortestGap = math.min(shortestGap, gates[i].z - previous);
                previous = gates[i].z;
            }

            return new Result(legibleDistance / speed, gates.Count == 0 ? float.MaxValue : shortestGap / speed);
        }

        // The farthest point straight ahead where the label is on screen and its capital height reads.
        private static float Farthest(SwarmCameraPose pose, float aspect, float x, float capHeight)
        {
            float3 forward = math.normalize(pose.Target - pose.Position);
            float3 right = math.normalize(math.cross(new float3(0f, 1f, 0f), forward));
            float3 up = math.cross(forward, right);
            float tanHalf = math.tan(math.radians(pose.VerticalFov) * 0.5f);
            for (float ahead = SearchFarthest; ahead > 0f; ahead -= SearchStep)
            {
                float3 bottom = new float3(x, LabelHeight - capHeight * 0.5f, ahead) - pose.Position;
                float3 top = bottom + new float3(0f, capHeight, 0f);
                float depthBottom = math.dot(bottom, forward);
                float depthTop = math.dot(top, forward);
                if (depthBottom <= 0f || depthTop <= 0f)
                {
                    continue;
                }

                float yBottom = math.dot(bottom, up) / depthBottom / tanHalf;
                float yTop = math.dot(top, up) / depthTop / tanHalf;
                float xNdc = math.dot(bottom, right) / depthBottom / (tanHalf * aspect);
                bool onScreen = yTop <= 1f && yBottom >= -1f && math.abs(xNdc) <= 1f;
                if (onScreen && (yTop - yBottom) * 0.5f >= LegibleScreenShare)
                {
                    return ahead;
                }
            }

            return 0f;
        }
    }
}
