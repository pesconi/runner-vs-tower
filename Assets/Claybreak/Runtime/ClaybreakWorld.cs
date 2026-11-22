using System;
using System.Collections.Generic;
using JungleDegree.StudioKit.Toon;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// How a world looks (RT-6, RT-6t.6a, RT-6t.8): its toon palette (sky, fog, ground, striped track, the props' colors and
    /// the crowd's default color), how many trees and rocks line the road, and the sun and ambient light. The scenery is
    /// the same instanced set in every world; at run time <see cref="WorldScenery"/> applies the palette, the density and
    /// the light.
    /// </summary>
    [Serializable]
    public struct WorldLook
    {
        /// <summary>Most trees or rocks a world can show (the scene preallocates the largest density of its worlds).</summary>
        public const int MaximumProps = 256;

        [Tooltip("The world's toon palette: sky gradient, fog, ground, track, props, the crowd's default color.")]
        public ToonPalette palette;
        [Tooltip("Trees standing along the road at once (recycled as the camera moves on).")]
        [Range(0, MaximumProps)] public int trees;
        [Tooltip("Rocks standing along the road at once.")]
        [Range(0, MaximumProps)] public int rocks;
        public Color ambient;
        public Color sun;
        [Range(0.2f, 2f)] public float sunIntensity;

        public bool IsSet => sunIntensity > 0f && palette != null && trees >= 0 && rocks >= 0 && trees + rocks > 0
            && trees <= MaximumProps && rocks <= MaximumProps;
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

        public bool TryValidate(int crowdCapacity, int flightCapacity, out string error)
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

                if (!levels[i].TryValidate(crowdCapacity, flightCapacity, out error))
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
                error = "the world has no look (palette, ground and sun).";
                return false;
            }

            string paletteError = look.palette.Validate();
            if (paletteError != null)
            {
                error = $"palette {look.palette.name}: {paletteError}";
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
