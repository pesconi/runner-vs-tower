using System.Collections;
using System.IO;
using JungleDegree.StudioKit.Toon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6v3.4b record (face atlas and motion, direcao-visual-v3.md 3.2, 3.3, D36), with the game's vinyl crowd material and
    /// world 1's light and color:
    /// <list type="bullet">
    /// <item><c>bibopzi-rostos.png</c>: the 8 expressions from the front, rows: detailed level (leader and screens), crowd
    /// level, and the crowd level drawn at 40 px (the D36 legibility size) and enlarged.</item>
    /// <item><c>bibopzi-sequencia.png</c>: the run with 300 Bibopzis told a bad gate, a good gate and the victory through the
    /// game's own <see cref="CrowdFace"/> (columns: resting, scared, happy, victory); row 0 from the front, three quarters, row
    /// 1 the run camera (hands opening).</item>
    /// <item><c>bibopzi-costas-chave.png</c>: the crowd from behind, as the run camera sees it, in 4 moments 0.07 s apart:
    /// the wind-up key turning, the spring trailing, the squash at the landing (row 0 close behind, row 1 the run camera).</item>
    /// </list>
    /// Runs only when asked and with a GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.BibopziFaceCaptureTests -faceCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class BibopziFaceCaptureTests
    {
        private const string FolderArgument = "-faceCaptureDir";
        private const string CatalogPath = "Assets/Claybreak/Content/Data/Catalog.asset";
        private const string UnitsFolder = "Assets/Claybreak/Content/Generated/Units";
        private const string CrowdMaterialPath = UnitsFolder + "/CrowdToon.mat";
        private const int FaceSize = 320;
        private const int TinySize = 40;
        private const int PanelWidth = 360;
        private const int PanelHeight = 720;

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateRun = false;
#endif
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureTheFacesTheSequenceAndTheBack()
        {
            string folder = Folder();
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<ClaybreakCatalog>(CatalogPath);
            CrowdShapeOption bibopzi = catalog.Shapes[0];
            Assert.AreEqual(SaveMigration.DefaultShape, bibopzi.id);
            ClaybreakFlow flow = null;
            yield return ShapeSkinCaptureTests.LoadWorldRun(1, loaded => flow = loaded);
            Assert.IsTrue(flow.Progress.TryWearSkin(0));
            Color worldColor = flow.Progress.CrowdColorIn(0);
            var guide = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleGuide>(StyleGuide.AssetPath);

            // ---------- the 8 expressions, close up (mesh drawn without instances: the face cell from _VinylMotion) ----------
            var model = new GameObject("BibopziFace");
            var material = new Material(UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(CrowdMaterialPath));
            material.DisableKeyword(ToonShaderIds.InstancesKeyword);
            material.SetColor("_TintColor", worldColor);
            Assert.IsTrue(material.IsKeywordEnabled(ToonShaderIds.FaceKeyword), "the crowd material wears the face atlas");
            model.transform.position = new Vector3(0f, 60f, 0f);
            MeshFilter filter = model.AddComponent<MeshFilter>();
            model.AddComponent<MeshRenderer>().sharedMaterial = material;
            var cameraObject = new GameObject("BibopziFaceCamera");
            Camera close = cameraObject.AddComponent<Camera>();
            close.fieldOfView = 24f;
            close.nearClipPlane = 0.05f;
            close.farClipPlane = 50f;
            Vector3 center = model.transform.position + new Vector3(0f, 0.4f, 0f);
            cameraObject.transform.position = center + Quaternion.Euler(6f, 0f, 0f) * new Vector3(0f, 0f, 2f);
            cameraObject.transform.LookAt(center);
            string[] levels = { "BibopziDetail", "Bibopzi" };
            using (var shot = new ShapeSkinCaptureTests.Shot(close, FaceSize, FaceSize))
            using (var sheet = new ShapeSkinCaptureTests.Sheet(FaceRules.Cells, levels.Length + 1, FaceSize, FaceSize))
            {
                for (int row = 0; row < levels.Length; row++)
                {
                    filter.sharedMesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>($"{UnitsFolder}/{levels[row]}.asset");
                    for (int cell = 0; cell < FaceRules.Cells; cell++)
                    {
                        material.SetVector(ToonShaderIds.VinylMotion, Pose(guide, (FaceExpression)cell));
                        yield return shot.Take();
                        sheet.Put(cell, row, shot.Panel);
                    }
                }

                // The crowd level at 40 px (head and shoulders fill the panel), enlarged without filtering.
                cameraObject.transform.position = center + new Vector3(0f, 0.08f, 1.4f);
                cameraObject.transform.LookAt(center + new Vector3(0f, 0.08f, 0f));
                using (var tiny = new ShapeSkinCaptureTests.Shot(close, TinySize, TinySize))
                {
                    for (int cell = 0; cell < FaceRules.Cells; cell++)
                    {
                        material.SetVector(ToonShaderIds.VinylMotion, Pose(guide, (FaceExpression)cell));
                        yield return tiny.Take();
                        Texture2D big = Enlarge(tiny.Panel, FaceSize);
                        sheet.Put(cell, levels.Length, big);
                        Object.Destroy(big);
                    }
                }

                sheet.Save(Path.Combine(folder, "bibopzi-rostos.png"));
            }

            Object.Destroy(cameraObject);
            Object.Destroy(model);
            Object.Destroy(material);

            // ---------- the run: the game's CrowdFace told a bad gate, a good gate, the victory ----------
            flow.Crowd.UseShape(bibopzi);
            flow.Crowd.Wear(worldColor);
            var face = flow.GetComponent<CrowdFace>();
            Assert.IsNotNull(face, "the scene has the crowd's face");
            yield return new WaitForSeconds(1.5f);
            flow.Crowd.Speed = 0f; // the crowd hops in place: the two cameras keep it framed
            Camera run = flow.Crowd.CameraRig.GetComponent<Camera>();
            var frontObject = new GameObject("BibopziFrontCamera");
            Camera front = frontObject.AddComponent<Camera>();
            front.fieldOfView = 40f;
            front.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            Vector3 leader = flow.Crowd.Position;
            frontObject.transform.position = leader + new Vector3(-1.6f, 1.5f, 3.6f);
            frontObject.transform.LookAt(leader + new Vector3(0f, 0.5f, -1.2f));
            FaceEvent?[] moments = { null, FaceEvent.BadGate, FaceEvent.GoodGate, FaceEvent.Victory };
            using (var sheet = new ShapeSkinCaptureTests.Sheet(moments.Length, 2, PanelWidth, PanelHeight))
            {
                for (int i = 0; i < moments.Length; i++)
                {
                    if (moments[i].HasValue)
                    {
                        // A reaction shows over the base only once the one before it is over (priorities of the guide).
                        yield return new WaitForSeconds(1f);
                        face.Trigger(moments[i].Value);
                    }

                    yield return new WaitForSeconds(0.12f);
                    using (var shot = new ShapeSkinCaptureTests.Shot(front, PanelWidth, PanelHeight))
                    {
                        yield return shot.Take();
                        sheet.Put(i, 0, shot.Panel);
                    }

                    using (var shot = new ShapeSkinCaptureTests.Shot(run, PanelWidth, PanelHeight))
                    {
                        yield return shot.Take();
                        sheet.Put(i, 1, shot.Panel);
                    }

                    Debug.Log($"[FaceCapture] {(moments[i]?.ToString() ?? "rest")}: cell {flow.Crowd.Swarm.Expression.Cell} hands {flow.Crowd.Swarm.Expression.HandsOpen:0.##}");
                }

                Assert.AreEqual((int)FaceExpression.Victory, flow.Crowd.Swarm.Expression.Cell, "the crowd ends on its victory face");
                sheet.Save(Path.Combine(folder, "bibopzi-sequencia.png"));
            }

            // ---------- from behind: the key turning, the spring trailing, the squash ----------
            face.ResetFace();
            frontObject.transform.position = leader + new Vector3(0.9f, 1.1f, -4.2f);
            frontObject.transform.LookAt(leader + new Vector3(0f, 0.45f, -1.6f));
            front.fieldOfView = 32f;
            using (var sheet = new ShapeSkinCaptureTests.Sheet(4, 2, PanelWidth, PanelHeight))
            {
                using (var behind = new ShapeSkinCaptureTests.Shot(front, PanelWidth, PanelHeight))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        yield return new WaitForSeconds(0.07f);
                        yield return behind.Take();
                        sheet.Put(i, 0, behind.Panel);
                    }
                }

                using (var shot = new ShapeSkinCaptureTests.Shot(run, PanelWidth, PanelHeight))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        yield return new WaitForSeconds(0.07f);
                        yield return shot.Take();
                        sheet.Put(i, 1, shot.Panel);
                    }
                }

                sheet.Save(Path.Combine(folder, "bibopzi-costas-chave.png"));
            }

            Debug.Log($"[FaceCapture] army={flow.Crowd.Army} layout={flow.Crowd.Swarm.Layout} tris/unit={flow.Crowd.Swarm.UnitTriangles}");
            Assert.AreEqual(ToonInstanceLayout.Animated, flow.Crowd.Swarm.Layout, "the crowd draws with the animated layout");
            Object.Destroy(frontObject);
        }

        // The face cell, the hop at its top (still: no spring bend) and the hands of the guide's rule that shows it.
        private static Vector4 Pose(StyleGuide guide, FaceExpression expression)
        {
            float hands = 0f;
            foreach (FaceRule rule in guide.Faces.rules)
            {
                if (rule.expression == expression)
                {
                    hands = Mathf.Max(hands, rule.handsOpen);
                }
            }

            return new Vector4((int)expression, Mathf.PI * 0.5f, 0f, hands);
        }

        private static Texture2D Enlarge(Texture2D small, int size)
        {
            var big = new Texture2D(size, size, TextureFormat.RGB24, false);
            var pixels = new Color32[size * size];
            Color32[] source = small.GetPixels32();
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = source[y * small.height / size * small.width + x * small.width / size];
                }
            }

            big.SetPixels32(pixels);
            big.Apply();
            return big;
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
