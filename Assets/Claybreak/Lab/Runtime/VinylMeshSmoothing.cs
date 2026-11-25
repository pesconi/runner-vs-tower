using System.Collections.Generic;
using UnityEngine;

namespace Claybreak.Lab
{
    /// <summary>
    /// RT-6v3.2 laboratory: makes a low-poly toon mesh read as a smooth vinyl toy without remodelling it, for the concept
    /// screens and the phone rehearsal. Two steps: <see cref="SmoothNormals"/> averages the face normals of corners that meet
    /// at a crease softer than the angle (harder creases stay sharp); <see cref="Subdivide"/> splits every triangle in four
    /// with PN triangles (the new points sit on the cubic patch the normals describe, so the silhouette rounds out), using the
    /// straight midpoint on hard edges so the mesh never cracks. Colors (tint mask in alpha) are interpolated. The meshes of
    /// the game proper come from the series' own pipeline (RT-6v3.4a/5): this only shows the target.
    /// </summary>
    public static class VinylMeshSmoothing
    {
        /// <summary>A copy of <paramref name="source"/> with normals smoothed across creases under <paramref name="angle"/> degrees.</summary>
        public static Mesh SmoothNormals(Mesh source, float angle)
        {
            Vector3[] positions = source.vertices;
            Color[] colors = source.colors;
            int[] triangles = source.triangles;
            int corners = triangles.Length;
            var faceNormals = new Vector3[corners / 3];
            for (int t = 0; t < faceNormals.Length; t++)
            {
                Vector3 a = positions[triangles[3 * t]];
                Vector3 b = positions[triangles[3 * t + 1]];
                Vector3 c = positions[triangles[3 * t + 2]];
                faceNormals[t] = Vector3.Cross(b - a, c - a); // area weighted
            }

            // Corners grouped by position.
            var byPosition = new Dictionary<Vector3Int, List<int>>();
            for (int i = 0; i < corners; i++)
            {
                Vector3Int key = Key(positions[triangles[i]]);
                if (!byPosition.TryGetValue(key, out List<int> list))
                {
                    byPosition[key] = list = new List<int>();
                }

                list.Add(i);
            }

            float cosine = Mathf.Cos(angle * Mathf.Deg2Rad);
            var outPositions = new List<Vector3>(corners);
            var outNormals = new List<Vector3>(corners);
            var outColors = new List<Color>(corners);
            var outTriangles = new int[corners];
            var welded = new Dictionary<(Vector3Int, Vector3Int, Color32), int>();
            for (int i = 0; i < corners; i++)
            {
                Vector3 own = faceNormals[i / 3].normalized;
                Vector3 sum = Vector3.zero;
                foreach (int other in byPosition[Key(positions[triangles[i]])])
                {
                    Vector3 face = faceNormals[other / 3];
                    if (Vector3.Dot(own, face.normalized) >= cosine)
                    {
                        sum += face;
                    }
                }

                Vector3 normal = sum.sqrMagnitude > 0f ? sum.normalized : own;
                Vector3 position = positions[triangles[i]];
                Color color = colors.Length > 0 ? colors[triangles[i]] : Color.white;
                var weldKey = (Key(position), Vector3Int.RoundToInt(normal * 1000f), (Color32)color);
                if (!welded.TryGetValue(weldKey, out int index))
                {
                    index = outPositions.Count;
                    welded[weldKey] = index;
                    outPositions.Add(position);
                    outNormals.Add(normal);
                    outColors.Add(color);
                }

                outTriangles[i] = index;
            }

            return Build(source.name + "Smooth", outPositions, outNormals, outColors, outTriangles);
        }

