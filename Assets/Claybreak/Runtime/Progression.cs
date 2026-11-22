using System.Collections.Generic;
using UnityEngine;

namespace Claybreak
{
    /// <summary>What a finished level gave the player (shown on the result screen).</summary>
    public readonly struct LevelOutcome
    {
        public LevelOutcome(bool won, int stars, int coins, UnitType unlockedUnit, bool clearedWorld, ClaybreakWorld openedWorld)
        {
            Won = won;
            Stars = stars;
            Coins = coins;
            UnlockedUnit = unlockedUnit;
            ClearedWorld = clearedWorld;
            OpenedWorld = openedWorld;
        }

        public bool Won { get; }
        public int Stars { get; }
        public int Coins { get; }
        /// <summary>The special unit type this win unlocked (null if none).</summary>
        public UnitType UnlockedUnit { get; }
        public bool ClearedWorld { get; }
        /// <summary>The world this win opened, now picked for the next PLAY (null if none).</summary>
        public ClaybreakWorld OpenedWorld { get; }
    }

    /// <summary>The upgrades' effect on one run, read when a level is prepared.</summary>
    public readonly struct RunLoadout
    {
        public RunLoadout(int startingArmy, float damageMultiplier, int gateBonus, float launchRateMultiplier)
        {
            StartingArmy = startingArmy;
            DamageMultiplier = damageMultiplier;
            GateBonus = gateBonus;
            LaunchRateMultiplier = launchRateMultiplier;
        }

        public int StartingArmy { get; }
        public float DamageMultiplier { get; }
        public int GateBonus { get; }
        public float LaunchRateMultiplier { get; }
        public static RunLoadout None => new RunLoadout(0, 1f, 0, 1f);

        /// <summary>
        /// Attack power for the difficulty adjustment: the damage multiplier, plus half of the launch-rate bonus
        /// (a faster stream loses fewer units to the cannon but does not hit harder).
        /// </summary>
        public float Power => DamageMultiplier * (1f + 0.5f * (LaunchRateMultiplier - 1f));
    }

    /// <summary>
    /// The meta rules (RT-5), pure (no scene, no PlayerPrefs) so they are unit-tested: worlds and which are
    /// open, the next level, stars and coins for a result, the upgrade tracks, the special unit collection and
    /// loadout, the crowd skins. Everything it sells or unlocks comes from <see cref="ClaybreakCatalog"/>; the
    /// save only holds ids and counts and is normalized against the catalog here (unknown ids dropped, numbers
    /// clamped), so a hand-edited or older save is never trusted.
    /// </summary>
    public sealed class Progression
    {
        /// <summary>Coin ceiling: a hand-edited save can never overflow the sums below.</summary>
        public const int MaximumCoins = 99_999_999;
        public const int MaximumLoop = 1000;
        /// <summary>Ceiling of the win and loss counters (a hand-edited save can never wrap them negative).</summary>
        public const int MaximumCount = 1_000_000_000;

        private readonly ClaybreakCatalog catalog;
        private int worldIndex;

        public Progression(ClaybreakCatalog catalog, SaveData save)
        {
            this.catalog = catalog;
            Save = save ?? new SaveData();
            Normalize();
        }

        public SaveData Save { get; }
        public ClaybreakCatalog Catalog => catalog;

        // ---------- worlds and levels ----------

        public int WorldIndex => worldIndex;
        public ClaybreakWorld World => catalog.Worlds[worldIndex].world;
        public WorldSave WorldProgress => Save.worlds[worldIndex];
        public int LevelIndex => WorldProgress.nextLevel;
        public ClaybreakLevel CurrentLevel => World.Levels[LevelIndex];
        /// <summary>
        /// Tower and wall health multiplier of the current world: +HealthPerLoop per loop and, on replays only,
        /// the player's power above what the world was tuned for (D19, RT-6): (power / expected) to the world's
        /// exponent, never below 1. A first pass is never adjusted: buying upgrades always shows.
        /// </summary>
        public float HealthMultiplier => (1f + World.HealthPerLoop * WorldProgress.loop) * PowerAdjustment;

