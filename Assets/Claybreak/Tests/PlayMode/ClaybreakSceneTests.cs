using System.Collections;
using JungleDegree.StudioKit.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Claybreak.Tests
{
    /// <summary>
    /// Plays the real game scene in the Editor (fresh save: PlayerPrefs are never touched). Any error logged while it runs fails the test.
    /// Performance is never judged here: only the device gate on the OPPO approves it.
    /// </summary>
    public sealed class ClaybreakSceneTests
    {
        private const string ScenePath = "Assets/Claybreak/Content/Claybreak.unity";
        // ClaybreakSceneBuilder.TrackWidth (the play mode tests do not reference the Editor assembly).
        private const float TrackWidth = 8f;

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
            ClaybreakFlow.EditorUsesDeviceGateRun = false;
            ClaybreakFlow.EditorStartsWithoutTap = false;
            ClaybreakFlow.EditorUsesFreshSave = false;
#endif
        }

        // RT-2 device bug: the track had no collider and the crowd fell through the floor while the run kept going.
        // RT-6t.4: the crowd is the instanced swarm (no physics): every unit stays on the track, on the ground, from the
        // start to the firing line, and the army that arrives is the one the gates made.
        [UnityTest]
        public IEnumerator RunKeepsTheCrowdOnTheTrackUntilTheFiringLine()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorStartsWithoutTap = true;
#endif
            yield return LoadScene(false);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            RunCrowd crowd = flow.Crowd;
            foreach (CharacterController controller in Object.FindObjectsByType<CharacterController>(FindObjectsSortMode.None))
            {
                Assert.IsFalse(controller.enabled, "a CharacterController moves something in the run");
            }

            float halfWidth = TrackWidth * 0.5f;
            float startZ = crowd.Position.z;
            float runStart = -1f;
            float widest = 0f;
            float deadline = Time.realtimeSinceStartup + 60f;
            while (Time.realtimeSinceStartup < deadline && flow.State != FlowState.Assault)
            {
                if (flow.State == FlowState.Run && runStart < 0f)
                {
                    runStart = Time.time;
                }

                for (int i = 0; i < crowd.Army; i++)
                {
                    Vector3 unit = crowd.Swarm.UnitGroundPosition(i);
                    widest = Mathf.Max(widest, Mathf.Abs(unit.x));
                }

                yield return null;
            }

            Assert.AreEqual(FlowState.Assault, flow.State, "the run never reached the firing line");
            Assert.LessOrEqual(widest, halfWidth, "a unit left the track");
            Assert.Greater(crowd.Position.z - startZ, flow.ActiveLevel.FiringLineZ - 1f);
            Assert.Greater(crowd.Army, 0, "the army is empty at the firing line");
            Assert.AreEqual(0f, crowd.Speed, "the crowd kept running in the assault");
            float runSeconds = Time.time - runStart;
            Assert.That(runSeconds, Is.InRange(10f, 40f), "run length out of the GDD block (15-30 s)");

            // Level 1 untouched (no drag): the crowd runs down the middle, so it takes every right-hand gate.
            int expected = flow.ActiveLevel.StartCount;
            foreach (RunGatePair pair in flow.ActiveLevel.RunGates)
            {
                expected = GateMath.Apply(pair.right, expected, crowd.MaximumArmy);
            }

            Assert.AreEqual(expected, crowd.Army, "the gates did not make the army");
            Assert.AreEqual(expected, flow.Crowd.Swarm.Count);

            // The assault shot keeps its own field of view (the run rig widens it for the portrait fit).
            for (float wait = 0f; wait < 1f; wait += Time.deltaTime)
            {
                yield return null;
            }

            var assaultCamera = Object.FindFirstObjectByType<AssaultCamera>();
            Assert.AreEqual(assaultCamera.FieldOfView, assaultCamera.GetComponent<Camera>().fieldOfView, 0.01f,
                "the assault camera kept the run rig's field of view");
        }

        // RT-6t.4: the run gate keeps the crowd at its cap on the track (looping before the tower), never assaulting.
        [UnityTest]
        public IEnumerator DeviceGateRunHoldsTheCrowdCapOnTheTrack()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateRun = true;
