using System;
using System.Collections;
using System.IO;
using Claybreak.Lab;
using JungleDegree.StudioKit.Swarm;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.5 record: the procedural animations of the toon lab on the OPPO CPH2791 screen shape (1080×2354, half size)
    /// — 1000 at rest, a ÷2 bad gate 0.12 s, 0.4 s and 0.8 s later (ballistic death), and 500 → 1000 popping in while
    /// steered hard right (birth and sideways lean) — and the ground under the crowd (risk R-T7c, summed blob shadows):
    /// the lab framing and a close look from above, each with and without the bodies, with the darkening logged. Runs
    /// only when asked and with a GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter AnimationCaptureTests -animationCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class AnimationCaptureTests
    {
        private const string FolderArgument = "-animationCaptureDir";
        private const int Width = 540;
        private const int Height = 1177;
        private const int Gap = 8;

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureTheAnimationsAndTheGround()
        {
            string folder = Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
            yield return ToonLabSceneTests.LoadLab();
            var lab = UnityEngine.Object.FindAnyObjectByType<ToonLab>();
            SwarmRenderer swarm = lab.Swarm;
            Camera view = lab.CameraRig.GetComponent<Camera>();
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            view.targetTexture = target;
            var panel = new Texture2D(Width, Height, TextureFormat.RGB24, false);

            // Animations.
            Texture2D sheet = Sheet(5);
            yield return new WaitForSeconds(3f);
            Shot(sheet, 0, "rest", target, panel, swarm);
            lab.HalveCrowd();
            yield return new WaitForSeconds(0.12f);
            Shot(sheet, 1, "halved+0.12s", target, panel, swarm);
            yield return new WaitForSeconds(0.28f);
            Shot(sheet, 2, "halved+0.4s", target, panel, swarm);
            yield return new WaitForSeconds(0.4f);
            Shot(sheet, 3, "halved+0.8s", target, panel, swarm);
            yield return new WaitForSeconds(2.5f);
            lab.SetUnits(1000);
            lab.SteerTo(100f);
            yield return new WaitForSeconds(0.25f);
            Shot(sheet, 4, "pop+lean", target, panel, swarm);
            Save(sheet, folder, "animacoes-oppo.png");

            // The ground under 1000 settled units: the lab framing, then a close look from above.
            lab.SteerTo(0f);
            yield return new WaitForSeconds(3f);
            Texture2D ground = Sheet(4);
            Shot(ground, 0, "ground-lab-bodies", target, panel, swarm);
            swarm.DrawBodies = false;
            yield return null;
            Shot(ground, 1, "ground-lab-shadows", target, panel, swarm);
            LogDarkening("lab", panel);

            lab.CameraRig.enabled = false;
            Vector3 center = swarm.transform.position;
            view.transform.position = center + new Vector3(0f, 6f, -3f);
            view.transform.LookAt(center);
            yield return null;
            Shot(ground, 3, "ground-close-shadows", target, panel, swarm);
            LogDarkening("close", panel);
            swarm.DrawBodies = true;
            yield return null;
            Shot(ground, 2, "ground-close-bodies", target, panel, swarm);
            Save(ground, folder, "chao-sob-multidao.png");

            lab.CameraRig.enabled = true;
            view.targetTexture = null;
            UnityEngine.Object.Destroy(target);
            UnityEngine.Object.Destroy(panel);
        }

        private static Texture2D Sheet(int shots)
        {
            var sheet = new Texture2D(shots * Width + (shots - 1) * Gap, Height, TextureFormat.RGB24, false);
            var fill = new Color32[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++)
            {
                fill[i] = new Color32(255, 255, 255, 255);
            }

            sheet.SetPixels32(fill);
            return sheet;
        }

        // The camera renders into the texture every frame; the last finished frame is read (WaitForEndOfFrame never
        // fires in batchmode).
        private static void Shot(Texture2D sheet, int index, string label, RenderTexture target, Texture2D panel, SwarmRenderer swarm)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            panel.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            panel.Apply();
            RenderTexture.active = previous;
            sheet.SetPixels(index * (Width + Gap), 0, Width, Height, panel.GetPixels());
            Debug.Log($"[AnimationCapture] {label} units={swarm.Count} dying={swarm.DyingCount} bodies={swarm.DrawBodies}");
        }

        // Luminance spread of a shadows-only shot: the bare ground is the bright end (the sky and track are lighter
        // still, so the 99th percentile stands in for it), the most stacked shadows the dark end.
        private static void LogDarkening(string label, Texture2D panel)
        {
            Color32[] pixels = panel.GetPixels32();
            var luminance = new float[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                luminance[i] = (0.2126f * pixels[i].r + 0.7152f * pixels[i].g + 0.0722f * pixels[i].b) / 255f;
            }

            Array.Sort(luminance);
            float darkest = luminance[0];
            float p01 = luminance[luminance.Length / 100];
            float p50 = luminance[luminance.Length / 2];
            float p99 = luminance[luminance.Length * 99 / 100];
            Debug.Log($"[AnimationCapture] darkening {label}: min={darkest:0.000} p1={p01:0.000} p50={p50:0.000} p99={p99:0.000} "
                + $"min/p50={darkest / Mathf.Max(p50, 1e-3f):0.00}");
        }

        private static void Save(Texture2D sheet, string folder, string name)
        {
            sheet.Apply();
            File.WriteAllBytes(Path.Combine(folder, name), sheet.EncodeToPNG());
            UnityEngine.Object.Destroy(sheet);
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
