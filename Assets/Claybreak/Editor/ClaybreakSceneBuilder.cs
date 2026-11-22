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
        /// <summary>Most units in flight at once in the assault (the instanced renderer's capacity; D27 unanswered: 300).</summary>
        public const int FlightCapacity = 300;

        /// <summary>Most assault units dying at once (the ballistic death ring of the units in flight).</summary>
        public const int FlightDyingCapacity = 512;

        /// <summary>Crowd cap of the run (decision D27, unanswered: 300); the swarm is built for 1000+, the cap is data.</summary>
        public const int CrowdCapacity = 300;
        public const string CrowdCameraProfilePath = Root + "/Data/SwarmCameraRun.asset";
        /// <summary>The camera rig's fixed shots of the assault (RT-6t.7; the old AssaultCamera values).</summary>
        public const string AssaultShotPath = Root + "/Data/SwarmCameraAssault.asset";
        public const string BossAssaultShotPath = Root + "/Data/SwarmCameraAssaultBoss.asset";
        public const string CrowdLookPath = Root + "/Data/CrowdToonLook.asset";
        /// <summary>The skybox: the palette's screen-space sky gradient (RT-6t.6a).</summary>
        public const string SkyMaterialPath = ArtPalette.MaterialFolder + "/SkyGradient.mat";

        public const int ProjectilePoolSize = 32;
        public const int LevelDots = ClaybreakWorld.MaximumLevels;
        public const float RowStep = 230f;

        private const string GeneratedFolder = Root + "/Generated";
        public const string FontPath = GameFonts.TtfPath;
        private const string PillTexturePath = GeneratedFolder + "/ClayPill.png";
        private const string StarTexturePath = GeneratedFolder + "/Star.png";
        public const float TrackWidth = 8f;

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
        private static readonly Color CardColor = new Color(0.12f, 0.09f, 0.1f, 0.9f);

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
            WorldContent.Write();
            MonetizationContent.Write();
            BuildCrowdData();
            BuildTheme();
            new ArtPalette();
            GateArt.BuildAssets();
            LeaderArt.BuildAssets();
            CreatePillSprite();
            CreateStarSprite();
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
            var palette = new ArtPalette();

            float trackEnd = 0f;
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
            ClaybreakScreens screens = CreateScreens(canvas, pill, star, catalog);
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
            scaler.matchWidthOrHeight = 1f;
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

        private static ClaybreakScreens CreateScreens(Transform canvas, Sprite pill, Sprite star, ClaybreakCatalog catalog)
        {
            var screensObject = new GameObject("Screens", typeof(RectTransform));
            RectTransform screensRect = screensObject.GetComponent<RectTransform>();
            screensRect.SetParent(canvas, false);
            screensRect.anchorMin = Vector2.zero;
            screensRect.anchorMax = Vector2.one;
            screensRect.sizeDelta = Vector2.zero;
            ClaybreakScreens screens = screensObject.AddComponent<ClaybreakScreens>();
            Color grey = new Color(0.3f, 0.26f, 0.26f);

            // Home: the level waits behind a dark veil; buttons stack from the bottom (thumb reach). Every page's
            // veil covers the whole screen; its content sits in the safe area (RT-8).
            RectTransform home = MenuPage("Home", screensRect, 0.55f, out RectTransform homePage);
            Text homeCoins = Coins(home, pill);
            // Working name (D18), with the call to action under it.
            Text title = Label("Title", home, new Vector2(0.5f, 1f), new Vector2(0f, -270f), 120, TextAnchor.MiddleCenter);
            title.text = "CLAY SIEGE";
            title.color = Orange;
            Text tagline = Label("Tagline", home, new Vector2(0.5f, 1f), new Vector2(0f, -385f), 40, TextAnchor.MiddleCenter);
            tagline.text = "STORM THE TOWER";
            Text world = Label("World", home, new Vector2(0.5f, 1f), new Vector2(0f, -440f), 46, TextAnchor.MiddleCenter);
            Text worldStars = Label("Stars", home, new Vector2(0.5f, 1f), new Vector2(0f, -505f), 44, TextAnchor.MiddleCenter);
            worldStars.color = Gold;
            var dots = new UnityEngine.Object[LevelDots];
            for (int i = 0; i < LevelDots; i++)
            {
                dots[i] = Img("Dot" + (i + 1), home, pill, new Vector2(0.5f, 1f), new Vector2((i - (LevelDots - 1) * 0.5f) * 92f, -650f),
                    new Vector2(50f, 50f), Color.white);
            }

            Text loadout = Label("Loadout", home, new Vector2(0.5f, 1f), new Vector2(0f, -720f), 38, TextAnchor.MiddleCenter);
            loadout.color = new Color(1f, 0.86f, 0.72f);
            (Button play, Text playLabel) = PillButton("Play", home, pill, new Vector2(0.5f, 0f), new Vector2(0f, 420f),
                new Vector2(640f, 180f), Orange, "PLAY", 84);
            (Button shop, Text _) = PillButton("ShopButton", home, pill, new Vector2(0.5f, 0f), new Vector2(-235f, 235f),
                new Vector2(430f, 130f), new Color(0.55f, 0.32f, 0.16f), "SHOP", 56);
            Image badge = Img("Badge", shop.transform, pill, new Vector2(1f, 1f), new Vector2(-30f, -22f), new Vector2(46f, 46f), Gold);
            (Button worldsButton, Text _) = PillButton("WorldsButton", home, pill, new Vector2(0.5f, 0f), new Vector2(235f, 235f),
                new Vector2(430f, 130f), new Color(0.55f, 0.32f, 0.16f), "WORLDS", 56);
            // RT-7: the daily gift over PLAY (shown only when due). RT-8: SETTINGS top left opens the settings page.
            // RT-6t.6b: the crowd shape picker under the loadout line (free shapes, D26 A); the crowd behind the menu shows it.
            (Button shapeButton, Text shapeLabel) = PillButton("ShapeButton", home, pill, new Vector2(0.5f, 1f), new Vector2(0f, -830f),
                new Vector2(520f, 110f), new Color(0.42f, 0.3f, 0.5f), ClaybreakScreens.ShapeLine(catalog.Shapes[0].title), 44);
            (Button daily, Text dailyLabel) = PillButton("DailyGift", home, pill, new Vector2(0.5f, 0f), new Vector2(0f, 600f),
                new Vector2(560f, 110f), new Color(0.3f, 0.55f, 0.3f), "DAILY GIFT", 44);
            (Button settingsButton, Text _) = PillButton("SettingsButton", home, pill, new Vector2(0f, 1f), new Vector2(190f, -150f),
                new Vector2(300f, 96f), grey, "SETTINGS", 38);
            // Scene default shows the usual case (the flow sets it when the home screen opens): a gift due on a first launch.
            dailyLabel.text = $"DAILY GIFT  +{MonetizationContent.DailyRewards[0]} ¢";

            // Settings (RT-8): the kit's sound, music and vibration switches, the privacy policy link, the privacy
            // options (only where UMP requires them) and the credits of the game's own audio and font.
            RectTransform settingsRoot = MenuPage("Settings", screensRect, 0.82f, out RectTransform settingsPage);
            Heading(settingsRoot, "SETTINGS");
            (Button sound, Text soundLabel) = PillButton("Sound", settingsRoot, pill, new Vector2(0.5f, 1f), new Vector2(0f, -420f),
                new Vector2(640f, 120f), grey, "SOUND ON", 52);
            (Button music, Text musicLabel) = PillButton("Music", settingsRoot, pill, new Vector2(0.5f, 1f), new Vector2(0f, -570f),
                new Vector2(640f, 120f), grey, "MUSIC ON", 52);
            (Button vibration, Text vibrationLabel) = PillButton("Vibration", settingsRoot, pill, new Vector2(0.5f, 1f),
                new Vector2(0f, -720f), new Vector2(640f, 120f), grey, "VIBRATION ON", 52);
            (Button policy, Text _) = PillButton("PrivacyPolicy", settingsRoot, pill, new Vector2(0.5f, 1f), new Vector2(0f, -900f),
                new Vector2(640f, 110f), new Color(0.55f, 0.32f, 0.16f), "PRIVACY POLICY", 44);
            (Button privacy, Text _) = PillButton("PrivacyOptions", settingsRoot, pill, new Vector2(0.5f, 1f), new Vector2(0f, -1040f),
                new Vector2(640f, 110f), new Color(0.55f, 0.32f, 0.16f), "PRIVACY OPTIONS", 44);
            privacy.gameObject.SetActive(false);
            Text settingsNote = Label("Note", settingsRoot, new Vector2(0.5f, 0f), new Vector2(0f, 400f), 36, TextAnchor.MiddleCenter);
            settingsNote.color = new Color(1f, 0.86f, 0.72f);
            Text credits = Label("Credits", settingsRoot, new Vector2(0.5f, 0f), new Vector2(0f, 270f), 30, TextAnchor.MiddleCenter);
            credits.text = "CLAY SIEGE  ·  JUNGLE DEGREE\nMUSIC AND SOUND: JUNGLE DEGREE  ·  FONT: BALOO 2 (SIL OFL 1.1)";
            credits.color = new Color(1f, 1f, 1f, 0.6f);
            (Button settingsBack, Text _) = PillButton("Back", settingsRoot, pill, new Vector2(0.5f, 0f), new Vector2(0f, 120f),
                new Vector2(420f, 120f), grey, "BACK", 48);

            // Shop: three tabs over one list (rows = the longest catalog list), coins only (D6: no real money).
            RectTransform shopRoot = MenuPage("Shop", screensRect, 0.82f, out RectTransform shopPage);
            Text shopCoins = Coins(shopRoot, pill);
            Heading(shopRoot, "SHOP");
            string[] tabNames = { "UPGRADES", "ELITES", "SKINS" };
            var tabs = new UnityEngine.Object[tabNames.Length];
            for (int i = 0; i < tabNames.Length; i++)
            {
                (Button tab, Text _) = PillButton("Tab" + tabNames[i], shopRoot, pill, new Vector2(0.5f, 1f),
                    new Vector2((i - 1) * 330f, -350f), new Vector2(310f, 110f), grey, tabNames[i], 40);
                tabs[i] = tab;
            }

            int shopCount = Mathf.Max(catalog.Upgrades.Count, Mathf.Max(catalog.Units.Count, catalog.Skins.Count));
            (RectTransform shopContent, UnityEngine.Object[] shopRows) = List(shopRoot, pill, "ShopList", -430f, 250f, shopCount);
            (Button shopBack, Text _) = PillButton("Back", shopRoot, pill, new Vector2(0.5f, 0f), new Vector2(0f, 120f),
                new Vector2(420f, 120f), grey, "BACK", 48);

            // Worlds: one row per catalog world, any number (RT-6 adds them as data).
            RectTransform worldsRoot = MenuPage("Worlds", screensRect, 0.82f, out RectTransform worldsPage);
            Heading(worldsRoot, "WORLDS");
            Text worldsStars = Label("Stars", worldsRoot, new Vector2(0.5f, 1f), new Vector2(0f, -300f), 52, TextAnchor.MiddleCenter);
            worldsStars.color = Gold;
            (RectTransform worldsContent, UnityEngine.Object[] worldRows) =
                List(worldsRoot, pill, "WorldList", -400f, 330f, catalog.Worlds.Count);
            Text footer = Label("Footer", worldsRoot, new Vector2(0.5f, 0f), new Vector2(0f, 255f), 40, TextAnchor.MiddleCenter);
            footer.color = new Color(1f, 1f, 1f, 0.6f);
            (Button worldsBack, Text _) = PillButton("Back", worldsRoot, pill, new Vector2(0.5f, 0f), new Vector2(0f, 120f),
                new Vector2(420f, 120f), grey, "BACK", 48);

            // Result: veil, one card, one big button (NEXT / AGAIN) and a small HOME.
            RectTransform result = MenuPage("Result", screensRect, 0.6f, out RectTransform resultPage);
            Image card = Img("Card", result, pill, new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(900f, 1060f), CardColor);
            ClayUi.MakePill(card, pill, 140f);
            RectTransform cardRect = card.rectTransform;
            Text resultTitle = Label("Title", cardRect, new Vector2(0.5f, 1f), new Vector2(0f, -130f), 96, TextAnchor.MiddleCenter);
            var stars = new UnityEngine.Object[3];
            for (int i = 0; i < 3; i++)
            {
                Image starImage = Img("Star" + (i + 1), cardRect, star, new Vector2(0.5f, 1f), new Vector2((i - 1) * 190f, -290f - (i == 1 ? 20f : 0f)),
                    new Vector2(i == 1 ? 190f : 160f, i == 1 ? 190f : 160f), Gold);
                stars[i] = starImage;
            }

            Text resultDetail = Label("Detail", cardRect, new Vector2(0.5f, 1f), new Vector2(0f, -450f), 52, TextAnchor.MiddleCenter);
            Text resultReward = Label("Reward", cardRect, new Vector2(0.5f, 1f), new Vector2(0f, -540f), 60, TextAnchor.MiddleCenter);
            resultReward.color = Gold;
            Text resultUnlock = Label("Unlock", cardRect, new Vector2(0.5f, 1f), new Vector2(0f, -670f), 44, TextAnchor.MiddleCenter);
            resultUnlock.color = Orange;
            resultUnlock.rectTransform.sizeDelta = new Vector2(820f, 140f);
            (Button primary, Text primaryLabel) = PillButton("Primary", cardRect, pill, new Vector2(0.5f, 0f), new Vector2(0f, 150f),
                new Vector2(600f, 170f), Orange, "NEXT", 80);
            (Button homeButton, Text _) = PillButton("Home", cardRect, pill, new Vector2(0.5f, 0f), new Vector2(0f, 30f),
                new Vector2(340f, 96f), grey, "HOME", 40);
            // RT-7: the optional rewarded offer under the card, the gain written on it.
            (Button resultAd, Text resultAdLabel) = PillButton("RewardedOffer", result, pill, new Vector2(0.5f, 0f), new Vector2(0f, 330f),
                new Vector2(760f, 130f), new Color(0.3f, 0.55f, 0.3f), "WATCH AD", 44);

            // Continue offer: one card, what the ad gives, and NO THANKS as clear as the offer (GDD 7.1).
            RectTransform continueRoot = MenuPage("Continue", screensRect, 0.6f, out RectTransform continuePage);
            Image continueCard = Img("Card", continueRoot, pill, new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(900f, 760f), CardColor);
            ClayUi.MakePill(continueCard, pill, 140f);
            RectTransform continueRect = continueCard.rectTransform;
            Text continueTitle = Label("Title", continueRect, new Vector2(0.5f, 1f), new Vector2(0f, -120f), 88, TextAnchor.MiddleCenter);
            continueTitle.text = "ARMY LOST";
            Text continueText = Label("Offer", continueRect, new Vector2(0.5f, 1f), new Vector2(0f, -290f), 48, TextAnchor.MiddleCenter);
            continueText.rectTransform.sizeDelta = new Vector2(820f, 160f);
            (Button continueAd, Text continueAdLabel) = PillButton("WatchAd", continueRect, pill, new Vector2(0.5f, 0f), new Vector2(0f, 230f),
                new Vector2(640f, 150f), new Color(0.3f, 0.55f, 0.3f), "WATCH AD", 60);
            (Button continueDecline, Text _) = PillButton("NoThanks", continueRect, pill, new Vector2(0.5f, 0f), new Vector2(0f, 80f),
                new Vector2(480f, 110f), grey, "NO THANKS", 48);

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
            SetRef(screens, "playButton", play);
            SetRef(screens, "playLabel", playLabel);
            SetRef(screens, "shopButton", shop);
            SetRef(screens, "shopBadge", badge.gameObject);
            SetRef(screens, "worldsButton", worldsButton);
            SetRef(screens, "shapeButton", shapeButton);
            SetRef(screens, "shapeLabel", shapeLabel);
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
            SetArray(screens, "shopRows", shopRows);
            SetRef(screens, "shopBack", shopBack);
            SetRef(screens, "worlds", worldsPage.gameObject);
            SetRef(screens, "worldsStars", worldsStars);
            SetRef(screens, "worldsContent", worldsContent);
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

        // A full-screen menu page over a dark veil (the page, shown and hidden as a whole); returns its content
        // area, fitted to the phone's safe area (RT-8).
        private static RectTransform MenuPage(string name, RectTransform parent, float veilAlpha, out RectTransform page)
        {
            page = Stretch(name, parent);
            Image veil = page.gameObject.AddComponent<Image>();
            veil.color = new Color(0.06f, 0.04f, 0.05f, veilAlpha);
            RectTransform content = Stretch("Safe", page);
            content.gameObject.AddComponent<SafeAreaFitter>();
            return content;
        }

        private static Text Coins(RectTransform screen, Sprite pill)
        {
            Image coin = Img("CoinIcon", screen, pill, new Vector2(1f, 1f), new Vector2(-215f, -150f), new Vector2(46f, 46f), Gold);
            coin.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            Text coins = Label("Coins", screen, new Vector2(1f, 1f), new Vector2(-60f, -150f), 52, TextAnchor.MiddleRight);
            coins.rectTransform.sizeDelta = new Vector2(170f, 90f);
            coins.rectTransform.pivot = new Vector2(1f, 0.5f);
            return coins;
        }

        private static void Heading(RectTransform screen, string caption)
        {
            Text heading = Label("Heading", screen, new Vector2(0.5f, 1f), new Vector2(0f, -150f), 96, TextAnchor.MiddleCenter);
            heading.text = caption;
            heading.color = Orange;
        }

        // A vertical scroll list between <paramref name="top"/> (from the top) and <paramref name="bottom"/>
        // (from the bottom), with <paramref name="rows"/> preallocated rows at RowStep apart.
        private static (RectTransform, UnityEngine.Object[]) List(RectTransform screen, Sprite pill, string name, float top,
            float bottom, int rows)
        {
            RectTransform viewport = Stretch(name, screen);
            viewport.offsetMin = new Vector2(70f, bottom);
            viewport.offsetMax = new Vector2(-70f, top);
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

            return (content, created);
        }

        private static ClaybreakListRow ListRow(RectTransform content, Sprite pill, int index)
        {
            const float height = 210f;
            Image card = Img("Row" + (index + 1), content, pill, new Vector2(0.5f, 1f),
                new Vector2(0f, -index * RowStep - height * 0.5f), new Vector2(930f, height), CardColor);
            ClayUi.MakePill(card, pill, 100f);
            ClayUi.AddSlab(card, 8f);
            RectTransform rect = card.rectTransform;
            Text title = Label("Title", rect, new Vector2(0f, 1f), new Vector2(50f, -26f), 44, TextAnchor.UpperLeft);
            title.color = Orange;
            Text detail = Label("Detail", rect, new Vector2(0f, 1f), new Vector2(50f, -84f), 30, TextAnchor.UpperLeft);
            detail.rectTransform.sizeDelta = new Vector2(590f, 120f);
            detail.horizontalOverflow = HorizontalWrapMode.Wrap;
            Image swatch = Img("Swatch", rect, pill, new Vector2(1f, 1f), new Vector2(-310f, -48f), new Vector2(54f, 54f), Color.white);
            (Button button, Text label) = PillButton("Action", rect, pill, new Vector2(1f, 0.5f), new Vector2(-140f, -10f),
                new Vector2(220f, 110f), Orange, "BUY", 40);
            ClaybreakListRow row = card.gameObject.AddComponent<ClaybreakListRow>();
            SetRef(row, "title", title);
            SetRef(row, "detail", detail);
            SetRef(row, "swatch", swatch);
            SetRef(row, "button", button);
            SetRef(row, "buttonLabel", label);
            return row;
        }

        private static (Button, Text) PillButton(string name, RectTransform parent, Sprite pill, Vector2 anchor, Vector2 position,
            Vector2 size, Color color, string caption, int fontSize)
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
            ClayUi.AddGloss(rect, pill, size.y);
            Button button = buttonObject.GetComponent<Button>();
            ClayUi.AddPress(button, slab);
            ClayUi.ExpandTouchTarget(face, size.y, 120f);
            Text label = Label("Label", rect, new Vector2(0.5f, 0.5f), Vector2.zero, fontSize, TextAnchor.MiddleCenter);
            label.text = caption;
            return (button, label);
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