#endif
            yield return LoadScene(true);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            Assert.IsTrue(flow.MeasuresRun);
            RunCrowd crowd = flow.Crowd;
            Assert.AreEqual(300, crowd.MaximumArmy, "the crowd cap is decision D27's 300");
            bool looped = false;
            float lastZ = crowd.Position.z;
            float deadline = Time.realtimeSinceStartup + 20f;
            while (Time.realtimeSinceStartup < deadline && !looped)
            {
                Assert.AreEqual(FlowState.Run, flow.State, "the run gate left the run");
                Assert.AreEqual(crowd.MaximumArmy, crowd.Army, "the crowd is not at its cap");
                looped = crowd.Position.z < lastZ;
                lastZ = crowd.Position.z;
                yield return null;
            }

            Assert.IsTrue(looped, "the run gate never looped back to the start");
            for (int i = 0; i < 30; i++)
            {
                yield return null;
            }

            Assert.AreEqual(FlowState.Run, flow.State);
            Assert.AreEqual(crowd.MaximumArmy, crowd.Swarm.Count);
        }

        [UnityTest]
        public IEnumerator GameOpensOnTheMenuWithTheFirstLevelReady()
        {
            yield return LoadScene(false);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsNotNull(flow);
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            Assert.AreEqual(FlowState.Menu, flow.State);
            Assert.AreEqual(0, flow.Progress.LevelIndex);
            Assert.IsFalse(flow.ActiveLevel.IsDeviceGateLevel);
            Assert.IsTrue(GameObject.Find("Home") != null, "the home screen is not shown");

            Assert.AreEqual(flow.ActiveLevel.StartCount, flow.Crowd.Army);
            Assert.AreEqual(flow.Progress.Skin.color, flow.Crowd.Swarm.Tint, "the crowd does not wear the skin");
        }

        [UnityTest]
        public IEnumerator DeviceGateLevelHolds300UnitsInFlightAndTheProjectileLimit()
        {
            yield return LoadScene(true);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            Assert.IsTrue(flow.ActiveLevel.IsDeviceGateLevel);

            float deadline = Time.realtimeSinceStartup + 30f;
            while (Time.realtimeSinceStartup < deadline
                   && (flow.Field.PeakInFlight < 300 || flow.Cannon.PeakActive < flow.ActiveLevel.MaximumProjectiles))
            {
                yield return null;
            }

            Assert.AreEqual(FlowState.Assault, flow.State);
            Assert.AreEqual(TowerKind.Boss, flow.ActiveLevel.Tower, "the gate runs the heaviest tower");
            Assert.AreEqual(300, flow.Field.PeakInFlight, "units in flight never reached the pool");
            Assert.AreEqual(flow.ActiveLevel.MaximumProjectiles, flow.Cannon.PeakActive, "the cannon never reached its limit");
            // RT-5: the gate runs the maximum loadout (every upgrade, both elite slots, the dearest skin).
            Assert.AreEqual(ClaybreakCatalog.MaximumLoadoutSlots, flow.LoadoutCount, "the loadout is not full");
            Assert.Greater(flow.ActiveLoadout.DamageMultiplier, 1f);
            Assert.Greater(flow.ActiveLoadout.LaunchRateMultiplier, 1f);
            Assert.Greater(flow.Field.SpecialsInFlight, 0, "no elite units in flight");

            // Recycled units keep the field full while the measurement runs.
            for (int i = 0; i < 60; i++)
            {
                yield return null;
            }

            Assert.AreEqual(300, flow.Field.InFlight);
            Assert.LessOrEqual(flow.Cannon.ActiveCount, flow.ActiveLevel.MaximumProjectiles);
        }

        // RT-5 end to end: the shop sells an upgrade, an elite and a skin through its buttons, and the next
        // run starts with them (bigger army, elite crates, new color).
        [UnityTest]
        public IEnumerator ShopPurchasesReachTheNextRun()
        {
            yield return LoadScene(false);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            RunCrowd crowd = flow.Crowd;
            Assert.AreEqual(FlowState.Menu, flow.State);
            flow.Progress.Save.coins = 5000;
            Press("/Ui/Screens/Home/Safe/ShopButton");
            Assert.IsTrue(flow.Screens.ShopShown, "the shop did not open");

            Press("/Ui/Screens/Shop/Safe/ShopList/Content/Row1/Action");
            Assert.AreEqual(1, flow.Progress.UpgradeLevel(0), "the army upgrade was not bought");
            Press("/Ui/Screens/Shop/Safe/TabELITES");
            Assert.AreEqual(ShopTab.Units, flow.Screens.Tab);
            Press("/Ui/Screens/Shop/Safe/ShopList/Content/Row2/Action");
            Assert.IsTrue(flow.Progress.IsUnitEquipped(1), "the Sprinter was not bought into the loadout");
            Press("/Ui/Screens/Shop/Safe/TabSKINS");
            Press("/Ui/Screens/Shop/Safe/ShopList/Content/Row2/Action");
            Assert.AreEqual("ember", flow.Progress.Skin.id, "the skin was not bought and worn");
            Assert.Less(flow.Progress.Save.coins, 5000);

            Press("/Ui/Screens/Shop/Safe/Back");
            Assert.IsTrue(GameObject.Find("Home") != null, "BACK did not return home");
            Assert.AreEqual(flow.ActiveLevel.StartCount + flow.ActiveLoadout.StartingArmy, crowd.Army);
            Assert.AreEqual(flow.Progress.Skin.color, crowd.Swarm.Tint, "the crowd does not wear the new skin");
            Assert.AreEqual(5, flow.ActiveLoadout.StartingArmy);
            Assert.AreEqual(1, flow.LoadoutCount);

            Press("/Ui/Screens/Home/Safe/WorldsButton");
            Assert.IsTrue(flow.Screens.WorldsShown, "the world list did not open");
            Press("/Ui/Screens/Worlds/Safe/Back");
            Press("/Ui/Screens/Home/Safe/Play");
            Assert.AreEqual(FlowState.Run, flow.State);
            yield return null;
        }

        // RT-8: SETTINGS opens the settings page; its switches drive the kit's preferences and come back as they
        // were; the policy link says when it is not published yet; BACK returns home.
        [UnityTest]
        public IEnumerator SettingsPageDrivesTheKitSwitches()
        {
            yield return LoadScene(false);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            bool sound = GameAudio.SoundMuted;
            bool music = GameAudio.MusicMuted;
            bool vibration = GameHaptics.Enabled;
            try
            {
                Press("/Ui/Screens/Home/Safe/SettingsButton");
                Assert.IsTrue(flow.Screens.SettingsShown, "the settings page did not open");
                Press("/Ui/Screens/Settings/Safe/Sound");
                Assert.AreEqual(!sound, GameAudio.SoundMuted, "SOUND did not switch");
                Press("/Ui/Screens/Settings/Safe/Music");
                Assert.AreEqual(!music, GameAudio.MusicMuted, "MUSIC did not switch");
                Press("/Ui/Screens/Settings/Safe/Vibration");
                Assert.AreEqual(!vibration, GameHaptics.Enabled, "VIBRATION did not switch");
                Press("/Ui/Screens/Settings/Safe/PrivacyPolicy");
                var note = GameObject.Find("/Ui/Screens/Settings/Safe/Note").GetComponent<UnityEngine.UI.Text>();
                Assert.IsNotEmpty(note.text, "the policy link said nothing while its page is not published");
                Press("/Ui/Screens/Settings/Safe/Back");
                Assert.IsFalse(flow.Screens.SettingsShown);
                Assert.IsTrue(GameObject.Find("Home") != null, "BACK did not return home");
            }
            finally
            {
                GameAudio.SetSoundMuted(sound);
                GameAudio.SetMusicMuted(music);
                GameHaptics.SetEnabled(vibration);
            }

            yield return null;
        }

        private static void Press(string path)
        {
            GameObject target = GameObject.Find(path);
            Assert.IsNotNull(target, path + " is not on screen");
            var button = target.GetComponent<UnityEngine.UI.Button>();
            Assert.IsTrue(button.interactable, path + " is disabled");
            button.onClick.Invoke();
        }

        // End to end without touch: reach the assault, bring the tower down, check the saved progress and
        // that one tap on the result's big button starts the next level.
        [UnityTest]
        public IEnumerator WinningMovesToTheNextLevelInOneTap()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorStartsWithoutTap = true;
