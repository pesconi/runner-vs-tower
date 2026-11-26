using System;
using JungleDegree.StudioKit.Swarm;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The cells of the Bibopzi's face atlas (RT-6v3.4b, <c>docs/direcao-visual-v3.md</c> 3.2), in atlas order: 4 columns,
    /// row 0 at the top. Drawn by <c>Tools/art/bibopzi_faces.py</c> (same order).
    /// </summary>
    public enum FaceExpression
    {
        Neutral,
        Happy,
        Scared,
        Effort,
        Victory,
        Defeat,
        Blink,
        /// <summary>"ui!": launched in the assault, thrown out by a gate or an obstacle.</summary>
        Ouch
    }

    /// <summary>What happens to the crowd that its face answers to (the keys of the guide's table).</summary>
    public enum FaceEvent
    {
        /// <summary>The run: the resting face (base).</summary>
        Run,
        /// <summary>Every few seconds on the resting face (scheduled by <see cref="FaceDirector"/>).</summary>
        Blink,
        GoodGate,
        BadGate,
        ObstacleHit,
        /// <summary>The crowd waiting at the firing line (base).</summary>
        Assault,
        /// <summary>The units in flight in the assault.</summary>
        Launch,
        /// <summary>Units on their way out (thrown by a gate, an obstacle or a defense).</summary>
        Thrown,
        /// <summary>Base, held until the next level.</summary>
        Victory,
        /// <summary>Base, held until the next level.</summary>
        Defeat
    }

    /// <summary>One row of the guide's table: an event shows an expression for a while (or holds it) at a priority.</summary>
    [Serializable]
    public struct FaceRule
    {
        public FaceEvent trigger;
        public FaceExpression expression;
        [Tooltip("Seconds it shows; 0 = a base face, held until another base.")]
        [Min(0f)] public float seconds;
        [Tooltip("A reaction shows over a base of lower or equal priority, and over a running reaction of lower or equal priority.")]
        public int priority;
        [Tooltip("How open the hands are (the kit's animated layout: 0 at rest, 1 wide open).")]
        [Range(0f, 1f)] public float handsOpen;

        public FaceRule(FaceEvent trigger, FaceExpression expression, float seconds, int priority, float handsOpen)
        {
            this.trigger = trigger;
            this.expression = expression;
            this.seconds = seconds;
            this.priority = priority;
            this.handsOpen = handsOpen;
        }

        public bool IsBase => seconds <= 0f;
    }

    /// <summary>The event → expression table of the style guide (data, RT-6v3.4b) and the blink rhythm.</summary>
    [Serializable]
    public sealed class FaceRules
    {
        public const int Cells = 8;
        public const int AtlasColumns = 4;
        public const int AtlasRows = 2;

        public FaceRule[] rules = Array.Empty<FaceRule>();
        [Tooltip("Seconds between two blinks of the resting face (0 = never).")]
        [Min(0f)] public float blinkEvery = 3.4f;

        public FaceRule Of(FaceEvent trigger)
        {
            foreach (FaceRule rule in rules)
            {
                if (rule.trigger == trigger)
                {
                    return rule;
                }
            }

            throw new ArgumentException($"the face table has no rule for {trigger}", nameof(trigger));
        }

        /// <summary>Null when complete, otherwise what is wrong.</summary>
        public string Validate()
        {
            var seen = new bool[Enum.GetValues(typeof(FaceEvent)).Length];
            var shown = new bool[Cells];
            foreach (FaceRule rule in rules)
            {
                int e = (int)rule.trigger;
                if (e < 0 || e >= seen.Length || seen[e])
                {
                    return $"the face table needs exactly one rule per event ({rule.trigger} again or unknown)";
                }

                seen[e] = true;
                if ((int)rule.expression < 0 || (int)rule.expression >= Cells)
                {
                    return $"{rule.trigger}: expression {(int)rule.expression} is not an atlas cell";
                }

                shown[(int)rule.expression] = true;
                if (float.IsNaN(rule.seconds) || rule.seconds < 0f || rule.seconds > 10f || float.IsNaN(rule.handsOpen)
                    || rule.handsOpen < 0f || rule.handsOpen > 1f)
                {
                    return $"{rule.trigger}: seconds within 0..10 and hands within 0..1";
                }
            }

            for (int e = 0; e < seen.Length; e++)
            {
                if (!seen[e])
                {
                    return $"the face table has no rule for {(FaceEvent)e}";
                }
            }

            for (int c = 0; c < shown.Length; c++)
            {
                if (!shown[c])
                {
                    return $"the atlas cell {(FaceExpression)c} is never shown";
                }
            }

            FaceRule run = Of(FaceEvent.Run);
            FaceRule blink = Of(FaceEvent.Blink);
            if (!run.IsBase || !Of(FaceEvent.Assault).IsBase || !Of(FaceEvent.Victory).IsBase || !Of(FaceEvent.Defeat).IsBase)
            {
                return "run, assault, victory and defeat are base faces (seconds 0)";
            }

            if (blink.IsBase || blink.priority <= run.priority || !(blinkEvery == 0f || blinkEvery > blink.seconds * 4f))
            {
                return "the blink is short, over the run face, and far rarer than it lasts";
            }

            return null;
        }
    }

    /// <summary>
    /// Which face the crowd shows (RT-6v3.4b), pure and testable: a base face (run, assault, victory, defeat) and, over it,
    /// one timed reaction (a gate, an obstacle) when its priority is at least the base's and the running reaction's. The
    /// resting face blinks every <see cref="FaceRules.blinkEvery"/> seconds when the blink outranks the base. Time is passed
    /// in (no clock inside).
    /// </summary>
    public sealed class FaceDirector
    {
        private readonly FaceRules rules;
        private FaceRule baseFace;
        private FaceRule reaction;
        private float reactionUntil = float.NegativeInfinity;
        private float blinkPhase;

        public FaceDirector(FaceRules rules)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            string error = rules.Validate();
            if (error != null)
            {
                throw new ArgumentException(error, nameof(rules));
            }

            Reset(0f);
        }

        public FaceRule Base => baseFace;

        /// <summary>Back to the run face, no reaction; the blink rhythm starts at <paramref name="now"/>.</summary>
        public void Reset(float now)
        {
            baseFace = rules.Of(FaceEvent.Run);
            reactionUntil = float.NegativeInfinity;
            blinkPhase = now;
        }

        /// <summary>The crowd lived <paramref name="trigger"/> at <paramref name="now"/>; true when the face changes for it.</summary>
        public bool Trigger(FaceEvent trigger, float now)
        {
            FaceRule rule = rules.Of(trigger);
            if (rule.IsBase)
            {
                baseFace = rule;
                if (now < reactionUntil && reaction.priority < rule.priority)
                {
                    reactionUntil = float.NegativeInfinity;
                }

                return true;
            }

            if (rule.priority < baseFace.priority || (now < reactionUntil && rule.priority < reaction.priority))
            {
                return false;
            }

            reaction = rule;
            reactionUntil = now + rule.seconds;
            return true;
        }

        /// <summary>The rule shown at <paramref name="now"/>.</summary>
        public FaceRule Current(float now)
        {
            if (now < reactionUntil)
            {
                return reaction;
            }

            FaceRule blink = rules.Of(FaceEvent.Blink);
            if (rules.blinkEvery > 0f && blink.priority > baseFace.priority
                && Mathf.Repeat(now - blinkPhase, rules.blinkEvery) >= rules.blinkEvery - blink.seconds)
            {
                return blink;
            }

            return baseFace;
        }

        /// <summary>The crowd's expression at <paramref name="now"/>: the current face, and the thrown face for the dying.</summary>
        public SwarmExpression Crowd(float now)
        {
            FaceRule face = Current(now);
            FaceRule thrown = rules.Of(FaceEvent.Thrown);
            return new SwarmExpression((int)face.expression, face.handsOpen, (int)thrown.expression, thrown.handsOpen);
        }

        /// <summary>The units in flight: launched, or the level's end face once it is won or lost.</summary>
        public SwarmExpression Flyers()
        {
            FaceRule flying = baseFace.trigger == FaceEvent.Victory || baseFace.trigger == FaceEvent.Defeat
                ? baseFace
                : rules.Of(FaceEvent.Launch);
            FaceRule thrown = rules.Of(FaceEvent.Thrown);
            return new SwarmExpression((int)flying.expression, flying.handsOpen, (int)thrown.expression, thrown.handsOpen);
        }
    }
}