        /// <summary>The replay adjustment alone (1 on a first pass or when the player is not above the world's tuning).</summary>
        public float PowerAdjustment
        {
            get
            {
                if (WorldProgress.loop <= 0)
                {
                    return 1f;
                }

                float ratio = Loadout.Power / World.ExpectedDamage;
                return ratio <= 1f ? 1f : Mathf.Pow(ratio, World.LoopPowerExponent);
            }
        }
        public float CoinMultiplier => 1f + World.CoinsPerLoop * WorldProgress.loop;
        public int StarsAt(int level) => StarsAt(worldIndex, level);

        public int StarsAt(int world, int level)
        {
            int[] stars = Save.worlds[world].stars;
            return level >= 0 && level < stars.Length ? stars[level] : 0;
        }

        public int WorldStars(int world)
        {
            int total = 0;
            foreach (int stars in Save.worlds[world].stars)
            {
                total += stars;
            }

            return total;
        }

        public int MaximumWorldStars(int world) => catalog.Worlds[world].world.LevelCount * 3;

        public int TotalStars
        {
            get
            {
                int total = 0;
                for (int i = 0; i < Save.worlds.Length; i++)
                {
                    total += WorldStars(i);
                }

                return total;
            }
        }

        public bool IsWorldCleared(int world) => Save.worlds[world].loop > 0;

        /// <summary>The first world is open; each other one needs the previous boss beaten and enough stars.</summary>
        public bool IsWorldUnlocked(int world)
        {
            return world == 0
                || (world > 0 && world < catalog.Worlds.Count && IsWorldCleared(world - 1)
                    && TotalStars >= catalog.Worlds[world].starsToUnlock);
        }

        public bool TrySelectWorld(int world)
        {
            if (world < 0 || world >= catalog.Worlds.Count || !IsWorldUnlocked(world))
            {
                return false;
            }

            worldIndex = world;
            Save.world = World.WorldId;
            return true;
        }

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

        /// <summary>Records the result of the current level and moves the world forward on a win.</summary>
        public LevelOutcome Complete(bool won, int armyAtLine, int armyLeft)
        {
            WorldSave progress = WorldProgress;
            int index = progress.nextLevel;
            int reward = Mathf.RoundToInt(CurrentLevel.CoinReward * CoinMultiplier);
            int stars = StarsFor(won, armyAtLine, armyLeft);
            int coins = CoinsFor(won, reward, Mathf.Max(0, armyLeft), stars);
            AddCoins(coins);
            if (!won)
            {
                Save.levelsLost = Mathf.Min(Save.levelsLost + 1, MaximumCount);
                return new LevelOutcome(false, 0, coins, null, false, null);
            }

            Save.levelsWon = Mathf.Min(Save.levelsWon + 1, MaximumCount);
            progress.stars[index] = Mathf.Max(progress.stars[index], stars);
            UnitType unlocked = UnlockUnitsAt(worldIndex, index);
            bool cleared = index == World.LevelCount - 1;
            ClaybreakWorld opened = null;
            if (cleared)
            {
                bool nextWasOpen = IsWorldUnlocked(worldIndex + 1);
                progress.loop = Mathf.Min(progress.loop + 1, MaximumLoop);
                progress.nextLevel = 0;
                if (!nextWasOpen && TrySelectWorld(worldIndex + 1))
                {
                    opened = World;
                }
            }
            else
            {
                progress.nextLevel++;
            }

            return new LevelOutcome(true, stars, coins, unlocked, cleared, opened);
        }

        /// <summary>Scales a level health (tower, wall) by the replay loop.</summary>
        public int ScaleHealth(int health)
        {
            return Mathf.Max(1, Mathf.RoundToInt(health * HealthMultiplier));
        }

        // ---------- upgrades ----------

        public int UpgradeLevel(int track) => FindUpgrade(catalog.Upgrades[track].id);
        public bool IsUpgradeMaxed(int track) => UpgradeLevel(track) >= catalog.Upgrades[track].steps.Length;

        /// <summary>The track's total bonus after <paramref name="level"/> steps bought (0 = none).</summary>
        public int UpgradeValue(int track, int level) => level <= 0 ? 0 : catalog.Upgrades[track].steps[level - 1].bonus;
        public int UpgradeValue(int track) => UpgradeValue(track, UpgradeLevel(track));
        public int NextUpgradeCost(int track) => IsUpgradeMaxed(track) ? 0 : catalog.Upgrades[track].steps[UpgradeLevel(track)].cost;
        public bool CanBuyUpgrade(int track) => !IsUpgradeMaxed(track) && Save.coins >= NextUpgradeCost(track);

