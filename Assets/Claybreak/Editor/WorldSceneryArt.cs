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
                        default: throw new ArgumentOutOfRangeException(nameof(style), style, "no scenery for this style");
                    }
                }
            }

            switch (style)
            {
                case SceneryStyle.Marches: Ridge(set, trackEnd, m); break;
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
