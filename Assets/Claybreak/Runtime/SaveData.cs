using System;
using UnityEngine;

namespace Claybreak
{
    /// <summary>One world's progress: next level, replay loop and best stars per level.</summary>
    [Serializable]
    public sealed class WorldSave
    {
        public string id = string.Empty;
        [Tooltip("Index of the level the next PLAY starts (0-based, within the world).")]
        public int nextLevel;
        [Tooltip("Times the world was cleared (the boss beaten); each loop makes towers tougher.")]
        public int loop;
        [Tooltip("Best stars per level (0-3), indexed like the world's levels.")]
        public int[] stars = new int[0];

        public WorldSave Clone()
        {
            var copy = (WorldSave)MemberwiseClone();
            copy.stars = stars == null ? new int[0] : (int[])stars.Clone();
            return copy;
        }
    }

    /// <summary>An upgrade track's bought step count, by track id (JsonUtility has no dictionaries).</summary>
    [Serializable]
    public struct UpgradeSave
    {
        public string id;
        public int level;
    }

    /// <summary>
    /// The player's progress, versioned JSON (RT-5, version 2). Content is referenced by stable ids from
    /// <see cref="ClaybreakCatalog"/>, so adding worlds, upgrades, units or skins never breaks a save, and a
    /// removed id is dropped when the save is normalized. Preferences (sound, music, vibration) are not here:
    /// they stay in the kit's own PlayerPrefs keys, so a broken save never resets them and vice versa.
    /// Older versions are read by <see cref="SaveMigration"/>.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 2;

        public int version = CurrentVersion;
        public int coins;
        [Tooltip("Id of the world PLAY starts in (the one picked on the world screen).")]
        public string world = string.Empty;
        public WorldSave[] worlds = new WorldSave[0];
        public UpgradeSave[] upgrades = new UpgradeSave[0];
        [Tooltip("Ids of the special unit types owned.")]
        public string[] units = new string[0];
        [Tooltip("Ids of the special unit types equipped (at most the catalog's loadout slots).")]
        public string[] loadout = new string[0];
        [Tooltip("Ids of the crowd skins owned.")]
        public string[] skins = new string[0];
        [Tooltip("Id of the crowd skin worn.")]
        public string skin = string.Empty;
        public int levelsWon;
        public int levelsLost;

