using System;
using System.Collections;
using System.IO;
using JungleDegree.StudioKit.Swarm;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.7 record: the heaviest assault (device-gate level of world 4: the boss tower, every defense, 300 units in
    /// flight with the elites, the heaviest shape and the dearest skin) drawn by the instanced units in flight, on the OPPO
    /// CPH2791 screen shape (1080×2354, half size), in the boss assault shot: the field filling up, then full with the
    /// defenses killing (ballistic deaths). Proves the units in flight draw in the game scene (toon shader, per-instance
    /// size and color, deaths through the ring). Runs only when asked and with a GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.AssaultCaptureTests -assaultCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class AssaultCaptureTests
    {
        private const string FolderArgument = "-assaultCaptureDir";
        private const int Width = 540;
        private const int Height = 1177;
        private const int Shots = 2;
        private const int Gap = 8;

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
            ClaybreakFlow.EditorDeviceGateWorld = 1;
#endif
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureTheHeaviestAssault()
        {
            string folder = Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
            yield return ClaybreakSceneTests.LoadScene(true, 4);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            LooseSwarmRenderer flyers = flow.Field.Flyers;
            Assert.IsFalse(flyers.Headless, "no GPU: nothing to capture");
            Camera view = flow.Crowd.CameraRig.GetComponent<Camera>();
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            view.targetTexture = target;
            var sheet = new Texture2D(Shots * Width + (Shots - 1) * Gap, Height, TextureFormat.RGB24, false);
            var fill = new Color32[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++)
            {
                fill[i] = new Color32(255, 255, 255, 255);
            }

            sheet.SetPixels32(fill);
            var panel = new Texture2D(Width, Height, TextureFormat.RGB24, false);

            yield return WaitFor(() => flow.State == FlowState.Assault, 10f);
            Assert.AreEqual(FlowState.Assault, flow.State);
            yield return new WaitForSeconds(1.5f);
            yield return null;
            Shot(0, "filling", flow, view, target, panel, sheet);

            yield return WaitFor(() => flow.Field.InFlight >= 300 && flyers.DyingCount > 0, 20f);
            Assert.AreEqual(300, flyers.Count, "the field never filled");
            Assert.Greater(flyers.DyingCount, 0, "nobody dies in the heaviest assault");
            Shot(1, "full", flow, view, target, panel, sheet);

            sheet.Apply();
            File.WriteAllBytes(Path.Combine(folder, "assalto-oppo.png"), sheet.EncodeToPNG());
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
            sheet.SetPixels(index * (Width + Gap), 0, Width, Height, panel.GetPixels());
            LooseSwarmRenderer flyers = flow.Field.Flyers;
            Debug.Log($"[AssaultCapture] {label} level={flow.ActiveLevel.name} tower={flow.ActiveLevel.Tower} inFlight={flow.Field.InFlight} "
                + $"drawn={flyers.Count} specials={flow.Field.SpecialsInFlight} dying={flyers.DyingCount} fov={view.fieldOfView:0.#} "
                + $"camera={view.transform.position} shape={flyers.UnitMesh.name} tris/unit={flyers.UnitTriangles} tint={flow.Field.CrowdColor}");
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
