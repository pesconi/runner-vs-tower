using System.IO;
using Claybreak.Lab;
using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Claybreak.Editor
{
    /// <summary>
    /// Builds the toon lab of the crowd toon direction (RT-6t.1) from code: the crowd shapes (<see cref="UnitShapeAssets"/>,
    /// RT-6t.2) and the shadow disc (<see cref="UnitShapeRecipes"/>), the ground and track meshes, the toon materials, the Pastel look, the lab camera
    /// profile and the <c>ToonLab</c> scene (camera rig, light without shadows, linear fog, the swarm kept on the track,
    /// the test panel of RT-6t.3 with the shape picker of RT-6t.2, the debug FPS overlay).
    /// The scene goes only into measurement players (<see cref="ClaybreakBuild"/>), never into the game build list.
    /// Idempotent: assets keep their GUIDs. Runs inside <see cref="ClaybreakSceneBuilder.Build"/>.
    /// </summary>
    public static class ToonLabBuilder
    {
        public const string Root = "Assets/Claybreak/Lab";
        public const string GeneratedFolder = Root + "/Generated";
        // The crowd's own assets (shadow disc, swarm and shadow materials) are shared with the game run (RT-6t.4) and
        // live next to the crowd shapes; the lab keeps only its ground and static material.
        public const string ShadowDiscPath = UnitShapeAssets.Folder + "/BlobShadowDisc.asset";
        public const string GroundPath = GeneratedFolder + "/LabGround.asset";
        public const string SwarmMaterialPath = UnitShapeAssets.Folder + "/CrowdToon.mat";
        public const string StaticMaterialPath = GeneratedFolder + "/ToonLitStatic.mat";
        public const string ShadowMaterialPath = UnitShapeAssets.Folder + "/CrowdShadow.mat";
        public const string LookPath = Root + "/ToonLookPastel.asset";
        public const string CameraProfilePath = Root + "/SwarmCameraLab.asset";
        public const int SwarmCapacity = 2048;
        public const int DefaultUnits = 1000;

        // Pastel palette of the reference prototype (docs/direcao-visual-crowd.md, 1.1).
        private static readonly Color32 Fog = Hex(0xf3e3ea);
        private static readonly Color32 Ground = Hex(0xbfe6cf);
        private static readonly Color32 Track = Hex(0xfff4ea);
        private static readonly Color32 Stripe = Hex(0xffe3d1);
        private static readonly Color32 Crowd = Hex(0xff8fab);
        private static readonly Color32 Ink = Hex(0x3b2f4a);
        private static readonly Color32 Accent = Hex(0xff6f91);
        private static readonly string[] ShapeLabels = { "Bolinha", "Pintinho" };
        /// <summary>Half width of the lab track (the reference prototype track); the swarm is kept inside it.</summary>
        public const float TrackHalfWidth = 6f;

        [MenuItem("Claybreak/Build Toon Lab")]
        public static void Build()
        {
            EnsureFolder(Root);
            EnsureFolder(GeneratedFolder);
            UnitShapeAssets.Build();
            SaveMesh(UnitShapeRecipes.ShadowDisc(), ShadowDiscPath);
            SaveMesh(GroundMesh(), GroundPath);
            Shader toon = Find(ToonShaderIds.ToonLitShader);
            SaveMaterial(SwarmMaterialPath, toon, material =>
            {
                material.EnableKeyword(ToonShaderIds.InstancesKeyword);
                material.SetColor(ToonShaderIds.TintColor, Color.white);
                material.enableInstancing = true; // the fallback route draws it with RenderMeshInstanced (RT-6t.11)
            });
            SaveMaterial(StaticMaterialPath, toon, material =>
            {
                material.DisableKeyword(ToonShaderIds.InstancesKeyword);
                material.SetColor(ToonShaderIds.TintColor, Color.white);
            });
            SaveMaterial(ShadowMaterialPath, Find("StudioKit/BlobShadowInstanced"), material =>
            {
                material.SetColor("_ShadowColor", new Color(0x2b / 255f, 0x22 / 255f, 0x33 / 255f, 0.18f));
                material.enableInstancing = true;
            });
            if (AssetDatabase.LoadAssetAtPath<ToonLook>(LookPath) == null)
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ToonLook>(), LookPath);
            }

            // Data assets are created once and then left alone (they are tuned by hand).
            if (AssetDatabase.LoadAssetAtPath<SwarmCameraProfile>(CameraProfilePath) == null)
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SwarmCameraProfile>(), CameraProfilePath);
            }

            AssetDatabase.SaveAssets();
            CreateScene();
        }

        public static void BuildAndQuit()
        {
            BatchRunner.Run("ToonLabBuilder", Build);
        }

        private static void CreateScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Loaded by path after the new scene: references taken before it can go stale (assets unloaded with the
            // previous scene).
            var shapes = new Mesh[UnitShapeAssets.All.Length];
            for (int i = 0; i < shapes.Length; i++)
            {
                shapes[i] = Load<Mesh>(UnitShapeAssets.All[i].Path);
            }

            Mesh disc = Load<Mesh>(ShadowDiscPath);
            Mesh ground = Load<Mesh>(GroundPath);
            Material swarmMaterial = Load<Material>(SwarmMaterialPath);
            Material staticMaterial = Load<Material>(StaticMaterialPath);
            Material shadowMaterial = Load<Material>(ShadowMaterialPath);
            ToonLook look = Load<ToonLook>(LookPath);
            SwarmCameraProfile cameraProfile = Load<SwarmCameraProfile>(CameraProfilePath);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Fog;
            RenderSettings.fogStartDistance = 22f;
            RenderSettings.fogEndDistance = 75f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;
            RenderSettings.skybox = null;

            var lightObject = new GameObject("Sun");
            var sun = lightObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = Color.white;
            sun.intensity = 1f;
            sun.shadows = LightShadows.None;
            // Reference sun at (6, 12, 4) with the camera on +Z; mirrored to the +Z front of Unity.
            lightObject.transform.rotation = Quaternion.LookRotation(-new Vector3(6f, 12f, -4f));
            lightObject.AddComponent<ToonLookApplier>();
            SetObject(lightObject.GetComponent<ToonLookApplier>(), "look", look);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Fog;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 250f; // the portrait fit pushes the camera and the fog back (RT-6t.3)
            camera.fieldOfView = 50f;

            var groundObject = new GameObject("Ground");
            groundObject.AddComponent<MeshFilter>().sharedMesh = ground;
            var groundRenderer = groundObject.AddComponent<MeshRenderer>();
            groundRenderer.sharedMaterial = staticMaterial;
            groundRenderer.shadowCastingMode = ShadowCastingMode.Off;
            groundRenderer.receiveShadows = false;

            var swarmObject = new GameObject("Swarm");
            var swarm = swarmObject.AddComponent<SwarmRenderer>();
            var so = new SerializedObject(swarm);
            so.FindProperty("unitMesh").objectReferenceValue = shapes[0];
            so.FindProperty("unitMaterial").objectReferenceValue = swarmMaterial;
            so.FindProperty("shadowMesh").objectReferenceValue = disc;
            so.FindProperty("shadowMaterial").objectReferenceValue = shadowMaterial;
            so.FindProperty("capacity").intValue = SwarmCapacity;
            so.FindProperty("initialCount").intValue = DefaultUnits;
            so.FindProperty("tint").colorValue = Crowd;
            so.FindProperty("laneHalfWidth").floatValue = TrackHalfWidth;
            so.ApplyModifiedPropertiesWithoutUndo();

            var rig = cameraObject.AddComponent<SwarmCameraRig>();
            var rigSo = new SerializedObject(rig);
            rigSo.FindProperty("profile").objectReferenceValue = cameraProfile;
            rigSo.FindProperty("swarm").objectReferenceValue = swarm;
            rigSo.ApplyModifiedPropertiesWithoutUndo();

            var canvasObject = new GameObject("DebugCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0f;
            Text fps = Label(canvasObject.transform, "Fps", new Vector2(-40f, -60f));
            Text units = Label(canvasObject.transform, "Units", new Vector2(-40f, -120f));
            var overlay = canvasObject.AddComponent<DevFpsOverlay>();
            var overlayObject = new SerializedObject(overlay);
            overlayObject.FindProperty("fpsLabel").objectReferenceValue = fps;
            overlayObject.FindProperty("unitsLabel").objectReferenceValue = units;
            overlayObject.FindProperty("swarm").objectReferenceValue = swarm;
            overlayObject.ApplyModifiedPropertiesWithoutUndo();

            var labObject = new GameObject("ToonLab");
            var lab = labObject.AddComponent<ToonLab>();
            RectTransform panel = TestPanel(canvasObject.transform, lab);
            var labSo = new SerializedObject(lab);
            labSo.FindProperty("swarm").objectReferenceValue = swarm;
            labSo.FindProperty("cameraRig").objectReferenceValue = rig;
            labSo.FindProperty("panel").objectReferenceValue = panel;
            labSo.FindProperty("editorUnits").intValue = DefaultUnits;
            // Bolinha and Pintinho, each with and without eyes (UnitShapeAssets order); the eyed Bolinha is the
            // provisional default while D25 is open.
            SerializedProperty choices = labSo.FindProperty("shapes");
            choices.arraySize = ShapeLabels.Length;
            for (int i = 0; i < ShapeLabels.Length; i++)
            {
                SerializedProperty choice = choices.GetArrayElementAtIndex(i);
                choice.FindPropertyRelative("label").stringValue = ShapeLabels[i];
                choice.FindPropertyRelative("withEyes").objectReferenceValue = shapes[2 * i];
                choice.FindPropertyRelative("withoutEyes").objectReferenceValue = shapes[2 * i + 1];
            }

            labSo.FindProperty("shapeIndex").intValue = 0;
            labSo.FindProperty("eyes").boolValue = true;
            labSo.ApplyModifiedPropertiesWithoutUndo();

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            EditorSceneManager.SaveScene(scene, ToonLab.ScenePath);
        }

        // Test panel at the bottom of the screen (the prototype drawer), three rows: +100, 1000!, restart at 10, shake (RT-6t.3);
        // Bolinha, Pintinho, with eyes, without eyes (RT-6t.2); ÷2, a bad gate with the ballistic death (RT-6t.5).
        private static RectTransform TestPanel(Transform canvas, ToonLab lab)
        {
            var panelObject = new GameObject("TestPanel", typeof(RectTransform));
            panelObject.transform.SetParent(canvas, false);
            var rect = (RectTransform)panelObject.transform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 36f);
            rect.sizeDelta = new Vector2(1000f, 385f);
            var background = panelObject.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.82f);
            var layout = panelObject.AddComponent<GridLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = new Vector2(14f, 14f);
            layout.cellSize = new Vector2(230f, 107f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4;

            UnityEventTools.AddIntPersistentListener(PanelButton(rect, "Add100", "+100", false).onClick, lab.AddUnits, 100);
            UnityEventTools.AddIntPersistentListener(PanelButton(rect, "Set1000", "1000!", true).onClick, lab.SetUnits, 1000);
            UnityEventTools.AddIntPersistentListener(PanelButton(rect, "Restart", "Recomeçar", false).onClick, lab.SetUnits, 10);
            UnityEventTools.AddVoidPersistentListener(PanelButton(rect, "Shake", "Tremer", false).onClick, lab.ShakeCamera);
            UnityEventTools.AddIntPersistentListener(PanelButton(rect, "ShapeBolinha", ShapeLabels[0], false).onClick, lab.SelectShape, 0);
            UnityEventTools.AddIntPersistentListener(PanelButton(rect, "ShapePintinho", ShapeLabels[1], false).onClick, lab.SelectShape, 1);
            UnityEventTools.AddBoolPersistentListener(PanelButton(rect, "EyesOn", "Com olhos", false).onClick, lab.SetEyes, true);
            UnityEventTools.AddBoolPersistentListener(PanelButton(rect, "EyesOff", "Sem olhos", false).onClick, lab.SetEyes, false);
            UnityEventTools.AddVoidPersistentListener(PanelButton(rect, "Halve", "÷2", false).onClick, lab.HalveCrowd);
            return rect;
        }

        private static Button PanelButton(Transform parent, string name, string caption, bool accent)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);
            var image = buttonObject.AddComponent<Image>();
            image.color = accent ? (Color)Accent : Color.white;
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            var textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(buttonObject.transform, false);
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;
            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 40;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = accent ? Color.white : (Color)Ink;
            text.text = caption;
            text.raycastTarget = false;
            return button;
        }

        private static Text Label(Transform parent, string name, Vector2 position)
        {
            var labelObject = new GameObject(name, typeof(RectTransform));
            labelObject.transform.SetParent(parent, false);
            var rect = (RectTransform)labelObject.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(420f, 60f);
            var text = labelObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 44;
            text.alignment = TextAnchor.UpperRight;
            text.color = Ink;
            text.raycastTarget = false;
            return text;
        }

        // Ground, track (two stripe colors, from 60 m behind the crowd: the portrait camera stands far back) and side rails
        // as one static mesh; colors are fixed (alpha 0, no tint).
        private static Mesh GroundMesh()
        {
            var builder = new LowPolyMeshBuilder();
            builder.AddBox(LowPolyMeshBuilder.Place(new Vector3(0f, -0.06f, 30f), new Vector3(240f, 0.1f, 240f)), Fixed(Ground));
            for (int i = 0; i < 50; i++)
            {
                float z = -60f + i * 4f;
                builder.AddBox(LowPolyMeshBuilder.Place(new Vector3(0f, -0.005f, z + 1f), new Vector3(2f * TrackHalfWidth + 0.6f, 0.01f, 2f)),
                    Fixed(i % 2 == 0 ? Track : Stripe));
                builder.AddBox(LowPolyMeshBuilder.Place(new Vector3(0f, -0.005f, z + 3f), new Vector3(2f * TrackHalfWidth + 0.6f, 0.01f, 2f)),
                    Fixed(i % 2 == 0 ? Stripe : Track));
            }

            for (int side = -1; side <= 1; side += 2)
            {
                builder.AddBox(LowPolyMeshBuilder.Place(new Vector3(side * (TrackHalfWidth + 0.45f), 0.15f, 40f), new Vector3(0.3f, 0.3f, 200f)), Fixed(Stripe));
            }

            return builder.ToMesh("LabGround");
        }

        private static Mesh SaveMesh(Mesh mesh, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }

            existing.Clear();
            EditorUtility.CopySerialized(mesh, existing);
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static Material SaveMaterial(string path, Shader shader, System.Action<Material> configure)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            configure(material);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static T Load<T>(string path) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new System.InvalidOperationException("missing " + path);
            }

            return asset;
        }

        private static Shader Find(string name)
        {
            Shader shader = Shader.Find(name);
            if (shader == null)
            {
                throw new System.InvalidOperationException("shader not found: " + name);
            }

            return shader;
        }

        private static void SetObject(Object target, string property, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(property).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static Color32 Fixed(Color32 color)
        {
            color.a = 0;
            return color;
        }

        private static Color32 Hex(int rgb)
        {
            return new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);
        }
    }
}
