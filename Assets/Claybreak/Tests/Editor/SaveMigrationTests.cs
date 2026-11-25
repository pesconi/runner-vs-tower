using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Claybreak.Tests
{
    /// <summary>
    /// The versioned save (RT-5): every fixture save of the slice (version 1) migrates to version 2 keeping
    /// coins, stars, the upgrade and the Brute; damaged saves start fresh; normalized saves pass the
    /// validator and survive a write and a read. Also the world list rules (more than one world) and the crowd shape of
    /// version 4 (RT-6t.6b: older saves get the shape they were drawn with; an unknown shape falls back to the default) and
    /// the shapes bought in the shop of version 5 (RT-6t.20, D31 A: a version-4 save owns the free Bolinha and the shape it
    /// wore when that was on offer then, so a Pintinho player keeps it without paying; nothing else comes for free).
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
        public void VersionThreeSaveMigratesWearingBibopzi()
        {
            Assert.IsTrue(Fixture("v3-four-worlds.json").Contains("\"version\":3"), "the fixture stands for a version-3 save");
            Assert.IsFalse(Fixture("v3-four-worlds.json").Contains("\"shape\""));
            Progression progression = Load("v3-four-worlds.json", out int version);
            Assert.AreEqual(3, version);
            Assert.AreEqual(6, SaveData.CurrentVersion);
            Assert.AreEqual(SaveData.CurrentVersion, progression.Save.version);
            Assert.AreEqual(SaveMigration.DefaultShape, progression.Save.shape, "the old default (Bolinha) became Bibopzi");
            Assert.AreEqual(0, progression.ShapeIndex, "Bibopzi is the catalog default");
            Assert.AreEqual(4200, progression.Save.coins, "nothing else changes");
            StringAssert.Contains("\"shape\":\"bibopzi\"", SaveStore.Serialize(progression.Save));
            CollectionAssert.AreEqual(new[] { "bibopzi", "ball" }, progression.Save.ownedShapes, "the Bolinha stays owned");
        }

        [TestCase("v1-mid-world.json", 1)]
        [TestCase("v2-world1-at-boss.json", 2)]
        public void OlderSavesWearBibopziToo(string name, int expectedVersion)
        {
            Progression progression = Load(name, out int version);
            Assert.AreEqual(expectedVersion, version);
            Assert.AreEqual(SaveMigration.DefaultShape, progression.Save.shape);
            CollectionAssert.AreEqual(new[] { "bibopzi", "ball" }, progression.Save.ownedShapes);
        }

        [Test]
        public void VersionFourSaveKeepsItsShape()
        {
            Progression progression = Load("v4-chick-bubblegum.json", out int version);
            Assert.AreEqual(4, version);
            Assert.AreEqual("chick", progression.Save.shape);
            Assert.AreEqual(2, progression.ShapeIndex);
            Assert.AreEqual("chick", progression.Shape.id);
            Assert.IsTrue(progression.IsShapeOwned(2), "a version-4 Pintinho player keeps it without paying (D31 A)");
            CollectionAssert.AreEqual(new[] { "bibopzi", "ball", "chick" }, progression.Save.ownedShapes);
            Assert.AreEqual(9800, progression.Save.coins, "nothing is charged");
        }

        // RT-6t.20 acceptance fixtures: version 4 with the Bolinha, with the Pintinho, without a shape, and a hand edit. Since
        // save version 6 the Bolinha (the old default) becomes Bibopzi; a picked shape stays.
        [TestCase("v4-ball.json", "bibopzi", new[] { "bibopzi", "ball" }, 640)]
        [TestCase("v4-chick-bubblegum.json", "chick", new[] { "bibopzi", "ball", "chick" }, 9800)]
        [TestCase("v4-no-shape.json", "bibopzi", new[] { "bibopzi", "ball" }, 1180)]
        [TestCase("v4-hand-edited-shapes.json", "bibopzi", new[] { "bibopzi", "ball" }, 75)]
        public void VersionFourSavesMigrateOwningTheirShape(string name, string worn, string[] owned, int coins)
        {
            Progression progression = Load(name, out int version);
            Assert.AreEqual(4, version);
            Assert.AreEqual(SaveData.CurrentVersion, progression.Save.version);
            Assert.AreEqual(worn, progression.Save.shape);
            CollectionAssert.AreEqual(owned, progression.Save.ownedShapes);
            Assert.AreEqual(coins, progression.Save.coins, "nothing else changes");
            StringAssert.Contains("\"ownedShapes\"", SaveStore.Serialize(progression.Save));
        }

        // Versions 1 to 3 never had a shape: a crafted one is replaced by the Bolinha (save review, RT-6t.20).
        [TestCase("{\"version\":3,\"shape\":\"chick\",\"ownedShapes\":[\"chick\",\"bunny\"]}")]
        [TestCase("{\"version\":2,\"shape\":\"chick\"}")]
        public void OlderVersionsNeverKeepACraftedShape(string json)
        {
            Assert.IsTrue(SaveStore.TryDeserialize(json, out SaveData read));
            var progression = new Progression(content.Catalog, read);
            Assert.AreEqual(SaveMigration.DefaultShape, progression.Save.shape);
            CollectionAssert.AreEqual(new[] { SaveMigration.DefaultShape, SaveMigration.ShapeBeforeV4 }, progression.Save.ownedShapes);
        }

        // A save that cannot be read is kept once before the fresh save replaces it (save review, RT-6t.20).
        [Test]
        public void AnUnreadableSaveIsKeptBeforeStartingFresh()
        {
            string saved = PlayerPrefs.GetString(SaveStore.Key, null);
            bool hadBackup = PlayerPrefs.HasKey(SaveStore.UnreadableBackupKey);
            string oldBackup = PlayerPrefs.GetString(SaveStore.UnreadableBackupKey, null);
            try
            {
                PlayerPrefs.DeleteKey(SaveStore.UnreadableBackupKey);
                PlayerPrefs.SetString(SaveStore.Key, "{\"version\":99,\"coins\":5}");
                LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("could not be read"));
                SaveData fresh = SaveStore.Load();
                Assert.AreEqual(0, fresh.coins);
                Assert.AreEqual("{\"version\":99,\"coins\":5}", PlayerPrefs.GetString(SaveStore.UnreadableBackupKey));
            }
            finally
            {
                if (saved == null) PlayerPrefs.DeleteKey(SaveStore.Key); else PlayerPrefs.SetString(SaveStore.Key, saved);
                if (hadBackup) PlayerPrefs.SetString(SaveStore.UnreadableBackupKey, oldBackup); else PlayerPrefs.DeleteKey(SaveStore.UnreadableBackupKey);
            }
        }

        // A hand-edited version-4 save can never get a shape sold since version 5 for free.
        [Test]
        public void MigrationNeverGrantsAShapeSoldSinceVersionFive()
        {
            SaveData save = new SaveData { shape = "bunny", ownedShapes = new[] { "bunny" } };
            SaveMigration.AddOwnedShapes(save);
            CollectionAssert.AreEqual(new[] { SaveMigration.ShapeBeforeV4 }, save.ownedShapes);
            Assert.IsTrue(SaveStore.TryDeserialize("{\"version\":4,\"shape\":\"bunny\",\"ownedShapes\":[\"bunny\"]}", out SaveData read));
            var progression = new Progression(content.Catalog, read);
            Assert.AreEqual(SaveMigration.DefaultShape, progression.Save.shape);
            Assert.IsFalse(progression.IsShapeOwned(3));
        }

        [Test]
        public void VersionFiveSaveKeepsItsBoughtShapes()
        {
            Progression progression = Load("v5-bunny.json", out int version);
            Assert.AreEqual(5, version);
            Assert.AreEqual("bunny", progression.Save.shape);
            CollectionAssert.AreEqual(new[] { "bibopzi", "ball", "chick", "bunny" }, progression.Save.ownedShapes);
            Assert.AreEqual(2210, progression.Save.coins, "nothing else changes");
        }

        // Save version 6 (2026-11-25, user's decision): a version-5 player on the Bolinha (the default then) now wears Bibopzi,
        // keeps the Bolinha owned and pays nothing.
        [Test]
        public void VersionFiveBallSaveWearsBibopzi()
        {
            Progression progression = Load("v5-ball.json", out int version);
            Assert.AreEqual(5, version);
            Assert.AreEqual(SaveMigration.DefaultShape, progression.Save.shape);
            Assert.AreEqual(0, progression.ShapeIndex);
            CollectionAssert.AreEqual(new[] { "bibopzi", "ball" }, progression.Save.ownedShapes);
            Assert.AreEqual(530, progression.Save.coins, "nothing is charged");
            Assert.IsTrue(progression.TryWearShape(1), "going back to the Bolinha is free");
            Assert.AreEqual(530, progression.Save.coins);
        }

        // Once on version 6, the Bolinha is a pick like any other: it is never swapped for Bibopzi again.
        [Test]
        public void VersionSixKeepsAPickedBall()
        {
            Progression progression = Load("v6-ball-picked.json", out int version);
            Assert.AreEqual(6, version);
            Assert.AreEqual("ball", progression.Save.shape);
            CollectionAssert.AreEqual(new[] { "bibopzi", "ball", "chick" }, progression.Save.ownedShapes);
            Assert.IsTrue(SaveStore.TryDeserialize(SaveStore.Serialize(progression.Save), out SaveData again, out int againVersion));
            Assert.AreEqual(6, againVersion);
            Assert.AreEqual("ball", new Progression(content.Catalog, again).Save.shape, "the round trip keeps the pick");
        }

        // The version-6 step alone: only the old default (or no shape) changes; ownership of the rest is untouched.
        [TestCase("ball", new[] { "ball" }, "bibopzi", new[] { "bibopzi", "ball" })]
        [TestCase("", new string[0], "bibopzi", new[] { "bibopzi" })]
        [TestCase("chick", new[] { "ball", "chick" }, "chick", new[] { "bibopzi", "ball", "chick" })]
        [TestCase("bunny", new[] { "bibopzi", "bunny" }, "bunny", new[] { "bibopzi", "bunny" })]
        public void DefaultToBibopziOnlyReplacesTheOldDefault(string shape, string[] owned, string worn, string[] ownedAfter)
        {
            var save = new SaveData { shape = shape, ownedShapes = owned };
            SaveMigration.DefaultToBibopzi(save);
            Assert.AreEqual(worn, save.shape);
            CollectionAssert.AreEqual(ownedAfter, save.ownedShapes);
        }

        // Version 5: a worn shape that is not owned falls back to the default; unknown and repeated ids are dropped and the
        // free shapes (Bibopzi, the Bolinha) are always owned, in catalog order.
        [TestCase("{\"version\":5,\"shape\":\"bunny\",\"ownedShapes\":[\"chick\"]}", "bibopzi", new[] { "bibopzi", "ball", "chick" })]
        [TestCase("{\"version\":5,\"shape\":\"chick\",\"ownedShapes\":[\"dragon\",\"chick\",\"chick\"]}", "chick", new[] { "bibopzi", "ball", "chick" })]
        [TestCase("{\"version\":5,\"shape\":\"bunny\",\"ownedShapes\":null}", "bibopzi", new[] { "bibopzi", "ball" })]
        [TestCase("{\"version\":5}", "bibopzi", new[] { "bibopzi", "ball" })]
        [TestCase("{\"version\":6,\"shape\":\"ball\",\"ownedShapes\":[\"chick\",\"ball\"]}", "ball", new[] { "bibopzi", "ball", "chick" })]
        [TestCase("{\"version\":6,\"shape\":\"bunny\",\"ownedShapes\":[]}", "bibopzi", new[] { "bibopzi", "ball" })]
        public void VersionFiveIsNormalized(string json, string worn, string[] owned)
        {
            Assert.IsTrue(SaveStore.TryDeserialize(json, out SaveData read));
            var progression = new Progression(content.Catalog, read);
            Assert.AreEqual(worn, progression.Save.shape);
            CollectionAssert.AreEqual(owned, progression.Save.ownedShapes);
            var problems = new List<string>();
            Assert.IsTrue(SaveValidator.Validate(progression.Save, content.Catalog, problems), string.Join("; ", problems));
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

        // RT-6t.20 (D31 A): a shape is bought once with coins and worn at once; an owned shape is worn again for free.
        [Test]
        public void ShapesAreBoughtWornAndRoundTrip()
        {
            var progression = new Progression(content.Catalog, new SaveData { coins = 1200 });
            Assert.AreEqual(0, progression.ShapeIndex);
            Assert.IsTrue(progression.IsShapeOwned(0), "the free shape");
            Assert.IsTrue(progression.IsShapeOwned(1), "the Bolinha is free too");
            Assert.IsFalse(progression.TryWearShape(0), "already worn");
            Assert.IsFalse(progression.TryWearShape(2), "not owned");
            Assert.IsFalse(progression.TryBuyShape(-1));
            Assert.IsFalse(progression.TryBuyShape(content.Catalog.Shapes.Count));
            Assert.IsFalse(progression.TryBuyShape(0), "the free shape is not sold");
            Assert.IsFalse(progression.TryBuyShape(1), "the free Bolinha is not sold either");
            Assert.IsTrue(progression.CanBuyAnything);

            Assert.IsTrue(progression.TryBuyShape(2));
            Assert.AreEqual(900, progression.Save.coins, "the chick costs 300");
            Assert.AreEqual("chick", progression.Save.shape, "a bought shape is worn at once");
            Assert.IsFalse(progression.TryBuyShape(2), "bought only once");
            Assert.IsFalse(progression.TryBuyShape(3), "900 coins do not buy the bunny (1000)");
            Assert.IsTrue(progression.TryWearShape(0));
            Assert.AreEqual(900, progression.Save.coins, "wearing an owned shape is free");
            Assert.IsTrue(progression.TryWearShape(1));
            Assert.IsTrue(progression.TryWearShape(2));

            string written = SaveStore.Serialize(progression.Save);
            Assert.IsTrue(SaveStore.TryDeserialize(written, out SaveData read, out int version));
            Assert.AreEqual(SaveData.CurrentVersion, version);
            var again = new Progression(content.Catalog, read);
            Assert.AreEqual("chick", again.Save.shape);
            CollectionAssert.AreEqual(new[] { "bibopzi", "ball", "chick" }, again.Save.ownedShapes);
        }

        [Test]
        public void ValidatorRefusesAWornShapeThatIsNotOwned()
        {
            var progression = new Progression(content.Catalog, new SaveData());
            progression.Save.shape = "bunny";
            var problems = new List<string>();
            Assert.IsFalse(SaveValidator.Validate(progression.Save, content.Catalog, problems));
            StringAssert.Contains("not owned", string.Join("; ", problems));

            problems.Clear();
            progression.Save.shape = "chick";
            progression.Save.ownedShapes = new[] { "chick" };
            Assert.IsFalse(SaveValidator.Validate(progression.Save, content.Catalog, problems));
            StringAssert.Contains("free crowd shape", string.Join("; ", problems));
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
            Assert.AreEqual(content.Catalog.Shapes.Count, gate.ownedShapes.Length, "the gate loadout owns every shape");

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
