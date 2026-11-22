using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.13 record: the crowd's leader in the game scene, on the OPPO CPH2791 screen shape (1080×2354, half size): level 1
    /// a moment after the start (the leader with its crown at the front), the leader steered left just before the first pair,
    /// just after it (the side it took lit, the other out), the crowd at its cap behind the leader, and the assault with the
    /// leader at the firing line. Runs only when asked and with a GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.LeaderCaptureTests -leaderCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class LeaderCaptureTests
    {
        private const string FolderArgument = "-leaderCaptureDir";
        private const int Width = 540;
        private const int Height = 1177;

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorStartsWithoutTap = false;
            ClaybreakFlow.EditorUsesFreshSave = false;
#endif
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureTheLeader()
        {
            string folder = Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
#if UNITY_EDITOR
            ClaybreakFlow.EditorStartsWithoutTap = true;
#endif
            yield return ClaybreakSceneTests.LoadScene(false);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            RunCrowd crowd = flow.Crowd;
            Assert.IsFalse(crowd.Swarm.Headless, "no GPU: nothing to capture");
            Camera view = crowd.CameraRig.GetComponent<Camera>();
            using var shot = new ShapeSkinCaptureTests.Shot(view, Width, Height);
            using var sheet = new ShapeSkinCaptureTests.Sheet(5, 1, Width, Height);
            yield return WaitFor(() => flow.State == FlowState.Run, 5f);

            yield return new WaitForSeconds(1f);
            yield return Take(0, "start", flow, shot, sheet);

            RunGatePair first = flow.ActiveLevel.RunGates[0];
            crowd.Swarm.SteerTo(crowd.TrackCenterX - crowd.TrackHalfWidth * 0.5f);
            yield return WaitFor(() => crowd.Position.z > first.z - 3f, 30f);
            yield return Take(1, "before-gate", flow, shot, sheet);

            yield return WaitFor(() => crowd.Position.z > first.z + 2.5f, 10f);
            yield return Take(2, "after-gate", flow, shot, sheet);

            crowd.Swarm.SteerTo(crowd.TrackCenterX);
            crowd.SetArmy(crowd.MaximumArmy);
            yield return new WaitForSeconds(2.5f);
            yield return Take(3, "cap", flow, shot, sheet);

            yield return WaitFor(() => flow.State == FlowState.Assault, 40f);
            Assert.AreEqual(FlowState.Assault, flow.State, "the run never reached the firing line");
            yield return new WaitForSeconds(1.5f);
            yield return Take(4, "assault", flow, shot, sheet);

            sheet.Save(Path.Combine(folder, "lider-oppo.png"));
        }

        private static IEnumerator Take(int column, string label, ClaybreakFlow flow, ShapeSkinCaptureTests.Shot shot,
            ShapeSkinCaptureTests.Sheet sheet)
        {
            yield return shot.Take();
            sheet.Put(column, 0, shot.Panel);
            CrowdLeader leader = flow.Crowd.Leader;
            Debug.Log($"[LeaderCapture] {label} state={flow.State} army={flow.Crowd.Army} leader=({leader.Ground.x:0.0}, {leader.Ground.z:0.0}) "
                + $"formation z={flow.Crowd.FormationCenter.z:0.0} crown={leader.CrownColor} draws={leader.DrawCalls}");
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

        private static string Argument(string name)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                if (arguments[i] == name)
                {
                    return arguments[i + 1];
                }
            }

            return null;
        }
    }
}
