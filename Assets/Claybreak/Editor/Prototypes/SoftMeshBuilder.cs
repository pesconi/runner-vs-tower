using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Editor.Prototypes
{
    /// <summary>
    /// Direction v2 prototype: builds smooth, rounded meshes (lathed profiles, ellipsoids, rounded boxes) with
    /// smooth normals and per-vertex color (rgb) and gloss (alpha), merged into one mesh per object. Everything
    /// the "soft toy" look needs without a modelling tool; the same shapes become authored assets in RT-6v.
    /// </summary>
    public sealed class SoftMeshBuilder
    {
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Vector3> normals = new List<Vector3>();
        private readonly List<Color> colors = new List<Color>();
        private readonly List<int> triangles = new List<int>();

        public int VertexCount => vertices.Count;

        /// <summary>A surface of revolution around local +Y. <paramref name="profile"/> is (radius, height) from bottom to top.</summary>
        public SoftMeshBuilder Lathe(IReadOnlyList<Vector2> profile, int segments, Matrix4x4 transform, Color color)
        {
            int start = vertices.Count;
            int rows = profile.Count;
            Matrix4x4 normalMatrix = transform.inverse.transpose;
            for (int i = 0; i < rows; i++)
            {
                Vector2 before = profile[Mathf.Max(0, i - 1)];
                Vector2 after = profile[Mathf.Min(rows - 1, i + 1)];
                Vector2 tangent = (after - before).normalized;
                var flatNormal = new Vector2(tangent.y, -tangent.x);
                if (profile[i].x <= 0.0001f)
                {
                    flatNormal = new Vector2(0f, i == 0 ? -1f : 1f);
                }

                for (int s = 0; s <= segments; s++)
                {
                    float angle = s * Mathf.PI * 2f / segments;
                    float cos = Mathf.Cos(angle);
                    float sin = Mathf.Sin(angle);
                    var local = new Vector3(profile[i].x * cos, profile[i].y, profile[i].x * sin);
                    var normal = new Vector3(flatNormal.x * cos, flatNormal.y, flatNormal.x * sin);
                    vertices.Add(transform.MultiplyPoint3x4(local));
                    normals.Add(normalMatrix.MultiplyVector(normal).normalized);
                    colors.Add(color);
                }
            }

            int stride = segments + 1;
            for (int i = 0; i < rows - 1; i++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int a = start + i * stride + s;
                    int b = a + stride;
                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(a + 1);
                    triangles.Add(a + 1);
                    triangles.Add(b);
                    triangles.Add(b + 1);
                }
            }

            return this;
        }

        /// <summary>An ellipsoid of the given radii centred at <paramref name="center"/>, optionally rotated.</summary>
        public SoftMeshBuilder Ellipsoid(Vector3 center, Vector3 radii, Color color, int segments = 14, int rings = 9,
            Quaternion? rotation = null)
        {
            var profile = new Vector2[rings + 1];
            for (int i = 0; i <= rings; i++)
            {
                float angle = -Mathf.PI * 0.5f + Mathf.PI * i / rings;
                profile[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }

            Quaternion turn = rotation ?? Quaternion.identity;
            return Lathe(profile, segments, Matrix4x4.TRS(center, turn, radii), color);
        }

        /// <summary>A cylinder of radius r and height h standing on <paramref name="baseCenter"/>, with edges rounded by <paramref name="bevel"/>.</summary>
        public SoftMeshBuilder Drum(Vector3 baseCenter, float radius, float height, float bevel, Color color, int segments = 24,
            float topRadius = -1f)
        {
            float top = topRadius < 0f ? radius : topRadius;
            var profile = new List<Vector2> { new Vector2(0f, 0f) };
            AddArc(profile, new Vector2(radius - bevel, bevel), bevel, -90f, 0f);
            AddArc(profile, new Vector2(top - bevel, height - bevel), bevel, 0f, 90f);
            profile.Add(new Vector2(0f, height));
            return Lathe(profile, segments, Matrix4x4.Translate(baseCenter), color);
        }

        /// <summary>A dome (half ellipsoid) sitting on <paramref name="baseCenter"/>; <paramref name="pointed"/> > 0 lifts the tip.</summary>
        public SoftMeshBuilder Dome(Vector3 baseCenter, float radius, float height, Color color, float pointed = 0f, int segments = 24)
        {
            var profile = new List<Vector2>();
            const int rows = 10;
            for (int i = 0; i <= rows; i++)
            {
                float t = (float)i / rows;
                float angle = t * Mathf.PI * 0.5f;
                float r = radius * Mathf.Cos(angle) * (1f - pointed * t * t * 0.5f);
                profile.Add(new Vector2(i == rows ? 0f : r, height * Mathf.Sin(angle)));
            }

            profile.Insert(0, new Vector2(0f, 0f));
            return Lathe(profile, segments, Matrix4x4.Translate(baseCenter), color);
        }

        /// <summary>A ring (torus) around +Y at <paramref name="center"/>.</summary>
        public SoftMeshBuilder Ring(Vector3 center, float radius, float thickness, Color color, int segments = 24)
        {
            var profile = new List<Vector2>();
            const int steps = 10;
            for (int i = 0; i <= steps; i++)
            {
                float angle = -Mathf.PI * 0.5f + Mathf.PI * 2f * i / steps;
                profile.Add(new Vector2(radius + Mathf.Cos(angle) * thickness, Mathf.Sin(angle) * thickness));
            }

            return Lathe(profile, segments, Matrix4x4.Translate(center), color);
        }

        /// <summary>A box of <paramref name="size"/> with every edge rounded by <paramref name="radius"/> (smooth normals).</summary>
        public SoftMeshBuilder RoundedBox(Vector3 center, Vector3 size, float radius, Color color, int divisions = 6,
            Quaternion? rotation = null)
        {
            Quaternion turn = rotation ?? Quaternion.identity;
            Vector3 half = size * 0.5f;
            Vector3 inner = new Vector3(Mathf.Max(0f, half.x - radius), Mathf.Max(0f, half.y - radius), Mathf.Max(0f, half.z - radius));
            Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward };
            for (int face = 0; face < 6; face++)
            {
                Vector3 n = axes[face % 3] * (face < 3 ? 1f : -1f);
                Vector3 u = axes[(face + 1) % 3];
                Vector3 v = axes[(face + 2) % 3];
                if (face >= 3)
                {
                    (u, v) = (v, u);
                }

                int start = vertices.Count;
                for (int y = 0; y <= divisions; y++)
                {
                    for (int x = 0; x <= divisions; x++)
                    {
                        Vector3 cube = n + u * (x * 2f / divisions - 1f) + v * (y * 2f / divisions - 1f);
                        Vector3 point = Vector3.Scale(cube, half);
                        Vector3 clamped = new Vector3(Mathf.Clamp(point.x, -inner.x, inner.x), Mathf.Clamp(point.y, -inner.y, inner.y),
                            Mathf.Clamp(point.z, -inner.z, inner.z));
                        Vector3 normal = (point - clamped).normalized;
                        if (normal == Vector3.zero)
                        {
                            normal = n;
                        }

                        vertices.Add(center + turn * (clamped + normal * radius));
                        normals.Add(turn * normal);
                        colors.Add(color);
                    }
                }

                int stride = divisions + 1;
                for (int y = 0; y < divisions; y++)
                {
                    for (int x = 0; x < divisions; x++)
                    {
                        int a = start + y * stride + x;
                        triangles.Add(a);
                        triangles.Add(a + 1);
                        triangles.Add(a + stride);
                        triangles.Add(a + 1);
                        triangles.Add(a + stride + 1);
                        triangles.Add(a + stride);
                    }
                }
            }

            return this;
        }

        public Mesh Build(string name)
        {
            var mesh = new Mesh { name = name, hideFlags = HideFlags.DontSave };
            if (vertices.Count > 65000)
            {
                mesh.indexFormat = IndexFormat.UInt32;
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddArc(List<Vector2> profile, Vector2 center, float radius, float fromDegrees, float toDegrees)
        {
            const int steps = 5;
            for (int i = 0; i <= steps; i++)
            {
                float angle = Mathf.Lerp(fromDegrees, toDegrees, (float)i / steps) * Mathf.Deg2Rad;
                profile.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
        }
    }
}
