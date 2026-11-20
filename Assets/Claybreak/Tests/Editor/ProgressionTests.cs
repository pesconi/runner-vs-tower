using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>The meta rules (stars, coins, next level, upgrade, Brute, loop) and the save format.</summary>
    public sealed class ProgressionTests
    {
        private const int Levels = 6;
        private ClaybreakWorld world;
        private ClaybreakLevel[] levels;

        [SetUp]
        public void SetUp()
        {
            levels = new ClaybreakLevel[Levels];
            for (int i = 0; i < Levels; i++)
            {
                levels[i] = ScriptableObject.CreateInstance<ClaybreakLevel>();
                levels[i].name = "Level" + (i + 1);
                var level = new SerializedObject(levels[i]);
                level.FindProperty("coinReward").intValue = 40;
                if (i == Levels - 1)
                {
                    level.FindProperty("tower").enumValueIndex = (int)TowerKind.Boss;
                    level.FindProperty("floors").intValue = 4;
                    level.FindProperty("shiftGatesOnFloor").boolValue = true;
                }

                level.ApplyModifiedPropertiesWithoutUndo();
            }

            world = ScriptableObject.CreateInstance<ClaybreakWorld>();
            var so = new SerializedObject(world);
            SerializedProperty array = so.FindProperty("levels");
            array.arraySize = Levels;
            for (int i = 0; i < Levels; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];
            }

            (int cost, int bonus)[] steps = { (60, 5), (120, 10), (200, 16), (320, 24), (480, 34) };
            SerializedProperty upgrade = so.FindProperty("startingArmySteps");
            upgrade.arraySize = steps.Length;
            for (int i = 0; i < steps.Length; i++)
            {
                upgrade.GetArrayElementAtIndex(i).FindPropertyRelative("cost").intValue = steps[i].cost;
                upgrade.GetArrayElementAtIndex(i).FindPropertyRelative("bonus").intValue = steps[i].bonus;
            }

            so.FindProperty("bruteUnlockLevel").intValue = 2;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(world);
            foreach (ClaybreakLevel level in levels)
            {
                Object.DestroyImmediate(level);
            }
        }

        [Test]
        public void TestWorldIsValid()
        {
            Assert.IsTrue(world.TryValidate(300, out string error), error);
        }

        [Test]
        public void WorldNeedsTheBossLast()
        {
            var so = new SerializedObject(levels[Levels - 1]);
            so.FindProperty("tower").enumValueIndex = (int)TowerKind.Keep;
            so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsFalse(world.TryValidate(300, out string error));
            StringAssert.Contains("boss", error);
        }

        [TestCase(false, 100, 90, 0)]
        [TestCase(true, 100, 60, 3)]
        [TestCase(true, 100, 30, 2)]
        [TestCase(true, 100, 5, 1)]
        [TestCase(true, 0, 0, 1)]
        public void StarsFollowTheSurvivors(bool won, int atLine, int left, int expected)
        {
            Assert.AreEqual(expected, Progression.StarsFor(won, atLine, left));
        }

        [Test]
        public void WinAdvancesAndPays()
        {
            var progression = new Progression(world, new SaveData());
            LevelOutcome outcome = progression.Complete(true, 100, 60);
            Assert.IsTrue(outcome.Won);
            Assert.AreEqual(3, outcome.Stars);
            Assert.AreEqual(40 + 60 + 3 * 10, outcome.Coins);
            Assert.AreEqual(1, progression.LevelIndex);
            Assert.AreEqual(outcome.Coins, progression.Save.coins);
            Assert.AreEqual(3, progression.StarsAt(0));
        }

        [Test]
        public void LossKeepsTheLevelAndPaysALittle()
        {
            var progression = new Progression(world, new SaveData());
            LevelOutcome outcome = progression.Complete(false, 100, 0);
            Assert.IsFalse(outcome.Won);
            Assert.AreEqual(5, outcome.Coins);
            Assert.AreEqual(0, progression.LevelIndex);
            Assert.AreEqual(1, progression.Save.levelsLost);
        }

        [Test]
        public void BruteUnlocksOnTheThirdWin()
        {
            var progression = new Progression(world, new SaveData());
            Assert.IsFalse(progression.Complete(true, 10, 1).UnlockedBrute);
            Assert.IsFalse(progression.Complete(true, 10, 1).UnlockedBrute);
            Assert.IsTrue(progression.Complete(true, 10, 1).UnlockedBrute);
            Assert.IsTrue(progression.Save.bruteUnlocked);
            Assert.IsFalse(progression.Complete(true, 10, 1).UnlockedBrute);
        }

        [Test]
        public void BeatingTheBossLoopsTheWorldTougher()
        {
            var progression = new Progression(world, new SaveData { nextLevel = Levels - 1 });
            Assert.AreEqual(100, progression.ScaleHealth(100));
            LevelOutcome outcome = progression.Complete(true, 10, 5);
            Assert.IsTrue(outcome.ClearedWorld);
            Assert.AreEqual(0, progression.LevelIndex);
            Assert.AreEqual(1, progression.Save.loop);
            Assert.AreEqual(125, progression.ScaleHealth(100));
        }

        [Test]
        public void UpgradeCostsCoinsAndRaisesTheStartingArmy()
        {
            var progression = new Progression(world, new SaveData { coins = 100 });
            Assert.AreEqual(0, progression.StartingArmyBonus);
            Assert.IsTrue(progression.TryBuyUpgrade());
            Assert.AreEqual(40, progression.Save.coins);
            Assert.AreEqual(5, progression.StartingArmyBonus);
            Assert.AreEqual(120, progression.NextUpgradeCost);
            Assert.IsFalse(progression.TryBuyUpgrade(), "not enough coins");
            Assert.AreEqual(1, progression.Save.startingArmyStep);
        }

        [Test]
        public void UpgradeStopsAtTheLastStep()
        {
            var progression = new Progression(world, new SaveData { coins = 10000, startingArmyStep = 5 });
            Assert.IsTrue(progression.UpgradeMaxed);
            Assert.IsFalse(progression.TryBuyUpgrade());
            Assert.AreEqual(34, progression.StartingArmyBonus);
        }

        [Test]
        public void HandEditedSaveIsClamped()
        {
            var save = new SaveData { nextLevel = 99, startingArmyStep = 42, coins = -5, stars = new[] { 9, -1 } };
            var progression = new Progression(world, save);
            Assert.AreEqual(Levels - 1, progression.LevelIndex);
            Assert.AreEqual(5, progression.Save.startingArmyStep);
            Assert.AreEqual(0, progression.Save.coins);
            Assert.AreEqual(Levels, progression.Save.stars.Length);
            Assert.AreEqual(3, progression.StarsAt(0));
            Assert.AreEqual(0, progression.StarsAt(1));
        }

        [Test]
        public void CoinsNeverOverflow()
        {
            var progression = new Progression(world, new SaveData { coins = int.MaxValue });
            Assert.AreEqual(Progression.MaximumCoins, progression.Save.coins);
            progression.Complete(true, 100, 100);
            Assert.AreEqual(Progression.MaximumCoins, progression.Save.coins);
        }

        [Test]
        public void SaveRoundTrips()
        {
            var save = new SaveData { nextLevel = 3, loop = 1, coins = 250, startingArmyStep = 2, bruteUnlocked = true, stars = new[] { 3, 2, 1 } };
            Assert.IsTrue(SaveStore.TryDeserialize(SaveStore.Serialize(save), out SaveData read));
            Assert.AreEqual(3, read.nextLevel);
            Assert.AreEqual(1, read.loop);
            Assert.AreEqual(250, read.coins);
            Assert.AreEqual(2, read.startingArmyStep);
            Assert.IsTrue(read.bruteUnlocked);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, read.stars);
        }

        [TestCase("")]
        [TestCase("not json at all")]
        [TestCase("{\"version\":99,\"coins\":5}")]
        [TestCase("{\"version\":0}")]
        public void DamagedOrFutureSaveStartsFresh(string json)
        {
            Assert.IsFalse(SaveStore.TryDeserialize(json, out SaveData read));
            Assert.AreEqual(0, read.coins);
            Assert.AreEqual(SaveData.CurrentVersion, read.version);
        }
    }
}
