using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// Assault framing (risk R-D5): the swarm camera rig frames the crowd during the run (RT-6t.4); at the
    /// firing line it hands over to this fixed shot that shows the lanes, the defenses and the tower.
    /// </summary>
    public sealed class AssaultCamera : MonoBehaviour
    {
        [Tooltip("Camera position relative to the firing line (centre of the track).")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 17f, -11f);
        [Tooltip("Point looked at, as a fraction of the way from the firing line to the tower.")]
        [SerializeField, Range(0f, 1f)] private float focus = 0.55f;
        [Tooltip("Extra offset for the boss, whose tower is taller.")]
        [SerializeField] private Vector3 bossOffset = new Vector3(0f, 3f, -3f);
        [SerializeField, Min(0.1f)] private float blendSeconds = 0.8f;
        [Tooltip("Vertical field of view of the assault shot; the run camera rig changes it with the crowd and the screen.")]
        [SerializeField, Range(20f, 90f)] private float fieldOfView = 60f;

        private Vector3 fromPosition;
        private Quaternion fromRotation;
        private Vector3 toPosition;
        private Quaternion toRotation;
        private float fromFov;
        private float time = -1f;
        private Camera view;

        public float FieldOfView => fieldOfView;

        public void Begin(Vector3 firingLine, Vector3 towerFront, bool boss)
        {
            fromPosition = transform.position;
            fromRotation = transform.rotation;
            toPosition = firingLine + offset + (boss ? bossOffset : Vector3.zero);
            Vector3 lookAt = Vector3.Lerp(firingLine, towerFront, focus);
            toRotation = Quaternion.LookRotation(lookAt - toPosition, Vector3.up);
            view = view != null ? view : GetComponent<Camera>();
            fromFov = view != null ? view.fieldOfView : fieldOfView;
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
            if (view != null)
            {
                view.fieldOfView = Mathf.Lerp(fromFov, fieldOfView, t);
            }
        }
    }
}
