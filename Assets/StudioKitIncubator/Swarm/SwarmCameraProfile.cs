using UnityEngine;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// How a <see cref="SwarmCameraRig"/> frames a swarm, as data: it sits behind and above the crowd and backs away as
    /// the crowd grows (distance factor d = 1 + √count / growth), follows it across, shakes on demand and opens the
    /// field of view, then backs away further, until the lane fits a narrow (portrait) screen. Defaults are the
    /// reference values of the studio crowd look; a game keeps one asset per moment (for example one per game phase).
    /// </summary>
    [CreateAssetMenu(menuName = "StudioKit/Swarm Camera Profile", fileName = "SwarmCameraProfile")]
    public sealed class SwarmCameraProfile : ScriptableObject
    {
        [Header("Distance (d = 1 + √count / growth)")]
        [Tooltip("The larger, the slower the camera backs away as the crowd grows.")]
        [Min(0.01f)] public float Growth = 22f;
        [Tooltip("How fast d follows the count (1/s; the gap shrinks by 1 − e^(−sharpness·dt)).")]
        [Min(0f)] public float DistanceSharpness = 2f;

        [Header("Pose (meters; height and back grow with d)")]
        public float BaseHeight = 3.2f;
        public float HeightPerDistance = 4.2f;
        [Tooltip("Distance behind the crowd center along the run at d = 0.")]
        public float BaseBack = 5.5f;
        public float BackPerDistance = 5.5f;
        [Tooltip("The camera looks at the ground this far ahead of the crowd center.")]
        public float LookAhead = 5f;
        [Tooltip("Share of the crowd's offset from the lane center that the camera follows across (0 = fixed).")]
        [Range(0f, 1f)] public float FollowAcross = 0.35f;

        [Header("Portrait fit")]
        [Tooltip("Vertical field of view (degrees) when the lane already fits.")]
        [Range(10f, 120f)] public float VerticalFov = 50f;
        [Tooltip("Widest vertical field of view (degrees); past it the camera backs away instead.")]
        [Range(10f, 120f)] public float MaxVerticalFov = 65f;
        [Tooltip("Meters added to each side of the lane that must stay on screen (rails, gate posts).")]
        [Min(0f)] public float FrameMargin = 0.7f;
        [Tooltip("The lane must fit across at this distance ahead of the crowd center (0 = at the crowd).")]
        public float FitAhead;

        [Header("Shake")]
        [Tooltip("Length of a shake (s).")]
        [Min(0f)] public float ShakeDuration = 0.35f;
        [Tooltip("Jitter per second of shake left (meters): the jitter is ±(time left · amplitude)/2 on x and y.")]
        [Min(0f)] public float ShakeAmplitude = 0.6f;

        /// <summary>The profile's numbers for the pure formulas.</summary>
        public SwarmCameraSettings Settings => new SwarmCameraSettings
        {
            Growth = Growth,
            DistanceSharpness = DistanceSharpness,
            BaseHeight = BaseHeight,
            HeightPerDistance = HeightPerDistance,
            BaseBack = BaseBack,
            BackPerDistance = BackPerDistance,
            LookAhead = LookAhead,
            FollowAcross = FollowAcross,
            VerticalFov = VerticalFov,
            MaxVerticalFov = Mathf.Max(VerticalFov, MaxVerticalFov),
            FrameMargin = FrameMargin,
            FitAhead = FitAhead,
            ShakeDuration = ShakeDuration,
            ShakeAmplitude = ShakeAmplitude,
        };
    }
}
