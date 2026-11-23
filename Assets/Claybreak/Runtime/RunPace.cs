using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// This game's run pace, its one source of truth (RT-6t.15, the user on 2026-11-23: "A crowd está lenta, aumentar
    /// cerca de 30% de velocidade"). The levels keep their course pace (the world recipes ramp it from the first level
    /// to the boss, the hand-made levels state it) and the game runs every course <see cref="Factor"/> faster: the
    /// world content applies it once, when it writes the levels, so the level assets hold the speed the crowd really
    /// runs and everything that reads it (the run, the economy times, the reaction-window check) agrees. The device
    /// gate levels keep their own fixed 12 m/s, still above the fastest paced level, so the measured scenery recycling
    /// stays the worst case. Raising this means checking the reaction window first (the editor's <c>ReactionWindow</c>, run by the validator).
    /// </summary>
    public static class RunPace
    {
        /// <summary>How much faster than its course pace every level runs (+30%, constant in every world).</summary>
        public const float Factor = 1.3f;

        /// <summary>Run speeds are kept on quarter steps (m/s) so the level data stays readable.</summary>
        public const float Step = 0.25f;

        /// <summary>The run speed of a course authored at <paramref name="coursePace"/>.</summary>
        public static float Of(float coursePace) => Mathf.Round(coursePace * Factor / Step) * Step;

        /// <summary>
        /// The specs at the game's pace, as copies (the world content's levels, once, before they are written): the
        /// inputs keep their course pace, so pacing the same hand-made levels twice never compounds.
        /// </summary>
        public static LevelSpec[] Apply(LevelSpec[] specs)
        {
            var paced = new LevelSpec[specs.Length];
            for (int i = 0; i < specs.Length; i++)
            {
                paced[i] = specs[i].ShallowCopy();
                paced[i].runSpeed = Of(specs[i].runSpeed);
            }

            return paced;
        }

        /// <summary>Seconds the crowd runs from the start to the firing line.</summary>
        public static float RunSeconds(float firingLineZ, float runSpeed) => firingLineZ / Mathf.Max(0.01f, runSpeed);
    }
}
