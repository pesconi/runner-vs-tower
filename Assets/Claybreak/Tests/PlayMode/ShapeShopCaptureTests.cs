using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.20 record (D31 A: each shape must read from afar in a crowd of 300), on the OPPO CPH2791 screen shape
    /// (1080×2354, half size), with the default skin of the world (its own color):
    /// <list type="bullet">
    /// <item><c>formas-corrida.png</c>: the five shapes (columns) in the run camera of world 1's device-gate run (the crowd at
    /// its cap of 300), two moments a quarter second apart (rows), so each shape's own hop shows.</item>
    /// <item><c>formas-assalto.png</c>: the five shapes in the heaviest assault (world 4's device-gate level, 300 units in
    /// flight, the elites and the dearest skin).</item>
    /// </list>
    /// Runs only when asked and with a GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.ShapeShopCaptureTests -shapeShopCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class ShapeShopCaptureTests
    {
        private const string FolderArgument = "-shapeShopCaptureDir";
        private const string CatalogPath = "Assets/Claybreak/Content/Data/Catalog.asset";
        private const int Width = 540;
        private const int Height = 1177;

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
        public IEnumerator CaptureEveryShapeInTheRun()
        {
            string folder = Folder();
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(CatalogPath);
            int shapes = catalog.Shapes.Count;
            ClaybreakFlow flow = null;
            yield return ShapeSkinCaptureTests.LoadWorldRun(1, loaded => flow = loaded);
            using var shot = new ShapeSkinCaptureTests.Shot(flow.Crowd.CameraRig.GetComponent<Camera>(), Width, Height);
            using var sheet = new ShapeSkinCaptureTests.Sheet(shapes, 2, Width, Height);
            // The gate loadout wears the dearest skin: put the default one back, so the crowd takes the world's color.
            Assert.IsTrue(flow.Progress.TryWearSkin(0));
            flow.Crowd.Wear(flow.Progress.CrowdColorIn(0));
            yield return new WaitForSeconds(1.5f);
            for (int shape = 0; shape < shapes; shape++)
            {
                flow.Crowd.UseShape(catalog.Shapes[shape]);
                for (int moment = 0; moment < 2; moment++)
                {
                    yield return new WaitForSeconds(moment == 0 ? 0.6f : 0.25f);
                    yield return shot.Take();
                    sheet.Put(shape, moment, shot.Panel);
                }

                Debug.Log($"[ShapeShopCapture] run shape {catalog.Shapes[shape].id} army={flow.Crowd.Army} "
                    + $"tris/unit={flow.Crowd.Swarm.UnitTriangles} hop={SwarmMotionBridge.Read(flow.Crowd.Swarm).HopHeight:0.00}");
                Assert.AreEqual(flow.Crowd.MaximumArmy, flow.Crowd.Army, "the crowd is not at its cap");
            }

            sheet.Save(Path.Combine(folder, "formas-corrida.png"));
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureEveryShapeInTheAssault()
        {
            string folder = Folder();
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(CatalogPath);
            int shapes = catalog.Shapes.Count;
            yield return ClaybreakSceneTests.LoadScene(true, 4);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            Assert.IsFalse(flow.Field.Flyers.Headless, "no GPU: nothing to capture");
            using var shot = new ShapeSkinCaptureTests.Shot(flow.Crowd.CameraRig.GetComponent<Camera>(), Width, Height);
            using var sheet = new ShapeSkinCaptureTests.Sheet(shapes, 1, Width, Height);
            float deadline = Time.realtimeSinceStartup + 30f;
            while ((flow.State != FlowState.Assault || flow.Field.InFlight < 300) && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.AreEqual(FlowState.Assault, flow.State);
            Assert.AreEqual(300, flow.Field.Flyers.Count, "the field never filled");
            for (int shape = 0; shape < shapes; shape++)
            {
                flow.Field.UseShape(catalog.Shapes[shape]);
                yield return new WaitForSeconds(0.2f);
                yield return shot.Take();
                sheet.Put(shape, 0, shot.Panel);
                Debug.Log($"[ShapeShopCapture] assault shape {catalog.Shapes[shape].id} inFlight={flow.Field.InFlight} "
                    + $"tris/unit={flow.Field.Flyers.UnitTriangles}");
            }

            sheet.Save(Path.Combine(folder, "formas-assalto.png"));
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
