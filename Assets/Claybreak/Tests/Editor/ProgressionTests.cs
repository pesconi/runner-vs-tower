using System.Collections.Generic;
using NUnit.Framework;

namespace Claybreak.Tests
{
    /// <summary>The meta rules: stars, coins, next level, loop, worlds, upgrades, units and loadout, skins.</summary>
    public sealed class ProgressionTests
    {
        private const int Levels = TestContent.LevelsPerWorld;
        private TestContent content;
        private ClaybreakCatalog catalog;

        [SetUp]
        public void SetUp()
        {
            content = new TestContent();
            catalog = content.Catalog;
        }

        [TearDown]
        public void TearDown()
        {
            content.Dispose();
        }

        private Progression Fresh(int coins = 0)
        {
            return new Progression(catalog, new SaveData { coins = coins });
        }

        private static void Valid(Progression progression)
        {
            var problems = new List<string>();
            Assert.IsTrue(SaveValidator.Validate(progression.Save, progression.Catalog, problems), string.Join("; ", problems));
        }

        [Test]
        public void TestCatalogIsValid()
        {
            Assert.IsTrue(catalog.TryValidate(300, 300, out string error), error);
        }

        [Test]
        public void CatalogRejectsAUnitNobodyCanGet()
        {
            catalog.Units[1].price = 0;
            Assert.IsFalse(catalog.TryValidate(300, 300, out string error));
            StringAssert.Contains("never be had", error);
        }