        public bool TryBuyUpgrade(int track)
        {
            if (track < 0 || track >= catalog.Upgrades.Count || !CanBuyUpgrade(track))
            {
                return false;
            }

            Save.coins -= NextUpgradeCost(track);
            SetUpgrade(catalog.Upgrades[track].id, UpgradeLevel(track) + 1);
            return true;
        }

        /// <summary>The summed bonus of every track of <paramref name="kind"/>.</summary>
        public int Bonus(UpgradeKind kind)
        {
            int total = 0;
            for (int i = 0; i < catalog.Upgrades.Count; i++)
            {
                if (catalog.Upgrades[i].kind == kind)
                {
                    total += UpgradeValue(i);
                }
            }

            return total;
        }

        public RunLoadout Loadout => new RunLoadout(Bonus(UpgradeKind.StartingArmy), 1f + Bonus(UpgradeKind.Damage) / 100f,
            Bonus(UpgradeKind.GateBonus), 1f + Bonus(UpgradeKind.LaunchRate) / 100f);

        // ---------- special units ----------

        public bool IsUnitOwned(int unit) => Contains(Save.units, catalog.Units[unit].id);
        public bool IsUnitEquipped(int unit) => Contains(Save.loadout, catalog.Units[unit].id);
        public bool CanBuyUnit(int unit) => !IsUnitOwned(unit) && catalog.Units[unit].price > 0 && Save.coins >= catalog.Units[unit].price;
        public int EquippedCount => Save.loadout.Length;

        public bool TryBuyUnit(int unit)
        {
            if (unit < 0 || unit >= catalog.Units.Count || !CanBuyUnit(unit))
            {
                return false;
            }

            Save.coins -= catalog.Units[unit].price;
            Save.units = Append(Save.units, catalog.Units[unit].id);
            if (Save.loadout.Length < catalog.LoadoutSlots)
            {
                Save.loadout = Append(Save.loadout, catalog.Units[unit].id);
            }

            return true;
        }

        /// <summary>Equips an owned unit in a free slot, or takes it off. False when it is not owned or no slot is free.</summary>
        public bool TryToggleUnit(int unit)
        {
            if (unit < 0 || unit >= catalog.Units.Count || !IsUnitOwned(unit))
            {
                return false;
            }

            string id = catalog.Units[unit].id;
            if (IsUnitEquipped(unit))
            {
                Save.loadout = Remove(Save.loadout, id);
                return true;
            }

            if (Save.loadout.Length >= catalog.LoadoutSlots)
            {
                return false;
            }

            Save.loadout = Append(Save.loadout, id);
            return true;
        }

        /// <summary>Writes the equipped unit types into <paramref name="slots"/> (in loadout order); returns how many.</summary>
        public int FillLoadout(UnitType[] slots)
        {
            int count = 0;
            for (int i = 0; i < Save.loadout.Length && count < slots.Length; i++)
            {
                int unit = catalog.UnitIndexOf(Save.loadout[i]);
                if (unit >= 0)
                {
                    slots[count++] = catalog.Units[unit];
                }
            }

            for (int i = count; i < slots.Length; i++)
            {
                slots[i] = null;
            }

            return count;
        }

        /// <summary>The level that unlocks a unit for free, as "WORLD 1 · LEVEL 3" (null when it only sells).</summary>
        public string UnlockHint(int unit)
        {
            UnitType type = catalog.Units[unit];
            int world = catalog.WorldIndexOf(type.unlockWorld);
            if (!type.UnlocksByLevel || world < 0)
            {
                return null;
            }

            ClaybreakWorld unlockWorld = catalog.Worlds[world].world;
            return $"{unlockWorld.WorldName} · {unlockWorld.Levels[type.unlockLevel].Title}";
        }

        // ---------- skins ----------

        public bool IsSkinOwned(int skin) => Contains(Save.skins, catalog.Skins[skin].id);
        public bool IsSkinWorn(int skin) => Save.skin == catalog.Skins[skin].id;
        public bool CanBuySkin(int skin) => !IsSkinOwned(skin) && Save.coins >= catalog.Skins[skin].price;
        public CrowdSkin Skin => catalog.Skins[Mathf.Max(0, catalog.SkinIndexOf(Save.skin))];

