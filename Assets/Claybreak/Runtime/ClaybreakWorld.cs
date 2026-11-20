using System;
using System.Collections.Generic;
using UnityEngine;

namespace Claybreak
{
    /// <summary>The set dressing a world is built with (props, far silhouettes); stable values, new ones at the end.</summary>
    public enum SceneryStyle
    {
        /// <summary>World 1: rocks, pillars, dead trees and braziers under a dusk ridge.</summary>
        Marches
    }

    /// <summary>
    /// How a world looks (RT-6): the scenery it is dressed with and its palette. The scene builder bakes the
    /// scenery and the ground colors into one preallocated set per world; at run time
    /// <see cref="WorldScenery"/> shows the world's set and applies the sky, ambient and sun colors.
    /// </summary>
    [Serializable]
    public struct WorldLook
    {
        public SceneryStyle scenery;
        [Tooltip("Fog and camera background: the color of the horizon.")]
        public Color sky;
        public Color ambient;
        public Color sun;
        [Range(0.2f, 2f)] public float sunIntensity;
        [Tooltip("The track the army runs on.")]
        public Color ground;
        [Tooltip("The fields on both sides of the track.")]
        public Color field;
        [Tooltip("Main prop color (rocks, trunks, pillars).")]
        public Color prop;
        [Tooltip("Second prop color (caps, far ridge).")]
        public Color propDark;
        [Tooltip("Light sources of the scenery (braziers, lamps, crystals).")]
        public Color accent;

        public bool IsSet => sunIntensity > 0f && sky.a > 0f && ground.a > 0f;
    }

    /// <summary>
    /// A world: its stable id, its ordered levels (the last one is the boss), its look, and the replay rule
    /// once the world is cleared. Data only; the meta that spans worlds (upgrades, units, skins) lives in
    /// <see cref="ClaybreakCatalog"/> and the rules that read both in <see cref="Progression"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Claybreak/World")]
    public sealed class ClaybreakWorld : ScriptableObject
    {
        public const int MinimumLevels = 5;
        public const int MaximumLevels = 10;

        [Tooltip("Stable save id; never rename a shipped id.")]
        [SerializeField] private string worldId = "world1";
        [SerializeField] private string worldName = "WORLD 1";
        [SerializeField] private ClaybreakLevel[] levels = new ClaybreakLevel[0];
        [SerializeField] private WorldLook look;

        [Header("Difficulty profile")]
        [Tooltip("Damage multiplier (upgrades) the world's towers are tuned for.")]
        [SerializeField, Min(1f)] private float expectedDamage = 1f;
        [Tooltip("On replays, towers also grow with the player's power above what the world expects, by (power / expected) to this exponent (D19: the top upgrades must not make an old world trivial).")]
        [SerializeField, Range(0f, 1f)] private float loopPowerExponent = 0.7f;

        [Header("Replay after the boss (the world loops, harder)")]
        [Tooltip("Tower and wall health multiplier added per completed loop of the world.")]
        [SerializeField, Range(0f, 1f)] private float healthPerLoop = 0.25f;
        [Tooltip("Coin reward multiplier added per completed loop (keeps coins per minute in step with the harder towers).")]
        [SerializeField, Range(0f, 1f)] private float coinsPerLoop = 0.25f;

        public string WorldId => worldId;
        public string WorldName => worldName;
        public IReadOnlyList<ClaybreakLevel> Levels => levels;
        public int LevelCount => levels == null ? 0 : levels.Length;
        public float HealthPerLoop => healthPerLoop;
        public float CoinsPerLoop => coinsPerLoop;
        public WorldLook Look => look;
        public float ExpectedDamage => expectedDamage;
        public float LoopPowerExponent => loopPowerExponent;

        public bool TryValidate(int poolCapacity, out string error)
        {
            if (levels == null || levels.Length < MinimumLevels || levels.Length > MaximumLevels)
            {
                error = $"a world needs {MinimumLevels}-{MaximumLevels} levels.";
                return false;
            }

            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i] == null)
                {
                    error = $"levels[{i}] is missing.";
                    return false;
                }

                if (levels[i].IsDeviceGateLevel)
                {
                    error = $"levels[{i}] ({levels[i].name}) is a device-gate level.";
                    return false;
                }

                if (!levels[i].TryValidate(poolCapacity, out error))
                {
                    error = $"{levels[i].name}: {error}";
                    return false;
                }

                bool last = i == levels.Length - 1;
                if (levels[i].IsBoss != last)
                {
                    error = "exactly one boss, and it is the last level of the world (GDD 4.4).";
                    return false;
                }
            }

            if (!look.IsSet)
            {
                error = "the world has no look (sky, ground and sun).";
                return false;
            }

            error = null;
            return true;
        }

#if UNITY_EDITOR
        /// <summary>Editor only (content and tests).</summary>
        public void EditorAssign(string id, string displayName, ClaybreakLevel[] worldLevels, WorldLook worldLook,
            float expected, float healthLoop, float coinsLoop, float powerExponent = 0.7f)
        {
            worldId = id;
            worldName = displayName;
            levels = (ClaybreakLevel[])worldLevels.Clone();
            look = worldLook;
            expectedDamage = Mathf.Max(1f, expected);
            healthPerLoop = healthLoop;
            coinsPerLoop = coinsLoop;
            loopPowerExponent = powerExponent;
        }
#endif
    }
}
