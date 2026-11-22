using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.10 record (D28 = A): the four bosses in the toon style, with the structure and defense colors of each world's
    /// palette, on the OPPO CPH2791 screen shape (1080×2354, half size). Each world's device-gate level is its boss with every
    /// defense (cannon, wall, shield, sweeper) and the crowd attacking; two sheets: the game's own assault camera
    /// (<c>chefes-oppo.png</c>) and a closer look from the firing line at the boss and the defenses (<c>chefes-perto.png</c>,
    /// a capture-only camera). The gate's loadout wears the dearest skin (not the world color). Runs only when asked and with a
    /// GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.BossCaptureTests -bossCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class BossCaptureTests
    {
        private const string FolderArgument = "-bossCaptureDir";
        private const int Width = 540;
        private const int Height = 1177;
        private const int Gap = 8;

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
            ClaybreakFlow.EditorUsesFreshSave = false;
            ClaybreakFlow.EditorDeviceGateWorld = 1;
#endif
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureTheFourBosses()
        {
            string folder = Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
            const int worlds = 4;
            Texture2D game = Sheet(worlds);
            Texture2D close = Sheet(worlds);
            var panel = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            Camera view = null;
            GameObject cameraObject = null;
            try
            {
                for (int world = 1; world <= worlds; world++)
                {
                    yield return ClaybreakSceneTests.LoadScene(true, world);
                    var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
                    Assert.IsTrue(flow.enabled, $"world {world}: the flow disabled itself (see the log)");
                    view = flow.Crowd.CameraRig.GetComponent<Camera>();
                    view.targetTexture = target;
                    yield return WaitFor(() => flow.State == FlowState.Assault, 10f);
                    Assert.AreEqual(FlowState.Assault, flow.State, $"world {world}: no assault");
                    yield return new WaitForSeconds(2f);
                    yield return null;
                    Read(target, panel);
                    game.SetPixels((world - 1) * (Width + Gap), 0, Width, Height, panel.GetPixels());

                    // The closer look: from above the firing line, toward the boss, with the defenses in between.
                    ClaybreakLevel level = flow.ActiveLevel;
                    view.targetTexture = null;
                    cameraObject = new GameObject("BossCaptureCamera");
                    Camera look = cameraObject.AddComponent<Camera>();
                    look.CopyFrom(view);
                    look.fieldOfView = 42f;
                    look.farClipPlane = 400f;
                    look.targetTexture = target;
                    Vector3 tower = new Vector3(0f, 0f, level.TowerZ);
                    cameraObject.transform.position = tower + new Vector3(-6f, 11f, -27f);
                    cameraObject.transform.LookAt(tower + new Vector3(0f, 6f, -6f));
                    yield return null;
                    yield return null;
                    Read(target, panel);
                    close.SetPixels((world - 1) * (Width + Gap), 0, Width, Height, panel.GetPixels());
                    look.targetTexture = null;
                    UnityEngine.Object.Destroy(cameraObject);
                    cameraObject = null;
                    Debug.Log($"[BossCapture] world={world} level={level.name} tower={level.Tower} wall={level.WallHealth} "
                        + $"shield={level.ShieldHealth} inFlight={flow.Field.InFlight} fog={RenderSettings.fogColor} "
                        + $"structureLight={Shader.GetGlobalVector("_StudioKitStructureLight")}");
                }

                game.Apply();
                close.Apply();
                File.WriteAllBytes(Path.Combine(folder, "chefes-oppo.png"), game.EncodeToPNG());
                File.WriteAllBytes(Path.Combine(folder, "chefes-perto.png"), close.EncodeToPNG());
            }
            finally
            {
                // A failed assert mid-loop must not leave a camera drawing into a destroyed texture.
                if (view != null)
                {
                    view.targetTexture = null;
                }

                if (cameraObject != null)
                {
                    UnityEngine.Object.Destroy(cameraObject);
                }

                UnityEngine.Object.Destroy(target);
                UnityEngine.Object.Destroy(panel);
                UnityEngine.Object.Destroy(game);
                UnityEngine.Object.Destroy(close);
            }
        }

        private static Texture2D Sheet(int panels)
        {
            var sheet = new Texture2D(panels * Width + (panels - 1) * Gap, Height, TextureFormat.RGB24, false);
            var fill = new Color32[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++)
            {
                fill[i] = new Color32(255, 255, 255, 255);
            }

            sheet.SetPixels32(fill);
            return sheet;
        }

        private static void Read(RenderTexture target, Texture2D panel)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            panel.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            panel.Apply();
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