#endif
            yield return LoadScene(false);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            float deadline = Time.realtimeSinceStartup + 60f;
            while (Time.realtimeSinceStartup < deadline && flow.State != FlowState.Assault)
            {
                yield return null;
            }

            Assert.AreEqual(FlowState.Assault, flow.State, "the run never reached the firing line");
            flow.Tower.Hit(flow.Tower.Health);
            Assert.AreEqual(FlowState.Won, flow.State);
            Assert.AreEqual(1, flow.Progress.LevelIndex, "the win did not move the world forward");
            Assert.Greater(flow.Progress.Save.coins, 0, "the win paid nothing");
            deadline = Time.realtimeSinceStartup + 5f;
            GameObject primary = null;
            while (Time.realtimeSinceStartup < deadline && (primary = GameObject.Find("Primary")) == null)
            {
                yield return null;
            }

            Assert.IsNotNull(primary, "the result screen never showed its button");
            primary.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Assert.AreEqual(FlowState.Run, flow.State);
            Assert.AreEqual("Level02", flow.ActiveLevel.name);
        }

        // RT-6: every world's device gate shows its own scenery and boss tower; from world 2 on the shield
        // stands over the tower for the whole measurement and takes part of every hit.
        [UnityTest]
        public IEnumerator EveryWorldDeviceGateRunsItsSceneryBossAndShield()
        {
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>("Assets/Claybreak/Content/Data/Catalog.asset");
            Assert.IsNotNull(catalog);
            for (int world = 1; world <= catalog.Worlds.Count; world++)
            {
                yield return LoadScene(true, world);
                var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
                Assert.IsTrue(flow.enabled, $"world {world}: the flow disabled itself (see the log)");
                ClaybreakWorld data = catalog.Worlds[world - 1].world;
                Assert.AreEqual(world - 1, flow.Scenery.Shown, "the world's scenery set is shown");
                Assert.AreEqual(data.Levels[data.LevelCount - 1].Tower, flow.ActiveLevel.Tower, "the gate runs the world's boss tower");
                float deadline = Time.realtimeSinceStartup + 20f;
                while (Time.realtimeSinceStartup < deadline && flow.Field.PeakInFlight < 300)
                {
                    yield return null;
                }

                Assert.AreEqual(300, flow.Field.PeakInFlight, $"world {world}: units in flight never reached the pool");
                Assert.AreEqual(flow.ActiveLevel.ShieldEnabled, flow.Field.Shield.IsStanding, "the shield stands when the world has one");
                if (flow.ActiveLevel.ShieldEnabled)
                {
                    Assert.Greater(flow.Field.Shield.Block, 0f);
                }
            }

            ClaybreakFlow.EditorDeviceGateWorld = 1;
        }

        internal static IEnumerator LoadScene(bool deviceGate, int world = 1)
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesFreshSave = true;
            ClaybreakFlow.EditorUsesDeviceGateLevel = deviceGate;
            ClaybreakFlow.EditorDeviceGateWorld = world;
            // The test runner swaps the build scene list, so the Editor loads the scene by path.
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
