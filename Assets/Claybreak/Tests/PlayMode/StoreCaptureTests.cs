using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-9a: candidate frames for the Google Play listing, drawn by the game itself (no mock-ups, no measuring level).
    /// A mid-game save (earlier worlds cleared with stars, some upgrades, the elites bought) plays a real level of each
    /// world from the PLAY button to the result screen: the run goes straight, and in the assault a simulated finger
    /// holds the middle of the screen (the player's "hold to launch"). Frames carry the game's own HUD and screens, in
    /// phone portrait 9:16 (1080×1920); a HUD-free wide (2048×1000) and square (1024×1024) frame of the run and of the
    /// assault feed <c>Tools/store/make_store_art.py</c> (feature graphic and icon). The menus are captured with the
    /// same save. The chosen frames go to <c>docs/store/graficos/</c> through that script.
    /// Runs only when asked and with a GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.StoreCaptureTests -storeCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class StoreCaptureTests
    {
        private const string FolderArgument = "-storeCaptureDir";
        private const int PhoneWidth = 1080;
        private const int PhoneHeight = 1920;
        private const int WideWidth = 2048;
        private const int WideHeight = 1000;
        private const int Square = 1024;
        private const float Every = 0.6f;
        private const float Longest = 90f;

        private Mouse finger;
        private bool inputChanged;
        private InputSettings.BackgroundBehavior oldBackground;
        private InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;

        [TearDown]
        public void TearDown()
        {
            if (finger != null)
            {
                InputSystem.RemoveDevice(finger);
                finger = null;
            }

            if (inputChanged)
            {
                InputSystem.settings.backgroundBehavior = oldBackground;
                InputSystem.settings.editorInputBehaviorInPlayMode = oldEditorInput;
                inputChanged = false;
            }
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesFreshSave = false;
#endif
        }

        /// <summary>World and level (1-based) of each played level: a mid level, a later one, and a boss.</summary>
        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureRealLevel([Values("1-6", "2-4", "3-8", "4-10")] string worldLevel)
        {
            string folder = Folder();
            string[] parts = worldLevel.Split('-');
            int world = int.Parse(parts[0]) - 1;
            int level = int.Parse(parts[1]) - 1;
            ClaybreakFlow flow = null;
            yield return LoadMidGame(world, level, loaded => flow = loaded);
            Camera view = flow.Crowd.CameraRig.GetComponent<Camera>();
            string name = $"w{world + 1}-l{level + 1}";

            Press("/Ui/Screens/Home/Safe/Play");
            // Batchmode has no focused game view: let the simulated finger reach the game anyway (restored after).
            oldBackground = InputSystem.settings.backgroundBehavior;
            oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            inputChanged = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            finger = InputSystem.AddDevice<Mouse>();
            float start = Time.realtimeSinceStartup;
            float nextShot = start + 0.8f;
            int shots = 0;
            bool runExtras = false;
            bool assaultExtras = false;
            while (Time.realtimeSinceStartup - start < Longest)
            {
                FlowState state = flow.State;
                bool assault = state == FlowState.Assault;
                Hold(assault);
                if (state == FlowState.Run)
                {
                    Steer(flow);
                }
                if (state == FlowState.Won || state == FlowState.Lost || state == FlowState.ContinueOffer || (state == FlowState.Menu && shots > 0))
                {
                    break;
                }

                if (Time.realtimeSinceStartup >= nextShot)
                {
                    yield return TakeOnce(view, PhoneWidth, PhoneHeight,
                        Path.Combine(folder, $"{name}-{shots:00}-{(assault ? "assault" : "run")}.png"));
                    shots++;
                    if (!runExtras && !assault && Time.realtimeSinceStartup - start > 3f)
                    {
                        runExtras = true;
                        yield return Extras(flow, view, folder, $"{name}-run");
                    }

                    if (!assaultExtras && assault && flow.Field.InFlight >= 60)
                    {
                        assaultExtras = true;
                        yield return Extras(flow, view, folder, $"{name}-assault");
                    }

                    nextShot = Time.realtimeSinceStartup + Every;
                }

                yield return null;
            }

            Hold(false);
            yield return new WaitForSeconds(1.2f);
            yield return TakeOnce(view, PhoneWidth, PhoneHeight, Path.Combine(folder, $"{name}-zz-result.png"));
            Debug.Log($"[StoreCapture] {name} shots={shots} state={flow.State} outcome won={flow.LastOutcome.Won} "
                + $"stars={flow.LastOutcome.Stars} coins={flow.LastOutcome.Coins}");
            Assert.Greater(shots, 4, "the level ended before anything was captured");
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureMenus()
        {
            string folder = Folder();
            ClaybreakFlow flow = null;
            yield return LoadMidGame(1, 3, loaded => flow = loaded);
            Camera view = flow.Crowd.CameraRig.GetComponent<Camera>();
            flow.Progress.TryBuyShape(2);
            flow.Progress.TryBuyShape(3);
            string[] screens = { "home", "loja-upgrades", "loja-elites", "loja-formas", "mundos" };
            foreach (string screen in screens)
            {
                if (screen == "loja-elites")
                {
                    flow.Screens.ShowShop(flow.Progress);
                }

                ScreenFitTests.Show(flow, screen);
                yield return new WaitForSeconds(0.3f);
                yield return TakeOnce(view, PhoneWidth, PhoneHeight, Path.Combine(folder, $"menu-{screen}.png"));
            }
        }

        // Earlier worlds cleared with every star, this world's earlier levels won with 2-3 stars, half the upgrade
        // steps and the elites bought, a few hundred coins left: what a player has around that level.
        private static IEnumerator LoadMidGame(int world, int level, System.Action<ClaybreakFlow> loaded)
        {
            yield return ClaybreakSceneTests.LoadScene(false);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            Assert.IsFalse(flow.Crowd.Swarm.Headless, "no GPU: nothing to capture");
            Progression progress = flow.Progress;
            SaveData save = progress.Save;
            for (int w = 0; w <= world; w++)
            {
                WorldSave entry = save.worlds[w];
                int won = w < world ? entry.stars.Length : level;
                for (int i = 0; i < won; i++)
                {
                    entry.stars[i] = w < world || i % 3 != 1 ? 3 : 2;
                }

                entry.loop = w < world ? 1 : 0;
                entry.nextLevel = w < world ? 0 : level;
            }

            Assert.IsTrue(progress.TrySelectWorld(world), $"world {world + 1} did not open");
            save.coins = 1000000;
            int tracks = progress.Catalog.Upgrades.Count;
            for (int t = 0; t < tracks; t++)
            {
                int steps = progress.Catalog.Upgrades[t].steps.Length * (world + 1) / 5;
                for (int s = 0; s < steps; s++)
                {
                    progress.TryBuyUpgrade(t);
                }
            }

            for (int u = 0; u < progress.Catalog.Units.Count && u <= world; u++)
            {
                progress.TryBuyUnit(u);
            }

            save.coins = 640 + 1730 * world;
            save.levelsWon = world * 10 + level;
            flow.Screens.ShowHome(progress);
            UiOnCamera(flow.Crowd.CameraRig.GetComponent<Camera>());
            yield return null;
            yield return null;
            loaded(flow);
        }

        // A player reads the next pair of gates and takes the side that leaves the bigger army.
        private static void Steer(ClaybreakFlow flow)
        {
            RunCrowd crowd = flow.Crowd;
            var gates = flow.ActiveLevel.RunGates;
            float z = crowd.Position.z;
            for (int i = 0; i < gates.Count; i++)
            {
                if (gates[i].z < z + 0.5f)
                {
                    continue;
                }

                int army = crowd.Army;
                bool right = GateMath.Apply(gates[i].right, army, ArmyLimit.Units) > GateMath.Apply(gates[i].left, army, ArmyLimit.Units);
                crowd.Swarm.SteerTo(crowd.TrackCenterX + (right ? 0.5f : -0.5f) * crowd.TrackHalfWidth);
                return;
            }
        }

        // The finger in the middle of the screen, a little below the center (where a player's thumb rests).
        private void Hold(bool held)
        {
            var state = new MouseState { position = new Vector2(Screen.width * 0.5f, Screen.height * 0.35f) };
            if (held)
            {
                state = state.WithButton(MouseButton.Left);
            }

            InputSystem.QueueStateEvent(finger, state);
        }

        // HUD-free wide and square frames of the same moment; the HUD comes back after.
        private static IEnumerator Extras(ClaybreakFlow flow, Camera view, string folder, string name)
        {
            flow.Hud.Show(false);
            yield return TakeOnce(view, WideWidth, WideHeight, Path.Combine(folder, $"wide-{name}.png"));
            yield return TakeOnce(view, Square, Square, Path.Combine(folder, $"square-{name}.png"));
            flow.Hud.Show(true);
        }

        private static IEnumerator TakeOnce(Camera view, int width, int height, string path)
        {
            using var shot = new ShapeSkinCaptureTests.Shot(view, width, height);
            yield return shot.Take();
            File.WriteAllBytes(path, shot.Panel.EncodeToPNG());
        }

        // The overlay canvas is not drawn into a render texture: put it on the game camera, as ScreenFitTests does.
        private static void UiOnCamera(Camera view)
        {
            var canvas = GameObject.Find("/Ui").GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = view;
            canvas.planeDistance = 1f;
        }

        private static void Press(string path)
        {
            GameObject target = GameObject.Find(path);
            Assert.IsNotNull(target, path + " is not on screen");
            target.GetComponent<Button>().onClick.Invoke();
        }

        private static string Folder()
        {
            string folder = ShapeSkinCaptureTests.Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
