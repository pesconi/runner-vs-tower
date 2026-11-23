using Claybreak.Editor;
using JungleDegree.StudioKit.Swarm;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.15 (the user, 2026-11-23: "A crowd está lenta, aumentar cerca de 30% de velocidade"): every level runs about 30%
    /// faster than its course pace, from one value (<see cref="RunPace"/>), and still leaves time to read each gate number and
    /// steer between pairs (<see cref="ReactionWindow"/>), in every world and the bosses.
    /// </summary>
    public sealed class RunPaceTests
    {
        [Test]
        public void ThePaceIsAboutThirtyPercentFromOnePlace()
        {
            Assert.AreEqual(1.3f, RunPace.Factor, 1e-6f, "RT-6t.15: +30%, constant in every world");
            foreach (float course in new[] { 5f, 5.5f, 6f, 6.5f, 7f, 7.5f })
            {
                float paced = RunPace.Of(course);
                Assert.AreEqual(0f, paced % RunPace.Step, 1e-5f, $"{course} m/s stays on a quarter step");
                Assert.AreEqual(RunPace.Factor, paced / course, 0.03f, $"{course} m/s runs about 30% faster");
            }
        }

        [Test]
        public void EveryWorldLevelRunsAtTheGamePace()
        {
            int levels = 0;
            foreach (WorldDefinition world in WorldContent.Worlds)
            {
                LevelSpec[] course = LevelGenerator.GenerateWorld(world.Recipe, ArmyLimit.Units, world.FixedLevels);
                LevelSpec[] written = WorldContent.Specs(world);
                for (int i = 0; i < written.Length; i++)
                {
                    Assert.AreEqual(RunPace.Of(course[i].runSpeed), written[i].runSpeed, 1e-5f, $"{world.Id} level {i + 1}");
                    // Pacing changes the speed only: the gates, the crates and the tower stay the course's.
                    course[i].runSpeed = written[i].runSpeed;
                    Assert.AreEqual(JsonUtility.ToJson(course[i]), JsonUtility.ToJson(written[i]), $"{world.Id} level {i + 1}");
                    levels++;
                }
            }

            Assert.AreEqual(40, levels);
            WorldDefinition first = WorldContent.Worlds[0];
            float once = WorldContent.Specs(first)[0].runSpeed;
            Assert.AreEqual(once, WorldContent.Specs(first)[0].runSpeed, 1e-5f, "pacing the same hand-made levels twice never compounds");
            Assert.AreEqual(RunPace.Of(first.FixedLevels[0].runSpeed), once, 1e-5f, "the hand-made spec keeps its course pace");
        }

        [Test]
        public void TheDeviceGateStillOutrunsEveryLevel()
        {
            var gate = AssetDatabase.LoadAssetAtPath<ClaybreakLevel>(WorldContent.StressLevelPath(3));
            Assert.IsNotNull(gate, "build the scene first");
            foreach (WorldDefinition world in WorldContent.Worlds)
            {
                foreach (LevelSpec spec in WorldContent.Specs(world))
                {
                    Assert.Less(spec.runSpeed, gate.RunSpeed, $"{world.Id} {spec.title}: the gate measures the fastest scenery recycling");
                }
            }
        }

        [Test]
        public void EveryLevelLeavesTimeToReadAndSteer()
        {
            var profile = AssetDatabase.LoadAssetAtPath<SwarmCameraProfile>(ClaybreakSceneBuilder.CrowdCameraProfilePath);
            Assert.IsNotNull(profile, "build the scene first");
            float legible = ReactionWindow.LegibleDistance(profile.Settings, ArmyLimit.Units, ClaybreakSceneBuilder.TrackWidth * 0.5f,
                GateArt.LabelEm);
            Assert.Greater(legible, LevelGenerator.GateStep, "a gate number reads before the previous pair is passed");
            float sight = float.MaxValue, gap = float.MaxValue;
            foreach (WorldDefinition world in WorldContent.Worlds)
            {
                foreach (LevelSpec spec in WorldContent.Specs(world))
                {
                    ReactionWindow.Result result = ReactionWindow.Of(spec.runSpeed, spec.runGates, legible);
                    Assert.IsTrue(result.Passes, $"{world.Id} {spec.title} at {spec.runSpeed} m/s: sight {result.SightSeconds:0.00} s, "
                        + $"gap {result.GapSeconds:0.00} s");
                    sight = Mathf.Min(sight, result.SightSeconds);
                    gap = Mathf.Min(gap, result.GapSeconds);
                }
            }

            Debug.Log($"[RunPaceTests] legible {legible:0.0} m ahead; shortest sight {sight:0.00} s; shortest gap {gap:0.00} s");
        }

        [Test]
        public void TheWindowShrinksWithSpeedAndFailsWhenTooFast()
        {
            var gates = new[] { new RunGatePair { z = 18f }, new RunGatePair { z = 38f } };
            ReactionWindow.Result slow = ReactionWindow.Of(6.5f, gates, 40f);
            ReactionWindow.Result fast = ReactionWindow.Of(9.75f, gates, 40f);
            Assert.Greater(slow.SightSeconds, fast.SightSeconds);
            Assert.AreEqual(18f / 9.75f, fast.GapSeconds, 1e-4f, "the start to the first pair is the shortest gap here");
            Assert.IsTrue(fast.Passes);
            Assert.IsFalse(ReactionWindow.Of(20f, gates, 40f).Passes, "20 m/s leaves no time to steer");
        }
    }
}
