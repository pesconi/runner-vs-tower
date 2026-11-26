using System;
using JungleDegree.StudioKit.Core;

namespace Claybreak
{
    /// <summary>
    /// This game's own sound cues, on top of the kit's fixed <see cref="GameSound"/> roles (the kit has no
    /// "tower falls" or "cannon fires"). RT-8: each cue is an entry of the kit's extra-sound table in the
    /// theme (<see cref="ThemeAudio.ExtraSounds"/>, id = the cue name), so the kit's voices, mute switch,
    /// pauses and vibration switch apply; this class only maps the cue to its index, once.
    /// </summary>
    public static class ClaybreakSfx
    {
        public enum Cue
        {
            CannonFire,
            TowerFall,
            FloorBreak,
            WallBreak,
            Sweep,
            Crate,
            Boss,
            ProjectileWarning,
            StoneImpact,
            TowerHit,
            KeyWind,
            NumberPop
        }

        private static readonly int[] Indices = CreateIndices();

        /// <summary>Theme id of a cue (the clip of cue X is "CueX" and its extra-sound id is "X").</summary>
        public static string IdOf(Cue cue)
        {
            return cue.ToString();
        }

        /// <summary>True when the theme has an extra sound for every cue.</summary>
        public static bool Covers(ThemeAudio audio, out string missing)
        {
            foreach (Cue cue in Enum.GetValues(typeof(Cue)))
            {
                if (audio == null || audio.FindExtraSound(IdOf(cue)) < 0)
                {
                    missing = IdOf(cue);
                    return false;
                }
            }

            missing = null;
            return true;
        }

        public static void Configure(ThemeAudio audio)
        {
            foreach (Cue cue in Enum.GetValues(typeof(Cue)))
            {
                Indices[(int)cue] = audio != null ? audio.FindExtraSound(IdOf(cue)) : -1;
            }
        }

        public static void Play(Cue cue)
        {
            GameAudio.PlayExtra(Indices[(int)cue]);
        }

        private static int[] CreateIndices()
        {
            var indices = new int[Enum.GetValues(typeof(Cue)).Length];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = -1;
            }

            return indices;
        }
    }
}