        public SaveData Clone()
        {
            var copy = (SaveData)MemberwiseClone();
            copy.worlds = new WorldSave[worlds == null ? 0 : worlds.Length];
            for (int i = 0; i < copy.worlds.Length; i++)
            {
                copy.worlds[i] = worlds[i]?.Clone();
            }

            copy.upgrades = upgrades == null ? new UpgradeSave[0] : (UpgradeSave[])upgrades.Clone();
            copy.units = units == null ? new string[0] : (string[])units.Clone();
            copy.loadout = loadout == null ? new string[0] : (string[])loadout.Clone();
            copy.skins = skins == null ? new string[0] : (string[])skins.Clone();
            return copy;
        }
    }

    /// <summary>
    /// Version 1 (RT-4 vertical slice), kept only to be read and migrated. Field names are the JSON keys
    /// the slice wrote; never change them.
    /// </summary>
    [Serializable]
    public sealed class SaveDataV1
    {
        public int version = 1;
        public int nextLevel;
        public int loop;
        public int coins;
        public int startingArmyStep;
        public bool bruteUnlocked;
        public int[] stars = new int[0];
        public int levelsWon;
        public int levelsLost;
    }

    /// <summary>
    /// Turns older saves into the current one. Each step is a pure function with fixture tests
    /// (Editor/SaveFixtures), so a slice player keeps coins, stars, the upgrade and the Brute.
    /// </summary>
    public static class SaveMigration
    {
        /// <summary>The ids the slice's content got in the catalog. They are part of the save format.</summary>
        public const string V1WorldId = "world1";
        public const string V1StartingArmyId = "army";
        public const string V1BruteId = "brute";

        public static SaveData FromV1(SaveDataV1 old)
        {
            var save = new SaveData
            {
                coins = old.coins,
                world = V1WorldId,
                worlds = new[]
                {
                    new WorldSave
                    {
                        id = V1WorldId,
                        nextLevel = old.nextLevel,
                        loop = old.loop,
                        stars = old.stars == null ? new int[0] : (int[])old.stars.Clone()
                    }
                },
                upgrades = old.startingArmyStep > 0
                    ? new[] { new UpgradeSave { id = V1StartingArmyId, level = old.startingArmyStep } }
                    : new UpgradeSave[0],
                // A slice player who had the Brute keeps it equipped: it was always on in the slice.
                units = old.bruteUnlocked ? new[] { V1BruteId } : new string[0],
                loadout = old.bruteUnlocked ? new[] { V1BruteId } : new string[0],
                levelsWon = old.levelsWon,
                levelsLost = old.levelsLost
            };
            return save;
        }
    }

    /// <summary>Where the save lives and how it is read back (tolerant: a damaged save starts fresh, never crashes).</summary>
    public static class SaveStore
    {
        public const string Key = "Claybreak.Save";
        /// <summary>The untouched JSON of a migrated save, kept once, so a bad migration can be undone by hand.</summary>
        public const string BackupKeyPrefix = "Claybreak.Save.backup.v";
        /// <summary>A save larger than this is not the game's (the biggest real one is a few KB).</summary>
        public const int MaximumJsonLength = 64 * 1024;
        /// <summary>No list in a real save comes close; longer ones are cut, never trusted.</summary>
        public const int MaximumListLength = 256;

        [Serializable]
        private sealed class VersionProbe
        {
            public int version;
        }

        public static string Serialize(SaveData data)
        {
            return JsonUtility.ToJson(data);
        }

        /// <summary>
        /// Parses a save of any known version; false (and a fresh save) when it is empty, damaged, too large
        /// or from a newer version. <paramref name="fromVersion"/> is the version found (0 when unreadable),
        /// so the caller knows a migration happened. Values are only made safe here (no nulls, no negative
        /// numbers, no oversized lists); the catalog rules are applied by <see cref="Progression"/>.
        /// </summary>
        public static bool TryDeserialize(string json, out SaveData data, out int fromVersion)
        {
            data = new SaveData();
            fromVersion = 0;
            if (string.IsNullOrEmpty(json) || json.Length > MaximumJsonLength)
            {
                return false;
            }

            try
            {
                VersionProbe probe = JsonUtility.FromJson<VersionProbe>(json);
                if (probe == null)
                {
                    return false;
                }

                SaveData parsed;
                switch (probe.version)
                {
                    case 1:
                        SaveDataV1 old = JsonUtility.FromJson<SaveDataV1>(json);
                        parsed = old == null ? null : SaveMigration.FromV1(old);
                        break;
                    case SaveData.CurrentVersion:
                        parsed = JsonUtility.FromJson<SaveData>(json);
                        break;
                    default:
                        return false;
                }

                if (parsed == null)
                {
                    return false;
                }

                fromVersion = probe.version;
                parsed.version = SaveData.CurrentVersion;
                MakeSafe(parsed);
                data = parsed;
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static bool TryDeserialize(string json, out SaveData data)
        {
            return TryDeserialize(json, out data, out _);
        }

        public static SaveData Load()
        {
            string json = PlayerPrefs.GetString(Key, string.Empty);
            if (!TryDeserialize(json, out SaveData data, out int fromVersion))
            {
                if (!string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning("[Claybreak] The save could not be read; starting fresh.");
                }

                return data;
            }

            if (fromVersion != SaveData.CurrentVersion)
            {
                string backupKey = BackupKeyPrefix + fromVersion;
                if (!PlayerPrefs.HasKey(backupKey))
                {
                    PlayerPrefs.SetString(backupKey, json);
                }

                Debug.Log($"[Claybreak] Save migrated from version {fromVersion} to {SaveData.CurrentVersion} (original kept in {backupKey}).");
            }

            return data;
        }

        public static void Save(SaveData data)
        {
            PlayerPrefs.SetString(Key, Serialize(data));
            PlayerPrefs.Save();
        }

        private static void MakeSafe(SaveData save)
        {
            save.coins = Mathf.Max(0, save.coins);
            save.levelsWon = Mathf.Max(0, save.levelsWon);
            save.levelsLost = Mathf.Max(0, save.levelsLost);
            save.world ??= string.Empty;
            save.skin ??= string.Empty;
            save.worlds = Cut(save.worlds ?? new WorldSave[0]);
            for (int i = 0; i < save.worlds.Length; i++)
            {
                WorldSave world = save.worlds[i] ?? new WorldSave();
                world.id ??= string.Empty;
                world.nextLevel = Mathf.Max(0, world.nextLevel);
                world.loop = Mathf.Max(0, world.loop);
                world.stars = Cut(world.stars ?? new int[0]);
                save.worlds[i] = world;
            }

            save.upgrades = Cut(save.upgrades ?? new UpgradeSave[0]);
            save.units = Cut(save.units ?? new string[0]);
            save.loadout = Cut(save.loadout ?? new string[0]);
            save.skins = Cut(save.skins ?? new string[0]);
        }

        private static T[] Cut<T>(T[] items)
        {
            if (items.Length <= MaximumListLength)
            {
                return items;
            }

            var cut = new T[MaximumListLength];
            Array.Copy(items, cut, MaximumListLength);
            return cut;
        }
    }
}
