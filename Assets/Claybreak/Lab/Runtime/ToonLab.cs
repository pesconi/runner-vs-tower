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
    /// default). RT-6t.5: lowering the count throws the outer ring out in the ballistic death (as the prototype does), a
    /// "÷2" button halves the crowd like a bad gate, and a churn mode keeps N units live while D are always dying (the
    /// death gate: the outer ring is thrown out and pops back in, frame after frame). In measurement players the launch extras set the count, shape, eyes, MSAA, HDR and frame rate
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
        [Tooltip("Units kept dying at all times in the Editor (churn of the outer ring; 0 = off). Players read --ei dying.")]
        [Min(0)] [SerializeField] private int editorDying;
        [SerializeField] private SwarmDragSettings drag = SwarmDragSettings.Default;
        [Tooltip("Push the fog away as far as the portrait fit pushes the camera (the reference fog is tuned for the "
            + "reference framing; without this the crowd sits inside the fog on portrait screens).")]
        [SerializeField] private bool scaleFogWithFraming = true;
        [Tooltip("Crowd shapes the panel picks from, each with and without eyes (RT-6t.2).")]
        [SerializeField] private ShapeChoice[] shapes = Array.Empty<ShapeChoice>();
        [Min(0)] [SerializeField] private int shapeIndex;
        [SerializeField] private bool eyes = true;
        [Header("RT-6v3.2 vinyl rehearsal (--ei vinyl N, --ez post)")]
        [Tooltip("An instanced material on Claybreak/VinylLab: carries the shader and its instancing variants into the "
            + "measurement player (variants of a shader no instanced material uses are stripped); its shader replaces ToonLit.")]
        [SerializeField] private Material vinylMaterial;
        [Tooltip("The smoothed Bibopzi of the concept screens (PN subdivided).")]
        [SerializeField] private Mesh vinylUnit;
        [Tooltip("Post profiles of directions A..C (VinylDirection.All order).")]
        [SerializeField] private VolumeProfile[] vinylProfiles = Array.Empty<VolumeProfile>();
        [SerializeField] private Volume vinylVolume;
        [SerializeField] private Light sun;
        [Tooltip("Editor only: the direction to show (-1 = the plain lab, 0 = ToonLit with the smoothed Bibopzi, 1..3 = A..C).")]
        [SerializeField] private int editorVinyl = -1;
        [SerializeField] private bool editorPost;
        private int vinyl = -1;

        private bool dragging;
        private float pressX;
        private float targetAtPress;
        private int logFrame;
        private float fogStart;
        private float fogEnd;
        private float fogScale;
        private ToonLabSettings settings;
        private int churnUnits;
        private float churnRate;
        private float churnDebt;

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

        /// <summary>
        /// Panel: sets the crowd to <paramref name="units"/> (1000!, restart at 10). As in the prototype, going down clears
        /// the units still flying and throws the removed outer ring out in the ballistic death.
        /// </summary>
        public void SetUnits(int units)
        {
            if (units < swarm.Count)
            {
                swarm.ClearDying();
            }

            swarm.SetCount(units, SwarmRemoval.Die);
        }

        /// <summary>Panel: a bad gate (÷2, rounded up as the prototype does): the outer half dies and the camera shakes.</summary>
        public void HalveCrowd()
        {
            swarm.SetCount((swarm.Count + 1) / 2, SwarmRemoval.Die);
            cameraRig.Shake();
        }

        /// <summary>
        /// Keeps <paramref name="dying"/> units dying at all times around the current live count (0 = off): the outer ring
        /// is thrown out and popped back in at the rate that fills the ring of dying units within their lifetime.
        /// </summary>
        public void Churn(int dying)
        {
            churnUnits = Mathf.Clamp(dying, 0, swarm.DyingCapacity);
            churnRate = churnUnits == 0 ? 0f : ChurnOverfill * churnUnits / swarm.Death.Lifetime;
            churnDebt = 0f;
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
            vinyl = fromLaunch ? settings.Vinyl : editorVinyl;
            ApplyVinyl(fromLaunch ? settings.Post : editorPost);
            ApplyShape();

            swarm.SetCount(Mathf.Clamp(settings.Units, 0, swarm.Capacity));
            Churn(fromLaunch ? settings.Dying : editorDying);
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
            UpdateChurn();
        }

        // A little more than the ring holds per lifetime, so it stays full (the oldest makes room) instead of thinning out
        // between frames. Units die from the outer ring and pop back in at once: the live count never changes.
        private const float ChurnOverfill = 1.15f;

        private void UpdateChurn()
        {
            if (churnUnits == 0 || swarm.Count == 0)
            {
                return;
            }

            churnDebt += churnRate * Time.deltaTime;
            int thrown = Mathf.Min((int)churnDebt, swarm.Count);
            if (thrown == 0)
            {
                return;
            }

            churnDebt -= thrown;
            int live = swarm.Count;
            swarm.SetCount(live - thrown, SwarmRemoval.Die);
            swarm.SetCount(live);
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

        // RT-6v3.2: the direction's light, sky and post, the swarm on the lab shader and the smoothed Bibopzi. In players
        // only the loaded copies of the materials change; in the Editor the swarm material asset would be switched, so the
        // Editor shows the direction without switching shaders (captures of the game use VinylConceptTests instead).
        private void ApplyVinyl(bool post)
        {
            Camera view = cameraRig.GetComponent<Camera>();
            VinylDirection.SetPost(view, false);
            if (vinyl < 0)
            {
                return;
            }

            if (vinyl > 0 && vinyl <= VinylDirection.All.Length && vinylMaterial != null)
            {
                VinylDirection direction = VinylDirection.All[vinyl - 1];
                direction.ApplyLight(sun);
                if (!Application.isEditor)
                {
                    VinylDirection.SwitchMaterials(vinylMaterial.shader, "CrowdToon");
                }

                if (vinylVolume != null && vinyl - 1 < vinylProfiles.Length)
                {
                    vinylVolume.sharedProfile = vinylProfiles[vinyl - 1];
                    vinylVolume.enabled = post;
                }

                VinylDirection.SetPost(view, post);
            }
        }

        private void ApplyShape()
        {
            if (vinyl >= 0 && vinylUnit != null)
            {
                swarm.SetUnitMesh(vinylUnit);
                return;
            }

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
            string look = vinyl < 0 ? "lab" : vinyl == 0 ? "toonlit-smooth" : "vinyl-" + VinylDirection.All[Mathf.Min(vinyl, VinylDirection.All.Length) - 1].id;
            bool postOn = view.GetUniversalAdditionalCameraData().renderPostProcessing;
            Debug.Log($"[ToonLab] look={look} post={(postOn ? "on" : "off")} units={units} dying={churnUnits} shape=\"{ShapeLabel}\" {pipeline} fps={Application.targetFrameRate} "
                + $"tris/unit={swarm.UnitTriangles} draws=1+{(swarm.DrawsShadows ? 1 : 0)} "
                + $"api={SystemInfo.graphicsDeviceType} gpu={SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceVersion}) "
                + $"cpu={SystemInfo.processorType} x{SystemInfo.processorCount} screen={Screen.width}x{Screen.height} "
                + $"fov={view.fieldOfView:0.#} camera={view.transform.position} framing={cameraRig.FramingScale:0.##} "
                + $"fog={RenderSettings.fogStartDistance:0.#}-{RenderSettings.fogEndDistance:0.#}", this);
        }
    }
}
