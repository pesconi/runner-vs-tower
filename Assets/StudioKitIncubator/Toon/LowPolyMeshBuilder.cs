using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace JungleDegree.StudioKit.Toon
{
    /// <summary>
    /// Builds one low-poly mesh from placed primitives (sphere/ellipsoid, cone, cylinder, box, icosahedron, disc),
    /// each with one vertex color, merged into a single mesh with 16-bit indices: one model, one draw.
    /// Every primitive is the unit shape (radius 1 / size 1) transformed by a placement matrix
    /// (<see cref="Place"/>); normals follow the inverse transpose and mirrored placements keep their winding.
    /// Vertex color alpha is the tint mask of <c>StudioKit/ToonLit</c>: 255 = takes the tint, 0 = keeps its color.
    /// Unity convention: front faces are clockwise seen from outside; the model's front is +Z.
    /// </summary>
    public sealed class LowPolyMeshBuilder
    {
        public const int MaximumVertices = 65535;

        private readonly List<Vector3> positions = new List<Vector3>(256);
        private readonly List<Vector3> normals = new List<Vector3>(256);
        private readonly List<Color32> colors = new List<Color32>(256);
        private readonly List<int> indices = new List<int>(1024);

        public int VertexCount => positions.Count;
        public int TriangleCount => indices.Count / 3;

        /// <summary>Placement of a unit primitive: translation, rotation (Euler degrees) and scale.</summary>
        public static Matrix4x4 Place(Vector3 position, Vector3 eulerDegrees, Vector3 scale)
        {
            return Matrix4x4.TRS(position, Quaternion.Euler(eulerDegrees), scale);
        }

        public static Matrix4x4 Place(Vector3 position, Vector3 scale)
        {
            return Matrix4x4.TRS(position, Quaternion.identity, scale);
        }

        public void Clear()
        {
            positions.Clear();
            normals.Clear();
            colors.Clear();
            indices.Clear();
        }

        /// <summary>UV sphere of radius 1 (smooth): <paramref name="segments"/> around, <paramref name="bands"/> from pole to pole.
        /// Triangles: 2 × segments × (bands − 1).</summary>
        public LowPolyMeshBuilder AddSphere(Matrix4x4 placement, int segments, int bands, Color32 color)
        {
            Require(segments >= 3 && bands >= 2, "a sphere needs at least 3 segments and 2 bands");
            int top = Vertex(placement, Vector3.up, Vector3.up, color);
            int firstRing = positions.Count;
            for (int band = 1; band < bands; band++)
            {
                float polar = Mathf.PI * band / bands;
                for (int s = 0; s < segments; s++)
                {
                    float azimuth = 2f * Mathf.PI * s / segments;
                    var p = new Vector3(Mathf.Sin(polar) * Mathf.Cos(azimuth), Mathf.Cos(polar), Mathf.Sin(polar) * Mathf.Sin(azimuth));
                    Vertex(placement, p, p, color);
                }
            }

            int bottom = Vertex(placement, Vector3.down, Vector3.down, color);
            bool mirrored = placement.determinant < 0f;
            for (int s = 0; s < segments; s++)
            {
                int next = (s + 1) % segments;
                Triangle(top, firstRing + next, firstRing + s, mirrored);
                for (int band = 0; band < bands - 2; band++)
                {
                    int a = firstRing + band * segments + s;
                    int b = firstRing + band * segments + next;
                    int c = a + segments;
                    int d = b + segments;
                    Triangle(a, b, d, mirrored);
                    Triangle(a, d, c, mirrored);
                }

                int last = firstRing + (bands - 2) * segments;
                Triangle(bottom, last + s, last + next, mirrored);
            }

            return this;
        }

        /// <summary>Cone of radius 1 and height 1, base at y = 0, apex at y = 1 (smooth sides).
        /// Triangles: segments (+ segments with the base cap).</summary>
        public LowPolyMeshBuilder AddCone(Matrix4x4 placement, int segments, Color32 color, bool capBase = true)
        {
            Require(segments >= 3, "a cone needs at least 3 segments");
            bool mirrored = placement.determinant < 0f;
            float slope = 1f / Mathf.Sqrt(2f);
            for (int s = 0; s < segments; s++)
            {
                float a0 = 2f * Mathf.PI * s / segments;
                float a1 = 2f * Mathf.PI * (s + 1) / segments;
                float am = 0.5f * (a0 + a1);
                var r0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0));
                var r1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1));
                var rm = new Vector3(Mathf.Cos(am), 0f, Mathf.Sin(am));
                int i0 = Vertex(placement, r0, (r0 + Vector3.up) * slope, color);
                int i1 = Vertex(placement, r1, (r1 + Vector3.up) * slope, color);
                int apex = Vertex(placement, Vector3.up, (rm + Vector3.up) * slope, color);
                Triangle(apex, i1, i0, mirrored);
            }

            if (capBase)
            {
                AddCap(placement, segments, 0f, false, color, mirrored);
            }

            return this;
        }

        /// <summary>Cylinder of radius 1 from y = −0.5 to y = 0.5 (smooth sides).
        /// Triangles: 2 × segments (+ segments per cap).</summary>
        public LowPolyMeshBuilder AddCylinder(Matrix4x4 placement, int segments, Color32 color, bool caps = true)
        {
            Require(segments >= 3, "a cylinder needs at least 3 segments");
            bool mirrored = placement.determinant < 0f;
            int first = positions.Count;
            for (int s = 0; s < segments; s++)
            {
                float azimuth = 2f * Mathf.PI * s / segments;
                var radial = new Vector3(Mathf.Cos(azimuth), 0f, Mathf.Sin(azimuth));
                Vertex(placement, radial + 0.5f * Vector3.down, radial, color);
                Vertex(placement, radial + 0.5f * Vector3.up, radial, color);
            }

            for (int s = 0; s < segments; s++)
            {
                int next = (s + 1) % segments;
                int b0 = first + 2 * s;
                int t0 = b0 + 1;
                int b1 = first + 2 * next;
                int t1 = b1 + 1;
                Triangle(b0, t0, t1, mirrored);
                Triangle(b0, t1, b1, mirrored);
            }

            if (caps)
            {
                AddCap(placement, segments, 0.5f, true, color, mirrored);
                AddCap(placement, segments, -0.5f, false, color, mirrored);
            }

            return this;
        }

        /// <summary>Flat-shaded disc of radius 1 in the XZ plane, facing +Y. Triangles: segments.</summary>
        public LowPolyMeshBuilder AddDisc(Matrix4x4 placement, int segments, Color32 color)
        {
            Require(segments >= 3, "a disc needs at least 3 segments");
            AddCap(placement, segments, 0f, true, color, placement.determinant < 0f);
            return this;
        }

        /// <summary>Flat-shaded cube of size 1 centered on the origin. Triangles: 12.</summary>
        public LowPolyMeshBuilder AddBox(Matrix4x4 placement, Color32 color)
        {
            bool mirrored = placement.determinant < 0f;
            Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward };
            for (int axis = 0; axis < 3; axis++)
            {
                Vector3 u = axes[(axis + 1) % 3];
                Vector3 v = axes[(axis + 2) % 3];
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    Vector3 n = axes[axis] * sign;
                    Vector3 center = 0.5f * n;
                    int a = Vertex(placement, center - 0.5f * u - 0.5f * v, n, color);
                    int b = Vertex(placement, center + 0.5f * u - 0.5f * v, n, color);
                    int c = Vertex(placement, center + 0.5f * u + 0.5f * v, n, color);
                    int d = Vertex(placement, center - 0.5f * u + 0.5f * v, n, color);
                    // u × v = +axis, so (a, b, c) faces +axis (cross(b − a, c − a) points to the front side).
                    bool flip = sign < 0;
                    Triangle(a, b, c, mirrored ^ flip);
                    Triangle(a, c, d, mirrored ^ flip);
                }
            }

            return this;
        }

        /// <summary>Flat-shaded icosahedron of radius 1. Triangles: 20.</summary>
        public LowPolyMeshBuilder AddIcosahedron(Matrix4x4 placement, Color32 color)
        {
            bool mirrored = placement.determinant < 0f;
            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            Vector3[] v =
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1),
            };
            int[] faces =
            {
                0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
                3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1,
            };
            for (int f = 0; f < faces.Length; f += 3)
            {
                Vector3 a = v[faces[f]].normalized;
                Vector3 b = v[faces[f + 1]].normalized;
                Vector3 c = v[faces[f + 2]].normalized;
                Vector3 n = (a + b + c).normalized;
                // Order the corners clockwise seen from outside (Unity front face).
                if (Vector3.Dot(Vector3.Cross(b - a, c - a), n) < 0f)
                {
                    (b, c) = (c, b);
                }

                Triangle(Vertex(placement, a, n, color), Vertex(placement, b, n, color), Vertex(placement, c, n, color), mirrored);
            }

            return this;
        }

        /// <summary>The merged mesh (16-bit indices), with bounds. Throws past 65535 vertices.</summary>
        public Mesh ToMesh(string name)
        {
            Require(positions.Count <= MaximumVertices, $"{positions.Count} vertices do not fit 16-bit indices");
            var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt16 };
            mesh.SetVertices(positions);
            mesh.SetNormals(normals);
            mesh.SetColors(colors);
            mesh.SetTriangles(indices, 0, true);
            return mesh;
        }

        private void AddCap(Matrix4x4 placement, int segments, float y, bool facingUp, Color32 color, bool mirrored)
        {
            Vector3 n = facingUp ? Vector3.up : Vector3.down;
            int center = Vertex(placement, new Vector3(0f, y, 0f), n, color);
            int first = positions.Count;
            for (int s = 0; s < segments; s++)
            {
                float azimuth = 2f * Mathf.PI * s / segments;
                Vertex(placement, new Vector3(Mathf.Cos(azimuth), y, Mathf.Sin(azimuth)), n, color);
            }

            for (int s = 0; s < segments; s++)
            {
                int a = first + s;
                int b = first + (s + 1) % segments;
                // Azimuth grows from +X toward +Z, which is counter-clockwise seen from +Y in a left-handed frame.
                if (facingUp)
                {
                    Triangle(center, b, a, mirrored);
                }
                else
                {
                    Triangle(center, a, b, mirrored);
                }
            }
        }

        private int Vertex(Matrix4x4 placement, Vector3 position, Vector3 normal, Color32 color)
        {
            positions.Add(placement.MultiplyPoint3x4(position));
            normals.Add(placement.inverse.transpose.MultiplyVector(normal).normalized);
            colors.Add(color);
            return positions.Count - 1;
        }

        private void Triangle(int a, int b, int c, bool mirrored)
        {
            indices.Add(a);
            indices.Add(mirrored ? c : b);
            indices.Add(mirrored ? b : c);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new ArgumentException(message);
            }
        }
    }
}
