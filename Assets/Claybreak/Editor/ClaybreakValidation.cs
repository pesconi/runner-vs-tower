using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using JungleDegree.StudioKit.Crowd;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The game's validator: the meta catalog (RT-5) and every world and level against the pool, the
    /// device-gate level (heaviest tower, every defense at its limit), the theme, the save validator over
    /// the fixture saves (every version a phone may hold must migrate and pass <see cref="SaveValidator"/>),
    /// the scene wiring (tower bodies, HUD, screens with one row per catalog entry, sounds, preallocated
    /// pools), and the code rules (no Instantiate/Destroy at run time, rule 2.2.1; no physics queries,
    /// rule 2.2.7).
    /// Batchmode: -executeMethod Claybreak.Editor.ClaybreakValidation.RunAndQuit
    /// </summary>
    public static class ClaybreakValidation
    {
        private const string RuntimeFolder = "Assets/Claybreak/Runtime";
        public const string SaveFixtureFolder = "Assets/Claybreak/Editor/SaveFixtures";
        private static readonly Regex ForbiddenRuntimeCalls = new Regex(
            @"\b(Instantiate|Destroy|DestroyImmediate)\s*\(|new\s+GameObject\s*\(|\.AddComponent\s*<|\bPhysics\.",
            RegexOptions.Compiled);

        [MenuItem("Claybreak/Validate")]
        public static void ValidateFromMenu()
        {
            Report(Run());
        }

        public static void RunAndQuit()
        {
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
            return failures;
        }

        private static void Report(List<string> failures)
        {
            if (failures.Count == 0)
            {
                Debug.Log("PASS Claybreak validation: catalog, worlds, levels, device-gate level, theme, saves, scene, pools and runtime code rules.");
            }
            else
            {
                Debug.LogError("FAIL Claybreak validation:\n" + string.Join("\n", failures));
            }
        }

        private static void CheckData(List<string> failures)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(WorldContent.CatalogPath);
            var stress = AssetDatabase.LoadAssetAtPath<ClaybreakLevel>(WorldContent.StressLevelPath);
            var theme = AssetDatabase.LoadAssetAtPath<ClaybreakTheme>(ClaybreakSceneBuilder.ThemePath);
            if (catalog == null || stress == null || theme == null)
            {
                failures.Add("missing catalog, device-gate level or theme (run Claybreak > Build Scene)");
                return;
            }

            if (!catalog.TryValidate(ClaybreakSceneBuilder.PoolSize, out string error))
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

            foreach (WorldEntry entry in catalog.Worlds)
            {
                ClaybreakWorld world = entry.world;
                if (world.LevelCount > ClaybreakSceneBuilder.LevelDots)
                {
                    failures.Add($"{world.name}: {world.LevelCount} levels but the menu has {ClaybreakSceneBuilder.LevelDots} dots");
                }

                var kinds = new HashSet<TowerKind>();
                for (int i = 0; i < world.LevelCount; i++)
                {
                    ClaybreakLevel level = world.Levels[i];
                    CheckLevel(level, failures);
                    kinds.Add(level.Tower);
                    if (level.RunGates.Count < 3 || level.RunGates.Count > RunGateCourse.MaximumPairs)
                    {
                        failures.Add($"{level.name}: 3-{RunGateCourse.MaximumPairs} run gate pairs, has {level.RunGates.Count}");
                    }
                }

                if (kinds.Count < 3)
                {
                    failures.Add($"{world.name}: a world shows 2-3 towers and a boss; its levels use {kinds.Count} tower kinds");
                }
            }

            CheckLevel(stress, failures);
            if (!stress.IsDeviceGateLevel || stress.StartCount != ClaybreakSceneBuilder.PoolSize
                || stress.MaximumUnitsInFlight != ClaybreakSceneBuilder.PoolSize)
            {
                failures.Add($"{stress.name}: the device-gate level must auto-launch with {ClaybreakSceneBuilder.PoolSize} units in flight");
            }

            if (stress.Tower != TowerKind.Boss || !stress.CannonEnabled || !stress.SweeperEnabled || !stress.WallEnabled
                || stress.MaximumProjectiles < 12)
            {
                failures.Add($"{stress.name}: the device gate runs the heaviest tower (boss) with every defense on and 12 projectiles");
            }

            if (!theme.TryValidate(out error))
            {
                failures.Add("theme: " + error);
            }
        }

        // The save validator (RT-5 acceptance) over every fixture: real-format saves of each version.
        private static void CheckSaves(List<string> failures)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(WorldContent.CatalogPath);
            if (catalog == null || !catalog.TryValidate(ClaybreakSceneBuilder.PoolSize, out _))
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
            if (!level.TryValidate(ClaybreakSceneBuilder.PoolSize, out string error))
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
            foreach (string file in Directory.GetFiles(RuntimeFolder, "*.cs", SearchOption.AllDirectories))
            {
                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    string code = lines[i].Trim();
                    if (code.StartsWith("//") || code.StartsWith("///"))
                    {
                        continue;
                    }

                    if (ForbiddenRuntimeCalls.IsMatch(code))
                    {
                        failures.Add($"{file.Replace('\\', '/')}:{i + 1}: runtime code creates/destroys objects or queries physics: {code}");
                    }
                }
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

            // The kit's leader is a CharacterController with gravity: it needs a collider to stand on.
            var track = so.FindProperty("track").objectReferenceValue as Renderer;
            var leader = Object.FindFirstObjectByType<PlayerController>();
            if (track == null || !track.TryGetComponent(out Collider ground) || !ground.enabled)
            {
                failures.Add("the track needs an enabled collider (the leader would fall through it)");
            }
            else if (leader == null || !ground.bounds.Contains(new Vector3(leader.transform.position.x,
                         ground.bounds.max.y - 0.01f, leader.transform.position.z)))
            {
                failures.Add("the leader does not start above the track collider");
            }

            if (Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length
                > 1 + ClaybreakSceneBuilder.PoolSize + 1)
            {
                failures.Add("only the track, the leader and the pool units may carry colliders (no physics in the game)");
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
            if (wall == null || !wall.HasParts || sweeper == null || !sweeper.HasParts)
            {
                failures.Add("the wall and the sweeper need their parts");
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

            var sfx = Object.FindFirstObjectByType<ClaybreakSfx>(FindObjectsInactive.Include);
            if (sfx == null || !sfx.HasEveryCue)
            {
                failures.Add("the game's sound cues need a clip each and preallocated voices");
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
