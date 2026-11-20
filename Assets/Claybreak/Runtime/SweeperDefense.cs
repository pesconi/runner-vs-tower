using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The sweeper defense (GDD 4.3, third type): a spiked arm that swings across the track at one
    /// distance, back and forth. A unit that reaches the arm's line while the arm covers it falls; timing
    /// the stream (or switching lanes) avoids it. No collider: the field asks <see cref="Covers"/>.
    /// </summary>
    public sealed class SweeperDefense : MonoBehaviour
    {
        [SerializeField] private Transform arm;
        [Tooltip("Half the width of the arm's deadly band along x, in metres.")]
        [SerializeField, Min(0.1f)] private float halfWidth = 1.1f;

        private float minimumX;
        private float maximumX;
        private float period = 3f;
        private float time;
        private bool present;

        public bool Running { get; set; }
        public bool IsPresent => present;
        public float ArmX { get; private set; }
        public bool HasParts => arm != null;

        public void Configure(bool enabled, float z, float period, float trackMinX, float trackMaxX)
        {
            present = enabled && arm != null;
            gameObject.SetActive(present);
            this.period = Mathf.Max(1f, period);
            minimumX = trackMinX + halfWidth;
            maximumX = trackMaxX - halfWidth;
            Vector3 position = transform.position;
            transform.position = new Vector3(position.x, position.y, z);
            time = 0f;
            Running = false;
            Place();
        }

        /// <summary>True when the arm's band covers <paramref name="x"/> right now.</summary>
        public bool Covers(float x)
        {
            return present && Mathf.Abs(x - ArmX) <= halfWidth;
        }

        private void Update()
        {
            if (!present || !Running)
            {
                return;
            }

            time += Time.deltaTime;
            Place();
        }

        // Smooth back-and-forth (ping-pong on a cosine), so the arm slows at the edges and can be read.
        private void Place()
        {
            float phase = 0.5f - 0.5f * Mathf.Cos(time / period * Mathf.PI * 2f);
            ArmX = Mathf.Lerp(minimumX, maximumX, phase);
            if (arm != null)
            {
                arm.position = new Vector3(ArmX, arm.position.y, arm.position.z);
                arm.localRotation = Quaternion.Euler(0f, time * 360f, 0f);
            }
        }
    }
}
