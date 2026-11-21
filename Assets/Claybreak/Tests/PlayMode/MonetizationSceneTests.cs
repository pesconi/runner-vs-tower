using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-7 in the real scene with a stand-in for the ads SDK (the Editor has no ads): the continue offer and its
    /// NO THANKS, doubling the coins on the result, the interstitial call between levels and the daily gift.
    /// </summary>
    public sealed class MonetizationSceneTests
    {
        private const string ScenePath = "Assets/Claybreak/Content/Claybreak.unity";

        private sealed class FakeAds : IAdGateway
        {
            public bool Ready = true;
            public bool Earns = true;
            public int RewardedShown;
            public int LevelsFinished;
            public int InterstitialAsked;

            public bool RewardedReady => Ready;
            public bool PrivacyOptionsRequired => false;
            public event Action Changed;

            public void ShowRewarded(Action<bool> earned)
            {
                RewardedShown++;
                earned(Earns);
            }

            public void LevelFinished()
            {
                LevelsFinished++;
            }

            public void ShowInterstitialIfDue(Action done)
            {
                InterstitialAsked++;
                done();
            }

            public void ShowPrivacyOptions()
            {
            }

            public void RaiseChanged()
            {
                Changed?.Invoke();
            }
        }

        private FakeAds ads;

        [SetUp]
        public void SetUp()
        {
            ads = new FakeAds();
#if UNITY_EDITOR
            ClaybreakFlow.EditorAds = ads;
#endif
        }

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorAds = null;
            ClaybreakFlow.EditorStartsWithoutTap = false;
            ClaybreakFlow.EditorUsesFreshSave = false;
#endif
        }

        [UnityTest]
        public IEnumerator ContinueBringsTheArmyBackOnceThenTheLossCounts()
        {
            yield return ReachAssault();
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            RunCrowd crowd = flow.Crowd;
            crowd.SetArmy(0);
            yield return null;
            Assert.AreEqual(FlowState.ContinueOffer, flow.State, "no continue was offered");
            Assert.IsTrue(flow.Screens.ContinueShown);
            Press("/Ui/Screens/Continue/Safe/Card/WatchAd");
            Assert.AreEqual(1, ads.RewardedShown);
            Assert.AreEqual(FlowState.Assault, flow.State, "the earned continue did not resume the assault");
            Assert.Greater(crowd.Army, 0, "the continue brought no units back");

            crowd.SetArmy(0);
            yield return null;
            Assert.AreEqual(FlowState.Lost, flow.State, "a second continue was offered in the same level");
            Assert.AreEqual(1, flow.Progress.Save.levelsLost);
        }

        [UnityTest]
        public IEnumerator NoThanksEndsTheLevelWithoutAnAd()
        {
            yield return ReachAssault();
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            flow.Crowd.SetArmy(0);
            yield return null;
            Assert.AreEqual(FlowState.ContinueOffer, flow.State);
            Press("/Ui/Screens/Continue/Safe/Card/NoThanks");
            Assert.AreEqual(FlowState.Lost, flow.State);
            Assert.AreEqual(0, ads.RewardedShown);
            Assert.AreEqual(1, flow.Progress.Save.levelsLost);
        }

        [UnityTest]
        public IEnumerator NoAdLoadedMeansNoOffer()
        {
            ads.Ready = false;
            yield return ReachAssault();
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            flow.Crowd.SetArmy(0);
            yield return null;
            Assert.AreEqual(FlowState.Lost, flow.State, "a continue was offered without an ad to play");
            yield return WaitForResult(flow);
            Assert.IsFalse(flow.Screens.ResultAdShown, "the result offered an ad that is not loaded");
        }

        [UnityTest]
        public IEnumerator DoubleCoinsPaysTheLevelAgainAndSkipsTheInterstitial()
        {
            yield return ReachAssault();
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            flow.Tower.Hit(flow.Tower.Health);
            Assert.AreEqual(FlowState.Won, flow.State);
            Assert.AreEqual(1, ads.LevelsFinished);
            int coins = flow.Progress.Save.coins;
            yield return WaitForResult(flow);
            Assert.IsTrue(flow.Screens.ResultAdShown, "the result did not offer to double the coins");
            Press("/Ui/Screens/Result/Safe/RewardedOffer");
            Assert.AreEqual(coins + flow.LastOutcome.Coins, flow.Progress.Save.coins, "the coins were not doubled");
            Assert.IsFalse(flow.Screens.ResultAdShown, "the offer stayed after it was used");
            Press("/Ui/Screens/Result/Safe/Card/Primary");
            Assert.AreEqual(FlowState.Run, flow.State);
            Assert.AreEqual(0, ads.InterstitialAsked, "an interstitial right after a chosen rewarded ad");
        }

        [UnityTest]
        public IEnumerator NextAfterAWinAsksForTheInterstitial()
        {
            ads.Ready = false;
            yield return ReachAssault();
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            flow.Tower.Hit(flow.Tower.Health);
            yield return WaitForResult(flow);
            Press("/Ui/Screens/Result/Safe/Card/Primary");
            Assert.AreEqual(1, ads.InterstitialAsked);
            Assert.AreEqual(FlowState.Run, flow.State, "the next level did not start after the interstitial call");
        }

        [UnityTest]
        public IEnumerator DailyGiftPaysOnceFromTheHomeScreen()
        {
            yield return LoadScene(false);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.AreEqual(FlowState.Menu, flow.State);
            Assert.IsTrue(flow.Screens.DailyShown, "a fresh player has no daily gift");
            int coins = flow.Progress.Save.coins;
            Press("/Ui/Screens/Home/Safe/DailyGift");
            Assert.Greater(flow.Progress.Save.coins, coins);
            Assert.IsFalse(flow.Screens.DailyShown, "the gift stayed after the claim");
            Assert.AreEqual(1, flow.Progress.Save.dailyClaims);
        }

        private IEnumerator ReachAssault()
        {
            yield return LoadScene(true);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            RunCrowd crowd = flow.Crowd;
            bool grown = false;
            float deadline = Time.realtimeSinceStartup + 60f;
            while (Time.realtimeSinceStartup < deadline && flow.State != FlowState.Assault)
            {
                // A real army at the firing line (the continue needs one).
                if (!grown && flow.State == FlowState.Run)
                {
                    crowd.SetArmy(40);
                    grown = true;
                }

                yield return null;
            }

            Assert.AreEqual(FlowState.Assault, flow.State, "the run never reached the firing line");
        }

        private static IEnumerator WaitForResult(ClaybreakFlow flow)
        {
            float deadline = Time.realtimeSinceStartup + 5f;
            while (Time.realtimeSinceStartup < deadline && !flow.Screens.ResultShown)
            {
                yield return null;
            }

            Assert.IsTrue(flow.Screens.ResultShown, "the result never showed");
        }

        private static void Press(string path)
        {
            GameObject target = GameObject.Find(path);
            Assert.IsNotNull(target, path + " is not on screen");
            var button = target.GetComponent<UnityEngine.UI.Button>();
            Assert.IsTrue(button.interactable, path + " is disabled");
            button.onClick.Invoke();
        }

        private static IEnumerator LoadScene(bool startsWithoutTap)
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesFreshSave = true;
            ClaybreakFlow.EditorStartsWithoutTap = startsWithoutTap;
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
            AsyncOperation load = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            while (!load.isDone)
            {
                yield return null;
            }
#else
            Assert.Ignore("Editor only.");
#endif

            yield return null;
            yield return null;
        }
    }
}
