using System.Collections;
using JungleDegree.StudioKit.Crowd;
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

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
            ClaybreakFlow.EditorStartsWithoutTap = false;
            ClaybreakFlow.EditorUsesFreshSave = false;
#endif
        }

        // RT-2 device bug: the track had no collider, so the leader (a CharacterController with gravity)
        // and the crowd fell through the floor while the run kept going.
        [UnityTest]
        public IEnumerator RunKeepsTheCrowdOnTheTrackUntilTheFiringLine()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorStartsWithoutTap = true;
#endif
            yield return LoadScene(false);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            var leader = Object.FindFirstObjectByType<PlayerController>().transform;
            var crowd = Object.FindFirstObjectByType<RunnerCrowd>();
            float startZ = leader.position.z;
            float lowestY = leader.position.y;
            float runStart = -1f;
            float deadline = Time.realtimeSinceStartup + 60f;
            while (Time.realtimeSinceStartup < deadline && flow.State != FlowState.Assault)
            {
                if (flow.State == FlowState.Run && runStart < 0f)
                {
                    runStart = Time.time;
                }

                lowestY = Mathf.Min(lowestY, leader.position.y);
                yield return null;
            }

            Assert.AreEqual(FlowState.Assault, flow.State, "the run never reached the firing line");
            Assert.Greater(lowestY, -0.3f, "the leader fell through the track");
            Assert.Greater(leader.position.z - startZ, flow.ActiveLevel.FiringLineZ - 1f);
            Assert.Greater(crowd.CurrentCount, 1, "the army is empty at the firing line");
            float runSeconds = Time.time - runStart;
            Assert.That(runSeconds, Is.InRange(10f, 40f), "run length out of the GDD block (15-30 s)");
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

            var crowd = Object.FindFirstObjectByType<RunnerCrowd>();
            Assert.AreEqual(flow.ActiveLevel.StartCount + 1, crowd.CurrentCount);
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

            // Recycled units keep the field full while the measurement runs.
            for (int i = 0; i < 60; i++)
            {
                yield return null;
            }

            Assert.AreEqual(300, flow.Field.InFlight);
            Assert.LessOrEqual(flow.Cannon.ActiveCount, flow.ActiveLevel.MaximumProjectiles);
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

        private static IEnumerator LoadScene(bool deviceGate)
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesFreshSave = true;
            ClaybreakFlow.EditorUsesDeviceGateLevel = deviceGate;
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