        /// <summary>True when the player wears the default look (the catalog's first skin, free; an unknown id falls back to it).</summary>
        public bool WearsDefaultSkin => catalog.SkinIndexOf(Save.skin) <= 0;

        /// <summary>
        /// The army's body color in the world at catalog index <paramref name="world"/> (RT-6t.6a): the default skin wears
        /// the world's palette color, any other skin its own color in every world. The save keeps the skin id as it is.
        /// </summary>
        public Color CrowdColorIn(int world) => WearsDefaultSkin ? WorldCrowdColor(world) : Skin.color;

        /// <summary>
        /// The color the default skin wears in the world at catalog index <paramref name="world"/>: its palette's crowd color
        /// (the default skin's own color when the world or its palette is missing). The shop shows it on the default skin's row.
        /// </summary>
        public Color WorldCrowdColor(int world)
        {
            if (world >= 0 && world < catalog.Worlds.Count)
            {
                ClaybreakWorld entry = catalog.Worlds[world].world;
                if (entry != null && entry.Look.palette != null)
                {
                    return entry.Look.palette.Crowd;
                }
            }

            return catalog.Skins[0].color;
        }

        public bool TryBuySkin(int skin)
        {
            if (skin < 0 || skin >= catalog.Skins.Count || !CanBuySkin(skin))
            {
                return false;
            }

            Save.coins -= catalog.Skins[skin].price;
            Save.skins = Append(Save.skins, catalog.Skins[skin].id);
            Save.skin = catalog.Skins[skin].id;
            return true;
        }

        public bool TryWearSkin(int skin)
        {
            if (skin < 0 || skin >= catalog.Skins.Count || !IsSkinOwned(skin))
            {
                return false;
            }

            Save.skin = catalog.Skins[skin].id;
            return true;
        }

        // ---------- crowd shape (RT-6t.6b, D26 A: free, picked on the home screen) ----------

        public int ShapeIndex => Mathf.Max(0, catalog.ShapeIndexOf(Save.shape));
        public CrowdShapeOption Shape => catalog.Shapes[ShapeIndex];

        /// <summary>Picks the crowd shape at catalog index <paramref name="shape"/>; false when it is out of range or already picked.</summary>
        public bool TrySelectShape(int shape)
        {
            if (shape < 0 || shape >= catalog.Shapes.Count || shape == ShapeIndex)
            {
                return false;
            }

            Save.shape = catalog.Shapes[shape].id;
            return true;
        }

        /// <summary>Picks the next crowd shape of the catalog (the home screen's single button walks through them).</summary>
        public bool TrySelectNextShape() => TrySelectShape((ShapeIndex + 1) % catalog.Shapes.Count);

