using System;
using System.Collections.Generic;
using JungleDegree.StudioKit.Ads;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>RT-7: the daily reward (D5), the bonus coins of a rewarded ad and the shipped monetization asset (D6).</summary>
    public sealed class MonetizationTests
    {
        private static readonly int[] Cycle = { 50, 75, 100 };
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

        private Progression Fresh()
        {
            return new Progression(content.Catalog, new SaveData());
        }

        [Test]
        public void DailyRewardPaysOncePerDay()
        {
            Progression progression = Fresh();
            Assert.IsTrue(progression.TryClaimDaily(20000, Cycle, out int coins));
            Assert.AreEqual(50, coins);
            Assert.IsFalse(progression.TryClaimDaily(20000, Cycle, out coins), "a second claim on the same day");
            Assert.AreEqual(0, coins);
            Assert.AreEqual(50, progression.Save.coins);
            Assert.IsTrue(progression.TryClaimDaily(20001, Cycle, out coins));
            Assert.AreEqual(75, coins);
        }

        [Test]
        public void MissingDaysNeverResetsTheCycle()
        {
            Progression progression = Fresh();
            progression.TryClaimDaily(20000, Cycle, out _);
            progression.TryClaimDaily(20001, Cycle, out _);
            Assert.IsTrue(progression.TryClaimDaily(20030, Cycle, out int coins), "a month away still pays");
            Assert.AreEqual(100, coins, "the cycle went on where it was");
            Assert.IsTrue(progression.TryClaimDaily(20031, Cycle, out coins));
            Assert.AreEqual(50, coins, "after the last amount the cycle starts again");
        }

        [Test]
        public void ClockMovedBackNeverLocksTheGift()
        {
            Progression progression = Fresh();
            progression.TryClaimDaily(20010, Cycle, out _);
            Assert.IsFalse(progression.CanClaimDaily(20010), "same day");
            Assert.IsTrue(progression.CanClaimDaily(20005), "a claim made with the clock ahead never locks the real days");
            Assert.IsTrue(progression.CanClaimDaily(20011));
        }

        [Test]
        public void HandEditedDailyFieldsAreRepaired()
        {
            var save = new SaveData { dailyClaims = -3, dailyLastDay = -1, firstDay = -9 };
            var progression = new Progression(content.Catalog, save);
            var problems = new List<string>();
            Assert.IsTrue(SaveValidator.Validate(progression.Save, content.Catalog, problems), string.Join("; ", problems));
            Assert.AreEqual(50, DailyReward.NextAmount(progression.Save, Cycle));
        }

        [Test]
        public void FirstDayIsRecordedOnceForRetention()
        {
            Progression progression = Fresh();
            progression.RecordFirstDay(20000);
            progression.RecordFirstDay(20003);
            Assert.AreEqual(20000, progression.Save.firstDay);
            Assert.AreEqual(7, DailyReward.DaysSinceInstall(progression.Save, 20007));
            Assert.AreEqual(0, DailyReward.DaysSinceInstall(new SaveData(), 20007), "unknown install day reads as day 0");
        }

        [Test]
        public void DayNumbersFollowTheLocalCalendar()
        {
            Assert.AreEqual(DailyReward.DayOf(new DateTime(2026, 11, 20, 0, 1, 0)), DailyReward.DayOf(new DateTime(2026, 11, 20, 23, 59, 0)));
            Assert.AreEqual(DailyReward.DayOf(new DateTime(2026, 11, 20)) + 1, DailyReward.DayOf(new DateTime(2026, 11, 21, 0, 0, 1)));
        }

        [Test]
        public void BonusCoinsAreClamped()
        {
            Progression progression = Fresh();
            progression.GrantBonusCoins(120);
            progression.GrantBonusCoins(-50);
            Assert.AreEqual(120, progression.Save.coins);
            progression.GrantBonusCoins(int.MaxValue);
            Assert.AreEqual(Progression.MaximumCoins, progression.Save.coins);
        }

        [Test]
        public void OldSavesWithoutTheNewFieldsStillRead()
        {
            const string json = "{\"version\":3,\"coins\":40,\"world\":\"world1\"}";
            Assert.IsTrue(SaveStore.TryDeserialize(json, out SaveData data, out int version));
            Assert.AreEqual(3, version);
            Assert.AreEqual(0, data.dailyLastDay);
            Assert.IsTrue(DailyReward.CanClaim(data, 20000), "an old save gets the daily reward");
        }

        [Test]
        public void ContinueBringsBackAShareOfTheArmy()
        {
            var monetization = ScriptableObject.CreateInstance<ClaybreakMonetization>();
            try
            {
                monetization.Configure(ScriptableObject.CreateInstance<AdUnitSettings>(), true, true, 0.5f, 10, false,
                    new InterstitialPacing(3, 3, 180f, 180f), new[] { 50 });
                Assert.AreEqual(30, monetization.ContinueArmy(60));
                Assert.AreEqual(1, monetization.ContinueArmy(1));
                Assert.IsTrue(monetization.TryValidate(out string error), error);
                monetization.Configure(monetization.AdUnits, true, true, 0.5f, 10, false, new InterstitialPacing(3, 3, 180f, 180f), new[] { 0 });
                Assert.IsFalse(monetization.TryValidate(out _), "a daily reward of 0 coins is refused");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(monetization.AdUnits);
                UnityEngine.Object.DestroyImmediate(monetization);
            }
        }

        // The shipped asset keeps the decisions: interstitial off (D6), only Google test units until RT-9 (D7).
        [Test]
        public void ShippedMonetizationKeepsTheDecisions()
        {
            var shipped = AssetDatabase.LoadAssetAtPath<ClaybreakMonetization>("Assets/Claybreak/Content/Data/Monetization.asset");
            Assert.IsNotNull(shipped, "rebuild the scene (Claybreak > Build Scene)");
            Assert.IsTrue(shipped.TryValidate(out string error), error);
            Assert.IsFalse(shipped.InterstitialOn, "D6: the interstitial ships off");
            Assert.IsTrue(shipped.DoubleCoins && shipped.ContinueOnce);
            Assert.AreEqual(string.Empty, shipped.AdUnits.AndroidRewardedUnitId);
            Assert.AreEqual(string.Empty, shipped.AdUnits.AndroidInterstitialUnitId);
        }
    }
}
