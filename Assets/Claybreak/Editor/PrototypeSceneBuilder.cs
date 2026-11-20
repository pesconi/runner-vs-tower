using System;
using System.Collections.Generic;
using System.IO;
using JungleDegree.StudioKit.Characters;
using JungleDegree.StudioKit.Core;
using JungleDegree.StudioKit.Crowd;
using JungleDegree.StudioKit.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Claybreak.Editor
{
    /// <summary>
    /// Builds the RT-2 prototype module from code: Android settings, generated materials and placeholder
    /// sounds, the theme, the level data (created once, then tuned by hand) and the scene, with the
    /// 300-unit runner pool and every projectile preallocated (rule 2.2.1). Idempotent: running it again
    /// rebuilds the scene and the generated assets and keeps the level data.
    /// Batchmode (Editor closed): -executeMethod Claybreak.Editor.PrototypeSceneBuilder.BuildAndQuit
    /// </summary>
    public static class PrototypeSceneBuilder
    {
        public const string Root = "Assets/Claybreak/Prototype";
        public const string ScenePath = Root + "/ClaybreakPrototype.unity";
        public const string LevelPath = Root + "/Data/PrototypeLevel.asset";
        public const string StressLevelPath = Root + "/Data/DeviceGateLevel.asset";
        public const string ThemePath = Root + "/Data/ClaybreakTheme.asset";
        public const int PoolSize = 300;
        public const int ProjectilePoolSize = 32;

        private const string MaterialFolder = Root + "/Materials";
        private const string AudioFolder = Root + "/Audio";
        private const string GeneratedFolder = Root + "/Generated";
        private const string UnitPrefabPath = GeneratedFolder + "/RunnerUnit.prefab";
        private const string PillTexturePath = GeneratedFolder + "/ClayPill.png";
        private const string KitShared = "Packages/com.jungledegree.studiokit/Shared";
        private const string BodyPath = KitShared + "/ClayRunner/ClayRunner.fbx";
        private const string BodyFramesPath = KitShared + "/ClayRunner/ClayRunnerFrames.asset";
        private const string ClayMaterialPath = KitShared + "/RunnerClay.mat";
        private const string SplatMaterialPath = KitShared + "/RunnerSplat.mat";
        private const float TrackWidth = 8f;
        private const float TrackStartZ = -12f;

        [MenuItem("Claybreak/Build Prototype Scene")]
        public static void Build()
        {
            EnsureFolder(Root + "/Data");
            EnsureFolder(MaterialFolder);
            EnsureFolder(AudioFolder);
            EnsureFolder(GeneratedFolder);
            ProjectConfiguration.Apply();
            LoadOrCreateLevel(LevelPath, false);
            LoadOrCreateLevel(StressLevelPath, true);
            BuildTheme();
            new Palette();
            AssetDatabase.SaveAssets();
            CreateScene();
            AssetDatabase.SaveAssets();
        }

        public static void BuildAndQuit()
        {
            BatchRunner.Run("PrototypeSceneBuilder", Build);
        }

        // ---------- data ----------

        private static PrototypeLevel LoadOrCreateLevel(string path, bool deviceGate)
        {
            var existing = AssetDatabase.LoadAssetAtPath<PrototypeLevel>(path);
            if (existing != null)
            {
                return existing;
            }

            var level = ScriptableObject.CreateInstance<PrototypeLevel>();
            AssetDatabase.CreateAsset(level, path);
            var so = new SerializedObject(level);
            if (deviceGate)
            {
                // Device gate (reuso-tecnico 6.3): 300 units in flight and the projectile limit in use,
                // held for as long as the measurement runs.
                Set(so, "startCount", PoolSize);
                Set(so, "runSpeed", 12f);
                SetRunGates(so.FindProperty("runGates"));
                Set(so, "firingLineZ", 24f);
                Set(so, "launchRate", 150f);
                Set(so, "unitSpeed", 5f);
                Set(so, "laneSpread", 1.1f);
                Set(so, "maximumUnitsInFlight", PoolSize);
                Set(so, "assaultGateDistance", 9f);
                SetAssaultGates(so.FindProperty("assaultGates"), GateOperation.Multiply, 1, GateOperation.Multiply, 1,
                    GateOperation.Multiply, 1);
                Set(so, "autoLaunch", true);
                Set(so, "recycleUnits", true);
                Set(so, "towerDistance", 62f);
                Set(so, "towerHealth", NumberLabels.CachedMaximum);
                Set(so, "wallLane", 2);
                Set(so, "wallDistance", 20f);
                Set(so, "wallHealth", 1);
                Set(so, "cannonInterval", 0.05f);
                Set(so, "projectileFlightTime", 1f);
                Set(so, "blastRadius", 1.2f);
                Set(so, "maximumProjectiles", 12);
            }
            else
            {
                // Played level: about 18 s of run and 15-20 s of assault (GDD 3: 30-90 s in all). The
                // good gates give 160 units; x2 lane then wins with room (GDD 3: the 1st level is won
                // at the 1st try); a careless run (about 60) loses unless it uses the x3 behind the wall.
                Set(so, "startCount", 10);
                Set(so, "runSpeed", 5.5f);
                SetRunGates(so.FindProperty("runGates"),
                    (20f, new GateSpec(GateOperation.Add, 10), new GateSpec(GateOperation.Multiply, 2)),
                    (40f, new GateSpec(GateOperation.Multiply, 3), new GateSpec(GateOperation.Subtract, 8)),
                    (60f, new GateSpec(GateOperation.Subtract, 15), new GateSpec(GateOperation.Add, 20)),
                    (80f, new GateSpec(GateOperation.Divide, 2), new GateSpec(GateOperation.Multiply, 2)));
                Set(so, "firingLineZ", 96f);
                Set(so, "launchRate", 14f);
                Set(so, "unitSpeed", 9f);
                Set(so, "laneSpread", 0.7f);
                Set(so, "maximumUnitsInFlight", PoolSize);
                Set(so, "assaultGateDistance", 9f);
                SetAssaultGates(so.FindProperty("assaultGates"), GateOperation.Multiply, 2, GateOperation.Divide, 2,
                    GateOperation.Multiply, 3);
                Set(so, "towerDistance", 32f);
                Set(so, "towerHealth", 150);
                Set(so, "damagePerUnit", 1);
                Set(so, "wallLane", 2);
                Set(so, "wallDistance", 20f);
                Set(so, "wallHealth", 60);
                Set(so, "cannonInterval", 2.2f);
                Set(so, "projectileFlightTime", 0.9f);
                Set(so, "blastRadius", 1.1f);
                Set(so, "maximumProjectiles", 4);
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
            return level;
        }

        private static void SetRunGates(SerializedProperty array, params (float z, GateSpec left, GateSpec right)[] pairs)
        {
            array.arraySize = pairs.Length;
            for (int i = 0; i < pairs.Length; i++)
            {
                SerializedProperty pair = array.GetArrayElementAtIndex(i);
                pair.FindPropertyRelative("z").floatValue = pairs[i].z;
                SetGate(pair.FindPropertyRelative("left"), pairs[i].left);
                SetGate(pair.FindPropertyRelative("right"), pairs[i].right);
            }
        }

        private static void SetAssaultGates(SerializedProperty array, GateOperation a, int av, GateOperation b, int bv,
            GateOperation c, int cv)
        {
            array.arraySize = PrototypeLevel.LaneCount;
            SetGate(array.GetArrayElementAtIndex(0), new GateSpec(a, av));
            SetGate(array.GetArrayElementAtIndex(1), new GateSpec(b, bv));
            SetGate(array.GetArrayElementAtIndex(2), new GateSpec(c, cv));
        }

        private static void SetGate(SerializedProperty gate, GateSpec spec)
        {
            gate.FindPropertyRelative("operation").enumValueIndex = (int)spec.operation;
            gate.FindPropertyRelative("value").intValue = spec.value;
        }

        // The theme is builder-owned: body, clips and splat material are regenerated on every build.
        private static ClaybreakTheme BuildTheme()
        {
            var theme = AssetDatabase.LoadAssetAtPath<ClaybreakTheme>(ThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<ClaybreakTheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }

            var so = new SerializedObject(theme);
            so.FindProperty("roster.builtInBody").objectReferenceValue = Load<RunnerBodyFrames>(BodyFramesPath);
            SerializedProperty audio = so.FindProperty("audio");
            audio.FindPropertyRelative("enabled").boolValue = true;
            Array roles = Enum.GetValues(typeof(GameSound));
            SerializedProperty sounds = audio.FindPropertyRelative("sounds");
            sounds.arraySize = roles.Length;
            for (int i = 0; i < roles.Length; i++)
            {
                var role = (GameSound)roles.GetValue(i);
                SerializedProperty entry = sounds.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("sound").enumValueIndex = (int)role;
                entry.FindPropertyRelative("clip").objectReferenceValue = PlaceholderAudio.Get(AudioFolder, role);
                entry.FindPropertyRelative("volume").floatValue = role == GameSound.Coin ? 0.45f : 0.8f;
                entry.FindPropertyRelative("pitchVariation").floatValue = 0.08f;
                entry.FindPropertyRelative("minimumInterval").floatValue =
                    role == GameSound.Coin || role == GameSound.UnitLoss ? 0.08f : 0.05f;
            }

            audio.FindPropertyRelative("menuMusic").objectReferenceValue = PlaceholderAudio.GetMusic(AudioFolder, false);
            audio.FindPropertyRelative("runMusic").objectReferenceValue = PlaceholderAudio.GetMusic(AudioFolder, true);
            audio.FindPropertyRelative("musicVolume").floatValue = 0.3f;
            SerializedProperty haptics = audio.FindPropertyRelative("haptics");
            haptics.arraySize = 2;
            SetHaptic(haptics.GetArrayElementAtIndex(0), GameSound.UnitLoss, 25, 120, 0.15f);
            SetHaptic(haptics.GetArrayElementAtIndex(1), GameSound.Victory, 80, 200, 0.5f);

            SerializedProperty death = so.FindProperty("deathEffect");
            death.FindPropertyRelative("enabled").boolValue = true;
            death.FindPropertyRelative("splatMaterial").objectReferenceValue = Load<Material>(SplatMaterialPath);
            death.FindPropertyRelative("primaryColor").colorValue = new Color(0.95f, 0.55f, 0.18f);
            death.FindPropertyRelative("secondaryColor").colorValue = new Color(0.6f, 0.25f, 0.1f);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(theme);
            return theme;
        }

        private static void SetHaptic(SerializedProperty entry, GameSound sound, int milliseconds, int amplitude, float interval)
        {
            entry.FindPropertyRelative("sound").enumValueIndex = (int)sound;
            entry.FindPropertyRelative("milliseconds").intValue = milliseconds;
            entry.FindPropertyRelative("amplitude").intValue = amplitude;
            entry.FindPropertyRelative("minimumInterval").floatValue = interval;
        }

        // ---------- scene ----------

        private static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Load every asset after NewScene: opening a scene unloads assets nothing references, and a
            // reference taken before it would be serialized as missing.
            var level = Load<PrototypeLevel>(LevelPath);
            var stress = Load<PrototypeLevel>(StressLevelPath);
            var theme = Load<ClaybreakTheme>(ThemePath);
            var palette = new Palette();
            Mesh body = LoadFirstMesh(BodyPath);
            Material clay = Load<Material>(ClayMaterialPath);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.42f, 0.36f, 0.34f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.16f, 0.12f, 0.14f);
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 120f;
            var sun = new GameObject("Sun", typeof(Light));
            Light light = sun.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.86f, 0.7f);
            light.intensity = 1.2f;
            light.shadows = LightShadows.None;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            float trackEnd = Mathf.Max(level.TowerZ, stress.TowerZ) + 16f;
            Renderer track = CreateBlock("Track", null, new Vector3(0f, -0.1f, (TrackStartZ + trackEnd) * 0.5f),
                new Vector3(TrackWidth, 0.2f, trackEnd - TrackStartZ), palette.Ground);
            CreateBlock("FieldLeft", null, new Vector3(-TrackWidth - 6f, -0.15f, (TrackStartZ + trackEnd) * 0.5f),
                new Vector3(TrackWidth + 4f, 0.2f, trackEnd - TrackStartZ), palette.Field);
            CreateBlock("FieldRight", null, new Vector3(TrackWidth + 6f, -0.15f, (TrackStartZ + trackEnd) * 0.5f),
                new Vector3(TrackWidth + 4f, 0.2f, trackEnd - TrackStartZ), palette.Field);

            var flowObject = new GameObject("ClaybreakFlow");
            ClaybreakFlow flow = flowObject.AddComponent<ClaybreakFlow>();
            GameAudio gameAudio = flowObject.AddComponent<GameAudio>();
            FrameRateGovernor governor = flowObject.AddComponent<FrameRateGovernor>();

            RunnerPool pool = CreatePool(body, clay);

            var leader = new GameObject("Leader", typeof(MeshFilter), typeof(MeshRenderer), typeof(CharacterController));
            leader.transform.position = new Vector3(0f, 0.5f, 0f);
            leader.GetComponent<MeshFilter>().sharedMesh = body;
            leader.GetComponent<MeshRenderer>().sharedMaterial = clay;
            var controller = leader.GetComponent<CharacterController>();
            controller.height = 1f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.5f, 0f);
            var player = leader.AddComponent<PlayerController>();
            SetRef(player, "runPhaseSource", flow);
            SetRef(player, "trackRenderer", track);
            // The run speed comes from the level (course speed multiplier); this is the unit speed.
            Set(player, "forwardSpeed", 1f);
            var crowd = leader.AddComponent<RunnerCrowd>();
            SetRef(crowd, "runnerPool", pool);
            SetRef(crowd, "trackRenderer", track);
            Set(crowd, "initialCount", 1);

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = RenderSettings.fogColor;
            camera.farClipPlane = 160f;
            camera.fieldOfView = 60f;
            cameraObject.transform.position = new Vector3(0f, 9f, -8f);
            cameraObject.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
            var follow = cameraObject.AddComponent<CameraFollow>();
            SetRef(follow, "target", leader.transform);
            Set(follow, "offset", new Vector3(0f, 9f, -8f));
            Set(follow, "lookAheadDistance", 6f);
            var assaultCamera = cameraObject.AddComponent<AssaultCamera>();

            RunGateCourse gates = CreateRunGates(palette);
            Transform firingLine = CreateBlock("FiringLine", null, new Vector3(0f, 0.01f, level.FiringLineZ),
                new Vector3(TrackWidth, 0.03f, 0.35f), palette.Line).transform;
            Transform laneMarker = CreateBlock("LaneMarker", null, new Vector3(0f, 0.015f, level.FiringLineZ + 1.5f),
                new Vector3(TrackWidth / PrototypeLevel.LaneCount * 0.9f, 0.02f, 3f), palette.Marker).transform;
            AssaultTower tower = CreateTower(level, palette, out CannonDefense cannon);
            AssaultWall wall = CreateWall(level, palette);
            AssaultField field = CreateField(pool, tower, wall, level, palette);
            PrototypeHud hud = CreateHud();

            SetRef(flow, "level", level);
            SetRef(flow, "stressLevel", stress);
            SetRef(flow, "theme", theme);
            SetRef(flow, "track", track);
            SetRef(flow, "player", player);
            SetRef(flow, "crowd", crowd);
            SetRef(flow, "pool", pool);
            SetRef(flow, "runCamera", follow);
            SetRef(flow, "assaultCamera", assaultCamera);
            SetRef(flow, "gates", gates);
            SetRef(flow, "field", field);
            SetRef(flow, "tower", tower);
            SetRef(flow, "cannon", cannon);
            SetRef(flow, "hud", hud);
            SetRef(flow, "firingLine", firingLine);
            SetRef(flow, "laneMarker", laneMarker);
            SetRef(flow, "gameAudio", gameAudio);
            SetRef(flow, "frameRateGovernor", governor);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log($"[Claybreak] Scene {ScenePath}: pool {PoolSize}, {ProjectilePoolSize} projectiles, "
                + $"{RunGateCourse.MaximumPairs} gate pairs, track to z={trackEnd:0}.");
        }

        private static RunnerPool CreatePool(Mesh body, Material clay)
        {
            var unitObject = new GameObject("RunnerUnit", typeof(MeshFilter), typeof(MeshRenderer));
            unitObject.GetComponent<MeshFilter>().sharedMesh = body;
            unitObject.GetComponent<MeshRenderer>().sharedMaterial = clay;
            unitObject.AddComponent<RunnerUnit>();
            var capsule = unitObject.GetComponent<CapsuleCollider>();
            capsule.height = 1f;
            capsule.radius = 0.25f;
            capsule.center = new Vector3(0f, 0.5f, 0f);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(unitObject, UnitPrefabPath);
            UnityEngine.Object.DestroyImmediate(unitObject);

            var poolObject = new GameObject("RunnerPool");
            var pool = poolObject.AddComponent<RunnerPool>();
            var so = new SerializedObject(pool);
            so.FindProperty("runnerPrefab").objectReferenceValue = prefab.GetComponent<RunnerUnit>();
            so.FindProperty("poolSize").intValue = PoolSize;
            SerializedProperty units = so.FindProperty("preallocatedUnits");
            units.arraySize = PoolSize;
            for (int i = 0; i < PoolSize; i++)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, poolObject.transform);
                instance.name = $"RunnerUnit_{i + 1:000}";
                instance.SetActive(false);
                units.GetArrayElementAtIndex(i).objectReferenceValue = instance.GetComponent<RunnerUnit>();
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return pool;
        }

        private static RunGateCourse CreateRunGates(Palette palette)
        {
            var root = new GameObject("RunGates");
            RunGateCourse course = root.AddComponent<RunGateCourse>();
            int pairs = RunGateCourse.MaximumPairs;
            var roots = new UnityEngine.Object[pairs];
            var leftLabels = new UnityEngine.Object[pairs];
            var rightLabels = new UnityEngine.Object[pairs];
            var leftPanels = new UnityEngine.Object[pairs];
            var rightPanels = new UnityEngine.Object[pairs];
            float half = TrackWidth * 0.25f;
            for (int i = 0; i < pairs; i++)
            {
                var pair = new GameObject("Pair" + (i + 1));
                pair.transform.SetParent(root.transform, false);
                pair.transform.localPosition = new Vector3(0f, 0f, 20f * (i + 1));
                leftPanels[i] = CreateBlock("Left", pair.transform, new Vector3(-half, 1.1f, 0f),
                    new Vector3(TrackWidth * 0.48f, 2.2f, 0.12f), palette.GateGood);
                rightPanels[i] = CreateBlock("Right", pair.transform, new Vector3(half, 1.1f, 0f),
                    new Vector3(TrackWidth * 0.48f, 2.2f, 0.12f), palette.GateBad);
                leftLabels[i] = CreateLabel("LeftLabel", pair.transform, new Vector3(-half, 1.2f, -0.1f), 0.32f);
                rightLabels[i] = CreateLabel("RightLabel", pair.transform, new Vector3(half, 1.2f, -0.1f), 0.32f);
                CreateBlock("Post", pair.transform, new Vector3(0f, 1.2f, 0f), new Vector3(0.18f, 2.4f, 0.18f), palette.Line);
                roots[i] = pair.transform;
            }

            SetArray(course, "pairRoots", roots);
            SetArray(course, "leftLabels", leftLabels);
            SetArray(course, "rightLabels", rightLabels);
            SetArray(course, "leftPanels", leftPanels);
            SetArray(course, "rightPanels", rightPanels);
            SetRef(course, "goodMaterial", palette.GateGood);
            SetRef(course, "badMaterial", palette.GateBad);
            return course;
        }

        private static AssaultField CreateField(RunnerPool pool, AssaultTower tower, AssaultWall wall, PrototypeLevel level,
            Palette palette)
        {
            var root = new GameObject("AssaultField");
            AssaultField field = root.AddComponent<AssaultField>();
            var visuals = new UnityEngine.Object[PrototypeLevel.LaneCount];
            var labels = new UnityEngine.Object[PrototypeLevel.LaneCount];
            var panels = new UnityEngine.Object[PrototypeLevel.LaneCount];
            float laneWidth = TrackWidth / PrototypeLevel.LaneCount;
            for (int i = 0; i < PrototypeLevel.LaneCount; i++)
            {
                var gate = new GameObject("AssaultGate" + (i + 1));
                gate.transform.SetParent(root.transform, false);
                gate.transform.position = new Vector3(-TrackWidth * 0.5f + laneWidth * (i + 0.5f), 0f, level.AssaultGateZ);
                panels[i] = CreateBlock("Panel", gate.transform, new Vector3(0f, 0.9f, 0f),
                    new Vector3(laneWidth * 0.92f, 1.8f, 0.12f), palette.GateGood);
                labels[i] = CreateLabel("Label", gate.transform, new Vector3(0f, 1f, -0.1f), 0.28f);
                visuals[i] = gate.transform;
            }

            SetRef(field, "runnerPool", pool);
            SetRef(field, "tower", tower);
            SetRef(field, "wall", wall);
            SetArray(field, "gateVisuals", visuals);
            SetArray(field, "gateLabels", labels);
            SetArray(field, "gatePanels", panels);
            SetRef(field, "goodGateMaterial", palette.GateGood);
            SetRef(field, "badGateMaterial", palette.GateBad);
            return field;
        }

        private static AssaultTower CreateTower(PrototypeLevel level, Palette palette, out CannonDefense cannon)
        {
            var root = new GameObject("Tower");
            root.transform.position = new Vector3(0f, 0f, level.TowerZ);
            var bodyRoot = new GameObject("Body").transform;
            bodyRoot.SetParent(root.transform, false);
            CreateBlock("Base", bodyRoot, new Vector3(0f, 2.5f, 3f), new Vector3(TrackWidth + 1f, 5f, 6f), palette.Stone);
            CreateBlock("Keep", bodyRoot, new Vector3(0f, 7f, 3.5f), new Vector3(TrackWidth * 0.55f, 4f, 4f), palette.StoneDark);
            for (int i = 0; i < 5; i++)
            {
                CreateBlock("Battlement" + i, bodyRoot, new Vector3(-TrackWidth * 0.5f + i * TrackWidth * 0.25f, 5.5f, 0.5f),
                    new Vector3(0.9f, 1f, 0.9f), palette.Stone);
            }

            CreateBlock("Gate", bodyRoot, new Vector3(0f, 1.4f, -0.02f), new Vector3(2.4f, 2.8f, 0.1f), palette.StoneDark);

            var bar = new GameObject("HealthBar").transform;
            bar.SetParent(root.transform, false);
            bar.localPosition = new Vector3(0f, 10.2f, 1f);
            bar.localScale = new Vector3(6f, 0.45f, 0.2f);
            CreateBlock("Back", bar, Vector3.zero, Vector3.one, palette.StoneDark);
            Transform fill = CreateBlock("Fill", bar, new Vector3(0f, 0f, -0.2f), new Vector3(1f, 0.8f, 0.8f), palette.Health).transform;
            TextMesh label = CreateLabel("HealthLabel", root.transform, new Vector3(0f, 11.4f, 1f), 0.45f);

            AssaultTower tower = root.AddComponent<AssaultTower>();
            SetRef(tower, "body", bodyRoot);
            SetRef(tower, "healthFill", fill);
            SetRef(tower, "healthLabel", label);

            var cannonObject = new GameObject("Cannon");
            cannonObject.transform.SetParent(root.transform, false);
            CreateBlock("Barrel", cannonObject.transform, new Vector3(0f, 9.4f, 1.2f), new Vector3(0.9f, 0.9f, 2.2f), palette.Iron);
            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(cannonObject.transform, false);
            muzzle.localPosition = new Vector3(0f, 9.4f, 0f);
            cannon = cannonObject.AddComponent<CannonDefense>();
            var projectiles = new UnityEngine.Object[ProjectilePoolSize];
            var shadows = new UnityEngine.Object[ProjectilePoolSize];
            var pool = new GameObject("ProjectilePool").transform;
            pool.SetParent(cannonObject.transform, false);
            for (int i = 0; i < ProjectilePoolSize; i++)
            {
                GameObject stone = CreatePrimitive(PrimitiveType.Sphere, "Stone" + (i + 1), pool, Vector3.zero,
                    Vector3.one * 1.1f, palette.Iron);
                stone.SetActive(false);
                projectiles[i] = stone.transform;
                GameObject shadow = CreatePrimitive(PrimitiveType.Cylinder, "Shadow" + (i + 1), pool, Vector3.zero,
                    new Vector3(2f, 0.02f, 2f), palette.Shadow);
                shadow.SetActive(false);
                shadows[i] = shadow.transform;
            }

            SetRef(cannon, "muzzle", muzzle);
            SetArray(cannon, "projectiles", projectiles);
            SetArray(cannon, "shadows", shadows);
            return tower;
        }

        private static AssaultWall CreateWall(PrototypeLevel level, Palette palette)
        {
            var root = new GameObject("Wall");
            float laneWidth = TrackWidth / PrototypeLevel.LaneCount;
            root.transform.position = new Vector3(-TrackWidth * 0.5f + laneWidth * (level.WallLane + 0.5f), 0f, level.WallZ);
            Transform body = CreateBlock("Body", root.transform, new Vector3(0f, 1.3f, 0f),
                new Vector3(laneWidth * 0.95f, 2.6f, 0.9f), palette.Wall).transform;
            TextMesh label = CreateLabel("HealthLabel", root.transform, new Vector3(0f, 3.4f, 0f), 0.3f);
            AssaultWall wall = root.AddComponent<AssaultWall>();
            SetRef(wall, "body", body);
            SetRef(wall, "healthLabel", label);
            return wall;
        }

        private static PrototypeHud CreateHud()
        {
            Sprite pill = CreatePillSprite();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("Hud", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;
            Transform canvas = canvasObject.transform;

            Text army = CreateText("Army", canvas, font, new Vector2(0f, 1f), new Vector2(60f, -110f), 64, TextAnchor.UpperLeft);
            Text tower = CreateText("Tower", canvas, font, new Vector2(1f, 1f), new Vector2(-60f, -110f), 64, TextAnchor.UpperRight);
            Text hint = CreateText("Hint", canvas, font, new Vector2(0.5f, 0f), new Vector2(0f, 260f), 56, TextAnchor.MiddleCenter);

            var card = new GameObject("ResultCard", typeof(RectTransform), typeof(Image));
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.SetParent(canvas, false);
            cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(860f, 640f);
            var cardImage = card.GetComponent<Image>();
            cardImage.color = new Color(0.12f, 0.09f, 0.1f, 0.88f);
            ClayUi.MakePill(cardImage, pill, 120f);
            Text title = CreateText("Title", cardRect, font, new Vector2(0.5f, 1f), new Vector2(0f, -150f), 96, TextAnchor.MiddleCenter);
            Text detail = CreateText("Detail", cardRect, font, new Vector2(0.5f, 1f), new Vector2(0f, -280f), 52, TextAnchor.MiddleCenter);
            Button retry = CreateButton(cardRect, pill, font, "AGAIN");
            card.SetActive(false);

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            PrototypeHud hud = canvasObject.AddComponent<PrototypeHud>();
            SetRef(hud, "armyText", army);
            SetRef(hud, "towerText", tower);
            SetRef(hud, "hintText", hint);
            SetRef(hud, "resultCard", card);
            SetRef(hud, "resultTitle", title);
            SetRef(hud, "resultDetail", detail);
            SetRef(hud, "retryButton", retry);
            return hud;
        }

        private static Text CreateText(string name, Transform parent, Font font, Vector2 anchor, Vector2 position, int size,
            TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(900f, 120f);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            go.GetComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.7f);
            return text;
        }

        private static Button CreateButton(RectTransform parent, Sprite pill, Font font, string caption)
        {
            const float height = 150f;
            const float slab = 10f;
            var buttonObject = new GameObject("Retry", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 80f);
            rect.sizeDelta = new Vector2(520f, height);
            var face = buttonObject.GetComponent<Image>();
            face.color = new Color(0.95f, 0.55f, 0.18f);
            ClayUi.MakePill(face, pill, height);
            ClayUi.AddSlab(face, slab);
            ClayUi.AddGloss(rect, pill, height);
            ClayUi.AddPress(buttonObject.GetComponent<Button>(), slab);
            Text label = CreateText("Label", rect, font, new Vector2(0.5f, 0.5f), Vector2.zero, 68, TextAnchor.MiddleCenter);
            label.text = caption;
            return buttonObject.GetComponent<Button>();
        }

        // ---------- primitives ----------

        private static MeshRenderer CreateBlock(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            return CreatePrimitive(PrimitiveType.Cube, name, parent, position, scale, material).GetComponent<MeshRenderer>();
        }

        // Scene props carry no collider: the prototype never uses physics queries.
        private static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent, Vector3 position,
            Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return go;
        }

        private static TextMesh CreateLabel(string name, Transform parent, Vector3 position, float size)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go = new GameObject(name, typeof(MeshRenderer), typeof(TextMesh));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var text = go.GetComponent<TextMesh>();
            text.font = font;
            text.fontSize = 64;
            text.characterSize = size * 0.16f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.text = "0";
            go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return text;
        }

        // A 64 px white circle sliced with a 32 px border: the pill shape the kit's ClayUi expects.
        private static Sprite CreatePillSprite()
        {
            if (!File.Exists(PillTexturePath))
            {
                const int size = 64;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x + 0.5f - size * 0.5f;
                        float dy = y + 0.5f - size * 0.5f;
                        float alpha = Mathf.Clamp01(size * 0.5f - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                        pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                    }
                }

                texture.SetPixels32(pixels);
                File.WriteAllBytes(PillTexturePath, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(PillTexturePath);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(PillTexturePath);
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteBorder != Vector4.one * 32f)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = Vector4.one * 32f;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            return Load<Sprite>(PillTexturePath);
        }

        /// <summary>Generated flat materials (URP Simple Lit; translucent ones on URP Unlit).</summary>
        private sealed class Palette
        {
            public readonly Material Ground = Solid("Ground", new Color(0.34f, 0.27f, 0.24f));
            public readonly Material Field = Solid("Field", new Color(0.2f, 0.17f, 0.17f));
            public readonly Material Stone = Solid("Stone", new Color(0.46f, 0.42f, 0.42f));
            public readonly Material StoneDark = Solid("StoneDark", new Color(0.24f, 0.21f, 0.22f));
            public readonly Material Iron = Solid("Iron", new Color(0.12f, 0.12f, 0.13f));
            public readonly Material Wall = Solid("Wall", new Color(0.55f, 0.36f, 0.24f));
            public readonly Material Health = Solid("Health", new Color(0.86f, 0.2f, 0.16f));
            public readonly Material Line = Solid("Line", new Color(0.95f, 0.92f, 0.86f));
            public readonly Material GateGood = Clear("GateGood", new Color(0.2f, 0.75f, 0.95f, 0.55f));
            public readonly Material GateBad = Clear("GateBad", new Color(0.92f, 0.25f, 0.2f, 0.55f));
            public readonly Material Marker = Clear("LaneMarker", new Color(1f, 0.8f, 0.25f, 0.35f));
            public readonly Material Shadow = Clear("Shadow", new Color(0f, 0f, 0f, 0.5f));

            private static Material Solid(string name, Color color)
            {
                Material material = LoadOrCreate(name, "Universal Render Pipeline/Simple Lit");
                material.SetColor("_BaseColor", color);
                EditorUtility.SetDirty(material);
                return material;
            }

            private static Material Clear(string name, Color color)
            {
                Material material = LoadOrCreate(name, "Universal Render Pipeline/Unlit");
                material.SetColor("_BaseColor", color);
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f);
                material.SetOverrideTag("RenderType", "Transparent");
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Transparent;
                EditorUtility.SetDirty(material);
                return material;
            }

            private static Material LoadOrCreate(string name, string shaderName)
            {
                string path = MaterialFolder + "/" + name + ".mat";
                Shader shader = Shader.Find(shaderName) ?? throw new InvalidOperationException("Shader not found: " + shaderName);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(shader) { name = name };
                    AssetDatabase.CreateAsset(material, path);
                }
                else if (material.shader != shader)
                {
                    material.shader = shader;
                }

                return material;
            }
        }

        // ---------- serialized helpers ----------

        private static void Set(UnityEngine.Object target, string property, float value)
        {
            var so = new SerializedObject(target);
            Find(so, property).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(UnityEngine.Object target, string property, int value)
        {
            var so = new SerializedObject(target);
            Find(so, property).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(UnityEngine.Object target, string property, Vector3 value)
        {
            var so = new SerializedObject(target);
            Find(so, property).vector3Value = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(SerializedObject so, string property, float value)
        {
            Find(so, property).floatValue = value;
        }

        private static void Set(SerializedObject so, string property, int value)
        {
            Find(so, property).intValue = value;
        }

        private static void Set(SerializedObject so, string property, bool value)
        {
            Find(so, property).boolValue = value;
        }

        private static void SetRef(UnityEngine.Object target, string property, UnityEngine.Object value)
        {
            var so = new SerializedObject(target);
            Find(so, property).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(UnityEngine.Object target, string property, IReadOnlyList<UnityEngine.Object> values)
        {
            var so = new SerializedObject(target);
            SerializedProperty array = Find(so, property);
            array.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedProperty Find(SerializedObject so, string property)
        {
            return so.FindProperty(property)
                ?? throw new InvalidOperationException($"{so.targetObject.GetType().Name}.{property} not found");
        }

        private static Mesh LoadFirstMesh(string path)
        {
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Mesh mesh)
                {
                    return mesh;
                }
            }

            throw new InvalidOperationException("No mesh in " + path);
        }

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("Missing " + path);
        }

        public static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
