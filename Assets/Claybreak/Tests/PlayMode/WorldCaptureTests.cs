using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.6a record: the four worlds with their palettes (sky gradient, fog, the default skin wearing the world's crowd
    /// color), on the OPPO CPH2791 screen shape (1080×2354, half size). Each panel is the world's device-gate run (the crowd
    /// at its cap) a moment after the start; the gate's loadout wears the dearest skin, so the capture puts the default skin
    /// back on and the crowd takes the color the rule gives it (the menu test covers the flow's own wiring). RT-6t.8: the toon scenery (striped track,
    /// rails, ground, the world's trees and rocks in its palette and density). Runs only when asked and with a GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter WorldCaptureTests -worldCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class WorldCaptureTests
    {
        private const string FolderArgument = "-worldCaptureDir";
        private const int Width = 540;
        private const int Height = 1177;
        private const int Gap = 8;

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
            ClaybreakFlow.EditorUsesDeviceGateRun = false;
            ClaybreakFlow.EditorUsesFreshSave = false;
            ClaybreakFlow.EditorDeviceGateWorld = 1;
#endif
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureTheFourWorlds()
        {
            string folder = Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>("Assets/Claybreak/Content/Data/Catalog.asset");
            int worlds = catalog.Worlds.Count;
            var sheet = new Texture2D(worlds * Width + (worlds - 1) * Gap, Height, TextureFormat.RGB24, false);
            var fill = new Color32[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++)
            {
                fill[i] = new Color32(255, 255, 255, 255);
            }

            sheet.SetPixels32(fill);
            var panel = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            for (int world = 1; world <= worlds; world++)
            {
#if UNITY_EDITOR
                ClaybreakFlow.EditorUsesDeviceGateRun = true;
#endif
                yield return ClaybreakSceneTests.LoadScene(true, world);
                var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
                Assert.IsTrue(flow.enabled, $"world {world}: the flow disabled itself (see the log)");
                Assert.IsFalse(flow.Crowd.Swarm.Headless, "no GPU: nothing to capture");
                Assert.IsTrue(flow.Progress.TryWearSkin(0));
                // RT-6t.6b: the gate wears the heaviest shape; this record keeps the default one (ShapeSkinCaptureTests shows both).
                flow.Crowd.UseShape(catalog.Shapes[0]);
                flow.Crowd.Wear(flow.Progress.CrowdColorIn(world - 1));
                Camera view = flow.Crowd.CameraRig.GetComponent<Camera>();
                view.targetTexture = target;
                yield return new WaitForSeconds(1.5f);
                // The camera renders into the texture every frame (WaitForEndOfFrame never fires in batchmode).
                yield return null;

                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = target;
                panel.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                panel.Apply();
                RenderTexture.active = previous;
                view.targetTexture = null;
                sheet.SetPixels((world - 1) * (Width + Gap), 0, Width, Height, panel.GetPixels());
                ClaybreakWorld data = catalog.Worlds[world - 1].world;
                Color top = panel.GetPixel(Width / 2, Height - 2);
                Color bottom = panel.GetPixel(Width / 2, 1);
                Debug.Log($"[WorldCapture] world {world} palette={data.Look.palette.name} army={flow.Crowd.Army} tint={flow.Crowd.Swarm.Tint} "
                    + $"fog={RenderSettings.fogColor} {RenderSettings.fogStartDistance}-{RenderSettings.fogEndDistance} "
                    + $"screenTop=#{ColorUtility.ToHtmlStringRGB(top)} screenBottom=#{ColorUtility.ToHtmlStringRGB(bottom)} "
                    + $"skyTop=#{ColorUtility.ToHtmlStringRGB(data.Look.palette.SkyTop)} skyBottom=#{ColorUtility.ToHtmlStringRGB(data.Look.palette.SkyBottom)}");
            }

            sheet.Apply();
            File.WriteAllBytes(Path.Combine(folder, "mundos-oppo.png"), sheet.EncodeToPNG());
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
