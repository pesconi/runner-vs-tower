using Unity.Mathematics;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>The numbers of a <see cref="SwarmCameraProfile"/> (see its fields), as a plain value.</summary>
    public struct SwarmCameraSettings
    {
        public float Growth;
        public float DistanceSharpness;
        public float BaseHeight;
        public float HeightPerDistance;
        public float BaseBack;
        public float BackPerDistance;
        public float LookAhead;
        public float FollowAcross;
        public float VerticalFov;
        public float MaxVerticalFov;
        public float FrameMargin;
        public float FitAhead;
        public float ShakeDuration;
        public float ShakeAmplitude;

        /// <summary>Reference values of the studio crowd look (the max field of view is the portrait fit's choice).</summary>
        public static SwarmCameraSettings Default => new SwarmCameraSettings
        {
            Growth = 22f,
            DistanceSharpness = 2f,
            BaseHeight = 3.2f,
            HeightPerDistance = 4.2f,
            BaseBack = 5.5f,
            BackPerDistance = 5.5f,
            LookAhead = 5f,
            FollowAcross = 0.35f,
            VerticalFov = 50f,
            MaxVerticalFov = 65f,
            FrameMargin = 0.7f,
            FitAhead = 0f,
            ShakeDuration = 0.35f,
            ShakeAmplitude = 0.6f,
        };
    }

    /// <summary>Camera pose of a swarm before the shake: where it stands, where it looks, its field of view.</summary>
    public struct SwarmCameraPose
    {
        public float3 Position;
        public float3 Target;
        public float VerticalFov;
    }

    /// <summary>
    /// The swarm camera formulas, pure (tested in EditMode against the reference). The run goes toward +Z; the
    /// camera stands behind (−Z) and above the crowd.
    /// </summary>
    public static class SwarmCameraMath
    {
        /// <summary>Distance factor the camera settles at for <paramref name="count"/> units: 1 + √count / growth.</summary>
        public static float TargetDistance(int count, float growth)
        {
            return 1f + math.sqrt(math.max(0, count)) / growth;
        }

        /// <summary>One frame of an exponential follow: current + (target − current)·(1 − e^(−sharpness·dt)).</summary>
        public static float Smooth(float current, float target, float sharpness, float deltaTime)
        {
            return current + (target - current) * (1f - math.exp(-deltaTime * sharpness));
        }

        /// <summary>
        /// Reference pose for distance factor <paramref name="distance"/>: across at lane center + FollowAcross·(crowd −
        /// lane center), height BaseHeight + HeightPerDistance·d, BaseBack + BackPerDistance·d behind the crowd,
        /// looking at the ground LookAhead ahead of it. Field of view = VerticalFov (no fit).
        /// </summary>
        public static SwarmCameraPose Pose(in SwarmCameraSettings settings, float laneCenter, float centerX, float centerZ, float distance)
        {
            float x = laneCenter + (centerX - laneCenter) * settings.FollowAcross;
            return new SwarmCameraPose
            {
                Position = new float3(x, settings.BaseHeight + settings.HeightPerDistance * distance,
                    centerZ - (settings.BaseBack + settings.BackPerDistance * distance)),
                Target = new float3(x, 0f, centerZ + settings.LookAhead),
                VerticalFov = settings.VerticalFov,
            };
        }

        /// <summary>
        /// Fits the lane across a screen of <paramref name="aspect"/> (width / height): the ground points
        /// (lane center ± halfWidth, 0, fitZ) must be inside the view. The field of view opens from VerticalFov up to
        /// MaxVerticalFov; if that is not enough the camera backs away from its target along the view line (same
        /// angle, so the look is kept). With <paramref name="halfWidth"/> ≤ 0 the pose is returned unchanged.
        /// </summary>
        public static SwarmCameraPose FitAcross(SwarmCameraPose pose, in SwarmCameraSettings settings, float aspect,
            float laneCenter, float halfWidth, float fitZ)
        {
            if (halfWidth <= 0f || aspect <= 0f)
            {
                return pose;
            }

            float3 back = pose.Position - pose.Target;
            float length = math.length(back);
            float3 forward = -back / length;
            float3 right = math.normalize(math.cross(new float3(0f, 1f, 0f), forward));
            // Camera-space x and depth of the two lane edges, measured from the target: moving the camera back along
            // the view line adds to the depth only (right is perpendicular to it).
            float tanNeeded = 0f;
            float2 edges = default;
            for (int side = 0; side < 2; side++)
            {
                float3 edge = new float3(laneCenter + (side == 0 ? -halfWidth : halfWidth), 0f, fitZ) - pose.Target;
                float across = math.abs(math.dot(edge, right));
                float depth = math.dot(edge, forward);
                edges[side] = depth;
                tanNeeded = math.max(tanNeeded, across / math.max(1e-4f, depth + length));
            }

            float fov = math.degrees(2f * math.atan(tanNeeded / aspect));
            if (fov <= settings.VerticalFov)
            {
                pose.VerticalFov = settings.VerticalFov;
                return pose;
            }

            if (fov <= settings.MaxVerticalFov)
            {
                pose.VerticalFov = fov;
                return pose;
            }

            pose.VerticalFov = settings.MaxVerticalFov;
            float tanAllowed = math.tan(math.radians(settings.MaxVerticalFov) * 0.5f) * aspect;
            float scale = 1f;
            for (int side = 0; side < 2; side++)
            {
                float3 edge = new float3(laneCenter + (side == 0 ? -halfWidth : halfWidth), 0f, fitZ) - pose.Target;
                float across = math.abs(math.dot(edge, right));
                scale = math.max(scale, (across / tanAllowed - edges[side]) / length);
            }

            pose.Position = pose.Target + back * scale;
            return pose;
        }

        /// <summary>
        /// The pose for a crowd at <paramref name="centerX"/> on a lane of <paramref name="laneHalfWidth"/>, fitted to a
        /// screen of <paramref name="aspect"/> with the lane plus FrameMargin on each side across.
        /// The fit is solved once, for the smallest crowd (d = 1) at the lane edge (the camera's worst case), and kept
        /// as a fixed extra distance along the view line plus the field of view it needs; the reference growth with the
        /// count is added on top. So the camera still backs away as the crowd grows (the reference feel), the lane
        /// fits at every count (a larger d only widens the view) and steering across never zooms. No lane (≤ 0) or a
        /// screen where the lane already fits: the reference pose.
        /// </summary>
        public static SwarmCameraPose FitLane(in SwarmCameraSettings settings, float aspect, float laneCenter, float laneHalfWidth,
            float centerX, float centerZ, float distance)
        {
            SwarmCameraPose pose = Pose(settings, laneCenter, centerX, centerZ, distance);
            if (laneHalfWidth <= 0f)
            {
                return pose;
            }

            SwarmCameraPose edge = Pose(settings, laneCenter, laneCenter + laneHalfWidth, centerZ, 1f);
            SwarmCameraPose fitted = FitAcross(edge, settings, aspect, laneCenter, laneHalfWidth + settings.FrameMargin,
                centerZ + settings.FitAhead);
            float extra = math.distance(fitted.Position, fitted.Target) - math.distance(edge.Position, edge.Target);
            float3 back = pose.Position - pose.Target;
            pose.Position += math.normalize(back) * extra;
            pose.VerticalFov = fitted.VerticalFov;
            return pose;
        }

        /// <summary>How much farther a pose is from its target than the reference pose at the same distance factor (≥ 1).</summary>
        public static float FramingScale(in SwarmCameraSettings settings, SwarmCameraPose pose, float distance)
        {
            SwarmCameraPose reference = Pose(settings, 0f, 0f, 0f, distance);
            return math.distance(pose.Position, pose.Target) / math.distance(reference.Position, reference.Target);
        }

        /// <summary>
        /// Pose of a fixed shot (<see cref="SwarmCameraShot"/>): the camera at <paramref name="anchor"/> +
        /// <paramref name="offset"/>, looking at the point <paramref name="focus"/> of the way from the anchor to
        /// <paramref name="target"/>, with field of view <paramref name="verticalFov"/>.
        /// </summary>
        public static SwarmCameraPose ShotPose(float3 anchor, float3 target, float3 offset, float focus, float verticalFov)
        {
            return new SwarmCameraPose
            {
                Position = anchor + offset,
                Target = math.lerp(anchor, target, focus),
                VerticalFov = verticalFov,
            };
        }

        /// <summary>Jitter (x, y, meters) for <paramref name="shakeLeft"/> seconds of shake and two uniform 0..1 draws.</summary>
        public static float2 ShakeJitter(float shakeLeft, float amplitude, float2 uniform)
        {
            return (uniform - 0.5f) * (math.max(0f, shakeLeft) * amplitude);
        }
    }
}
