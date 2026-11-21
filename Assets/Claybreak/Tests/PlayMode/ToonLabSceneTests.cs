using System.Collections;
using Claybreak.Lab;
using JungleDegree.StudioKit.Swarm;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Claybreak.Tests
{
    /// <summary>
    /// Plays the toon lab in the Editor. RT-6t.1: the swarm comes up with its 1000 units, the toon look and the draws run
    /// for a few frames, and any error logged (missing keyword, device without structured buffers in the vertex shader,
    /// shader failure) fails the test. RT-6t.3: the panel changes the count, steering keeps the crowd on the track, the
    /// camera backs away as the crowd grows and shakes on demand. Performance is judged only by the device gate.
    /// </summary>
    public sealed class ToonLabSceneTests
    {
        [UnityTest]
        public IEnumerator ToonLabRunsWithItsThousandUnits()
        {
            yield return LoadLab();
            for (int frame = 0; frame < 30; frame++)
            {
                yield return null;
            }

            var swarm = Object.FindAnyObjectByType<SwarmRenderer>();
            Assert.IsNotNull(swarm, "the lab has no swarm");
            Assert.IsTrue(swarm.enabled, "the swarm disabled itself (see the log)");
            Assert.AreEqual(1000, swarm.Count);
            Assert.AreEqual("Bolinha", swarm.UnitMesh.name, "the eyed Bolinha is the provisional default (D25 open)");
            Assert.AreEqual(484, swarm.UnitTriangles);
            Assert.IsTrue(swarm.DrawsShadows);
            Assert.IsTrue(Object.FindAnyObjectByType<ToonLab>().enabled, "the lab disabled itself (see the log)");
            Assert.IsTrue(Object.FindAnyObjectByType<SwarmCameraRig>().enabled, "the camera rig disabled itself (see the log)");
        }

        [UnityTest]
        public IEnumerator PanelPicksTheShapeAndTheEyes()
        {
            yield return LoadLab();
            var lab = Object.FindAnyObjectByType<ToonLab>();
            SwarmRenderer swarm = lab.Swarm;
            Assert.AreEqual(2, lab.ShapeCount);
            int count = swarm.Count;
            string[] steps = { "ShapePintinho", "EyesOff", "ShapeBolinha", "EyesOn" };
            string[] meshes = { "Pintinho", "PintinhoNoEyes", "BolinhaNoEyes", "Bolinha" };
            int[] triangles = { 492, 420, 412, 484 };
            for (int i = 0; i < steps.Length; i++)
            {
                Press(steps[i]);
                yield return null;
                Assert.AreEqual(meshes[i], swarm.UnitMesh.name, steps[i]);
                Assert.AreEqual(triangles[i], swarm.UnitTriangles, steps[i]);
                Assert.AreEqual(count, swarm.Count, "picking a shape keeps the crowd");
            }

            Assert.AreEqual("Bolinha", lab.ShapeLabel);
            lab.SetEyes(false);
            Assert.AreEqual("Bolinha sem olhos", lab.ShapeLabel);
        }

        [UnityTest]
        public IEnumerator PanelSteeringAndCameraFollowTheCrowd()
        {
            yield return LoadLab();
            var lab = Object.FindAnyObjectByType<ToonLab>();
            SwarmRenderer swarm = lab.Swarm;
            SwarmCameraRig rig = lab.CameraRig;
            float growth = rig.Profile.Growth;

            Press("Restart");
            Assert.AreEqual(10, swarm.Count, "Recomeçar sets 10");
            yield return new WaitForSeconds(3.5f);
            Assert.AreEqual(SwarmCameraMath.TargetDistance(10, growth), rig.Distance, 0.01f, "settled at 10");
            float nearZ = rig.transform.position.z;

            Press("Add100");
            Assert.AreEqual(110, swarm.Count, "+100");
            Press("Set1000");
            Assert.AreEqual(1000, swarm.Count, "1000!");
            yield return new WaitForSeconds(3.5f);
            Assert.AreEqual(SwarmCameraMath.TargetDistance(1000, growth), rig.Distance, 0.02f, "settled at 1000");
            Assert.Less(rig.transform.position.z, nearZ - 5f, "the camera backed away from the bigger crowd");

            lab.SteerTo(100f);
            Assert.AreEqual(swarm.LaneHalfWidth - SwarmDragSettings.Default.EdgeMargin, swarm.TargetX, 1e-4f, "the target stays on the track");
            yield return new WaitForSeconds(2f);
            Assert.AreEqual(swarm.TargetX, swarm.CenterX, 0.01f, "the center reached the target");
            float limit = swarm.LaneHalfWidth - swarm.Formation.EdgeMargin + 1e-3f;
            for (int i = 0; i < swarm.Count; i += 5)
            {
                Assert.LessOrEqual(Mathf.Abs(swarm.UnitGroundPosition(i).x), limit, $"unit {i} left the track");
            }

            Assert.Greater(rig.transform.position.x, 1f, "the camera follows the crowd across");

            Press("Shake");
            Assert.Greater(rig.ShakeLeft, 0f);
            yield return new WaitForSeconds(0.6f);
            Assert.AreEqual(0f, rig.ShakeLeft, "the shake is over after its length");
        }

        internal static IEnumerator LoadLab()
        {
#if UNITY_EDITOR
            AsyncOperation load = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                ToonLab.ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
#else
            Assert.Ignore("Editor only.");
            yield break;
#endif
        }

        private static void Press(string button)
        {
            GameObject found = GameObject.Find("/DebugCanvas/TestPanel/" + button);
            Assert.IsNotNull(found, "missing panel button " + button);
            found.GetComponent<Button>().onClick.Invoke();
        }
    }
}
