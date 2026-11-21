using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.4 record: the game run drawn with the instanced crowd, on the OPPO CPH2791 screen shape (1080×2354, half
    /// size): level 1 just after the start, the crowd grown to its cap (300), the crowd steered to the right edge, and
    /// the assault shot at the firing line. Proves the crowd draws in the game scene (toon shader compiled, look applied,
    /// world fog) and the camera handover. Runs only when asked and with a GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter RunCaptureTests -runCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class RunCaptureTests
    {
        private const string FolderArgument = "-runCaptureDir";
        private const int Width = 540;
        private const int Height = 1177;
        private const int Shots = 4;

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorStartsWithoutTap = false;
            ClaybreakFlow.EditorUsesFreshSave = false;
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
#endif
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureTheRun()
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
            Assert.IsFalse(flow.Crowd.Swarm.Headless, "no GPU: nothing to capture");
            Camera view = flow.Crowd.CameraRig.GetComponent<Camera>();
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            view.targetTexture = target;
            const int gap = 8;
            var sheet = new Texture2D(Shots * Width + (Shots - 1) * gap, Height, TextureFormat.RGB24, false);
            var fill = new Color32[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++)
            {
                fill[i] = new Color32(255, 255, 255, 255);
            }

            sheet.SetPixels32(fill);
            var panel = new Texture2D(Width, Height, TextureFormat.RGB24, false);

            yield return WaitFor(() => flow.State == FlowState.Run, 5f);
            yield return new WaitForSeconds(1.5f);
            Shot(0, "start", flow, view, target, panel, sheet);

            flow.Crowd.SetArmy(flow.Crowd.MaximumArmy);
            yield return new WaitForSeconds(2.5f);
            Shot(1, "cap", flow, view, target, panel, sheet);

            // As far right as a drag can take it (the lane minus the drag margin).
            flow.Crowd.Swarm.SteerTo(flow.Crowd.TrackCenterX + flow.Crowd.TrackHalfWidth - 0.5f);
            yield return new WaitForSeconds(1.5f);
            Shot(2, "right", flow, view, target, panel, sheet);

            flow.Crowd.Swarm.SteerTo(0f);
            yield return WaitFor(() => flow.State == FlowState.Assault, 40f);
            Assert.AreEqual(FlowState.Assault, flow.State, "the run never reached the firing line");
            yield return new WaitForSeconds(1.2f);
            Shot(3, "assault", flow, view, target, panel, sheet);

            sheet.Apply();
            File.WriteAllBytes(Path.Combine(folder, "corrida-oppo.png"), sheet.EncodeToPNG());
            view.targetTexture = null;
            UnityEngine.Object.Destroy(target);
            UnityEngine.Object.Destroy(panel);
            UnityEngine.Object.Destroy(sheet);
        }

        private static IEnumerator WaitFor(Func<bool> condition, float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            // The camera renders into the texture every frame (WaitForEndOfFrame never fires in batchmode).
            yield return null;
        }

        private static void Shot(int index, string label, ClaybreakFlow flow, Camera view, RenderTexture target, Texture2D panel,
            Texture2D sheet)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            panel.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            panel.Apply();
            RenderTexture.active = previous;
            sheet.SetPixels(index * (Width + 8), 0, Width, Height, panel.GetPixels());
            Vector3 center = flow.Crowd.Position;
            Debug.Log($"[RunCapture] {label} state={flow.State} army={flow.Crowd.Army} center=({center.x:0.0}, {center.z:0.0}) "
                + $"fov={view.fieldOfView:0.#} camera={view.transform.position} tris/unit={flow.Crowd.Swarm.UnitTriangles} "
                + $"shape={flow.Crowd.Swarm.UnitMesh.name} tint={flow.Crowd.Swarm.Tint}");
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
