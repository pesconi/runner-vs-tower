using System;
using System.Collections.Generic;
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

        [Header("Run obstacles (RT-6t.19): which kinds, from when, how many")]
        [Tooltip("Kinds the world's levels draw from, in the order the player meets them; with newObstacleAt set, the last is the world's new kind.")]
        public ObstacleKind[] obstacleKinds = new ObstacleKind[0];
        [Tooltip("Level index where the world's new kind (the last of obstacleKinds) first runs, alone and slow; it is not drawn before (-1 = no new kind).")]
        public int newObstacleAt = -1;
        [Tooltip("First level index with obstacles (-1 = the world has none).")]
        public int obstaclesFrom = -1;
        [Tooltip("Obstacles per level at obstaclesFrom and at the last plain level (ramped between them).")]
        public int obstaclesFirst = 1;
        public int obstaclesLast = 2;
        [Tooltip("The boss's own combination (empty = drawn like a plain level).")]
        public ObstacleKind[] bossObstacles = new ObstacleKind[0];
        [Tooltip("Obstacle cycle as a multiple of each kind's default, at the first and at the last level (above 1 = slower).")]
        public float obstacleSlowFirst = 1.15f;
        public float obstacleSlowLast = 1f;
    }

    /// <summary>
    /// An obstacle of a level before it is placed (RT-6t.19): what it is and after which run gate pair it stands. The course
    /// is opened around it by <see cref="LevelGenerator.PlaceObstacles"/>; its own <c>z</c> is ignored.
    /// </summary>
    public readonly struct ObstacleBeat
    {
        public ObstacleBeat(ObstaclePlacement obstacle, int afterPair)
        {
            Obstacle = obstacle;
            AfterPair = afterPair;
        }

        public ObstaclePlacement Obstacle { get; }

        /// <summary>Index of the run gate pair it follows; the last pair puts it between that pair and the firing line.</summary>
        public int AfterPair { get; }
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
        public const int GateStep = 20;
        public const int FirstGateZ = 18;

        /// <summary>
        /// The whole world: generated levels with <paramref name="fixedLevels"/> (null entries generate) in place.
        /// <paramref name="armyCap"/> is the game's crowd limit (<see cref="ArmyLimit"/>): no generated run gate promises
        /// more units than it, on any path (RT-6t.14).
        /// </summary>
        public static LevelSpec[] GenerateWorld(WorldRecipe recipe, int armyCap, LevelSpec[] fixedLevels = null)
        {
            if (recipe == null || recipe.levelCount < 2)
            {
                throw new ArgumentException("A recipe needs at least two levels (the last is the boss).", nameof(recipe));
            }

            if (armyCap < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(armyCap), "The crowd limit is at least one unit.");
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

            // The layouts are picked on drafts whose gates are not yet fitted to the limit (RT-6t.14): fitting lowers the
            // potential of the levels that near the limit, and picking on fitted levels would swap their layouts and
            // defenses; the picked layout is then fitted and its health follows its honest potential.
            var drafts = (LevelSpec[])specs.Clone();
            var variants = new int[specs.Length];
            if (generated[boss])
            {
                variants[boss] = StrongestVariant(recipe, boss, armyCap);
                drafts[boss] = Layout(recipe, boss, armyCap, variants[boss], false);
            }

            for (int i = 0; i < boss; i++)
            {
                if (generated[i])
                {
                    int lower = i > 0 ? drafts[i - 1].towerHealth + 10 : 0;
                    int upper = drafts[boss].towerHealth - 10 * (boss - i);
                    int target = Mathf.RoundToInt(drafts[boss].towerHealth * Mathf.Lerp(FirstLevelShareOfBoss, LastLevelShareOfBoss, (float)i / boss));
                    variants[i] = ClosestVariant(recipe, i, lower, upper, target, armyCap);
                    drafts[i] = Layout(recipe, i, armyCap, variants[i], false);
                }
            }

            for (int i = 0; i < specs.Length; i++)
            {
                specs[i] = generated[i] ? Generate(recipe, i, armyCap, variants[i]) : specs[i];
            }

            Ramp(specs, generated);
            return specs;
        }

        /// <summary>Layout variants tried per level so its own health (a fair share of its potential) fits the ramp.</summary>
        public const int Variants = 12;

        /// <summary>The ramp a world aims at: its first level near this share of the boss's health, its last plain level near the second.</summary>
        public const float FirstLevelShareOfBoss = 0.2f;
        public const float LastLevelShareOfBoss = 0.9f;

        // The boss is the world's peak: of its layouts, the one with the most potential hits.
        private static int StrongestVariant(WorldRecipe recipe, int index, int armyCap)
        {
            int best = 0;
            int bestHits = -1;
            for (int variant = 0; variant < Variants; variant++)
            {
                int hits = PotentialHits(Layout(recipe, index, armyCap, variant, false), armyCap);
                if (hits > bestHits)
                {
                    best = variant;
                    bestHits = hits;
                }
            }

            return best;
        }

        // The variant whose natural health lands inside [lower, upper] and nearest the target curve (or the
        // nearest to the interval): the ramp then has little to force, and a forced health never strays far
        // from the level's real potential.
        private static int ClosestVariant(WorldRecipe recipe, int index, int lower, int upper, int target, int armyCap)
        {
            int best = 0;
            long bestScore = long.MaxValue;
            for (int variant = 0; variant < Variants; variant++)
            {
                int health = Layout(recipe, index, armyCap, variant, false).towerHealth;
                int outside = health < lower ? lower - health : health > upper ? health - upper : 0;
                long score = outside * 100000L + Math.Abs(health - target);
                if (score < bestScore)
                {
                    best = variant;
                    bestScore = score;
                }
            }

            return best;
        }

        /// <summary>
        /// One level of the recipe (no ramp against its neighbours) under the crowd limit <paramref name="armyCap"/>;
        /// <paramref name="variant"/> picks another layout.
        /// </summary>
        public static LevelSpec Generate(WorldRecipe recipe, int index, int armyCap, int variant = 0) =>
            Layout(recipe, index, armyCap, variant, true);

        // fitToLimit false: the draft the layout is picked on (gates as rolled, the crowd only clamped at the limit).
        private static LevelSpec Layout(WorldRecipe recipe, int index, int armyCap, int variant, bool fitToLimit)
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

            // Run: 3 pairs early, up to 5; the boss runs the longest course. RT-6t.19: obstacles may take the room of a
            // pair, so the level's time stays the same (FitObstacles).
            int pairs = boss ? 5 : 3 + (t >= 0.25f ? 1 : 0) + (t >= 0.6f ? 1 : 0);
            ObstaclePlacement[] obstacles = FitObstacles(Obstacles(recipe, index), ref pairs,
                RunPace.Of(Round(Mathf.Lerp(recipe.runSpeedFirst, recipe.runSpeedLast, t), 0.5f)));

            spec.runGates = RunGates(recipe, spec.startCount, pairs, t, armyCap, ref random);
            spec.firingLineZ = spec.runGates[pairs - 1].z + 16f;
            int crates = index < recipe.cratesFrom || recipe.cratesFrom < 0 ? 0
                : boss ? ClaybreakLevel.MaximumCrates : Mathf.Clamp(1 + (index - recipe.cratesFrom) / 2, 1, 3);
            spec.crates = Crates(crates, spec.firingLineZ, ref random);
            if (fitToLimit)
            {
                FitToLimit(spec, armyCap);
            }

            PlaceObstacles(spec, Beats(obstacles, pairs));

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
            spec.towerHealth = Math.Max(spec.floors * 10, RoundTo(share * ExpectedHits(spec, armyCap) * recipe.expectedDamage, 10));

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
        public static int BestArmy(LevelSpec spec, int armyCap)
        {
            int army = spec.startCount;
            for (int i = 0; i < spec.runGates.Length; i++)
            {
                army = Math.Max(GateMath.Apply(spec.runGates[i].left, army, armyCap), GateMath.Apply(spec.runGates[i].right, army, armyCap));
            }

            return Math.Min(army, armyCap);
        }

        /// <summary>Hits on the tower if the whole best-path army went through the best gate of the row and none fell.</summary>
        public static int PotentialHits(LevelSpec spec, int armyCap) => BestArmy(spec, armyCap) * BestRowMultiplier(spec);

        /// <summary>
        /// RT-6t.19: <see cref="PotentialHits"/> after the run obstacles, for the player who dodges them
        /// (<see cref="ObstacleLoss"/>), at the level's game pace (<paramref name="spec"/> holds its course pace, as the
        /// generator and the world content write it before <see cref="RunPace"/>). The tower health is a share of it.
        /// </summary>
        public static int ExpectedHits(LevelSpec spec, int armyCap) =>
            ExpectedArmy(spec, armyCap, RunPace.Of(spec.runSpeed), true) * BestRowMultiplier(spec);

        /// <summary>
        /// RT-6t.19: the army at the firing line on the best path through the run gates after the obstacles take their
        /// expected share (<see cref="ObstacleLoss"/>) where they stand, at <paramref name="runSpeed"/> m/s; the player
        /// dodges them (<paramref name="dodging"/>) or runs straight down the middle. Without obstacles it is
        /// <see cref="BestArmy"/>.
        /// </summary>
        public static int ExpectedArmy(LevelSpec spec, int armyCap, float runSpeed, bool dodging)
        {
            ObstaclePlacement[] obstacles = spec.obstacles ?? Array.Empty<ObstaclePlacement>();
            var order = (ObstaclePlacement[])obstacles.Clone();
            Array.Sort(order, (a, b) => a.z.CompareTo(b.z));
            int army = Math.Min(spec.startCount, armyCap);
            int next = 0;
            for (int i = 0; i < spec.runGates.Length; i++)
            {
                for (; next < order.Length && order[next].z < spec.runGates[i].z; next++)
                {
                    army = Lose(order[next], army, runSpeed, dodging);
                }

                army = Math.Max(GateMath.Apply(spec.runGates[i].left, army, armyCap), GateMath.Apply(spec.runGates[i].right, army, armyCap));
            }

            for (; next < order.Length; next++)
            {
                army = Lose(order[next], army, runSpeed, dodging);
            }

            return Math.Min(army, armyCap);
        }

        /// <summary>
        /// RT-6t.19, for hand-made levels: scales the tower health (and the wall and the shield with it) by the share of the
        /// best-path army the obstacles are expected to leave, as the generator does for its own levels.
        /// </summary>
        public static void RetuneForLosses(LevelSpec spec, int armyCap)
        {
            int best = BestArmy(spec, armyCap);
            if (best <= 0 || spec.obstacles == null || spec.obstacles.Length == 0)
            {
                return;
            }

            float kept = (float)ExpectedArmy(spec, armyCap, RunPace.Of(spec.runSpeed), true) / best;
            Retune(spec, Math.Max(spec.floors, RoundTo(spec.towerHealth * kept, 10)));
        }

        private static int Lose(in ObstaclePlacement obstacle, int army, float runSpeed, bool dodging)
        {
            ObstacleLoss.Estimate loss = ObstacleLoss.Of(obstacle, army, runSpeed);
            return ObstacleLoss.Survivors(army, dodging ? loss.Dodging : loss.Straight);
        }

        private static int BestRowMultiplier(LevelSpec spec)
        {
            int best = 1;
            foreach (GateSpec gate in spec.assaultGates)
            {
                if (gate.operation == GateOperation.Multiply)
                {
                    best = Math.Max(best, gate.value);
                }
            }

            return best;
        }

        /// <summary>Time kept between an obstacle's deadly stretch and a run gate pair (s): the reaction window's 1.6 s and a margin.</summary>
        public const float ObstacleGapSeconds = 1.65f;

        /// <summary>Time between two obstacles that follow each other (s): one is passed before the next must be read.</summary>
        public const float ObstacleSpacingSeconds = 0.8f;

        /// <summary>A world's new obstacle kind runs this much slower the first time the player meets it.</summary>
        public const float IntroSlowdown = 1.3f;

        /// <summary>
        /// RT-6t.19: the world's obstacles for level <paramref name="index"/> (their place along the track still open): none
        /// before <see cref="WorldRecipe.obstaclesFrom"/>; the world's new kind alone and slow at
        /// <see cref="WorldRecipe.newObstacleAt"/> and first again on the next level; the boss's own combination; otherwise
        /// a count ramped from <see cref="WorldRecipe.obstaclesFirst"/> to <see cref="WorldRecipe.obstaclesLast"/>, kinds in
        /// turn so neighbouring levels differ. Cycles ramp from slow to the kind's pace; sides and phases come from the seed.
        /// </summary>
        public static ObstaclePlacement[] Obstacles(WorldRecipe recipe, int index)
        {
            ObstacleKind[] known = recipe.obstacleKinds ?? Array.Empty<ObstacleKind>();
            if (recipe.obstaclesFrom < 0 || index < recipe.obstaclesFrom || known.Length == 0)
            {
                return Array.Empty<ObstaclePlacement>();
            }

            int last = recipe.levelCount - 1;
            bool boss = index == last;
            int plainLast = Math.Max(recipe.obstaclesFrom + 1, last - 1);
            float ramp = boss ? 1f : Mathf.Clamp01((float)(index - recipe.obstaclesFrom) / (plainLast - recipe.obstaclesFrom));
            float slow = Mathf.Lerp(recipe.obstacleSlowFirst, recipe.obstacleSlowLast, ramp);
            bool hasNew = recipe.newObstacleAt >= 0;
            ObstacleKind newKind = known[known.Length - 1];

            var kinds = new List<ObstacleKind>();
            if (hasNew && index == recipe.newObstacleAt)
            {
                kinds.Add(newKind);
                slow *= IntroSlowdown;
            }
            else if (boss && recipe.bossObstacles != null && recipe.bossObstacles.Length > 0)
            {
                kinds.AddRange(recipe.bossObstacles);
            }
            else
            {
                var pool = new List<ObstacleKind>(known);
                if (hasNew && index < recipe.newObstacleAt)
                {
                    pool.Remove(newKind);
                }

                int count = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(recipe.obstaclesFirst, recipe.obstaclesLast, ramp)), 0,
                    Math.Min(pool.Count, ClaybreakLevel.MaximumObstacles));
                int start = hasNew && index == recipe.newObstacleAt + 1 ? pool.IndexOf(newKind) : index % Math.Max(1, pool.Count);
                for (int k = 0; k < count; k++)
                {
                    kinds.Add(pool[(start + k) % pool.Count]);
                }
            }

            if (kinds.Count > ClaybreakLevel.MaximumObstacles)
            {
                throw new ArgumentException($"{recipe.worldId} level {index + 1}: at most {ClaybreakLevel.MaximumObstacles} obstacles.", nameof(recipe));
            }

            var random = new Random32((uint)(recipe.seed * 6151 + index * 92821 + 29));
            var placed = new ObstaclePlacement[kinds.Count];
            for (int k = 0; k < kinds.Count; k++)
            {
                ObstacleKind kind = kinds[k];
                float period = ObstacleMotion.Moves(kind)
                    ? Mathf.Clamp(Round(ObstacleMotion.DefaultPeriod(kind) * slow, 0.05f), ObstacleMotion.MinimumPeriod, ObstacleMotion.MaximumPeriod)
                    : 0f;
                float side = random.Chance(0.5f) ? -1f : 1f;
                // The hole stays near the middle: on a side, a crowd squeezed against the other edge walks past it.
                float across = kind == ObstacleKind.Hammer ? side
                    : kind == ObstacleKind.Hole ? side * Round(0.1f + 0.4f * random.Value, 0.1f)
                    : kind == ObstacleKind.Roller ? side * Round(0.3f + 0.5f * random.Value, 0.1f)
                    : 0f;
                placed[k] = new ObstaclePlacement(kind, 0f, across, period, Round(random.Value, 0.05f));
            }

            return placed;
        }

        /// <summary>
        /// Seconds a level takes after its run on a win (assault, result and menu): the economy's estimate [I], kept here so
        /// the generator holds each level's time against the same number the economy counts.
        /// </summary>
        public const float SecondsAfterRun = 39f;

        /// <summary>
        /// How far obstacles may move a level's time (run plus <see cref="SecondsAfterRun"/>) from what it was before them,
        /// as a share (RT-6t.19 keeps it within 5%; a hair under, for the half-metre rounding of the course).
        /// </summary>
        public const float TimeTolerance = 0.048f;

        /// <summary>
        /// RT-6t.19: keeps a level's time while it gains obstacles. The course a level would run (its pairs on the usual steps)
        /// is opened around the obstacles as <see cref="PlaceObstacles"/> does; when that moves the level's time by more than
        /// <see cref="TimeTolerance"/> at <paramref name="runSpeed"/> (game pace), the obstacles take the room of one
        /// gate pair (never below 3 pairs), and if the time is still off the last obstacles are left out. Returns the obstacles
        /// kept and sets <paramref name="pairs"/>.
        /// </summary>
        public static ObstaclePlacement[] FitObstacles(ObstaclePlacement[] obstacles, ref int pairs, float runSpeed)
        {
            float baseCourse = CourseLength(pairs, Array.Empty<ObstaclePlacement>(), runSpeed);
            float budget = TimeTolerance * (baseCourse / runSpeed + SecondsAfterRun);
            int fewer = Math.Max(3, pairs - 1);
            for (int kept = obstacles.Length; kept > 0; kept--)
            {
                var trial = new ObstaclePlacement[kept];
                Array.Copy(obstacles, trial, kept);
                if (Math.Abs(CourseLength(pairs, trial, runSpeed) - baseCourse) / runSpeed <= budget)
                {
                    return trial;
                }

                if (fewer < pairs && Math.Abs(CourseLength(fewer, trial, runSpeed) - baseCourse) / runSpeed <= budget)
                {
                    pairs = fewer;
                    return trial;
                }
            }

            return Array.Empty<ObstaclePlacement>();
        }

        // The course (start to firing line) of a generated level with that many pairs and those obstacles.
        private static float CourseLength(int pairs, ObstaclePlacement[] obstacles, float runSpeed)
        {
            var gates = new RunGatePair[pairs];
            for (int k = 0; k < pairs; k++)
            {
                gates[k].z = FirstGateZ + GateStep * k;
            }

            var course = new LevelSpec { title = "course", runGates = gates, firingLineZ = gates[pairs - 1].z + 16f };
            PlaceObstacles(course, Beats(obstacles, pairs), runSpeed);
            return course.firingLineZ;
        }

        /// <summary>
        /// Where a level's obstacles stand among its <paramref name="pairs"/> run gate pairs: the first, third... between the
        /// last pair and the firing line; the second, fourth... in a gap earlier on the course.
        /// </summary>
        public static ObstacleBeat[] Beats(ObstaclePlacement[] obstacles, int pairs)
        {
            var beats = new ObstacleBeat[obstacles.Length];
            for (int k = 0; k < obstacles.Length; k++)
            {
                int after = k % 2 == 0 ? pairs - 1 : Math.Max(0, pairs - 2 - (k + 1) / 2);
                beats[k] = new ObstacleBeat(obstacles[k], after);
            }

            return beats;
        }

        /// <summary>
        /// RT-6t.19: puts the obstacles on the course and opens it just enough around them. Each group stands after its
        /// gate pair, <see cref="ObstacleGapSeconds"/> past it at the level's game pace, its members
        /// <see cref="ObstacleSpacingSeconds"/> apart; the next pair (or the firing line, 4 m on) moves on only if the group
        /// needs the room, and everything after it moves with it. A crate in a group's gap sits halfway between the pair and
        /// the group. With no obstacles nothing moves.
        /// </summary>
        public static void PlaceObstacles(LevelSpec spec, IReadOnlyList<ObstacleBeat> beats) =>
            PlaceObstacles(spec, beats, RunPace.Of(spec.runSpeed));

        // As above at the game-pace speed given (the time check works on a bare course).
        private static void PlaceObstacles(LevelSpec spec, IReadOnlyList<ObstacleBeat> beats, float speed)
        {
            if (beats == null || beats.Count == 0)
            {
                spec.obstacles = Array.Empty<ObstaclePlacement>();
                return;
            }

            int pairs = spec.runGates.Length;
            if (pairs == 0 || beats.Count > ClaybreakLevel.MaximumObstacles)
            {
                throw new ArgumentException($"{spec.title}: obstacles need a run gate pair to follow, at most {ClaybreakLevel.MaximumObstacles}.");
            }

            float gap = ObstacleGapSeconds * speed;
            float spacing = ObstacleSpacingSeconds * speed;
            var gates = (RunGatePair[])spec.runGates.Clone();
            CratePlacement[] crates = spec.crates == null ? new CratePlacement[0] : (CratePlacement[])spec.crates.Clone();
            var placed = new List<ObstaclePlacement>(beats.Count);
            float shift = 0f;
            for (int k = 0; k < pairs; k++)
            {
                float gateZ = spec.runGates[k].z;
                float nextZ = k + 1 < pairs ? spec.runGates[k + 1].z : spec.firingLineZ;
                gates[k].z = gateZ + shift;
                float cursor = gates[k].z + gap;
                float lastFar = float.MinValue;
                for (int b = 0; b < beats.Count; b++)
                {
                    if (beats[b].AfterPair < 0 || beats[b].AfterPair >= pairs)
                    {
                        throw new ArgumentException($"{spec.title}: obstacle {b} follows pair {beats[b].AfterPair}, the level has {pairs}.");
                    }

                    if (beats[b].AfterPair != k)
                    {
                        continue;
                    }

                    ObstaclePlacement obstacle = beats[b].Obstacle;
                    obstacle.z = 0f;
                    ObstacleMotion.Reach(obstacle, out float near, out float far);
                    obstacle.z = Mathf.Ceil((cursor - near) * 2f) * 0.5f;
                    placed.Add(obstacle);
                    lastFar = obstacle.z + far;
                    cursor = lastFar + spacing;
                }

                bool group = lastFar > float.MinValue;
                for (int c = 0; c < crates.Length; c++)
                {
                    if (spec.crates[c].z > gateZ && spec.crates[c].z < nextZ)
                    {
                        crates[c].z = group ? gates[k].z + Mathf.Round(gap * 0.5f) : spec.crates[c].z + shift;
                    }
                }

                if (group)
                {
                    float need = lastFar + (k + 1 < pairs ? gap : ClaybreakLevel.ObstacleClearance + 0.5f);
                    shift = Mathf.Max(shift, Mathf.Ceil((need - nextZ) * 2f) * 0.5f);
                }
            }

            spec.runGates = gates;
            spec.crates = crates;
            spec.firingLineZ += shift;
            placed.Sort((a, b) => a.z.CompareTo(b.z));
            spec.obstacles = placed.ToArray();
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

        private static RunGatePair[] RunGates(WorldRecipe recipe, int start, int pairs, float t, int armyCap, ref Random32 random)
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
                army = Math.Max(GateMath.Apply(a, army, armyCap), GateMath.Apply(b, army, armyCap));
            }

            return gates;
        }

        /// <summary>
        /// RT-6t.14: makes every run gate honest under the crowd limit. Walks the course with the largest army that can
        /// reach each pair (every operation is non-decreasing, so that army bounds every path; crates before the pair
        /// are counted as picked up) and shrinks any gate whose label would promise past <paramref name="armyCap"/>:
        /// ×N steps down to the largest multiplier that fits, then to a "+" of the room left; a "+" shrinks to the room
        /// left; with no room left the gate becomes a small loss (the pair turns into a lesser-evil choice). Gates that
        /// already fit are untouched, so levels that never neared the limit keep their exact numbers.
        /// </summary>
        public static void FitToLimit(LevelSpec spec, int armyCap)
        {
            int army = Math.Min(spec.startCount, armyCap);
            int crate = 0;
            for (int k = 0; k < spec.runGates.Length; k++)
            {
                for (; crate < spec.crates.Length && spec.crates[crate].z < spec.runGates[k].z; crate++)
                {
                    army = Math.Min(army + spec.crates[crate].units, armyCap);
                }

                GateSpec left = Fit(spec.runGates[k].left, army, armyCap);
                GateSpec right = Fit(spec.runGates[k].right, army, armyCap);
                if (Same(left, right))
                {
                    // Both sides fitted to the same gate: keep a real choice (half the room, or halving against the loss).
                    right = left.IsGood ? Fit(Add(Math.Max(2, (armyCap - army) / 2)), army, armyCap) : Div(2);
                    right = Same(left, right) ? Div(2) : right;
                }

                spec.runGates[k].left = left;
                spec.runGates[k].right = right;
                army = Math.Max(GateMath.Apply(left, army, armyCap), GateMath.Apply(right, army, armyCap));
            }
        }

        private static bool Same(GateSpec a, GateSpec b) => a.operation == b.operation && a.value == b.value;

        private static GateSpec Fit(GateSpec gate, int army, int armyCap)
        {
            if (GateMath.Promise(gate, army) <= armyCap)
            {
                return gate;
            }

            if (gate.operation == GateOperation.Multiply)
            {
                for (int value = gate.value - 1; value >= 2; value--)
                {
                    if ((long)army * value <= armyCap)
                    {
                        return Mul(value);
                    }
                }
            }

            // A "+" of the room left, on a round step when there is room for one.
            int room = armyCap - army;
            if (room >= 2)
            {
                int step = room >= 20 ? 5 : 1;
                return Add(room / step * step);
            }

            return Sub(Math.Max(2, Math.Min(10, army / 20)));
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
