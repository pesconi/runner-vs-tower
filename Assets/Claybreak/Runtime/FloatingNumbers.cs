using TMPro;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The number that floats up when the crowd crosses a run gate (RT-6t.9, after the prototype's <c>floatText</c>):
    /// a pool of preallocated TextMeshPro labels on the HUD, reused in turn. Each one pops in (scale 0.4 to 1.25 in
    /// the first fifth), then rises and fades out in <see cref="Seconds"/>. Green for a good gate, red for a bad one,
    /// white outline from the material. Nothing is created or destroyed in play and the text is written without allocating.
    /// </summary>
    public sealed class FloatingNumbers : MonoBehaviour
    {
        public const float Seconds = 1f;

        // The prototype's keyframes, in its CSS pixels (scaled to the canvas by pixelScale).
        private const float PeakTime = 0.2f;
        private const float PeakRise = 20f;
        private const float FullRise = 110f;
        private const float StartScale = 0.4f;
        private const float PeakScale = 1.25f;

        [SerializeField] private TMP_Text[] slots = new TMP_Text[0];
        [SerializeField] private Color goodColor = new Color(0.184f, 0.749f, 0.525f);
        [SerializeField] private Color badColor = new Color(1f, 0.302f, 0.427f);
        [Tooltip("Canvas units per prototype pixel (the canvas is 1920 tall, a phone page about 800).")]
        [SerializeField, Min(0.1f)] private float pixelScale = 2.4f;

        private readonly char[] buffer = new char[GateSpec.MaximumLabelLength];
        private float[] ages;
        private Vector2[] homes;
        private int next;

        public int Capacity => slots == null ? 0 : slots.Length;

        /// <summary>Labels on screen now (tests).</summary>
        public int Showing
        {
            get
            {
                int showing = 0;
                for (int i = 0; i < Capacity; i++)
                {
                    showing += slots[i].gameObject.activeSelf ? 1 : 0;
                }

                return showing;
            }
        }

        public bool HasParts
        {
            get
            {
                if (Capacity == 0)
                {
                    return false;
                }

                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i] == null)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private void Awake()
        {
            if (!HasParts)
            {
                Debug.LogError("FloatingNumbers needs its preallocated labels (rebuild the scene).", this);
                enabled = false;
                return;
            }

            ages = new float[slots.Length];
            homes = new Vector2[slots.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                homes[i] = slots[i].rectTransform.anchoredPosition;
                slots[i].gameObject.SetActive(false);
            }
        }

        /// <summary>Floats the label of <paramref name="gate"/>; the oldest label is reused when all are showing.</summary>
        public void Show(GateSpec gate)
        {
            if (ages == null)
            {
                return;
            }

            int slot = next;
            next = (next + 1) % slots.Length;
            TMP_Text label = slots[slot];
            label.SetCharArray(buffer, 0, gate.WriteLabel(buffer));
            Color color = gate.IsGood ? goodColor : badColor;
            color.a = 1f;
            label.color = color;
            ages[slot] = 0f;
            label.gameObject.SetActive(true);
            Apply(slot, 0f);
        }

        /// <summary>Hides every label (level start, leaving the level).</summary>
        public void Clear()
        {
            for (int i = 0; i < Capacity; i++)
            {
                slots[i].gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].gameObject.activeSelf)
                {
                    continue;
                }

                ages[i] += delta;
                if (ages[i] >= Seconds)
                {
                    slots[i].gameObject.SetActive(false);
                    continue;
                }

                Apply(i, ages[i] / Seconds);
            }
        }

        private void Apply(int slot, float t)
        {
            Pose(t, out float rise, out float scale, out float alpha);
            TMP_Text label = slots[slot];
            label.rectTransform.anchoredPosition = homes[slot] + new Vector2(0f, rise * pixelScale);
            label.rectTransform.localScale = new Vector3(scale, scale, 1f);
            // Fade through the canvas renderer: setting TMP's alpha would rebuild the text mesh every frame.
            label.canvasRenderer.SetAlpha(alpha);
        }

        /// <summary>
        /// The prototype's animation at normalized time <paramref name="t"/> (0..1): rise in prototype pixels, scale and
        /// opacity. Keyframes 0% (0, 0.4, 0), 20% (20, 1.25, 1), 100% (110, 1, 0), each segment eased out.
        /// </summary>
        public static void Pose(float t, out float rise, out float scale, out float alpha)
        {
            t = Mathf.Clamp01(t);
            if (t < PeakTime)
            {
                float u = EaseOut(t / PeakTime);
                rise = PeakRise * u;
                scale = Mathf.LerpUnclamped(StartScale, PeakScale, u);
                alpha = u;
                return;
            }

            float v = EaseOut((t - PeakTime) / (1f - PeakTime));
            rise = Mathf.LerpUnclamped(PeakRise, FullRise, v);
            scale = Mathf.LerpUnclamped(PeakScale, 1f, v);
            alpha = 1f - v;
        }

        // CSS "ease-out" is a cubic Bezier (0, 0, 0.58, 1); a quadratic ease-out is close enough for a 1 s float.
        private static float EaseOut(float u)
        {
            float inverse = 1f - u;
            return 1f - inverse * inverse;
        }
    }
}
