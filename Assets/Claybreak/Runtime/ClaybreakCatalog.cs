using System;
using System.Collections.Generic;
using JungleDegree.StudioKit.Swarm;
using UnityEngine;

namespace Claybreak
{
    /// <summary>What an upgrade track improves; the step bonus is read in the unit of its kind.</summary>
    public enum UpgradeKind
    {
        /// <summary>+N units at the start of every run.</summary>
        StartingArmy,
        /// <summary>+N% damage to the tower and the wall.</summary>
        Damage,
        /// <summary>+N on every "+" gate of the run.</summary>
        GateBonus,
        /// <summary>+N% units launched per second in the assault.</summary>
        LaunchRate
    }

    /// <summary>One step of an upgrade track: its price in coins and its total bonus (not the increment).</summary>
    [Serializable]
    public struct UpgradeStep
    {
        [Min(1)] public int cost;
        [Min(0)] public int bonus;
    }

    /// <summary>An upgrade bought with coins in the shop (GDD 6: few and legible, 5-8 steps each).</summary>
    [Serializable]
    public sealed class UpgradeTrack
    {
        [Tooltip("Stable save id; never rename a shipped id.")]
        public string id = "upgrade";
        public string title = "UPGRADE";
        public UpgradeKind kind;
        public UpgradeStep[] steps = new UpgradeStep[0];
    }

    /// <summary>
    /// A special unit type (GDD 6, collection): it comes in crates on the run once it is in the loadout and
    /// goes out every few launches in the assault. Owned by beating a level (unlockWorld/unlockLevel) or
    /// bought in the shop (price), or both: whichever comes first.
    /// </summary>
    [Serializable]
    public sealed class UnitType
    {
        [Tooltip("Stable save id; never rename a shipped id.")]
        public string id = "unit";
        public string title = "UNIT";
        [Tooltip("One short line for the shop card.")]
        public string description = string.Empty;
        public Color color = Color.red;
        [Range(0.6f, 2f)] public float scale = 1f;
        [Tooltip("Damage per hit on the tower and the wall (a plain unit deals 1).")]
        [Min(1)] public int damage = 1;
        [Tooltip("Cannon blasts or sweeps it shrugs off before it falls.")]
        [Min(0)] public int armor;
        [Tooltip("Speed in flight, relative to the level's unit speed.")]
        [Range(0.5f, 2f)] public float speed = 1f;
        [Tooltip("Shop price in coins; 0 = not sold (only unlocked by a level).")]
        [Min(0)] public int price;
        [Tooltip("World id whose level unlocks it for free; empty = no level unlock.")]
        public string unlockWorld = string.Empty;
        [Min(0)] public int unlockLevel;

        public bool UnlocksByLevel => !string.IsNullOrEmpty(unlockWorld);
    }

    /// <summary>A crowd skin: a clay color drawn by code (no texture, no paid art).</summary>
    [Serializable]
    public sealed class CrowdSkin
    {
        [Tooltip("Stable save id; never rename a shipped id.")]
        public string id = "skin";
        public string title = "SKIN";
        public Color color = Color.white;
        [Tooltip("Price in coins; the first skin must be free (the default look).")]
        [Min(0)] public int price;
    }

    /// <summary>
    /// A crowd shape sold in the shop's SHAPES tab (RT-6t.20, D31 A / D32 A; before, D26 A: free on the home screen). The
    /// mesh is one of the generated crowd shapes (one submesh, body tinted by the skin); each shape hops its own way
    /// (<see cref="motion"/>), so it reads from afar in a crowd of 300. The first shape is free and the default for a new
    /// or migrated save.
    /// </summary>
    [Serializable]
    public sealed class CrowdShapeOption
    {
        [Tooltip("Stable save id; never rename a shipped id.")]
        public string id = "shape";
        public string title = "SHAPE";
        public Mesh mesh;
        [Tooltip("Price in coins; the first shape must be free (the default look).")]
        [Min(0)] public int price;
        [Tooltip("How the units of this shape hop (the crowd, the leader, the assault).")]
        public SwarmMotionSettings motion = SwarmMotionSettings.Default;
        [Tooltip("Height of the leader's crown seat (m); 0 = the top of the mesh (a shape with ears seats it on the head).")]
        [Min(0f)] public float crownSeat;
        [Tooltip("Shop figure, body layer: white shading the shop tints with the army's color (generated from the mesh).")]
        public Sprite figureBody;
        [Tooltip("Shop figure, detail layer: the fixed colors and the outline, drawn over the body layer untinted.")]
        public Sprite figureDetail;
    }

    /// <summary>A world in the world list and what opens it.</summary>
    [Serializable]
    public sealed class WorldEntry
    {
        public ClaybreakWorld world;
        [Tooltip("Total stars (all worlds) needed to open it, on top of beating the previous world's boss.")]
        [Min(0)] public int starsToUnlock;
    }

