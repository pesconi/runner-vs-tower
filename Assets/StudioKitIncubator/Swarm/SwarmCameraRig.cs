using Unity.Mathematics;
using UnityEngine;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// Drives a camera after a <see cref="SwarmRenderer"/> with a <see cref="SwarmCameraProfile"/>: it backs away as
    /// the crowd grows (smoothly), follows it across, keeps the whole lane on a narrow (portrait) screen (framed for the
    /// crowd anywhere on the lane, so steering does not zoom; see <see cref="SwarmCameraMath.FitLane"/>) and shakes on
    /// <see cref="Shake"/>. It can also hold a fixed <see cref="SwarmCameraShot"/> (blended to, shake included) until
    /// released. No allocation per frame.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class SwarmCameraRig : MonoBehaviour
    {
        [SerializeField] private SwarmCameraProfile profile;
        [SerializeField] private SwarmRenderer swarm;
        [SerializeField] private uint seed = 1;
        [Tooltip("Longest frame step of the follow and the shake (s), as the swarm simulation.")]
        [Min(0.001f)] [SerializeField] private float maxDeltaTime = 0.05f;

        private Camera view;
        private float distance;
        private float shakeLeft;
        private Unity.Mathematics.Random random;
        private bool placed;
        private SwarmCameraShot shot;
        private Vector3 shotFromPosition;
        private Quaternion shotFromRotation;
        private float shotFromFov;
        private Vector3 shotPosition;
        private Quaternion shotRotation;
        private float shotFov;
        private float shotTime;

        /// <summary>The distance factor now (1 + √count / growth once settled).</summary>
        public float Distance => distance;

        /// <summary>
        /// How much farther the camera stands than the reference framing at the same count (1 on wide screens, more on
        /// portrait screens where the lane has to fit). Distance cues tuned for the reference (fog) scale by it.
        /// </summary>
        public float FramingScale { get; private set; } = 1f;

        /// <summary>Seconds of shake left.</summary>
        public float ShakeLeft => shakeLeft;

        public SwarmCameraProfile Profile => profile;

        /// <summary>The fixed shot held now (null while following the crowd).</summary>
        public SwarmCameraShot HeldShot => shot;

        /// <summary>
        /// Leaves the crowd and blends from where the camera is to <paramref name="fixedShot"/> framed for
        /// <paramref name="anchor"/> and <paramref name="target"/>; it stays there until <see cref="ReleaseShot"/>.
        /// </summary>
        public void HoldShot(SwarmCameraShot fixedShot, Vector3 anchor, Vector3 target)
        {
            if (fixedShot == null)
            {
                Debug.LogError("[SwarmCamera] a shot is required", this);
                return;
            }

            SwarmCameraPose pose = fixedShot.Pose(anchor, target);
            shot = fixedShot;
            shotFromPosition = transform.position;
            shotFromRotation = transform.rotation;
            shotFromFov = view != null ? view.fieldOfView : fixedShot.VerticalFov;
            shotPosition = pose.Position;
            shotRotation = Quaternion.LookRotation((Vector3)pose.Target - shotPosition, Vector3.up);
            shotFov = pose.VerticalFov;
            shotTime = 0f;
            shakeLeft = 0f; // a shake from before does not carry into the shot
        }

        /// <summary>Back to following the crowd, at its settled framing at once.</summary>
        public void ReleaseShot()
        {
            shot = null;
            Snap();
        }

        /// <summary>Starts a shake of the profile's length (a bad gate, a hit); a shake in progress restarts.</summary>
        public void Shake()
        {
            if (profile != null)
            {
                shakeLeft = profile.ShakeDuration;
            }
        }

        /// <summary>Jumps to the settled distance for the current count (scene start, after a cut).</summary>
        public void Snap()
        {
            if (profile != null && swarm != null)
            {
                distance = SwarmCameraMath.TargetDistance(swarm.Count, profile.Growth);
                placed = true;
            }
        }

        private void Awake()
        {
            view = GetComponent<Camera>();
            random = new Unity.Mathematics.Random(seed == 0 ? 1u : seed);
            if (profile == null || swarm == null)
            {
                Debug.LogError("[SwarmCamera] profile and swarm are required", this);
                enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (!placed)
            {
                Snap();
            }

            SwarmCameraSettings settings = profile.Settings;
            float deltaTime = Mathf.Min(Time.deltaTime, maxDeltaTime);
            if (shot != null)
            {
                HoldShotFrame(settings, deltaTime, Time.deltaTime);
                return;
            }

            distance = SwarmCameraMath.Smooth(distance, SwarmCameraMath.TargetDistance(swarm.Count, settings.Growth),
                settings.DistanceSharpness, deltaTime);
            shakeLeft = Mathf.Max(0f, shakeLeft - deltaTime);

            Vector3 origin = swarm.transform.position;
            SwarmCameraPose pose = SwarmCameraMath.FitLane(settings, view.aspect, origin.x, swarm.LaneHalfWidth,
                swarm.CenterX, origin.z, distance);
            FramingScale = SwarmCameraMath.FramingScale(settings, pose, distance);

            float2 jitter = SwarmCameraMath.ShakeJitter(shakeLeft, settings.ShakeAmplitude, random.NextFloat2());
            Vector3 position = (Vector3)pose.Position + new Vector3(jitter.x, jitter.y, 0f);
            transform.SetPositionAndRotation(position, Quaternion.LookRotation((Vector3)pose.Target - position));
            view.fieldOfView = pose.VerticalFov;
        }

        // The blend runs on the real frame time (it lasts BlendSeconds even on slow frames); the shake on the clamped one.
        private void HoldShotFrame(in SwarmCameraSettings settings, float deltaTime, float blendDeltaTime)
        {
            shakeLeft = Mathf.Max(0f, shakeLeft - deltaTime);
            shotTime = Mathf.Min(shotTime + blendDeltaTime, shot.BlendSeconds);
            float t = Mathf.SmoothStep(0f, 1f, shotTime / shot.BlendSeconds);
            float2 jitter = SwarmCameraMath.ShakeJitter(shakeLeft, settings.ShakeAmplitude, random.NextFloat2());
            Vector3 position = Vector3.Lerp(shotFromPosition, shotPosition, t) + new Vector3(jitter.x, jitter.y, 0f);
            transform.SetPositionAndRotation(position, Quaternion.Slerp(shotFromRotation, shotRotation, t));
            view.fieldOfView = Mathf.Lerp(shotFromFov, shotFov, t);
        }
    }
}
