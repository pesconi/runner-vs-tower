using System;
using System.Collections.Generic;
using Claybreak.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.14 (D27 revised): the crowd limit is one value of this game (300), the engine takes it as a parameter, and
    /// no run gate of the 40 levels promises more units than the crowd can hold, on any path (crates included).
    /// </summary>
    public sealed class ArmyLimitTests
    {
        [Test]
        public void TheGameLimitIs300AndEverythingReadsItFromOnePlace()
        {
            Assert.AreEqual(300, ArmyLimit.Units, "D27 revised: 300 in the run and in flight");
            Assert.AreEqual(ArmyLimit.Units, ClaybreakSceneBuilder.CrowdCapacity, "the run swarm's capacity");
            Assert.AreEqual(ArmyLimit.Units, ClaybreakSceneBuilder.FlightCapacity, "the units-in-flight capacity");
            Assert.AreEqual(ArmyLimit.Units, LevelSpec.DefaultUnitsInFlight, "a level's units in flight");
        }

        [Test]
        public void NoRunGateOfThe40LevelsPromisesPastTheLimit()
        {
            int levels = 0;
            foreach (WorldDefinition world in WorldContent.Worlds)
            {
                LevelSpec[] specs = WorldContent.Specs(world);
                for (int i = 0; i < specs.Length; i++)
                {
                    long promise = GateMath.HighestPromise(specs[i].startCount, specs[i].runGates, specs[i].crates,
                        ArmyLimit.Units, out int pair);
                    Assert.LessOrEqual(promise, ArmyLimit.Units, $"{world.Id} level {i + 1}: pair {pair + 1} promises {promise}");
                    Assert.AreEqual(promise, BruteForcePromise(specs[i], ArmyLimit.Units), $"{world.Id} level {i + 1}");
                    levels++;
                }
            }

            Assert.AreEqual(40, levels);
        }

        [Test]
        public void TheGeneratorHonoursAnyLimit([Values(40, 100, 300, 1000)] int cap, [Values(1, 7, 42, 99, 303)] int seed)
        {
            var recipe = new WorldRecipe
            {
                worldId = "test", seed = seed, levelCount = 10, startFirst = 20, startLast = 30, addFirst = 15, addLast = 45,
                shieldFrom = 1, cratesFrom = 0
            };

            LevelSpec[] specs = LevelGenerator.GenerateWorld(recipe, cap);
            var level = ScriptableObject.CreateInstance<ClaybreakLevel>();
            try
            {
                for (int i = 0; i < specs.Length; i++)
                {
                    long promise = BruteForcePromise(specs[i], cap);
                    Assert.LessOrEqual(promise, cap, $"cap {cap} seed {seed} level {i + 1}");
                    level.EditorAssign(specs[i]);
                    Assert.IsTrue(level.TryValidate(cap, ArmyLimit.Units, out string error), $"cap {cap} seed {seed} level {i + 1}: {error}");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(level);
            }
        }

        [Test]
        public void ALevelThatNeverNearsTheLimitKeepsItsExactGates()
        {
            var recipe = new WorldRecipe { worldId = "test", seed = 5, levelCount = 10 };
            LevelSpec low = LevelGenerator.Generate(recipe, 0, 300);
            LevelSpec high = LevelGenerator.Generate(recipe, 0, 100000);
            Assert.AreEqual(JsonUtility.ToJson(high.runGates[0]), JsonUtility.ToJson(low.runGates[0]));
            Assert.AreEqual(JsonUtility.ToJson(high), JsonUtility.ToJson(low), "a first level never nears 300: nothing to fit");
        }

        [Test]
        public void FittingShrinksOnlyTheGatesThatWouldPassTheLimit()
        {
            var spec = new LevelSpec
            {
                startCount = 200,
                runGates = new[]
                {
                    Pair(20f, Gate(GateOperation.Multiply, 3), Gate(GateOperation.Subtract, 10)),
                    Pair(40f, Gate(GateOperation.Add, 50), Gate(GateOperation.Multiply, 2)),
                    Pair(60f, Gate(GateOperation.Divide, 2), Gate(GateOperation.Add, 5))
                },
                crates = new[] { new CratePlacement { z = 30f, units = 4 } }
            };

            LevelGenerator.FitToLimit(spec, 300);
            Assert.AreEqual(Gate(GateOperation.Add, 100), spec.runGates[0].left, "x3 and x2 do not fit 200: +100 does");
            Assert.AreEqual(Gate(GateOperation.Subtract, 10), spec.runGates[0].right, "a loss is never touched");
            // After the first pair the crowd is at 300 and the crate is full: no good gate fits, the pair is a lesser evil.
            Assert.AreEqual(Gate(GateOperation.Subtract, 10), spec.runGates[1].left);
            Assert.AreEqual(Gate(GateOperation.Divide, 2), spec.runGates[1].right, "never two equal gates");
            Assert.AreEqual(Gate(GateOperation.Divide, 2), spec.runGates[2].left);
            Assert.AreEqual(Gate(GateOperation.Add, 5), spec.runGates[2].right, "after -10 the crowd has room for +5 again");
            Assert.LessOrEqual(GateMath.HighestPromise(spec.startCount, spec.runGates, spec.crates, 300, out _), 300);
        }

        [Test]
        public void ALevelWhoseGatePromisesPastTheLimitIsRejected()
        {
            var spec = new LevelSpec
            {
                startCount = 160,
                runGates = new[] { Pair(20f, Gate(GateOperation.Multiply, 2), Gate(GateOperation.Add, 10)) },
                firingLineZ = 60f
            };

            var level = ScriptableObject.CreateInstance<ClaybreakLevel>();
            try
            {
                level.EditorAssign(spec);
                Assert.IsFalse(level.TryValidate(300, 300, out string error), "x2 at 160 promises 320");
                StringAssert.Contains("runGates[0]", error);
                Assert.IsTrue(level.TryValidate(400, 300, out error), "the same level fits a game whose limit is 400: " + error);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(level);
            }
        }

        [Test]
        public void CratesBeforeAGateCountTowardsItsPromise()
        {
            RunGatePair[] gates = { Pair(40f, Gate(GateOperation.Multiply, 2), Gate(GateOperation.Subtract, 5)) };
            var crates = new[] { new CratePlacement { z = 30f, units = 4 }, new CratePlacement { z = 50f, units = 4 } };
            Assert.AreEqual(300, GateMath.HighestPromise(146, gates, crates, 300, out int pair));
            Assert.AreEqual(0, pair);
            Assert.AreEqual(308, GateMath.HighestPromise(150, gates, crates, 300, out _), "150 + 4 from the crate, x2");
        }

        // Every path (left or right at each pair, all crates before it taken), the largest label promise.
        private static long BruteForcePromise(LevelSpec spec, int cap)
        {
            long best = Math.Min(spec.startCount, cap);
            int pairs = spec.runGates.Length;
            for (int path = 0; path < 1 << pairs; path++)
            {
                int army = Math.Min(spec.startCount, cap);
                var taken = new HashSet<int>();
                for (int k = 0; k < pairs; k++)
                {
                    for (int c = 0; c < spec.crates.Length; c++)
                    {
                        if (spec.crates[c].z < spec.runGates[k].z && taken.Add(c))
                        {
                            army = Math.Min(army + spec.crates[c].units, cap);
                        }
                    }

                    GateSpec gate = (path & (1 << k)) == 0 ? spec.runGates[k].left : spec.runGates[k].right;
                    long promise = GateMath.Promise(gate, army);
                    best = Math.Max(best, promise);
                    army = (int)Math.Max(0L, Math.Min(promise, cap));
                }
            }

            return best;
        }

        private static GateSpec Gate(GateOperation operation, int value) => new GateSpec(operation, value);

        private static RunGatePair Pair(float z, GateSpec left, GateSpec right) => new RunGatePair { z = z, left = left, right = right };
    }
}