    /// <summary>
    /// Everything the meta (RT-5) offers, as data: the worlds in order, the upgrade tracks, the special
    /// unit types, the crowd skins and the loadout rules. Prices and curves are provisional [P] until the
    /// user approves them (decisoes.md); changing them is a data change, never a code change.
    /// </summary>
    [CreateAssetMenu(menuName = "Claybreak/Catalog")]
    public sealed class ClaybreakCatalog : ScriptableObject
    {
        public const int MaximumLoadoutSlots = 2;

        [SerializeField] private WorldEntry[] worlds = new WorldEntry[0];
        [SerializeField] private UpgradeTrack[] upgrades = new UpgradeTrack[0];
        [SerializeField] private UnitType[] units = new UnitType[0];
        [SerializeField] private CrowdSkin[] skins = new CrowdSkin[0];
        [Tooltip("Crowd shapes sold in the shop (RT-6t.20); the first is free and the default.")]
        [SerializeField] private CrowdShapeOption[] shapes = new CrowdShapeOption[0];
        [Tooltip("Special unit types equipped at once (GDD 6: 1-2).")]
        [SerializeField, Range(1, MaximumLoadoutSlots)] private int loadoutSlots = MaximumLoadoutSlots;
        [Tooltip("A special unit goes out every Nth launch while there are any.")]
        [SerializeField, Range(2, 10)] private int specialEvery = 3;
        [Tooltip("Line under the world list (e.g. more worlds coming); empty hides it.")]
        [SerializeField] private string worldsFooter = "MORE WORLDS SOON";

        public IReadOnlyList<WorldEntry> Worlds => worlds;
        public IReadOnlyList<UpgradeTrack> Upgrades => upgrades;
        public IReadOnlyList<UnitType> Units => units;
        public IReadOnlyList<CrowdSkin> Skins => skins;
        public IReadOnlyList<CrowdShapeOption> Shapes => shapes;
        public int LoadoutSlots => loadoutSlots;
        public int SpecialEvery => specialEvery;
        public string WorldsFooter => worldsFooter;

        public int WorldIndexOf(string id) => IndexOf(worlds, id, entry => entry.world == null ? null : entry.world.WorldId);
        public int UpgradeIndexOf(string id) => IndexOf(upgrades, id, track => track.id);
        public int UnitIndexOf(string id) => IndexOf(units, id, unit => unit.id);
        public int SkinIndexOf(string id) => IndexOf(skins, id, skin => skin.id);
        public int ShapeIndexOf(string id) => IndexOf(shapes, id, shape => shape.id);

        /// <summary>Total levels over every world (the most stars the player can hold is 3x this).</summary>
        public int LevelCount
        {
            get
            {
                int total = 0;
                foreach (WorldEntry entry in worlds)
                {
                    total += entry.world == null ? 0 : entry.world.LevelCount;
                }

                return total;
            }
        }

#if UNITY_EDITOR
        /// <summary>Editor only: the content writer and the tests fill the catalog from code.</summary>
        public void EditorAssign(WorldEntry[] worldEntries, UpgradeTrack[] upgradeTracks, UnitType[] unitTypes,
            CrowdSkin[] crowdSkins, CrowdShapeOption[] crowdShapes, int slots, int every)
        {
            shapes = crowdShapes;
            worlds = worldEntries;
            upgrades = upgradeTracks;
            units = unitTypes;
            skins = crowdSkins;
            loadoutSlots = slots;
            specialEvery = every;
        }
#endif

