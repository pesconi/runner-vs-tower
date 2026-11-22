using System.Collections.Generic;
using JungleDegree.StudioKit.Swarm;
using Unity.Mathematics;

namespace Claybreak.Editor
{
    /// <summary>
    /// RT-6t.13: the side props wrap to the far end of their window (<see cref="PropScatter"/>) as the camera moves on. Up to
    /// RT-6t.12 the fog hid that (it ended before the window did); with the fog near zero it would not, so the guard asks the
    /// real question instead: from every camera the game uses (the run rig at every crowd size and steering, fitted to a
    /// portrait screen like the game does, and the assault shots), is any point where a prop wraps on screen and nearer than
    /// the fog end? A point is on screen when it is in front of the camera and inside its vertical and horizontal field of
    /// view; its depth is along the view axis (what the linear fog and the far clip use). Pure (no scene); the validator
    /// gives it the scene's numbers.
    /// </summary>
    public static class PropWrapGuard
    {
        /// <summary>Portrait screens checked (width / height): 21:9, 19.5:9 (the OPPO is 1080×2354) and 16:9.</summary>
        public static readonly float[] PortraitAspects = { 9f / 21f, 9f / 19.5f, 9f / 16f };

        private static readonly int[] SampleArmies = { 0, 1, 10, 25, 50, 100, 150, 200, 300, 500, 1000, 2048 };
        private const int AcrossSamples = 9;

        /// <summary>A fixed assault shot and how far its target stands ahead of its anchor (the tower beyond the firing line).</summary>
        public struct Shot
        {
            public float3 Offset;
            public float Focus;
            public float VerticalFov;
            public float TargetAhead;
        }

        /// <summary>
        /// The nearest view depth of a wrap point any of the cameras sees (float.MaxValue when none does). Run cameras follow
        /// a crowd of up to <paramref name="crowdCap"/> units on a lane of <paramref name="laneHalfWidth"/> centered at 0;
        /// the props reach <paramref name="propTop"/> meters up.
        /// </summary>
        public static float NearestVisibleWrapDepth(in SwarmCameraSettings run, int crowdCap, float laneHalfWidth,
            in PropScatterSettings scatter, float propTop, IReadOnlyList<Shot> shots)
        {
            float nearest = float.MaxValue;
            foreach (float aspect in PortraitAspects)
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
                        float centerX = side * laneHalfWidth;
                        SwarmCameraPose pose = SwarmCameraMath.FitLane(run, aspect, 0f, laneHalfWidth, centerX, 0f, distance);
                        nearest = math.min(nearest, Nearest(pose, aspect, scatter, propTop));
                    }
                }

                if (shots == null)
                {
                    continue;
                }

                foreach (Shot shot in shots)
                {
                    SwarmCameraPose pose = SwarmCameraMath.ShotPose(float3.zero, new float3(0f, 0f, shot.TargetAhead), shot.Offset,
                        shot.Focus, shot.VerticalFov);
                    nearest = math.min(nearest, Nearest(pose, aspect, scatter, propTop));
                }
            }

            return nearest;
        }

        // The props window follows the camera: its far end is Length − Behind ahead of the camera along Z.
        private static float Nearest(SwarmCameraPose pose, float aspect, in PropScatterSettings scatter, float propTop)
        {
            float3 forward = math.normalize(pose.Target - pose.Position);
            float3 right = math.normalize(math.cross(new float3(0f, 1f, 0f), forward));
            float3 up = math.cross(forward, right);
            float tanV = math.tan(math.radians(pose.VerticalFov) * 0.5f);
            float tanH = tanV * aspect;
            float farZ = pose.Position.z + scatter.Length - scatter.Behind;
            float nearest = float.MaxValue;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < AcrossSamples; i++)
                {
                    float x = scatter.CenterX + side * (scatter.InnerX + scatter.BandWidth * i / (AcrossSamples - 1));
                    for (int h = 0; h < 2; h++)
                    {
                        float3 toPoint = new float3(x, h * propTop, farZ) - pose.Position;
                        float depth = math.dot(toPoint, forward);
                        if (depth <= 0f || math.abs(math.dot(toPoint, right)) > depth * tanH || math.abs(math.dot(toPoint, up)) > depth * tanV)
                        {
                            continue;
                        }

                        nearest = math.min(nearest, depth);
                    }
                }
            }

            return nearest;
        }
    }
}
