using System;
using System.Collections.Generic;
using System.IO;
using JungleDegree.StudioKit.Core;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// RT-8: the game's final audio, as data. The clips are WAV files made by the studio
    /// (`Tools/audio/clay_siege_audio.py`, own work, listed in docs/audio-licenses.md) and committed under
    /// <see cref="Folder"/>; this class sets their import settings and writes the theme's audio block:
    /// one clip per kit role, one kit extra sound per game cue (<see cref="ClaybreakSfx.Cue"/>), the two
    /// music loops and the vibration table (reviewed in RT-8: feedback on the big moments, never on the
    /// per-unit events that come in 300-unit bursts).
    /// </summary>
    public static class ClaybreakAudio
    {
        public const string Folder = ClaybreakSceneBuilder.Root + "/Audio";
        public const string LicensesPath = "docs/audio-licenses.md";
        public const int VoiceLimit = 10;
        public const float MusicVolume = 0.35f;

        private struct Mix
        {
            public float volume;
            public float pitch;
            public float interval;
            public int vibrationMs;
            public int vibrationAmplitude;
            public float vibrationInterval;

            public Mix(float volume, float pitch, float interval, int vibrationMs = 0, int vibrationAmplitude = 0,
                float vibrationInterval = 0f)
            {
                this.volume = volume;
                this.pitch = pitch;
                this.interval = interval;
                this.vibrationMs = vibrationMs;
                this.vibrationAmplitude = vibrationAmplitude;
                this.vibrationInterval = vibrationInterval;
            }
        }

        // Volumes are relative: every clip is written peak-normalized (-1 dBFS) by the generator.
        private static readonly Dictionary<GameSound, Mix> Roles = new Dictionary<GameSound, Mix>
        {
            { GameSound.Coin, new Mix(0.5f, 0.05f, 0.06f) },
            { GameSound.GateGood, new Mix(0.7f, 0.03f, 0.05f, 12, 70, 0.1f) },
            { GameSound.GateBad, new Mix(0.65f, 0.03f, 0.05f, 40, 170, 0.3f) },
            { GameSound.UnitLoss, new Mix(0.35f, 0.12f, 0.08f) },
            { GameSound.Finish, new Mix(0.8f, 0f, 0.5f) },
            { GameSound.Victory, new Mix(0.85f, 0f, 0.5f) },
            { GameSound.Defeat, new Mix(0.8f, 0f, 0.5f, 80, 160, 0.5f) },
            { GameSound.ButtonClick, new Mix(0.55f, 0.04f, 0.05f) },
            { GameSound.ButtonBack, new Mix(0.55f, 0.04f, 0.05f) },
            { GameSound.AdReward, new Mix(0.75f, 0f, 0.3f, 25, 120, 0.3f) },
        };

        private static readonly Dictionary<ClaybreakSfx.Cue, Mix> Cues = new Dictionary<ClaybreakSfx.Cue, Mix>
        {
            { ClaybreakSfx.Cue.CannonFire, new Mix(0.6f, 0.08f, 0.12f) },
            { ClaybreakSfx.Cue.ProjectileWarning, new Mix(0.3f, 0.06f, 0.35f) },
            { ClaybreakSfx.Cue.StoneImpact, new Mix(0.55f, 0.1f, 0.1f) },
            { ClaybreakSfx.Cue.TowerHit, new Mix(0.45f, 0.12f, 0.06f) },
            { ClaybreakSfx.Cue.TowerFall, new Mix(0.95f, 0f, 1f, 160, 255, 1f) },
            { ClaybreakSfx.Cue.FloorBreak, new Mix(0.8f, 0.05f, 0.2f, 50, 200, 0.3f) },
            { ClaybreakSfx.Cue.WallBreak, new Mix(0.75f, 0.06f, 0.15f, 35, 180, 0.3f) },
            { ClaybreakSfx.Cue.Sweep, new Mix(0.45f, 0.1f, 0.15f) },
            { ClaybreakSfx.Cue.Crate, new Mix(0.7f, 0.03f, 0.1f, 15, 100, 0.2f) },
            { ClaybreakSfx.Cue.Boss, new Mix(0.85f, 0f, 1f, 90, 220, 1f) },
        };

        public static string RoleClip(GameSound sound) => sound.ToString();
        public static string CueClip(ClaybreakSfx.Cue cue) => "Cue" + cue;
        public const string MenuMusicClip = "MenuLoop";
        public const string RunMusicClip = "RunLoop";

        /// <summary>Every clip name the game uses (the generator writes exactly these).</summary>
        public static IEnumerable<string> ClipNames()
        {
            foreach (GameSound sound in Enum.GetValues(typeof(GameSound)))
            {
                yield return RoleClip(sound);
            }

            foreach (ClaybreakSfx.Cue cue in Enum.GetValues(typeof(ClaybreakSfx.Cue)))
            {
                yield return CueClip(cue);
            }

            yield return MenuMusicClip;
            yield return RunMusicClip;
        }

        /// <summary>Writes the theme's audio block (the theme's SerializedProperty "audio").</summary>
        public static void Apply(SerializedProperty audio)
        {
            audio.FindPropertyRelative("enabled").boolValue = true;
            audio.FindPropertyRelative("voiceLimit").intValue = VoiceLimit;
            Array roles = Enum.GetValues(typeof(GameSound));
            SerializedProperty sounds = audio.FindPropertyRelative("sounds");
            sounds.arraySize = roles.Length;
            var haptics = new List<(GameSound, Mix)>();
            for (int i = 0; i < roles.Length; i++)
            {
                var role = (GameSound)roles.GetValue(i);
                Mix mix = Roles[role];
                SerializedProperty entry = sounds.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("sound").enumValueIndex = (int)role;
                entry.FindPropertyRelative("clip").objectReferenceValue = Load(RoleClip(role), false);
                entry.FindPropertyRelative("volume").floatValue = mix.volume;
                entry.FindPropertyRelative("pitchVariation").floatValue = mix.pitch;
                entry.FindPropertyRelative("minimumInterval").floatValue = mix.interval;
                if (mix.vibrationMs > 0)
                {
                    haptics.Add((role, mix));
                }
            }

            SerializedProperty hapticTable = audio.FindPropertyRelative("haptics");
            hapticTable.arraySize = haptics.Count;
            for (int i = 0; i < haptics.Count; i++)
            {
                SerializedProperty entry = hapticTable.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("sound").enumValueIndex = (int)haptics[i].Item1;
                entry.FindPropertyRelative("milliseconds").intValue = haptics[i].Item2.vibrationMs;
                entry.FindPropertyRelative("amplitude").intValue = haptics[i].Item2.vibrationAmplitude;
                entry.FindPropertyRelative("minimumInterval").floatValue = haptics[i].Item2.vibrationInterval;
            }

            Array cues = Enum.GetValues(typeof(ClaybreakSfx.Cue));
            SerializedProperty extras = audio.FindPropertyRelative("extraSounds");
            extras.arraySize = cues.Length;
            for (int i = 0; i < cues.Length; i++)
            {
                var cue = (ClaybreakSfx.Cue)cues.GetValue(i);
                Mix mix = Cues[cue];
                SerializedProperty entry = extras.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("id").stringValue = ClaybreakSfx.IdOf(cue);
                entry.FindPropertyRelative("clip").objectReferenceValue = Load(CueClip(cue), false);
                entry.FindPropertyRelative("volume").floatValue = mix.volume;
                entry.FindPropertyRelative("pitchVariation").floatValue = mix.pitch;
                entry.FindPropertyRelative("minimumInterval").floatValue = mix.interval;
                entry.FindPropertyRelative("vibrate").boolValue = mix.vibrationMs > 0;
                entry.FindPropertyRelative("vibrationMilliseconds").intValue = Mathf.Max(5, mix.vibrationMs);
                entry.FindPropertyRelative("vibrationAmplitude").intValue = Mathf.Max(1, mix.vibrationAmplitude);
                entry.FindPropertyRelative("vibrationInterval").floatValue = mix.vibrationInterval;
            }

            audio.FindPropertyRelative("menuMusic").objectReferenceValue = Load(MenuMusicClip, true);
            audio.FindPropertyRelative("runMusic").objectReferenceValue = Load(RunMusicClip, true);
            audio.FindPropertyRelative("musicVolume").floatValue = MusicVolume;
        }

        /// <summary>
        /// Every clip exists with the release import settings and is listed in docs/audio-licenses.md (and no
        /// audio file in the folder is unlisted or unused).
        /// </summary>
        public static bool TryValidateLicensed(out string error)
        {
            string licenses = File.Exists(LicensesPath) ? File.ReadAllText(LicensesPath) : null;
            if (licenses == null)
            {
                error = LicensesPath + " is missing";
                return false;
            }

            var expected = new HashSet<string>();
            foreach (string name in ClipNames())
            {
                expected.Add(name);
                string path = $"{Folder}/{name}.wav";
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null)
                {
                    error = $"{path} is missing (run Tools/audio/clay_siege_audio.py)";
                    return false;
                }

                bool music = name == MenuMusicClip || name == RunMusicClip;
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                if (!importer.forceToMono || settings.compressionFormat != AudioCompressionFormat.Vorbis
                    || settings.loadType != (music ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad))
                {
                    error = $"{path} does not have the release import settings (rebuild the scene)";
                    return false;
                }

                if (!licenses.Contains($"`{name}.wav`"))
                {
                    error = $"{name}.wav is not listed in {LicensesPath}";
                    return false;
                }
            }

            foreach (string file in Directory.GetFiles(Folder))
            {
                string extension = Path.GetExtension(file).ToLowerInvariant();
                if (extension != ".meta" && !expected.Contains(Path.GetFileNameWithoutExtension(file)))
                {
                    error = $"{file} is not used by the game (remove it or add it to the theme and the licenses)";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private static AudioClip Load(string name, bool music)
        {
            string path = $"{Folder}/{name}.wav";
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"{path} is missing: run python Tools/audio/clay_siege_audio.py");
            }

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            AudioClipLoadType loadType = music ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            float quality = music ? 0.45f : 0.7f;
            if (settings.loadType != loadType || settings.compressionFormat != AudioCompressionFormat.Vorbis
                || !Mathf.Approximately(settings.quality, quality) || !importer.forceToMono || importer.loadInBackground != music)
            {
                settings.loadType = loadType;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = quality;
                settings.preloadAudioData = true;
                importer.defaultSampleSettings = settings;
                importer.forceToMono = true;
                importer.loadInBackground = music;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
