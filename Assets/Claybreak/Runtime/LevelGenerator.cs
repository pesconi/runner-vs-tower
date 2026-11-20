using System;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// A world's difficulty profile (RT-6; GDD 6, the "ChallengeDifficultyProfile"-like data): how its levels
    /// ramp from the first to the last, which towers and boss rule it uses, which defenses it introduces and
    /// when, what it pays, and the upgrade damage it is tuned for. <see cref="LevelGenerator"/> turns it into
    /// levels; a new world is a new profile plus its art, never new code.
    /// </summary>
    [Serializable]
    public sealed class WorldRecipe
    {
        public string worldId = "world";
        [Tooltip("Seed of the run gates, rows and lanes: the same recipe always makes the same levels.")]
        public int seed = 1;
        public int levelCount = ClaybreakWorld.MaximumLevels;

        [Header("Towers")]
        public TowerKind mainTower = TowerKind.Keep;
        public TowerKind secondTower = TowerKind.Forge;
        [Tooltip("Every Nth level stands the second tower (0 = never).")]
        public int secondTowerEvery = 3;
        public TowerKind bossTower = TowerKind.Boss;
        public BossRule bossRule = BossRule.ShiftGates;

        [Header("Difficulty profile")]
        [Tooltip("Damage multiplier (upgrades) the world is tuned for: tower health scales with it.")]
        public float expectedDamage = 1f;
        public int startFirst = 10;
        public int startLast = 16;
        public float runSpeedFirst = 5.5f;
        public float runSpeedLast = 6.5f;
        [Tooltip("Tower health as a share of the potential hits (best path through the gates, best gate of the row).")]
        public float shareFirst = 0.5f;
        public float shareLast = 0.65f;
        public float bossShare = 0.68f;
        [Tooltip("Size of a '+' run gate at the first and at the last level.")]
        public int addFirst = 8;
        public int addLast = 30;

        [Header("Defenses: first level index that may use each one (-1 = never)")]
        public int wallFrom = 1;
        public int sweeperFrom = 2;
        public int shieldFrom = -1;
        public int cratesFrom = 3;
        public float cannonIntervalFirst = 2.4f;
        public float cannonIntervalLast = 1.6f;
        public int projectilesFirst = 3;
        public int projectilesLast = 6;
        public float sweeperPeriodFirst = 3.4f;
        public float sweeperPeriodLast = 2.8f;
        public float shieldBlockFirst = 0.4f;
        public float shieldBlockLast = 0.6f;

        [Header("Reward")]
        public int rewardFirst = 40;
        public int rewardLast = 120;
        public int bossReward = 200;
    }

    /// <summary>
    /// Makes levels from a <see cref="WorldRecipe"/> (RT-6, "geração de fases por dados"). Deterministic: the
    /// seed and the level index pick the run gates (each pair offers a real choice or a trap), the gate row, the
    /// lanes of the wall and the shield; the profile ramps everything else. Tower health is not hand-picked:
    /// it is a share of the level's potential hits times the damage the world expects, so a harder gate
    /// layout is answered by a sturdier tower. Hand-made levels can replace any index; health then ramps
    /// between them.
    /// </summary>
    public static class LevelGenerator
    {
        /// <summary>The crowd's ceiling (the runner pool): no path through the gates goes past it.</summary>
        public const int ArmyCap = LevelSpec.DefaultUnitsInFlight;
        public const int GateStep = 20;
        public const int FirstGateZ = 18;

        /// <summary>The whole world: generated levels with <paramref name="fixedLevels"/> (null entries generate) in place.</summary>
        public static LevelSpec[] GenerateWorld(WorldRecipe recipe, LevelSpec[] fixedLevels = null)
        {
            if (recipe == null || recipe.levelCount < 2)
            {
                throw new ArgumentException("A recipe needs at least two levels (the last is the boss).", nameof(recipe));
            }

            var specs = new LevelSpec[recipe.levelCount];
            var generated = new bool[recipe.levelCount];
            int boss = specs.Length - 1;
            for (int i = 0; i < specs.Length; i++)
            {
                LevelSpec fixedSpec = fixedLevels != null && i < fixedLevels.Length ? fixedLevels[i] : null;
                generated[i] = fixedSpec == null;
                specs[i] = fixedSpec;
            }

            specs[boss] ??= Generate(recipe, boss);
            for (int i = 0; i < boss; i++)
            {
                if (generated[i])
                {
                    int lower = i > 0 ? specs[i - 1].towerHealth + 10 : 0;
                    int upper = specs[boss].towerHealth - 10 * (boss - i);
                    specs[i] = Closest(recipe, i, lower, upper);
                }
            }

            Ramp(specs, generated);
            return specs;
        }

        /// <summary>Layout variants tried per level so its own health (a fair share of its potential) fits the ramp.</summary>
        public const int Variants = 12;

        // The variant whose natural health lands inside [lower, upper], or the nearest one: the ramp then has
        // little to force, and a forced health never strays far from the level's real potential.
        private static LevelSpec Closest(WorldRecipe recipe, int index, int lower, int upper)
        {
            LevelSpec best = null;
            int bestDistance = int.MaxValue;
            for (int variant = 0; variant < Variants && bestDistance > 0; variant++)
            {
                LevelSpec candidate = Generate(recipe, index, variant);
                int health = candidate.towerHealth;
                int distance = health < lower ? lower - health : health > upper ? health - upper : 0;
                if (distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }

            return best;
        }

        /// <summary>One level of the recipe (no ramp against its neighbours); <paramref name="variant"/> picks another layout.</summary>
        public static LevelSpec Generate(WorldRecipe recipe, int index, int variant = 0)
        {
            int last = recipe.levelCount - 1;
            float t = last <= 0 ? 1f : Mathf.Clamp01((float)index / last);
            bool boss = index == last;
            var random = new Random32((uint)(recipe.seed * 7919 + index * 104729 + variant * 31337 + 17));
            var spec = new LevelSpec
            {
                title = boss ? "BOSS" : "LEVEL " + (index + 1),
                startCount = Mathf.RoundToInt(Mathf.Lerp(recipe.startFirst, recipe.startLast, t)),
                runSpeed = Round(Mathf.Lerp(recipe.runSpeedFirst, recipe.runSpeedLast, t), 0.5f)
            };

            // Run: 3 pairs early, up to 5; the boss runs the longest course.
            int pairs = boss ? 5 : 3 + (t >= 0.25f ? 1 : 0) + (t >= 0.6f ? 1 : 0);
            spec.runGates = RunGates(recipe, spec.startCount, pairs, t, ref random);
            spec.firingLineZ = spec.runGates[pairs - 1].z + 16f;
            int crates = index < recipe.cratesFrom || recipe.cratesFrom < 0 ? 0
                : boss ? ClaybreakLevel.MaximumCrates : Mathf.Clamp(1 + (index - recipe.cratesFrom) / 2, 1, 3);
            spec.crates = Crates(crates, spec.firingLineZ, ref random);

            // Gate row: one strong multiplier, one divider, one x2, shuffled (the first level stays gentle).
            int strong = t < 0.15f ? 2 : 3;
            var row = new[] { Mul(strong), Div(t >= 0.7f && random.Chance(0.4f) ? 3 : 2), Mul(2) };
            Shuffle(row, ref random);
            spec.assaultGates = row;
            int bestLane = BestRowLane(row);

            // Tower: floors and body by the ramp; health from the potential hits.
            spec.floors = boss ? ClaybreakLevel.MaximumFloors : Mathf.Clamp(1 + Mathf.FloorToInt(t * 3f), 1, 3);
            spec.tower = boss ? recipe.bossTower
                : recipe.secondTowerEvery > 0 && index % recipe.secondTowerEvery == recipe.secondTowerEvery - 1
                    ? recipe.secondTower : recipe.mainTower;
            spec.bossRule = boss ? recipe.bossRule : BossRule.None;
            spec.towerDistance = 30f + spec.floors * 1.5f;
            float share = boss ? recipe.bossShare : Mathf.Lerp(recipe.shareFirst, recipe.shareLast, t);
            spec.towerHealth = Math.Max(spec.floors * 10, RoundTo(share * PotentialHits(spec) * recipe.expectedDamage, 10));

            // Defenses, each from its first level on, not on every level (variety), always on the boss.
            spec.cannonEnabled = true;
            spec.cannonInterval = Round(Mathf.Lerp(recipe.cannonIntervalFirst, recipe.cannonIntervalLast, t), 0.05f);
            spec.projectileFlightTime = 0.9f - 0.05f * t;
            spec.blastRadius = Round(1.05f + 0.2f * t, 0.05f);
            spec.maximumProjectiles = Mathf.RoundToInt(Mathf.Lerp(recipe.projectilesFirst, recipe.projectilesLast, t));

            spec.wallEnabled = Allowed(recipe.wallFrom, index) && (boss || random.Chance(0.7f));
            spec.wallLane = random.Chance(0.6f) ? bestLane : random.Range(ClaybreakLevel.LaneCount);
            spec.wallDistance = 20f + random.Range(3) * 2f;
            spec.wallHealth = Math.Max(10, RoundTo(spec.towerHealth * 0.18f, 10));

            spec.sweeperEnabled = Allowed(recipe.sweeperFrom, index) && (boss || random.Chance(0.6f));
            spec.sweeperDistance = 14f + random.Range(3);
            spec.sweeperPeriod = Round(Mathf.Lerp(recipe.sweeperPeriodFirst, recipe.sweeperPeriodLast, t), 0.1f);

            bool shieldRule = boss && recipe.bossRule == BossRule.ShieldReturns;
            spec.shieldEnabled = shieldRule || (Allowed(recipe.shieldFrom, index) && (boss || random.Chance(0.65f)));
            spec.shieldLane = spec.wallEnabled && spec.wallLane != 1 ? 2 - spec.wallLane : (random.Chance(0.5f) ? 0 : 2);
            spec.shieldDistance = spec.towerDistance - 4f;
            spec.shieldHealth = Math.Max(10, RoundTo(spec.towerHealth * 0.14f, 10));
            spec.shieldBlock = Round(Mathf.Lerp(recipe.shieldBlockFirst, recipe.shieldBlockLast, t), 0.05f);

            spec.coinReward = boss ? recipe.bossReward : RoundTo(Mathf.Lerp(recipe.rewardFirst, recipe.rewardLast, t), 5);
            return spec;
        }

        /// <summary>Army at the firing line on the best path through the run gates (capped like the crowd).</summary>
        public static int BestArmy(LevelSpec spec)
        {
            int army = spec.startCount;
            for (int i = 0; i < spec.runGates.Length; i++)
            {
                army = Math.Max(GateMath.Apply(spec.runGates[i].left, army, ArmyCap), GateMath.Apply(spec.runGates[i].right, army, ArmyCap));
            }

            return Math.Min(army, ArmyCap);
        }

        /// <summary>Hits on the tower if the whole best-path army went through the best gate of the row and none fell.</summary>
        public static int PotentialHits(LevelSpec spec)
        {
            int best = 1;
            foreach (GateSpec gate in spec.assaultGates)
            {
                if (gate.operation == GateOperation.Multiply)
                {
                    best = Math.Max(best, gate.value);
                }
            }

            return BestArmy(spec) * best;
        }

        // Generated levels never get easier than the level before nor reach the boss (health rescales the
        // level's wall and shield with it); hand-made levels are left as they are.
        private static void Ramp(LevelSpec[] specs, bool[] generated)
        {
            int boss = specs.Length - 1;
            for (int i = 0; i < specs.Length; i++)
            {
                if (!generated[i])
                {
                    continue;
                }

                int health = specs[i].towerHealth;
                if (i > 0)
                {
                    health = Math.Max(health, specs[i - 1].towerHealth + 10);
                }

                if (i < boss)
                {
                    health = Math.Min(health, specs[boss].towerHealth - 10 * (boss - i));
                }

                Retune(specs[i], Math.Max(health, specs[i].floors));
            }
        }

        private static void Retune(LevelSpec spec, int health)
        {
            if (health == spec.towerHealth)
            {
                return;
            }

            float scale = (float)health / spec.towerHealth;
            spec.towerHealth = health;
            spec.wallHealth = Math.Max(10, RoundTo(spec.wallHealth * scale, 10));
            spec.shieldHealth = Math.Max(10, RoundTo(spec.shieldHealth * scale, 10));
        }

        private static RunGatePair[] RunGates(WorldRecipe recipe, int start, int pairs, float t, ref Random32 random)
        {
            var gates = new RunGatePair[pairs];
            int army = start;
            for (int k = 0; k < pairs; k++)
            {
                int add = Math.Max(2, RoundTo(Mathf.Lerp(recipe.addFirst, recipe.addLast, t) * (0.8f + 0.4f * random.Value), 5));
                GateSpec a;
                GateSpec b;
                float roll = random.Value;
                if (k == 0 || roll < 0.3f)
                {
                    // A real choice: "+n" against "x2", n near the army so either can be the better one.
                    a = Add(Math.Max(2, RoundTo(army * (0.7f + 0.7f * random.Value), army > 20 ? 5 : 1)));
                    b = Mul(2);
                }
                else if (roll < 0.9f || t < 0.3f)
                {
                    // One good gate and a trap.
                    a = army < 100 && random.Chance(0.45f) ? Mul(3) : random.Chance(0.5f) ? Mul(2) : Add(add);
                    b = random.Chance(0.5f) ? Sub(Math.Max(2, RoundTo(add * (0.8f + 0.4f * random.Value), 5))) : Div(2);
                }
                else
                {
                    // The lesser evil: a small loss against halving the army.
                    a = Sub(Math.Max(2, Math.Min(army / 3, add)));
                    b = Div(2);
                }

                bool swap = random.Chance(0.5f);
                gates[k] = new RunGatePair
                {
                    z = FirstGateZ + GateStep * k,
                    left = swap ? b : a,
                    right = swap ? a : b
                };
                army = Math.Max(GateMath.Apply(a, army, ArmyCap), GateMath.Apply(b, army, ArmyCap));
            }

            return gates;
        }

        // Crates sit halfway between gate pairs (10 m from each), alternating sides from a random start.
        private static CratePlacement[] Crates(int count, float firingLineZ, ref Random32 random)
        {
            var crates = new CratePlacement[count];
            bool right = random.Chance(0.5f);
            int placed = 0;
            for (int i = 0; i < count; i++)
            {
                float z = FirstGateZ + GateStep * i + GateStep * 0.5f;
                if (z >= firingLineZ - 3f)
                {
                    break;
                }

                crates[placed++] = new CratePlacement { z = z, right = right, units = 3 + random.Range(2) };
                right = !right;
            }

            Array.Resize(ref crates, placed);
            return crates;
        }

        private static int BestRowLane(GateSpec[] row)
        {
            int best = 0;
            for (int i = 1; i < row.Length; i++)
            {
                if (row[i].operation == GateOperation.Multiply
                    && (row[best].operation != GateOperation.Multiply || row[i].value > row[best].value))
                {
                    best = i;
                }
            }

            return best;
        }

        private static void Shuffle(GateSpec[] row, ref Random32 random)
        {
            for (int i = row.Length - 1; i > 0; i--)
            {
                int j = random.Range(i + 1);
                (row[i], row[j]) = (row[j], row[i]);
            }
        }

        private static bool Allowed(int from, int index) => from >= 0 && index >= from;
        private static int RoundTo(float value, int step) => Math.Max(step, (int)Math.Round(value / step) * step);
        private static float Round(float value, float step) => Mathf.Round(value / step) * step;
        private static GateSpec Add(int v) => new GateSpec(GateOperation.Add, v);
        private static GateSpec Sub(int v) => new GateSpec(GateOperation.Subtract, v);
        private static GateSpec Mul(int v) => new GateSpec(GateOperation.Multiply, v);
        private static GateSpec Div(int v) => new GateSpec(GateOperation.Divide, v);

        /// <summary>Small deterministic generator (xorshift32): the same seed gives the same levels on every machine.</summary>
        private struct Random32
        {
            private uint state;

            public Random32(uint seed)
            {
                state = seed == 0 ? 0x9E3779B9u : seed;
                for (int i = 0; i < 4; i++)
                {
                    Next();
                }
            }

            public float Value => (Next() & 0xFFFFFF) / 16777216f;
            public bool Chance(float probability) => Value < probability;
            public int Range(int count) => (int)(Next() % (uint)Math.Max(1, count));

            private uint Next()
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                return state;
            }
        }
    }
}
