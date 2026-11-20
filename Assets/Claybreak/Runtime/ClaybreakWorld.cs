using System.Collections.Generic;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// A world: its stable id, its ordered levels (the last one is the boss) and the replay rule once the
    /// world is cleared. Data only; the meta that spans worlds (upgrades, units, skins) lives in
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

            error = null;
            return true;
        }
    }
}
