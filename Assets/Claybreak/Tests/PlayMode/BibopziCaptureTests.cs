using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    /// <summary>
    /// 2026-11-25 record (Bibopzi becomes the default shape, user's decision): the shape with the game's own toon shader and
    /// light, in world 1's run scene, on the OPPO CPH2791 screen shape (half size):
    /// <list type="bullet">
    /// <item><c>bibopzi-perto.png</c>: close-ups (front, three quarters, three quarters from behind as the run camera sees
    /// it) in the reference teal #22B3A6, and the front in world 1's crowd color (what a new player sees).</item>
    /// <item><c>bibopzi-corrida-300.png</c>: the run camera with the crowd at its cap of 300 Bibopzis, in world 1's color and
    /// in the reference teal, two moments a quarter second apart.</item>
    /// </list>
    /// Runs only when asked and with a GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.BibopziCaptureTests -bibopziCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class BibopziCaptureTests
    {
        private const string FolderArgument = "-bibopziCaptureDir";
        private const string CatalogPath = "Assets/Claybreak/Content/Data/Catalog.asset";
        private const int Width = 540;
        private const int Height = 1177;
        private const int CloseSize = 600;
        private static readonly Color Teal = new Color32(0x22, 0xb3, 0xa6, 255);

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateRun = false;
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
            ClaybreakFlow.EditorUsesFreshSave = false;
            ClaybreakFlow.EditorDeviceGateWorld = 1;
#endif
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureCloseUpsAndTheRun()
        {
            string folder = Folder();
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(CatalogPath);
            CrowdShapeOption bibopzi = catalog.Shapes[0];
            Assert.AreEqual(SaveMigration.DefaultShape, bibopzi.id);
            ClaybreakFlow flow = null;
            yield return ShapeSkinCaptureTests.LoadWorldRun(1, loaded => flow = loaded);
            Assert.IsTrue(flow.Progress.TryWearSkin(0));
            Color worldColor = flow.Progress.CrowdColorIn(0);

            // Close-ups: the mesh alone high above the track (out of the props), lit by the scene's own light and look.
            var model = new GameObject("BibopziCloseUp");
            var material = new Material(Shader.Find("StudioKit/ToonLit"));
            model.transform.position = new Vector3(0f, 60f, 0f);
            model.AddComponent<MeshFilter>().sharedMesh = bibopzi.mesh;
            model.AddComponent<MeshRenderer>().sharedMaterial = material;
            var cameraObject = new GameObject("BibopziCloseUpCamera");
            Camera close = cameraObject.AddComponent<Camera>();
            close.fieldOfView = 28f;
            close.nearClipPlane = 0.05f;
            close.farClipPlane = 50f;
            Vector3 center = model.transform.position + new Vector3(0f, 0.38f, 0f);
            float[] yaws = { 0f, 35f, 150f, 0f };
            Color[] tints = { Teal, Teal, Teal, worldColor };
            using (var shot = new ShapeSkinCaptureTests.Shot(close, CloseSize, CloseSize))
            using (var sheet = new ShapeSkinCaptureTests.Sheet(yaws.Length, 1, CloseSize, CloseSize))
            {
                for (int i = 0; i < yaws.Length; i++)
                {
                    material.SetColor("_TintColor", tints[i]);
                    Vector3 from = Quaternion.Euler(8f, yaws[i], 0f) * new Vector3(0f, 0f, 2.2f);
                    cameraObject.transform.position = center + from;
                    cameraObject.transform.LookAt(center);
                    yield return shot.Take();
                    sheet.Put(i, 0, shot.Panel);
                }

                sheet.Save(Path.Combine(folder, "bibopzi-perto.png"));
            }

            Object.Destroy(cameraObject);
            Object.Destroy(model);
            Object.Destroy(material);

            // The run: 300 Bibopzis behind the leader, in world 1's color, then in the reference teal.
            flow.Crowd.UseShape(bibopzi);
            using (var shot = new ShapeSkinCaptureTests.Shot(flow.Crowd.CameraRig.GetComponent<Camera>(), Width, Height))
            using (var sheet = new ShapeSkinCaptureTests.Sheet(2, 2, Width, Height))
            {
                for (int column = 0; column < 2; column++)
                {
                    flow.Crowd.Wear(column == 0 ? worldColor : Teal);
                    for (int moment = 0; moment < 2; moment++)
                    {
                        yield return new WaitForSeconds(moment == 0 ? (column == 0 ? 1.5f : 0.4f) : 0.25f);
                        yield return shot.Take();
                        sheet.Put(column, moment, shot.Panel);
                    }

                    Debug.Log($"[BibopziCapture] run army={flow.Crowd.Army} tris/unit={flow.Crowd.Swarm.UnitTriangles} "
                        + $"hop={SwarmMotionBridge.Read(flow.Crowd.Swarm).HopHeight:0.00}");
                    Assert.AreEqual(flow.Crowd.MaximumArmy, flow.Crowd.Army, "the crowd is not at its cap");
                    Assert.AreEqual(bibopzi.mesh, flow.Crowd.Swarm.UnitMesh);
                }

                sheet.Save(Path.Combine(folder, "bibopzi-corrida-300.png"));
            }
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
