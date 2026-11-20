using System.Collections;
using JungleDegree.StudioKit.Crowd;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Claybreak.Tests
{
    /// <summary>
    /// Plays the real prototype scene in the Editor. Any error logged while it runs fails the test.
    /// Performance is never judged here: only the device gate on the OPPO approves it.
    /// </summary>
    public sealed class PrototypeSceneTests
    {
        private const string ScenePath = "Assets/Claybreak/Prototype/ClaybreakPrototype.unity";

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
#endif
        }

        [UnityTest]
        public IEnumerator PlayedLevelWaitsForTheFirstTap()
        {
            yield return LoadScene(false);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsNotNull(flow);
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            Assert.AreEqual(FlowState.Ready, flow.State);
            Assert.IsFalse(flow.ActiveLevel.IsDeviceGateLevel);

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

        private static IEnumerator LoadScene(bool deviceGate)
        {
#if UNITY_EDITOR
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
