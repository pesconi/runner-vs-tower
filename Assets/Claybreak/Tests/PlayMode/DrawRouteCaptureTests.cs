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
    /// RT-6t.11 record: the two draw routes of the instanced units (<see cref="ToonInstanceRoute.Buffer"/>, one structured
    /// buffer; <see cref="ToonInstanceRoute.Chunks"/>, the OpenGL ES fallback in blocks of matrices) render the same frame.
    /// The heaviest run and assault of world 4 (device-gate level: crowd at its cap with deaths, the scenery, 300 in flight
    /// with the elites and the defenses killing) are frozen (time scale 0), rendered once by each route on the OPPO
    /// CPH2791 screen shape (1080×2354, half size) and compared pixel by pixel. Saves the four panels (run buffer, run
    /// chunks, assault buffer, assault chunks). Runs only when asked and with a GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.DrawRouteCaptureTests -routeCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class DrawRouteCaptureTests
    {
        private const string FolderArgument = "-routeCaptureDir";
        private const int Width = 540;
        private const int Height = 1177;
        private const int Shots = 4;
        private const int Gap = 8;

        // A channel off by more than this counts the pixel as different; at most this share of pixels may differ.
        private const int ChannelTolerance = 8;
        private const float DifferentShare = 0.002f;

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            ToonInstanceRouting.Forced = null;
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
            ClaybreakFlow.EditorUsesDeviceGateRun = false;
            ClaybreakFlow.EditorDeviceGateWorld = 1;
#endif
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator BothRoutesDrawTheSameFrame()
        {
            string folder = Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
            var sheet = new Texture2D(Shots * Width + (Shots - 1) * Gap, Height, TextureFormat.RGB24, false);
            var fill = new Color32[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++)
            {
                fill[i] = new Color32(255, 255, 255, 255);
            }

            sheet.SetPixels32(fill);
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            var buffer = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            var chunks = new Texture2D(Width, Height, TextureFormat.RGB24, false);

            // The run: crowd at its cap on the track with the scenery, the outer ring thrown out by the gates.
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateRun = true;
#endif
            ToonInstanceRouting.Forced = ToonInstanceRoute.Buffer;
            yield return ClaybreakSceneTests.LoadScene(true, 4);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            SwarmRenderer swarm = flow.Crowd.Swarm;
            Assert.IsFalse(swarm.Headless, "no GPU: nothing to capture");
            yield return WaitFor(() => swarm.Count >= flow.Crowd.MaximumArmy, 10f);
            yield return new WaitForSeconds(1.5f);
            Camera view = flow.Crowd.CameraRig.GetComponent<Camera>();
            view.targetTexture = target;
            yield return Compare("run", 0, target, buffer, chunks, sheet, () =>
                $"units={swarm.Count} dying={swarm.DyingCount} calls={swarm.DrawCalls} props={flow.Scenery.Props.DrawCalls} route={swarm.Route}");
            Assert.AreEqual(ToonInstanceRoute.Chunks, swarm.Route);
            Assert.AreEqual(ToonInstanceDraw.Calls(ToonInstanceRoute.Chunks, swarm.Count + swarm.DyingCount)
                + ToonInstanceDraw.Calls(ToonInstanceRoute.Chunks, swarm.Count), swarm.DrawCalls, "one call per block");
            view.targetTexture = null;

            // The assault: 300 in flight with the elites, the defenses killing (ballistic deaths).
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateRun = false;
#endif
            ToonInstanceRouting.Forced = ToonInstanceRoute.Buffer;
            yield return ClaybreakSceneTests.LoadScene(true, 4);
            flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            LooseSwarmRenderer flyers = flow.Field.Flyers;
            yield return WaitFor(() => flow.State == FlowState.Assault, 10f);
            yield return WaitFor(() => flow.Field.InFlight >= 300 && flyers.DyingCount > 0, 20f);
            Assert.AreEqual(300, flyers.Count, "the field never filled");
            view = flow.Crowd.CameraRig.GetComponent<Camera>();
            view.targetTexture = target;
            yield return Compare("assault", 2, target, buffer, chunks, sheet, () =>
                $"inFlight={flyers.Count} dying={flyers.DyingCount} calls={flyers.DrawCalls} route={flyers.Route}");
            Assert.AreEqual(ToonInstanceRoute.Chunks, flyers.Route);
            Assert.AreEqual(ToonInstanceDraw.Calls(ToonInstanceRoute.Chunks, flyers.Count + flyers.DyingCount)
                + ToonInstanceDraw.Calls(ToonInstanceRoute.Chunks, flyers.Count), flyers.DrawCalls, "one call per block");
            view.targetTexture = null;

            sheet.Apply();
            File.WriteAllBytes(Path.Combine(folder, "rotas-oppo.png"), sheet.EncodeToPNG());
            UnityEngine.Object.Destroy(target);
            UnityEngine.Object.Destroy(buffer);
            UnityEngine.Object.Destroy(chunks);
            UnityEngine.Object.Destroy(sheet);
        }

        // Freezes the frame, renders it by the buffer route then by the chunks route, puts both on the sheet (panels
        // index and index + 1) and asserts they match.
        private static IEnumerator Compare(string label, int index, RenderTexture target, Texture2D buffer, Texture2D chunks,
            Texture2D sheet, Func<string> describe)
        {
            Time.timeScale = 0f;
            ToonInstanceRouting.Forced = ToonInstanceRoute.Buffer;
            yield return null;
            yield return null;
            Read(target, buffer);
            ToonInstanceRouting.Forced = ToonInstanceRoute.Chunks;
            yield return null;
            yield return null;
            Read(target, chunks);
            Time.timeScale = 1f;

            Color32[] a = buffer.GetPixels32();
            Color32[] b = chunks.GetPixels32();
            int different = 0;
            int largest = 0;
            for (int i = 0; i < a.Length; i++)
            {
                int d = Math.Max(Math.Abs(a[i].r - b[i].r), Math.Max(Math.Abs(a[i].g - b[i].g), Math.Abs(a[i].b - b[i].b)));
                largest = Math.Max(largest, d);
                different += d > ChannelTolerance ? 1 : 0;
            }

            float share = different / (float)a.Length;
            Debug.Log($"[RouteCapture] {label} {describe()} differentPixels={different} ({share:P3}) largestChannelDelta={largest}");
            sheet.SetPixels(index * (Width + Gap), 0, Width, Height, buffer.GetPixels());
            sheet.SetPixels((index + 1) * (Width + Gap), 0, Width, Height, chunks.GetPixels());
            Assert.LessOrEqual(share, DifferentShare, $"{label}: the chunks route does not draw the buffer route's frame");
        }

        private static void Read(RenderTexture target, Texture2D into)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            into.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            into.Apply();
            RenderTexture.active = previous;
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