        /// <summary>Checks the whole catalog against the crowd cap and the assault pool the game runs on. Used by the validator and at start.</summary>
        public bool TryValidate(int crowdCapacity, int flightCapacity, out string error)
        {
            if (worlds == null || worlds.Length == 0)
            {
                error = "the catalog needs at least one world.";
                return false;
            }

            var ids = new HashSet<string>();
            int starsBefore = 0;
            for (int i = 0; i < worlds.Length; i++)
            {
                WorldEntry entry = worlds[i];
                if (entry == null || entry.world == null)
                {
                    error = $"worlds[{i}] is missing.";
                    return false;
                }

                if (!entry.world.TryValidate(crowdCapacity, flightCapacity, out error))
                {
                    error = $"{entry.world.name}: {error}";
                    return false;
                }

                if (!IsId(entry.world.WorldId) || !ids.Add("world:" + entry.world.WorldId))
                {
                    error = $"worlds[{i}] needs a unique id (lowercase letters, digits, '-').";
                    return false;
                }

                if ((i == 0 && entry.starsToUnlock != 0) || entry.starsToUnlock > starsBefore)
                {
                    error = $"worlds[{i}] asks for {entry.starsToUnlock} stars, but only {starsBefore} exist before it (the first world is open).";
                    return false;
                }

                starsBefore += entry.world.LevelCount * 3;
            }

            if (upgrades == null || upgrades.Length == 0)
            {
                error = "the catalog needs at least one upgrade track.";
                return false;
            }

            for (int i = 0; i < upgrades.Length; i++)
            {
                UpgradeTrack track = upgrades[i];
                if (track == null || !IsId(track.id) || !ids.Add("upgrade:" + track.id) || string.IsNullOrEmpty(track.title))
                {
                    error = $"upgrades[{i}] needs a unique id and a title.";
                    return false;
                }

                if (track.steps == null || track.steps.Length < 5 || track.steps.Length > 8)
                {
                    error = $"upgrade '{track.id}' needs 5-8 steps (GDD 6).";
                    return false;
                }

                for (int s = 0; s < track.steps.Length; s++)
                {
                    bool rises = s == 0 ? track.steps[0].bonus > 0
                        : track.steps[s].cost >= track.steps[s - 1].cost && track.steps[s].bonus > track.steps[s - 1].bonus;
                    if (track.steps[s].cost < 1 || !rises)
                    {
                        error = $"upgrade '{track.id}': every step costs at least as much as the one before and gives more.";
                        return false;
                    }
                }
            }

            if (units == null || units.Length == 0)
            {
                error = "the catalog needs at least one special unit type.";
                return false;
            }

            for (int i = 0; i < units.Length; i++)
            {
                UnitType unit = units[i];
                if (unit == null || !IsId(unit.id) || !ids.Add("unit:" + unit.id) || string.IsNullOrEmpty(unit.title))
                {
                    error = $"units[{i}] needs a unique id and a title.";
                    return false;
                }

                if (unit.price == 0 && !unit.UnlocksByLevel)
                {
                    error = $"unit '{unit.id}' can never be had: give it a price or a level that unlocks it.";
                    return false;
                }

                if (unit.UnlocksByLevel)
                {
                    int world = WorldIndexOf(unit.unlockWorld);
                    if (world < 0 || unit.unlockLevel >= worlds[world].world.LevelCount - 1)
                    {
                        error = $"unit '{unit.id}' unlocks at an unknown world or at/after its boss.";
                        return false;
                    }
                }
            }

            if (skins == null || skins.Length == 0 || skins[0] == null || skins[0].price != 0)
            {
                error = "the catalog needs skins, and the first one is the free default look.";
                return false;
            }

            for (int i = 0; i < skins.Length; i++)
            {
                if (skins[i] == null || !IsId(skins[i].id) || !ids.Add("skin:" + skins[i].id) || string.IsNullOrEmpty(skins[i].title))
                {
                    error = $"skins[{i}] needs a unique id and a title.";
                    return false;
                }
            }

            if (shapes == null || shapes.Length == 0 || shapes[0] == null || shapes[0].price != 0)
            {
                error = "the catalog needs crowd shapes, and the first one is the free default.";
                return false;
            }

            for (int i = 0; i < shapes.Length; i++)
            {
                CrowdShapeOption shape = shapes[i];
                if (shape == null || !IsId(shape.id) || !ids.Add("shape:" + shape.id) || string.IsNullOrEmpty(shape.title))
                {
                    error = $"shapes[{i}] needs a unique id and a title.";
                    return false;
                }

                if (shape.mesh == null || shape.mesh.subMeshCount != 1)
                {
                    error = $"shape '{shape.id}' needs one of the generated crowd shapes (one submesh).";
                    return false;
                }

                // Price 0 = free and owned by every player (the default first; since save version 6 the Bolinha too, the old
                // default, 2026-11-25). A negative price would pay the player for it ([Min(0)] guards the inspector only).
                if (shape.price < 0)
                {
                    error = $"shape '{shape.id}' needs a price of 0 (free for everyone) or more.";
                    return false;
                }

                SwarmMotionSettings hop = shape.motion;
                if (!(hop.Frequency > 0f) || !(hop.HopHeight >= 0f) || !(hop.SquashBase > 0f) || !(hop.SquashAmount >= 0f))
                {
                    error = $"shape '{shape.id}' needs a hop with a positive speed and squash.";
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <summary>Save ids: short, lowercase, stable (a typo in data can never collide with another entry).</summary>
        public static bool IsId(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length > 32)
            {
                return false;
            }

            foreach (char c in id)
            {
                if (!(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9') && c != '-')
                {
                    return false;
                }
            }

            return true;
        }

        private static int IndexOf<T>(T[] items, string id, Func<T, string> idOf)
        {
            if (items == null || string.IsNullOrEmpty(id))
            {
                return -1;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null && idOf(items[i]) == id)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