        /// <summary>True when the shop has something the player can buy now (the home screen shows a dot).</summary>
        public bool CanBuyAnything
        {
            get
            {
                for (int i = 0; i < catalog.Upgrades.Count; i++)
                {
                    if (CanBuyUpgrade(i))
                    {
                        return true;
                    }
                }

                for (int i = 0; i < catalog.Units.Count; i++)
                {
                    if (CanBuyUnit(i))
                    {
                        return true;
                    }
                }

                for (int i = 0; i < catalog.Skins.Count; i++)
                {
                    if (CanBuySkin(i))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // ---------- device gate ----------

        /// <summary>
        /// The maximum loadout (RT-5 device gate): every upgrade at its top step, every unit owned, the loadout
        /// full, the last (dearest) skin worn and the heaviest crowd shape. Never written to the player's save.
        /// </summary>
        public static SaveData MaximumLoadout(ClaybreakCatalog catalog)
        {
            var save = new SaveData();
            var upgrades = new UpgradeSave[catalog.Upgrades.Count];
            for (int i = 0; i < upgrades.Length; i++)
            {
                upgrades[i] = new UpgradeSave { id = catalog.Upgrades[i].id, level = catalog.Upgrades[i].steps.Length };
            }

            save.upgrades = upgrades;
            var units = new string[catalog.Units.Count];
            for (int i = 0; i < units.Length; i++)
            {
                units[i] = catalog.Units[i].id;
            }

            save.units = units;
            save.loadout = new string[Mathf.Min(catalog.LoadoutSlots, units.Length)];
            System.Array.Copy(units, save.loadout, save.loadout.Length);
            var skins = new string[catalog.Skins.Count];
            for (int i = 0; i < skins.Length; i++)
            {
                skins[i] = catalog.Skins[i].id;
            }

            save.skins = skins;
            save.skin = skins[skins.Length - 1];
            save.shape = catalog.Shapes[HeaviestShape(catalog)].id;
            return save;
        }

        // The device gate draws the crowd with the shape of most triangles (the run's worst case).
        private static int HeaviestShape(ClaybreakCatalog catalog)
        {
            int heaviest = 0;
            for (int i = 1; i < catalog.Shapes.Count; i++)
            {
                if (catalog.Shapes[i].mesh.GetIndexCount(0) > catalog.Shapes[heaviest].mesh.GetIndexCount(0))
                {
                    heaviest = i;
                }
            }

            return heaviest;
        }

        // ---------- normalization ----------

        // A save from another catalog (content added or removed), an older version or a hand edit is fitted to
        // the catalog: one entry per world in catalog order, unknown ids dropped, numbers clamped.
        private void Normalize()
        {
            Save.version = SaveData.CurrentVersion;
            Save.coins = Mathf.Clamp(Save.coins, 0, MaximumCoins);
            Save.levelsWon = Mathf.Clamp(Save.levelsWon, 0, MaximumCount);
            Save.levelsLost = Mathf.Clamp(Save.levelsLost, 0, MaximumCount);
            Save.firstDay = Mathf.Max(0, Save.firstDay);
            Save.dailyLastDay = Mathf.Max(0, Save.dailyLastDay);
            Save.dailyClaims = Mathf.Clamp(Save.dailyClaims, 0, MaximumCount);

            var worlds = new WorldSave[catalog.Worlds.Count];
            for (int w = 0; w < worlds.Length; w++)
            {
                ClaybreakWorld world = catalog.Worlds[w].world;
                WorldSave found = Find(Save.worlds, world.WorldId) ?? new WorldSave { id = world.WorldId };
                int count = world.LevelCount;
                var stars = new int[count];
                for (int i = 0; i < count; i++)
                {
                    stars[i] = found.stars != null && i < found.stars.Length ? Mathf.Clamp(found.stars[i], 0, 3) : 0;
                }

                worlds[w] = new WorldSave
                {
                    id = world.WorldId,
                    nextLevel = Mathf.Clamp(found.nextLevel, 0, count - 1),
                    loop = Mathf.Clamp(found.loop, 0, MaximumLoop),
                    stars = stars
                };
            }

            Save.worlds = worlds;

            var upgrades = new UpgradeSave[catalog.Upgrades.Count];
            for (int i = 0; i < upgrades.Length; i++)
            {
                UpgradeTrack track = catalog.Upgrades[i];
                upgrades[i] = new UpgradeSave { id = track.id, level = Mathf.Clamp(FindUpgradeIn(Save.upgrades, track.id), 0, track.steps.Length) };
            }

            Save.upgrades = upgrades;

            Save.units = Known(Save.units, id => catalog.UnitIndexOf(id) >= 0, int.MaxValue);
            for (int w = 0; w < worlds.Length; w++)
            {
                // Repair: a unit whose level is already beaten is owned (a lost unlock never sticks).
                for (int level = 0; level < catalog.Worlds[w].world.LevelCount; level++)
                {
                    if (StarsAt(w, level) > 0 || worlds[w].loop > 0)
                    {
                        OwnUnitsUnlockedAt(w, level);
                    }
                }
            }

            Save.loadout = Known(Save.loadout, id => Contains(Save.units, id), catalog.LoadoutSlots);
            Save.skins = Known(Save.skins, id => catalog.SkinIndexOf(id) >= 0, int.MaxValue);
            string free = catalog.Skins[0].id;
            if (!Contains(Save.skins, free))
            {
                Save.skins = Prepend(Save.skins, free);
            }

            if (!Contains(Save.skins, Save.skin))
            {
                Save.skin = free;
            }

            if (catalog.ShapeIndexOf(Save.shape) < 0)
            {
                Save.shape = catalog.Shapes[0].id;
            }

            int selected = catalog.WorldIndexOf(Save.world);
            worldIndex = selected >= 0 && IsWorldUnlocked(selected) ? selected : 0;
            Save.world = World.WorldId;
        }

        private UnitType UnlockUnitsAt(int world, int level)
        {
            UnitType first = null;
            for (int i = 0; i < catalog.Units.Count; i++)
            {
                if (UnlocksAt(catalog.Units[i], world, level) && !IsUnitOwned(i))
                {
                    Save.units = Append(Save.units, catalog.Units[i].id);
                    if (Save.loadout.Length < catalog.LoadoutSlots)
                    {
                        Save.loadout = Append(Save.loadout, catalog.Units[i].id);
                    }

                    first ??= catalog.Units[i];
                }
            }

            return first;
        }

        private void OwnUnitsUnlockedAt(int world, int level)
        {
            for (int i = 0; i < catalog.Units.Count; i++)
            {
                if (UnlocksAt(catalog.Units[i], world, level) && !Contains(Save.units, catalog.Units[i].id))
                {
                    Save.units = Append(Save.units, catalog.Units[i].id);
                }
            }
        }

        private bool UnlocksAt(UnitType unit, int world, int level)
        {
            return unit.UnlocksByLevel && unit.unlockLevel == level && catalog.WorldIndexOf(unit.unlockWorld) == world;
        }

        // ---------- rewards outside a level (RT-7) ----------

        /// <summary>Coins from a rewarded ad (the level's coins once more); clamped like every other gain.</summary>
        public void GrantBonusCoins(int coins)
        {
            AddCoins(coins);
        }

        /// <summary>Records the first launch day once (D1/D7); later calls keep the first one.</summary>
        public void RecordFirstDay(int today)
        {
            if (Save.firstDay <= 0 && today > 0)
            {
                Save.firstDay = today;
            }
        }

        public bool CanClaimDaily(int today) => DailyReward.CanClaim(Save, today);

        /// <summary>Pays today's daily reward from <paramref name="cycle"/>; false (0 coins) when already claimed today.</summary>
        public bool TryClaimDaily(int today, int[] cycle, out int coins)
        {
            coins = 0;
            if (!CanClaimDaily(today))
            {
                return false;
            }

            coins = DailyReward.NextAmount(Save, cycle);
            AddCoins(coins);
            Save.dailyLastDay = today;
            Save.dailyClaims = Mathf.Min(Save.dailyClaims + 1, MaximumCount);
            return true;
        }

        private void AddCoins(int coins)
        {
            Save.coins = (int)System.Math.Min((long)Save.coins + Mathf.Max(0, coins), MaximumCoins);
        }

        private int FindUpgrade(string id) => FindUpgradeIn(Save.upgrades, id);

        private static int FindUpgradeIn(UpgradeSave[] upgrades, string id)
        {
            foreach (UpgradeSave upgrade in upgrades)
            {
                if (upgrade.id == id)
                {
                    return upgrade.level;
                }
            }

            return 0;
        }

        private void SetUpgrade(string id, int level)
        {
            for (int i = 0; i < Save.upgrades.Length; i++)
            {
                if (Save.upgrades[i].id == id)
                {
                    Save.upgrades[i].level = level;
                    return;
                }
            }
        }

        private static WorldSave Find(WorldSave[] worlds, string id)
        {
            foreach (WorldSave world in worlds)
            {
                if (world != null && world.id == id)
                {
                    return world;
                }
            }

            return null;
        }

        private static bool Contains(string[] ids, string id)
        {
            foreach (string item in ids)
            {
                if (item == id)
                {
                    return true;
                }
            }

            return false;
        }

        private static string[] Known(string[] ids, System.Func<string, bool> keep, int maximum)
        {
            var kept = new List<string>();
            foreach (string id in ids)
            {
                if (kept.Count < maximum && !string.IsNullOrEmpty(id) && keep(id) && !kept.Contains(id))
                {
                    kept.Add(id);
                }
            }

            return kept.ToArray();
        }

        private static string[] Append(string[] ids, string id)
        {
            var grown = new string[ids.Length + 1];
            ids.CopyTo(grown, 0);
            grown[ids.Length] = id;
            return grown;
        }

        private static string[] Prepend(string[] ids, string id)
        {
            var grown = new string[ids.Length + 1];
            grown[0] = id;
            ids.CopyTo(grown, 1);
            return grown;
        }

        private static string[] Remove(string[] ids, string id)
        {
            var kept = new List<string>(ids);
            kept.Remove(id);
            return kept.ToArray();
        }
    }
}
