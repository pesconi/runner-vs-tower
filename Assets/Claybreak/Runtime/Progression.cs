using UnityEngine;

namespace Claybreak
{
    /// <summary>What a finished level gave the player (shown on the result screen).</summary>
    public readonly struct LevelOutcome
    {
        public LevelOutcome(bool won, int stars, int coins, bool unlockedBrute, bool clearedWorld)
        {
            Won = won;
            Stars = stars;
            Coins = coins;
            UnlockedBrute = unlockedBrute;
            ClearedWorld = clearedWorld;
        }

        public bool Won { get; }
        public int Stars { get; }
        public int Coins { get; }
        public bool UnlockedBrute { get; }
        public bool ClearedWorld { get; }
    }

    /// <summary>
    /// The meta rules of the slice, pure (no scene, no PlayerPrefs) so they are unit-tested: which level
    /// comes next, stars and coins for a result, the starting-army upgrade and the Brute unlock.
    /// </summary>
    public sealed class Progression
    {
        private readonly ClaybreakWorld world;

        public Progression(ClaybreakWorld world, SaveData save)
        {
            this.world = world;
            Save = save ?? new SaveData();
            Normalize();
        }

        public SaveData Save { get; }
        public int LevelIndex => Save.nextLevel;
        public ClaybreakLevel CurrentLevel => world.Levels[Save.nextLevel];
        public float HealthMultiplier => 1f + world.HealthPerLoop * Save.loop;
        public int StartingArmyBonus => Save.startingArmyStep == 0 ? 0 : world.StartingArmySteps[Save.startingArmyStep - 1].bonus;
        public bool UpgradeMaxed => Save.startingArmyStep >= world.StartingArmySteps.Count;
        public int NextUpgradeCost => UpgradeMaxed ? 0 : world.StartingArmySteps[Save.startingArmyStep].cost;
        public int NextUpgradeBonus => UpgradeMaxed ? StartingArmyBonus : world.StartingArmySteps[Save.startingArmyStep].bonus;
        public bool CanBuyUpgrade => !UpgradeMaxed && Save.coins >= NextUpgradeCost;
        public int StarsAt(int level) => level < Save.stars.Length ? Save.stars[level] : 0;

        /// <summary>Stars for a win (GDD 4.5): by the share of the army at the firing line that survived.</summary>
        public static int StarsFor(bool won, int armyAtLine, int armyLeft)
        {
            if (!won)
            {
                return 0;
            }

            float share = armyAtLine <= 0 ? 0f : (float)armyLeft / armyAtLine;
            return share >= 0.5f ? 3 : share >= 0.2f ? 2 : 1;
        }

        /// <summary>Coins for a result: a win pays the level reward plus one coin per survivor; a loss pays a little.</summary>
        public static int CoinsFor(bool won, int reward, int armyLeft, int stars)
        {
            if (!won)
            {
                return reward / 8;
            }

            return reward + armyLeft + stars * reward / 4;
        }

        public bool TryBuyUpgrade()
        {
            if (!CanBuyUpgrade)
            {
                return false;
            }

            Save.coins -= NextUpgradeCost;
            Save.startingArmyStep++;
            return true;
        }

        /// <summary>Records the result of the current level and moves the world forward on a win.</summary>
        public LevelOutcome Complete(bool won, int armyAtLine, int armyLeft)
        {
            int index = Save.nextLevel;
            ClaybreakLevel level = CurrentLevel;
            int stars = StarsFor(won, armyAtLine, armyLeft);
            int coins = CoinsFor(won, level.CoinReward, armyLeft, stars);
            Save.coins += coins;
            if (!won)
            {
                Save.levelsLost++;
                return new LevelOutcome(false, 0, coins, false, false);
            }

            Save.levelsWon++;
            Save.stars[index] = Mathf.Max(Save.stars[index], stars);
            bool unlocked = !Save.bruteUnlocked && index >= world.BruteUnlockLevel;
            Save.bruteUnlocked |= unlocked;
            bool cleared = index == world.LevelCount - 1;
            if (cleared)
            {
                Save.loop++;
                Save.nextLevel = 0;
            }
            else
            {
                Save.nextLevel++;
            }

            return new LevelOutcome(true, stars, coins, unlocked, cleared);
        }

        /// <summary>Scales a level health (tower, wall) by the replay loop.</summary>
        public int ScaleHealth(int health)
        {
            return Mathf.Max(1, Mathf.RoundToInt(health * HealthMultiplier));
        }

        // A save written for a different world size (or edited by hand) is clamped, never trusted.
        private void Normalize()
        {
            int count = world.LevelCount;
            if (Save.stars == null || Save.stars.Length != count)
            {
                var stars = new int[count];
                if (Save.stars != null)
                {
                    System.Array.Copy(Save.stars, stars, Mathf.Min(count, Save.stars.Length));
                }

                Save.stars = stars;
            }

            for (int i = 0; i < count; i++)
            {
                Save.stars[i] = Mathf.Clamp(Save.stars[i], 0, 3);
            }

            Save.nextLevel = Mathf.Clamp(Save.nextLevel, 0, count - 1);
            Save.startingArmyStep = Mathf.Clamp(Save.startingArmyStep, 0, world.StartingArmySteps.Count);
            Save.coins = Mathf.Max(0, Save.coins);
            Save.loop = Mathf.Max(0, Save.loop);
        }
    }
}
