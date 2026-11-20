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
    /// The game's validator (the theme completeness check of the plan): the meta catalog (RT-5) and every
    /// world and level against the pool; per world (RT-6) its look, its own scenery and towers (no obvious
    /// visual repetition between worlds), a boss with a rule, a level ramp that never gets easier, and its
    /// device-gate level (the world's heaviest tower, every defense at its limit); the theme, the save validator over
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
            var theme = AssetDatabase.LoadAssetAtPath<ClaybreakTheme>(ClaybreakSceneBuilder.ThemePath);
            if (catalog == null || theme == null)
            {
                failures.Add("missing catalog or theme (run Claybreak > Build Scene)");
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

            if (catalog.Worlds.Count != WorldContent.Worlds.Length)
            {
                failures.Add($"catalog: {catalog.Worlds.Count} worlds, content defines {WorldContent.Worlds.Length} (rebuild the scene)");
            }

            var sceneries = new HashSet<SceneryStyle>();
            var skies = new HashSet<Color>();
            var towersSeen = new Dictionary<TowerKind, string>();
            for (int w = 0; w < catalog.Worlds.Count; w++)
            {
                ClaybreakWorld world = catalog.Worlds[w].world;
                CheckWorld(world, w, sceneries, skies, towersSeen, failures);
                var stress = AssetDatabase.LoadAssetAtPath<ClaybreakLevel>(WorldContent.StressLevelPath(w));
                if (stress == null)
                {
                    failures.Add($"{world.name}: missing device-gate level {WorldContent.StressLevelPath(w)}");
                    continue;
                }

                CheckStress(world, stress, failures);
            }

            if (!theme.TryValidate(out error))
            {
                failures.Add("theme: " + error);
            }
        }

        // One world: levels within the menu, a look of its own (scenery style and sky not shared with another
        // world), towers that no earlier world uses for its main tower or boss, 2+ kinds plus the boss,
        // health that never drops from one plain level to the next and a boss above them all.
        private static void CheckWorld(ClaybreakWorld world, int index, HashSet<SceneryStyle> sceneries, HashSet<Color> skies,
            Dictionary<TowerKind, string> towersSeen, List<string> failures)
        {
            if (world.LevelCount > ClaybreakSceneBuilder.LevelDots)
            {
                failures.Add($"{world.name}: {world.LevelCount} levels but the menu has {ClaybreakSceneBuilder.LevelDots} dots");
            }

            if (!world.Look.IsSet)
            {
                failures.Add($"{world.name}: no look (sky, ground, sun)");
            }
            else if (!sceneries.Add(world.Look.scenery) || !skies.Add(world.Look.sky))
            {
                failures.Add($"{world.name}: scenery {world.Look.scenery} or its sky repeats another world's (no obvious visual repetition)");
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

            if (!stress.IsDeviceGateLevel || stress.StartCount != ClaybreakSceneBuilder.PoolSize
                || stress.MaximumUnitsInFlight != ClaybreakSceneBuilder.PoolSize)
            {
                failures.Add($"{stress.name}: the device-gate level must auto-launch with {ClaybreakSceneBuilder.PoolSize} units in flight");
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
