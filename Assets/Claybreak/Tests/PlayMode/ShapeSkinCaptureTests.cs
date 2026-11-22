using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.6b record, on the OPPO CPH2791 screen shape (1080×2354, at half or quarter size):
    /// <list type="bullet">
    /// <item><c>menu-forma.png</c>: the home screen with the shape picker, before and after one tap (the crowd waiting behind
    /// the menu changes shape).</item>
    /// <item><c>formas-mundos.png</c>: both crowd shapes (rows) in the four worlds (columns), wearing the default skin (each
    /// world's color), in the world's device-gate run (the crowd at its cap).</item>
    /// <item><c>skins-mundos.png</c>: the default skin and the 8 paid skins (columns) in the four worlds (rows), the eye check
    /// behind the validator's contrast warning (D26 A).</item>
    /// </list>
    /// Runs only when asked and with a GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter ShapeSkinCaptureTests -shapeSkinCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class ShapeSkinCaptureTests
    {
        private const string FolderArgument = "-shapeSkinCaptureDir";
        private const string CatalogPath = "Assets/Claybreak/Content/Data/Catalog.asset";
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
        public IEnumerator CaptureTheMenuShapePicker()
        {
            string folder = Folder();
            yield return ClaybreakSceneTests.LoadScene(false);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.AreEqual(FlowState.Menu, flow.State);
            Camera view = flow.Crowd.CameraRig.GetComponent<Camera>();
            // The overlay canvas never reaches a render texture: draw it through the run camera, as the edit-mode captures do.
            var canvas = GameObject.Find("/Ui").GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = view;
            canvas.planeDistance = 1f;
            using var shot = new Shot(view, Width, Height);
            using var sheet = new Sheet(2, 1, Width, Height);
            yield return new WaitForSeconds(1f);
            yield return shot.Take();
            sheet.Put(0, 0, shot.Panel);
            Assert.AreEqual(0, flow.Progress.ShapeIndex);

            GameObject.Find("/Ui/Screens/Home/Safe/ShapeButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Assert.AreEqual(1, flow.Progress.ShapeIndex, "the picker did not move to the next shape");
            yield return new WaitForSeconds(0.5f);
            yield return shot.Take();
            sheet.Put(1, 0, shot.Panel);
            sheet.Save(Path.Combine(folder, "menu-forma.png"));
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureBothShapesInTheFourWorlds()
        {
            string folder = Folder();
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(CatalogPath);
            int worlds = catalog.Worlds.Count;
            int shapes = catalog.Shapes.Count;
            using var sheet = new Sheet(worlds, shapes, Width, Height);
            for (int world = 1; world <= worlds; world++)
            {
                ClaybreakFlow flow = null;
                yield return LoadWorldRun(world, loaded => flow = loaded);
                using var shot = new Shot(flow.Crowd.CameraRig.GetComponent<Camera>(), Width, Height);
                // The gate loadout wears the dearest skin: put the default one back, so the crowd takes the world's color.
                Assert.IsTrue(flow.Progress.TryWearSkin(0));
                flow.Crowd.Wear(flow.Progress.CrowdColorIn(world - 1));
                for (int shape = 0; shape < shapes; shape++)
                {
                    flow.Crowd.UseShape(catalog.Shapes[shape].mesh);
                    yield return new WaitForSeconds(shape == 0 ? 1.5f : 0.1f);
                    yield return shot.Take();
                    sheet.Put(world - 1, shape, shot.Panel);
                    Debug.Log($"[ShapeSkinCapture] world {world} shape {catalog.Shapes[shape].id} army={flow.Crowd.Army}");
                }
            }

            sheet.Save(Path.Combine(folder, "formas-mundos.png"));
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureEverySkinInTheFourWorlds()
        {
            string folder = Folder();
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(CatalogPath);
            int worlds = catalog.Worlds.Count;
            int skins = catalog.Skins.Count;
            const int small = 2;
            using var sheet = new Sheet(skins, worlds, Width / small, Height / small);
            for (int world = 1; world <= worlds; world++)
            {
                ClaybreakFlow flow = null;
                yield return LoadWorldRun(world, loaded => flow = loaded);
                using var shot = new Shot(flow.Crowd.CameraRig.GetComponent<Camera>(), Width / small, Height / small);
                flow.Crowd.UseShape(catalog.Shapes[0].mesh);
                for (int skin = 0; skin < skins; skin++)
                {
                    Assert.IsTrue(flow.Progress.TryWearSkin(skin), "the gate loadout owns every skin");
                    flow.Crowd.Wear(flow.Progress.CrowdColorIn(world - 1));
                    yield return new WaitForSeconds(skin == 0 ? 1.5f : 0.1f);
                    yield return shot.Take();
                    sheet.Put(skin, world - 1, shot.Panel);
                }
            }

            sheet.Save(Path.Combine(folder, "skins-mundos.png"));
        }

        private static IEnumerator LoadWorldRun(int world, Action<ClaybreakFlow> loaded)
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateRun = true;
#endif
            yield return ClaybreakSceneTests.LoadScene(true, world);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, $"world {world}: the flow disabled itself (see the log)");
            Assert.IsFalse(flow.Crowd.Swarm.Headless, "no GPU: nothing to capture");
            loaded(flow);
        }

        private static string Folder()
        {
            string folder = Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
            return folder;
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

        // One camera drawn into a render texture; WaitForEndOfFrame never fires in batchmode, so the texture is read a
        // frame after the camera drew into it.
        internal sealed class Shot : IDisposable
        {
            private readonly Camera view;
            private readonly RenderTexture target;

            public Shot(Camera camera, int width, int height)
            {
                view = camera;
                target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                Panel = new Texture2D(width, height, TextureFormat.RGB24, false);
                view.targetTexture = target;
            }

            public Texture2D Panel { get; }

            public IEnumerator Take()
            {
                yield return null;
                yield return null;
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = target;
                Panel.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                Panel.Apply();
                RenderTexture.active = previous;
            }

            public void Dispose()
            {
                if (view != null)
                {
                    view.targetTexture = null;
                }

                UnityEngine.Object.Destroy(target);
                UnityEngine.Object.Destroy(Panel);
            }
        }

        // Panels in a grid on white (column 0 on the left, row 0 at the top).
        internal sealed class Sheet : IDisposable
        {
            private readonly Texture2D texture;
            private readonly int width;
            private readonly int height;
            private readonly int rows;

            public Sheet(int columns, int rowCount, int panelWidth, int panelHeight)
            {
                width = panelWidth;
                height = panelHeight;
                rows = rowCount;
                texture = new Texture2D(columns * width + (columns - 1) * Gap, rows * height + (rows - 1) * Gap, TextureFormat.RGB24, false);
                var fill = new Color32[texture.width * texture.height];
                for (int i = 0; i < fill.Length; i++)
                {
                    fill[i] = new Color32(255, 255, 255, 255);
                }

                texture.SetPixels32(fill);
            }

            public void Put(int column, int row, Texture2D panel)
            {
                texture.SetPixels(column * (width + Gap), (rows - 1 - row) * (height + Gap), width, height, panel.GetPixels());
            }

            public void Save(string path)
            {
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }

            public void Dispose()
            {
                UnityEngine.Object.Destroy(texture);
            }
        }
    }
}
