using System;
using System.Collections.Generic;
using System.IO;
using JungleDegree.StudioKit.Ads;
using JungleDegree.StudioKit.Core;
using JungleDegree.StudioKit.Crowd;
using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using JungleDegree.StudioKit.UI;
using TMPro;
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
    /// Builds the game module from code: Android settings, the content (<see cref="WorldContent"/>: worlds,
    /// levels and the meta catalog), generated materials, placeholder sounds, the theme and the one scene
    /// with everything preallocated: the instanced run crowd (RT-6t.4, capped at <see cref="CrowdCapacity"/>), the
    /// instanced units in flight of the assault (RT-6t.7, <see cref="FlightCapacity"/>), every projectile, the gate pairs and crates,
    /// the three tower bodies (<see cref="TowerArt"/>), the wall, the sweeper, the HUD and the menu screens
    /// with one shop row and one world row per catalog entry (rule 2.2.1: nothing is created or destroyed
    /// while playing). Idempotent.
    /// Batchmode (Editor closed): -executeMethod Claybreak.Editor.ClaybreakSceneBuilder.BuildAndQuit
    /// </summary>
    public static class ClaybreakSceneBuilder
    {
        public const string Root = "Assets/Claybreak/Content";
        public const string ScenePath = Root + "/Claybreak.unity";
        public const string ThemePath = Root + "/Data/ClaybreakTheme.asset";
        /// <summary>Most units in flight at once in the assault (the instanced renderer's capacity): the game's crowd limit.</summary>
        public const int FlightCapacity = ArmyLimit.Units;

        /// <summary>Most assault units dying at once (the ballistic death ring of the units in flight).</summary>
        public const int FlightDyingCapacity = 512;

        /// <summary>
        /// Crowd cap of the run (D27 revised, RT-6t.14): the game's crowd limit, written into the swarm's capacity (the
        /// engine's swarm is built for 2000+; the cap is this game's data, from <see cref="ArmyLimit"/> alone).
        /// </summary>
        public const int CrowdCapacity = ArmyLimit.Units;
        public const string CrowdCameraProfilePath = Root + "/Data/SwarmCameraRun.asset";
        /// <summary>The camera rig's fixed shots of the assault (RT-6t.7; the old AssaultCamera values).</summary>
        public const string AssaultShotPath = Root + "/Data/SwarmCameraAssault.asset";
        public const string BossAssaultShotPath = Root + "/Data/SwarmCameraAssaultBoss.asset";
        public const string CrowdLookPath = Root + "/Data/CrowdToonLook.asset";
        /// <summary>The skybox: the palette's screen-space sky gradient (RT-6t.6a).</summary>
        public const string SkyMaterialPath = ArtPalette.MaterialFolder + "/SkyGradient.mat";

        public const int ProjectilePoolSize = 32;
        public const int LevelDots = ClaybreakWorld.MaximumLevels;
        public const float RowStep = 250f;
        /// <summary>The canvas scaler's match (0 = width): RT-6t.16, no side is cut from 16:9 to 21:9.</summary>
        public const float CanvasMatch = 0f;
        /// <summary>The home level meter's share of the safe width at most (RT-6t.16, plan 3.4).</summary>
        public const float LevelMeterShare = 0.6f;

        private const string GeneratedFolder = Root + "/Generated";
        public const string FontPath = GameFonts.TtfPath;
        private const string PillTexturePath = GeneratedFolder + "/ClayPill.png";
        private const string StarTexturePath = GeneratedFolder + "/Star.png";
        /// <summary>RT-6t.17: the boss level's mark on the home level meter.</summary>
        private const string CrownTexturePath = GeneratedFolder + "/Crown.png";
        /// <summary>RT-6t.17: the fade at the foot of a scroll list (opaque at the bottom, clear at the top).</summary>
        private const string FadeTexturePath = GeneratedFolder + "/FadeUp.png";
        public const float TrackWidth = 8f;

        /// <summary>Units an obstacle hit can have on their way out at once (RT-6t.18): two seconds of the course's kill cap is far more.</summary>
        public const int ObstacleDyingCapacity = 128;

        /// <summary>Far clip of the game camera (m): the props wrap past it, so none pops in on screen (RT-6t.13, no fog).</summary>
        public const float CameraFarClip = 170f;
        private const float TrackStartZ = -12f;
        /// <summary>The game scene's ground mesh (it spans this scene's track, so it is saved apart from the shared props).</summary>
        public const string GroundMeshPath = SceneryArt.Folder + "/GroundAndRails.asset";
        // The reference drag moves 16 m per screen width on a 6 m half-width track; the same feel on this track.
        private const float ReferenceDragMeters = 16f;
        private const float ReferenceTrackHalfWidth = 6f;
        private static readonly Color Orange = new Color(0.95f, 0.55f, 0.18f);
        private static readonly Color Gold = new Color(1f, 0.8f, 0.25f);

        [MenuItem("Claybreak/Build Scene")]
        public static void Build()
        {
            EnsureFolder(Root + "/Data");
            EnsureFolder(ArtPalette.MaterialFolder);
            EnsureFolder(GeneratedFolder);
            ProjectConfiguration.Apply();
            // The palettes and the sky come before the worlds: each world's look points at its palette.
            PaletteContent.Write();
            BuildSkyMaterial();
            AssetDatabase.SaveAssets();
            // The crowd shapes and the crowd materials come first: the catalog offers the shapes and the run draws with them.
            ToonLabBuilder.Build();
            // RT-6t.20: the shop figures of the shapes, drawn from their meshes, before the catalog points at them.
            ShapeFigures.Build();
            WorldContent.Write();
            MonetizationContent.Write();
            BuildCrowdData();
            // RT-6v3.1: the style guide (the toon look it reads exists by now).
            StyleGuideContent.Write();
            StyleChecklist.Write();
            BuildTheme();
            new ArtPalette();
            GateArt.BuildAssets();
            LeaderArt.BuildAssets();
            ObstacleArt.BuildAssets();
            CreatePillSprite();
            CreateStarSprite();
            CreateCrownSprite();
            CreateFadeSprite();
            AssetDatabase.SaveAssets();
            CreateScene();
            AssetDatabase.SaveAssets();
        }

        public static void BuildAndQuit()
        {
            BatchRunner.Run("ClaybreakSceneBuilder", Build);
        }

        // ---------- theme ----------

        // The theme is builder-owned: crowd color and clips are regenerated on every build.
        private static void BuildTheme()
        {
            var theme = AssetDatabase.LoadAssetAtPath<ClaybreakTheme>(ThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<ClaybreakTheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }

            var so = new SerializedObject(theme);
            so.FindProperty("crowdColor").colorValue = Orange;
            ClaybreakAudio.Apply(so.FindProperty("audio"));
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(theme);
        }

        // The run crowd's data assets (camera profile and toon look), created once with the reference values and then
        // left alone (tuned by hand).
        private static void BuildCrowdData()
        {
            if (AssetDatabase.LoadAssetAtPath<SwarmCameraProfile>(CrowdCameraProfilePath) == null)
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SwarmCameraProfile>(), CrowdCameraProfilePath);
            }

            if (AssetDatabase.LoadAssetAtPath<ToonLook>(CrowdLookPath) == null)
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ToonLook>(), CrowdLookPath);
            }

            // The assault shots keep the framing the assault camera had (17 m up and 11 m back, the boss 3 m higher and
            // farther back, looking 55% of the way to the tower, 60 degrees, 0.8 s blend).
            CreateShotOnce(AssaultShotPath, new Vector3(0f, 17f, -11f));
            CreateShotOnce(BossAssaultShotPath, new Vector3(0f, 20f, -14f));

            AssetDatabase.SaveAssets();
        }

        // The sky material (no properties of its own: it reads the palette's globals), created once.
        private static void BuildSkyMaterial()
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(SkyMaterialPath) != null)
            {
                return;
            }

            Shader shader = Shader.Find(ToonShaderIds.SkyGradientShader);
            if (shader == null)
            {
                throw new System.InvalidOperationException($"shader {ToonShaderIds.SkyGradientShader} not found");
            }

            AssetDatabase.CreateAsset(new Material(shader) { name = "SkyGradient" }, SkyMaterialPath);
        }

        // ---------- scene ----------

        private static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Load every asset after NewScene: opening a scene unloads assets nothing references, and a
            // reference taken before it would be serialized as missing.
            var catalog = Load<ClaybreakCatalog>(WorldContent.CatalogPath);
            var stress = new ClaybreakLevel[catalog.Worlds.Count];
            for (int w = 0; w < stress.Length; w++)
            {
                stress[w] = Load<ClaybreakLevel>(WorldContent.StressLevelPath(w));
            }

            var theme = Load<ClaybreakTheme>(ThemePath);
            var monetization = Load<ClaybreakMonetization>(MonetizationContent.MonetizationPath);
            var obstacleLab = Load<ClaybreakLevel>(WorldContent.ObstacleLabPath);
            var palette = new ArtPalette();

            float trackEnd = obstacleLab.TowerZ + 16f;
            foreach (ClaybreakLevel level in stress)
            {
                trackEnd = Mathf.Max(trackEnd, level.TowerZ + 16f);
            }

            foreach (WorldEntry entry in catalog.Worlds)
            {
                foreach (ClaybreakLevel level in entry.world.Levels)
                {
                    trackEnd = Mathf.Max(trackEnd, level.TowerZ + 16f);
                }
            }

            Light sun = CreateEnvironment();
            sun.gameObject.AddComponent<ToonLookApplier>();
            SetRef(sun.GetComponent<ToonLookApplier>(), "look", Load<ToonLook>(CrowdLookPath));
            ClaybreakLevel first = catalog.Worlds[0].world.Levels[0];

            var flowObject = new GameObject("ClaybreakFlow");
            ClaybreakFlow flow = flowObject.AddComponent<ClaybreakFlow>();
            GameAudio gameAudio = flowObject.AddComponent<GameAudio>();
            FrameRateGovernor governor = flowObject.AddComponent<FrameRateGovernor>();
            ClaybreakAds ads = CreateAds(flowObject.transform, monetization);

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.farClipPlane = ClaybreakSceneBuilder.CameraFarClip;
            camera.fieldOfView = 60f;
            cameraObject.transform.position = new Vector3(0f, 9f, -8f);
            cameraObject.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
            RunCrowd crowd = CreateCrowd(cameraObject, theme, catalog);
            WorldScenery scenery = CreateScenery(catalog, trackEnd, sun, camera);
            Renderer track = scenery.Track;

            RunGateCourse gates = CreateRunGates(palette);
            RunObstacleCourse obstacles = CreateObstacles(catalog);
            Transform firingLine = Prim.Box(null, "FiringLine", new Vector3(0f, 0.01f, first.FiringLineZ),
                new Vector3(TrackWidth, 0.03f, 0.35f), palette.Line).transform;
            Transform laneMarker = Prim.Box(null, "LaneMarker", new Vector3(0f, 0.015f, first.FiringLineZ + 1.5f),
                new Vector3(TrackWidth / ClaybreakLevel.LaneCount * 0.9f, 0.02f, 3f), palette.Marker).transform;
            AssaultTower tower = CreateTower(first, palette);
            CannonDefense cannon = CreateCannon(palette);
            AssaultWall wall = CreateWall(first, palette);
            SweeperDefense sweeper = CreateSweeper(palette);
            ShieldDefense shield = CreateShield(palette);
            AssaultField field = CreateField(tower, wall, sweeper, shield, first, palette, catalog);
            Sprite pill = Load<Sprite>(PillTexturePath);
            Sprite star = Load<Sprite>(StarTexturePath);
            Transform canvas = CreateCanvas();
            ClaybreakHud hud = CreateHud(canvas, pill);
            ClaybreakScreens screens = CreateScreens(canvas, pill, star, Load<Sprite>(CrownTexturePath), Load<Sprite>(FadeTexturePath), catalog);
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            SetRef(flow, "catalog", catalog);
            SetArray(flow, "stressLevels", stress);
            SetRef(flow, "scenery", scenery);
            SetRef(flow, "theme", theme);
            SetRef(flow, "track", track);
            SetRef(flow, "crowd", crowd);
            SetRef(flow, "assaultShot", Load<SwarmCameraShot>(AssaultShotPath));
            SetRef(flow, "bossAssaultShot", Load<SwarmCameraShot>(BossAssaultShotPath));
            SetRef(flow, "gates", gates);
            SetRef(flow, "obstacles", obstacles);
            SetRef(flow, "obstacleLab", obstacleLab);
            SetRef(flow, "field", field);
            SetRef(flow, "tower", tower);
            SetRef(flow, "cannon", cannon);
            SetRef(flow, "hud", hud);
            SetRef(flow, "screens", screens);
            SetRef(flow, "firingLine", firingLine);
            SetRef(flow, "laneMarker", laneMarker);
            SetRef(flow, "gameAudio", gameAudio);
            SetRef(flow, "frameRateGovernor", governor);
            SetRef(flow, "monetization", monetization);
            SetRef(flow, "ads", ads);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log($"[Claybreak] Scene {ScenePath}: {catalog.Worlds.Count} world(s) with their scenery, {catalog.LevelCount} levels, {stress.Length} device-gate levels, "
                + $"{catalog.Upgrades.Count} upgrades, {catalog.Units.Count} units, {catalog.Skins.Count} skins, crowd cap {CrowdCapacity}, {FlightCapacity} in flight, {ProjectilePoolSize} projectiles, "
                + $"{RunGateCourse.MaximumPairs} gate pairs, {ClaybreakLevel.MaximumCrates} crates, track to z={trackEnd:0}.");
        }

        /// <summary>Lighting (no fog: RT-6t.13); the track and the scenery come with <see cref="CreateWorldScenery"/>.</summary>
        public static Light CreateEnvironment()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            // RT-6t.13: no fog (the user's call); WorldScenery keeps it off when a world is shown.
            RenderSettings.fog = false;
            var sun = new GameObject("Sun", typeof(Light));
            Light light = sun.GetComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.None;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            // Explicit: a scene made in batchmode (art captures) does not pick its sun by itself.
            RenderSettings.sun = light;
            return light;
        }

        // RT-6t.4: the run crowd, instanced (one draw for the bodies, one for the shadows), kept on the track, in the
        // theme's shape and the skin's tint, with the camera rig on the main camera.
        private static RunCrowd CreateCrowd(GameObject cameraObject, ClaybreakTheme theme, ClaybreakCatalog catalog)
        {
            var crowdObject = new GameObject("Crowd");
            crowdObject.transform.position = Vector3.zero;
            var swarm = crowdObject.AddComponent<SwarmRenderer>();
            var so = new SerializedObject(swarm);
            // The scene starts with the default shape; the flow swaps in the player's pick (RT-6t.6b).
            so.FindProperty("unitMesh").objectReferenceValue = catalog.Shapes[0].mesh;
            so.FindProperty("unitMaterial").objectReferenceValue = Load<Material>(ToonLabBuilder.SwarmMaterialPath);
            so.FindProperty("shadowMesh").objectReferenceValue = Load<Mesh>(ToonLabBuilder.ShadowDiscPath);
            so.FindProperty("shadowMaterial").objectReferenceValue = Load<Material>(ToonLabBuilder.ShadowMaterialPath);
            so.FindProperty("capacity").intValue = CrowdCapacity;
            so.FindProperty("initialCount").intValue = 0;
            so.FindProperty("laneHalfWidth").floatValue = TrackWidth * 0.5f;
            so.FindProperty("tint").colorValue = theme.CrowdColor;
            so.ApplyModifiedPropertiesWithoutUndo();

            var rig = cameraObject.AddComponent<SwarmCameraRig>();
            SetRef(rig, "profile", Load<SwarmCameraProfile>(CrowdCameraProfilePath));
            SetRef(rig, "swarm", swarm);

            // RT-6t.13: the leader at the front, drawn instanced with the crowd's material and shadow plus its crown.
            var leaderObject = new GameObject("Leader");
            leaderObject.transform.SetParent(crowdObject.transform, false);
            CrowdLeader leader = leaderObject.AddComponent<CrowdLeader>();
            var leaderSo = new SerializedObject(leader);
            leaderSo.FindProperty("swarm").objectReferenceValue = swarm;
            leaderSo.FindProperty("bodyMaterial").objectReferenceValue = Load<Material>(ToonLabBuilder.SwarmMaterialPath);
            leaderSo.FindProperty("shadowMesh").objectReferenceValue = Load<Mesh>(ToonLabBuilder.ShadowDiscPath);
            leaderSo.FindProperty("shadowMaterial").objectReferenceValue = Load<Material>(ToonLabBuilder.ShadowMaterialPath);
            leaderSo.FindProperty("crownMesh").objectReferenceValue = Load<Mesh>(LeaderArt.CrownMeshPath);
            leaderSo.FindProperty("crownMaterial").objectReferenceValue = Load<Material>(LeaderArt.CrownMaterialPath);
            // It bounces and leans like the crowd: the swarm's own motion and animation settings, copied.
            leaderSo.CopyFromSerializedProperty(so.FindProperty("motion"));
            leaderSo.CopyFromSerializedProperty(so.FindProperty("unitAnimation"));
            leaderSo.ApplyModifiedPropertiesWithoutUndo();

            var crowd = crowdObject.AddComponent<RunCrowd>();
            var crowdSo = new SerializedObject(crowd);
            crowdSo.FindProperty("swarm").objectReferenceValue = swarm;
            crowdSo.FindProperty("cameraRig").objectReferenceValue = rig;
            crowdSo.FindProperty("leader").objectReferenceValue = leader;
            crowdSo.FindProperty("drag.MetersPerScreenWidth").floatValue = ReferenceDragMeters * TrackWidth * 0.5f / ReferenceTrackHalfWidth;
            crowdSo.FindProperty("startZ").floatValue = 0f;
            crowdSo.ApplyModifiedPropertiesWithoutUndo();
            return crowd;
        }

        /// <summary>
        /// The toon scenery (RT-6t.8) of a scene made by <see cref="CreateEnvironment"/>: the striped track up to
        /// <paramref name="trackEnd"/>, the ground with its rails, the recycled side props following
        /// <paramref name="anchor"/> (room for <paramref name="trees"/> trees and <paramref name="rocks"/> rocks) and the
        /// palette applier, wired into a <see cref="WorldScenery"/>. No world is applied yet.
        /// </summary>
        public static WorldScenery CreateWorldScenery(float trackEnd, Light sun, Transform anchor, int trees, int rocks, string groundMeshPath)
        {
            var root = new GameObject("Scenery");
            SceneryParts parts = SceneryArt.Create(root.transform, TrackWidth * 0.5f, TrackStartZ, trackEnd, anchor, trees, rocks, groundMeshPath);
            ToonPaletteApplier applier = root.AddComponent<ToonPaletteApplier>();
            SetRef(applier, "skyMaterial", Load<Material>(SkyMaterialPath));
            WorldScenery scenery = root.AddComponent<WorldScenery>();
            SetRef(scenery, "track", parts.Track);
            SetRef(scenery, "ground", parts.Ground);
            SetRef(scenery, "props", parts.Props);
            SetRef(scenery, "sun", sun);
            SetRef(scenery, "palette", applier);
            return scenery;
        }

        /// <summary>
        /// One world's scenery and look on a throwaway scene made by <see cref="CreateEnvironment"/> (captures): the props
        /// follow <paramref name="camera"/>; a capture draws them with <see cref="InstancedProps.Render"/> before rendering.
        /// </summary>
        public static WorldScenery DressWorld(string worldId, WorldLook look, float trackEnd, Light sun, Camera camera)
        {
            // The caller may have taken the look before opening this scene, which unloads assets nothing references (the
            // palette among them): the world's palette is loaded again here, by the world id.
            look.palette = WorldContent.PaletteOf(worldId);
            string error = look.palette == null ? "no palette (rebuild the scene)" : look.palette.Validate();
            if (error != null)
            {
                throw new System.InvalidOperationException("world look: " + error);
            }

            WorldScenery scenery = CreateWorldScenery(trackEnd, sun, camera.transform, look.trees, look.rocks, null);
            scenery.Apply(0, look);
            camera.clearFlags = CameraClearFlags.Skybox;
            return scenery;
        }

        // The one scenery set of the game, with room for the densest world, showing the first world (the flow applies the
        // world of each level).
        private static WorldScenery CreateScenery(ClaybreakCatalog catalog, float trackEnd, Light sun, Camera camera)
        {
            int trees = 0;
            int rocks = 0;
            foreach (WorldEntry entry in catalog.Worlds)
            {
                trees = Mathf.Max(trees, entry.world.Look.trees);
                rocks = Mathf.Max(rocks, entry.world.Look.rocks);
            }

            WorldScenery scenery = CreateWorldScenery(trackEnd, sun, camera.transform, trees, rocks, GroundMeshPath);
            WorldLook first = catalog.Worlds[0].world.Look;
            scenery.Apply(0, first);
            SetRef(scenery.GetComponent<ToonPaletteApplier>(), "palette", first.palette);
            // Apply wrote the first world's density and prop colors into the serialized layers; the track and ground colors are
            // property blocks (not saved): the flow applies the world of each level, menu included, before anything shows.
            EditorUtility.SetDirty(scenery.Props);
            return scenery;
        }

        // RT-7: the kit's rewarded and interstitial services (consent first, in the kit's MobileAdsGate) behind the
        // game's gateway; both read the game's ad units (empty until RT-9: Google's test units).
        private static ClaybreakAds CreateAds(Transform parent, ClaybreakMonetization monetization)
        {
            var adsObject = new GameObject("Ads");
            adsObject.transform.SetParent(parent, false);
            RewardedAdService rewarded = adsObject.AddComponent<RewardedAdService>();
            InterstitialAdService interstitial = adsObject.AddComponent<InterstitialAdService>();
            ClaybreakAds ads = adsObject.AddComponent<ClaybreakAds>();
            SetRef(rewarded, "units", monetization.AdUnits);
            SetRef(interstitial, "units", monetization.AdUnits);
            SetRef(ads, "monetization", monetization);
            SetRef(ads, "rewarded", rewarded);
            SetRef(ads, "interstitial", interstitial);
            return ads;
        }

        private static RunGateCourse CreateRunGates(ArtPalette palette)
        {
            var root = new GameObject("RunGates");
            RunGateCourse course = root.AddComponent<RunGateCourse>();
            int pairs = RunGateCourse.MaximumPairs;
            var roots = new UnityEngine.Object[pairs];
            var leftLabels = new UnityEngine.Object[pairs];
            var rightLabels = new UnityEngine.Object[pairs];
            var leftPanels = new UnityEngine.Object[pairs];
            var rightPanels = new UnityEngine.Object[pairs];
            // RT-6t.9, after the prototype: two translucent panels, each half the track wide less a post's gap, a big
            // outlined label on each and three white posts (edges and middle); no lintel.
            float half = TrackWidth * 0.25f;
            var panelSize = new Vector3(TrackWidth * 0.5f - 0.25f, 2.3f, 0.12f);
            for (int i = 0; i < pairs; i++)
            {
                var pair = new GameObject("Pair" + (i + 1));
                pair.transform.SetParent(root.transform, false);
                pair.transform.localPosition = new Vector3(0f, 0f, 20f * (i + 1));
                leftPanels[i] = Prim.Box(pair.transform, "Left", new Vector3(-half, 1.15f, 0f), panelSize, palette.GateGood);
                rightPanels[i] = Prim.Box(pair.transform, "Right", new Vector3(half, 1.15f, 0f), panelSize, palette.GateBad);
                leftLabels[i] = GateArt.Label(pair.transform, "LeftLabel", new Vector3(-half, 1.3f, -0.08f), GateArt.LabelEm, panelSize.x, GameFonts.GateGoodPath);
                rightLabels[i] = GateArt.Label(pair.transform, "RightLabel", new Vector3(half, 1.3f, -0.08f), GateArt.LabelEm, panelSize.x, GameFonts.GateBadPath);
                GateArt.Post(pair.transform, "PostLeft", -TrackWidth * 0.5f, 2.6f);
                GateArt.Post(pair.transform, "PostMiddle", 0f, 2.6f);
                GateArt.Post(pair.transform, "PostRight", TrackWidth * 0.5f, 2.6f);
                roots[i] = pair.transform;
            }

            int crates = ClaybreakLevel.MaximumCrates;
            var crateRoots = new UnityEngine.Object[crates];
            var crateLabels = new UnityEngine.Object[crates];
            for (int i = 0; i < crates; i++)
            {
                var crate = new GameObject("Crate" + (i + 1));
                crate.transform.SetParent(root.transform, false);
                crate.transform.localPosition = new Vector3(0f, 0f, 30f + 20f * i);
                Prim.Box(crate.transform, "Box", new Vector3(0f, 0.55f, 0f), new Vector3(1.1f, 1.1f, 1.1f), palette.Crate, new Vector3(0f, 20f, 0f));
                Prim.Box(crate.transform, "Band", new Vector3(0f, 0.55f, 0f), new Vector3(1.16f, 0.2f, 1.16f), palette.Iron, new Vector3(0f, 20f, 0f));
                Prim.Box(crate.transform, "Band2", new Vector3(0f, 0.55f, 0f), new Vector3(0.2f, 1.16f, 1.16f), palette.Iron, new Vector3(0f, 20f, 0f));
                crateLabels[i] = GateArt.Label(crate.transform, "Label", new Vector3(0f, 1.7f, 0f), 0.6f, 2f, GameFonts.GateGoodPath);
                crate.SetActive(false);
                crateRoots[i] = crate.transform;
            }

            SetArray(course, "pairRoots", roots);
            SetArray(course, "leftLabels", leftLabels);
            SetArray(course, "rightLabels", rightLabels);
            SetArray(course, "leftPanels", leftPanels);
            SetArray(course, "rightPanels", rightPanels);
            SetArray(course, "crateRoots", crateRoots);
            SetArray(course, "crateLabels", crateLabels);
            SetRef(course, "goodMaterial", palette.GateGood);
            SetRef(course, "badMaterial", palette.GateBad);
            SetRef(course, "goodLitMaterial", palette.GateGoodLit);
            SetRef(course, "badLitMaterial", palette.GateBadLit);
            SetRef(course, "offMaterial", palette.GateOff);
            SetRef(course, "goodLabelMaterial", GameFonts.Preset(GameFonts.GateGoodPath));
            SetRef(course, "badLabelMaterial", GameFonts.Preset(GameFonts.GateBadPath));
            return course;
        }

        // RT-6t.18: the run's obstacles (one instanced draw per kind) and the thrower of the units they kill: the crowd's
        // shape and material, no live units and no shadows, a ring of dying units (the flow swaps in the player's shape).
        private static RunObstacleCourse CreateObstacles(ClaybreakCatalog catalog)
        {
            var root = new GameObject("RunObstacles");
            var deathsObject = new GameObject("ObstacleDeaths");
            deathsObject.transform.SetParent(root.transform, false);
            var deaths = deathsObject.AddComponent<LooseSwarmRenderer>();
            var so = new SerializedObject(deaths);
            so.FindProperty("unitMesh").objectReferenceValue = catalog.Shapes[0].mesh;
            so.FindProperty("unitMaterial").objectReferenceValue = Load<Material>(ToonLabBuilder.SwarmMaterialPath);
            so.FindProperty("capacity").intValue = 1;
            so.FindProperty("dyingCapacity").intValue = ObstacleDyingCapacity;
            so.ApplyModifiedPropertiesWithoutUndo();

            RunObstacleCourse course = root.AddComponent<RunObstacleCourse>();
            SetArray(course, "meshes", ObstacleArt.LoadMeshes());
            SetRef(course, "material", Load<Material>(ObstacleArt.MaterialPath));
            SetRef(course, "deaths", deaths);
            return course;
        }

        // RT-6t.7: the assault's units in flight, instanced like the run crowd (same shape, material and shadows), with
        // their own ballistic death ring. The flow swaps in the player's shape.
        private static LooseSwarmRenderer CreateFlyers(Transform parent, ClaybreakCatalog catalog)
        {
            var flyersObject = new GameObject("UnitsInFlight");
            flyersObject.transform.SetParent(parent, false);
            var flyers = flyersObject.AddComponent<LooseSwarmRenderer>();
            var so = new SerializedObject(flyers);
            so.FindProperty("unitMesh").objectReferenceValue = catalog.Shapes[0].mesh;
            so.FindProperty("unitMaterial").objectReferenceValue = Load<Material>(ToonLabBuilder.SwarmMaterialPath);
            so.FindProperty("shadowMesh").objectReferenceValue = Load<Mesh>(ToonLabBuilder.ShadowDiscPath);
            so.FindProperty("shadowMaterial").objectReferenceValue = Load<Material>(ToonLabBuilder.ShadowMaterialPath);
            so.FindProperty("capacity").intValue = FlightCapacity;
            so.FindProperty("dyingCapacity").intValue = FlightDyingCapacity;
            so.ApplyModifiedPropertiesWithoutUndo();
            return flyers;
        }

        private static void CreateShotOnce(string path, Vector3 offset)
        {
            if (AssetDatabase.LoadAssetAtPath<SwarmCameraShot>(path) != null)
            {
                return;
            }

            var shot = ScriptableObject.CreateInstance<SwarmCameraShot>();
            shot.Offset = offset;
            AssetDatabase.CreateAsset(shot, path);
        }

        private static AssaultField CreateField(AssaultTower tower, AssaultWall wall, SweeperDefense sweeper,
            ShieldDefense shield, ClaybreakLevel level, ArtPalette palette, ClaybreakCatalog catalog)
        {
            var root = new GameObject("AssaultField");
            AssaultField field = root.AddComponent<AssaultField>();
            LooseSwarmRenderer flyers = CreateFlyers(root.transform, catalog);
            var visuals = new UnityEngine.Object[ClaybreakLevel.LaneCount];
            var labels = new UnityEngine.Object[ClaybreakLevel.LaneCount];
            var panels = new UnityEngine.Object[ClaybreakLevel.LaneCount];
            float laneWidth = TrackWidth / ClaybreakLevel.LaneCount;
            for (int i = 0; i < ClaybreakLevel.LaneCount; i++)
            {
                var gate = new GameObject("AssaultGate" + (i + 1));
                gate.transform.SetParent(root.transform, false);
                gate.transform.position = new Vector3(-TrackWidth * 0.5f + laneWidth * (i + 0.5f), 0f, level.AssaultGateZ);
                panels[i] = Prim.Box(gate.transform, "Panel", new Vector3(0f, 0.9f, 0f),
                    new Vector3(laneWidth * 0.92f, 1.8f, 0.12f), palette.GateGood);
                labels[i] = GateArt.Label(gate.transform, "Label", new Vector3(0f, 1f, -0.08f), GateArt.LabelEm * 0.8f, laneWidth * 0.92f, GameFonts.GateGoodPath);
                // RT-6t.9: white posts between the lanes (each gate its left one, the last also its right one).
                GateArt.Post(gate.transform, "PostLeft", -laneWidth * 0.5f, 2f);
                if (i == ClaybreakLevel.LaneCount - 1)
                {
                    GateArt.Post(gate.transform, "PostRight", laneWidth * 0.5f, 2f);
                }
                visuals[i] = gate.transform;
            }

            SetRef(field, "flyers", flyers);
            SetRef(field, "tower", tower);
            SetRef(field, "wall", wall);
            SetRef(field, "sweeper", sweeper);
            SetRef(field, "shield", shield);
            SetArray(field, "gateVisuals", visuals);
            SetArray(field, "gateLabels", labels);
            SetArray(field, "gatePanels", panels);
            SetRef(field, "goodGateMaterial", palette.GateGood);
            SetRef(field, "badGateMaterial", palette.GateBad);
            SetRef(field, "goodLabelMaterial", GameFonts.Preset(GameFonts.GateGoodPath));
            SetRef(field, "badLabelMaterial", GameFonts.Preset(GameFonts.GateBadPath));
            return field;
        }

        private static AssaultTower CreateTower(ClaybreakLevel level, ArtPalette palette)
        {
            var root = new GameObject("Tower");
            root.transform.position = new Vector3(0f, 0f, level.TowerZ);
            int kinds = Enum.GetValues(typeof(TowerKind)).Length;

            var bodies = new UnityEngine.Object[kinds];
            var blocks = new UnityEngine.Object[kinds * AssaultTower.BlocksPerBody];
            var muzzles = new UnityEngine.Object[kinds];
            var heights = new float[kinds];
            for (int k = 0; k < kinds; k++)
            {
                TowerParts parts = TowerArt.Build(TowerArt.StyleFor((TowerKind)k), root.transform, palette);
                parts.Body.name = ((TowerKind)k) + "Body";
                bodies[k] = parts.Body;
                muzzles[k] = parts.Muzzle;
                heights[k] = parts.BarHeight;
                for (int b = 0; b < AssaultTower.BlocksPerBody; b++)
                {
                    blocks[k * AssaultTower.BlocksPerBody + b] = parts.Tiers[b];
                }

                parts.Body.gameObject.SetActive(k == (int)level.Tower);
            }

            var bar = new GameObject("HealthBar").transform;
            bar.SetParent(root.transform, false);
            bar.localPosition = new Vector3(0f, heights[0], 1f);
            bar.localScale = new Vector3(6f, 0.45f, 0.2f);
            Prim.Box(bar, "Back", Vector3.zero, Vector3.one, palette.StoneDark);
            Transform fill = Prim.Box(bar, "Fill", new Vector3(0f, 0f, -0.2f), new Vector3(1f, 0.8f, 0.8f), palette.Health).transform;
            TextMesh label = CreateLabel("HealthLabel", root.transform, new Vector3(0f, heights[0] + 1.2f, 1f), 0.45f);

            AssaultTower tower = root.AddComponent<AssaultTower>();
            SetArray(tower, "bodies", bodies);
            SetArray(tower, "floorBlocks", blocks);
            SetArray(tower, "muzzles", muzzles);
            var so = new SerializedObject(tower);
            SerializedProperty heightArray = so.FindProperty("barHeights");
            heightArray.arraySize = kinds;
            for (int k = 0; k < kinds; k++)
            {
                heightArray.GetArrayElementAtIndex(k).floatValue = heights[k];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            SetRef(tower, "healthBar", bar);
            SetRef(tower, "healthFill", fill);
            SetRef(tower, "healthLabel", label);
            return tower;
        }

        private static CannonDefense CreateCannon(ArtPalette palette)
        {
            var cannonObject = new GameObject("CannonDefense");
            CannonDefense cannon = cannonObject.AddComponent<CannonDefense>();
            var projectiles = new UnityEngine.Object[ProjectilePoolSize];
            var shadows = new UnityEngine.Object[ProjectilePoolSize];
            for (int i = 0; i < ProjectilePoolSize; i++)
            {
                GameObject stone = Prim.Ball(cannonObject.transform, "Stone" + (i + 1), Vector3.zero, Vector3.one * 1.1f, palette.Iron).gameObject;
                stone.SetActive(false);
                projectiles[i] = stone.transform;
                GameObject shadow = Prim.Cyl(cannonObject.transform, "Shadow" + (i + 1), Vector3.zero, new Vector3(2f, 0.02f, 2f),
                    palette.Shadow).gameObject;
                shadow.SetActive(false);
                shadows[i] = shadow.transform;
            }

            SetArray(cannon, "projectiles", projectiles);
            SetArray(cannon, "shadows", shadows);
            return cannon;
        }

        private static AssaultWall CreateWall(ClaybreakLevel level, ArtPalette palette)
        {
            var root = new GameObject("Wall");
            float laneWidth = TrackWidth / ClaybreakLevel.LaneCount;
            root.transform.position = new Vector3(-TrackWidth * 0.5f + laneWidth * (level.WallLane + 0.5f), 0f, level.WallZ);
            Transform body = Prim.Box(root.transform, "Body", new Vector3(0f, 1.3f, 0f),
                new Vector3(laneWidth * 0.95f, 2.6f, 0.9f), palette.Wall).transform;
            for (int i = 0; i < 3; i++)
            {
                Prim.Box(body, "Merlon", new Vector3(-0.33f + i * 0.33f, 0.58f, 0f), new Vector3(0.2f, 0.16f, 1.1f), palette.ClayDark);
                Prim.Box(body, "Course", new Vector3(0f, -0.25f + i * 0.25f, -0.52f), new Vector3(1.02f, 0.03f, 0.05f), palette.ClayDark);
            }

            TextMesh label = CreateLabel("HealthLabel", root.transform, new Vector3(0f, 3.4f, 0f), 0.3f);
            AssaultWall wall = root.AddComponent<AssaultWall>();
            SetRef(wall, "body", body);
            SetRef(wall, "healthLabel", label);
            return wall;
        }

        // The shield (GDD 4.3, fourth defense): a pylon with a glowing core on one lane, a ring of plates
        // around it, and a translucent dome placed over the tower's front at level time.
        private static ShieldDefense CreateShield(ArtPalette palette)
        {
            var root = new GameObject("Shield");
            var generator = new GameObject("Generator").transform;
            generator.SetParent(root.transform, false);
            Prim.Cyl(generator, "Base", new Vector3(0f, 0.25f, 0f), new Vector3(1.6f, 0.25f, 1.6f), palette.IronWarm);
            Prim.Cyl(generator, "Pylon", new Vector3(0f, 1.3f, 0f), new Vector3(0.7f, 1.05f, 0.7f), palette.Iron);
            Prim.Ball(generator, "Core", new Vector3(0f, 2.55f, 0f), new Vector3(0.9f, 0.9f, 0.9f), palette.ShieldCore);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f + 45f;
                float r = Mathf.Deg2Rad * a;
                Prim.Box(generator, "Fin", new Vector3(Mathf.Cos(r) * 0.6f, 1.2f, Mathf.Sin(r) * 0.6f), new Vector3(0.12f, 1.8f, 0.5f),
                    palette.IronWarm, new Vector3(0f, -a, 0f));
            }

            TextMesh label = CreateLabel("HealthLabel", root.transform, new Vector3(0f, 3.6f, 0f), 0.3f);
            var dome = Prim.Ball(null, "ShieldDome", new Vector3(0f, 4f, 60f), new Vector3(TrackWidth + 2.5f, 9f, 4f),
                palette.ShieldDome).transform;
            dome.SetParent(root.transform, true);
            ShieldDefense shield = root.AddComponent<ShieldDefense>();
            SetRef(shield, "generator", generator);
            SetRef(shield, "healthLabel", label);
            SetRef(shield, "dome", dome);
            root.SetActive(true);
            generator.gameObject.SetActive(false);
            label.gameObject.SetActive(false);
            dome.gameObject.SetActive(false);
            return shield;
        }

        private static SweeperDefense CreateSweeper(ArtPalette palette)
        {
            var root = new GameObject("Sweeper");
            root.transform.position = new Vector3(0f, 0f, 60f);
            Prim.Box(root.transform, "Rail", new Vector3(0f, 0.04f, 0f), new Vector3(TrackWidth + 0.6f, 0.08f, 0.35f), palette.Iron);
            Prim.Box(root.transform, "DangerStrip", new Vector3(0f, 0.02f, 0f), new Vector3(TrackWidth, 0.02f, 1.4f), palette.Danger);
            var arm = new GameObject("Arm").transform;
            arm.SetParent(root.transform, false);
            Prim.Cyl(arm, "Post", new Vector3(0f, 0.9f, 0f), new Vector3(0.6f, 0.9f, 0.6f), palette.IronWarm);
            Prim.Cyl(arm, "Cap", new Vector3(0f, 1.85f, 0f), new Vector3(0.75f, 0.08f, 0.75f), palette.Glow);
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f;
                float r = Mathf.Deg2Rad * a;
                Prim.Box(arm, "Spike", new Vector3(Mathf.Cos(r) * 0.55f, 0.5f + (i % 2) * 0.7f, Mathf.Sin(r) * 0.55f),
                    new Vector3(0.7f, 0.14f, 0.14f), palette.Bone, new Vector3(0f, -a, 0f));
            }

            SweeperDefense sweeper = root.AddComponent<SweeperDefense>();
            SetRef(sweeper, "arm", arm);
            return sweeper;
        }

        // ---------- UI ----------

        private static Transform CreateCanvas()
        {
            var canvasObject = new GameObject("Ui", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            // RT-6t.16: the canvas matches the width, so a portrait screen is always 1080 units wide and only grows taller
            // (16:9 = 1920, 20:9 = 2400, 21:9 = 2520); top and bottom anchored content spreads, nothing is cut at the sides.
            scaler.matchWidthOrHeight = CanvasMatch;
            return canvasObject.transform;
        }

        private static ClaybreakHud CreateHud(Transform canvas, Sprite pill)
        {
            // RT-8: the HUD lives inside the phone's safe area (camera cut-out, rounded corners, gesture bar).
            RectTransform root = Stretch("Hud", canvas);
            root.gameObject.AddComponent<SafeAreaFitter>();
            Text level = Label("Level", root, new Vector2(0.5f, 1f), new Vector2(0f, -150f), 54, TextAnchor.MiddleCenter);
            // RT-6t.9: the army counter is TextMeshPro with an outline and pops on change; it scales from its middle-left.
            TextMeshProUGUI army = UiLabel("Army", root, new Vector2(0f, 1f), new Vector2(60f, -270f), 66f, GameFonts.CounterPath);
            army.alignment = TextAlignmentOptions.Left;
            army.rectTransform.pivot = new Vector2(0f, 0.5f);
            Text specials = Label("Specials", root, new Vector2(0f, 1f), new Vector2(60f, -315f), 46, TextAnchor.UpperLeft);
            specials.color = new Color(1f, 0.55f, 0.45f);
            Image coin = Img("CoinIcon", root, pill, new Vector2(1f, 1f), new Vector2(-215f, -150f), new Vector2(46f, 46f), Gold);
            coin.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            Text coins = Label("Coins", root, new Vector2(1f, 1f), new Vector2(-60f, -150f), 52, TextAnchor.MiddleRight);
            coins.rectTransform.sizeDelta = new Vector2(170f, 90f);
            coins.rectTransform.pivot = new Vector2(1f, 0.5f);

            Image barBack = Img("TowerBar", root, pill, new Vector2(1f, 1f), new Vector2(-60f, -262f), new Vector2(520f, 64f),
                new Color(0.1f, 0.08f, 0.08f, 0.85f));
            barBack.rectTransform.pivot = new Vector2(1f, 0.5f);
            ClayUi.MakePill(barBack, pill, 64f);
            Image fill = Img("Fill", barBack.rectTransform, pill, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 46f),
                new Color(0.86f, 0.2f, 0.16f));
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            Text towerText = Label("TowerHealth", barBack.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, 44, TextAnchor.MiddleCenter);
            barBack.gameObject.SetActive(false);

            Text hint = Label("Hint", root, new Vector2(0.5f, 0f), new Vector2(0f, 260f), 56, TextAnchor.MiddleCenter);
            Text banner = Label("Banner", root, new Vector2(0.5f, 0.5f), new Vector2(0f, 330f), 70, TextAnchor.MiddleCenter);
            banner.color = Gold;
            banner.gameObject.SetActive(false);

            FloatingNumbers floating = CreateFloatingNumbers(root);

            ClaybreakHud hud = root.gameObject.AddComponent<ClaybreakHud>();
            SetRef(hud, "root", root.gameObject);
            SetRef(hud, "levelText", level);
            SetRef(hud, "armyText", army);
            SetRef(hud, "specialText", specials);
            SetRef(hud, "coinsText", coins);
            SetRef(hud, "towerBar", barBack.gameObject);
            SetRef(hud, "towerFill", fill);
            SetRef(hud, "towerText", towerText);
            SetRef(hud, "hintText", hint);
            SetRef(hud, "bannerText", banner);
            SetRef(hud, "floatingNumbers", floating);
            return hud;
        }

        // RT-6t.9: four preallocated labels (the prototype's floatText: 38% from the top, 40 px Baloo 2 ExtraBold,
        // white outline), reused in turn; colors are set per gate.
        public const int FloatingNumberSlots = 4;

        private static FloatingNumbers CreateFloatingNumbers(Transform hudRoot)
        {
            RectTransform root = Stretch("FloatingNumbers", hudRoot);
            var slots = new UnityEngine.Object[FloatingNumberSlots];
            for (int i = 0; i < slots.Length; i++)
            {
                TextMeshProUGUI label = UiLabel("Number" + (i + 1), root, new Vector2(0.5f, 0.62f), Vector2.zero, 96f, GameFonts.FloatPath);
                label.gameObject.SetActive(false);
                slots[i] = label;
            }

            FloatingNumbers floating = root.gameObject.AddComponent<FloatingNumbers>();
            SetArray(floating, "slots", slots);
            return floating;
        }

        private static TextMeshProUGUI UiLabel(string name, Transform parent, Vector2 anchor, Vector2 position, float size, string preset)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(600f, size * 1.4f);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = GameFonts.Sdf();
            text.fontSharedMaterial = GameFonts.Preset(preset);
            text.fontSize = size;
            text.enableAutoSizing = false;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.color = Color.white;
            text.raycastTarget = false;
            text.text = "0";
            return text;
        }

        // RT-6t.17 (plan 3.1): the toon pastel screens. Vivid palette buttons with a dark slab and no gloss, Baloo 2
        // TextMeshPro labels with the ink outline, light cards with ink text, a lighter veil (the level shows behind).
        // Buttons keep their RT-8 paths (/Ui/Screens/<Screen>/Safe/...): the PlayMode tests press them by path.
        private static ClaybreakScreens CreateScreens(Transform canvas, Sprite pill, Sprite star, Sprite crown, Sprite fade,
            ClaybreakCatalog catalog)
        {
            var screensObject = new GameObject("Screens", typeof(RectTransform));
            RectTransform screensRect = screensObject.GetComponent<RectTransform>();
            screensRect.SetParent(canvas, false);
            screensRect.anchorMin = Vector2.zero;
            screensRect.anchorMax = Vector2.one;
            screensRect.sizeDelta = Vector2.zero;
            ClaybreakScreens screens = screensObject.AddComponent<ClaybreakScreens>();

            // Home, top to bottom: settings and coins, the name, the world card (world, level, stars), the compact level
            // meter; from the bottom (thumb reach): SHOP and WORLDS side by side, the big PLAY, and a small seal over it (the
            // daily gift when due). The elites line moved to the shop; the shape picker left for the shop's SHAPES tab (RT-6t.20).
            RectTransform home = MenuPage("Home", screensRect, 0.28f, out RectTransform homePage);
            (Button settingsButton, TMP_Text _) = PillButton("SettingsButton", home, pill, new Vector2(0f, 1f), new Vector2(170f, -110f),
                new Vector2(290f, 104f), UiColors.Lilac, "SETTINGS", 42);
            TMP_Text homeCoins = Coins(home, pill);
            // The game's name (D29), with the call to action under it: a stampede storming the tower (RT-6t.21 kept it).
            TMP_Text title = Caption("Title", home, new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(1000f, 180f), 150,
                UiColors.Primary, false);
            title.text = ProjectConfiguration.GameName.ToUpperInvariant();
            TMP_Text tagline = Caption("Tagline", home, new Vector2(0.5f, 1f), new Vector2(0f, -410f), new Vector2(900f, 70f), 44,
                Color.white, false);
            tagline.text = "STORM THE TOWER";
            Image worldCard = Card("WorldCard", home, pill, new Vector2(0.5f, 1f), new Vector2(0f, -570f), new Vector2(820f, 190f));
            TMP_Text world = Caption("World", worldCard.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(760f, 70f),
                46, UiColors.Ink, true);
            TMP_Text worldStars = Caption("Stars", worldCard.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 55f), new Vector2(760f, 70f),
                48, UiColors.Ink, true);
            (UnityEngine.Object[] dots, TMP_Text levelMeterText) = LevelMeter(home, pill);

            (Button play, TMP_Text playLabel) = PillButton("Play", home, pill, new Vector2(0.5f, 0f), new Vector2(0f, 420f),
                new Vector2(700f, 210f), UiColors.Primary, "PLAY", 100);
            (Button shop, TMP_Text _) = PillButton("ShopButton", home, pill, new Vector2(0.5f, 0f), new Vector2(-235f, 210f),
                new Vector2(430f, 140f), UiColors.Mint, "SHOP", 60);
            StretchAcross(shop, 0.06f, 0.49f);
            Image badge = Img("Badge", shop.transform, pill, new Vector2(1f, 1f), new Vector2(-26f, -18f), new Vector2(54f, 54f), UiColors.Pink);
            ClayUi.AddSlab(badge, 4f);
            (Button worldsButton, TMP_Text _) = PillButton("WorldsButton", home, pill, new Vector2(0.5f, 0f), new Vector2(235f, 210f),
                new Vector2(430f, 140f), UiColors.Sky, "WORLDS", 60);
            StretchAcross(worldsButton, 0.51f, 0.94f);
            (Button daily, TMP_Text dailyLabel) = PillButton("DailyGift", home, pill, new Vector2(0.5f, 0f), new Vector2(250f, 605f),
                new Vector2(360f, 100f), UiColors.Gold, "GIFT", 42, true);
            // A seal, not a full button: tilted a little over PLAY's corner.
            daily.transform.localRotation = Quaternion.Euler(0f, 0f, -5f);
            // Scene default shows the usual case (the flow sets it when the home screen opens): a gift due on a first launch.
            dailyLabel.text = ClaybreakScreens.DailyLine(MonetizationContent.DailyRewards[0]);

            // Settings (RT-8): the kit's sound, music and vibration switches, the privacy policy link, the privacy
            // options (only where UMP requires them) and the credits of the game's own audio and font, on one light card.
            RectTransform settingsRoot = MenuPage("Settings", screensRect, 0.5f, out RectTransform settingsPage);
            Heading(settingsRoot, "SETTINGS");
            Card("Card", settingsRoot, pill, new Vector2(0.5f, 1f), new Vector2(0f, -690f), new Vector2(820f, 820f));
            (Button sound, TMP_Text soundLabel) = PillButton("Sound", settingsRoot, pill, new Vector2(0.5f, 1f), new Vector2(0f, -400f),
                new Vector2(640f, 120f), UiColors.Sky, "SOUND ON", 52);
            (Button music, TMP_Text musicLabel) = PillButton("Music", settingsRoot, pill, new Vector2(0.5f, 1f), new Vector2(0f, -550f),
                new Vector2(640f, 120f), UiColors.Sky, "MUSIC ON", 52);
            (Button vibration, TMP_Text vibrationLabel) = PillButton("Vibration", settingsRoot, pill, new Vector2(0.5f, 1f),
                new Vector2(0f, -700f), new Vector2(640f, 120f), UiColors.Sky, "VIBRATION ON", 52);
            (Button policy, TMP_Text _) = PillButton("PrivacyPolicy", settingsRoot, pill, new Vector2(0.5f, 1f), new Vector2(0f, -860f),
                new Vector2(640f, 104f), UiColors.Lilac, "PRIVACY POLICY", 42);
            (Button privacy, TMP_Text _) = PillButton("PrivacyOptions", settingsRoot, pill, new Vector2(0.5f, 1f), new Vector2(0f, -990f),
                new Vector2(640f, 104f), UiColors.Lilac, "PRIVACY OPTIONS", 42);
            privacy.gameObject.SetActive(false);
            TMP_Text settingsNote = Caption("Note", settingsRoot, new Vector2(0.5f, 0f), new Vector2(0f, 420f), new Vector2(940f, 110f), 36,
                Color.white, false);
            settingsNote.textWrappingMode = TextWrappingModes.Normal;
            TMP_Text credits = Caption("Credits", settingsRoot, new Vector2(0.5f, 0f), new Vector2(0f, 280f), new Vector2(960f, 110f), 30,
                UiColors.WithAlpha(Color.white, 0.85f), false);
            credits.textWrappingMode = TextWrappingModes.Normal;
            credits.text = ProjectConfiguration.GameName.ToUpperInvariant() + "  ·  JUNGLE DEGREE\nMUSIC AND SOUND: JUNGLE DEGREE  ·  FONT: BALOO 2 (SIL OFL 1.1)";
            Button settingsBack = BackButton(settingsRoot, pill);

            // Shop: four tabs (RT-6t.20 added SHAPES) in equal parts of the width over one list (rows = the longest catalog list), coins only (D6:
            // no real money). Over the list, what the army takes into a run; under it, a fade that says the list goes on.
            RectTransform shopRoot = MenuPage("Shop", screensRect, 0.5f, out RectTransform shopPage);
            TMP_Text shopCoins = Coins(shopRoot, pill);
            Heading(shopRoot, "SHOP");
            string[] tabNames = { "UPGRADES", "ELITES", "SKINS", "SHAPES" };
            var tabs = new UnityEngine.Object[tabNames.Length];
            for (int i = 0; i < tabNames.Length; i++)
            {
                (Button tab, TMP_Text _) = PillButton("Tab" + tabNames[i], shopRoot, pill, new Vector2(0.5f, 1f),
                    new Vector2(0f, -300f), new Vector2(240f, 110f), UiColors.Lilac, tabNames[i], 40);
                // RT-6t.16: equal parts of the safe width (4% margins since RT-6t.20, 1.5% gaps), never a fixed width past the screen.
                const float margin = 0.04f;
                const float gap = 0.015f;
                float part = (1f - 2f * margin - (tabNames.Length - 1) * gap) / tabNames.Length;
                float left = margin + i * (part + gap);
                StretchAcross(tab, left, left + part);
                tabs[i] = tab;
            }

            Image loadoutCard = Card("LoadoutCard", shopRoot, pill, new Vector2(0.5f, 1f), new Vector2(0f, -418f), new Vector2(900f, 80f));
            TMP_Text loadout = Caption("Loadout", loadoutCard.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(840f, 64f), 36,
                UiColors.Ink, true);
            int shopCount = 0;
            for (int i = 0; i < ClaybreakScreens.ShopTabCount; i++)
            {
                shopCount = Mathf.Max(shopCount, ClaybreakScreens.ShopCount(catalog, (ShopTab)i));
            }

            (RectTransform shopContent, UnityEngine.Object[] shopRows, GameObject shopFade) =
                List(shopRoot, pill, fade, "ShopList", -480f, 240f, shopCount);
            Button shopBack = BackButton(shopRoot, pill);

            // Worlds: one row per catalog world, any number (RT-6 adds them as data).
            RectTransform worldsRoot = MenuPage("Worlds", screensRect, 0.5f, out RectTransform worldsPage);
            Heading(worldsRoot, "WORLDS");
            TMP_Text worldsStars = Caption("Stars", worldsRoot, new Vector2(0.5f, 1f), new Vector2(0f, -290f), new Vector2(900f, 80f), 56,
                Color.white, false);
            (RectTransform worldsContent, UnityEngine.Object[] worldRows, GameObject worldsFade) =
                List(worldsRoot, pill, fade, "WorldList", -370f, 330f, catalog.Worlds.Count);
            TMP_Text footer = Caption("Footer", worldsRoot, new Vector2(0.5f, 0f), new Vector2(0f, 260f), new Vector2(940f, 90f), 38,
                Color.white, false);
            footer.textWrappingMode = TextWrappingModes.Normal;
            Button worldsBack = BackButton(worldsRoot, pill);

            // Result: veil, one light card, one big button (NEXT / AGAIN) and a small HOME; the rewarded offer under the card.
            RectTransform result = MenuPage("Result", screensRect, 0.5f, out RectTransform resultPage);
            Image card = Card("Card", result, pill, new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(900f, 1060f));
            RectTransform cardRect = card.rectTransform;
            TMP_Text resultTitle = Caption("Title", cardRect, new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(820f, 140f), 100,
                UiColors.Coral, false);
            var stars = new UnityEngine.Object[3];
            for (int i = 0; i < 3; i++)
            {
                Image starImage = Img("Star" + (i + 1), cardRect, star, new Vector2(0.5f, 1f), new Vector2((i - 1) * 190f, -290f - (i == 1 ? 20f : 0f)),
                    new Vector2(i == 1 ? 190f : 160f, i == 1 ? 190f : 160f), UiColors.Gold);
                stars[i] = starImage;
            }

            TMP_Text resultDetail = Caption("Detail", cardRect, new Vector2(0.5f, 1f), new Vector2(0f, -455f), new Vector2(820f, 80f), 50,
                UiColors.Ink, true);
            TMP_Text resultReward = Caption("Reward", cardRect, new Vector2(0.5f, 1f), new Vector2(0f, -545f), new Vector2(820f, 90f), 64,
                UiColors.Ink, true);
            TMP_Text resultUnlock = Caption("Unlock", cardRect, new Vector2(0.5f, 1f), new Vector2(0f, -675f), new Vector2(820f, 140f), 44,
                UiColors.Coral, true);
            resultUnlock.textWrappingMode = TextWrappingModes.Normal;
            (Button primary, TMP_Text primaryLabel) = PillButton("Primary", cardRect, pill, new Vector2(0.5f, 0f), new Vector2(0f, 240f),
                new Vector2(600f, 170f), UiColors.Primary, "NEXT", 84);
            (Button homeButton, TMP_Text _) = PillButton("Home", cardRect, pill, new Vector2(0.5f, 0f), new Vector2(0f, 82f),
                new Vector2(340f, 96f), UiColors.Cream, "HOME", 44, true);
            (Button resultAd, TMP_Text resultAdLabel) = PillButton("RewardedOffer", result, pill, new Vector2(0.5f, 0f), new Vector2(0f, 330f),
                new Vector2(760f, 130f), UiColors.Mint, "WATCH AD", 52);

            // Continue offer: one light card, what the ad gives, and NO THANKS as clear as the offer (GDD 7.1).
            RectTransform continueRoot = MenuPage("Continue", screensRect, 0.5f, out RectTransform continuePage);
            Image continueCard = Card("Card", continueRoot, pill, new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(900f, 760f));
            RectTransform continueRect = continueCard.rectTransform;
            TMP_Text continueTitle = Caption("Title", continueRect, new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(820f, 130f), 90,
                UiColors.Coral, false);
            continueTitle.text = "ARMY LOST";
            TMP_Text continueText = Caption("Offer", continueRect, new Vector2(0.5f, 1f), new Vector2(0f, -290f), new Vector2(820f, 160f), 48,
                UiColors.Ink, true);
            continueText.textWrappingMode = TextWrappingModes.Normal;
            (Button continueAd, TMP_Text continueAdLabel) = PillButton("WatchAd", continueRect, pill, new Vector2(0.5f, 0f), new Vector2(0f, 240f),
                new Vector2(640f, 150f), UiColors.Mint, "WATCH AD", 60);
            (Button continueDecline, TMP_Text _) = PillButton("NoThanks", continueRect, pill, new Vector2(0.5f, 0f), new Vector2(0f, 90f),
                new Vector2(480f, 110f), UiColors.Cream, "NO THANKS", 48, true);

            foreach (RectTransform screen in new[] { homePage, settingsPage, shopPage, worldsPage, resultPage, continuePage })
            {
                screen.gameObject.SetActive(false);
            }

            SetRef(screens, "home", homePage.gameObject);
            SetRef(screens, "settingsButton", settingsButton);
            SetRef(screens, "settings", settingsPage.gameObject);
            SetRef(screens, "policyButton", policy);
            SetRef(screens, "settingsNote", settingsNote);
            SetRef(screens, "settingsBack", settingsBack);
            SetRef(screens, "homeCoins", homeCoins);
            SetRef(screens, "worldText", world);
            SetRef(screens, "starsText", worldStars);
            SetRef(screens, "loadoutText", loadout);
            SetArray(screens, "levelDots", dots);
            SetRef(screens, "levelMeterText", levelMeterText);
            SetRef(screens, "dotIcon", pill);
            SetRef(screens, "bossIcon", crown);
            SetRef(screens, "playButton", play);
            SetRef(screens, "playLabel", playLabel);
            SetRef(screens, "shopButton", shop);
            SetRef(screens, "shopBadge", badge.gameObject);
            SetRef(screens, "worldsButton", worldsButton);
            SetRef(screens, "soundButton", sound);
            SetRef(screens, "soundLabel", soundLabel);
            SetRef(screens, "musicButton", music);
            SetRef(screens, "musicLabel", musicLabel);
            SetRef(screens, "vibrationButton", vibration);
            SetRef(screens, "vibrationLabel", vibrationLabel);
            SetRef(screens, "dailyButton", daily);
            SetRef(screens, "dailyLabel", dailyLabel);
            SetRef(screens, "privacyButton", privacy);
            SetRef(screens, "shop", shopPage.gameObject);
            SetRef(screens, "shopCoins", shopCoins);
            SetArray(screens, "shopTabs", tabs);
            SetRef(screens, "shopContent", shopContent);
            SetRef(screens, "shopFade", shopFade);
            SetArray(screens, "shopRows", shopRows);
            SetRef(screens, "shopBack", shopBack);
            SetRef(screens, "worlds", worldsPage.gameObject);
            SetRef(screens, "worldsStars", worldsStars);
            SetRef(screens, "worldsContent", worldsContent);
            SetRef(screens, "worldsFade", worldsFade);
            SetArray(screens, "worldRows", worldRows);
            SetRef(screens, "worldsFooter", footer);
            SetRef(screens, "worldsBack", worldsBack);
            SetRef(screens, "result", resultPage.gameObject);
            SetRef(screens, "resultTitle", resultTitle);
            SetArray(screens, "stars", stars);
            SetRef(screens, "resultDetail", resultDetail);
            SetRef(screens, "resultReward", resultReward);
            SetRef(screens, "resultUnlock", resultUnlock);
            SetRef(screens, "primaryButton", primary);
            SetRef(screens, "primaryLabel", primaryLabel);
            SetRef(screens, "homeButton", homeButton);
            SetRef(screens, "resultAdButton", resultAd);
            SetRef(screens, "resultAdLabel", resultAdLabel);
            SetRef(screens, "continueOffer", continuePage.gameObject);
            SetRef(screens, "continueText", continueText);
            SetRef(screens, "continueAdButton", continueAd);
            SetRef(screens, "continueAdLabel", continueAdLabel);
            SetRef(screens, "continueDecline", continueDecline);
            Set(screens, "rowStep", RowStep);
            return screens;
        }

        // A full-screen menu page over a light ink veil (the page, shown and hidden as a whole); returns its content
        // area, fitted to the phone's safe area (RT-8).
        private static RectTransform MenuPage(string name, RectTransform parent, float veilAlpha, out RectTransform page)
        {
            page = Stretch(name, parent);
            Image veil = page.gameObject.AddComponent<Image>();
            veil.color = UiColors.WithAlpha(UiColors.Veil, veilAlpha);
            RectTransform content = Stretch("Safe", page);
            content.gameObject.AddComponent<SafeAreaFitter>();
            return content;
        }

        // The coins capsule, top right: a light pill with the coin icon and the amount.
        private static TMP_Text Coins(RectTransform screen, Sprite pill)
        {
            Image capsule = Card("CoinsCapsule", screen, pill, new Vector2(1f, 1f), new Vector2(-170f, -110f), new Vector2(280f, 96f));
            TMP_Text coins = Caption("Coins", capsule.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240f, 80f), 46,
                UiColors.Ink, true);
            coins.text = UiIcons.Coins(0);
            return coins;
        }

        private static void Heading(RectTransform screen, string caption)
        {
            TMP_Text heading = Caption("Heading", screen, new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(560f, 140f), 100,
                UiColors.Primary, false);
            heading.text = caption;
        }

        // BACK at the bottom (thumb reach), a light button with ink text (cancel / back, plan 3.1).
        private static Button BackButton(RectTransform screen, Sprite pill)
        {
            (Button back, TMP_Text _) = PillButton("Back", screen, pill, new Vector2(0.5f, 0f), new Vector2(0f, 120f),
                new Vector2(420f, 120f), UiColors.Cream, "BACK", 50, true);
            return back;
        }

        // A light card: cream face, pink border, ink slab.
        private static Image Card(string name, Transform parent, Sprite pill, Vector2 anchor, Vector2 position, Vector2 size)
        {
            Image card = Img(name, parent, pill, anchor, position, size, UiColors.Cream);
            ClayUi.MakePill(card, pill, Mathf.Min(140f, size.y));
            // The slab first: ClayUi.AddSlab reuses the first Shadow it finds, and Outline is one.
            ClayUi.AddSlab(card, 8f);
            card.GetComponent<Shadow>().effectColor = UiColors.WithAlpha(UiColors.Ink, 0.35f);
            Outline border = card.gameObject.AddComponent<Outline>();
            border.effectColor = UiColors.CardBorder;
            border.effectDistance = new Vector2(5f, -5f);
            return card;
        }

        // A vertical scroll list between <paramref name="top"/> (from the top) and <paramref name="bottom"/>
        // (from the bottom), with <paramref name="rows"/> preallocated rows at RowStep apart and a fade at its foot (the
        // screens show it only when the rows overflow the list).
        private static (RectTransform, UnityEngine.Object[], GameObject) List(RectTransform screen, Sprite pill, Sprite fade, string name, float top,
            float bottom, int rows)
        {
            RectTransform viewport = Stretch(name, screen);
            viewport.offsetMin = new Vector2(60f, bottom);
            viewport.offsetMax = new Vector2(-60f, top);
            Image hit = viewport.gameObject.AddComponent<Image>();
            hit.color = new Color(0f, 0f, 0f, 0.001f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var contentObject = new GameObject("Content", typeof(RectTransform));
            RectTransform content = contentObject.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = new Vector2(0f, rows * RowStep);
            ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            var created = new UnityEngine.Object[rows];
            for (int i = 0; i < rows; i++)
            {
                created[i] = ListRow(content, pill, i);
            }

            // Over the rows, inside the mask, never catching a tap.
            Image foot = Img("Fade", viewport, fade, new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero, UiColors.WithAlpha(UiColors.Veil, 0.7f));
            RectTransform footRect = foot.rectTransform;
            footRect.anchorMin = new Vector2(0f, 0f);
            footRect.anchorMax = new Vector2(1f, 0f);
            footRect.pivot = new Vector2(0.5f, 0f);
            footRect.sizeDelta = new Vector2(0f, 80f);
            return (content, created, foot.gameObject);
        }

        private static ClaybreakListRow ListRow(RectTransform content, Sprite pill, int index)
        {
            const float height = 230f;
            Image card = Card("Row" + (index + 1), content, pill, new Vector2(0.5f, 1f),
                new Vector2(0f, -index * RowStep - height * 0.5f), new Vector2(940f, height));
            RectTransform rect = card.rectTransform;
            // RT-6t.16: the row is as wide as the list (no fixed width cut by the list's mask on a narrow screen).
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(-10f, height);
            // Title and detail between the left edge and the swatch, whatever the row's width; the button always inside.
            TMP_Text title = Caption("Title", rect, new Vector2(0f, 1f), Vector2.zero, Vector2.zero, 44, UiColors.Ink, true);
            title.alignment = TextAlignmentOptions.Left;
            Span(title.rectTransform, 44f, -340f, -18f, 64f);
            TMP_Text detail = Caption("Detail", rect, new Vector2(0f, 1f), Vector2.zero, Vector2.zero, 33, UiColors.WithAlpha(UiColors.Ink, 0.78f), true);
            detail.alignment = TextAlignmentOptions.TopLeft;
            detail.textWrappingMode = TextWrappingModes.Normal;
            Span(detail.rectTransform, 44f, -340f, -84f, 132f);
            Image swatch = Img("Swatch", rect, pill, new Vector2(1f, 1f), new Vector2(-305f, -52f), new Vector2(56f, 56f), Color.white);
            // RT-6t.20: a shape's figure in the same corner, larger (two layers: the tinted body, the details over it).
            Image figureBody = Img("FigureBody", rect, null, new Vector2(1f, 0.5f), new Vector2(-360f, 0f), new Vector2(160f, 160f), Color.white);
            Image figureDetail = Img("FigureDetail", rect, null, new Vector2(1f, 0.5f), new Vector2(-360f, 0f), new Vector2(160f, 160f), Color.white);
            figureBody.preserveAspect = true;
            figureDetail.preserveAspect = true;
            figureBody.raycastTarget = false;
            figureDetail.raycastTarget = false;
            figureBody.gameObject.SetActive(false);
            figureDetail.gameObject.SetActive(false);
            (Button button, TMP_Text label) = PillButton("Action", rect, pill, new Vector2(1f, 0.5f), new Vector2(-145f, 0f),
                new Vector2(240f, 120f), UiColors.Primary, "BUY", 42);
            ClaybreakListRow row = card.gameObject.AddComponent<ClaybreakListRow>();
            SetRef(row, "title", title);
            SetRef(row, "detail", detail);
            SetRef(row, "swatch", swatch);
            SetRef(row, "figureBody", figureBody);
            SetRef(row, "figureDetail", figureDetail);
            SetRef(row, "button", button);
            SetRef(row, "buttonLabel", label);
            return row;
        }

        // A label spanning its parent's width from <paramref name="left"/> to <paramref name="right"/> (negative = from the
        // right edge), its top <paramref name="top"/> below the parent's top, <paramref name="height"/> tall.
        private static void Span(RectTransform rect, float left, float right, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, top - height);
            rect.offsetMax = new Vector2(right, top);
        }

        // A pill button: a vivid face with the kit's slab and press (no gloss, plan 3.1), its label filling the face (it
        // shrinks to fit a long caption, never past the face). A disabled button keeps its color (the screens color it:
        // unavailable, owned), so a price stays readable.
        private static (Button, TMP_Text) PillButton(string name, RectTransform parent, Sprite pill, Vector2 anchor, Vector2 position,
            Vector2 size, Color color, string caption, int fontSize, bool inkText = false)
        {
            float slab = Mathf.Clamp(size.y * 0.07f, 6f, 12f);
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var face = buttonObject.GetComponent<Image>();
            face.color = color;
            ClayUi.MakePill(face, pill, size.y);
            ClayUi.AddSlab(face, slab);
            if (inkText)
            {
                // A light button (cancel, back, the gift seal) gets the card border, so it reads as a button on a light card
                // too. After the slab: ClayUi finds the slab as the first Shadow.
                Outline border = buttonObject.AddComponent<Outline>();
                border.effectColor = UiColors.CardBorder;
                border.effectDistance = new Vector2(4f, -4f);
            }

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.disabledColor = Color.white;
            button.colors = colors;
            ClayUi.AddPress(button, slab);
            ClayUi.ExpandTouchTarget(face, size.y, 120f);
            TMP_Text label = Caption("Label", rect, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, fontSize,
                inkText ? UiColors.Ink : Color.white, inkText);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            float pad = Mathf.Max(20f, size.y * 0.22f);
            labelRect.offsetMin = new Vector2(pad, 6f);
            labelRect.offsetMax = new Vector2(-pad, -6f);
            label.text = caption;
            return (button, label);
        }

        // RT-6t.16: a button that spans [left, right] of its parent's width (anchors), keeping its height and y.
        private static void StretchAcross(Button button, float left, float right)
        {
            var rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(left, rect.anchorMin.y);
            rect.anchorMax = new Vector2(right, rect.anchorMax.y);
            rect.sizeDelta = new Vector2(0f, rect.sizeDelta.y);
            rect.anchoredPosition = new Vector2(0f, rect.anchoredPosition.y);
        }

        // RT-6t.16 (plan 3.4): the home level meter, a compact capsule at most LevelMeterShare of the safe width, centered,
        // with "LEVEL x/n" over a row of small dots (RT-6t.17: light, ink text, the boss level a crown). The dots sit on
        // anchors spread over the row (ClaybreakScreens spaces them by the world's level count), never a fixed step.
        private static (UnityEngine.Object[], TMP_Text) LevelMeter(RectTransform home, Sprite pill)
        {
            const float height = 130f;
            const float top = -700f;
            Image capsule = Card("LevelMeter", home, pill, new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, height));
            RectTransform rect = capsule.rectTransform;
            float side = (1f - LevelMeterShare) * 0.5f;
            rect.anchorMin = new Vector2(side, 1f);
            rect.anchorMax = new Vector2(1f - side, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, top);
            rect.sizeDelta = new Vector2(0f, height);
            TMP_Text label = Caption("Label", rect, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero, 36, UiColors.Ink, true);
            Span(label.rectTransform, 40f, -40f, -10f, 50f);
            label.text = ClaybreakScreens.LevelMeterLine(0, LevelDots);
            RectTransform row = Stretch("Dots", rect);
            row.offsetMin = new Vector2(height * 0.4f, 14f);
            row.offsetMax = new Vector2(-height * 0.4f, -66f);
            var dots = new UnityEngine.Object[LevelDots];
            for (int i = 0; i < LevelDots; i++)
            {
                float x = (i + 0.5f) / LevelDots;
                dots[i] = Img("Dot" + (i + 1), row, pill, new Vector2(x, 0.5f), Vector2.zero, new Vector2(26f, 26f), Color.white);
            }

            return (dots, label);
        }

        // RT-6t.17: a menu label, TextMeshPro with the static Baloo 2 atlas and the game's inline icons. On a colored face
        // or the veil: white (or tinted) with the ink outline; on a light card: ink, no outline. It shrinks to fit its box.
        private static TextMeshProUGUI Caption(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size,
            float fontSize, Color color, bool onCard)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = GameFonts.Sdf();
            text.fontSharedMaterial = GameFonts.Preset(onCard ? GameFonts.InkPath : GameFonts.UiPath);
            text.spriteAsset = GameFonts.Icons();
            text.fontSize = fontSize;
            text.enableAutoSizing = true;
            text.fontSizeMax = fontSize;
            text.fontSizeMin = Mathf.Max(18f, Mathf.Round(fontSize * 0.55f));
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.color = color;
            text.raycastTarget = false;
            text.text = string.Empty;
            return text;
        }

        private static RectTransform Stretch(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            return rect;
        }

        private static Image Img(string name, Transform parent, Sprite sprite, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        // RT-6t.9: the game's own font (Baloo 2 ExtraBold, SIL OFL 1.1, Content/Fonts with its license; GameFonts).
        public static Font GameFont()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null)
            {
                throw new InvalidOperationException(FontPath + " is missing.");
            }

            return font;
        }

        private static Text Label(string name, Transform parent, Vector2 anchor, Vector2 position, int size, TextAnchor alignment)
        {
            Font font = GameFont();
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(940f, 120f);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = FontStyle.Normal;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            go.GetComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.7f);
            return text;
        }

        // ---------- world labels and generated sprites ----------

        private static TextMesh CreateLabel(string name, Transform parent, Vector3 position, float size)
        {
            Font font = GameFont();
            var go = new GameObject(name, typeof(MeshRenderer), typeof(TextMesh));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var text = go.GetComponent<TextMesh>();
            text.font = font;
            text.fontSize = 64;
            text.characterSize = size * 0.16f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontStyle = FontStyle.Normal;
            text.color = Color.white;
            text.text = "0";
            go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return text;
        }

        // A 64 px white circle sliced with a 32 px border: the pill shape the kit's ClayUi expects.
        private static void CreatePillSprite()
        {
            WriteSprite(PillTexturePath, 64, (x, y, size) =>
            {
                float dx = x + 0.5f - size * 0.5f;
                float dy = y + 0.5f - size * 0.5f;
                return Mathf.Clamp01(size * 0.5f - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
            }, Vector4.one * 32f);
        }

        // A 128 px five-point star, anti-aliased by 4x4 supersampling.
        private static void CreateStarSprite()
        {
            var points = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float angle = Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                float radius = i % 2 == 0 ? 60f : 25f;
                points[i] = new Vector2(64f + Mathf.Cos(angle) * radius, 62f + Mathf.Sin(angle) * radius);
            }

            WriteSprite(StarTexturePath, 128, (x, y, size) =>
            {
                int inside = 0;
                for (int sy = 0; sy < 4; sy++)
                {
                    for (int sx = 0; sx < 4; sx++)
                    {
                        if (Inside(points, new Vector2(x + (sx + 0.5f) / 4f, y + (sy + 0.5f) / 4f)))
                        {
                            inside++;
                        }
                    }
                }

                return inside / 16f;
            }, Vector4.zero);
        }

        // A 128 px crown (three points over a band), anti-aliased by 4x4 supersampling; white, tinted by the meter.
        private static void CreateCrownSprite()
        {
            var points = new[]
            {
                new Vector2(18f, 22f), new Vector2(110f, 22f), new Vector2(120f, 98f), new Vector2(88f, 64f),
                new Vector2(64f, 112f), new Vector2(40f, 64f), new Vector2(8f, 98f)
            };
            WriteSprite(CrownTexturePath, 128, (x, y, size) =>
            {
                int inside = 0;
                for (int sy = 0; sy < 4; sy++)
                {
                    for (int sx = 0; sx < 4; sx++)
                    {
                        if (Inside(points, new Vector2(x + (sx + 0.5f) / 4f, y + (sy + 0.5f) / 4f)))
                        {
                            inside++;
                        }
                    }
                }

                return inside / 16f;
            }, Vector4.zero);
        }

        // A 64 px vertical ramp: opaque at the bottom row, clear at the top (eased, so the fade starts soft).
        private static void CreateFadeSprite()
        {
            WriteSprite(FadeTexturePath, 64, (x, y, size) =>
            {
                float t = 1f - (y + 0.5f) / size;
                return t * t;
            }, Vector4.zero);
        }

        private static bool Inside(Vector2[] polygon, Vector2 point)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if ((polygon[i].y > point.y) != (polygon[j].y > point.y)
                    && point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private static void WriteSprite(string path, int size, Func<int, int, int, float> alpha, Vector4 border)
        {
            if (!File.Exists(path))
            {
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha(x, y, size) * 255f));
                    }
                }

                texture.SetPixels32(pixels);
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteBorder != border)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = border;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
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
