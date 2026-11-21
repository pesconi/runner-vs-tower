using JungleDegree.StudioKit.Swarm;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Claybreak.Lab
{
    /// <summary>
    /// Test scene of the crowd toon look (RT-6t.1, performance gate): N units of the provisional shape hopping in a fixed
    /// sunflower formation, the Pastel look, a fixed camera that frames the whole crowd in portrait. In measurement
    /// players the launch extras set the count, MSAA, HDR and frame rate (<see cref="ToonLabSettings"/>) and the kit's
    /// frame timing probe logs CPU/GPU per window; in the Editor the serialized values are used.
    /// </summary>
    public sealed class ToonLab : MonoBehaviour
    {
        public const string SceneName = "ToonLab";
        public const string ScenePath = "Assets/Claybreak/Lab/ToonLab.unity";

        [SerializeField] private SwarmRenderer swarm;
        [SerializeField] private Camera viewCamera;
        [Min(0)] [SerializeField] private int editorUnits = 1000;
        [Tooltip("Reference camera of the crowd look: distance grows with √units (prototype formula).")]
        [SerializeField] private float cameraGrowth = 22f;
        [SerializeField] private float minimumVerticalFov = 50f;
        [SerializeField] private float maximumVerticalFov = 100f;

        private void Awake()
        {
            if (swarm == null || viewCamera == null)
            {
                Debug.LogError("[ToonLab] swarm and camera are required", this);
                enabled = false;
                return;
            }

            ToonLabSettings settings = ToonLabSettings.Defaults(editorUnits);
            bool fromLaunch = ToonLabSettings.TryReadLaunchExtras(editorUnits, out ToonLabSettings launch);
            if (fromLaunch)
            {
                settings = launch;
            }

            int units = Mathf.Clamp(settings.Units, 0, swarm.Capacity);
            swarm.SetCount(units);
            FrameCrowd(units);
            ApplyRendering(settings);
            AttachTimingProbe(settings);
            Log(settings, units);
        }

        // Prototype camera (behind the crowd, looking 5 m ahead of it), mirrored to the +Z front of Unity; the vertical
        // field of view opens until the whole formation fits the portrait screen across.
        private void FrameCrowd(int units)
        {
            float distance = 1f + Mathf.Sqrt(units) / cameraGrowth;
            Vector3 position = new Vector3(0f, 3.2f + 4.2f * distance, -(5.5f + 5.5f * distance));
            Vector3 target = new Vector3(0f, 0f, 5f);
            viewCamera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position));
            float halfWidth = SwarmMotion.SunflowerRadius(units, swarm.Spacing) + 0.6f;
            float aspect = Mathf.Max(0.1f, viewCamera.aspect);
            float tanHorizontal = halfWidth / position.magnitude;
            float vertical = 2f * Mathf.Atan(tanHorizontal / aspect) * Mathf.Rad2Deg * 1.1f;
            viewCamera.fieldOfView = Mathf.Clamp(vertical, minimumVerticalFov, maximumVerticalFov);
        }

        // MSAA and HDR live on the pipeline asset; they are changed only in players (in the Editor the change would be
        // written to the asset).
        private static void ApplyRendering(ToonLabSettings settings)
        {
            if (settings.FrameRate > 0)
            {
                Application.targetFrameRate = settings.FrameRate;
            }

            if (Application.isEditor || !(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp))
            {
                return;
            }

            if (settings.Msaa == 1 || settings.Msaa == 2 || settings.Msaa == 4 || settings.Msaa == 8)
            {
                urp.msaaSampleCount = settings.Msaa;
            }

            if (settings.Hdr != ToonLabSettings.KeepHdr)
            {
                urp.supportsHDR = settings.Hdr == 1;
            }
        }

        private void AttachTimingProbe(ToonLabSettings settings)
        {
#if (DEVELOPMENT_BUILD || RELEASE_MEASUREMENT) && UNITY_ANDROID && !UNITY_EDITOR
            if (settings.FrameRate > 0)
            {
                gameObject.AddComponent<JungleDegree.StudioKit.Crowd.DevFrameTimingProbe>(); // load-time: measurement probe, once per scene (rule 2.2.1)
            }
#endif
        }

        private void Log(ToonLabSettings settings, int units)
        {
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            string pipeline = urp == null ? "none" : $"msaa={urp.msaaSampleCount} hdr={(urp.supportsHDR ? "on" : "off")} renderScale={urp.renderScale:0.##}";
            Debug.Log($"[ToonLab] units={units} {pipeline} fps={Application.targetFrameRate} "
                + $"tris/unit={swarm.UnitTriangles} draws=1+{(swarm.DrawsShadows ? 1 : 0)} "
                + $"api={SystemInfo.graphicsDeviceType} gpu={SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceVersion}) "
                + $"cpu={SystemInfo.processorType} x{SystemInfo.processorCount} screen={Screen.width}x{Screen.height} "
                + $"fov={viewCamera.fieldOfView:0.#}", this);
        }
    }
}
