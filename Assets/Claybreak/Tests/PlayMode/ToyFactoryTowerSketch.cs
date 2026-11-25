using System;
using System.Collections.Generic;
using Claybreak.Lab;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6v3.2 laboratory: the "toy factory" version of a tower/boss for decision D36 (the other version keeps each world's
    /// theme, D17/D28). A tower stacked from the Bibopzi's toy primitives, all smooth and rounded: a drum with a gear rim at
    /// the base, a bolted drum, a drum with a big gear on the front, two coil springs as side columns, a dome and a wind-up
    /// key on top. No face (D17). Colors follow the world's palette through the structure weights (light, dark, detail,
    /// defense) like the current towers, so the same sketch dresses any world. Built by the concept captures only (test
    /// assembly: never in a player).
    /// About 10 units high before <see cref="Build"/> scales it; triangle count logged by the caller.
    /// </summary>
    public static class ToyFactoryTowerSketch
    {
        public const float Height = 10f;

        /// <summary>Builds the sketch under a new object, scaled so it is <paramref name="height"/> tall, with <paramref name="shader"/>.</summary>
        public static GameObject Build(Shader shader, float height)
        {
            var root = new GameObject("ToyFactoryTowerSketch");
            Material light = Make(shader, new Vector4(1f, 0f, 0f, 0f), Color.white);
            Material dark = Make(shader, new Vector4(0f, 1f, 0f, 0f), Color.white);
            Material detail = Make(shader, new Vector4(0f, 0f, 1f, 0f), Color.white);
            Material accent = Make(shader, new Vector4(0f, 0f, 0f, 1f), Color.white);
            Material metal = Make(shader, Vector4.zero, new Color(0.83f, 0.85f, 0.9f));

            // Base: a low drum whose rim is a 16-tooth gear.
            Part(root, "BaseGear", Lathe(Profile((0f, 0f), (2.9f, 0f), (3.1f, 0.15f), (3.1f, 0.95f), (2.9f, 1.15f), (0f, 1.15f)), 96,
                angle => 1f + 0.07f * Tooth(angle * 16f)), dark, Vector3.zero, Quaternion.identity);
            // Bolted drum.
            Part(root, "Drum1", Lathe(Profile((0f, 1.1f), (2.3f, 1.1f), (2.45f, 1.3f), (2.45f, 3.3f), (2.3f, 3.5f), (0f, 3.5f)), 48, null),
                light, Vector3.zero, Quaternion.identity);
            for (int i = 0; i < 10; i++)
            {
                float a = i * Mathf.PI * 2f / 10f;
                var position = new Vector3(Mathf.Sin(a) * 2.43f, 2.3f, Mathf.Cos(a) * 2.43f);
                Part(root, "Bolt" + i, Lathe(Profile((0f, 0f), (0.32f, 0f), (0.32f, 0.12f), (0.22f, 0.24f), (0f, 0.27f)), 6, null),
                    metal, position, Quaternion.FromToRotation(Vector3.up, new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a))));
            }

            // Upper drum with the big gear on the front (toward -Z, the side the army comes from).
            Part(root, "Drum2", Lathe(Profile((0f, 3.45f), (1.85f, 3.45f), (1.95f, 3.6f), (1.95f, 5.7f), (1.8f, 5.85f), (0f, 5.85f)), 48, null),
                detail, Vector3.zero, Quaternion.identity);
            Part(root, "FrontGear", Lathe(Profile((0f, 0f), (1.25f, 0f), (1.32f, 0.08f), (1.32f, 0.32f), (1.25f, 0.4f), (0f, 0.4f)), 96,
                angle => 1f + 0.12f * Tooth(angle * 12f)), accent, new Vector3(0f, 4.65f, -1.8f), Quaternion.Euler(-90f, 0f, 0f));
            Part(root, "GearCap", Lathe(Profile((0f, 0f), (0.42f, 0f), (0.42f, 0.12f), (0.3f, 0.32f), (0f, 0.38f)), 24, null),
                metal, new Vector3(0f, 4.65f, -2.15f), Quaternion.Euler(-90f, 0f, 0f));
            // Coil springs as side columns.
            Part(root, "SpringL", Spring(0.55f, 0.13f, 7.5f, 4.2f, 14, 140), metal, new Vector3(-2.75f, 1.15f, -0.6f), Quaternion.identity);
            Part(root, "SpringR", Spring(0.55f, 0.13f, 7.5f, 4.2f, 14, 140), metal, new Vector3(2.75f, 1.15f, -0.6f), Quaternion.identity);
            Part(root, "SpringCapL", Lathe(Dome(0.62f, 0.45f), 24, null), dark, new Vector3(-2.75f, 5.35f, -0.6f), Quaternion.identity);
            Part(root, "SpringCapR", Lathe(Dome(0.62f, 0.45f), 24, null), dark, new Vector3(2.75f, 5.35f, -0.6f), Quaternion.identity);
            // Dome and wind-up key.
            Part(root, "Dome", Lathe(Dome(1.7f, 1.5f), 48, null), light, new Vector3(0f, 5.8f, 0f), Quaternion.identity);
            Part(root, "KeyStem", Lathe(Profile((0f, 0f), (0.22f, 0f), (0.22f, 1.0f), (0f, 1.0f)), 16, null), metal,
                new Vector3(0f, 7.15f, 0f), Quaternion.identity);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(root, side < 0 ? "KeyLobeL" : "KeyLobeR", Lathe(Profile((0f, -0.18f), (0.55f, -0.16f), (0.62f, 0f), (0.55f, 0.16f), (0f, 0.18f)), 32, null),
                    accent, new Vector3(side * 0.62f, 8.45f, 0f), Quaternion.Euler(0f, 0f, 90f));
            }

            root.transform.localScale = Vector3.one * (height / Height);
            return root;
        }

        /// <summary>A smooth ball with white vertex colors (tint mask 1): backdrops of the icon sketches.</summary>
        public static Mesh Ball(float radius)
        {
            const int steps = 16;
            var profile = new Vector2[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                float a = -Mathf.PI * 0.5f + i * Mathf.PI / steps;
                profile[i] = new Vector2(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius);
            }

            return Lathe(profile, 32, null);
        }

        /// <summary>Total triangles of the sketch (for the log and the D36 card).</summary>
        public static int Triangles(GameObject sketch)
        {
            int total = 0;
            foreach (MeshFilter filter in sketch.GetComponentsInChildren<MeshFilter>())
            {
                total += filter.sharedMesh.triangles.Length / 3;
            }

            return total;
        }

        private static Material Make(Shader shader, Vector4 weights, Color tint)
        {
            var material = new Material(shader);
            material.SetVector("_StructureWeights", weights);
            material.SetColor("_TintColor", tint);
            material.SetFloat("_ToonFogAmount", 0.35f);
            return material;
        }

        private static void Part(GameObject root, string name, Mesh mesh, Material material, Vector3 position, Quaternion rotation)
        {
            var part = new GameObject(name);
            part.transform.SetParent(root.transform, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // Rounded tooth: 0 in the gap, 1 on the tooth, smooth flanks.
        private static float Tooth(float phase)
        {
            float s = Mathf.Sin(phase);
            return Mathf.Clamp01(s * 2.2f + 0.2f);
        }

        private static Vector2[] Profile(params (float r, float y)[] points)
        {
            var profile = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                profile[i] = new Vector2(points[i].r, points[i].y);
            }

            return profile;
        }

        private static Vector2[] Dome(float radius, float height)
        {
            const int steps = 10;
            var profile = new Vector2[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                float a = i * Mathf.PI * 0.5f / steps;
                profile[i] = new Vector2(Mathf.Cos(a) * radius, Mathf.Sin(a) * height);
            }

            return profile;
        }

        /// <summary>Surface of revolution of a (radius, height) profile, the radius scaled by <paramref name="radial"/>(angle).</summary>
        private static Mesh Lathe(Vector2[] profile, int segments, Func<float, float> radial)
        {
            var positions = new List<Vector3>();
            var triangles = new List<int>();
            int rows = profile.Length;
            for (int s = 0; s <= segments; s++)
            {
                float a = s * Mathf.PI * 2f / segments;
                float scale = radial == null ? 1f : radial(a);
                for (int r = 0; r < rows; r++)
                {
                    float radius = profile[r].x;
                    // The rim scales with the teeth; the axis stays put.
                    float tooth = radius > 0.001f ? Mathf.Lerp(1f, scale, Mathf.Clamp01(radius / MaxRadius(profile))) : 1f;
                    positions.Add(new Vector3(Mathf.Sin(a) * radius * tooth, profile[r].y, Mathf.Cos(a) * radius * tooth));
                }
            }

            for (int s = 0; s < segments; s++)
            {
                for (int r = 0; r < rows - 1; r++)
                {
                    int i = s * rows + r;
                    int j = (s + 1) * rows + r;
                    triangles.AddRange(new[] { i, j, i + 1, j, j + 1, i + 1 });
                }
            }

            return Finish("Lathe", positions, triangles, true);
        }

        private static float MaxRadius(Vector2[] profile)
        {
            float max = 0f;
            foreach (Vector2 point in profile)
            {
                max = Mathf.Max(max, point.x);
            }

            return max;
        }

        /// <summary>A tube following a helix: coil radius, wire radius, turns, height.</summary>
        private static Mesh Spring(float coil, float wire, float turns, float height, int sides, int steps)
        {
            var positions = new List<Vector3>();
            var triangles = new List<int>();
            for (int s = 0; s <= steps; s++)
            {
                float t = (float)s / steps;
                float a = t * turns * Mathf.PI * 2f;
                var center = new Vector3(Mathf.Cos(a) * coil, t * height, Mathf.Sin(a) * coil);
                var tangent = new Vector3(-Mathf.Sin(a) * coil * turns * Mathf.PI * 2f, height, Mathf.Cos(a) * coil * turns * Mathf.PI * 2f).normalized;
                var outward = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Vector3 binormal = Vector3.Cross(tangent, outward).normalized;
                for (int k = 0; k <= sides; k++)
                {
                    float b = k * Mathf.PI * 2f / sides;
                    positions.Add(center + (outward * Mathf.Cos(b) + binormal * Mathf.Sin(b)) * wire);
                }
            }

            int ring = sides + 1;
            for (int s = 0; s < steps; s++)
            {
                for (int k = 0; k < sides; k++)
                {
                    int i = s * ring + k;
                    int j = i + ring;
                    triangles.AddRange(new[] { i, i + 1, j, i + 1, j + 1, j });
                }
            }

            return Finish("Spring", positions, triangles, false);
        }

        private static Mesh Finish(string name, List<Vector3> positions, List<int> triangles, bool weldSeam)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(positions);
            var colors = new Color[positions.Count];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white; // alpha 1: takes the tint
            }

            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return weldSeam ? VinylMeshSmoothing.SmoothNormals(mesh, 70f) : mesh;
        }
    }
}
