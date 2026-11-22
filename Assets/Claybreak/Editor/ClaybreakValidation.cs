using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using JungleDegree.StudioKit.Crowd;
using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The game's validator (the theme completeness check of the plan): the meta catalog (RT-5) and every
    /// world and level against the crowd cap of the run and the assault pool (RT-6t.4); per world (RT-6) its look, its own scenery and towers (no obvious
    /// visual repetition between worlds), a boss with a rule, a level ramp that never gets easier, and its
    /// device-gate level (the world's heaviest tower, every defense at its limit); the theme, the save validator over
    /// the fixture saves (every version a phone may hold must migrate and pass <see cref="SaveValidator"/>),
    /// the scene wiring (the instanced run crowd and its camera, tower bodies, HUD, screens with one row per catalog
    /// entry, sounds, preallocated pools), and the code rules (no Instantiate/Destroy at run time, rule 2.2.1; no physics queries,
    /// rule 2.2.7).
    /// Batchmode: -executeMethod Claybreak.Editor.ClaybreakValidation.RunAndQuit
    /// </summary>
    public static class ClaybreakValidation
    {
        private const string RuntimeFolder = "Assets/Claybreak/Runtime";

        // Run-time code under rule 2.2.1: the game, its toon lab and the incubated kit code (RT-6t). A line marked
        // "// load-time:" is allowed: it runs once when its owner loads or unloads, never while playing.
        private static readonly string[] RuntimeCodeFolders =
        {
            RuntimeFolder,
            "Assets/Claybreak/Lab/Runtime",
            "Assets/StudioKitIncubator/Toon",
            "Assets/StudioKitIncubator/Swarm",
        };

        private const string LoadTimeMarker = "// load-time:";
        public const string SaveFixtureFolder = "Assets/Claybreak/Editor/SaveFixtures";
        private static readonly Regex ForbiddenRuntimeCalls = new Regex(
            @"\b(Instantiate|Destroy|DestroyImmediate)\s*\(|new\s+GameObject\s*\(|\.AddComponent\s*<|\bPhysics\.",
            RegexOptions.Compiled);

        // The gate colors of the art direction (RT-6t.9 puts them on the gates): a crowd must not read as a gate.
        private static readonly Color ReferenceGateGood = new Color32(0x5a, 0xd1, 0xa0, 0xff);
        private static readonly Color ReferenceGateBad = new Color32(0xff, 0x6b, 0x81, 0xff);

        [MenuItem("Claybreak/Validate")]
        public static void ValidateFromMenu()
        {
            ReportWarnings(Warnings());
            Report(Run());
        }

        public static void RunAndQuit()
        {
            ReportWarnings(Warnings());
            List<string> failures = Run();
            Report(failures);
            EditorApplication.Exit(failures.Count == 0 ? 0 : 1);
        }

        public static List<string> Run()
        {
            var failures = new List<string>();
            CheckData(failures);
            CheckSaves(failures);
            CheckRuntimeCode(failures);
            CheckScene(failures);
            CheckToonLab(failures);
            failures.AddRange(MonetizationValidation.Run());
            return failures;
        }

        private static void Report(List<string> failures)
        {
            if (failures.Count == 0)
            {
                Debug.Log("PASS Claybreak validation: catalog, worlds, levels, device-gate level, theme, saves, scene, pools, runtime code rules, toon lab and monetization.");
            }
            else
            {
                Debug.LogError("FAIL Claybreak validation:\n" + string.Join("\n", failures));
            }
        }

        /// <summary>
        /// What deserves a look but does not fail the validation (RT-6t.6a): palette pairs that may be hard to tell apart.
        /// The crowd against its track, its fields and the two gate colors (CIE76 ΔE under
        /// <see cref="ToonContrast.DistinctDeltaE"/>), and the UI ink on the UI accent (WCAG ratio under
        /// <see cref="ToonContrast.LargeTextRatio"/>).
        /// </summary>
        public static List<string> Warnings()
        {
            var warnings = new List<string>();
            var catalog = AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(WorldContent.CatalogPath);
            if (catalog == null)
            {
                return warnings;
            }

            for (int w = 0; w < catalog.Worlds.Count; w++)
            {
                ToonPalette palette = catalog.Worlds[w].world == null ? null : catalog.Worlds[w].world.Look.palette;
                if (palette == null)
                {
                    continue;
                }

                string where = $"world {w + 1} ({palette.name})";
                WarnIfClose(warnings, where, "crowd", palette.Crowd, "track", palette.Track);
                WarnIfClose(warnings, where, "crowd", palette.Crowd, "ground", palette.Ground);
                WarnIfClose(warnings, where, "crowd", palette.Crowd, "good gate", ReferenceGateGood);
                WarnIfClose(warnings, where, "crowd", palette.Crowd, "bad gate", ReferenceGateBad);
                float ratio = ToonContrast.Ratio(palette.UiInk, palette.UiAccent);
                if (ratio < ToonContrast.LargeTextRatio)
                {
                    warnings.Add($"{where}: UI ink on the UI accent has contrast {ratio:0.00}:1 (under {ToonContrast.LargeTextRatio}:1)");
                }
            }

            return warnings;
        }

        private static void WarnIfClose(List<string> warnings, string where, string nameA, Color a, string nameB, Color b)
        {
            float deltaE = ToonContrast.DeltaE(a, b);
            if (deltaE < ToonContrast.DistinctDeltaE)
            {
                warnings.Add($"{where}: {nameA} #{ColorUtility.ToHtmlStringRGB(a)} and {nameB} #{ColorUtility.ToHtmlStringRGB(b)} "
                    + $"are close (delta E {deltaE:0.0}, contrast {ToonContrast.Ratio(a, b):0.00}:1)");
            }
        }

        private static void ReportWarnings(List<string> warnings)
        {
            if (warnings.Count > 0)
            {
                Debug.LogWarning("WARN Claybreak validation (contrast; not a failure):\n" + string.Join("\n", warnings));
            }
        }

        private static void CheckData(List<string> failures)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(WorldContent.CatalogPath);
            var theme = AssetDatabase.LoadAssetAtPath<ClaybreakTheme>(ClaybreakSceneBuilder.ThemePath);
            if (catalog == null || theme == null)
            {
                failures.Add("missing catalog or theme (run Claybreak > Build Scene)");
                return;
            }

            if (!catalog.TryValidate(ClaybreakSceneBuilder.CrowdCapacity, ClaybreakSceneBuilder.PoolSize, out string error))
            {
                failures.Add("catalog: " + error);
                return;
            }

            // The slice's save (version 1) names these; they must stay in the catalog forever.
            if (catalog.WorldIndexOf(SaveMigration.V1WorldId) != 0 || catalog.UpgradeIndexOf(SaveMigration.V1StartingArmyId) < 0
                || catalog.UnitIndexOf(SaveMigration.V1BruteId) < 0)
            {
                failures.Add("catalog: the version-1 save ids (world1 first, army, brute) must stay in the catalog");
            }

            if (catalog.Worlds.Count != WorldContent.Worlds.Length)
            {
                failures.Add($"catalog: {catalog.Worlds.Count} worlds, content defines {WorldContent.Worlds.Length} (rebuild the scene)");
            }

            var sceneries = new HashSet<SceneryStyle>();
            var palettes = new HashSet<ToonPalette>();
            var towersSeen = new Dictionary<TowerKind, string>();
            for (int w = 0; w < catalog.Worlds.Count; w++)
            {
                ClaybreakWorld world = catalog.Worlds[w].world;
                CheckWorld(world, w, sceneries, palettes, towersSeen, failures);
                var stress = AssetDatabase.LoadAssetAtPath<ClaybreakLevel>(WorldContent.StressLevelPath(w));
                if (stress == null)
                {
                    failures.Add($"{world.name}: missing device-gate level {WorldContent.StressLevelPath(w)}");
                    continue;
                }

                CheckStress(world, stress, failures);
            }

            if (!ClaybreakSfx.Covers(theme.Audio, out string missingCue))
            {
                failures.Add($"theme: the game's sound cue '{missingCue}' has no extra sound in the theme audio");
            }

            if (!ClaybreakAudio.TryValidateLicensed(out string audioError))
            {
                failures.Add("audio: " + audioError);
            }

            if (!theme.TryValidate(out error))
            {
                failures.Add("theme: " + error);
            }
        }

        // One world: levels within the menu, a look of its own (scenery style and palette not shared with another
        // world; the palette is the content's), towers that no earlier world uses for its main tower or boss, 2+ kinds
        // plus the boss, health that never drops from one plain level to the next and a boss above them all.
        private static void CheckWorld(ClaybreakWorld world, int index, HashSet<SceneryStyle> sceneries, HashSet<ToonPalette> palettes,
            Dictionary<TowerKind, string> towersSeen, List<string> failures)
        {
            if (world.LevelCount > ClaybreakSceneBuilder.LevelDots)
            {
                failures.Add($"{world.name}: {world.LevelCount} levels but the menu has {ClaybreakSceneBuilder.LevelDots} dots");
            }

            if (!world.Look.IsSet)
            {
                failures.Add($"{world.name}: no look (palette, ground, sun)");
            }
            else if (!sceneries.Add(world.Look.scenery) || !palettes.Add(world.Look.palette))
            {
                failures.Add($"{world.name}: scenery {world.Look.scenery} or its palette repeats another world's (no obvious visual repetition)");
            }
            else
            {
                CheckPalette(world, index, failures);
            }

            var kinds = new HashSet<TowerKind>();
            int previousHealth = 0;
            int bossHealth = world.LevelCount > 0 ? world.Levels[world.LevelCount - 1].TowerHealth : 0;
            for (int i = 0; i < world.LevelCount; i++)
            {
                ClaybreakLevel level = world.Levels[i];
                CheckLevel(level, failures);
                kinds.Add(level.Tower);
                if (level.RunGates.Count < 3 || level.RunGates.Count > RunGateCourse.MaximumPairs)
                {
                    failures.Add($"{level.name}: 3-{RunGateCourse.MaximumPairs} run gate pairs, has {level.RunGates.Count}");
                }

                bool boss = i == world.LevelCount - 1;
                if (!boss && (level.TowerHealth < previousHealth - 60 || level.TowerHealth >= bossHealth))
                {
                    failures.Add($"{level.name}: tower health {level.TowerHealth} breaks the ramp (previous {previousHealth}, boss {bossHealth})");
                }

                previousHealth = Mathf.Max(previousHealth, level.TowerHealth);
                if (boss && (level.BossRule == BossRule.None || level.Floors < 3))
                {
                    failures.Add($"{level.name}: the boss needs a rule and at least 3 floors");
                }
            }

            if (kinds.Count < 3)
            {
                failures.Add($"{world.name}: a world shows 2-3 towers and a boss; its levels use {kinds.Count} tower kinds");
            }

            // Each world brings its own main tower and boss; reusing an earlier world's second tower is fine.
            ClaybreakLevel last = world.LevelCount > 0 ? world.Levels[world.LevelCount - 1] : null;
            TowerKind main = world.LevelCount > 0 ? world.Levels[world.LevelCount > 1 ? 1 : 0].Tower : TowerKind.Keep;
            foreach (TowerKind own in new[] { main, last != null ? last.Tower : main })
            {
                if (towersSeen.TryGetValue(own, out string other) && other != world.WorldId)
                {
                    failures.Add($"{world.name}: tower {own} is already the main tower or boss of {other}");
                }

                towersSeen[own] = world.WorldId;
            }

            if (index == 0 && world.WorldId != SaveMigration.V1WorldId)
            {
                failures.Add("the first world must keep the slice id " + SaveMigration.V1WorldId);
            }
        }

        // The world's palette is the content's (PaletteContent: the user's exact values), complete and valid.
        private static void CheckPalette(ClaybreakWorld world, int index, List<string> failures)
        {
            ToonPalette palette = world.Look.palette;
            string error = palette.Validate();
            if (error != null)
            {
                failures.Add($"{world.name}: palette {palette.name}: {error}");
                return;
            }

            if (index >= PaletteContent.Palettes.Length || palette != PaletteContent.Load(index))
            {
                failures.Add($"{world.name}: palette {palette.name} is not the content's palette for world {index + 1} (rebuild the scene)");
                return;
            }

            PaletteDefinition definition = PaletteContent.Palettes[index];
            Color[] expected = PaletteContent.Colors(definition);
            Color[] actual = palette.Colors;
            for (int i = 0; i < expected.Length; i++)
            {
                if (ColorUtility.ToHtmlStringRGBA(expected[i]) != ColorUtility.ToHtmlStringRGBA(actual[i]))
                {
                    failures.Add($"{world.name}: palette {palette.name} {ToonPalette.ColorNames[i]} is #{ColorUtility.ToHtmlStringRGB(actual[i])}, "
                        + $"the content says {definition.Hex[i]} (rebuild the scene)");
                }
            }

            if (!Mathf.Approximately(palette.FogStart, definition.FogStart) || !Mathf.Approximately(palette.FogEnd, definition.FogEnd))
            {
                failures.Add($"{world.name}: palette {palette.name} fog {palette.FogStart}-{palette.FogEnd}, the content says {definition.FogStart}-{definition.FogEnd}");
            }
        }

        // The device gate of a world: its boss body, its boss rule, every defense the world has, at its limits.
        private static void CheckStress(ClaybreakWorld world, ClaybreakLevel stress, List<string> failures)
        {
            CheckLevel(stress, failures);
            ClaybreakLevel boss = world.Levels[world.LevelCount - 1];
            bool worldHasShield = false;
            foreach (ClaybreakLevel level in world.Levels)
            {
                worldHasShield |= level.ShieldEnabled;
            }

            if (!stress.IsDeviceGateLevel || stress.StartCount != ClaybreakSceneBuilder.CrowdCapacity
                || stress.MaximumUnitsInFlight != ClaybreakSceneBuilder.PoolSize || stress.RunGates.Count != 0)
            {
                failures.Add($"{stress.name}: the device-gate level must run the crowd at its cap ({ClaybreakSceneBuilder.CrowdCapacity}, "
                    + $"no run gate to change it) and auto-launch with {ClaybreakSceneBuilder.PoolSize} units in flight");
            }

            if (stress.Tower != boss.Tower || stress.BossRule != boss.BossRule || !stress.CannonEnabled || !stress.SweeperEnabled
                || !stress.WallEnabled || stress.MaximumProjectiles < 12 || stress.ShieldEnabled != worldHasShield)
            {
                failures.Add($"{stress.name}: the device gate of {world.WorldId} runs its boss tower ({boss.Tower}) with every defense of the world on and 12 projectiles");
            }
        }

        // The save validator (RT-5 acceptance) over every fixture: real-format saves of each version.
        private static void CheckSaves(List<string> failures)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(WorldContent.CatalogPath);
            if (catalog == null || !catalog.TryValidate(ClaybreakSceneBuilder.CrowdCapacity, ClaybreakSceneBuilder.PoolSize, out _))
            {
                return; // CheckData already reported it.
            }

            string[] files = Directory.GetFiles(SaveFixtureFolder, "*.json");
            if (files.Length == 0)
            {
                failures.Add("no save fixtures in " + SaveFixtureFolder);
            }

            var problems = new List<string>();
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                bool read = SaveStore.TryDeserialize(File.ReadAllText(file), out SaveData save, out int version);
                if (name.StartsWith("damaged-"))
                {
                    if (read)
                    {
                        failures.Add($"save {name}: a damaged save was accepted");
                    }

                    continue;
                }

                if (!read)
                {
                    failures.Add($"save {name}: could not be read (version {version})");
                    continue;
                }

                problems.Clear();
                var progression = new Progression(catalog, save);
                if (!SaveValidator.Validate(progression.Save, catalog, problems))
                {
                    failures.Add($"save {name}: " + string.Join("; ", problems));
                }

                // Normalized saves must survive a write and a read unchanged.
                string written = SaveStore.Serialize(progression.Save);
                if (!SaveStore.TryDeserialize(written, out SaveData again)
                    || SaveStore.Serialize(new Progression(catalog, again).Save) != written)
                {
                    failures.Add($"save {name}: not stable across a write and a read");
                }
            }

            problems.Clear();
            if (!SaveValidator.Validate(new Progression(catalog, new SaveData()).Save, catalog, problems)
                || !SaveValidator.Validate(new Progression(catalog, Progression.MaximumLoadout(catalog)).Save, catalog, problems))
            {
                failures.Add("save: a fresh or maximum-loadout save fails the validator: " + string.Join("; ", problems));
            }
        }

        private static void CheckLevel(ClaybreakLevel level, List<string> failures)
        {
            if (!level.TryValidate(ClaybreakSceneBuilder.CrowdCapacity, ClaybreakSceneBuilder.PoolSize, out string error))
            {
                failures.Add($"{level.name}: {error}");
            }

            if (level.MaximumProjectiles > ClaybreakSceneBuilder.ProjectilePoolSize)
            {
                failures.Add($"{level.name}: {level.MaximumProjectiles} projectiles exceed the scene pool of {ClaybreakSceneBuilder.ProjectilePoolSize}");
            }
        }

        private static void CheckRuntimeCode(List<string> failures)
        {
            foreach (string folder in RuntimeCodeFolders)
            {
                CheckRuntimeCode(folder, failures);
            }
        }

        private static void CheckRuntimeCode(string folder, List<string> failures)
        {
            foreach (string file in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories))
            {
                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    string code = lines[i].Trim();
                    if (code.StartsWith("//") || code.StartsWith("///"))
                    {
                        continue;
                    }

                    if (ForbiddenRuntimeCalls.IsMatch(code) && !code.Contains(LoadTimeMarker))
                    {
                        failures.Add($"{file.Replace('\\', '/')}:{i + 1}: runtime code creates/destroys objects or queries physics: {code}");
                    }
                }
            }
        }

        // RT-6t.1/6t.2: every crowd shape stays within the budget (one mesh, 200-500 triangles, 16-bit, tint mask, on the
        // ground, front +Z, eyes only where wanted) and the toon lab scene exists for the measurement player. RT-6t.4: the
        // crowd materials and the run camera profile are the game's too.
        private static void CheckToonLab(List<string> failures)
        {
            foreach (UnitShapeAssets.Entry entry in UnitShapeAssets.All)
            {
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(entry.Path);
                if (mesh == null)
                {
                    failures.Add("missing " + entry.Path + " (run Claybreak > Build Scene)");
                    continue;
                }

                foreach (string problem in UnitShapeRules.Check(mesh, entry.HasEyes, entry.FrontMarker))
                {
                    failures.Add(entry.Path + ": " + problem);
                }
            }

            // A shader that fails to compile renders magenta without any run-time error (RT-6t.1 lost a device run to
            // this): every toon lab material must use a shader that compiles.
            foreach (string path in new[] { ToonLabBuilder.SwarmMaterialPath, ToonLabBuilder.StaticMaterialPath, ToonLabBuilder.ShadowMaterialPath })
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null || material.shader == null)
                {
                    failures.Add("missing " + path + " or its shader (run Claybreak > Build Scene)");
                }
                else if (ShaderUtil.ShaderHasError(material.shader))
                {
                    failures.Add($"{material.shader.name} ({path}) has compile errors (see the Editor log)");
                }
            }

            // RT-6t.3: the lab and run cameras are framed by data (portrait fit), so the profiles must be there and coherent.
            foreach (string path in new[] { ToonLabBuilder.CameraProfilePath, ClaybreakSceneBuilder.CrowdCameraProfilePath })
            {
                var cameraProfile = AssetDatabase.LoadAssetAtPath<SwarmCameraProfile>(path);
                if (cameraProfile == null)
                {
                    failures.Add("missing " + path + " (run Claybreak > Build Scene)");
                }
                else if (cameraProfile.MaxVerticalFov < cameraProfile.VerticalFov || cameraProfile.Growth <= 0f)
                {
                    failures.Add(path + ": the widest field of view must be at least the base one, and growth positive");
                }
            }

            var look = AssetDatabase.LoadAssetAtPath<ToonLook>(ClaybreakSceneBuilder.CrowdLookPath);
            string lookError = look == null ? "missing (run Claybreak > Build Scene)" : look.Validate();
            if (lookError != null)
            {
                failures.Add(ClaybreakSceneBuilder.CrowdLookPath + ": " + lookError);
            }

            if (!File.Exists(Claybreak.Lab.ToonLab.ScenePath))
            {
                failures.Add("missing " + Claybreak.Lab.ToonLab.ScenePath + " (run Claybreak > Build Scene)");
            }
        }

        // RT-6t.4: the run crowd is the instanced swarm (no leader, no object per unit): capped at the crowd cap (D27), kept on
        // the track, drawn with the theme's shape (D25 is data) and the shared crowd materials, followed by the camera rig,
        // with the toon look on the scene. The kit's per-unit crowd must be gone from the run.
        private static void CheckCrowd(SerializedObject flow, List<string> failures)
        {
            if (Object.FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include) != null
                || Object.FindFirstObjectByType<RunnerCrowd>(FindObjectsInactive.Include) != null
                || Object.FindFirstObjectByType<CameraFollow>(FindObjectsInactive.Include) != null)
            {
                failures.Add("the run must not use the kit's leader, per-unit crowd or follow camera any more (RT-6t.4)");
            }

            var crowd = flow.FindProperty("crowd").objectReferenceValue as RunCrowd;
            var theme = AssetDatabase.LoadAssetAtPath<ClaybreakTheme>(ClaybreakSceneBuilder.ThemePath);
            if (crowd == null || theme == null)
            {
                return; // the unassigned reference is already reported
            }

            var crowdSo = new SerializedObject(crowd);
            var swarm = crowdSo.FindProperty("swarm").objectReferenceValue as SwarmRenderer;
            var rig = crowdSo.FindProperty("cameraRig").objectReferenceValue as SwarmCameraRig;
            if (swarm == null || rig == null)
            {
                failures.Add("the run crowd needs its swarm and camera rig");
                return;
            }

            if (swarm.Capacity != ClaybreakSceneBuilder.CrowdCapacity)
            {
                failures.Add($"the run crowd holds {swarm.Capacity} units; the crowd cap (D27) is {ClaybreakSceneBuilder.CrowdCapacity}");
            }

            if (!Mathf.Approximately(swarm.LaneHalfWidth, ClaybreakSceneBuilder.TrackWidth * 0.5f)
                || !Mathf.Approximately(swarm.transform.position.x, 0f))
            {
                failures.Add("the run crowd must be kept on the track (centered, half the track width)");
            }

            bool knownShape = false;
            foreach (UnitShapeAssets.Entry entry in UnitShapeAssets.All)
            {
                knownShape |= AssetDatabase.GetAssetPath(theme.CrowdShape) == entry.Path;
            }

            if (!knownShape || swarm.UnitMesh != theme.CrowdShape)
            {
                failures.Add("the run crowd must be drawn with the theme's crowd shape, one of the generated shapes");
            }

            var swarmSo = new SerializedObject(swarm);
            if (AssetDatabase.GetAssetPath(swarmSo.FindProperty("unitMaterial").objectReferenceValue) != ToonLabBuilder.SwarmMaterialPath
                || AssetDatabase.GetAssetPath(swarmSo.FindProperty("shadowMaterial").objectReferenceValue) != ToonLabBuilder.ShadowMaterialPath
                || AssetDatabase.GetAssetPath(swarmSo.FindProperty("shadowMesh").objectReferenceValue) != ToonLabBuilder.ShadowDiscPath)
            {
                failures.Add("the run crowd must use the crowd materials and the shadow disc");
            }

            if (AssetDatabase.GetAssetPath(new SerializedObject(rig).FindProperty("profile").objectReferenceValue)
                != ClaybreakSceneBuilder.CrowdCameraProfilePath || rig.GetComponent<Camera>() == null || !rig.CompareTag("MainCamera"))
            {
                failures.Add("the main camera needs the swarm rig with the run camera profile");
            }

            ToonLookApplier applier = Object.FindFirstObjectByType<ToonLookApplier>();
            if (applier == null || AssetDatabase.GetAssetPath(applier.Look) != ClaybreakSceneBuilder.CrowdLookPath)
            {
                failures.Add("the scene needs the toon look applier with " + ClaybreakSceneBuilder.CrowdLookPath);
            }
        }

        private static void CheckScene(List<string> failures)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Length != 1 || scenes[0].path != ClaybreakSceneBuilder.ScenePath || !scenes[0].enabled)
            {
                failures.Add("build settings must list only " + ClaybreakSceneBuilder.ScenePath);
            }

            if (!File.Exists(ClaybreakSceneBuilder.ScenePath))
            {
                failures.Add("missing scene " + ClaybreakSceneBuilder.ScenePath);
                return;
            }

            var previous = EditorSceneManager.GetActiveScene().path;
            var scene = EditorSceneManager.OpenScene(ClaybreakSceneBuilder.ScenePath, OpenSceneMode.Single);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            if (flow == null)
            {
                failures.Add("the scene has no ClaybreakFlow");
                return;
            }

            var so = new SerializedObject(flow);
            SerializedProperty property = so.GetIterator();
            property.NextVisible(true);
            while (property.NextVisible(false))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null)
                {
                    failures.Add("ClaybreakFlow." + property.name + " is not assigned");
                }
            }

            CheckCrowd(so, failures);

            // No physics in the game: only the pool units of the assault (kit prefab) and the disabled unit grounding
            // carry colliders.
            if (Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length
                > ClaybreakSceneBuilder.PoolSize + 1)
            {
                failures.Add("only the pool units and the unit grounding may carry colliders (no physics in the game)");
            }

            var grounding = so.FindProperty("unitGrounding").objectReferenceValue as CharacterController;
            if (grounding != null && grounding.enabled)
            {
                failures.Add("the unit grounding must be a disabled CharacterController (it only gives the units' size)");
            }

            var pool = Object.FindFirstObjectByType<RunnerPool>(FindObjectsInactive.Include);
            if (pool == null || pool.Capacity != ClaybreakSceneBuilder.PoolSize)
            {
                failures.Add($"the scene needs a RunnerPool with {ClaybreakSceneBuilder.PoolSize} preallocated units");
            }

            var cannon = Object.FindFirstObjectByType<CannonDefense>();
            if (cannon == null || cannon.PoolSize != ClaybreakSceneBuilder.ProjectilePoolSize)
            {
                failures.Add($"the cannon needs {ClaybreakSceneBuilder.ProjectilePoolSize} preallocated projectiles");
            }

            var gates = Object.FindFirstObjectByType<RunGateCourse>();
            if (gates == null || gates.Capacity != RunGateCourse.MaximumPairs || gates.CrateCapacity != ClaybreakLevel.MaximumCrates)
            {
                failures.Add($"the run needs {RunGateCourse.MaximumPairs} gate pairs and {ClaybreakLevel.MaximumCrates} crates preallocated");
            }

            var tower = Object.FindFirstObjectByType<AssaultTower>();
            string towerError = "missing";
            if (tower == null || !tower.TryValidate(out towerError))
            {
                failures.Add("tower: " + towerError);
            }

            var wall = Object.FindFirstObjectByType<AssaultWall>(FindObjectsInactive.Include);
            var sweeper = Object.FindFirstObjectByType<SweeperDefense>(FindObjectsInactive.Include);
            var shield = Object.FindFirstObjectByType<ShieldDefense>(FindObjectsInactive.Include);
            if (wall == null || !wall.HasParts || sweeper == null || !sweeper.HasParts || shield == null || !shield.HasParts)
            {
                failures.Add("the wall, the sweeper and the shield need their parts");
            }

            var worldCatalog = AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(WorldContent.CatalogPath);
            var scenery = Object.FindFirstObjectByType<WorldScenery>(FindObjectsInactive.Include);
            string sceneryError = "missing";
            if (scenery == null || worldCatalog == null || !scenery.TryValidate(worldCatalog.Worlds.Count, out sceneryError))
            {
                failures.Add("scenery: " + sceneryError);
            }

            var hud = Object.FindFirstObjectByType<ClaybreakHud>(FindObjectsInactive.Include);
            var screens = Object.FindFirstObjectByType<ClaybreakScreens>(FindObjectsInactive.Include);
            var catalog = AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(WorldContent.CatalogPath);
            if (hud == null || !hud.HasParts || screens == null || !screens.HasParts
                || screens.DotCapacity != ClaybreakSceneBuilder.LevelDots)
            {
                failures.Add("the HUD and the menu/result screens need every part");
            }
            else if (catalog != null && (screens.WorldRowCapacity != catalog.Worlds.Count
                || screens.ShopRowCapacity != Mathf.Max(catalog.Upgrades.Count, Mathf.Max(catalog.Units.Count, catalog.Skins.Count))))
            {
                failures.Add("the shop and world lists need one row per catalog entry (rebuild the scene after a catalog change)");
            }

            if (scene.isDirty)
            {
                failures.Add("opening the scene changed it (rebuild the scene)");
            }

            if (!string.IsNullOrEmpty(previous) && previous != scene.path)
            {
                EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
            }
        }
    }
}
