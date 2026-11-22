using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>One world of the content: identity, look, difficulty profile, hand-made levels and unlock rule.</summary>
    public sealed class WorldDefinition
    {
        public string Id;
        public string Name;
        public WorldLook Look;
        public WorldRecipe Recipe;
        /// <summary>Hand-made levels by index (null entries are generated from the recipe).</summary>
        public LevelSpec[] FixedLevels;
        public int StarsToUnlock;
        public float HealthPerLoop = 0.25f;
        public float CoinsPerLoop = 0.25f;

        /// <summary>Art options compared for the user (docs/art/rt6); index 0 is the recommendation in use until approved.</summary>
        public WorldLook[] LookOptions;
        public string[] LookNames;
        public TowerStyle[] TowerOptions;
        public TowerStyle[] BossOptions;
        public int ChosenLook;
        public int ChosenTower;
        public int ChosenBoss;

        /// <summary>Sets <see cref="Look"/> from the chosen palette option.</summary>
        public WorldDefinition WithChosenLook()
        {
            Look = LookOptions[ChosenLook];
            return this;
        }
    }

    /// <summary>
    /// The game's content as code (RT-6): every world (identity, look, difficulty profile and the levels
    /// generated from it by <see cref="LevelGenerator"/>, plus a few hand-made ones), one device-gate level per
    /// world (its heaviest tower, its scenery, every defense at its limit) and the meta catalog (upgrades,
    /// special units, skins, world list). The scene builder rewrites the assets from here on every build, so
    /// tuning is a reviewed code change, not an Inspector edit that a rebuild loses. Prices are [P] (D19 A:
    /// the RT-5 table is the starting point; RT-6 calibrates world pay and the shop total together).
    /// </summary>
    public static class WorldContent
    {
        public const string DataFolder = ClaybreakSceneBuilder.Root + "/Data";
        public const string CatalogPath = DataFolder + "/Catalog.asset";

        /// <summary>World 1 keeps the slice's asset names (Level01..., World1, DeviceGateLevel).</summary>
        public static string WorldPath(int world) => $"{DataFolder}/World{world + 1}.asset";

        public static string LevelPath(int world, int index) =>
            world == 0 ? $"{DataFolder}/Level{index + 1:00}.asset" : $"{DataFolder}/World{world + 1}Level{index + 1:00}.asset";

        public static string StressLevelPath(int world) =>
            world == 0 ? $"{DataFolder}/DeviceGateLevel.asset" : $"{DataFolder}/DeviceGateLevelWorld{world + 1}.asset";

        /// <summary>
        /// Every world in catalog order, each look (and each art option's look) on the world's palette
        /// (<see cref="PaletteContent"/>; null before the palettes are first written).
        /// </summary>
        public static WorldDefinition[] Worlds
        {
            get
            {
                WorldDefinition[] worlds = { World1(), World2(), World3(), World4() };
                for (int w = 0; w < worlds.Length; w++)
                {
                    JungleDegree.StudioKit.Toon.ToonPalette palette = PaletteContent.Load(w);
                    worlds[w].Look.palette = palette;
                    for (int i = 0; worlds[w].LookOptions != null && i < worlds[w].LookOptions.Length; i++)
                    {
                        worlds[w].LookOptions[i].palette = palette;
                    }
                }

                return worlds;
            }
        }

        /// <summary>The palette asset of the world with id <paramref name="worldId"/> (null when there is none).</summary>
        public static JungleDegree.StudioKit.Toon.ToonPalette PaletteOf(string worldId)
        {
            WorldDefinition[] worlds = { World1(), World2(), World3(), World4() };
            for (int w = 0; w < worlds.Length; w++)
            {
                if (worlds[w].Id == worldId)
                {
                    return PaletteContent.Load(w);
                }
            }

            return null;
        }

        /// <summary>The art style in use for a world's own tower (TowerArt.StyleFor).</summary>
        public static TowerStyle ChosenTowerStyle(int world)
        {
            WorldDefinition definition = Worlds[world];
            return definition.TowerOptions[definition.ChosenTower];
        }

        /// <summary>The art style in use for a world's boss.</summary>
        public static TowerStyle ChosenBossStyle(int world)
        {
            WorldDefinition definition = Worlds[world];
            return definition.BossOptions[definition.ChosenBoss];
        }

        /// <summary>Writes every level, the device-gate levels, the worlds and the catalog. Returns the catalog.</summary>
        public static ClaybreakCatalog Write()
        {
            WorldDefinition[] definitions = Worlds;
            var worlds = new ClaybreakWorld[definitions.Length];
            for (int w = 0; w < definitions.Length; w++)
            {
                WorldDefinition definition = definitions[w];
                LevelSpec[] specs = LevelGenerator.GenerateWorld(definition.Recipe, definition.FixedLevels);
                var levels = new ClaybreakLevel[specs.Length];
                for (int i = 0; i < specs.Length; i++)
                {
                    levels[i] = WriteLevel(LevelPath(w, i), specs[i]);
                }

                var world = LoadOrCreate<ClaybreakWorld>(WorldPath(w));
                world.EditorAssign(definition.Id, definition.Name, levels, definition.Look, definition.Recipe.expectedDamage,
                    definition.HealthPerLoop, definition.CoinsPerLoop);
                EditorUtility.SetDirty(world);
                worlds[w] = world;
                WriteLevel(StressLevelPath(w), DeviceGateLevel(definition));
            }

            return WriteCatalog(worlds, definitions);
        }

        /// <summary>The generated specs of a world (reports and tests read them without writing assets).</summary>
        public static LevelSpec[] Specs(WorldDefinition definition) =>
            LevelGenerator.GenerateWorld(definition.Recipe, definition.FixedLevels);

        // ---------- world 1: the marches (RT-4 slice, 7 -> 10 levels) ----------

        // Levels 1-6 and the boss are the slice's, played and tuned on the OPPO (unchanged numbers); levels 7-9
        // are generated from the profile and ramp between level 6 and the boss.
        private static WorldDefinition World1()
        {
            var fixedLevels = new LevelSpec[10];

            // 1: learn the run and the hold-to-launch. Cannon only, slow. Good path: 10>20>60>80.
            fixedLevels[0] = Spec("LEVEL 1", 10, 5f, 78f,
                new[] { Pair(20f, Add(10), Mul(2)), Pair(40f, Mul(3), Sub(8)), Pair(60f, Sub(15), Add(20)) }, null,
                Row(Mul(2), Div(2), Mul(2)), TowerKind.Keep, 110, 1, 30f, 40,
                s => { Wall(s, false, 2, 18f, 40); Cannon(s, 3f, 1f, 1f, 2); });

            // 2: the RT-2 level (tested on the OPPO): the wall guards the x3 lane.
            fixedLevels[1] = Spec("LEVEL 2", 10, 5f, 96f,
                new[] { Pair(20f, Add(10), Mul(2)), Pair(40f, Mul(3), Sub(8)), Pair(60f, Sub(15), Add(20)), Pair(80f, Div(2), Mul(2)) }, null,
                Row(Mul(2), Div(2), Mul(3)), TowerKind.Keep, 250, 1, 32f, 50,
                s => { Wall(s, true, 2, 20f, 60); Cannon(s, 2.2f, 0.9f, 1.1f, 4); });

            // 3: the forge and the sweeper (third defense). Its first win unlocks the Brute.
            fixedLevels[2] = Spec("LEVEL 3", 12, 5.5f, 98f,
                new[] { Pair(18f, Mul(2), Add(8)), Pair(38f, Sub(10), Mul(3)), Pair(58f, Add(25), Div(2)), Pair(80f, Mul(2), Sub(20)) }, null,
                Row(Div(2), Mul(3), Mul(2)), TowerKind.Forge, 300, 2, 32f, 60,
                s => { Wall(s, false, 0, 22f, 60); Cannon(s, 2.3f, 0.9f, 1.1f, 4); Sweeper(s, 16f, 3.4f); });

            // 4: first crates; the wall now sits on the left x3 lane.
            fixedLevels[3] = Spec("LEVEL 4", 12, 5.5f, 100f,
                new[] { Pair(18f, Add(12), Sub(5)), Pair(38f, Mul(2), Mul(3)), Pair(60f, Sub(20), Add(15)), Pair(82f, Mul(2), Div(2)) },
                new[] { Crate(28f, true, 3), Crate(70f, false, 3) },
                Row(Mul(3), Div(2), Mul(2)), TowerKind.Keep, 320, 2, 32f, 70,
                s => { Wall(s, true, 0, 20f, 70); Cannon(s, 2f, 0.9f, 1.15f, 4); });

            // 5: forge with three floors, sweeper and a faster cannon.
            fixedLevels[4] = Spec("LEVEL 5", 14, 5.5f, 104f,
                new[]
                {
                    Pair(18f, Mul(2), Add(10)), Pair(38f, Add(20), Sub(15)), Pair(58f, Div(2), Mul(3)),
                    Pair(80f, Add(30), Mul(2)), Pair(94f, Sub(25), Add(10))
                },
                new[] { Crate(28f, false, 3), Crate(68f, true, 4) },
                Row(Mul(2), Mul(3), Div(2)), TowerKind.Forge, 440, 3, 34f, 85,
                s => { Wall(s, false, 1, 22f, 60); Cannon(s, 1.9f, 0.85f, 1.15f, 5); Sweeper(s, 17f, 3f); });

            // 6: every defense at once before the new levels.
            fixedLevels[5] = Spec("LEVEL 6", 15, 6f, 108f,
                new[]
                {
                    Pair(18f, Add(15), Mul(2)), Pair(38f, Mul(3), Div(2)), Pair(58f, Sub(30), Add(20)),
                    Pair(80f, Mul(2), Sub(10)), Pair(96f, Add(25), Div(3))
                },
                new[] { Crate(28f, true, 4), Crate(48f, false, 3), Crate(70f, true, 4) },
                Row(Mul(2), Div(2), Mul(3)), TowerKind.Keep, 470, 3, 34f, 100,
                s => { Wall(s, true, 2, 24f, 90); Cannon(s, 1.8f, 0.85f, 1.2f, 5); Sweeper(s, 16f, 3f); });

            // Boss (level 10): four floors; each fallen floor turns the gate row (GDD 4.4).
            fixedLevels[9] = Spec("BOSS", 16, 6f, 112f,
                new[]
                {
                    Pair(18f, Mul(2), Add(15)), Pair(38f, Add(25), Sub(10)), Pair(58f, Div(2), Mul(3)),
                    Pair(80f, Add(30), Mul(2)), Pair(100f, Sub(30), Add(20))
                },
                new[] { Crate(28f, false, 4), Crate(48f, true, 4), Crate(70f, false, 4), Crate(90f, true, 4) },
                Row(Mul(3), Div(2), Mul(2)), TowerKind.Boss, 600, 4, 36f, 200,
                s =>
                {
                    s.bossRule = BossRule.ShiftGates;
                    Wall(s, true, 1, 24f, 100);
                    Cannon(s, 1.6f, 0.85f, 1.2f, 6);
                    Sweeper(s, 17f, 2.8f);
                });

            return new WorldDefinition
            {
                Id = SaveMigration.V1WorldId,
                Name = "WORLD 1",
                // Approved art (D17): bastion and citadel; the forge is the world's second tower.
                TowerOptions = new[] { TowerArt.ChosenTower },
                BossOptions = new[] { TowerArt.ChosenBoss },
                FixedLevels = fixedLevels,
                StarsToUnlock = 0,
                LookNames = new[] { "dusk marches (RT-4)" },
                Look = new WorldLook
                {
                    trees = 65,
                    rocks = 29,
                    ambient = new Color(0.42f, 0.36f, 0.34f),
                    sun = new Color(1f, 0.86f, 0.7f),
                    sunIntensity = 1.2f,
                },
                Recipe = new WorldRecipe
                {
                    worldId = SaveMigration.V1WorldId,
                    seed = 101,
                    levelCount = 10,
                    mainTower = TowerKind.Keep,
                    secondTower = TowerKind.Forge,
                    secondTowerEvery = 3,
                    bossTower = TowerKind.Boss,
                    bossRule = BossRule.ShiftGates,
                    expectedDamage = 1f,
                    startFirst = 10,
                    startLast = 16,
                    runSpeedFirst = 5f,
                    runSpeedLast = 6f,
                    shareFirst = 0.5f,
                    shareLast = 0.62f,
                    bossShare = 0.67f,
                    addFirst = 8,
                    addLast = 30,
                    wallFrom = 1,
                    sweeperFrom = 2,
                    shieldFrom = -1,
                    cratesFrom = 3,
                    cannonIntervalFirst = 2.4f,
                    cannonIntervalLast = 1.6f,
                    projectilesFirst = 3,
                    projectilesLast = 6,
                    rewardFirst = 40,
                    rewardLast = 135,
                    bossReward = 200
                }
            };
        }

        // ---------- world 2: the mire (shield world) ----------

        // Swamp ruins at night. Teaches the shield from level 3; the boss raises it again at every floor.
        private static WorldDefinition World2()
        {
            return new WorldDefinition
            {
                Id = "world2",
                Name = "WORLD 2",
                StarsToUnlock = 10,
                LookNames = new[] { "night mire (teal, lantern green)", "amber bog (ochre, orange lamps)" },
                LookOptions = new[]
                {
                    new WorldLook
                    {
                        trees = 52,
                        rocks = 36,
                        ambient = new Color(0.3f, 0.38f, 0.36f),
                        sun = new Color(0.72f, 0.88f, 0.82f),
                        sunIntensity = 1.05f,
                    },
                    new WorldLook
                    {
                        trees = 52,
                        rocks = 36,
                        ambient = new Color(0.44f, 0.36f, 0.26f),
                        sun = new Color(1f, 0.78f, 0.5f),
                        sunIntensity = 1.15f,
                    }
                },
                TowerOptions = new[] { TowerStyle.StiltFort, TowerStyle.ClayRuin },
                BossOptions = new[] { TowerStyle.SunkenBell, TowerStyle.ClayTotem },
                Recipe = new WorldRecipe
                {
                    worldId = "world2",
                    seed = 202,
                    levelCount = 10,
                    mainTower = TowerKind.World2Tower,
                    secondTower = TowerKind.Forge,
                    secondTowerEvery = 4,
                    bossTower = TowerKind.World2Boss,
                    bossRule = BossRule.ShieldReturns,
                    expectedDamage = 1.3f,
                    startFirst = 14,
                    startLast = 20,
                    runSpeedFirst = 5.5f,
                    runSpeedLast = 6.5f,
                    shareFirst = 0.52f,
                    shareLast = 0.64f,
                    bossShare = 0.68f,
                    addFirst = 10,
                    addLast = 35,
                    wallFrom = 0,
                    sweeperFrom = 1,
                    shieldFrom = 2,
                    cratesFrom = 1,
                    cannonIntervalFirst = 2.2f,
                    cannonIntervalLast = 1.5f,
                    projectilesFirst = 4,
                    projectilesLast = 7,
                    shieldBlockFirst = 0.4f,
                    shieldBlockLast = 0.6f,
                    rewardFirst = 110,
                    rewardLast = 170,
                    bossReward = 350
                }
            }.WithChosenLook();
        }

        // ---------- world 3: the foundry (overheat boss) ----------

        // Ironworks at night. Harder cannons and sweepers; the boss heats its guns at every floor.
        private static WorldDefinition World3()
        {
            return new WorldDefinition
            {
                Id = "world3",
                Name = "WORLD 3",
                StarsToUnlock = 25,
                LookNames = new[] { "soot and ember (black-red, molten orange)", "smog (yellow haze, white-blue lamps)" },
                LookOptions = new[]
                {
                    new WorldLook
                    {
                        trees = 78,
                        rocks = 22,
                        ambient = new Color(0.4f, 0.3f, 0.28f),
                        sun = new Color(1f, 0.62f, 0.42f),
                        sunIntensity = 1.15f,
                    },
                    new WorldLook
                    {
                        trees = 78,
                        rocks = 22,
                        ambient = new Color(0.46f, 0.46f, 0.38f),
                        sun = new Color(0.95f, 0.95f, 0.8f),
                        sunIntensity = 1f,
                    }
                },
                // Palette B in use: A (soot and ember) repeats world 1's brown and orange (second opinion, D20).
                ChosenLook = 1,
                TowerOptions = new[] { TowerStyle.SmokestackMill, TowerStyle.Gasholder },
                BossOptions = new[] { TowerStyle.PistonHammer, TowerStyle.ForgeColossus },
                Recipe = new WorldRecipe
                {
                    worldId = "world3",
                    seed = 303,
                    levelCount = 10,
                    mainTower = TowerKind.World3Tower,
                    secondTower = TowerKind.World2Tower,
                    secondTowerEvery = 4,
                    bossTower = TowerKind.World3Boss,
                    bossRule = BossRule.Overheat,
                    expectedDamage = 1.5f,
                    startFirst = 18,
                    startLast = 24,
                    runSpeedFirst = 6f,
                    runSpeedLast = 7f,
                    shareFirst = 0.54f,
                    shareLast = 0.66f,
                    bossShare = 0.7f,
                    addFirst = 12,
                    addLast = 40,
                    wallFrom = 0,
                    sweeperFrom = 0,
                    shieldFrom = 3,
                    cratesFrom = 0,
                    cannonIntervalFirst = 1.9f,
                    cannonIntervalLast = 1.3f,
                    projectilesFirst = 5,
                    projectilesLast = 8,
                    sweeperPeriodFirst = 3.1f,
                    sweeperPeriodLast = 2.5f,
                    shieldBlockFirst = 0.45f,
                    shieldBlockLast = 0.6f,
                    rewardFirst = 170,
                    rewardLast = 240,
                    bossReward = 500
                }
            }.WithChosenLook();
        }

        // ---------- world 4: the peaks (turn-gate boss) ----------

        // A frozen range at night. Every defense from the start; the boss turns its best gate at every floor.
        private static WorldDefinition World4()
        {
            return new WorldDefinition
            {
                Id = "world4",
                Name = "WORLD 4",
                StarsToUnlock = 45,
                LookNames = new[] { "frost night (blue snow, ice light)", "ash peaks (black-violet, magenta glow)" },
                LookOptions = new[]
                {
                    new WorldLook
                    {
                        trees = 44,
                        rocks = 44,
                        ambient = new Color(0.4f, 0.45f, 0.56f),
                        sun = new Color(0.78f, 0.86f, 1f),
                        sunIntensity = 1f,
                    },
                    new WorldLook
                    {
                        trees = 44,
                        rocks = 44,
                        ambient = new Color(0.38f, 0.32f, 0.42f),
                        sun = new Color(1f, 0.78f, 0.9f),
                        sunIntensity = 1.05f,
                    }
                },
                TowerOptions = new[] { TowerStyle.FrostSpire, TowerStyle.BoneBastion },
                BossOptions = new[] { TowerStyle.CrownSpire, TowerStyle.BoneHorns },
                Recipe = new WorldRecipe
                {
                    worldId = "world4",
                    seed = 404,
                    levelCount = 10,
                    mainTower = TowerKind.World4Tower,
                    secondTower = TowerKind.World3Tower,
                    secondTowerEvery = 4,
                    bossTower = TowerKind.World4Boss,
                    bossRule = BossRule.TurnGate,
                    expectedDamage = 1.75f,
                    startFirst = 22,
                    startLast = 30,
                    runSpeedFirst = 6.5f,
                    runSpeedLast = 7.5f,
                    shareFirst = 0.56f,
                    shareLast = 0.68f,
                    bossShare = 0.72f,
                    addFirst = 15,
                    addLast = 45,
                    wallFrom = 0,
                    sweeperFrom = 0,
                    shieldFrom = 1,
                    cratesFrom = 0,
                    cannonIntervalFirst = 1.7f,
                    cannonIntervalLast = 1.15f,
                    projectilesFirst = 6,
                    projectilesLast = 9,
                    sweeperPeriodFirst = 2.9f,
                    sweeperPeriodLast = 2.3f,
                    shieldBlockFirst = 0.5f,
                    shieldBlockLast = 0.65f,
                    rewardFirst = 240,
                    rewardLast = 320,
                    bossReward = 700
                }
            }.WithChosenLook();
        }

        // ---------- device gate (reuso-tecnico 6.3), one per world ----------

        // The world's heaviest tower (its boss body), every defense the world has, 300 units in flight and 12
        // projectiles, held for as long as the measurement runs, in the world's scenery.
        private static LevelSpec DeviceGateLevel(WorldDefinition world)
        {
            bool shield = world.Recipe.shieldFrom >= 0 || world.Recipe.bossRule == BossRule.ShieldReturns;
            return new LevelSpec
            {
                title = "DEVICE GATE",
                // The crowd at its cap: the run gate measures it on the track (RT-6t.4), the assault gate launches it.
                startCount = ClaybreakSceneBuilder.CrowdCapacity,
                runSpeed = 12f,
                firingLineZ = 24f,
                runGates = new RunGatePair[0],
                launchRate = 150f,
                unitSpeed = 5f,
                laneSpread = 1.1f,
                assaultGates = Row(Mul(1), Mul(1), Mul(1)),
                autoLaunch = true,
                recycleUnits = true,
                tower = world.Recipe.bossTower,
                towerHealth = NumberLabels.CachedMaximum,
                floors = 4,
                towerDistance = 62f,
                bossRule = world.Recipe.bossRule,
                wallEnabled = true,
                wallLane = 2,
                wallDistance = 20f,
                wallHealth = 1,
                cannonEnabled = true,
                cannonInterval = 0.05f,
                projectileFlightTime = 1f,
                blastRadius = 1.2f,
                maximumProjectiles = 12,
                sweeperEnabled = true,
                sweeperDistance = 30f,
                sweeperPeriod = 3f,
                // The shield stands for the whole measurement: its dome is drawn over the tower all along.
                shieldEnabled = shield,
                shieldLane = 0,
                shieldDistance = 56f,
                shieldHealth = NumberLabels.CachedMaximum,
                shieldBlock = 0.5f
            };
        }

        // ---------- meta (RT-5), prices [P] (D19 A) ----------

        // Prices grow about 1.5x per step (docs/rt5-economia.md and docs/rt6-economia.md, written by EconomyReport).
        private static ClaybreakCatalog WriteCatalog(ClaybreakWorld[] worlds, WorldDefinition[] definitions)
        {
            var catalog = LoadOrCreate<ClaybreakCatalog>(CatalogPath);
            UpgradeTrack[] upgrades =
            {
                // The slice's starting-army track, unchanged: a migrated save keeps the same bonus.
                Track(SaveMigration.V1StartingArmyId, "ARMY", UpgradeKind.StartingArmy,
                    (60, 5), (120, 10), (200, 16), (320, 24), (480, 34), (700, 46)),
                Track("damage", "DAMAGE", UpgradeKind.Damage,
                    (80, 15), (150, 30), (250, 45), (400, 60), (600, 80), (900, 100)),
                Track("gates", "GATE BOOST", UpgradeKind.GateBonus,
                    (100, 1), (180, 2), (300, 3), (450, 5), (650, 7), (950, 10)),
                Track("launch", "LAUNCH SPEED", UpgradeKind.LaunchRate,
                    (70, 10), (130, 20), (220, 30), (350, 45), (520, 60), (750, 80))
            };
            UnitType[] units =
            {
                // The slice's Brute, unchanged: free when level 3 of world 1 falls.
                new UnitType
                {
                    id = SaveMigration.V1BruteId, title = "BRUTE", description = "Heavy hitter, slow to fall.",
                    color = new Color(0.5f, 0.14f, 0.1f), scale = 1.45f, damage = 4, armor = 1, speed = 1f,
                    price = 0, unlockWorld = SaveMigration.V1WorldId, unlockLevel = 2
                },
                new UnitType
                {
                    id = "sprinter", title = "SPRINTER", description = "Light and fast: first to the tower.",
                    color = new Color(0.98f, 0.82f, 0.22f), scale = 0.9f, damage = 2, armor = 0, speed = 1.6f,
                    price = 600
                },
                new UnitType
                {
                    id = "warden", title = "WARDEN", description = "Shield bearer: walks through blasts.",
                    color = new Color(0.33f, 0.52f, 0.78f), scale = 1.25f, damage = 2, armor = 3, speed = 0.85f,
                    price = 900
                }
            };
            var skins = new List<CrowdSkin>
            {
                // The free default wears each world's palette color (RT-6t.6a, D26 A); its own color is never drawn.
                new CrowdSkin { id = "clay", title = "WORLD COLOR", color = new Color(0.95f, 0.55f, 0.18f), price = 0 },
            };
            foreach (SkinDefinition skin in Skins)
            {
                skins.Add(new CrowdSkin { id = skin.Id, title = skin.Title, color = Hex(skin.Hex), price = skin.Price });
            }

            var shapes = new List<CrowdShapeOption>();
            foreach (ShapeDefinition shape in Shapes)
            {
                shapes.Add(new CrowdShapeOption { id = shape.Id, title = shape.Title, mesh = UnitShapeAssets.Load(shape.Asset) });
            }
            var entries = new WorldEntry[worlds.Length];
            for (int w = 0; w < worlds.Length; w++)
            {
                entries[w] = new WorldEntry { world = worlds[w], starsToUnlock = definitions[w].StarsToUnlock };
            }

            catalog.EditorAssign(entries, upgrades, units, skins.ToArray(), shapes.ToArray(), ClaybreakCatalog.MaximumLoadoutSlots, 3);
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        /// <summary>A paid crowd skin: stable save id, shop title, sRGB color and price in coins.</summary>
        public readonly struct SkinDefinition
        {
            public SkinDefinition(string id, string title, string hex, int price)
            {
                Id = id;
                Title = title;
                Hex = hex;
                Price = price;
            }

            public string Id { get; }
            public string Title { get; }
            public string Hex { get; }
            public int Price { get; }
        }

        /// <summary>
        /// The 8 paid skins (D26 A, RT-6t.6b): the ids, order and prices of RT-5/RT-6 (D19 A), with new pastel colors and names
        /// in the crowd-toon style. Each color stays apart (CIE76 delta E of 20 or more) from every palette's track, stripe and
        /// ground, from both gate colors and from the other skins; the validator warns if a change breaks that.
        /// </summary>
        public static readonly SkinDefinition[] Skins =
        {
            new SkinDefinition("ember", "CORAL", "#ff8a5c", 150),
            new SkinDefinition("moss", "MANGO", "#ffc94a", 300),
            new SkinDefinition("slate", "SKY", "#6fa8ff", 500),
            new SkinDefinition("bone", "LILAC", "#b98cf0", 800),
            new SkinDefinition("ink", "INDIGO", "#3d3a7c", 1200),
            new SkinDefinition("mire", "LAGOON", "#2fb3c4", 1500),
            new SkinDefinition("cinder", "BERRY", "#e0569b", 2000),
            new SkinDefinition("frost", "BUBBLEGUM", "#ff9ed2", 2500),
        };

        /// <summary>A crowd shape on offer: stable save id, title on the home screen and the generated mesh it draws.</summary>
        public readonly struct ShapeDefinition
        {
            public ShapeDefinition(string id, string title, UnitShapeAssets.Entry asset)
            {
                Id = id;
                Title = title;
                Asset = asset;
            }

            public string Id { get; }
            public string Title { get; }
            public UnitShapeAssets.Entry Asset { get; }
        }

        /// <summary>
        /// The free crowd shapes (D26 A), both with eyes (D25 A; the eyeless meshes stay generated as the reserve). The first
        /// is the default and the shape every save before version 4 was drawn with (<see cref="SaveMigration.ShapeBeforeV4"/>).
        /// </summary>
        public static readonly ShapeDefinition[] Shapes =
        {
            new ShapeDefinition(SaveMigration.ShapeBeforeV4, "BALL", UnitShapeAssets.All[0]),
            new ShapeDefinition("chick", "CHICK", UnitShapeAssets.All[2]),
        };

        private static Color Hex(string hex)
        {
            if (!ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                throw new System.FormatException($"'{hex}' is not a color");
            }

            return color;
        }

        private static UpgradeTrack Track(string id, string title, UpgradeKind kind, params (int cost, int bonus)[] steps)
        {
            var track = new UpgradeTrack { id = id, title = title, kind = kind, steps = new UpgradeStep[steps.Length] };
            for (int i = 0; i < steps.Length; i++)
            {
                track.steps[i] = new UpgradeStep { cost = steps[i].cost, bonus = steps[i].bonus };
            }

            return track;
        }

        // ---------- helpers ----------

        private static ClaybreakLevel WriteLevel(string path, LevelSpec spec)
        {
            var level = LoadOrCreate<ClaybreakLevel>(path);
            level.name = System.IO.Path.GetFileNameWithoutExtension(path);
            level.EditorAssign(spec);
            EditorUtility.SetDirty(level);
            return level;
        }

        private static LevelSpec Spec(string title, int start, float speed, float firingLine, RunGatePair[] pairs,
            CratePlacement[] crates, GateSpec[] row, TowerKind tower, int health, int floors, float towerDistance, int reward,
            System.Action<LevelSpec> defenses)
        {
            var spec = new LevelSpec
            {
                title = title,
                startCount = start,
                runSpeed = speed,
                firingLineZ = firingLine,
                runGates = pairs,
                crates = crates ?? new CratePlacement[0],
                assaultGates = row,
                tower = tower,
                towerHealth = health,
                floors = floors,
                towerDistance = towerDistance,
                coinReward = reward
            };
            defenses(spec);
            return spec;
        }

        private static void Wall(LevelSpec s, bool enabled, int lane, float distance, int health)
        {
            s.wallEnabled = enabled;
            s.wallLane = lane;
            s.wallDistance = distance;
            s.wallHealth = health;
        }

        private static void Cannon(LevelSpec s, float interval, float flight, float radius, int projectiles)
        {
            s.cannonEnabled = true;
            s.cannonInterval = interval;
            s.projectileFlightTime = flight;
            s.blastRadius = radius;
            s.maximumProjectiles = projectiles;
        }

        private static void Sweeper(LevelSpec s, float distance, float period)
        {
            s.sweeperEnabled = true;
            s.sweeperDistance = distance;
            s.sweeperPeriod = period;
        }

        private static GateSpec[] Row(GateSpec a, GateSpec b, GateSpec c) => new[] { a, b, c };
        private static GateSpec Add(int v) => new GateSpec(GateOperation.Add, v);
        private static GateSpec Sub(int v) => new GateSpec(GateOperation.Subtract, v);
        private static GateSpec Mul(int v) => new GateSpec(GateOperation.Multiply, v);
        private static GateSpec Div(int v) => new GateSpec(GateOperation.Divide, v);
        private static RunGatePair Pair(float z, GateSpec left, GateSpec right) => new RunGatePair { z = z, left = left, right = right };
        private static CratePlacement Crate(float z, bool right, int units) => new CratePlacement { z = z, right = right, units = units };

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }

            return asset;
        }
    }
}
