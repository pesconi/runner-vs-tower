using System.Collections;
using Claybreak.Lab;
using JungleDegree.StudioKit.Swarm;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Claybreak.Tests
{
    /// <summary>
    /// Plays the toon lab (RT-6t.1) in the Editor: the swarm comes up with its 1000 units, the toon look and the
    /// draws run for a few frames, and any error logged (missing keyword, device without structured buffers in the
    /// vertex shader, shader failure) fails the test. Performance is judged only by the device gate on the OPPO.
    /// </summary>
    public sealed class ToonLabSceneTests
    {
        [UnityTest]
        public IEnumerator ToonLabRunsWithItsThousandUnits()
        {
#if UNITY_EDITOR
            AsyncOperation load = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                ToonLab.ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            while (!load.isDone)
            {
                yield return null;
            }

            for (int frame = 0; frame < 30; frame++)
            {
                yield return null;
            }

            var swarm = Object.FindFirstObjectByType<SwarmRenderer>();
            Assert.IsNotNull(swarm, "the lab has no swarm");
            Assert.IsTrue(swarm.enabled, "the swarm disabled itself (see the log)");
            Assert.AreEqual(1000, swarm.Count);
            Assert.AreEqual(474, swarm.UnitTriangles);
            Assert.IsTrue(swarm.DrawsShadows);
            Assert.IsTrue(Object.FindFirstObjectByType<ToonLab>().enabled, "the lab disabled itself (see the log)");
#else
            Assert.Ignore("Editor only.");
            yield break;
#endif
        }
    }
}
