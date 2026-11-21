using System;
using JungleDegree.StudioKit.Swarm;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Claybreak.Lab
{
    /// <summary>
    /// Test scene of the crowd toon look. RT-6t.1: N units of the provisional shape hopping in a sunflower formation,
    /// the Pastel look, the performance gate. RT-6t.3: a finger drag (arrow keys in the Editor) steers the crowd across
    /// the track, the camera rig backs away as it grows and keeps the track on the portrait screen, and a test panel
    /// adds units (+100), jumps to 1000, restarts at 10 and shakes the camera. RT-6t.2: the panel also picks the crowd
    /// shape (Bolinha, Pintinho) and the eyes (with, without: decision D25 is open, the eyed shapes are the provisional
    /// default). In measurement players the launch extras set the count, shape, eyes, MSAA, HDR and frame rate
    /// (<see cref="ToonLabSettings"/>), the panel is hidden and the kit's frame timing probe logs CPU/GPU per window; in
    /// the Editor the serialized values are used.
    /// </summary>
    public sealed class ToonLab : MonoBehaviour
    {
        public const string SceneName = "ToonLab";
        public const string ScenePath = "Assets/Claybreak/Lab/ToonLab.unity";

        [SerializeField] private SwarmRenderer swarm;
        [SerializeField] private SwarmCameraRig cameraRig;
        [Tooltip("Test panel (its screen rectangle does not start drags); hidden in measurement launches.")]
        [SerializeField] private RectTransform panel;
        [Min(0)] [SerializeField] private int editorUnits = 1000;
        [SerializeField] private SwarmDragSettings drag = SwarmDragSettings.Default;
        [Tooltip("Push the fog away as far as the portrait fit pushes the camera (the reference fog is tuned for the "
            + "reference framing; without this the crowd sits inside the fog on portrait screens).")]
        [SerializeField] private bool scaleFogWithFraming = true;
        [Tooltip("Crowd shapes the panel picks from, each with and without eyes (RT-6t.2).")]
        [SerializeField] private ShapeChoice[] shapes = Array.Empty<ShapeChoice>();
        [Min(0)] [SerializeField] private int shapeIndex;
        [SerializeField] private bool eyes = true;

        private bool dragging;
        private float pressX;
        private float targetAtPress;
        private int logFrame;
        private float fogStart;
        private float fogEnd;
        private float fogScale;
        private ToonLabSettings settings;

        public SwarmRenderer Swarm => swarm;
        public SwarmCameraRig CameraRig => cameraRig;
        public int ShapeCount => shapes.Length;
        public int ShapeIndex => shapeIndex;
        public bool Eyes => eyes;

        /// <summary>Label of the shape on screen now ("Bolinha", "Pintinho sem olhos").</summary>
        public string ShapeLabel => shapes.Length == 0 ? "none" : shapes[shapeIndex].label + (eyes ? "" : " sem olhos");

        /// <summary>One crowd shape of the lab, with and without eyes.</summary>
        [Serializable]
        public struct ShapeChoice
        {
            public string label;
            public Mesh withEyes;
            public Mesh withoutEyes;
        }

        /// <summary>Mesh of shape <paramref name="index"/> with or without eyes (captures and tests).</summary>
        public Mesh ShapeMesh(int index, bool withEyes)
        {
            return withEyes ? shapes[index].withEyes : shapes[index].withoutEyes;
        }

        /// <summary>Panel: shows shape <paramref name="index"/> (Bolinha 0, Pintinho 1), keeping the eyes choice.</summary>
        public void SelectShape(int index)
        {
            if (index < 0 || index >= shapes.Length)
            {
                Debug.LogWarning($"[ToonLab] no shape {index} (there are {shapes.Length})", this);
                return;
            }

            shapeIndex = index;
            ApplyShape();
        }

        /// <summary>Panel: shows the current shape with or without eyes.</summary>
        public void SetEyes(bool withEyes)
        {
            eyes = withEyes;
            ApplyShape();
        }

        /// <summary>Panel: adds <paramref name="units"/> to the crowd (clamped to the capacity).</summary>
        public void AddUnits(int units)
        {
            swarm.SetCount(swarm.Count + units);
        }

        /// <summary>Panel: sets the crowd to <paramref name="units"/> (1000!, restart at 10).</summary>
        public void SetUnits(int units)
        {
            swarm.SetCount(units);
        }

        /// <summary>Panel: shakes the camera as a bad gate will.</summary>
        public void ShakeCamera()
        {
            cameraRig.Shake();
        }

        /// <summary>Steers the crowd toward <paramref name="x"/> (kept inside the track), as a drag would.</summary>
        public void SteerTo(float x)
        {
            swarm.SteerTo(SwarmSteering.Clamp(x, drag, swarm.transform.position.x, swarm.LaneHalfWidth));
        }

        private void Awake()
        {
            if (swarm == null || cameraRig == null)
            {
                Debug.LogError("[ToonLab] swarm and camera rig are required", this);
                enabled = false;
                return;
            }

            settings = ToonLabSettings.Defaults(editorUnits);
            bool fromLaunch = ToonLabSettings.TryReadLaunchExtras(editorUnits, out ToonLabSettings launch);
            if (fromLaunch)
            {
                settings = launch;
            }

            if (panel != null && settings.Requested)
            {
                panel.gameObject.SetActive(false);
            }

            if (settings.Shape >= 0 && settings.Shape < shapes.Length)
            {
                shapeIndex = settings.Shape;
            }

            if (settings.Eyes != ToonLabSettings.KeepEyes)
            {
                eyes = settings.Eyes == 1;
            }

            shapeIndex = Mathf.Clamp(shapeIndex, 0, Mathf.Max(0, shapes.Length - 1));
            ApplyShape();

            swarm.SetCount(Mathf.Clamp(settings.Units, 0, swarm.Capacity));
            ApplyRendering(settings);
            AttachTimingProbe(settings);
            fogStart = RenderSettings.fogStartDistance;
            fogEnd = RenderSettings.fogEndDistance;
            logFrame = Time.frameCount + 2;
        }

        private void OnDestroy()
        {
            // RenderSettings belong to the scene; restored so an Editor session leaves the scene file as built.
            if (fogScale > 0f)
            {
                RenderSettings.fogStartDistance = fogStart;
                RenderSettings.fogEndDistance = fogEnd;
            }
        }

        private void Update()
        {
            ReadDrag();
            ReadKeys();
        }

        // Logged once the rig has framed the crowd (its field of view is set in LateUpdate).
        private void LateUpdate()
        {
            ScaleFog();
            if (logFrame > 0 && Time.frameCount >= logFrame)
            {
                logFrame = 0;
                Log(settings, swarm.Count);
            }
        }

        private void ScaleFog()
        {
            float scale = scaleFogWithFraming ? cameraRig.FramingScale : 1f;
            if (Mathf.Abs(scale - fogScale) < 0.005f)
            {
                return;
            }

            fogScale = scale;
            RenderSettings.fogStartDistance = fogStart * scale;
            RenderSettings.fogEndDistance = fogEnd * scale;
        }

        private void ApplyShape()
        {
            if (shapes.Length == 0)
            {
                return; // the swarm keeps the mesh set on it
            }

            ShapeChoice choice = shapes[shapeIndex];
            swarm.SetUnitMesh(eyes ? choice.withEyes : choice.withoutEyes);
        }

        private void ReadDrag()
        {
            bool pressed;
            Vector2 position;
            Touchscreen touch = Touchscreen.current;
            Mouse mouse = Mouse.current;
            if (touch != null && touch.primaryTouch.press.isPressed)
            {
                pressed = touch.primaryTouch.press.wasPressedThisFrame;
                position = touch.primaryTouch.position.ReadValue();
            }
            else if (mouse != null && mouse.leftButton.isPressed)
            {
                pressed = mouse.leftButton.wasPressedThisFrame;
                position = mouse.position.ReadValue();
            }
            else
            {
                dragging = false;
                return;
            }

            float x = position.x;
            if (pressed)
            {
                dragging = panel == null || !panel.gameObject.activeInHierarchy
                    || !RectTransformUtility.RectangleContainsScreenPoint(panel, position, null);
                pressX = x;
                targetAtPress = swarm.TargetX;
            }

            if (dragging)
            {
                swarm.SteerTo(SwarmSteering.DragTarget(targetAtPress, pressX, x, Screen.width, drag,
                    swarm.transform.position.x, swarm.LaneHalfWidth));
            }
        }

        private void ReadKeys()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            int direction = (keyboard.rightArrowKey.wasPressedThisFrame ? 1 : 0) - (keyboard.leftArrowKey.wasPressedThisFrame ? 1 : 0);
            if (direction != 0)
            {
                swarm.SteerTo(SwarmSteering.KeyTarget(swarm.TargetX, direction, drag, swarm.transform.position.x, swarm.LaneHalfWidth));
            }
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
            Camera view = cameraRig.GetComponent<Camera>();
            Debug.Log($"[ToonLab] units={units} shape=\"{ShapeLabel}\" {pipeline} fps={Application.targetFrameRate} "
                + $"tris/unit={swarm.UnitTriangles} draws=1+{(swarm.DrawsShadows ? 1 : 0)} "
                + $"api={SystemInfo.graphicsDeviceType} gpu={SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceVersion}) "
                + $"cpu={SystemInfo.processorType} x{SystemInfo.processorCount} screen={Screen.width}x{Screen.height} "
                + $"fov={view.fieldOfView:0.#} camera={view.transform.position} framing={cameraRig.FramingScale:0.##} "
                + $"fog={RenderSettings.fogStartDistance:0.#}-{RenderSettings.fogEndDistance:0.#}", this);
        }
    }
}