        [Test]
        public void CatalogRejectsDuplicateIdsAndAPaidDefaultSkin()
        {
            catalog.Upgrades[1].id = catalog.Upgrades[0].id;
            Assert.IsFalse(catalog.TryValidate(300, 300, out _));
            catalog.Upgrades[1].id = "damage";
            catalog.Skins[0].price = 10;
            Assert.IsFalse(catalog.TryValidate(300, 300, out string error));
            StringAssert.Contains("free", error);
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
        public void FreshSaveIsValidAndWearsTheFreeSkin()
        {
            Progression progression = Fresh();
            Valid(progression);
            Assert.AreEqual("clay", progression.Skin.id);
            Assert.AreEqual(0, progression.EquippedCount);
            Assert.AreEqual(SaveMigration.V1WorldId, progression.Save.world);
        }

        [Test]
        public void WinAdvancesAndPays()
        {
            Progression progression = Fresh();
            LevelOutcome outcome = progression.Complete(true, 100, 60);
            Assert.IsTrue(outcome.Won);
            Assert.AreEqual(3, outcome.Stars);
            Assert.AreEqual(40 + 60 + 3 * 10, outcome.Coins);
            Assert.AreEqual(1, progression.LevelIndex);
            Assert.AreEqual(outcome.Coins, progression.Save.coins);
            Assert.AreEqual(3, progression.StarsAt(0));
            Assert.AreEqual(3, progression.TotalStars);
        }

        [Test]
        public void LossKeepsTheLevelAndPaysALittle()
        {
            Progression progression = Fresh();
            LevelOutcome outcome = progression.Complete(false, 100, 0);
            Assert.IsFalse(outcome.Won);
            Assert.AreEqual(5, outcome.Coins);
            Assert.AreEqual(0, progression.LevelIndex);
            Assert.AreEqual(1, progression.Save.levelsLost);
        }

        [Test]
        public void BestStarsAreKept()
        {
            Progression progression = Fresh();
            progression.Complete(true, 100, 60);
            progression.Save.worlds[0].nextLevel = 0;
            progression.Complete(true, 100, 5);
            Assert.AreEqual(3, progression.StarsAt(0));
        }

        [Test]
        public void TheBruteUnlocksOnTheThirdWinAndIsEquipped()
        {
            Progression progression = Fresh();
            Assert.IsNull(progression.Complete(true, 10, 1).UnlockedUnit);
            Assert.IsNull(progression.Complete(true, 10, 1).UnlockedUnit);
            Assert.AreEqual(SaveMigration.V1BruteId, progression.Complete(true, 10, 1).UnlockedUnit?.id);
            Assert.IsTrue(progression.IsUnitOwned(0));
            Assert.IsTrue(progression.IsUnitEquipped(0));
            Assert.IsNull(progression.Complete(true, 10, 1).UnlockedUnit);
            Valid(progression);
        }

        [Test]
        public void BeatingTheBossLoopsTheWorldTougherAndRicher()
        {
            var save = new SaveData();
            save.worlds = new[] { new WorldSave { id = SaveMigration.V1WorldId, nextLevel = Levels - 1 } };
            var progression = new Progression(catalog, save);
            Assert.AreEqual(100, progression.ScaleHealth(100));
            LevelOutcome outcome = progression.Complete(true, 10, 5);
            Assert.IsTrue(outcome.ClearedWorld);
            Assert.IsNull(outcome.OpenedWorld, "there is no second world");
            Assert.AreEqual(0, progression.LevelIndex);
            Assert.AreEqual(1, progression.WorldProgress.loop);
            Assert.AreEqual(125, progression.ScaleHealth(100));
            Assert.AreEqual(1.25f, progression.CoinMultiplier, 1e-5f);
            int coins = progression.Save.coins;
            Assert.AreEqual(Progression.CoinsFor(true, 50, 5, 3), progression.Complete(true, 10, 5).Coins, "a loop pays 25% more reward");
            Assert.Greater(progression.Save.coins, coins);
        }

        [Test]
        public void UpgradeCostsCoinsAndRaisesItsBonus()
        {
            Progression progression = Fresh(100);
            Assert.AreEqual(0, progression.Loadout.StartingArmy);
            Assert.IsTrue(progression.TryBuyUpgrade(0));
            Assert.AreEqual(40, progression.Save.coins);
            Assert.AreEqual(5, progression.Loadout.StartingArmy);
            Assert.AreEqual(120, progression.NextUpgradeCost(0));
            Assert.IsFalse(progression.TryBuyUpgrade(0), "not enough coins");
            Assert.AreEqual(1, progression.UpgradeLevel(0));
            Valid(progression);
        }

        [Test]
        public void EveryUpgradeKindReachesTheRunLoadout()
        {
            Progression progression = Fresh(10_000);
            for (int track = 0; track < catalog.Upgrades.Count; track++)
            {
                Assert.IsTrue(progression.TryBuyUpgrade(track));
                Assert.IsTrue(progression.TryBuyUpgrade(track));
            }

            RunLoadout loadout = progression.Loadout;
            Assert.AreEqual(10, loadout.StartingArmy);
            Assert.AreEqual(1.3f, loadout.DamageMultiplier, 1e-5f);
            Assert.AreEqual(2, loadout.GateBonus);
            Assert.AreEqual(1.2f, loadout.LaunchRateMultiplier, 1e-5f);
        }

        [Test]
        public void UpgradeStopsAtTheLastStep()
        {
            var save = new SaveData { coins = 10_000, upgrades = new[] { new UpgradeSave { id = SaveMigration.V1StartingArmyId, level = 5 } } };
            var progression = new Progression(catalog, save);
            Assert.IsTrue(progression.IsUpgradeMaxed(0));
            Assert.IsFalse(progression.TryBuyUpgrade(0));
            Assert.AreEqual(34, progression.Loadout.StartingArmy);
            Assert.AreEqual(10_000, progression.Save.coins);
        }

        [Test]
        public void UnitsAreBoughtEquippedAndSwapped()
        {
            Progression progression = Fresh(2_000);
            Assert.IsFalse(progression.TryBuyUnit(0), "the Brute is not sold");
            Assert.IsTrue(progression.TryBuyUnit(1));
            Assert.AreEqual(1_400, progression.Save.coins);
            Assert.IsTrue(progression.IsUnitEquipped(1), "a bought unit goes in a free slot");
            Assert.IsTrue(progression.TryBuyUnit(2));
            Assert.AreEqual(2, progression.EquippedCount);

            var slots = new UnitType[ClaybreakCatalog.MaximumLoadoutSlots];
            Assert.AreEqual(2, progression.FillLoadout(slots));
            Assert.AreEqual("sprinter", slots[0].id);
            Assert.AreEqual("warden", slots[1].id);

            Assert.IsFalse(progression.TryToggleUnit(0), "the Brute is not owned");
            Assert.IsTrue(progression.TryToggleUnit(1), "take the Sprinter off");
            Assert.AreEqual(1, progression.FillLoadout(slots));
            Assert.AreEqual("warden", slots[0].id);
            Assert.IsNull(slots[1]);
            Valid(progression);
        }

        [Test]
        public void ALoadoutHasAtMostItsSlots()
        {
            Progression progression = Fresh(5_000);
            progression.Save.units = new[] { "brute", "sprinter", "warden" };
            progression.Save.loadout = new[] { "brute", "sprinter" };
            Assert.IsFalse(progression.TryToggleUnit(2), "both slots are taken");
            Assert.IsTrue(progression.TryToggleUnit(0));
            Assert.IsTrue(progression.TryToggleUnit(2));
            CollectionAssert.AreEqual(new[] { "sprinter", "warden" }, progression.Save.loadout);
        }

        [Test]
        public void SkinsAreBoughtAndWorn()
        {
            Progression progression = Fresh(200);
            Assert.IsFalse(progression.TryBuySkin(2), "too dear");
            Assert.IsTrue(progression.TryBuySkin(1));
            Assert.AreEqual(50, progression.Save.coins);
            Assert.AreEqual("ember", progression.Skin.id);
            Assert.IsTrue(progression.TryWearSkin(0));
            Assert.AreEqual("clay", progression.Skin.id);
            Assert.IsFalse(progression.TryWearSkin(2), "not owned");
            Valid(progression);
        }

        [Test]
        public void TheDefaultSkinWearsEachWorldsPaletteColorAndOtherSkinsTheirOwn()
        {
            using (var twoWorlds = new TestContent(2))
            {
                var progression = new Progression(twoWorlds.Catalog, new SaveData { coins = 200 });
                Assert.IsTrue(progression.WearsDefaultSkin);
                Assert.AreEqual(TestContent.PaletteCrowd(0), progression.CrowdColorIn(0));
                Assert.AreEqual(TestContent.PaletteCrowd(1), progression.CrowdColorIn(1));
                Assert.AreNotEqual(progression.CrowdColorIn(0), progression.CrowdColorIn(1));

                Assert.IsTrue(progression.TryBuySkin(1));
                Assert.IsFalse(progression.WearsDefaultSkin);
                Assert.AreEqual(progression.Skin.color, progression.CrowdColorIn(0));
                Assert.AreEqual(progression.Skin.color, progression.CrowdColorIn(1));
                Assert.AreEqual(TestContent.PaletteCrowd(1), progression.WorldCrowdColor(1), "the shop still shows the world color on the default row");
                Assert.AreEqual("ember", progression.Save.skin, "the save keeps the skin id");

                Assert.IsTrue(progression.TryWearSkin(0));
                Assert.AreEqual(TestContent.PaletteCrowd(1), progression.CrowdColorIn(1));
                Assert.AreEqual("clay", progression.Save.skin);

                // An unknown skin id falls back to the default look, so it wears the world's color too.
                progression.Save.skin = "gone";
                Assert.IsTrue(progression.WearsDefaultSkin);
                Assert.AreEqual(TestContent.PaletteCrowd(0), progression.CrowdColorIn(0));
            }
        }

        [Test]
        public void CanBuyAnythingFollowsTheCheapestItem()
        {
            Assert.IsFalse(Fresh(59).CanBuyAnything);
            Assert.IsTrue(Fresh(60).CanBuyAnything);
        }

        [Test]
        public void MaximumLoadoutHasEverythingAndPassesTheValidator()
        {
            var progression = new Progression(catalog, Progression.MaximumLoadout(catalog));
            for (int track = 0; track < catalog.Upgrades.Count; track++)
            {
                Assert.IsTrue(progression.IsUpgradeMaxed(track));
            }

            Assert.AreEqual(catalog.LoadoutSlots, progression.EquippedCount);
            Assert.AreEqual("ink", progression.Skin.id);
            Assert.AreEqual(80, progression.Bonus(UpgradeKind.Damage));
            Valid(progression);
        }

        [Test]
        public void HandEditedSaveIsClamped()
        {
            var save = new SaveData
            {
                coins = -5,
                worlds = new[] { new WorldSave { id = SaveMigration.V1WorldId, nextLevel = 99, loop = -2, stars = new[] { 9, -1 } } },
                upgrades = new[] { new UpgradeSave { id = SaveMigration.V1StartingArmyId, level = 42 } },
                loadout = new[] { "sprinter" },
                skin = "ink"
            };
            var progression = new Progression(catalog, save);
            Assert.AreEqual(Levels - 1, progression.LevelIndex);
            Assert.AreEqual(0, progression.WorldProgress.loop);
            Assert.AreEqual(5, progression.UpgradeLevel(0));
            Assert.AreEqual(0, progression.Save.coins);
            Assert.AreEqual(Levels, progression.WorldProgress.stars.Length);
            Assert.AreEqual(3, progression.StarsAt(0));
            Assert.AreEqual(0, progression.StarsAt(1));
            Assert.AreEqual(0, progression.EquippedCount, "an unowned unit cannot be equipped");
            Assert.AreEqual("clay", progression.Skin.id, "an unowned skin cannot be worn");
            Valid(progression);
        }

        [Test]
        public void CoinsNeverOverflow()
        {
            Progression progression = Fresh(int.MaxValue);
            Assert.AreEqual(Progression.MaximumCoins, progression.Save.coins);
            progression.Complete(true, 100, 100);
            Assert.AreEqual(Progression.MaximumCoins, progression.Save.coins);
        }
    }
}
