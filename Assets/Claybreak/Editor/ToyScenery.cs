using JungleDegree.StudioKit.Editor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// RT-6v (direction v2, D24 = A): world 1's set dressing, a sunny meadow in three planes, as ONE mesh and one
    /// material (vertex colors) plus one mesh of soft contact shadows: the track stands as a cream plateau with
    /// coral curbs over mint grass; near the track white and yellow flowers and pebbles, in the middle round
    /// "lollipop" trees, far away green hills and white clouds, all read through a light fog. Deterministic (fixed
    /// seed), built in the Editor and saved as mesh assets; nothing is created at run time.
    /// </summary>
    public static class ToyScenery
    {
        public const float GrassTop = -0.6f;
        private const int Seed = 2411;

        public static GameObject Build(string name, float trackStart, float trackEnd)
        {
            var set = new GameObject(name).transform;
            var random = new System.Random(Seed);
            var props = new SoftMeshBuilder();
            var shadows = new SoftMeshBuilder();
            float half = ClaybreakSceneBuilder.TrackWidth * 0.5f;
            float length = trackEnd - trackStart;
            float middle = (trackStart + trackEnd) * 0.5f;

            // The plateau under the track (the track itself is the game's collider box, top at y = 0), its coral
            // curbs and the grass around it, which runs on past the tower.
            props.RoundedBox(new Vector3(0f, -0.52f, middle), new Vector3(2f * half + 0.5f, 0.92f, length + 0.5f), 0.25f,
                ToyArt.Paint(ToyPalette.TrackSide, 0.25f), 3);
            for (int side = -1; side <= 1; side += 2)
            {
                props.RoundedBox(new Vector3(side * (half + 0.2f), 0.06f, middle), new Vector3(0.42f, 0.34f, length), 0.16f,
                    ToyArt.Paint(ToyPalette.Curb, 0.8f), 3);
                props.RoundedBox(new Vector3(side * (half + 40f), GrassTop - 0.2f, middle + 40f), new Vector3(80f, 0.4f, length + 200f), 0.2f,
                    ToyArt.Paint(ToyPalette.Grass, 0.15f), 2);
            }

            props.RoundedBox(new Vector3(0f, GrassTop - 0.2f, trackEnd + 60f), new Vector3(2f * half + 2f, 0.4f, 120f), 0.2f,
                ToyArt.Paint(ToyPalette.Grass, 0.15f), 2);

            // Near plane: flowers and pebbles along both curbs.
            for (float z = trackStart + 1f; z < trackEnd + 30f; z += 1.6f)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    float x = side * (half + 0.9f + Next(random, 0f, 3.5f));
                    float at = z + Next(random, -0.6f, 0.6f);
                    if (random.NextDouble() < 0.72)
                    {
                        Flower(props, new Vector3(x, GrassTop, at), random);
                    }
                    else
                    {
                        float size = Next(random, 0.18f, 0.36f);
                        props.Ellipsoid(new Vector3(x, GrassTop + size * 0.35f, at), new Vector3(size, size * 0.6f, size * 0.8f),
                            ToyArt.Paint(ToyPalette.Pebble, 0.7f), 6, 4);
                    }
                }
            }

            // Middle plane: round trees, mostly green and teal, an orange one now and then.
            int tree = 0;
            for (float z = trackStart + 2f; z < trackEnd + 40f; z += 4.2f, tree++)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    float x = side * (half + 5f + Next(random, 0f, 9f));
                    float at = z + Next(random, -1.5f, 1.5f);
                    float size = Next(random, 0.8f, 1.35f);
                    Color leaves = (tree * 7 + side + 11) % 9 == 0 ? ToyPalette.Leaves[2] : ToyPalette.Leaves[(tree + (side > 0 ? 1 : 0)) % 2];
                    var foot = new Vector3(x, GrassTop, at);
                    props.Drum(foot, 0.16f * size, 1.4f * size, 0.06f, ToyArt.Paint(ToyPalette.Trunk, 0.5f), 8, 0.12f * size, 1);
                    props.Ellipsoid(foot + Vector3.up * 2.1f * size, new Vector3(1.05f, 1.15f, 1.05f) * size, ToyArt.Paint(leaves), 12, 8);
                    shadows.GroundQuad(foot + Vector3.up * 0.02f, Vector2.one * 2.6f * size, new Color(1f, 1f, 1f, 0.42f));
                }
            }

            // Far plane: rows of soft hills on both sides and behind the towers, and clouds.
            for (float z = trackStart - 20f; z < trackEnd + 90f; z += 14f)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    float x = side * (half + 26f + Next(random, 0f, 20f));
                    float radius = Next(random, 9f, 16f);
                    Color hill = ToyPalette.Hills[random.Next(ToyPalette.Hills.Length)];
                    props.Dome(new Vector3(x, GrassTop - 0.3f, z + Next(random, -4f, 4f)), radius, radius * Next(random, 0.5f, 0.8f),
                        ToyArt.Paint(hill, 0.35f), 0f, 22, 6, false);
                }
            }

            for (float x = -60f; x <= 60f; x += 13f)
            {
                float radius = Next(random, 10f, 18f);
                Color hill = ToyPalette.Hills[random.Next(ToyPalette.Hills.Length)];
                props.Dome(new Vector3(x, GrassTop - 0.3f, trackEnd + 55f + Next(random, 0f, 25f)), radius, radius * Next(random, 0.5f, 0.85f),
                    ToyArt.Paint(hill, 0.35f), 0f, 22, 6, false);
            }

            for (float z = trackStart + 10f; z < trackEnd + 100f; z += 22f)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Cloud(props, new Vector3(side * Next(random, 22f, 55f), Next(random, 16f, 26f), z + Next(random, -6f, 6f)), random);
                }
            }

            Material painted = ToyArt.Painted("Meadow", 1f, 0.5f);
            ToyArt.Place(set, "Meadow", ToyArt.SaveMesh(props.Build("Meadow"), "Meadow"), painted);
            ToyArt.Place(set, "MeadowShadows", ToyArt.SaveMesh(shadows.Build("MeadowShadows"), "MeadowShadows"),
                ToyArt.ContactShadow("MeadowShadows", ToyPalette.ShadowTint));
            return set.gameObject;
        }

        // Four petals around a gold heart, flat on the grass.
        private static void Flower(SoftMeshBuilder b, Vector3 at, System.Random random)
        {
            float size = Next(random, 0.75f, 1.2f);
            Color petal = ToyArt.Paint(ToyPalette.Petals[random.Next(ToyPalette.Petals.Length)], 0.6f);
            float turn = Next(random, 0f, Mathf.PI);
            for (int p = 0; p < 4; p++)
            {
                float angle = turn + p * Mathf.PI * 0.5f;
                var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.13f * size;
                b.Ellipsoid(at + offset + Vector3.up * 0.05f * size, new Vector3(0.11f, 0.05f, 0.11f) * size, petal, 5, 3);
            }

            b.Ellipsoid(at + Vector3.up * 0.09f * size, new Vector3(0.08f, 0.06f, 0.08f) * size, ToyArt.Paint(ToyPalette.FlowerCenter), 5, 3);
        }

        private static void Cloud(SoftMeshBuilder b, Vector3 at, System.Random random)
        {
            Color white = ToyArt.Paint(ToyPalette.Cloud, 0.2f);
            float size = Next(random, 1.6f, 3f);
            b.Ellipsoid(at, new Vector3(2.2f, 1.2f, 1.4f) * size, white, 10, 6);
            b.Ellipsoid(at + new Vector3(-1.9f, -0.3f, 0.2f) * size, new Vector3(1.4f, 0.9f, 1.1f) * size, white, 10, 6);
            b.Ellipsoid(at + new Vector3(1.8f, -0.35f, -0.1f) * size, new Vector3(1.5f, 0.85f, 1.1f) * size, white, 10, 6);
        }

        private static float Next(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }
    }
}
