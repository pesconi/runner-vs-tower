using System;
using System.Collections.Generic;
using UnityEngine;

namespace Claybreak
{
    /// <summary>One step of an upgrade track: its effect and its price in coins.</summary>
    [Serializable]
    public struct UpgradeStep
    {
        [Min(1)] public int cost;
        [Min(0)] public int bonus;
    }

    /// <summary>
    /// A world (RT-4 vertical slice): its ordered levels (the last one is the boss), the replay rule once
    /// the world is cleared, the starting-army upgrade track and the special unit. Data only; the rules
    /// that read it live in <see cref="Progression"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Claybreak/World")]
    public sealed class ClaybreakWorld : ScriptableObject
    {
        [SerializeField] private string worldName = "WORLD 1";
        [SerializeField] private ClaybreakLevel[] levels = new ClaybreakLevel[0];

        [Header("Replay after the boss (the world loops, harder)")]
        [Tooltip("Tower and wall health multiplier added per completed loop of the world.")]
        [SerializeField, Range(0f, 1f)] private float healthPerLoop = 0.25f;

        [Header("Upgrade: starting army (+N units at the start of every run)")]
        [SerializeField] private UpgradeStep[] startingArmySteps = new UpgradeStep[0];

        [Header("Special unit: the Brute")]
        [Tooltip("Level index (0-based) whose first win unlocks the Brute.")]
        [SerializeField, Min(0)] private int bruteUnlockLevel = 2;
        [Tooltip("Damage a Brute deals to the tower and to the wall.")]
        [SerializeField, Min(1)] private int bruteDamage = 4;
        [Tooltip("Cannon blasts a Brute shrugs off before it falls.")]
        [SerializeField, Min(0)] private int bruteArmor = 1;
        [SerializeField] private Color bruteColor = new Color(0.55f, 0.16f, 0.12f);
        [SerializeField, Range(1f, 2f)] private float bruteScale = 1.45f;

        public string WorldName => worldName;
        public IReadOnlyList<ClaybreakLevel> Levels => levels;
        public int LevelCount => levels == null ? 0 : levels.Length;
        public float HealthPerLoop => healthPerLoop;
        public IReadOnlyList<UpgradeStep> StartingArmySteps => startingArmySteps;
        public int BruteUnlockLevel => bruteUnlockLevel;
        public int BruteDamage => bruteDamage;
        public int BruteArmor => bruteArmor;
        public Color BruteColor => bruteColor;
        public float BruteScale => bruteScale;

        public bool TryValidate(int poolCapacity, out string error)
        {
            if (levels == null || levels.Length < 5 || levels.Length > 8)
            {
                error = "a world needs 5-8 levels (RT-4).";
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

            if (startingArmySteps == null || startingArmySteps.Length < 5 || startingArmySteps.Length > 8)
            {
                error = "the upgrade needs 5-8 steps (GDD 6).";
                return false;
            }

            for (int i = 1; i < startingArmySteps.Length; i++)
            {
                if (startingArmySteps[i].cost < startingArmySteps[i - 1].cost
                    || startingArmySteps[i].bonus <= startingArmySteps[i - 1].bonus)
                {
                    error = "upgrade steps must cost at least as much as the one before and give more.";
                    return false;
                }
            }

            if (bruteUnlockLevel >= levels.Length - 1)
            {
                error = "the Brute must unlock before the boss.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
