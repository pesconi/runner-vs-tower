using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// World 1 of the vertical slice (RT-4) as code: 7 levels (the last is the boss), the starting-army
    /// upgrade track, the Brute, and the device-gate level. The scene builder rewrites the assets from here
    /// on every build, so tuning is a reviewed code change (not an Inspector edit that a rebuild loses).
    /// Session target (GDD 3, RT-4): about 35-50 s per level, 7 levels plus menus in about 5 minutes.
    /// </summary>
    public static class WorldContent
    {
        public const string DataFolder = ClaybreakSceneBuilder.Root + "/Data";
        public const string WorldPath = DataFolder + "/World1.asset";
        public const string StressLevelPath = DataFolder + "/DeviceGateLevel.asset";
        public const int LevelCount = 7;

        public static string LevelPath(int index) => $"{DataFolder}/Level{index + 1:00}.asset";

        private static GateSpec Add(int v) => new GateSpec(GateOperation.Add, v);
        private static GateSpec Sub(int v) => new GateSpec(GateOperation.Subtract, v);
        private static GateSpec Mul(int v) => new GateSpec(GateOperation.Multiply, v);
        private static GateSpec Div(int v) => new GateSpec(GateOperation.Divide, v);

        private static RunGatePair Pair(float z, GateSpec left, GateSpec right) =>
            new RunGatePair { z = z, left = left, right = right };

        private static CratePlacement Crate(float z, bool right, int units) =>
            new CratePlacement { z = z, right = right, units = units };

        /// <summary>Writes every level, the device-gate level and the world asset. Returns the world.</summary>
        public static ClaybreakWorld Write()
        {
            var levels = new ClaybreakLevel[LevelCount];

            // 1: learn the run and the hold-to-launch. Cannon only, slow. Good path: 10>20>60>80.
            levels[0] = Level(0, "LEVEL 1", s =>
            {
                Run(s, 10, 5f, 78f,
                    new[] { Pair(20f, Add(10), Mul(2)), Pair(40f, Mul(3), Sub(8)), Pair(60f, Sub(15), Add(20)) });
                Row(s, Mul(2), Div(2), Mul(2));
                Tower(s, TowerKind.Keep, 110, 1, 30f);
                Wall(s, false, 2, 18f, 40);
                Cannon(s, true, 3f, 1f, 1f, 2);
                Sweeper(s, false, 14f, 3.4f);
                Set(s, "coinReward", 40);
            });

            // 2: the RT-2 level (tested on the OPPO): the wall guards the x3 lane.
            levels[1] = Level(1, "LEVEL 2", s =>
            {
                Run(s, 10, 5f, 96f, new[]
                {
                    Pair(20f, Add(10), Mul(2)), Pair(40f, Mul(3), Sub(8)), Pair(60f, Sub(15), Add(20)),
                    Pair(80f, Div(2), Mul(2))
                });
                Row(s, Mul(2), Div(2), Mul(3));
                Tower(s, TowerKind.Keep, 250, 1, 32f);
                Wall(s, true, 2, 20f, 60);
                Cannon(s, true, 2.2f, 0.9f, 1.1f, 4);
                Sweeper(s, false, 14f, 3.4f);
                Set(s, "coinReward", 50);
            });

            // 3: the forge and the sweeper (third defense). Its first win unlocks the Brute.
            levels[2] = Level(2, "LEVEL 3", s =>
            {
                Run(s, 12, 5.5f, 98f, new[]
                {
                    Pair(18f, Mul(2), Add(8)), Pair(38f, Sub(10), Mul(3)), Pair(58f, Add(25), Div(2)),
                    Pair(80f, Mul(2), Sub(20))
                });
                Row(s, Div(2), Mul(3), Mul(2));
                Tower(s, TowerKind.Forge, 300, 2, 32f);
                Wall(s, false, 0, 22f, 60);
                Cannon(s, true, 2.3f, 0.9f, 1.1f, 4);
                Sweeper(s, true, 16f, 3.4f);
                Set(s, "coinReward", 60);
            });

            // 4: first crates; the wall now sits on the left x3 lane.
            levels[3] = Level(3, "LEVEL 4", s =>
            {
                Run(s, 12, 5.5f, 100f, new[]
                {
                    Pair(18f, Add(12), Sub(5)), Pair(38f, Mul(2), Mul(3)), Pair(60f, Sub(20), Add(15)),
                    Pair(82f, Mul(2), Div(2))
                }, new[] { Crate(28f, true, 3), Crate(70f, false, 3) });
                Row(s, Mul(3), Div(2), Mul(2));
                Tower(s, TowerKind.Keep, 320, 2, 32f);
                Wall(s, true, 0, 20f, 70);
                Cannon(s, true, 2f, 0.9f, 1.15f, 4);
                Sweeper(s, false, 14f, 3.4f);
                Set(s, "coinReward", 70);
            });

            // 5: forge with three floors, sweeper and a faster cannon.
            levels[4] = Level(4, "LEVEL 5", s =>
            {
                Run(s, 14, 5.5f, 104f, new[]
                {
                    Pair(18f, Mul(2), Add(10)), Pair(38f, Add(20), Sub(15)), Pair(58f, Div(2), Mul(3)),
                    Pair(80f, Add(30), Mul(2)), Pair(94f, Sub(25), Add(10))
                }, new[] { Crate(28f, false, 3), Crate(68f, true, 4) });
                Row(s, Mul(2), Mul(3), Div(2));
                Tower(s, TowerKind.Forge, 440, 3, 34f);
                Wall(s, false, 1, 22f, 60);
                Cannon(s, true, 1.9f, 0.85f, 1.15f, 5);
                Sweeper(s, true, 17f, 3f);
                Set(s, "coinReward", 85);
            });

            // 6: every defense at once before the boss.
            levels[5] = Level(5, "LEVEL 6", s =>
            {
                Run(s, 15, 6f, 108f, new[]
                {
                    Pair(18f, Add(15), Mul(2)), Pair(38f, Mul(3), Div(2)), Pair(58f, Sub(30), Add(20)),
                    Pair(80f, Mul(2), Sub(10)), Pair(96f, Add(25), Div(3))
                }, new[] { Crate(28f, true, 4), Crate(48f, false, 3), Crate(70f, true, 4) });
                Row(s, Mul(2), Div(2), Mul(3));
                Tower(s, TowerKind.Keep, 470, 3, 34f);
                Wall(s, true, 2, 24f, 90);
                Cannon(s, true, 1.8f, 0.85f, 1.2f, 5);
                Sweeper(s, true, 16f, 3f);
                Set(s, "coinReward", 100);
            });

            // 7: the boss. Four floors; each fallen floor turns the gate row (GDD 4.4).
            levels[6] = Level(6, "BOSS", s =>
            {
                Run(s, 16, 6f, 112f, new[]
                {
                    Pair(18f, Mul(2), Add(15)), Pair(38f, Add(25), Sub(10)), Pair(58f, Div(2), Mul(3)),
                    Pair(80f, Add(30), Mul(2)), Pair(100f, Sub(30), Add(20))
                }, new[] { Crate(28f, false, 4), Crate(48f, true, 4), Crate(70f, false, 4), Crate(90f, true, 4) });
                Row(s, Mul(3), Div(2), Mul(2));
                Tower(s, TowerKind.Boss, 600, 4, 36f);
                Set(s, "shiftGatesOnFloor", true);
                Wall(s, true, 1, 24f, 100);
                Cannon(s, true, 1.6f, 0.85f, 1.2f, 6);
                Sweeper(s, true, 17f, 2.8f);
                Set(s, "coinReward", 200);
            });

            WriteDeviceGateLevel();
            return WriteWorld(levels);
        }

        // Device gate (reuso-tecnico 6.3): the heaviest tower (boss body), every defense on, 300 units in
        // flight and 12 projectiles, held for as long as the measurement runs.
        private static void WriteDeviceGateLevel()
        {
            Level(StressLevelPath, "DEVICE GATE", s =>
            {
                Run(s, ClaybreakSceneBuilder.PoolSize, 12f, 24f, new RunGatePair[0]);
                Set(s, "launchRate", 150f);
                Set(s, "unitSpeed", 5f);
                Set(s, "laneSpread", 1.1f);
                Row(s, Mul(1), Mul(1), Mul(1));
                Set(s, "autoLaunch", true);
                Set(s, "recycleUnits", true);
                Tower(s, TowerKind.Boss, NumberLabels.CachedMaximum, 4, 62f);
                Wall(s, true, 2, 20f, 1);
                Cannon(s, true, 0.05f, 1f, 1.2f, 12);
                Sweeper(s, true, 30f, 3f);
            });
        }

        private static ClaybreakWorld WriteWorld(ClaybreakLevel[] levels)
        {
            var world = LoadOrCreate<ClaybreakWorld>(WorldPath);
            var so = new SerializedObject(world);
            so.FindProperty("worldName").stringValue = "WORLD 1";
            SerializedProperty array = so.FindProperty("levels");
            array.arraySize = levels.Length;
            for (int i = 0; i < levels.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];
            }

            so.FindProperty("healthPerLoop").floatValue = 0.25f;
            (int cost, int bonus)[] steps = { (60, 5), (120, 10), (200, 16), (320, 24), (480, 34), (700, 46) };
            SerializedProperty upgrade = so.FindProperty("startingArmySteps");
            upgrade.arraySize = steps.Length;
            for (int i = 0; i < steps.Length; i++)
            {
                upgrade.GetArrayElementAtIndex(i).FindPropertyRelative("cost").intValue = steps[i].cost;
                upgrade.GetArrayElementAtIndex(i).FindPropertyRelative("bonus").intValue = steps[i].bonus;
            }

            so.FindProperty("bruteUnlockLevel").intValue = 2;
            so.FindProperty("bruteDamage").intValue = 4;
            so.FindProperty("bruteArmor").intValue = 1;
            so.FindProperty("bruteColor").colorValue = new Color(0.5f, 0.14f, 0.1f);
            so.FindProperty("bruteScale").floatValue = 1.45f;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(world);
            return world;
        }

        // ---------- helpers ----------

        private static ClaybreakLevel Level(int index, string title, System.Action<SerializedObject> fill)
        {
            return Level(LevelPath(index), title, fill);
        }

        private static ClaybreakLevel Level(string path, string title, System.Action<SerializedObject> fill)
        {
            var level = LoadOrCreate<ClaybreakLevel>(path);
            // Start from the defaults every time, so a field removed from a level does not linger.
            var defaults = ScriptableObject.CreateInstance<ClaybreakLevel>();
            EditorUtility.CopySerialized(defaults, level);
            Object.DestroyImmediate(defaults);
            level.name = System.IO.Path.GetFileNameWithoutExtension(path);
            var so = new SerializedObject(level);
            so.FindProperty("title").stringValue = title;
            Set(so, "launchRate", 12f);
            Set(so, "unitSpeed", 9f);
            Set(so, "laneSpread", 0.7f);
            Set(so, "maximumUnitsInFlight", ClaybreakSceneBuilder.PoolSize);
            Set(so, "assaultGateDistance", 9f);
            Set(so, "damagePerUnit", 1);
            fill(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
            return level;
        }

        private static void Run(SerializedObject so, int start, float speed, float firingLine, RunGatePair[] pairs,
            CratePlacement[] crates = null)
        {
            Set(so, "startCount", start);
            Set(so, "runSpeed", speed);
            Set(so, "firingLineZ", firingLine);
            SerializedProperty array = so.FindProperty("runGates");
            array.arraySize = pairs.Length;
            for (int i = 0; i < pairs.Length; i++)
            {
                SerializedProperty pair = array.GetArrayElementAtIndex(i);
                pair.FindPropertyRelative("z").floatValue = pairs[i].z;
                SetGate(pair.FindPropertyRelative("left"), pairs[i].left);
                SetGate(pair.FindPropertyRelative("right"), pairs[i].right);
            }

            crates ??= new CratePlacement[0];
            SerializedProperty crateArray = so.FindProperty("crates");
            crateArray.arraySize = crates.Length;
            for (int i = 0; i < crates.Length; i++)
            {
                SerializedProperty crate = crateArray.GetArrayElementAtIndex(i);
                crate.FindPropertyRelative("z").floatValue = crates[i].z;
                crate.FindPropertyRelative("right").boolValue = crates[i].right;
                crate.FindPropertyRelative("units").intValue = crates[i].units;
            }
        }

        private static void Row(SerializedObject so, GateSpec a, GateSpec b, GateSpec c)
        {
            SerializedProperty array = so.FindProperty("assaultGates");
            array.arraySize = ClaybreakLevel.LaneCount;
            SetGate(array.GetArrayElementAtIndex(0), a);
            SetGate(array.GetArrayElementAtIndex(1), b);
            SetGate(array.GetArrayElementAtIndex(2), c);
        }

        private static void Tower(SerializedObject so, TowerKind kind, int health, int floors, float distance)
        {
            so.FindProperty("tower").enumValueIndex = (int)kind;
            Set(so, "towerHealth", health);
            Set(so, "floors", floors);
            Set(so, "towerDistance", distance);
        }

        private static void Wall(SerializedObject so, bool enabled, int lane, float distance, int health)
        {
            Set(so, "wallEnabled", enabled);
            Set(so, "wallLane", lane);
            Set(so, "wallDistance", distance);
            Set(so, "wallHealth", health);
        }

        private static void Cannon(SerializedObject so, bool enabled, float interval, float flight, float radius, int projectiles)
        {
            Set(so, "cannonEnabled", enabled);
            Set(so, "cannonInterval", interval);
            Set(so, "projectileFlightTime", flight);
            Set(so, "blastRadius", radius);
            Set(so, "maximumProjectiles", projectiles);
        }

        private static void Sweeper(SerializedObject so, bool enabled, float distance, float period)
        {
            Set(so, "sweeperEnabled", enabled);
            Set(so, "sweeperDistance", distance);
            Set(so, "sweeperPeriod", period);
        }

        private static void SetGate(SerializedProperty gate, GateSpec spec)
        {
            gate.FindPropertyRelative("operation").enumValueIndex = (int)spec.operation;
            gate.FindPropertyRelative("value").intValue = spec.value;
        }

        private static void Set(SerializedObject so, string property, float value) => Find(so, property).floatValue = value;
        private static void Set(SerializedObject so, string property, int value) => Find(so, property).intValue = value;
        private static void Set(SerializedObject so, string property, bool value) => Find(so, property).boolValue = value;

        private static SerializedProperty Find(SerializedObject so, string property)
        {
            return so.FindProperty(property)
                ?? throw new System.InvalidOperationException($"{so.targetObject.GetType().Name}.{property} not found");
        }

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
