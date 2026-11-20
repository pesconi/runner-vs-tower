using System;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The materials of one world's look (RT-6): generated from its <see cref="WorldLook"/> colors into
    /// Materials/&lt;worldId&gt;/ and reused on every build.
    /// </summary>
    public sealed class WorldMaterials
    {
        public Material Ground;
        public Material Field;
        public Material Prop;
        public Material PropDark;
        public Material Accent;

        public static WorldMaterials For(string worldId, WorldLook look)
        {
            string folder = worldId;
            return new WorldMaterials
            {
                Ground = ArtPalette.SolidIn(folder, "Ground", look.ground),
                Field = ArtPalette.SolidIn(folder, "Field", look.field),
                Prop = ArtPalette.SolidIn(folder, "Prop", look.prop),
                PropDark = ArtPalette.SolidIn(folder, "PropDark", look.propDark),
                Accent = ArtPalette.UnlitIn(folder, "Accent", look.accent)
            };
        }
    }

    /// <summary>
    /// The set dressing of each <see cref="SceneryStyle"/>, built from stylized primitives in code (free, own
    /// means), outside the playable width: side fields, curbs, props every 9 m, deterministic, and a far
    /// silhouette read through the fog (GDD 9: three planes, no real shadows). One set per world is built into
    /// the scene; <see cref="WorldScenery"/> shows one at a time.
    /// </summary>
    public static class WorldSceneryArt
    {
        public const float PropStep = 9f;

        public static GameObject Build(SceneryStyle style, string name, WorldMaterials m, ArtPalette p, float trackStart, float trackEnd)
        {
            var set = new GameObject(name).transform;
            float width = ClaybreakSceneBuilder.TrackWidth;
            float middle = (trackStart + trackEnd) * 0.5f;
            float length = trackEnd - trackStart;
            Prim.Box(set, "FieldLeft", new Vector3(-width - 6f, -0.15f, middle), new Vector3(width + 4f, 0.2f, length), m.Field);
            Prim.Box(set, "FieldRight", new Vector3(width + 6f, -0.15f, middle), new Vector3(width + 4f, 0.2f, length), m.Field);
            for (int side = -1; side <= 1; side += 2)
            {
                Prim.Box(set, "Curb", new Vector3(side * (width * 0.5f + 0.2f), 0.05f, middle), new Vector3(0.4f, 0.3f, length), m.PropDark);
            }

            int index = 0;
            for (float z = trackStart + 4f; z < trackEnd; z += PropStep, index++)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    float x = side * (width * 0.5f + 2.2f + (index * 37 % 5) * 0.5f);
                    int kind = (index + (side > 0 ? 1 : 0)) % 4;
                    switch (style)
                    {
                        case SceneryStyle.Marches: Marches(set, kind, x, z, side, m, p); break;
                        case SceneryStyle.Mire: Mire(set, kind, x, z, side, index, m, p); break;
                        case SceneryStyle.Foundry: Foundry(set, kind, x, z, side, m, p); break;
                        default: throw new ArgumentOutOfRangeException(nameof(style), style, "no scenery for this style");
                    }
                }
            }

            switch (style)
            {
                case SceneryStyle.Marches: Ridge(set, trackEnd, m); break;
                case SceneryStyle.Mire: Grove(set, trackEnd, m, p); break;
                case SceneryStyle.Foundry: Skyline(set, trackEnd, m); break;
            }

            foreach (Transform prop in set.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.SetStaticEditorFlags(prop.gameObject, StaticEditorFlags.BatchingStatic);
            }

            return set.gameObject;
        }

        // ---------- world 1: the marches (rocks, pillars, dead trees, braziers; a far ridge) ----------

        private static void Marches(Transform set, int kind, float x, float z, int side, WorldMaterials m, ArtPalette p)
        {
            switch (kind)
            {
                case 0:
                    Prim.Ball(set, "Rock", new Vector3(x, 0.4f, z), new Vector3(1.8f, 1.1f, 1.5f), m.PropDark);
                    break;
                case 1:
                    Prim.Box(set, "Pillar", new Vector3(x, 1.4f, z), new Vector3(0.8f, 2.8f, 0.8f), m.Prop);
                    Prim.Box(set, "PillarCap", new Vector3(x, 2.95f, z), new Vector3(1.1f, 0.3f, 1.1f), m.PropDark);
                    break;
                case 2:
                    Prim.Cyl(set, "Trunk", new Vector3(x, 1.2f, z), new Vector3(0.35f, 1.2f, 0.35f), p.Wood);
                    Prim.Box(set, "Branch", new Vector3(x + side * 0.4f, 2.1f, z), new Vector3(0.9f, 0.18f, 0.18f), p.Wood,
                        new Vector3(0f, 0f, side * 35f));
                    break;
                default:
                    Prim.Cyl(set, "Brazier", new Vector3(x, 0.6f, z), new Vector3(0.5f, 0.6f, 0.5f), p.Iron);
                    Prim.Ball(set, "Coals", new Vector3(x, 1.25f, z), new Vector3(0.55f, 0.3f, 0.55f), m.Accent);
                    break;
            }
        }

        // ---------- world 2: the mire (reeds, stilt huts, mossy trees, lanterns, dark pools; a far grove) ----------

        private static void Mire(Transform set, int kind, float x, float z, int side, int index, WorldMaterials m, ArtPalette p)
        {
            if (index % 3 == 0)
            {
                Prim.Cyl(set, "Pool", new Vector3(x + side * 4f, -0.02f, z + 3f), new Vector3(4f, 0.02f, 3f), m.PropDark);
            }

            switch (kind)
            {
                case 0:
                    for (int r = 0; r < 4; r++)
                    {
                        Prim.Cyl(set, "Reed", new Vector3(x + (r - 1.5f) * 0.35f, 0.9f, z + (r % 2) * 0.4f), new Vector3(0.12f, 0.9f + r % 2 * 0.3f, 0.12f),
                            m.Prop, new Vector3((r - 1.5f) * 6f, 0f, (r - 1.5f) * 8f));
                    }

                    break;
                case 1:
                    for (int s = 0; s < 4; s++)
                    {
                        Prim.Cyl(set, "HutStilt", new Vector3(x + (s % 2 - 0.5f) * 1.4f, 0.6f, z + (s / 2 - 0.5f) * 1.2f),
                            new Vector3(0.18f, 0.6f, 0.18f), p.WoodDark);
                    }

                    Prim.Box(set, "Hut", new Vector3(x, 1.75f, z), new Vector3(1.9f, 1.1f, 1.6f), m.Prop);
                    Prim.Box(set, "HutRoof", new Vector3(x, 2.5f, z), new Vector3(1.6f, 1.6f, 1.9f), m.PropDark, new Vector3(0f, 0f, 45f));
                    break;
                case 2:
                    Prim.Cyl(set, "Trunk", new Vector3(x, 1.4f, z), new Vector3(0.45f, 1.4f, 0.45f), p.WoodDark, new Vector3(0f, 0f, side * 6f));
                    Prim.Box(set, "Bough", new Vector3(x - side * 0.5f, 2.5f, z), new Vector3(1.4f, 0.2f, 0.2f), p.WoodDark, new Vector3(0f, 0f, -side * 25f));
                    Prim.Ball(set, "Hanging", new Vector3(x - side * 0.9f, 2f, z), new Vector3(0.35f, 0.9f, 0.35f), m.Prop);
                    Prim.Ball(set, "Hanging", new Vector3(x + side * 0.2f, 2.2f, z + 0.2f), new Vector3(0.3f, 0.7f, 0.3f), m.Prop);
                    break;
                default:
                    Prim.Cyl(set, "Pole", new Vector3(x, 1.5f, z), new Vector3(0.15f, 1.5f, 0.15f), p.Wood);
                    Prim.Box(set, "Arm", new Vector3(x - side * 0.35f, 2.9f, z), new Vector3(0.8f, 0.1f, 0.1f), p.Wood);
                    Prim.Ball(set, "Lamp", new Vector3(x - side * 0.7f, 2.6f, z), new Vector3(0.4f, 0.5f, 0.4f), m.Accent);
                    break;
            }
        }

        private static void Grove(Transform set, float trackEnd, WorldMaterials m, ArtPalette p)
        {
            for (int i = 0; i < 9; i++)
            {
                float x = -48f + i * 12f;
                float h = 10f + (i % 3) * 4f;
                Prim.Cyl(set, "FarTrunk", new Vector3(x, h * 0.5f, trackEnd + 22f + (i % 2) * 6f), new Vector3(1.6f, h * 0.5f, 1.6f), m.PropDark,
                    new Vector3(0f, 0f, (i % 2 == 0 ? 5f : -7f)));
                Prim.Ball(set, "FarCrown", new Vector3(x, h + 2f, trackEnd + 22f + (i % 2) * 6f), new Vector3(9f, 5f, 6f), m.PropDark);
            }
        }

        // ---------- world 3: the foundry (barrels, chimneys, pipe arches, crucibles; a far lit skyline) ----------

        private static void Foundry(Transform set, int kind, float x, float z, int side, WorldMaterials m, ArtPalette p)
        {
            switch (kind)
            {
                case 0:
                    Prim.Cyl(set, "Barrel", new Vector3(x, 0.55f, z), new Vector3(0.8f, 0.55f, 0.8f), m.Prop);
                    Prim.Cyl(set, "Barrel", new Vector3(x + side * 0.9f, 0.55f, z + 0.5f), new Vector3(0.8f, 0.55f, 0.8f), m.PropDark);
                    Prim.Box(set, "Crate", new Vector3(x + side * 0.4f, 0.5f, z - 0.9f), new Vector3(1f, 1f, 1f), m.Prop, new Vector3(0f, 25f, 0f));
                    break;
                case 1:
                    Prim.Cyl(set, "Chimney", new Vector3(x + side * 1f, 2.5f, z), new Vector3(0.9f, 2.5f, 0.9f), m.PropDark);
                    Prim.Cyl(set, "ChimneyRim", new Vector3(x + side * 1f, 5.02f, z), new Vector3(0.75f, 0.04f, 0.75f), m.Accent);
                    break;
                case 2:
                    Prim.Cyl(set, "PipeLeg", new Vector3(x, 1.2f, z - 1f), new Vector3(0.35f, 1.2f, 0.35f), m.Prop);
                    Prim.Cyl(set, "PipeLeg", new Vector3(x, 1.2f, z + 1f), new Vector3(0.35f, 1.2f, 0.35f), m.Prop);
                    Prim.Cyl(set, "PipeTop", new Vector3(x, 2.4f, z), new Vector3(0.35f, 1.15f, 0.35f), m.Prop, new Vector3(90f, 0f, 0f));
                    break;
                default:
                    Prim.Cyl(set, "Crucible", new Vector3(x, 0.45f, z), new Vector3(1.4f, 0.45f, 1.4f), p.Iron);
                    Prim.Cyl(set, "Melt", new Vector3(x, 0.92f, z), new Vector3(1.15f, 0.03f, 1.15f), m.Accent);
                    break;
            }
        }

        private static void Skyline(Transform set, float trackEnd, WorldMaterials m)
        {
            for (int i = 0; i < 8; i++)
            {
                float x = -49f + i * 14f;
                float h = 8f + (i % 3) * 4f;
                float z = trackEnd + 24f + (i % 2) * 5f;
                Prim.Box(set, "FarHall", new Vector3(x, h * 0.5f, z), new Vector3(12f, h, 8f), m.PropDark);
                Prim.Cyl(set, "FarStack", new Vector3(x + 3f, h + 5f, z), new Vector3(1.4f, 5f, 1.4f), m.PropDark);
                Prim.Box(set, "FarWindows", new Vector3(x, h * 0.55f, z - 4.05f), new Vector3(8f, 0.6f, 0.1f), m.Accent);
            }
        }

        private static void Ridge(Transform set, float trackEnd, WorldMaterials m)
        {
            for (int i = 0; i < 7; i++)
            {
                float x = -42f + i * 14f;
                Prim.Box(set, "Ridge", new Vector3(x, 5f + (i % 3) * 3f, trackEnd + 25f), new Vector3(16f, 14f + (i % 3) * 6f, 10f),
                    m.PropDark, new Vector3(0f, 0f, (i % 2 == 0 ? 8f : -6f)));
            }
        }
    }
}
