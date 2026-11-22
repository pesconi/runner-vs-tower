using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.13 (user decision 2026-11-22): the crowd's leader. It is not part of the army, runs at the front with the whole
    /// formation behind it, decides the gate (the side of the track it is on when it reaches the pair), and the pair it
    /// crosses stays up with the side taken lit and the other out. In the assault it stays at the firing line (never
    /// launched). Nothing is created in play.
    /// </summary>
    public sealed class CrowdLeaderTests
    {
        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorStartsWithoutTap = false;
            ClaybreakFlow.EditorUsesFreshSave = false;
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
#endif
        }

        [UnityTest]
        public IEnumerator TheLeaderLeadsTheCrowdAndDecidesTheGate()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorStartsWithoutTap = true;
#endif
            yield return ClaybreakSceneTests.LoadScene(false);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            yield return WaitFor(() => flow.State == FlowState.Run, 5f);
            RunCrowd crowd = flow.Crowd;
            CrowdLeader leader = crowd.Leader;
            Assert.IsNotNull(leader, "the crowd has no leader");
            Assert.AreEqual(flow.ActiveLevel.StartCount, crowd.Army, "the leader must not count in the army");
            int objectsBefore = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

            // Steer to the left half: the side the untouched run (down the middle) never takes.
            RunGatePair first = flow.ActiveLevel.RunGates[0];
            crowd.Swarm.SteerTo(crowd.TrackCenterX - crowd.TrackHalfWidth * 0.5f);
            yield return new WaitForSeconds(1f);

            // At the front: every unit stands behind the leader, and the leader is where the gates read the crowd.
            // A coroutine resumes between Update and LateUpdate: the drawn leader is still last frame's (one step behind).
            Vector3 front = crowd.Position;
            Assert.AreEqual(front.z, leader.Ground.z, crowd.Speed * Time.maximumDeltaTime + 0.01f, "the leader is not drawn where the run is");
            for (int i = 0; i < crowd.Army; i++)
            {
                Assert.Less(crowd.Swarm.UnitGroundPosition(i).z, front.z - 0.2f, $"unit {i} is not behind the leader");
            }

            Assert.Less(crowd.FormationCenter.z, front.z - crowd.FormationBehind(crowd.Army) + 1f, "the formation is not behind the leader");

            int army = crowd.Army;
            yield return WaitFor(() => crowd.Position.z > first.z + 0.5f, 30f);
            Assert.Less(crowd.Position.x, crowd.TrackCenterX, "the leader was not on the left at the gate");
            Assert.AreEqual(GateMath.Apply(first.left, army, crowd.MaximumArmy), crowd.Army, "the left gate (the leader's) did not decide");

            // The pair stays up: the side taken lit, the other out with its label faded.
            Assert.IsTrue(flow.Gates.PanelOf(0, false).gameObject.activeInHierarchy, "the crossed pair was hidden");
            Assert.AreSame(flow.Gates.LitMaterial(first.left.IsGood), flow.Gates.PanelOf(0, false).sharedMaterial, "the gate taken did not light");
            Assert.AreSame(flow.Gates.OffMaterial, flow.Gates.PanelOf(0, true).sharedMaterial, "the other gate did not go out");
            Assert.Less(flow.Gates.LabelOf(0, true).alpha, 0.5f, "the other gate's label did not fade");
            Assert.AreEqual(1f, flow.Gates.LabelOf(0, false).alpha, 0.001f);
            Assert.AreEqual(objectsBefore,
                UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length,
                "objects were created in play");
        }

        [UnityTest]
        public IEnumerator TheLeaderStaysAtTheFiringLineInTheAssault()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorStartsWithoutTap = true;
#endif
            yield return ClaybreakSceneTests.LoadScene(false);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            yield return WaitFor(() => flow.State == FlowState.Assault, 60f);
            Assert.AreEqual(FlowState.Assault, flow.State, "the run never reached the firing line");
            CrowdLeader leader = flow.Crowd.Leader;
            float lineZ = leader.Ground.z;
            Assert.GreaterOrEqual(lineZ, flow.ActiveLevel.FiringLineZ - 0.01f, "the leader stopped before the firing line");
            Assert.Less(lineZ, flow.ActiveLevel.FiringLineZ + 1f, "the leader ran on past the firing line");
            yield return new WaitForSeconds(2f);
            Assert.AreEqual(lineZ, leader.Ground.z, 0.001f, "the leader left the firing line");
            Assert.IsTrue(leader.isActiveAndEnabled);
        }

        private static IEnumerator WaitFor(Func<bool> condition, float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            yield return null;
        }
    }
}
