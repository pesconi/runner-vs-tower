using System;
using System.Collections.Generic;
using JungleDegree.StudioKit.Swarm;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// RT-6t.19: the share of the crowd an obstacle is expected to kill, estimated with the game's own rules (the formation
    /// of <see cref="SunflowerFormation"/>, the motion and deadly areas of <see cref="ObstacleMotion"/>), so the tower health
    /// and the economy follow the army that really reaches the firing line. Pure and deterministic; results are cached, since
    /// the generator asks for the same obstacles many times.
    /// <para>The crowd passes the obstacle at the run speed with its units on their formation slots (kept inside the lane).
    /// The obstacle's clock at the crowd's arrival is unknown, so every result is an average over the arrival phase. Two
    /// players [I] (no analytics yet):</para>
    /// <list type="bullet">
    /// <item><see cref="Estimate.Dodging"/>: reads the obstacle and steers the crowd to where it loses least, but only
    /// within ±<see cref="PlaceSlackMetres"/> of that place and ±<see cref="TimingSlackSeconds"/> of the best moment (the
    /// best place over that window);</item>
    /// <item><see cref="Estimate.Straight"/>: never steers for it and crosses down the middle of the track.</item>
    /// </list>
    /// The game also caps kills per frame (6, so a burst reads); the estimate leaves the cap out, so it errs on the side of
    /// more losses and a slightly weaker tower.
    /// </summary>
    public static class ObstacleLoss
    {
        /// <summary>Half the run track (m): the game's 8 m track (the scene builder's <c>TrackWidth</c>, checked by a test).</summary>
        public const float TrackHalfWidth = 4f;

        /// <summary>How well a dodging player times an obstacle: anywhere within this many seconds of the best moment.</summary>
        public const float TimingSlackSeconds = 0.3f;

        /// <summary>How well a dodging player places the crowd across: anywhere within this many metres of the best place.</summary>
        public const float PlaceSlackMetres = 0.5f;

        private const int PhaseSamples = 24;
        private const int PlaceSamples = 17;
        private const int MaximumSampledUnits = 120;
        private const float StepSeconds = 1f / 30f;

        private static readonly Dictionary<Key, Estimate> cache = new Dictionary<Key, Estimate>();

        /// <summary>Shares of the crowd one obstacle kills (0-1).</summary>
        public readonly struct Estimate
        {
            public Estimate(float dodging, float straight)
            {
                Dodging = dodging;
                Straight = straight;
            }

            /// <summary>Share lost by a player who steers for it (the economy's player).</summary>
            public float Dodging { get; }

            /// <summary>Share lost by a player who runs straight down the middle.</summary>
            public float Straight { get; }
        }

        /// <summary>
        /// The share of a crowd of <paramref name="army"/> units, running at <paramref name="runSpeed"/> m/s, that
        /// <paramref name="obstacle"/> kills (its place along the track and its phase do not matter: the arrival phase is
        /// averaged out).
        /// </summary>
        public static Estimate Of(in ObstaclePlacement obstacle, int army, float runSpeed)
        {
            return Of(obstacle, army, runSpeed, SwarmFormationSettings.Default, TrackHalfWidth);
        }

        /// <summary>As <see cref="Of(in ObstaclePlacement, int, float)"/> on another formation or track.</summary>
        public static Estimate Of(in ObstaclePlacement obstacle, int army, float runSpeed, SwarmFormationSettings formation,
            float halfWidth)
        {
            if (army <= 0)
            {
                return new Estimate(0f, 0f);
            }

            // Bigger crowds are sampled on steps of 5 units: the share barely moves within one, and the cache stays small.
            int sampledArmy = army <= 20 ? army : Mathf.RoundToInt(army / 5f) * 5;
            var key = new Key(obstacle.kind, Mathf.Round(ObstacleMotion.Period(obstacle) * 100f), Mathf.Round(obstacle.across * 100f),
                sampledArmy, Mathf.Round(runSpeed * 100f), Mathf.Round(formation.Spacing * 1000f), Mathf.Round(formation.DepthStretch * 1000f),
                Mathf.Round(halfWidth * 100f));
            if (cache.TryGetValue(key, out Estimate known))
            {
                return known;
            }

            Estimate estimate = Compute(obstacle, sampledArmy, Mathf.Max(0.5f, runSpeed), formation, halfWidth);
            cache[key] = estimate;
            return estimate;
        }

        /// <summary>Units of <paramref name="army"/> left after an obstacle kills <paramref name="share"/> of them.</summary>
        public static int Survivors(int army, float share)
        {
            return Math.Max(0, army - Mathf.RoundToInt(army * Mathf.Clamp01(share)));
        }

        private static Estimate Compute(in ObstaclePlacement obstacle, int army, float speed, SwarmFormationSettings formation,
            float halfWidth)
        {
            // Units sampled evenly over the sunflower (every k-th slot covers the disc evenly).
            int stride = Math.Max(1, (army + MaximumSampledUnits - 1) / MaximumSampledUnits);
            int units = (army + stride - 1) / stride;
            var offsets = new Vector2[units];
            float deepest = 0f;
            for (int u = 0; u < units; u++)
            {
                Unity.Mathematics.float2 offset = SunflowerFormation.Offset(u * stride, formation.Spacing, formation.DepthStretch);
                offsets[u] = new Vector2(offset.x, offset.y);
                deepest = Mathf.Max(deepest, Mathf.Abs(offset.y));
            }

            float limit = Mathf.Max(0f, halfWidth - formation.EdgeMargin);
            var placement = new ObstaclePlacement(obstacle.kind, obstacle.z, obstacle.across, obstacle.period, 0f);
            ObstacleMotion.Reach(placement, out float near, out float far);
            float period = Mathf.Max(ObstacleMotion.MinimumPeriod, ObstacleMotion.Period(placement));

            // loss[place, phase]: the crowd centered at that place across, arriving at that share of the obstacle's cycle.
            var loss = new float[PlaceSamples, PhaseSamples];
            var hit = new bool[units];
            for (int p = 0; p < PlaceSamples; p++)
            {
                float centerX = Mathf.Lerp(-halfWidth, halfWidth, p / (PlaceSamples - 1f));
                for (int f = 0; f < PhaseSamples; f++)
                {
                    Array.Clear(hit, 0, units);
                    int killed = 0;
                    float arrival = period * f / PhaseSamples;

                    // From the crowd's front touching the stretch to its back leaving it.
                    float startZ = near - deepest - 0.5f;
                    float endZ = far + deepest + 0.5f;
                    for (float centerZ = startZ; centerZ <= endZ; centerZ += speed * StepSeconds)
                    {
                        float time = arrival + (centerZ - obstacle.z) / speed;
                        ObstacleMotion.Pose pose = ObstacleMotion.PoseAt(placement, time, 0f, halfWidth);
                        if (!pose.Deadly)
                        {
                            continue;
                        }

                        for (int u = 0; u < units; u++)
                        {
                            if (hit[u])
                            {
                                continue;
                            }

                            float x = Mathf.Clamp(centerX + offsets[u].x, -limit, limit);
                            if (ObstacleMotion.Hits(placement.kind, pose, x, centerZ + offsets[u].y))
                            {
                                hit[u] = true;
                                killed++;
                            }
                        }
                    }

                    loss[p, f] = (float)killed / units;
                }
            }

            int slack = Mathf.Max(0, Mathf.RoundToInt(TimingSlackSeconds / period * PhaseSamples));
            int placeSlack = Mathf.Max(0, Mathf.RoundToInt(PlaceSlackMetres / (2f * halfWidth / (PlaceSamples - 1))));
            float dodging = 0f;
            float straight = 0f;
            int middle = PlaceSamples / 2;
            for (int f = 0; f < PhaseSamples; f++)
            {
                float best = float.MaxValue;
                for (int p = 0; p < PlaceSamples; p++)
                {
                    // Places past the track edge count as the edge (the crowd squeezes against it).
                    float sum = 0f;
                    for (int q = -placeSlack; q <= placeSlack; q++)
                    {
                        int place = Mathf.Clamp(p + q, 0, PlaceSamples - 1);
                        for (int d = -slack; d <= slack; d++)
                        {
                            sum += loss[place, ((f + d) % PhaseSamples + PhaseSamples) % PhaseSamples];
                        }
                    }

                    best = Mathf.Min(best, sum / ((2 * slack + 1) * (2 * placeSlack + 1)));
                }

                dodging += best;
                straight += loss[middle, f];
            }

            return new Estimate(dodging / PhaseSamples, straight / PhaseSamples);
        }

        private readonly struct Key : IEquatable<Key>
        {
            private readonly ObstacleKind kind;
            private readonly float period;
            private readonly float across;
            private readonly int army;
            private readonly float speed;
            private readonly float spacing;
            private readonly float stretch;
            private readonly float halfWidth;

            public Key(ObstacleKind kind, float period, float across, int army, float speed, float spacing, float stretch, float halfWidth)
            {
                this.kind = kind;
                this.period = period;
                this.across = across;
                this.army = army;
                this.speed = speed;
                this.spacing = spacing;
                this.stretch = stretch;
                this.halfWidth = halfWidth;
            }

            public bool Equals(Key other) => kind == other.kind && period == other.period && across == other.across && army == other.army
                && speed == other.speed && spacing == other.spacing && stretch == other.stretch && halfWidth == other.halfWidth;

            public override bool Equals(object obj) => obj is Key other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = (int)kind;
                    hash = hash * 397 ^ period.GetHashCode();
                    hash = hash * 397 ^ across.GetHashCode();
                    hash = hash * 397 ^ army;
                    hash = hash * 397 ^ speed.GetHashCode();
                    hash = hash * 397 ^ spacing.GetHashCode();
                    hash = hash * 397 ^ stretch.GetHashCode();
                    return hash * 397 ^ halfWidth.GetHashCode();
                }
            }
        }
    }
}
