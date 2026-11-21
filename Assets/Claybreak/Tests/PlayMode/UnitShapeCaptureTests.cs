using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using Claybreak.Lab;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.2 record: the four crowd shapes (Bolinha and Pintinho, with and without eyes) rendered in the toon lab, to
    /// be put side by side with the three.js reference (Tools/rt6t/shape-captures.sh does both and the sheets).
    /// Close-ups: one unit at rest on the fog color, three views (front three-quarter, side, back three-quarter) with
    /// the camera poses that script also gives the reference (mirrored: three.js z = −Unity z). Crowd: 100 units of each
    /// shape on a 9:16 screen with the lab camera; the camera pose and fog go to <c>unity-crowd.txt</c> so the reference
    /// is rendered from the same place. Runs only when asked and with a GPU (batchmode without -nographics):
    /// <c>-runTests -testPlatform PlayMode -testFilter UnitShapeCaptureTests -shapeCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class UnitShapeCaptureTests
    {
        private const string FolderArgument = "-shapeCaptureDir";
        private const int CloseUpSize = 360;
        private const int CrowdWidth = 540;
        private const int CrowdHeight = 960;
        private const int CrowdUnits = 100;
        private const float CloseUpFov = 30f;
        private static readonly Vector3 CloseUpTarget = new Vector3(0f, 0.22f, 0f);

        /// <summary>Close-up views (Unity coordinates, front +Z), shared with the reference script.</summary>
        public static readonly (string name, Vector3 position)[] Views =
        {
            ("frente", new Vector3(0.55f, 0.5f, 1.15f)),
            ("lado", new Vector3(1.3f, 0.4f, 0f)),
            ("costas", new Vector3(-0.55f, 0.5f, -1.15f)),
        };

        /// <summary>File ids of the variants, in lab order (shape × eyes).</summary>
        public static readonly (string id, int shape, bool eyes)[] Variants =
        {
            ("Bolinha", 0, true), ("BolinhaNoEyes", 0, false), ("Pintinho", 1, true), ("PintinhoNoEyes", 1, false),
        };

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureShapes()
        {
            string folder = Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
            yield return ToonLabSceneTests.LoadLab();
            var lab = UnityEngine.Object.FindAnyObjectByType<ToonLab>();
            yield return CloseUps(folder, lab);
            yield return Crowds(folder, lab);
        }

        private static IEnumerator CloseUps(string folder, ToonLab lab)
        {
            Camera view = lab.CameraRig.GetComponent<Camera>();
            GameObject ground = GameObject.Find("/Ground");
            Material toon = ground.GetComponent<MeshRenderer>().sharedMaterial;
            lab.SetUnits(0);
            lab.CameraRig.enabled = false;
            ground.SetActive(false);
            float fieldOfView = view.fieldOfView;
            view.fieldOfView = CloseUpFov;

            var probe = new GameObject("ShapeProbe");
            var filter = probe.AddComponent<MeshFilter>();
            var renderer = probe.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = toon;
            var tint = new MaterialPropertyBlock();
            tint.SetColor("_TintColor", lab.Swarm.Tint);
            renderer.SetPropertyBlock(tint);

            var target = new RenderTexture(CloseUpSize, CloseUpSize, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            view.targetTexture = target;
            foreach ((string id, int shape, bool eyes) in Variants)
            {
                filter.sharedMesh = lab.ShapeMesh(shape, eyes);
                foreach ((string name, Vector3 position) in Views)
                {
                    view.transform.SetPositionAndRotation(position, Quaternion.LookRotation(CloseUpTarget - position));
                    yield return null;
                    yield return null;
                    Save(target, Path.Combine(folder, $"unity-{id}-{name}.png"));
                }
            }

            view.targetTexture = null;
            UnityEngine.Object.Destroy(target);
            UnityEngine.Object.Destroy(probe);
            ground.SetActive(true);
            view.fieldOfView = fieldOfView;
            lab.CameraRig.enabled = true;
        }

        private static IEnumerator Crowds(string folder, ToonLab lab)
        {
            Camera view = lab.CameraRig.GetComponent<Camera>();
            var target = new RenderTexture(CrowdWidth, CrowdHeight, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            view.targetTexture = target;
            lab.SetUnits(CrowdUnits);
            lab.SteerTo(0f);
            var poses = new StringBuilder();
            foreach ((string id, int shape, bool eyes) in Variants)
            {
                lab.SelectShape(shape);
                lab.SetEyes(eyes);
                yield return new WaitForSeconds(3.5f);
                yield return null;
                Save(target, Path.Combine(folder, $"unity-crowd-{id}.png"));
                Transform pose = view.transform;
                poses.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0} cam={1:0.###},{2:0.###},{3:0.###} forward={4:0.#####},{5:0.#####},{6:0.#####} fov={7:0.##} fog={8:0.##},{9:0.##}",
                    id, pose.position.x, pose.position.y, pose.position.z, pose.forward.x, pose.forward.y, pose.forward.z,
                    view.fieldOfView, RenderSettings.fogStartDistance, RenderSettings.fogEndDistance));
            }

            File.WriteAllText(Path.Combine(folder, "unity-crowd.txt"), poses.ToString());
            Debug.Log("[ShapeCapture] crowd poses\n" + poses);
            view.targetTexture = null;
            UnityEngine.Object.Destroy(target);
            lab.SelectShape(0);
            lab.SetEyes(true);
        }

        private static void Save(RenderTexture target, string path)
        {
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
            RenderTexture.active = previous;
            File.WriteAllBytes(path, image.EncodeToPNG());
            UnityEngine.Object.Destroy(image);
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
