using System;
using System.Collections;
using System.IO;
using Claybreak.Lab;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.3 record: the toon lab from 10 to 1000 units (and 1000 steered to the right edge) rendered on portrait
    /// screens, one sheet per screen shape: 9:16 and the OPPO CPH2791 (1080×2354). Runs only when asked and with a GPU
    /// (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter ToonLabCaptureTests -toonLabCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class ToonLabCaptureTests
    {
        private const string FolderArgument = "-toonLabCaptureDir";
        private static readonly int[] Counts = { 10, 100, 300, 1000, 1000 };

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CapturePortraitSheets()
        {
            string folder = Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
            yield return ToonLabSceneTests.LoadLab();
            yield return Sheet(folder, "toonlab-9x16-10-1000", 540, 960);
            yield return Sheet(folder, "toonlab-oppo-10-1000", 540, 1177);
        }

        private static IEnumerator Sheet(string folder, string name, int width, int height)
        {
            var lab = UnityEngine.Object.FindAnyObjectByType<ToonLab>();
            Camera view = lab.CameraRig.GetComponent<Camera>();
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            view.targetTexture = target;
            const int gap = 8;
            var sheet = new Texture2D(Counts.Length * width + (Counts.Length - 1) * gap, height, TextureFormat.RGB24, false);
            var fill = new Color32[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++)
            {
                fill[i] = new Color32(255, 255, 255, 255);
            }

            sheet.SetPixels32(fill);
            var panel = new Texture2D(width, height, TextureFormat.RGB24, false);
            for (int shot = 0; shot < Counts.Length; shot++)
            {
                lab.SetUnits(Counts[shot]);
                lab.SteerTo(shot == Counts.Length - 1 ? 100f : 0f);
                // The camera renders into the texture every frame; after a frame it holds the settled view
                // (WaitForEndOfFrame never fires in batchmode).
                yield return new WaitForSeconds(3.5f);
                yield return null;
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = target;
                panel.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                panel.Apply();
                RenderTexture.active = previous;
                sheet.SetPixels(shot * (width + gap), 0, width, height, panel.GetPixels());
                Debug.Log($"[ToonLabCapture] {name} units={Counts[shot]} fov={view.fieldOfView:0.#} camera={view.transform.position} framing={lab.CameraRig.FramingScale:0.##} fog={RenderSettings.fogStartDistance:0.#}-{RenderSettings.fogEndDistance:0.#}");
            }

            sheet.Apply();
            File.WriteAllBytes(Path.Combine(folder, name + ".png"), sheet.EncodeToPNG());
            view.targetTexture = null;
            UnityEngine.Object.Destroy(target);
            UnityEngine.Object.Destroy(panel);
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
