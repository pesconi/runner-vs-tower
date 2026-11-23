using System.Collections.Generic;
using System.Text;
using Claybreak.Editor;
using JungleDegree.StudioKit.Swarm;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.19: the run obstacles in the 40 levels. A friendly start (world 1 teaches one kind at a time, its first two
    /// levels have none), one new kind per later world met alone and slow, 2-3 per level at the end of world 4; every level
    /// keeps its reaction window and its RT-6t.15 time (±5%); the tower health follows the army the obstacles are expected to
    /// leave (<see cref="ObstacleLoss"/>), so wins and stars stay as they were.
    /// </summary>
    public sealed class ObstacleCurveTests
    {
        /// <summary>Win time after the run (assault, result and menu), as the economy counts it.</summary>
        private const float AfterRunSeconds = LevelGenerator.SecondsAfterRun;

        /// <summary>World 1's hand-made courses before the obstacles (RT-6t.15), by level index.</summary>
        private static readonly Dictionary<int, float> World1Courses = new Dictionary<int, float>
        {
            { 0, 78f }, { 1, 96f }, { 2, 98f }, { 3, 100f }, { 4, 104f }, { 5, 108f }, { 9, 112f }
        };

        [Test]
        public void TheRuleMatchesTheGameTrackAndTheReactionWindow()
        {
            Assert.AreEqual(ClaybreakSceneBuilder.TrackWidth * 0.5f, ObstacleLoss.TrackHalfWidth, 1e-6f);
            Assert.GreaterOrEqual(LevelGenerator.ObstacleGapSeconds, ReactionWindow.MinimumGapSeconds);
        }

        [Test]
        public void World1StartsCleanAndTeachesOneKindAtATime()
        {
            LevelSpec[] world = WorldContent.Specs(WorldContent.Worlds[0]);
            Assert.AreEqual(0, world[0].obstacles.Length, "level 1 wins on the first try (GDD 3)");
            Assert.AreEqual(0, world[1].obstacles.Length);
            AssertKinds(world[2], ObstacleKind.SwingingLog);
            AssertKinds(world[3], ObstacleKind.SwingingLog);
            AssertKinds(world[4], ObstacleKind.Hammer);
            Assert.Greater(ObstacleMotion.Period(world[2].obstacles[0]), ObstacleMotion.DefaultPeriod(ObstacleKind.SwingingLog), "the first log is slow");
            Assert.Greater(ObstacleMotion.Period(world[4].obstacles[0]), ObstacleMotion.DefaultPeriod(ObstacleKind.Hammer), "the first hammer is slow");
            for (int i = 5; i < world.Length; i++)
            {
                Assert.That(world[i].obstacles.Length, Is.InRange(1, 2), world[i].title);
                foreach (ObstaclePlacement obstacle in world[i].obstacles)
                {
                    Assert.That(obstacle.kind, Is.EqualTo(ObstacleKind.SwingingLog).Or.EqualTo(ObstacleKind.Hammer), world[i].title);
                }
            }

            CollectionAssert.AreEquivalent(new[] { ObstacleKind.SwingingLog, ObstacleKind.Hammer }, Kinds(world[9]), "the boss: log and hammer");
        }

        [Test]
        public void EachLaterWorldMeetsItsNewKindAloneAndSlow()
        {
            var met = new HashSet<ObstacleKind>();
            foreach (LevelSpec spec in WorldContent.Specs(WorldContent.Worlds[0]))
            {
                met.UnionWith(Kinds(spec));
            }

            for (int w = 1; w < 4; w++)
            {
                WorldDefinition definition = WorldContent.Worlds[w];
                WorldRecipe recipe = definition.Recipe;
                ObstacleKind fresh = recipe.obstacleKinds[recipe.obstacleKinds.Length - 1];
                Assert.IsFalse(met.Contains(fresh), $"{definition.Id}: {fresh} is new");
                LevelSpec[] world = WorldContent.Specs(definition);
                for (int i = 0; i < recipe.newObstacleAt; i++)
                {
                    CollectionAssert.DoesNotContain(Kinds(world[i]), fresh, $"{definition.Id} {world[i].title}");
                }

                LevelSpec intro = world[recipe.newObstacleAt];
                AssertKinds(intro, fresh);
                if (ObstacleMotion.Moves(fresh))
                {
                    Assert.Greater(ObstacleMotion.Period(intro.obstacles[0]), ObstacleMotion.DefaultPeriod(fresh), $"{definition.Id}: the new kind is slow");
                }

                CollectionAssert.Contains(Kinds(world[recipe.newObstacleAt + 1]), fresh, $"{definition.Id}: met again on the next level");
                foreach (LevelSpec spec in world)
                {
                    met.UnionWith(Kinds(spec));
                }
            }

            Assert.AreEqual(ObstacleMotion.KindCount, met.Count, "every kind runs somewhere in the 40 levels");
            LevelSpec[] last = WorldContent.Specs(WorldContent.Worlds[3]);
            for (int i = 6; i < last.Length; i++)
            {
                Assert.That(last[i].obstacles.Length, Is.InRange(2, 3), $"world 4 {last[i].title}: 2-3 at the end");
            }
        }

        [Test]
        public void EveryLevelKeepsItsReactionWindowAndItsCourse()
        {
            var profile = AssetDatabase.LoadAssetAtPath<SwarmCameraProfile>(ClaybreakSceneBuilder.CrowdCameraProfilePath);
            Assert.IsNotNull(profile, "build the scene first");
            float legible = ReactionWindow.LegibleDistance(profile.Settings, ArmyLimit.Units, ClaybreakSceneBuilder.TrackWidth * 0.5f,
                GateArt.LabelEm);
            foreach (WorldDefinition world in WorldContent.Worlds)
            {
                foreach (LevelSpec spec in WorldContent.Specs(world))
                {
                    Assert.LessOrEqual(spec.obstacles.Length, 3, $"{world.Id} {spec.title}: at most 3 (the device gate measured 6)");
                    ReactionWindow.Result result = ReactionWindow.Of(spec.runSpeed, spec.runGates, spec.obstacles, legible);
                    Assert.IsTrue(result.Passes, $"{world.Id} {spec.title}: sight {result.SightSeconds:0.00} s, gap {result.GapSeconds:0.00} s, "
                        + $"obstacle sight {result.ObstacleSightSeconds:0.00} s, obstacle gap {result.ObstacleGapSeconds:0.00} s");
                    var level = ScriptableObject.CreateInstance<ClaybreakLevel>();
                    try
                    {
                        level.EditorAssign(spec);
                        Assert.IsTrue(level.TryValidate(ArmyLimit.Units, ArmyLimit.Units, out string error), $"{world.Id} {spec.title}: {error}");
                    }
                    finally
                    {
                        Object.DestroyImmediate(level);
                    }
                }
            }
        }

        [Test]
        public void EveryLevelTakesItsRt6t15TimeWithinFivePercent()
        {
            foreach (WorldDefinition world in WorldContent.Worlds)
            {
                LevelSpec[] written = WorldContent.Specs(world);
                LevelSpec[] before = BeforeObstacles(world);
                for (int i = 0; i < written.Length; i++)
                {
                    float was = RunPace.RunSeconds(before[i].firingLineZ, written[i].runSpeed) + AfterRunSeconds;
                    float now = RunPace.RunSeconds(written[i].firingLineZ, written[i].runSpeed) + AfterRunSeconds;
                    Assert.AreEqual(was, now, was * 0.05f, $"{world.Id} {written[i].title}: {was:0.0} s before the obstacles, {now:0.0} s now");
                }
            }
        }

        [Test]
        public void TheExpectedLossesFollowTheCurve()
        {
            var table = new StringBuilder("[ObstacleCurve] world level: obstacles, loss dodging / straight\n");
            float[] firstWorld = Losses(WorldContent.Worlds[0], table);
            float[] lastWorld = Losses(WorldContent.Worlds[3], table);
            for (int w = 1; w < 3; w++)
            {
                Losses(WorldContent.Worlds[w], table);
            }

            Debug.Log(table.ToString());
            // The plan's targets [P]: a player who dodges loses 5-10% in world 1 and 15-25% at the end of world 4 (a level that
            // teaches a kind alone may cost nothing to one who dodges it).
            float first = 0f;
            for (int i = 2; i < firstWorld.Length; i++)
            {
                Assert.LessOrEqual(firstWorld[i], 0.15f, $"world 1 level {i + 1}");
                first += firstWorld[i] / (firstWorld.Length - 2);
            }

            Assert.That(first, Is.InRange(0.04f, 0.11f), "world 1, levels 3-10 on average");
            float end = (lastWorld[7] + lastWorld[8] + lastWorld[9]) / 3f;
            Assert.That(end, Is.InRange(0.13f, 0.27f), "world 4, its last three levels on average");
        }

        [Test]
        public void TheTowerHealthFollowsTheArmyAfterTheObstacles()
        {
            WorldRecipe recipe = WorldContent.Worlds[3].Recipe;
            LevelSpec level = LevelGenerator.Generate(recipe, 8, ArmyLimit.Units);
            Assert.Greater(level.obstacles.Length, 0);
            int potential = LevelGenerator.PotentialHits(level, ArmyLimit.Units);
            int expected = LevelGenerator.ExpectedHits(level, ArmyLimit.Units);
            Assert.Less(expected, potential, "the obstacles take their share before the firing line");
            float share = Mathf.Lerp(recipe.shareFirst, recipe.shareLast, 8f / 9f);
            Assert.AreEqual(share * expected * recipe.expectedDamage, level.towerHealth, 10f, "health is a share of the army left");

            LevelSpec plain = LevelGenerator.Generate(recipe, 8, ArmyLimit.Units);
            plain.obstacles = new ObstaclePlacement[0];
            Assert.AreEqual(LevelGenerator.PotentialHits(plain, ArmyLimit.Units), LevelGenerator.ExpectedHits(plain, ArmyLimit.Units),
                "without obstacles nothing is lost");
        }

        [Test]
        public void PlacingObstaclesOpensTheCourseOnlyWhereNeeded()
        {
            var spec = new LevelSpec
            {
                title = "TEST",
                runSpeed = 6f,
                runGates = new[] { Pair(18f), Pair(38f), Pair(58f) },
                crates = new[] { new CratePlacement { z = 28f, units = 3 }, new CratePlacement { z = 48f, units = 3 } },
                firingLineZ = 74f
            };
            LevelGenerator.PlaceObstacles(spec, new ObstacleBeat[0]);
            Assert.AreEqual(74f, spec.firingLineZ, "no obstacle, nothing moves");

            LevelGenerator.PlaceObstacles(spec, new[]
            {
                new ObstacleBeat(new ObstaclePlacement(ObstacleKind.Saw, 0f), 0),
                new ObstacleBeat(new ObstaclePlacement(ObstacleKind.Roller, 0f, 0.5f), 2)
            });
            float speed = RunPace.Of(6f);
            float gap = LevelGenerator.ObstacleGapSeconds * speed;
            Assert.AreEqual(2, spec.obstacles.Length);
            Assert.AreEqual(18f, spec.runGates[0].z, "the pair before every obstacle stays");
            foreach (ObstaclePlacement obstacle in spec.obstacles)
            {
                ObstacleMotion.Reach(obstacle, out float near, out float far);
                Assert.GreaterOrEqual(near, ClaybreakLevel.ObstacleClearance);
                Assert.LessOrEqual(far, spec.firingLineZ - ClaybreakLevel.ObstacleClearance);
                foreach (RunGatePair pair in spec.runGates)
                {
                    float apart = pair.z < near ? near - pair.z : pair.z > far ? pair.z - far : 0f;
                    Assert.GreaterOrEqual(apart, gap - 1e-3f, $"{obstacle.kind} keeps {gap:0.0} m from the pair at {pair.z}");
                }
            }

            Assert.Greater(spec.runGates[1].z, 38f, "the saw needed more than the 20 m gap");
            Assert.AreEqual(20f, spec.runGates[2].z - spec.runGates[1].z, 1e-3f, "a gap without obstacles keeps its length");
            Assert.Less(spec.crates[0].z, spec.obstacles[0].z, "the crate of the saw's gap stays before it");
            Assert.Greater(spec.crates[0].z, spec.runGates[0].z + 3f);
            Assert.AreEqual(spec.runGates[1].z + 10f, spec.crates[1].z, 1e-3f, "a crate in a plain gap moves with its pair");
        }

        [Test]
        public void TheLossEstimateReadsTheRules()
        {
            var hole = new ObstaclePlacement(ObstacleKind.Hole, 40f, 1f);
            ObstacleLoss.Estimate small = ObstacleLoss.Of(hole, 10, 8f);
            ObstacleLoss.Estimate big = ObstacleLoss.Of(hole, 300, 8f);
            Assert.AreEqual(0f, small.Dodging, 1e-6f, "a small crowd walks past a hole on the side");
            Assert.Greater(big.Dodging, small.Dodging, "a crowd wider than the room left cannot");
            Assert.GreaterOrEqual(big.Straight, big.Dodging);
            Assert.AreEqual(0f, ObstacleLoss.Of(hole, 0, 8f).Straight);
            foreach (ObstacleKind kind in new[] { ObstacleKind.SwingingLog, ObstacleKind.Saw, ObstacleKind.Hammer, ObstacleKind.Roller })
            {
                ObstacleLoss.Estimate loss = ObstacleLoss.Of(new ObstaclePlacement(kind, 40f, 0.5f), 100, 8f);
                Assert.That(loss.Dodging, Is.InRange(0f, 1f), kind.ToString());
                Assert.GreaterOrEqual(loss.Straight + 1e-6f, loss.Dodging, $"{kind}: steering never costs more than running straight");
                Assert.Greater(loss.Straight, 0f, $"{kind} kills whoever runs into it");
            }

            Assert.AreEqual(90, ObstacleLoss.Survivors(100, 0.1f));
        }

        private static float[] Losses(WorldDefinition world, StringBuilder table)
        {
            LevelSpec[] specs = WorldContent.Specs(world);
            var losses = new float[specs.Length];
            for (int i = 0; i < specs.Length; i++)
            {
                losses[i] = EconomyReport.ExpectedLoss(specs[i], specs[i].runSpeed, true);
                float straight = EconomyReport.ExpectedLoss(specs[i], specs[i].runSpeed, false);
                Assert.GreaterOrEqual(straight + 1e-6f, losses[i], $"{world.Id} {specs[i].title}");
                table.Append($"{world.Id} {specs[i].title}: {string.Join(" ", Kinds(specs[i]))}, {losses[i] * 100f:0.0}% / {straight * 100f:0.0}%, "
                    + $"army {LevelGenerator.BestArmy(specs[i], ArmyLimit.Units)}, health {specs[i].towerHealth}\n");
            }

            return losses;
        }

        // The world as RT-6t.15 wrote it: generated levels without obstacles, world 1's hand-made courses as they were.
        private static LevelSpec[] BeforeObstacles(WorldDefinition world)
        {
            WorldRecipe recipe = JsonUtility.FromJson<WorldRecipe>(JsonUtility.ToJson(world.Recipe));
            recipe.obstaclesFrom = -1;
            LevelSpec[] specs = LevelGenerator.GenerateWorld(recipe, ArmyLimit.Units);
            if (world.Id == SaveMigration.V1WorldId)
            {
                foreach (KeyValuePair<int, float> course in World1Courses)
                {
                    specs[course.Key].firingLineZ = course.Value;
                }
            }

            return specs;
        }

        private static List<ObstacleKind> Kinds(LevelSpec spec)
        {
            var kinds = new List<ObstacleKind>();
            foreach (ObstaclePlacement obstacle in spec.obstacles)
            {
                kinds.Add(obstacle.kind);
            }

            return kinds;
        }

        private static void AssertKinds(LevelSpec spec, ObstacleKind only)
        {
            Assert.AreEqual(1, spec.obstacles.Length, $"{spec.title}: one obstacle");
            Assert.AreEqual(only, spec.obstacles[0].kind, spec.title);
        }

        private static RunGatePair Pair(float z) => new RunGatePair
        {
            z = z,
            left = new GateSpec(GateOperation.Add, 5),
            right = new GateSpec(GateOperation.Multiply, 2)
        };
    }
}
