using System;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The player's progress (RT-4), versioned JSON. Preferences (sound, music, vibration) are not here:
    /// they stay in the kit's own PlayerPrefs keys, so a broken save never resets them and vice versa.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        [Tooltip("Index of the level the next PLAY starts (0-based, within the world).")]
        public int nextLevel;
        [Tooltip("Times the world was cleared (the boss beaten); each loop makes towers tougher.")]
        public int loop;
        public int coins;
        public int startingArmyStep;
        public bool bruteUnlocked;
        [Tooltip("Best stars per level (0-3), indexed like the world's levels.")]
        public int[] stars = new int[0];
        public int levelsWon;
        public int levelsLost;

        public SaveData Clone()
        {
            var copy = (SaveData)MemberwiseClone();
            copy.stars = stars == null ? new int[0] : (int[])stars.Clone();
            return copy;
        }
    }

    /// <summary>Where the save lives and how it is read back (tolerant: a damaged save starts fresh, never crashes).</summary>
    public static class SaveStore
    {
        public const string Key = "Claybreak.Save";

        public static string Serialize(SaveData data)
        {
            return JsonUtility.ToJson(data);
        }

        /// <summary>Parses a save; false (and a fresh save) when it is empty, damaged or from a newer version.</summary>
        public static bool TryDeserialize(string json, out SaveData data)
        {
            data = new SaveData();
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                var parsed = JsonUtility.FromJson<SaveData>(json);
                if (parsed == null || parsed.version < 1 || parsed.version > SaveData.CurrentVersion)
                {
                    return false;
                }

                parsed.stars ??= new int[0];
                parsed.coins = Mathf.Max(0, parsed.coins);
                parsed.nextLevel = Mathf.Max(0, parsed.nextLevel);
                parsed.loop = Mathf.Max(0, parsed.loop);
                parsed.startingArmyStep = Mathf.Max(0, parsed.startingArmyStep);
                data = parsed;
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static SaveData Load()
        {
            string json = PlayerPrefs.GetString(Key, string.Empty);
            if (!TryDeserialize(json, out SaveData data) && !string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[Claybreak] The save could not be read; starting fresh.");
            }

            return data;
        }

        public static void Save(SaveData data)
        {
            PlayerPrefs.SetString(Key, Serialize(data));
            PlayerPrefs.Save();
        }
    }
}
