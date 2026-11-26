using System.Collections;
using System.Collections.Generic;
using System.IO;
using Claybreak.Lab;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6v3.2: the concept screens of the "premium vinyl toy" directions for decision D36, made in the game itself with the
    /// laboratory shader (<c>Claybreak/VinylLab</c>, local to the game) and the same fixed framings as the "before" captures
    /// (<see cref="StyleCaptureRig"/>). For each direction of <see cref="VinylDirection.All"/> it writes into
    /// <c>&lt;folder&gt;/&lt;id&gt;/</c>: the run with 300 (<c>corrida-300</c>), the heaviest assault with the world's tower
    /// (<c>assalto-300</c>, <c>torre-perto</c>) and with the "toy factory" tower (<c>-fabrica</c>), the home screen in four
    /// shapes (<c>telas</c>), the smoothed Bibopzi close up (<c>lider-perto</c>) and three icon sketches (<c>icone-1..3</c>).
    /// Every ToonLit material is switched to the lab shader for the captures and switched back at the end (the asset files
    /// never change); the crowd draws the smoothed Bibopzi; the world towers get smoothed normals (no remodelling: that is
    /// RT-6v3.5). Only with a GPU and <c>-vinylConceptDir &lt;folder&gt;</c>:
    /// <c>-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.VinylConceptTests -vinylConceptDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class VinylConceptTests
    {
        public const string FolderArgument = "-vinylConceptDir";
        private const string UnitPath = "Assets/Claybreak/Lab/Generated/BibopziVinyl.asset";
        private const string ProfileFolder = "Assets/Claybreak/Lab/Generated";
        private static readonly string[] Heroes = { "CrowdToon", "LeaderCrown" };
        private const int IconSize = 512;
        /// <summary>Icon backgrounds per direction (A warm yellow, B pastel pink, C sunset, M warm orange).</summary>
        private static readonly Color[] IconBackgrounds = { new Color(1f, 0.74f, 0.3f), new Color(0.97f, 0.84f, 0.89f), new Color(1f, 0.62f, 0.45f), new Color(1f, 0.66f, 0.4f) };
        private static readonly Color[] IconBackdrops = { new Color(1f, 0.9f, 0.55f), new Color(1f, 0.95f, 0.97f), new Color(0.55f, 0.45f, 0.8f), new Color(1f, 0.85f, 0.6f) };

        private readonly List<(Material material, Shader shader)> switched = new List<(Material, Shader)>();
        private readonly HashSet<AssaultTower> smoothedTowers = new HashSet<AssaultTower>();
        private Shader vinyl;
        private Mesh unit;
        private VolumeProfile profile;
        private VinylDirection direction;
        private GameObject volumeObject;

        [TearDown]
        public void TearDown()
        {
            Restore();
            StyleCaptureRig.End();
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureTheDirections()
        {
            string folder = ShapeSkinCaptureTests.Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            vinyl = Shader.Find(VinylDirection.ShaderName);
            Assert.IsNotNull(vinyl, VinylDirection.ShaderName + " is missing");
            Assert.IsTrue(vinyl.isSupported, VinylDirection.ShaderName + " does not compile here");
            unit = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(UnitPath);
            Assert.IsNotNull(unit, UnitPath + " is missing (run the scene builder)");
            Debug.Log($"[VinylConcept] unit {unit.name} tris={unit.triangles.Length / 3}");
            // -vinylConceptQuick: only the run of each direction (tuning the numbers); -vinylConceptOnly B: one direction.
            bool quick = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-vinylConceptQuick") >= 0;
            string only = ShapeSkinCaptureTests.Argument("-vinylConceptOnly");
            // -vinylConceptIcons: only the close-up and the icon sketches (on the home scene).
            bool iconsOnly = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-vinylConceptIcons") >= 0;
            StyleCaptureRig.Begin();
            for (int d = 0; d < VinylDirection.All.Length; d++)
            {
                direction = VinylDirection.All[d];
                if (only != null && only != direction.id)
                {
                    continue;
                }

                profile = UnityEditor.AssetDatabase.LoadAssetAtPath<VolumeProfile>($"{ProfileFolder}/VinylPost{direction.id}.asset");
                Assert.IsNotNull(profile, "post profile of " + direction.id);
                string target = Path.Combine(folder, direction.id);
                Directory.CreateDirectory(target);
                smoothedTowers.Clear();
                StyleCaptureRig.Dress = Dress;
                StyleCaptureRig.AlternateTower = ToyTower;
                StyleCaptureRig.Measure = false; // the lab's concepts are not the game's look
                // The game refuses to start when its own materials are not on ToonLit (validation of the scenery): every
                // scene load starts from the materials as they are on disk, and the framing switches them again.
                Restore();
                if (iconsOnly)
                {
                    yield return ClaybreakSceneTests.LoadScene(false);
                    yield return CaptureLeader(target);
                    yield return CaptureIcons(target, d);
                    continue;
                }

                yield return StyleCaptureRig.CaptureRun(target);
                if (quick)
                {
                    continue;
                }

                Restore();
                yield return StyleCaptureRig.CaptureAssault(target);
                Restore();
                yield return StyleCaptureRig.CaptureScreens(target);
                // The home scene is still loaded: its light dresses the close-up and the icons.
                yield return CaptureLeader(target);
                yield return CaptureIcons(target, d);
                foreach (string name in new[] { "corrida-300", "assalto-300", "torre-perto", "assalto-300-fabrica", "torre-perto-fabrica", "telas", "lider-perto", "icone-1", "icone-2", "icone-3" })
                {
                    Assert.IsTrue(File.Exists(Path.Combine(target, name + ".png")), $"{direction.id}/{name}.png was not written");
                }
            }
        }

        private void Restore()
        {
            VinylDirection.RestoreMaterials(switched.ToArray());
            switched.Clear();
        }

        // ---------- dressing a framing ----------

        private void Dress(ClaybreakFlow flow)
        {
            switched.AddRange(VinylDirection.SwitchMaterials(vinyl, Heroes));
            if (flow != null)
            {
                flow.Crowd.Swarm.SetUnitMesh(unit);
                if (flow.Field != null && flow.Field.Flyers != null)
                {
                    flow.Field.Flyers.SetUnitMesh(unit);
                }

                foreach (AssaultTower tower in Object.FindObjectsByType<AssaultTower>())
                {
                    SmoothTower(tower);
                }
            }

            direction.ApplyLight(Sun());
            if (volumeObject == null)
            {
                volumeObject = new GameObject("VinylConceptVolume");
                var volume = volumeObject.AddComponent<Volume>();
                volume.isGlobal = true;
                volume.priority = 100f;
            }

            volumeObject.GetComponent<Volume>().sharedProfile = profile;
            foreach (Camera camera in Object.FindObjectsByType<Camera>())
            {
                VinylDirection.SetPost(camera, true);
            }
        }

        // The world's tower keeps its model; its normals are smoothed under 45° so the vinyl light reads round on curved parts
        // while the crenels stay crisp. Remodelling with real bevels is RT-6v3.5.
        private void SmoothTower(AssaultTower tower)
        {
            if (!smoothedTowers.Add(tower))
            {
                return;
            }

            foreach (MeshFilter filter in tower.GetComponentsInChildren<MeshFilter>(true))
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh != null && mesh.isReadable && mesh.subMeshCount == 1)
                {
                    filter.sharedMesh = VinylMeshSmoothing.SmoothNormals(mesh, 45f);
                }
            }
        }

        private string ToyTower(ClaybreakFlow flow, AssaultTower tower)
        {
            var bounds = new Bounds(tower.transform.position, Vector3.zero);
            bool any = false;
            foreach (MeshRenderer renderer in tower.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!renderer.enabled)
                {
                    continue;
                }

                if (any)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
                else
                {
                    bounds = renderer.bounds;
                    any = true;
                }

                renderer.enabled = false;
            }

            GameObject sketch = ToyFactoryTowerSketch.Build(vinyl, Mathf.Max(6f, bounds.size.y * 0.95f));
            sketch.transform.position = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            Debug.Log($"[VinylConcept] {direction.id} toy factory tower tris={ToyFactoryTowerSketch.Triangles(sketch)} height={bounds.size.y:0.0} at {sketch.transform.position}");
            return "-fabrica";
        }

        private static Light Sun()
        {
            foreach (Light light in Object.FindObjectsByType<Light>())
            {
                if (light.type == LightType.Directional)
                {
                    return light;
                }
            }

            return null;
        }

        // ---------- the smoothed Bibopzi up close (the framing of lider-perto) ----------

        private IEnumerator CaptureLeader(string folder)
        {
            var flow = Object.FindAnyObjectByType<ClaybreakFlow>();
            Color worldColor = flow.Progress.CrowdColorIn(0);
            var model = new GameObject("VinylConceptLeader");
            Material material = HeroMaterial(worldColor);
            model.transform.position = new Vector3(0f, 60f, 0f);
            model.AddComponent<MeshFilter>().sharedMesh = unit;
            model.AddComponent<MeshRenderer>().sharedMaterial = material;
            var cameraObject = new GameObject("VinylConceptLeaderCamera");
            Camera close = cameraObject.AddComponent<Camera>();
            close.fieldOfView = 28f;
            close.nearClipPlane = 0.05f;
            close.farClipPlane = 50f;
            Dress(null);
            Vector3 center = model.transform.position + new Vector3(0f, 0.38f, 0f);
            float[] yaws = { 0f, 35f, 150f };
            using (var shot = new ShapeSkinCaptureTests.Shot(close, StyleCaptureRig.CloseSize, StyleCaptureRig.CloseSize))
            using (var sheet = new ShapeSkinCaptureTests.Sheet(yaws.Length, 1, StyleCaptureRig.CloseSize, StyleCaptureRig.CloseSize))
            {
                for (int i = 0; i < yaws.Length; i++)
                {
                    cameraObject.transform.position = center + Quaternion.Euler(8f, yaws[i], 0f) * new Vector3(0f, 0f, 2.2f);
                    cameraObject.transform.LookAt(center);
                    yield return shot.Take();
                    sheet.Put(i, 0, shot.Panel);
                }

                sheet.Save(Path.Combine(folder, "lider-perto.png"));
            }

            Object.Destroy(cameraObject);
            Object.Destroy(model);
            Object.Destroy(material);
        }

        private Material HeroMaterial(Color tint)
        {
            var material = new Material(vinyl);
            material.SetColor("_TintColor", tint);
            material.SetFloat(VinylDirection.Hero, 1f);
            return material;
        }

        // ---------- icon sketches (decision D25: with and without the face in front) ----------

        private IEnumerator CaptureIcons(string folder, int index)
        {
            var flow = Object.FindAnyObjectByType<ClaybreakFlow>();
            Color worldColor = flow.Progress.CrowdColorIn(0);
            var stage = new GameObject("VinylConceptIconStage");
            stage.transform.position = new Vector3(0f, 300f, 0f);
            Material hero = HeroMaterial(worldColor);
            var backdropMaterial = new Material(vinyl);
            backdropMaterial.SetColor("_TintColor", IconBackdrops[index]);
            backdropMaterial.SetFloat("_ToonEmission", 1f);
            backdropMaterial.SetFloat("_ToonFogAmount", 0f);
            var cameraObject = new GameObject("VinylConceptIconCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = IconBackgrounds[index];
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 60f;
            camera.fieldOfView = 30f;
            float fogStart = RenderSettings.fogStartDistance;
            float fogEnd = RenderSettings.fogEndDistance;
            RenderSettings.fogStartDistance = 500f;
            RenderSettings.fogEndDistance = 1000f;
            Vector3 origin = stage.transform.position;

            // 1: the face in front (3/4), a soft disc behind.
            GameObject one = new GameObject("Icon1");
            one.transform.SetParent(stage.transform, false);
            const float faceYaw = 25f;
            Figure(one.transform, hero, Vector3.zero, faceYaw - 10f, 1f);
            Vector3 toCamera = Quaternion.Euler(0f, faceYaw, 0f) * Vector3.forward;
            GameObject disc = Backdrop(one.transform, backdropMaterial, -toCamera * 2f + new Vector3(0f, 0.5f, 0f), 1.4f);
            disc.transform.localRotation = Quaternion.Euler(0f, faceYaw, 0f);
            yield return Icon(folder, "icone-1", camera, origin + new Vector3(0f, 0.46f, 0f), faceYaw, -4f, 2.05f, one);

            // 2: the crowd from behind charging a toy tower: no face.
            GameObject two = new GameObject("Icon2");
            two.transform.SetParent(stage.transform, false);
            for (int row = 0; row < 4; row++)
            {
                int count = row + 2;
                for (int k = 0; k < count; k++)
                {
                    float x = (k - (count - 1) * 0.5f) * 0.55f + (row % 2) * 0.08f;
                    Figure(two.transform, hero, new Vector3(x, 0f, -row * 0.55f), x * 6f, 0.85f);
                }
            }

            GameObject tower = ToyFactoryTowerSketch.Build(vinyl, 3.2f);
            tower.transform.SetParent(two.transform, false);
            tower.transform.localPosition = new Vector3(0f, -0.1f, 5f);
            tower.transform.localRotation = Quaternion.identity;
            yield return Icon(folder, "icone-2", camera, origin + new Vector3(0f, 0.9f, 1.5f), 180f, -10f, 7.6f, two);

            // 3: one Bibopzi from behind, three quarters, in front of the big toy tower.
            GameObject three = new GameObject("Icon3");
            three.transform.SetParent(stage.transform, false);
            Figure(three.transform, hero, new Vector3(-0.55f, 0.15f, 0.6f), 45f, 1.15f);
            GameObject big = ToyFactoryTowerSketch.Build(vinyl, 3.8f);
            big.transform.SetParent(three.transform, false);
            big.transform.localPosition = new Vector3(0.9f, -0.3f, 4.2f);
            big.transform.localRotation = Quaternion.Euler(0f, 20f, 0f);
            yield return Icon(folder, "icone-3", camera, origin + new Vector3(0.1f, 1.05f, 1.6f), 205f, 4f, 4.4f, three);

            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
            Object.Destroy(cameraObject);
            Object.Destroy(stage);
        }

        private void Figure(Transform parent, Material material, Vector3 position, float yaw, float scale)
        {
            var figure = new GameObject("Bibopzi");
            figure.transform.SetParent(parent, false);
            figure.transform.localPosition = position;
            figure.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            figure.transform.localScale = Vector3.one * scale;
            figure.AddComponent<MeshFilter>().sharedMesh = unit;
            figure.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static GameObject Backdrop(Transform parent, Material material, Vector3 position, float radius)
        {
            var disc = new GameObject("Backdrop");
            disc.transform.SetParent(parent, false);
            disc.transform.localPosition = position;
            disc.transform.localScale = new Vector3(1f, 1f, 0.05f);
            disc.AddComponent<MeshFilter>().sharedMesh = ToyFactoryTowerSketch.Ball(radius);
            disc.AddComponent<MeshRenderer>().sharedMaterial = material;
            return disc;
        }

        // Only the icon's own group is visible while it is taken; the camera orbits the look point.
        private IEnumerator Icon(string folder, string name, Camera camera, Vector3 look, float yaw, float pitch, float distance, GameObject group)
        {
            foreach (Transform child in group.transform.parent)
            {
                child.gameObject.SetActive(child.gameObject == group);
            }

            camera.transform.position = look + Quaternion.Euler(pitch, yaw, 0f) * new Vector3(0f, 0f, distance);
            camera.transform.LookAt(look);
            Dress(null);
            using var shot = new ShapeSkinCaptureTests.Shot(camera, IconSize, IconSize);
            yield return shot.Take();
            StyleCaptureRig.Save(folder, name, shot.Panel);
        }
    }
}
