using System.Collections.Generic;
using JungleDegree.StudioKit.Toon;
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
        /// <summary>Damage multiplier the test worlds are tuned for (the replay adjustment compares with it).</summary>
        public const float ExpectedDamage = 1.2f;

        private readonly List<Object> made = new List<Object>();

        public TestContent(int worlds = 1, int starsForWorld2 = 0)
        {
            var entries = new WorldEntry[worlds];
            for (int w = 0; w < worlds; w++)
            {
                entries[w] = new WorldEntry { world = World(w == 0 ? SaveMigration.V1WorldId : "world" + (w + 1), w), starsToUnlock = w == 0 ? 0 : starsForWorld2 };
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
                new[]
                {
                    new CrowdShapeOption { id = SaveMigration.ShapeBeforeV4, title = "BALL", mesh = Shape("ball", 12) },
                    new CrowdShapeOption { id = "chick", title = "CHICK", mesh = Shape("chick", 24) }
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

        // A stand-in crowd shape: one submesh of <paramref name="triangles"/> triangles (the gate picks the heaviest).
        private Mesh Shape(string name, int triangles)
        {
            var mesh = new Mesh { name = name };
            made.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.up, Vector3.right };
            var indices = new int[triangles * 3];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i % 3;
            }

            mesh.triangles = indices;
            return mesh;
        }

        /// <summary>The crowd color of test world <paramref name="world"/>'s palette (each world has its own).</summary>
        public static Color PaletteCrowd(int world) => new Color(0.2f + 0.2f * world, 0.9f, 0.4f, 1f);

        private ClaybreakWorld World(string id, int index)
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
                    level.FindProperty("bossRule").enumValueIndex = (int)BossRule.ShiftGates;
                }

                level.ApplyModifiedPropertiesWithoutUndo();
            }

            ClaybreakWorld world = Make<ClaybreakWorld>(id);
            ToonPalette palette = Make<ToonPalette>(id + "-Palette");
            Color[] colors = palette.Colors;
            colors[6] = PaletteCrowd(index);
            palette.EditorAssign(colors, 22f, 75f);
            var look = new WorldLook
            {
                palette = palette, trees = 10, rocks = 5, ambient = Color.gray, sun = Color.white, sunIntensity = 1f
            };
            world.EditorAssign(id, id.ToUpperInvariant(), levels, look, ExpectedDamage, 0.25f, 0.25f);
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
