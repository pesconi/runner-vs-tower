using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.16 (bug 2a): the boss's spiked arm (<see cref="SweeperDefense"/>) swings while the assault runs. Before, the flow
    /// switched the field on but nothing switched the arm on, so it stood still at the edge and killed only in that fixed band.
    /// The test plays the world 1 device-gate level (the boss with every defense) and checks that the arm moves, that the drawn
    /// arm follows it and that the deadly band goes with it; stopping the field stops the arm. With a GPU and
    /// <c>-sweeperCaptureDir &lt;folder&gt;</c> (batchmode without -nographics) it also saves the boss at two instants
    /// (<c>chefe-braco-dois-instantes.png</c>), the arm in two places.
    /// </summary>
    public sealed class SweeperMotionTests
    {
        private const string FolderArgument = "-sweeperCaptureDir";
        private const int Width = 540;
        private const int Height = 1177;
        private const int Gap = 8;

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
            ClaybreakFlow.EditorUsesFreshSave = false;
            ClaybreakFlow.EditorDeviceGateWorld = 1;
#endif
        }

        [UnityTest]
        public IEnumerator BossArmSwingsAndItsDeadlyBandFollowsIt()
        {
            yield return ClaybreakSceneTests.LoadScene(true, 1);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            float deadline = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < deadline && flow.State != FlowState.Assault)
            {
                yield return null;
            }

            Assert.AreEqual(FlowState.Assault, flow.State, "no assault");
            Assert.IsTrue(flow.ActiveLevel.SweeperEnabled, "the gate level's boss has no sweeper");
            SweeperDefense sweeper = flow.Field.Sweeper;
            Assert.IsTrue(sweeper.IsPresent, "the sweeper is not placed");
            Assert.IsTrue(flow.Field.Running, "the field is not running in the assault");
            Assert.IsTrue(sweeper.Running, "the field runs but the arm does not (the RT-6t.16 bug)");

            // Wait until the arm is well away from where it was: past two half-widths, so the old place is out of its band.
            float startX = sweeper.ArmX;
            float far = 0f;
            deadline = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < deadline && far < 2.5f)
            {
                yield return null;
                far = Mathf.Abs(sweeper.ArmX - startX);
            }

            float nowX = sweeper.ArmX;
            Assert.GreaterOrEqual(far, 2.5f, $"the arm did not swing (from x={startX:0.00} to x={nowX:0.00})");
            Assert.AreEqual(nowX, sweeper.Arm.position.x, 0.001f, "the drawn arm is not where the arm kills");
            Assert.IsTrue(sweeper.Covers(nowX), "the deadly band is not under the arm");
            Assert.IsFalse(sweeper.Covers(startX), "the deadly band stayed where the arm was");

            // Stopping the field stops the arm (the one switch, AssaultField.Running).
            flow.Field.Running = false;
            Assert.IsFalse(sweeper.Running, "the arm kept running with the field stopped");
            float stoppedX = sweeper.ArmX;
            yield return null;
            yield return null;
            Assert.AreEqual(stoppedX, sweeper.ArmX, 0.0001f, "the arm moved with the field stopped");
            flow.Field.Running = true;
            Assert.IsTrue(sweeper.Running);
        }

        [UnityTest]
        public IEnumerator CaptureTheBossArmAtTwoInstants()
        {
            string folder = ScreenFitTests.Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
            yield return ClaybreakSceneTests.LoadScene(true, 1);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            float deadline = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < deadline && flow.State != FlowState.Assault)
            {
                yield return null;
            }

            Assert.AreEqual(FlowState.Assault, flow.State, "no assault");
            SweeperDefense sweeper = flow.Field.Sweeper;
            Camera view = flow.Crowd.CameraRig.GetComponent<Camera>();
            var sheet = new Texture2D(2 * Width + Gap, Height, TextureFormat.RGB24, false);
            var panel = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            try
            {
                // The game's own assault camera: the boss, the defenses and the arm's line, as the player sees them.
                view.targetTexture = target;
                // Let the rig settle on the assault shot first.
                yield return new WaitForSeconds(1.5f);
                float[] armX = new float[2];
                float firstX = 0f;
                for (int shot = 0; shot < 2; shot++)
                {
                    if (shot == 1)
                    {
                        // The second instant: once the arm is well away from the first.
                        deadline = Time.realtimeSinceStartup + 10f;
                        while (Time.realtimeSinceStartup < deadline && Mathf.Abs(sweeper.ArmX - firstX) < 3f)
                        {
                            yield return null;
                        }
                    }

                    yield return null;
                    armX[shot] = sweeper.ArmX;
                    firstX = shot == 0 ? armX[0] : firstX;
                    RenderTexture previous = RenderTexture.active;
                    RenderTexture.active = target;
                    panel.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                    panel.Apply();
                    RenderTexture.active = previous;
                    sheet.SetPixels(shot * (Width + Gap), 0, Width, Height, panel.GetPixels());
                }

                sheet.Apply();
                File.WriteAllBytes(Path.Combine(folder, "chefe-braco-dois-instantes.png"), sheet.EncodeToPNG());
                Debug.Log($"[SweeperCapture] arm x at the two instants: {armX[0]:0.00} and {armX[1]:0.00}");
                Assert.Greater(Mathf.Abs(armX[1] - armX[0]), 3f, "the arm did not move between the two captures");
            }
            finally
            {
                view.targetTexture = null;
                UnityEngine.Object.Destroy(target);
                UnityEngine.Object.Destroy(panel);
                UnityEngine.Object.Destroy(sheet);
            }
        }
    }
}
