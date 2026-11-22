using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>
    /// The versioned save (RT-5): every fixture save of the slice (version 1) migrates to version 2 keeping
    /// coins, stars, the upgrade and the Brute; damaged saves start fresh; normalized saves pass the
    /// validator and survive a write and a read. Also the world list rules (more than one world) and the crowd shape of
    /// version 4 (RT-6t.6b: older saves get the shape they were drawn with; an unknown shape falls back to the default).
    /// </summary>
    public sealed class SaveMigrationTests
    {
        private const string Fixtures = "Assets/Claybreak/Editor/SaveFixtures/";
        private TestContent content;

        [SetUp]
        public void SetUp()
        {
            content = new TestContent();
        }

        [TearDown]
        public void TearDown()
        {
            content.Dispose();
        }

        private static string Fixture(string name)
        {
            return File.ReadAllText(Fixtures + name);
        }

        private Progression Load(string name, out int version)
        {
            Assert.IsTrue(SaveStore.TryDeserialize(Fixture(name), out SaveData save, out version), name + " was refused");
            var progression = new Progression(content.Catalog, save);
            var problems = new List<string>();
            Assert.IsTrue(SaveValidator.Validate(progression.Save, content.Catalog, problems), string.Join("; ", problems));
            return progression;
        }

        [Test]
        public void SliceSaveMidWorldKeepsEverything()
        {
            Progression progression = Load("v1-mid-world.json", out int version);
            Assert.AreEqual(1, version);
            Assert.AreEqual(SaveData.CurrentVersion, progression.Save.version);
            Assert.AreEqual(315, progression.Save.coins);
            Assert.AreEqual(4, progression.LevelIndex);
            Assert.AreEqual(0, progression.WorldProgress.loop);
            Assert.AreEqual(2, progression.UpgradeLevel(0), "starting army step");
            Assert.AreEqual(10, progression.Loadout.StartingArmy);
            Assert.IsTrue(progression.IsUnitOwned(0), "the Brute");
            Assert.IsTrue(progression.IsUnitEquipped(0), "the Brute stays on, as in the slice");
            Assert.AreEqual(3, progression.StarsAt(0));
            Assert.AreEqual(1, progression.StarsAt(3));
            Assert.AreEqual(3 + 2 + 3 + 1, progression.TotalStars);
            Assert.AreEqual(4, progression.Save.levelsWon);
            Assert.AreEqual(2, progression.Save.levelsLost);
            Assert.AreEqual("clay", progression.Skin.id);
        }

        [Test]
        public void SliceSaveAfterTheBossKeepsItsLoopAndTheMaxedUpgradeFitsTheTrack()
        {
            Progression progression = Load("v1-looped.json", out _);
            Assert.AreEqual(2, progression.WorldProgress.loop);
            Assert.AreEqual(1, progression.LevelIndex);
            Assert.AreEqual(2400, progression.Save.coins);
            // The slice had 6 army steps; the test catalog has 5: the save is clamped, never trusted.
            Assert.AreEqual(5, progression.UpgradeLevel(0));
            Assert.AreEqual(TestContent.LevelsPerWorld, progression.WorldProgress.stars.Length);
            Assert.IsTrue(progression.IsUnitOwned(2), "the Warden's level was beaten (the world was cleared), so it is owned");
        }

        [Test]
        public void FreshSliceSaveMigratesToAFreshSave()
        {
            Progression progression = Load("v1-fresh.json", out _);
            Assert.AreEqual(0, progression.Save.coins);
            Assert.AreEqual(0, progression.LevelIndex);
            Assert.AreEqual(0, progression.UpgradeLevel(0));
            Assert.AreEqual(0, progression.EquippedCount);
        }

        [Test]
        public void HandEditedSliceSaveIsClamped()
        {
            Progression progression = Load("v1-hand-edited.json", out _);
            Assert.AreEqual(0, progression.Save.coins);
            Assert.AreEqual(TestContent.LevelsPerWorld - 1, progression.LevelIndex);
            Assert.AreEqual(0, progression.WorldProgress.loop);
            Assert.AreEqual(3, progression.StarsAt(0));
            Assert.AreEqual(0, progression.StarsAt(1));
            Assert.AreEqual(5, progression.UpgradeLevel(0));
        }

        [Test]
        public void UnknownIdsAreDroppedAndKnownOnesKept()
        {
            Progression progression = Load("v2-unknown-ids.json", out int version);
            Assert.AreEqual(2, version);
            Assert.AreEqual(1, progression.Save.worlds.Length, "the removed world is gone");
            Assert.AreEqual(2, progression.LevelIndex);
            Assert.AreEqual(1, progression.UpgradeLevel(0));
            Assert.AreEqual(5, progression.UpgradeLevel(1), "damage clamped to its last step");
            CollectionAssert.AreEqual(new[] { "brute" }, progression.Save.units);
            CollectionAssert.AreEqual(new[] { "brute" }, progression.Save.loadout);
            CollectionAssert.AreEquivalent(new[] { "clay", "ember" }, progression.Save.skins, "the free skin is always owned");
            Assert.AreEqual("ember", progression.Skin.id);
        }

        [TestCase("damaged-truncated.json")]
        [TestCase("damaged-future.json")]
        [TestCase("damaged-text.json")]
        public void DamagedSavesStartFresh(string name)
        {
            Assert.IsFalse(SaveStore.TryDeserialize(Fixture(name), out SaveData read, out _));
            Assert.AreEqual(0, read.coins);
            Assert.AreEqual(SaveData.CurrentVersion, read.version);
        }

        [TestCase("")]
        [TestCase("{\"version\":0}")]
        [TestCase("{\"coins\":5}")]
        public void SavesWithoutAKnownVersionStartFresh(string json)
        {
            Assert.IsFalse(SaveStore.TryDeserialize(json, out SaveData read));
            Assert.AreEqual(0, read.coins);
        }

        [Test]
        public void OversizedSaveIsRefused()
        {
            string json = "{\"version\":2,\"units\":[\"" + new string('a', SaveStore.MaximumJsonLength) + "\"]}";
            Assert.IsFalse(SaveStore.TryDeserialize(json, out _));
        }

        [Test]
        public void LongListsAreCut()
        {
            var units = new string[SaveStore.MaximumListLength + 50];
            for (int i = 0; i < units.Length; i++)
            {
                units[i] = "brute";
            }

            string json = SaveStore.Serialize(new SaveData { units = units });
            Assert.IsTrue(SaveStore.TryDeserialize(json, out SaveData read));
            Assert.AreEqual(SaveStore.MaximumListLength, read.units.Length);
            Assert.AreEqual(1, new Progression(content.Catalog, read).Save.units.Length, "duplicates collapse");
        }

        [Test]
        public void NormalizedSaveRoundTrips()
        {
            Progression progression = Load("v1-mid-world.json", out _);
            progression.Save.coins += 2000;
            Assert.IsTrue(progression.TryBuyUnit(1));
            Assert.IsTrue(progression.TryBuySkin(1));
            string written = SaveStore.Serialize(progression.Save);
            Assert.IsTrue(SaveStore.TryDeserialize(written, out SaveData read, out int version));
            Assert.AreEqual(SaveData.CurrentVersion, version);
            Assert.AreEqual(written, SaveStore.Serialize(new Progression(content.Catalog, read).Save));
        }

        [Test]
        public void SecondWorldOpensAfterTheBossWithEnoughStars()
        {
            using var two = new TestContent(2, starsForWorld2: 10);
            var save = new SaveData();
            save.worlds = new[] { new WorldSave { id = SaveMigration.V1WorldId, nextLevel = TestContent.LevelsPerWorld - 1, stars = new[] { 3, 3, 3, 0, 0, 0 } } };
            var progression = new Progression(two.Catalog, save);
            Assert.IsFalse(progression.IsWorldUnlocked(1));
            Assert.IsFalse(progression.TrySelectWorld(1));

            LevelOutcome outcome = progression.Complete(true, 10, 1);
            Assert.AreEqual(10, progression.TotalStars);
            Assert.AreEqual("world2", outcome.OpenedWorld?.WorldId, "boss beaten with 10 stars opens world 2");
            Assert.AreEqual(1, progression.WorldIndex, "and NEXT plays it");
            Assert.AreEqual("world2", progression.Save.world);
            Assert.IsTrue(progression.TrySelectWorld(0), "world 1 can be replayed");
            Assert.AreEqual(0, progression.LevelIndex);
            Assert.AreEqual(1, progression.WorldProgress.loop);
        }

        [Test]
        public void SecondWorldStaysShutWithoutTheStars()
        {
            using var two = new TestContent(2, starsForWorld2: 15);
            var save = new SaveData();
            save.worlds = new[] { new WorldSave { id = SaveMigration.V1WorldId, nextLevel = TestContent.LevelsPerWorld - 1 } };
            var progression = new Progression(two.Catalog, save);
            LevelOutcome outcome = progression.Complete(true, 10, 1);
            Assert.IsTrue(outcome.ClearedWorld);
            Assert.IsNull(outcome.OpenedWorld);
            Assert.AreEqual(0, progression.WorldIndex);
            Assert.IsFalse(progression.IsWorldUnlocked(1));
        }

        [Test]
        public void ASavedLockedWorldFallsBackToTheFirst()
        {
            using var two = new TestContent(2, starsForWorld2: 15);
            var progression = new Progression(two.Catalog, new SaveData { world = "world2" });
            Assert.AreEqual(0, progression.WorldIndex);
            Assert.AreEqual(SaveMigration.V1WorldId, progression.Save.world);
        }

        [Test]
        public void VersionThreeSaveMigratesToVersionFourWithTheBall()
        {
            Assert.IsTrue(Fixture("v3-four-worlds.json").Contains("\"version\":3"), "the fixture stands for a version-3 save");
            Assert.IsFalse(Fixture("v3-four-worlds.json").Contains("\"shape\""));
            Progression progression = Load("v3-four-worlds.json", out int version);
            Assert.AreEqual(3, version);
            Assert.AreEqual(4, SaveData.CurrentVersion);
            Assert.AreEqual(SaveData.CurrentVersion, progression.Save.version);
            Assert.AreEqual(SaveMigration.ShapeBeforeV4, progression.Save.shape);
            Assert.AreEqual(0, progression.ShapeIndex, "the Bolinha older saves were drawn with is the catalog default");
            Assert.AreEqual(4200, progression.Save.coins, "nothing else changes");
            StringAssert.Contains("\"shape\":\"ball\"", SaveStore.Serialize(progression.Save));
        }

        [TestCase("v1-mid-world.json", 1)]
        [TestCase("v2-world1-at-boss.json", 2)]
        public void OlderSavesGetTheBallToo(string name, int expectedVersion)
        {
            Progression progression = Load(name, out int version);
            Assert.AreEqual(expectedVersion, version);
            Assert.AreEqual(SaveMigration.ShapeBeforeV4, progression.Save.shape);
        }

        [Test]
        public void VersionFourSaveKeepsItsShape()
        {
            Progression progression = Load("v4-chick-bubblegum.json", out int version);
            Assert.AreEqual(4, version);
            Assert.AreEqual("chick", progression.Save.shape);
            Assert.AreEqual(1, progression.ShapeIndex);
            Assert.AreEqual("chick", progression.Shape.id);
        }

        [TestCase("{\"version\":4,\"shape\":\"dragon\"}")]
        [TestCase("{\"version\":4,\"shape\":\"\"}")]
        [TestCase("{\"version\":4}")]
        [TestCase("{\"version\":4,\"shape\":null}")]
        public void UnknownOrMissingShapeFallsBackToTheDefault(string json)
        {
            Assert.IsTrue(SaveStore.TryDeserialize(json, out SaveData read));
            var progression = new Progression(content.Catalog, read);
            Assert.AreEqual(content.Catalog.Shapes[0].id, progression.Save.shape);
            var problems = new List<string>();
            Assert.IsTrue(SaveValidator.Validate(progression.Save, content.Catalog, problems), string.Join("; ", problems));
        }

        [Test]
        public void ShapePickWalksTheCatalogAndRoundTrips()
        {
            var progression = new Progression(content.Catalog, new SaveData());
            Assert.AreEqual(0, progression.ShapeIndex);
            Assert.IsFalse(progression.TrySelectShape(0), "already picked");
            Assert.IsFalse(progression.TrySelectShape(-1));
            Assert.IsFalse(progression.TrySelectShape(content.Catalog.Shapes.Count));
            int coins = progression.Save.coins;
            Assert.IsTrue(progression.TrySelectNextShape());
            Assert.AreEqual("chick", progression.Save.shape);
            Assert.AreEqual(coins, progression.Save.coins, "shapes are free (D26 A)");
            string written = SaveStore.Serialize(progression.Save);
            Assert.IsTrue(SaveStore.TryDeserialize(written, out SaveData read, out int version));
            Assert.AreEqual(SaveData.CurrentVersion, version);
            Assert.AreEqual("chick", new Progression(content.Catalog, read).Save.shape);
            Assert.IsTrue(progression.TrySelectNextShape(), "wraps around");
            Assert.AreEqual(SaveMigration.ShapeBeforeV4, progression.Save.shape);
        }

        [Test]
        public void ValidatorRefusesAShapeOutsideTheCatalog()
        {
            var progression = new Progression(content.Catalog, new SaveData());
            progression.Save.shape = "dragon";
            var problems = new List<string>();
            Assert.IsFalse(SaveValidator.Validate(progression.Save, content.Catalog, problems));
            StringAssert.Contains("dragon", string.Join("; ", problems));
        }

        [Test]
        public void DeviceGateWearsTheHeaviestShape()
        {
            SaveData gate = Progression.MaximumLoadout(content.Catalog);
            Assert.AreEqual("chick", gate.shape, "the test chick has the most triangles");

            // The heaviest shape first: the pick is by triangles, not by position.
            var heavy = new Mesh { name = "heavy", vertices = new[] { Vector3.zero, Vector3.up, Vector3.right }, triangles = new int[30 * 3] };
            var light = new Mesh { name = "light", vertices = heavy.vertices, triangles = new int[3] };
            var catalog = ScriptableObject.CreateInstance<ClaybreakCatalog>();
            try
            {
                catalog.EditorAssign(new WorldEntry[0], new UpgradeTrack[0], new UnitType[0], new[] { new CrowdSkin { id = "clay", title = "CLAY" } },
                    new[]
                    {
                        new CrowdShapeOption { id = "heavy", title = "HEAVY", mesh = heavy },
                        new CrowdShapeOption { id = "light", title = "LIGHT", mesh = light }
                    },
                    1, 3);
                Assert.AreEqual("heavy", Progression.MaximumLoadout(catalog).shape);
            }
            finally
            {
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(heavy);
                Object.DestroyImmediate(light);
            }
        }
    }
}