        /// <summary>Splits every triangle of <paramref name="source"/> in four on its PN-triangle patch (4× the triangles).</summary>
        public static Mesh Subdivide(Mesh source)
        {
            Vector3[] p = source.vertices;
            Vector3[] n = source.normals;
            Color[] c = source.colors.Length > 0 ? source.colors : Fill(p.Length, Color.white);
            int[] triangles = source.triangles;

            // An edge (by positions) is hard when the triangles on it disagree on the normals at its ends.
            var edgeNormals = new Dictionary<(Vector3Int, Vector3Int), (Vector3, Vector3)>();
            var hard = new HashSet<(Vector3Int, Vector3Int)>();
            for (int t = 0; t < triangles.Length; t += 3)
            {
                for (int e = 0; e < 3; e++)
                {
                    int i = triangles[t + e];
                    int j = triangles[t + (e + 1) % 3];
                    (Vector3Int, Vector3Int) key = EdgeKey(p[i], p[j], n[i], n[j], out Vector3 ni, out Vector3 nj);
                    if (edgeNormals.TryGetValue(key, out (Vector3, Vector3) seen))
                    {
                        if (Vector3.Dot(seen.Item1, ni) < 0.999f || Vector3.Dot(seen.Item2, nj) < 0.999f)
                        {
                            hard.Add(key);
                        }
                    }
                    else
                    {
                        edgeNormals[key] = (ni, nj);
                    }
                }
            }

            var positions = new List<Vector3>(p);
            var normals = new List<Vector3>(n);
            var colors = new List<Color>(c);
            var midpoints = new Dictionary<(int, int), int>();
            var outTriangles = new List<int>(triangles.Length * 4);
            for (int t = 0; t < triangles.Length; t += 3)
            {
                int a = triangles[t];
                int b = triangles[t + 1];
                int d = triangles[t + 2];
                int ab = Midpoint(a, b, p, n, c, hard, positions, normals, colors, midpoints);
                int bd = Midpoint(b, d, p, n, c, hard, positions, normals, colors, midpoints);
                int da = Midpoint(d, a, p, n, c, hard, positions, normals, colors, midpoints);
                outTriangles.AddRange(new[] { a, ab, da, ab, b, bd, da, bd, d, ab, bd, da });
            }

            return Build(source.name + "Sub", positions, normals, colors, outTriangles.ToArray());
        }

        private static int Midpoint(int i, int j, Vector3[] p, Vector3[] n, Color[] c, HashSet<(Vector3Int, Vector3Int)> hard,
            List<Vector3> positions, List<Vector3> normals, List<Color> colors, Dictionary<(int, int), int> midpoints)
        {
            (int, int) key = i < j ? (i, j) : (j, i);
            if (midpoints.TryGetValue(key, out int index))
            {
                return index;
            }

            Vector3 straight = (p[i] + p[j]) * 0.5f;
            Vector3 position = straight;
            if (!hard.Contains(EdgeKey(p[i], p[j], n[i], n[j], out _, out _)))
            {
                // PN triangles: the edge's cubic control points b210 and b120, evaluated at the middle.
                Vector3 b210 = (2f * p[i] + p[j] - Vector3.Dot(p[j] - p[i], n[i]) * n[i]) / 3f;
                Vector3 b120 = (2f * p[j] + p[i] - Vector3.Dot(p[i] - p[j], n[j]) * n[j]) / 3f;
                position = (p[i] + p[j]) * 0.125f + (b210 + b120) * 0.375f;
            }

            index = positions.Count;
            positions.Add(position);
            normals.Add((n[i] + n[j]).normalized);
            colors.Add(Color.Lerp(c[i], c[j], 0.5f));
            midpoints[key] = index;
            return index;
        }

        private static (Vector3Int, Vector3Int) EdgeKey(Vector3 pi, Vector3 pj, Vector3 ni, Vector3 nj, out Vector3 first, out Vector3 second)
        {
            Vector3Int ki = Key(pi);
            Vector3Int kj = Key(pj);
            bool ordered = ki.x < kj.x || (ki.x == kj.x && (ki.y < kj.y || (ki.y == kj.y && ki.z <= kj.z)));
            first = ordered ? ni : nj;
            second = ordered ? nj : ni;
            return ordered ? (ki, kj) : (kj, ki);
        }

        private static Vector3Int Key(Vector3 position)
        {
            return Vector3Int.RoundToInt(position * 10000f);
        }

        private static Color[] Fill(int count, Color color)
        {
            var colors = new Color[count];
            for (int i = 0; i < count; i++)
            {
                colors[i] = color;
            }

            return colors;
        }

        private static Mesh Build(string name, List<Vector3> positions, List<Vector3> normals, List<Color> colors, int[] triangles)
        {
            var mesh = new Mesh { name = name };
            if (positions.Count > 65000)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.SetVertices(positions);
            mesh.SetNormals(normals);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
