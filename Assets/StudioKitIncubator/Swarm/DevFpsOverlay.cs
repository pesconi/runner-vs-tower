using UnityEngine;
using UnityEngine.UI;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// Debug readout of the presented frame rate and the swarm size: a uGUI label refreshed every half second and a
    /// log line per window that measurement scripts read (<c>[DevFps] window=N fps=F units=U</c>). Active only in the
    /// Editor, development players and measurement players (RELEASE_MEASUREMENT); release players hide it.
    /// The label text is picked from strings built once, so refreshing it does not allocate.
    /// </summary>
    public sealed class DevFpsOverlay : MonoBehaviour
    {
        private const float LabelSeconds = 0.5f;
        private const float LogSeconds = 5f;
        private const int MaximumShownFps = 240;

        [SerializeField] private Text fpsLabel;
        [SerializeField] private Text unitsLabel;
        [SerializeField] private SwarmRenderer swarm;

        private string[] fpsTexts;
        private int shownUnits = -1;
        private float labelTime;
        private int labelFrames;
        private float logTime;
        private int logFrames;
        private int window;

        public static bool Allowed
        {
            get
            {
#if RELEASE_MEASUREMENT
                return true;
#else
                return Application.isEditor || Debug.isDebugBuild;
#endif
            }
        }

        private void Awake()
        {
            if (!Allowed)
            {
                SetLabelsActive(false);
                enabled = false;
                return;
            }

            fpsTexts = new string[MaximumShownFps + 1];
            for (int i = 0; i <= MaximumShownFps; i++)
            {
                fpsTexts[i] = i + " fps";
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            labelTime += dt;
            logTime += dt;
            labelFrames++;
            logFrames++;
            if (labelTime >= LabelSeconds)
            {
                int fps = Mathf.Clamp(Mathf.RoundToInt(labelFrames / labelTime), 0, MaximumShownFps);
                if (fpsLabel != null)
                {
                    fpsLabel.text = fpsTexts[fps];
                }

                labelTime = 0f;
                labelFrames = 0;
            }

            int units = swarm == null ? 0 : swarm.Count;
            if (units != shownUnits && unitsLabel != null)
            {
                unitsLabel.text = units + " units";
            }

            shownUnits = units;
            if (logTime >= LogSeconds)
            {
                window++;
                Debug.Log($"[DevFps] window={window} fps={logFrames / logTime:F1} units={units}");
                logTime = 0f;
                logFrames = 0;
            }
        }

        private void SetLabelsActive(bool active)
        {
            if (fpsLabel != null)
            {
                fpsLabel.gameObject.SetActive(active);
            }

            if (unitsLabel != null)
            {
                unitsLabel.gameObject.SetActive(active);
            }
        }
    }
}
