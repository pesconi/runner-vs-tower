using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// Assault framing (risk R-D5): the kit's CameraFollow frames the crowd during the run; at the
    /// firing line it hands over to this fixed shot that shows the lanes, the defenses and the tower.
    /// </summary>
    public sealed class AssaultCamera : MonoBehaviour
    {
        [Tooltip("Camera position relative to the firing line (centre of the track).")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 17f, -11f);
        [Tooltip("Point looked at, as a fraction of the way from the firing line to the tower.")]
        [SerializeField, Range(0f, 1f)] private float focus = 0.55f;
        [SerializeField, Min(0.1f)] private float blendSeconds = 0.8f;

        private Vector3 fromPosition;
        private Quaternion fromRotation;
        private Vector3 toPosition;
        private Quaternion toRotation;
        private float time = -1f;

        public void Begin(Vector3 firingLine, Vector3 towerFront)
        {
            fromPosition = transform.position;
            fromRotation = transform.rotation;
            toPosition = firingLine + offset;
            Vector3 lookAt = Vector3.Lerp(firingLine, towerFront, focus);
            toRotation = Quaternion.LookRotation(lookAt - toPosition, Vector3.up);
            time = 0f;
            enabled = true;
        }

        public void Stop()
        {
            time = -1f;
            enabled = false;
        }

        private void LateUpdate()
        {
            if (time < 0f)
            {
                return;
            }

            time = Mathf.Min(time + Time.deltaTime, blendSeconds);
            float t = Mathf.SmoothStep(0f, 1f, time / blendSeconds);
            transform.SetPositionAndRotation(
                Vector3.Lerp(fromPosition, toPosition, t),
                Quaternion.Slerp(fromRotation, toRotation, t));
        }
    }
}
