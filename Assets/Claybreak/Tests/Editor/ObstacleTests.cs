using System;
using Claybreak.Editor;
using JungleDegree.StudioKit.Swarm;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.18: the run obstacles' motion and deadly areas (<see cref="ObstacleMotion"/>), the level rules for them
    /// (<see cref="ClaybreakLevel.TryValidate"/>), their reaction window (<see cref="ReactionWindow"/>), the device gate and lab
    /// levels, and their art (<see cref="ObstacleArt"/>). The game's lane: centered on 0, 4 m half width.
    /// </summary>
    public sealed class ObstacleTests
    {
        private const float Half = ClaybreakSceneBuilder.TrackWidth * 0.5f;

        // ---------- motion and deadly areas ----------

        [Test]
        public void TheLogSwingsAcrossTheWholeLaneAndKillsOnlyWhenLow()
        {
            var log = new ObstaclePlacement(ObstacleKind.SwingingLog, 30f);
            float period = ObstacleMotion.Period(log);
            ObstacleMotion.Pose middle = ObstacleMotion.PoseAt(log, 0f, 0f, Half);
            Assert.AreEqual(0f, middle.Hazard.x, 1e-4f, "the swing starts in the middle");
            Assert.IsTrue(middle.Deadly, "the log is low in the middle");
            Assert.IsTrue(ObstacleMotion.Hits(log.kind, middle, 0.3f, 30.5f));
            Assert.IsFalse(ObstacleMotion.Hits(log.kind, middle, 2f, 30f), "a unit 2 m aside is safe while the log is in the middle");
            Assert.IsFalse(ObstacleMotion.Hits(log.kind, middle, 0f, 32f), "a unit 2 m behind the log's line is safe");

            ObstacleMotion.Pose end = ObstacleMotion.PoseAt(log, period * 0.25f, 0f, Half);
            Assert.Greater(end.Hazard.x, Half, "at the end of its swing the log is past the lane edge");
            Assert.IsFalse(end.Deadly, "at the end of its swing the log is up, over the units");

            // Over one cycle the deadly band covers the whole lane: no place across is safe to wait.
            for (float x = -Half + 0.1f; x < Half; x += 0.25f)
            {
                bool hit = false;
                for (float t = 0f; t < period && !hit; t += period / 400f)
                {
                    hit = ObstacleMotion.Hits(log.kind, ObstacleMotion.PoseAt(log, t, 0f, Half), x, 30f);
                }

                Assert.IsTrue(hit, $"the log never reaches x={x:0.00}");
            }
        }

        [Test]
        public void TheSawRunsFromEdgeToEdgeAndCutsOnlyItsLine()
        {
            var saw = new ObstaclePlacement(ObstacleKind.Saw, 40f);
            float period = ObstacleMotion.Period(saw);
            ObstacleMotion.Pose left = ObstacleMotion.PoseAt(saw, 0f, 0f, Half);
            ObstacleMotion.Pose right = ObstacleMotion.PoseAt(saw, period * 0.5f, 0f, Half);
            Assert.AreEqual(-(Half - ObstacleMotion.SawEdgeInset), left.Hazard.x, 1e-3f);
            Assert.AreEqual(Half - ObstacleMotion.SawEdgeInset, right.Hazard.x, 1e-3f);
            Assert.AreNotEqual(left.Angle, right.Angle, "the blade spins");
            Assert.IsTrue(ObstacleMotion.Hits(saw.kind, right, right.Hazard.x - 0.5f, 40.2f));
            Assert.IsFalse(ObstacleMotion.Hits(saw.kind, right, right.Hazard.x, 41f), "a unit 1 m before the blade's line is safe");
            Assert.IsFalse(ObstacleMotion.Hits(saw.kind, right, 0f, 40f), "the middle is safe while the blade is at the edge");
        }

        [Test]
        public void TheHammerKillsItsHalfOnlyWhileDown()
        {
            var hammer = new ObstaclePlacement(ObstacleKind.Hammer, 50f, -1f);
            float period = ObstacleMotion.Period(hammer);
            Assert.AreEqual(0f, ObstacleMotion.HammerHeight(0f), 1e-4f, "the cycle starts down");
            Assert.AreEqual(ObstacleMotion.HammerLift, ObstacleMotion.HammerHeight(0.6f), 1e-4f, "it waits up");
            Assert.AreEqual(0f, ObstacleMotion.HammerHeight(0.95f), 1e-4f, "it ends the cycle down");
            Assert.Greater(ObstacleMotion.HammerHeight(0.82f), 0f);
            Assert.Less(ObstacleMotion.HammerHeight(0.84f), ObstacleMotion.HammerHeight(0.80f), "the slam goes down");

            ObstacleMotion.Pose down = ObstacleMotion.PoseAt(hammer, period * 0.95f, 0f, Half);
            ObstacleMotion.Pose up = ObstacleMotion.PoseAt(hammer, period * 0.6f, 0f, Half);
            Assert.Less(down.Hazard.x, 0f, "across -1 is the left half");
            Assert.IsTrue(ObstacleMotion.Hits(hammer.kind, down, -2f, 50.3f));
            Assert.IsFalse(ObstacleMotion.Hits(hammer.kind, up, -2f, 50.3f), "up, it kills nobody");
            for (float t = 0f; t < period; t += period / 200f)
            {
                Assert.IsFalse(ObstacleMotion.Hits(hammer.kind, ObstacleMotion.PoseAt(hammer, t, 0f, Half), 0.5f, 50f),
                    "the other half is always safe");
            }

            var right = new ObstaclePlacement(ObstacleKind.Hammer, 50f, 0.3f);
            Assert.Greater(ObstacleMotion.PoseAt(right, 0f, 0f, Half).Hazard.x, 0f, "across >= 0 is the right half");
        }

        [Test]
        public void TheHoleSwallowsOnlyUnitsOverItAndLeavesAWayPast()
        {
            var hole = new ObstaclePlacement(ObstacleKind.Hole, 60f, 1f);
            ObstacleMotion.Pose pose = ObstacleMotion.PoseAt(hole, 3f, 0f, Half);
            Assert.AreEqual(Half - ObstacleMotion.HoleHalfWidth, pose.Hazard.x, 1e-4f, "across 1 puts the pit against the right edge");
            Assert.IsTrue(ObstacleMotion.Hits(hole.kind, pose, pose.Hazard.x, 60f));
            Assert.IsFalse(ObstacleMotion.Hits(hole.kind, pose, pose.Hazard.x - ObstacleMotion.HoleHalfWidth - 0.1f, 60f));
            Assert.IsFalse(ObstacleMotion.Hits(hole.kind, pose, pose.Hazard.x, 60f + ObstacleMotion.HoleHalfDepth + 0.1f));
            Assert.Greater(Half * 2f - ObstacleMotion.HoleHalfWidth * 2f, Half, "the pit leaves more than half the track free");
        }

        [Test]
        public void TheRollerRollsBackTowardTheCrowdRisingAndSinking()
        {
            var roller = new ObstaclePlacement(ObstacleKind.Roller, 70f, -1f);
            float period = ObstacleMotion.Period(roller);
            ObstacleMotion.Pose start = ObstacleMotion.PoseAt(roller, 0.001f, 0f, Half);
            ObstacleMotion.Pose middle = ObstacleMotion.PoseAt(roller, period * 0.5f, 0f, Half);
            Assert.IsFalse(start.Deadly, "it starts sunk in the track");
            Assert.Less(start.Anchor.y, 0f);
            Assert.IsTrue(middle.Deadly, "half way, it is up and rolling");
            Assert.AreEqual(70f - ObstacleMotion.RollerRun * 0.5f, middle.Hazard.z, 1e-3f, "it rolls back toward the crowd");
            Assert.Less(middle.Angle, start.Angle, "it turns as it rolls");
            Assert.IsTrue(ObstacleMotion.Hits(roller.kind, middle, middle.Hazard.x + 1f, middle.Hazard.z + 0.5f));
            Assert.IsFalse(ObstacleMotion.Hits(roller.kind, middle, 1.5f, middle.Hazard.z), "the other lanes are safe");
        }

        [Test]
        public void EveryKindKillsOnlyInsideItsReach()
        {
            foreach (ObstacleKind kind in Enum.GetValues(typeof(ObstacleKind)))
            {
                var obstacle = new ObstaclePlacement(kind, 50f, 0.5f, 0f, 0.3f);
                ObstacleMotion.Reach(obstacle, out float near, out float far);
                float period = Mathf.Max(1f, ObstacleMotion.Period(obstacle));
                for (float t = 0f; t < period; t += period / 100f)
                {
                    ObstacleMotion.Pose pose = ObstacleMotion.PoseAt(obstacle, t, 0f, Half);
                    for (float z = near - 3f; z < far + 3f; z += 0.1f)
                    {
                        for (float x = -Half; x <= Half; x += 0.5f)
                        {
                            if (ObstacleMotion.Hits(kind, pose, x, z))
                            {
                                Assert.That(z, Is.InRange(near, far), $"{kind} killed at z={z:0.00} outside its reach {near:0.00}-{far:0.00}");
                            }
                        }
                    }
                }
            }
        }

        // ---------- level rules ----------

        [Test]
        public void LevelsRejectObstaclesThatBreakTheRules()
        {
            ClaybreakLevel level = ScriptableObject.CreateInstance<ClaybreakLevel>();
            try
            {
                Assert.IsTrue(Validate(level, new ObstaclePlacement(ObstacleKind.Saw, 40f)), "one saw mid-run is fine");
                Assert.IsFalse(Validate(level, new ObstaclePlacement(ObstacleKind.Hole, 3f)), "too close to the start");
                Assert.IsFalse(Validate(level, new ObstaclePlacement(ObstacleKind.Hammer, 77f)), "too close to the firing line (80)");
                Assert.IsFalse(Validate(level, new ObstaclePlacement(ObstacleKind.Roller, 12f)), "the roller's run starts behind the start");
                Assert.IsFalse(Validate(level, new ObstaclePlacement(ObstacleKind.Saw, 40f, 0f, 0.5f)), "faster than the eye follows");
                Assert.IsTrue(Validate(level, new ObstaclePlacement(ObstacleKind.Hole, 40f, 0f, 0.5f)), "the hole has no cycle");
                var many = new ObstaclePlacement[ClaybreakLevel.MaximumObstacles + 1];
                for (int i = 0; i < many.Length; i++)
                {
                    many[i] = new ObstaclePlacement(ObstacleKind.Hole, 10f + i * 8f);
                }

                Assert.IsFalse(Validate(level, many), "more obstacles than the course holds");
            }
            finally
            {
                Object.DestroyImmediate(level);
            }
        }

        [Test]
        public void TheReactionWindowKeepsObstaclesAwayFromTheStartAndTheGates()
        {
            RunGatePair[] gates = { new RunGatePair { z = 40f, left = new GateSpec(GateOperation.Add, 5), right = new GateSpec(GateOperation.Multiply, 2) } };
            const float legible = 35f;
            const float speed = 9f;
            ReactionWindow.Result fine = ReactionWindow.Of(speed, gates, new[] { new ObstaclePlacement(ObstacleKind.Saw, 60f) }, legible);
            Assert.IsTrue(fine.Passes, $"sight {fine.ObstacleSightSeconds:0.00} s, gap {fine.ObstacleGapSeconds:0.00} s");
            ReactionWindow.Result nearGate = ReactionWindow.Of(speed, gates, new[] { new ObstaclePlacement(ObstacleKind.Saw, 48f) }, legible);
            Assert.IsFalse(nearGate.ObstaclesPass, "8 m after a gate pair at 9 m/s is under 1.6 s");
            ReactionWindow.Result rollerOverGate = ReactionWindow.Of(speed, gates, new[] { new ObstaclePlacement(ObstacleKind.Roller, 45f) }, legible);
            Assert.AreEqual(0f, rollerOverGate.ObstacleGapSeconds, 1e-4f, "a roller rolling through a gate pair");
            ReactionWindow.Result early = ReactionWindow.Of(speed, new RunGatePair[0], new[] { new ObstaclePlacement(ObstacleKind.Hole, 20f) }, legible);
            Assert.IsFalse(early.ObstaclesPass, "20 m from the start at 9 m/s is under 3 s");
            ReactionWindow.Result none = ReactionWindow.Of(speed, gates, legible);
            Assert.IsTrue(none.ObstaclesPass, "no obstacle, nothing to dodge");
        }

        // ---------- content ----------

        [Test]
        public void TheLabHasEveryKindAndIsFair()
        {
            LevelSpec spec = WorldContent.ObstacleLab();
            ClaybreakLevel lab = ScriptableObject.CreateInstance<ClaybreakLevel>();
            try
            {
                lab.EditorAssign(spec);
                Assert.IsTrue(lab.TryValidate(ArmyLimit.Units, ClaybreakSceneBuilder.FlightCapacity, out string error), error);
                Assert.AreEqual(ObstacleMotion.KindCount, Kinds(lab), "the lab shows every kind");
                Assert.IsFalse(lab.IsDeviceGateLevel, "the lab is played");
                var profile = AssetDatabase.LoadAssetAtPath<SwarmCameraProfile>(ClaybreakSceneBuilder.CrowdCameraProfilePath);
                Assert.IsNotNull(profile, "build the scene first");
                float legible = ReactionWindow.LegibleDistance(profile.Settings, ArmyLimit.Units, Half, GateArt.LabelEm);
                ReactionWindow.Result result = ReactionWindow.Of(lab.RunSpeed, lab.RunGates, lab.Obstacles, legible);
                Assert.IsTrue(result.Passes, $"lab at {lab.RunSpeed} m/s: obstacle sight {result.ObstacleSightSeconds:0.00} s, "
                    + $"gap {result.ObstacleGapSeconds:0.00} s, gates {result.SightSeconds:0.00}/{result.GapSeconds:0.00} s");
            }
            finally
            {
                Object.DestroyImmediate(lab);
            }
        }

        [Test]
        public void TheDeviceGateRunsTheMostObstaclesOfEveryKindBeforeItsFiringLine()
        {
            for (int w = 0; w < WorldContent.Worlds.Length; w++)
            {
                var stress = AssetDatabase.LoadAssetAtPath<ClaybreakLevel>(WorldContent.StressLevelPath(w));
                Assert.IsNotNull(stress, "build the scene first");
                Assert.AreEqual(ClaybreakLevel.MaximumObstacles, stress.Obstacles.Count, stress.name);
                Assert.AreEqual(ObstacleMotion.KindCount, Kinds(stress), stress.name);
                Assert.IsTrue(stress.TryValidate(ArmyLimit.Units, ClaybreakSceneBuilder.FlightCapacity, out string error), error);
            }

            Assert.AreEqual(0, WorldContent.Specs(WorldContent.Worlds[0])[0].obstacles.Length,
                "the 40 levels get their obstacles in RT-6t.19, not here");
        }

        [Test]
        public void EveryKindHasAToonMeshWithABodyAndFixedParts()
        {
            foreach (ObstacleKind kind in Enum.GetValues(typeof(ObstacleKind)))
            {
                Mesh mesh = ObstacleArt.Build(kind);
                try
                {
                    Assert.Greater(mesh.vertexCount, 0, kind.ToString());
                    Assert.AreEqual(1, mesh.subMeshCount, $"{kind}: one draw per kind needs one submesh");
                    Color32[] colors = mesh.colors32;
                    bool tinted = false, fixedColor = false;
                    foreach (Color32 c in colors)
                    {
                        tinted |= c.a == 255;
                        fixedColor |= c.a == 0;
                    }

                    Assert.IsTrue(fixedColor, $"{kind} has parts in their own color");
                    if (kind != ObstacleKind.Hole)
                    {
                        Assert.IsTrue(tinted, $"{kind} has a body in the world's defense color");
                    }

                    Assert.Less(mesh.triangles.Length / 3, 2000, $"{kind} stays low-poly");
                }
                finally
                {
                    Object.DestroyImmediate(mesh);
                }
            }
        }

        private static bool Validate(ClaybreakLevel level, params ObstaclePlacement[] obstacles)
        {
            var spec = new LevelSpec { firingLineZ = 80f, obstacles = obstacles };
            level.EditorAssign(spec);
            bool valid = level.TryValidate(ArmyLimit.Units, ArmyLimit.Units, out string error);
            Debug.Log($"[ObstacleTests] {obstacles.Length} obstacle(s): {(valid ? "valid" : error)}");
            return valid;
        }

        private static int Kinds(ClaybreakLevel level)
        {
            var kinds = new System.Collections.Generic.HashSet<ObstacleKind>();
            foreach (ObstaclePlacement obstacle in level.Obstacles)
            {
                kinds.Add(obstacle.kind);
            }

            return kinds.Count;
        }
    }
}
