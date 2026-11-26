using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Claybreak.Tests
{
    /// <summary>
    /// The art series' camera rig (RT-6v3.1, <c>docs/direcao-visual-v3.md</c> section 6): six fixed framings, the same positions,
    /// seeds and instants every time, so a capture "before" and a capture after an art delivery differ only by what the delivery
    /// changed. Each area of the art has its own framing:
    /// <list type="number">
    /// <item><c>corrida-300</c>: the run of world 1 with the crowd at its cap (300), the game camera.</item>
    /// <item><c>assalto-300</c>: the heaviest assault (world 4 device-gate level) with 300 units in flight, the assault camera.</item>
    /// <item><c>lider-perto</c>: the default shape close up, three angles, with the game's own shader and light.</item>
    /// <item><c>torre-perto</c>: the world 4 boss tower from the firing line, up close.</item>
    /// <item><c>telas</c>: the home screen on the four portrait shapes (1920, 2340, 2400 and 2520 tall, 1080 wide, half size).</item>
    /// <item><c>icone</c>: the launcher icon at 512×512 (the icon asset is not drawn by a camera).</item>
    /// </list>
    /// RT-6v3.3: the run and the assault are taken in the four worlds (<c>corrida-300</c> is world 1 and <c>assalto-300</c> world 4,
    /// as before; the others carry <c>-m1</c>..<c>-m4</c>), each measured by a <see cref="ContrastProbe"/> into
    /// <see cref="StyleContrast.MeasuresPath"/> for the style validator; the screens draw the UI outside the post, as the game does.
    /// Determinism: the simulation runs at a fixed 30 frames per second (<see cref="Time.captureFramerate"/>), waits are counted in
    /// frames, and the random generator is seeded at the start of each framing. Runs with a GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.StyleCaptureTests -styleCaptureDir &lt;folder&gt;</c>
    /// writes the six PNG files into the folder.
    /// </summary>
    public static class StyleCaptureRig
    {
        public const string FolderArgument = "-styleCaptureDir";
        /// <summary>The simulation's frame rate while capturing (the wait counts below are in frames of this rate).</summary>
        public const int FrameRate = 30;
        public const int Seed = 20261125;

        /// <summary>The six framings: file name without extension (world 1 run, world 4 assault).</summary>
        public static readonly string[] Framings = { "corrida-300", "assalto-300", "lider-perto", "torre-perto", "telas", "icone" };

        /// <summary>The run and assault file of a world (1-based): the original names for world 1's run and world 4's assault.</summary>
        public static string RunFile(int world) => world == 1 ? "corrida-300" : $"corrida-300-m{world}";
        public static string AssaultFile(int world) => world == 4 ? "assalto-300" : $"assalto-300-m{world}";

        /// <summary>RT-6v3.3: the contrast measured by the run and assault captures of this session (written by <see cref="WriteMeasures"/>).</summary>
        public static readonly List<WorldMeasure> Measured = new List<WorldMeasure>();

        /// <summary>When false the captures skip the contrast probe (the concept screens of the lab).</summary>
        public static bool Measure = true;

        /// <summary>
        /// RT-6v3.2: called with the flow right before every shot of the run, the assault and the screens (null = the game as it
        /// is). The concept screens of the vinyl directions use it to dress the same framings (<c>VinylConceptTests</c>).
        /// </summary>
        public static Action<ClaybreakFlow> Dress;

        /// <summary>
        /// RT-6v3.2: when set, after the two assault shots the tower is swapped by this callback (it returns the file suffix) and
        /// both shots are taken again, e.g. <c>assalto-300-fabrica</c> and <c>torre-perto-fabrica</c>.
        /// </summary>
        public static Func<ClaybreakFlow, AssaultTower, string> AlternateTower;

        public const int RunWidth = 540;
        public const int RunHeight = 1177;
        public const int CloseSize = 600;
        public const int IconSize = 512;
        public static readonly int[] ScreenHeights = { 1920, 2340, 2400, 2520 };
        private const int ScreenWidth = 1080;
        private const string IconPath = "Assets/Claybreak/Content/Icon/AppIcon.png";
        private const string CatalogPath = "Assets/Claybreak/Content/Data/Catalog.asset";

        /// <summary>The folder from <see cref="FolderArgument"/>, or ignores the test when there is none or no GPU.</summary>
        public static string Folder()
        {
            string folder = ShapeSkinCaptureTests.Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
            return folder;
        }

        /// <summary>Fixes the simulation's time step and seeds the generator; undone by <see cref="End"/>.</summary>
        public static void Begin()
        {
            Time.captureFramerate = FrameRate;
            Random.InitState(Seed);
        }

        public static void End()
        {
            Time.captureFramerate = 0;
#if UNITY_EDITOR
            ClaybreakFlow.EditorStartsWithoutTap = false;
            ClaybreakFlow.EditorUsesFreshSave = false;
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
            ClaybreakFlow.EditorUsesDeviceGateRun = false;
            ClaybreakFlow.EditorDeviceGateWorld = 1;
#endif
            Dress = null;
            AlternateTower = null;
            Measure = true;
        }

        /// <summary>
        /// Writes the measures of the four worlds, with the guide's look fingerprint, into the repository
        /// (<see cref="StyleContrast.MeasuresPath"/>, read by the style validator) and into the capture folder.
        /// </summary>
        public static void WriteMeasures(string folder)
        {
            var guide = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleGuide>(StyleGuide.AssetPath);
            Assert.IsNotNull(guide, StyleGuide.AssetPath + " is missing");
            var measures = new ContrastMeasures
            {
                fingerprint = guide.LookFingerprint(),
                taken = "StyleCaptureTests (RT-6v3.3)",
                shots = Measured.ToArray(),
            };
            string json = JsonUtility.ToJson(measures, true).Replace("\r\n", "\n");
            string repository = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? ".", StyleContrast.MeasuresPath);
            File.WriteAllText(repository, json + "\n");
            File.WriteAllText(Path.Combine(folder, "contraste.json"), json + "\n");
            Debug.Log($"[StyleCapture] contrast of {Measured.Count} shots written to {StyleContrast.MeasuresPath}");
        }

        // Puts the roles of the scene on their layers for the probe: the crowd (run and flyers), the floor, the scenery, the enemies
        // (obstacles and defenses), the tower and the two sides of gate numbers.
        private static ContrastProbe Probe(Camera view, ClaybreakFlow flow)
        {
            var probe = new ContrastProbe(view);
            probe.Role(StyleContrast.Crowd, true, flow.Crowd, flow.Field != null ? flow.Field.Flyers : null);
            var scenery = Object.FindFirstObjectByType<WorldScenery>();
            probe.Role(StyleContrast.Track, false, scenery.Track);
            probe.Role(StyleContrast.Ground, false, scenery.Ground);
            probe.Role(StyleContrast.Props, false, scenery.Props);
            probe.Role(StyleContrast.Enemy, true, Object.FindFirstObjectByType<CannonDefense>(), Object.FindFirstObjectByType<AssaultWall>(),
                Object.FindFirstObjectByType<SweeperDefense>(), Object.FindFirstObjectByType<ShieldDefense>());
            probe.Role(StyleContrast.Obstacle, true, Object.FindFirstObjectByType<RunObstacleCourse>());
            probe.Role(StyleContrast.Tower, true, Object.FindFirstObjectByType<AssaultTower>());
            var gates = Object.FindFirstObjectByType<RunGateCourse>();
            var left = new List<Component>();
            var right = new List<Component>();
            if (gates != null)
            {
                foreach (TMP_Text label in gates.GetComponentsInChildren<TMP_Text>())
                {
                    (label.name.StartsWith("Left", StringComparison.Ordinal) ? left : right).Add(label);
                }
            }

            probe.Role("labelLeft", false, left.ToArray());
            probe.Role("labelRight", false, right.ToArray());
            // The lane gates before the tower (x1, x2...) are the numbers the run and the assault show most.
            probe.Role("labelLanes", false, flow.Field != null ? flow.Field.GetComponentsInChildren<TMP_Text>(true) : new Component[0]);
            return probe;
        }

        /// <summary>Waits <paramref name="frames"/> simulation frames (never seconds: the step is fixed).</summary>
        public static IEnumerator Frames(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
        }

        /// <summary>Waits until the condition holds or <paramref name="maximumFrames"/> pass, then one more frame.</summary>
        public static IEnumerator WaitFor(Func<bool> condition, int maximumFrames)
        {
            for (int i = 0; i < maximumFrames && !condition(); i++)
            {
                yield return null;
            }

            yield return null;
        }

        // ---------- 1: the run ----------

        public static IEnumerator CaptureRun(string folder) => CaptureRun(folder, 1);

        public static IEnumerator CaptureRun(string folder, int world)
        {
            Random.InitState(Seed);
            ClaybreakFlow flow = null;
            yield return ShapeSkinCaptureTests.LoadWorldRun(world, loaded => flow = loaded);
            // The default skin in the world's color, the default shape: what a new player sees.
            Assert.IsTrue(flow.Progress.TryWearSkin(0));
            Color worldColor = flow.Progress.CrowdColorIn(world - 1);
            flow.Crowd.Wear(worldColor);
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(CatalogPath);
            flow.Crowd.UseShape(catalog.Shapes[0]);
            Camera view = flow.Crowd.CameraRig.GetComponent<Camera>();
            using var shot = new ShapeSkinCaptureTests.Shot(view, RunWidth, RunHeight);
            yield return Frames(FrameRate * 2);
            Assert.AreEqual(flow.Crowd.MaximumArmy, flow.Crowd.Army, "the run is not at the crowd's cap");
            Dress?.Invoke(flow);
            yield return shot.Take();
            Save(folder, RunFile(world), shot.Panel);
            Debug.Log($"[StyleCapture] {RunFile(world)} army={flow.Crowd.Army} camera={flow.Crowd.CameraRig.transform.position}");
            if (Measure)
            {
                var measure = new WorldMeasure { world = world - 1, shot = "corrida" };
                var skins = new List<(string, Color)>();
                for (int s = 1; s < catalog.Skins.Count; s++)
                {
                    skins.Add((catalog.Skins[s].id, catalog.Skins[s].color));
                }

                yield return Probe(view, flow).Measure(measure, skins, flow.Crowd.Wear, worldColor);
                Measured.Add(measure);
                Debug.Log($"[StyleCapture] {RunFile(world)} contrast {JsonUtility.ToJson(measure)}");
            }
        }

        // ---------- 2 and 4: the assault, far and near the tower ----------

        public static IEnumerator CaptureAssault(string folder) => CaptureAssault(folder, 4);

        /// <summary>The assault of a world's device-gate level (the heaviest of it); the tower close-up only in world 4.</summary>
        public static IEnumerator CaptureAssault(string folder, int world)
        {
            Random.InitState(Seed);
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateRun = false;
#endif
            yield return ClaybreakSceneTests.LoadScene(true, world);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            LooseSwarmRenderer flyers = flow.Field.Flyers;
            Assert.IsFalse(flyers.Headless, "no GPU: nothing to capture");
            if (Measure)
            {
                // RT-6v3.3: the D36 contrast is about the world's own crowd color: the device-gate level wears the dearest skin
                // (the load is the same), so the measured assault puts the default skin back before the units take off.
                Assert.IsTrue(flow.Progress.TryWearSkin(0));
                flow.Crowd.Wear(flow.Progress.CrowdColorIn(world - 1));
                flow.Field.Wear(flow.Progress.CrowdColorIn(world - 1));
            }

            Camera view = flow.Crowd.CameraRig.GetComponent<Camera>();
            using (var shot = new ShapeSkinCaptureTests.Shot(view, RunWidth, RunHeight))
            {
                yield return WaitFor(() => flow.State == FlowState.Assault, FrameRate * 15);
                Assert.AreEqual(FlowState.Assault, flow.State);
                yield return WaitFor(() => flow.Field.InFlight >= 300, FrameRate * 30);
                Assert.AreEqual(300, flyers.Count, "the field never filled");
                Dress?.Invoke(flow);
                yield return shot.Take();
                Save(folder, AssaultFile(world), shot.Panel);
                Debug.Log($"[StyleCapture] {AssaultFile(world)} level={flow.ActiveLevel.name} inFlight={flow.Field.InFlight} dying={flyers.DyingCount}");
                if (Measure)
                {
                    var measure = new WorldMeasure { world = world - 1, shot = "assalto" };
                    yield return Probe(view, flow).Measure(measure, null, null, Color.white);
                    Measured.Add(measure);
                    Debug.Log($"[StyleCapture] {AssaultFile(world)} contrast {JsonUtility.ToJson(measure)}");
                }

                if (world != 4)
                {
                    yield break;
                }

                // The tower up close: a second camera at the firing line side, looking at the middle of the tower body.
                var tower = Object.FindFirstObjectByType<AssaultTower>();
                var closeObject = new GameObject("StyleCaptureTowerCamera");
                Camera close = closeObject.AddComponent<Camera>();
                close.CopyFrom(view);
                close.targetTexture = null;
                float towerZ = flow.ActiveLevel.TowerZ;
                closeObject.transform.position = new Vector3(0f, 9f, towerZ - 32f);
                closeObject.transform.LookAt(new Vector3(0f, 9f, towerZ));
                close.fieldOfView = 50f;
                using (var closeShot = new ShapeSkinCaptureTests.Shot(close, RunWidth, RunHeight))
                {
                    Dress?.Invoke(flow);
                    yield return closeShot.Take();
                    Save(folder, "torre-perto", closeShot.Panel);
                    Debug.Log($"[StyleCapture] torre-perto tower={flow.ActiveLevel.Tower} z={tower.transform.position.z:0.0}");
                }

                if (AlternateTower != null)
                {
                    string suffix = AlternateTower(flow, tower);
                    Dress?.Invoke(flow);
                    yield return shot.Take();
                    Save(folder, "assalto-300" + suffix, shot.Panel);
                    using var closeShot = new ShapeSkinCaptureTests.Shot(close, RunWidth, RunHeight);
                    yield return closeShot.Take();
                    Save(folder, "torre-perto" + suffix, closeShot.Panel);
                }

                Object.Destroy(closeObject);
            }
        }

        // ---------- 3: the leader up close ----------

        public static IEnumerator CaptureLeader(string folder)
        {
            Random.InitState(Seed);
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(CatalogPath);
            CrowdShapeOption shape = catalog.Shapes[0];
            ClaybreakFlow flow = null;
            yield return ShapeSkinCaptureTests.LoadWorldRun(1, loaded => flow = loaded);
            Assert.IsTrue(flow.Progress.TryWearSkin(0));
            Color worldColor = flow.Progress.CrowdColorIn(0);

            // The mesh alone high above the track (out of the props), with the crowd's own material (RT-6v3.3: the vinyl
            // profile), light and look; a copy without the instancing keyword, so a plain renderer draws it.
            var model = new GameObject("StyleCaptureLeader");
            Material crowdMaterial = null;
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("CrowdToon t:Material"))
            {
                crowdMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
            }

            Assert.IsNotNull(crowdMaterial, "CrowdToon material is missing");
            var material = new Material(crowdMaterial);
            material.DisableKeyword(ToonShaderIds.InstancesKeyword);
            material.SetColor("_TintColor", worldColor);
            model.transform.position = new Vector3(0f, 60f, 0f);
            model.AddComponent<MeshFilter>().sharedMesh = shape.mesh;
            model.AddComponent<MeshRenderer>().sharedMaterial = material;
            var cameraObject = new GameObject("StyleCaptureLeaderCamera");
            Camera close = cameraObject.AddComponent<Camera>();
            close.fieldOfView = 28f;
            close.nearClipPlane = 0.05f;
            close.farClipPlane = 50f;
            Vector3 center = model.transform.position + new Vector3(0f, 0.38f, 0f);
            float[] yaws = { 0f, 35f, 150f };
            using (var shot = new ShapeSkinCaptureTests.Shot(close, CloseSize, CloseSize))
            using (var sheet = new ShapeSkinCaptureTests.Sheet(yaws.Length, 1, CloseSize, CloseSize))
            {
                for (int i = 0; i < yaws.Length; i++)
                {
                    cameraObject.transform.position = center + Quaternion.Euler(8f, yaws[i], 0f) * new Vector3(0f, 0f, 2.2f);
                    cameraObject.transform.LookAt(center);
                    yield return shot.Take();
                    sheet.Put(i, 0, shot.Panel);
                }

                sheet.Save(Path.Combine(folder, "lider-perto.png"));
            }

            Debug.Log($"[StyleCapture] lider-perto shape={shape.id} tris={shape.mesh.triangles.Length / 3}");
            Object.Destroy(cameraObject);
            Object.Destroy(model);
            Object.Destroy(material);
        }

        // ---------- 5: the screens ----------

        public static IEnumerator CaptureScreens(string folder)
        {
            Random.InitState(Seed);
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateRun = false;
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
#endif
            yield return ClaybreakSceneTests.LoadScene(false);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            flow.Progress.Save.coins = 5000;
            // RT-6v3.3: as in the game, the UI is outside the post. The 3D picture is taken with the post and without the UI; the UI
            // is drawn by a camera of its own, without post, over black and over white (its coverage is where the two agree), and
            // laid over the picture: the buttons keep their own colors, like the overlay canvas on the phone.
            var canvas = GameObject.Find("/Ui").GetComponent<Canvas>();
            Camera view = flow.Crowd.CameraRig.GetComponent<Camera>();
            const int uiLayer = 5;
            SetLayer(canvas.transform, uiLayer);
            view.cullingMask &= ~(1 << uiLayer);
            var uiObject = new GameObject("StyleCaptureUiCamera");
            Camera ui = uiObject.AddComponent<Camera>();
            ui.cullingMask = 1 << uiLayer;
            ui.clearFlags = CameraClearFlags.SolidColor;
            ui.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            uiObject.transform.position = new Vector3(0f, -500f, 0f);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = ui;
            canvas.planeDistance = 1f;
            const int half = 2;
            using (var sheet = new ShapeSkinCaptureTests.Sheet(ScreenHeights.Length, 1, ScreenWidth / half, ScreenHeights[ScreenHeights.Length - 1] / half))
            {
                for (int i = 0; i < ScreenHeights.Length; i++)
                {
                    using var shot = new ShapeSkinCaptureTests.Shot(view, ScreenWidth / half, ScreenHeights[i] / half);
                    using var uiShot = new ShapeSkinCaptureTests.Shot(ui, ScreenWidth / half, ScreenHeights[i] / half);
                    yield return null;
                    Canvas.ForceUpdateCanvases();
                    ScreenFitTests.Show(flow, "home");
                    yield return null;
                    Canvas.ForceUpdateCanvases();
                    Dress?.Invoke(flow);
                    ui.backgroundColor = Color.black;
                    yield return shot.Take();
                    Color[] scene = shot.Panel.GetPixels();
                    yield return uiShot.Take();
                    Color[] overBlack = uiShot.Panel.GetPixels();
                    ui.backgroundColor = Color.white;
                    yield return uiShot.Take();
                    Color[] overWhite = uiShot.Panel.GetPixels();
                    for (int p = 0; p < scene.Length; p++)
                    {
                        // Over black the UI gives alpha x color; over white alpha x color + (1 - alpha).
                        float alpha = Mathf.Clamp01(1f - ((overWhite[p].r - overBlack[p].r) + (overWhite[p].g - overBlack[p].g) + (overWhite[p].b - overBlack[p].b)) / 3f);
                        scene[p] = new Color(overBlack[p].r + scene[p].r * (1f - alpha), overBlack[p].g + scene[p].g * (1f - alpha),
                            overBlack[p].b + scene[p].b * (1f - alpha), 1f);
                    }

                    shot.Panel.SetPixels(scene);
                    // Panels of different heights share the sheet's top edge.
                    sheet.Put(i, 0, shot.Panel, ScreenHeights[ScreenHeights.Length - 1] / half - ScreenHeights[i] / half);
                }

                sheet.Save(Path.Combine(folder, "telas.png"));
            }

            view.targetTexture = null;
            Object.Destroy(uiObject);
            Debug.Log("[StyleCapture] telas home in four shapes, UI outside the post");
        }

        private static void SetLayer(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            foreach (Transform child in root)
            {
                SetLayer(child, layer);
            }
        }

        // ---------- 6: the icon ----------

        public static void CaptureIcon(string folder)
        {
            var icon = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            Assert.IsNotNull(icon, IconPath + " is missing");
            var target = new RenderTexture(IconSize, IconSize, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(icon, target);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(IconSize, IconSize, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, IconSize, IconSize), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;
            Save(folder, "icone", texture);
            Object.Destroy(texture);
            Object.Destroy(target);
            Debug.Log("[StyleCapture] icone");
        }

        public static void Save(string folder, string name, Texture2D texture)
        {
            File.WriteAllBytes(Path.Combine(folder, name + ".png"), texture.EncodeToPNG());
        }
    }
}
