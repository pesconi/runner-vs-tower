using UnityEngine;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// A fixed shot a <see cref="SwarmCameraRig"/> can hold instead of following the crowd (for example while the crowd
    /// stands and something ahead of it is the show), as data: the camera stands at an offset from an anchor point and
    /// looks at a point part of the way from the anchor to a target, with its own field of view, reached by a smooth
    /// blend. The rig's shake still applies. A game keeps one asset per framing.
    /// </summary>
    [CreateAssetMenu(menuName = "StudioKit/Swarm Camera Shot", fileName = "SwarmCameraShot")]
    public sealed class SwarmCameraShot : ScriptableObject
    {
        [Tooltip("Camera position relative to the anchor (meters).")]
        public Vector3 Offset = new Vector3(0f, 17f, -11f);
        [Tooltip("Point looked at, as a share of the way from the anchor to the target.")]
        [Range(0f, 1f)] public float Focus = 0.55f;
        [Tooltip("Vertical field of view of the shot (degrees).")]
        [Range(10f, 120f)] public float VerticalFov = 60f;
        [Tooltip("Time to blend from where the camera is to the shot (s).")]
        [Min(0.01f)] public float BlendSeconds = 0.8f;

        /// <summary>The pose of this shot for <paramref name="anchor"/> and <paramref name="target"/>.</summary>
        public SwarmCameraPose Pose(Vector3 anchor, Vector3 target)
        {
            return SwarmCameraMath.ShotPose(anchor, target, Offset, Focus, VerticalFov);
        }
    }
}
