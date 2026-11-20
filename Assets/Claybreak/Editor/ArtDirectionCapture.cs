using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Claybreak.Editor
{
    /// <summary>
    /// Renders the art-direction options (RT-4, decision D17) and a few in-game shots to PNG, so the user
    /// can choose on the phone. Needs a graphics device: batchmode WITHOUT -nographics.
    /// -executeMethod Claybreak.Editor.ArtDirectionCapture.CaptureAndQuit [-claybreakCaptureDir C:\temp\rt4\art]
    /// Writes into the capture folder, copies the option sheets into docs/art/rt4 and the game screens into
    /// docs/art/telas.
    /// </summary>
    public static class ArtDirectionCapture
    {
        public const string RepositoryFolder = "docs/art/rt4";
        /// <summary>The game screens as they are now (rewritten on every capture; the RT-4 sheets stay as the D17 record).</summary>
        public const string ScreensFolder = "docs/art/telas";
        private const int Width = 720;
        private const int Height = 960;

        public static void CaptureAndQuit()
        {
            BatchRunner.Run("ArtDirectionCapture", () =>
            {
                string folder = BatchRunner.GetArgument("-claybreakCaptureDir") ?? @"C:\temp\rt4\art";
                Capture(folder);
            });
        }

        [MenuItem("Claybreak/Capture Art Options")]
        public static void CaptureFromMenu()
        {
            Capture(@"C:\temp\rt4\art");
        }

        public static void Capture(string folder)
        {
            Directory.CreateDirectory(folder);
            Directory.CreateDirectory(RepositoryFolder);
            Directory.CreateDirectory(ScreensFolder);
            TowerStyle[] towers = { TowerStyle.StoneBastion, TowerStyle.IronForge, TowerStyle.ClayRuin };
            TowerStyle[] bosses = { TowerStyle.Citadel, TowerStyle.ForgeColossus, TowerStyle.ClayTotem };
            Sheet(folder, "torre-opcoes", towers);
            Sheet(folder, "chefe-opcoes", bosses);
            GameShots(folder);
        }

        private static void Sheet(string folder, string name, TowerStyle[] styles)
        {
            // Pixels, not textures: opening the next scene unloads textures nothing in a scene references.
            var shots = new Color[styles.Length][];
            for (int i = 0; i < styles.Length; i++)
            {
                Texture2D shot = RenderStyle(styles[i]);
                File.WriteAllBytes(Path.Combine(folder, $"{name}-{(char)('A' + i)}-{styles[i]}.png"), shot.EncodeToPNG());
                shots[i] = shot.GetPixels();
                UnityEngine.Object.DestroyImmediate(shot);
            }

            // Side by side, labelled by position (A, B, C left to right), for one look on the phone.
            var sheet = new Texture2D(Width * styles.Length, Height, TextureFormat.RGB24, false);
            for (int i = 0; i < shots.Length; i++)
            {
                sheet.SetPixels(i * Width, 0, Width, Height, shots[i]);
                DrawLetter(sheet, i * Width + 30, Height - 110, i);
            }

            sheet.Apply();
            byte[] png = sheet.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(folder, name + ".png"), png);
            File.WriteAllBytes(Path.Combine(RepositoryFolder, name + ".png"), png);
            UnityEngine.Object.DestroyImmediate(sheet);
            Debug.Log($"[ArtDirectionCapture] {name}: {string.Join(", ", styles)}");
        }

        private static Texture2D RenderStyle(TowerStyle style)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var palette = new ArtPalette();
            ClaybreakSceneBuilder.CreateEnvironment(palette, 60f);
            var root = new GameObject("Tower").transform;
            root.position = new Vector3(0f, 0f, 40f);
            TowerParts parts = TowerArt.Build(style, root, palette);
            var cameraObject = new GameObject("Camera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = RenderSettings.fogColor;
            camera.fieldOfView = 50f;
            Vector3 focus = new Vector3(0f, parts.BarHeight * 0.42f, 43f);
            cameraObject.transform.position = focus + new Vector3(-10f, 3f + parts.BarHeight * 0.15f, -19f);
            cameraObject.transform.LookAt(focus);
            // A scene made in this frame has not registered its sun yet: the first render warms it up.
            UnityEngine.Object.DestroyImmediate(Render(camera, 64, 64));
            return Render(camera, Width, Height);
        }

        // The game scene as the player sees it: home, shop, world list and the assault of the boss.
        private static void GameShots(string folder)
        {
            EditorSceneManager.OpenScene(ClaybreakSceneBuilder.ScenePath, OpenSceneMode.Single);
            var catalog = AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(WorldContent.CatalogPath);
            ClaybreakWorld world = catalog.Worlds[0].world;
            var screens = UnityEngine.Object.FindAnyObjectByType<ClaybreakScreens>(FindObjectsInactive.Include);
            var hud = UnityEngine.Object.FindAnyObjectByType<ClaybreakHud>(FindObjectsInactive.Include);
            Camera camera = Camera.main;
            Canvas canvas = screens.GetComponentInParent<Canvas>(true);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;

            // A slice player after the migration: level 4, the Brute, one army step, some coins.
            var save = SaveMigration.FromV1(new SaveDataV1
            {
                nextLevel = 3, coins = 640, startingArmyStep = 1, bruteUnlocked = true, stars = new[] { 3, 2, 3 }
            });
            var progression = new Progression(catalog, save);
            progression.TryBuySkin(1);
            InvokeAwake(screens);
            screens.ShowHome(progression);
            hud.gameObject.SetActive(false);
            Shot(folder, "jogo-menu", camera);
            screens.ShowShop(progression);
            Shot(folder, "jogo-loja-upgrades", camera);
            PickTab(screens, ShopTab.Units, progression);
            Shot(folder, "jogo-loja-elites", camera);
            PickTab(screens, ShopTab.Skins, progression);
            Shot(folder, "jogo-loja-skins", camera);
            screens.ShowWorlds(progression);
            Shot(folder, "jogo-mundos", camera);

            screens.HideAll();
            hud.gameObject.SetActive(true);
            var tower = UnityEngine.Object.FindAnyObjectByType<AssaultTower>();
            InvokeAwake(tower);
            AssaultShot(folder, "jogo-assalto-torre", tower, camera, world.Levels[1], 17f);
            AssaultShot(folder, "jogo-assalto-chefe", tower, camera, world.Levels[world.LevelCount - 1], 20f);
        }

        // The tab buttons are wired in Awake; a capture presses them through the same listener.
        private static void PickTab(ClaybreakScreens screens, ShopTab tab, Progression progression)
        {
            var so = new SerializedObject(screens);
            var button = (UnityEngine.UI.Button)so.FindProperty("shopTabs").GetArrayElementAtIndex((int)tab).objectReferenceValue;
            screens.ShowShop(progression);
            button.onClick.Invoke();
        }

        // The assault camera's framing (AssaultCamera: 17 m up and 11 m back, plus the boss offset).
        private static void AssaultShot(string folder, string name, AssaultTower tower, Camera camera, ClaybreakLevel level,
            float height)
        {
            tower.ResetTower(level.Tower, level.TowerHealth, level.Floors);
            tower.transform.position = new Vector3(0f, 0f, level.TowerZ);
            Vector3 line = new Vector3(0f, 0f, level.FiringLineZ);
            camera.transform.position = line + new Vector3(0f, height, height - 31f);
            camera.transform.LookAt(Vector3.Lerp(line, new Vector3(0f, 0f, level.TowerZ), 0.55f));
            Shot(folder, name, camera);
        }

        private static void Shot(string folder, string name, Camera camera)
        {
            Canvas.ForceUpdateCanvases();
            Texture2D shot = Render(camera, 1080, 1920);
            byte[] png = shot.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(folder, name + ".png"), png);
            File.WriteAllBytes(Path.Combine(ScreensFolder, name + ".png"), png);
            UnityEngine.Object.DestroyImmediate(shot);
            Debug.Log("[ArtDirectionCapture] " + name);
        }

        private static void InvokeAwake(MonoBehaviour behaviour)
        {
            behaviour.GetType().GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(behaviour, null);
        }

        private static Texture2D Render(Camera camera, int width, int height)
        {
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            camera.targetTexture = target;
            camera.aspect = (float)width / height;
            camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(target);
            return texture;
        }

        // A blocky letter A, B or C (5x7 grid, 14 px cells) on a dark tile: which option is which.
        private static void DrawLetter(Texture2D texture, int x, int y, int index)
        {
            string[][] glyphs =
            {
                new[] { "01110", "10001", "10001", "11111", "10001", "10001", "10001" },
                new[] { "11110", "10001", "10001", "11110", "10001", "10001", "11110" },
                new[] { "01111", "10000", "10000", "10000", "10000", "10000", "01111" }
            };

            const int cell = 12;
            var dark = new Color(0.08f, 0.06f, 0.07f);
            for (int py = -12; py < 7 * cell + 12; py++)
            {
                for (int px = -12; px < 5 * cell + 12; px++)
                {
                    texture.SetPixel(x + px, y + py - 7 * cell + 84, dark);
                }
            }

            string[] glyph = glyphs[Math.Min(index, 2)];
            for (int row = 0; row < 7; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    if (glyph[row][column] != '1')
                    {
                        continue;
                    }

                    for (int py = 0; py < cell; py++)
                    {
                        for (int px = 0; px < cell; px++)
                        {
                            texture.SetPixel(x + column * cell + px, y + 84 - (row + 1) * cell + py, new Color(1f, 0.8f, 0.25f));
                        }
                    }
                }
            }
        }
    }
}
