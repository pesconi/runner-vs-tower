using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>
    /// In-memory content for the meta tests: worlds of plain levels (the last one a boss), and a catalog
    /// with the slice's ids (world1, army, brute) so the version-1 migration has somewhere to land.
    /// Everything it makes is destroyed by <see cref="Dispose"/>.
    /// </summary>
    public sealed class TestContent : System.IDisposable
    {
        public const int LevelsPerWorld = 6;
        private readonly List<Object> made = new List<Object>();

        public TestContent(int worlds = 1, int starsForWorld2 = 0)
        {
            var entries = new WorldEntry[worlds];
            for (int w = 0; w < worlds; w++)
            {
                entries[w] = new WorldEntry { world = World(w == 0 ? SaveMigration.V1WorldId : "world" + (w + 1)), starsToUnlock = w == 0 ? 0 : starsForWorld2 };
            }

            Catalog = Make<ClaybreakCatalog>("Catalog");
            Catalog.EditorAssign(entries,
                new[]
                {
                    Track(SaveMigration.V1StartingArmyId, UpgradeKind.StartingArmy, (60, 5), (120, 10), (200, 16), (320, 24), (480, 34)),
                    Track("damage", UpgradeKind.Damage, (80, 15), (150, 30), (250, 45), (400, 60), (600, 80)),
                    Track("gates", UpgradeKind.GateBonus, (100, 1), (180, 2), (300, 3), (450, 5), (650, 7)),
                    Track("launch", UpgradeKind.LaunchRate, (70, 10), (130, 20), (220, 30), (350, 45), (520, 60))
                },
                new[]
                {
                    new UnitType { id = SaveMigration.V1BruteId, title = "BRUTE", damage = 4, armor = 1, unlockWorld = SaveMigration.V1WorldId, unlockLevel = 2 },
                    new UnitType { id = "sprinter", title = "SPRINTER", damage = 2, speed = 1.6f, price = 600 },
                    new UnitType { id = "warden", title = "WARDEN", damage = 2, armor = 3, price = 900, unlockWorld = SaveMigration.V1WorldId, unlockLevel = 4 }
                },
                new[]
                {
                    new CrowdSkin { id = "clay", title = "CLAY", price = 0 },
                    new CrowdSkin { id = "ember", title = "EMBER", price = 150 },
                    new CrowdSkin { id = "ink", title = "INK", price = 1200 }
                },
                2, 3);
        }

        public ClaybreakCatalog Catalog { get; }

        public void Dispose()
        {
            foreach (Object item in made)
            {
                Object.DestroyImmediate(item);
            }
        }

        private ClaybreakWorld World(string id)
        {
            var levels = new ClaybreakLevel[LevelsPerWorld];
            for (int i = 0; i < LevelsPerWorld; i++)
            {
                levels[i] = Make<ClaybreakLevel>($"{id}-Level{i + 1}");
                var level = new SerializedObject(levels[i]);
                level.FindProperty("coinReward").intValue = 40;
                if (i == LevelsPerWorld - 1)
                {
                    level.FindProperty("tower").enumValueIndex = (int)TowerKind.Boss;
                    level.FindProperty("floors").intValue = 4;
                    level.FindProperty("shiftGatesOnFloor").boolValue = true;
                }

                level.ApplyModifiedPropertiesWithoutUndo();
            }

            ClaybreakWorld world = Make<ClaybreakWorld>(id);
            var so = new SerializedObject(world);
            so.FindProperty("worldId").stringValue = id;
            so.FindProperty("worldName").stringValue = id.ToUpperInvariant();
            SerializedProperty array = so.FindProperty("levels");
            array.arraySize = LevelsPerWorld;
            for (int i = 0; i < LevelsPerWorld; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return world;
        }

        private static UpgradeTrack Track(string id, UpgradeKind kind, params (int cost, int bonus)[] steps)
        {
            var track = new UpgradeTrack { id = id, title = id.ToUpperInvariant(), kind = kind, steps = new UpgradeStep[steps.Length] };
            for (int i = 0; i < steps.Length; i++)
            {
                track.steps[i] = new UpgradeStep { cost = steps[i].cost, bonus = steps[i].bonus };
            }

            return track;
        }

        private T Make<T>(string name) where T : ScriptableObject
        {
            T item = ScriptableObject.CreateInstance<T>();
            item.name = name;
            made.Add(item);
            return item;
        }
    }
}
