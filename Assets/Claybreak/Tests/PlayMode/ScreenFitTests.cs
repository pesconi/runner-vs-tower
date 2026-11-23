using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.16: every menu screen fits from 16:9 to 21:9. The canvas is drawn by the game camera into a texture of each
    /// portrait shape (1080 wide: 1920, 2340, 2400 and 2520 tall), so the canvas scaler sizes it as that phone would; on
    /// each screen (home, settings, the three shop tabs, worlds, result, continue offer) every active button must lie
    /// inside the screen, and a button in a scroll list inside the list's sides (the list's mask cuts the rest). With a GPU
    /// and <c>-screenFitCaptureDir &lt;folder&gt;</c> (batchmode without -nographics) it also saves one sheet per screen
    /// with the four shapes side by side (<c>telas-&lt;screen&gt;.png</c>, half size), plus the run HUD.
    /// </summary>
    public sealed class ScreenFitTests
    {
        private const string FolderArgument = "-screenFitCaptureDir";
        private const int ScreenWidth = 1080;
        private static readonly int[] Heights = { 1920, 2340, 2400, 2520 };
        private static readonly string[] ShapeNames = { "16x9", "19.5x9", "20x9", "21x9" };
        private const int Gap = 12;

        private static readonly string[] Screens = { "home", "ajustes", "loja-upgrades", "loja-elites", "loja-skins", "mundos",
            "resultado-vitoria", "resultado-derrota", "continuar", "hud" };

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesFreshSave = false;
#endif
        }

        [UnityTest]
        public IEnumerator EveryButtonFitsOnEveryPortraitShape()
        {
            string folder = Argument(FolderArgument);
            bool capture = folder != null && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null;
            yield return ClaybreakSceneTests.LoadScene(false);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            Assert.AreEqual(FlowState.Menu, flow.State);
            // Enough coins that the shop shows its buy buttons enabled (the layout is the same either way).
            flow.Progress.Save.coins = 5000;

            var canvas = GameObject.Find("/Ui").GetComponent<Canvas>();
            var canvasRect = (RectTransform)canvas.transform;
            Camera view = flow.Crowd.CameraRig.GetComponent<Camera>();
            RenderMode oldMode = canvas.renderMode;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = view;
            canvas.planeDistance = 1f;

            var sheets = new Dictionary<string, Texture2D>();
            int panelWidth = ScreenWidth / 2;
            int tallest = Heights[Heights.Length - 1] / 2;
            var problems = new List<string>();
            try
            {
                for (int shape = 0; shape < Heights.Length; shape++)
                {
                    int height = Heights[shape];
                    var target = new RenderTexture(ScreenWidth, height, 24, RenderTextureFormat.ARGB32);
                    view.targetTexture = target;
                    yield return null;
                    yield return null;
                    Canvas.ForceUpdateCanvases();
                    Vector2 size = canvasRect.rect.size;
                    Assert.AreEqual(ScreenWidth, size.x, 1f, $"{ShapeNames[shape]}: the canvas does not match the width");
                    Assert.AreEqual(height, size.y, 1f, $"{ShapeNames[shape]}: the canvas is not the screen's shape");

                    for (int s = 0; s < Screens.Length; s++)
                    {
                        Show(flow, Screens[s]);
                        yield return null;
                        Canvas.ForceUpdateCanvases();
                        yield return null;
                        CheckButtons(canvasRect, $"{ShapeNames[shape]} {Screens[s]}", problems);
                        if (capture)
                        {
                            if (!sheets.TryGetValue(Screens[s], out Texture2D sheet))
                            {
                                sheet = Sheet(Heights.Length * panelWidth + (Heights.Length - 1) * Gap, tallest);
                                sheets.Add(Screens[s], sheet);
                            }

                            Paste(target, sheet, shape * (panelWidth + Gap), panelWidth, height / 2);
                        }
                    }

                    view.targetTexture = null;
                    UnityEngine.Object.Destroy(target);
                }

                if (capture)
                {
                    Directory.CreateDirectory(folder);
                    foreach (KeyValuePair<string, Texture2D> sheet in sheets)
                    {
                        sheet.Value.Apply();
                        File.WriteAllBytes(Path.Combine(folder, $"telas-{sheet.Key}.png"), sheet.Value.EncodeToPNG());
                    }
                }
            }
            finally
            {
                view.targetTexture = null;
                canvas.renderMode = oldMode;
                foreach (Texture2D sheet in sheets.Values)
                {
                    UnityEngine.Object.Destroy(sheet);
                }
            }

            Assert.IsEmpty(problems, "buttons off the screen:\n" + string.Join("\n", problems));
        }

        private static void Show(ClaybreakFlow flow, string screen)
        {
            ClaybreakScreens screens = flow.Screens;
            Progression progress = flow.Progress;
            flow.Hud.Show(false);
            switch (screen)
            {
                case "home":
                    screens.ShowHome(progress);
                    break;
                case "ajustes":
                    screens.ShowSettings();
                    break;
                case "loja-upgrades":
                    screens.ShowShop(progress);
                    Press("Ui/Screens/Shop/Safe/TabUPGRADES");
                    break;
                case "loja-elites":
                    Press("Ui/Screens/Shop/Safe/TabELITES");
                    break;
                case "loja-skins":
                    Press("Ui/Screens/Shop/Safe/TabSKINS");
                    break;
                case "mundos":
                    screens.ShowWorlds(progress);
                    break;
                case "resultado-vitoria":
                    screens.ShowResult(new LevelOutcome(true, 3, 120, null, false, null), "ARMY 240  ·  3 ★", false);
                    screens.SetResultAd("WATCH AD  x2 COINS");
                    break;
                case "resultado-derrota":
                    screens.ShowResult(new LevelOutcome(false, 0, 20, null, false, null), "THE TOWER HELD", false);
                    break;
                case "continuar":
                    screens.ShowContinue(40);
                    break;
                default:
                    screens.HideAll();
                    flow.Hud.Show(true);
                    flow.Hud.SetLevel("LEVEL 10");
                    flow.Hud.SetArmy(300);
                    flow.Hud.SetSpecials(12, true);
                    flow.Hud.SetCoins(12345);
                    flow.Hud.ShowTower(true, 2400);
                    flow.Hud.SetTower(1800);
                    break;
            }
        }

        private static void Press(string path)
        {
            GameObject target = GameObject.Find(path);
            Assert.IsNotNull(target, path + " is not on screen");
            target.GetComponent<Button>().onClick.Invoke();
        }

        // Every active button inside the canvas; one under a scroll list also inside the list's left and right sides.
        private static void CheckButtons(RectTransform canvasRect, string where, List<string> problems)
        {
            Rect screen = canvasRect.rect;
            foreach (Button button in canvasRect.GetComponentsInChildren<Button>(false))
            {
                Rect box = Bounds(canvasRect, (RectTransform)button.transform);
                ScrollRect list = button.GetComponentInParent<ScrollRect>();
                if (list != null)
                {
                    Rect viewport = Bounds(canvasRect, list.viewport);
                    if (box.xMin < viewport.xMin - 0.5f || box.xMax > viewport.xMax + 0.5f)
                    {
                        problems.Add($"{where}: {HierarchyPath(button.transform)} x {box.xMin:0}..{box.xMax:0} past the list {viewport.xMin:0}..{viewport.xMax:0}");
                    }

                    continue;
                }

                if (box.xMin < screen.xMin - 0.5f || box.xMax > screen.xMax + 0.5f || box.yMin < screen.yMin - 0.5f
                    || box.yMax > screen.yMax + 0.5f)
                {
                    problems.Add($"{where}: {HierarchyPath(button.transform)} {box} outside the screen {screen}");
                }
            }
        }

        private static Rect Bounds(RectTransform canvasRect, RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 min = Vector2.positiveInfinity;
            Vector2 max = Vector2.negativeInfinity;
            for (int i = 0; i < 4; i++)
            {
                Vector2 local = canvasRect.InverseTransformPoint(corners[i]);
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static string HierarchyPath(Transform transform)
        {
            string path = transform.name;
            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
            {
                path = parent.name + "/" + path;
            }

            return path;
        }

        private static Texture2D Sheet(int width, int height)
        {
            var sheet = new Texture2D(width, height, TextureFormat.RGB24, false);
            var fill = new Color32[width * height];
            for (int i = 0; i < fill.Length; i++)
            {
                fill[i] = new Color32(255, 255, 255, 255);
            }

            sheet.SetPixels32(fill);
            return sheet;
        }

        // The full shape read and halved into the sheet's column, bottom-aligned (the sheet is as tall as the tallest shape).
        private static void Paste(RenderTexture target, Texture2D sheet, int x, int width, int height)
        {
            var full = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            full.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            full.Apply();
            RenderTexture.active = previous;
            var half = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int column = 0; column < width; column++)
                {
                    half[y * width + column] = full.GetPixel(column * 2, y * 2);
                }
            }

            sheet.SetPixels(x, 0, width, height, half);
            UnityEngine.Object.Destroy(full);
        }

        internal static string Argument(string name)
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
