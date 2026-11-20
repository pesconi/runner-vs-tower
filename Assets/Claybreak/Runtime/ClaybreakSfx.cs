using System;
using JungleDegree.StudioKit.Core;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// This game's own sound cues, on top of the kit's fixed <see cref="GameSound"/> roles (the kit has no
    /// "tower falls" or "cannon fires"). Voices are preallocated AudioSources made by the scene builder;
    /// the kit's mute switch and its vibration switch are respected. Kit candidate (pauta 5.1): a generic
    /// "extra cue" table in ThemeAudio would remove this class.
    /// </summary>
    public sealed class ClaybreakSfx : MonoBehaviour
    {
        public enum Cue
        {
            CannonFire,
            TowerFall,
            FloorBreak,
            WallBreak,
            Sweep,
            Crate,
            Boss
        }

        [Serializable]
        public struct Entry
        {
            public Cue cue;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
            [Min(0f)] public float minimumInterval;
            [Tooltip("Vibrate with the cue, through the kit's haptics entry of this role (and its switch).")]
            public bool vibrate;
            public GameSound hapticRole;
        }

        private static ClaybreakSfx instance;

        [SerializeField] private AudioSource[] voices = new AudioSource[0];
        [SerializeField] private Entry[] entries = new Entry[0];

        private ThemeAudio themeAudio;
        private float[] lastPlay;
        private int nextVoice;

        public bool HasEveryCue
        {
            get
            {
                foreach (Cue cue in Enum.GetValues(typeof(Cue)))
                {
                    if (Find(cue) < 0)
                    {
                        return false;
                    }
                }

                return voices != null && voices.Length > 0;
            }
        }

        private void Awake()
        {
            instance = this;
            lastPlay = new float[Enum.GetValues(typeof(Cue)).Length];
            for (int i = 0; i < lastPlay.Length; i++)
            {
                lastPlay[i] = -10f;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>The theme's audio block: its haptic entries drive the cues that vibrate.</summary>
        public void Configure(ThemeAudio audio)
        {
            themeAudio = audio;
        }

        public static void Play(Cue cue)
        {
            if (instance != null)
            {
                instance.PlayInternal(cue);
            }
        }

        private void PlayInternal(Cue cue)
        {
            int index = Find(cue);
            if (index < 0 || voices.Length == 0)
            {
                return;
            }

            Entry entry = entries[index];
            float now = Time.unscaledTime;
            if (now - lastPlay[(int)cue] < entry.minimumInterval)
            {
                return;
            }

            lastPlay[(int)cue] = now;
            if (entry.vibrate && themeAudio != null)
            {
                GameHaptics.Trigger(themeAudio, entry.hapticRole);
            }

            if (GameAudio.SoundMuted || entry.clip == null)
            {
                return;
            }

            AudioSource voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            voice.pitch = 1f;
            voice.PlayOneShot(entry.clip, entry.volume);
        }

        private int Find(Cue cue)
        {
            if (entries == null)
            {
                return -1;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].cue == cue)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
