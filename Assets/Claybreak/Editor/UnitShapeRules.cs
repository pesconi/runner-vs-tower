using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Editor
{
    /// <summary>
    /// Budget and shape rules of a crowd shape mesh (RT-6t.2), shared by the validator and the tests: one submesh within its
    /// level's triangle range (200–500 for the legacy recipes) with 16-bit indices; a tint mask (vertex alpha only 255 = body or 0 = fixed color, both
    /// present); resting on the ground (lowest point at y = 0); eyes only in the eyed variants, on the +Z front; and the
    /// front marker (eyes, beak) sticking out toward +Z, so a recipe that forgot the three.js → Unity mirror fails.
    /// </summary>
    public static class UnitShapeRules
    {
        private const float GroundTolerance = 1e-3f;

        /// <summary>
        /// The problems of a shape mesh; <paramref name="budget"/> is its triangle range (min, max), the legacy 200-500 when
        /// null (RT-6v3.4a: the Bibopzi levels pass theirs, <see cref="UnitShapeAssets.Budget"/>).
        /// </summary>
        public static List<string> Check(Mesh mesh, bool hasEyes, Color32? frontMarker, Vector2Int? budget = null)
        {
            var problems = new List<string>();
            Vector2Int range = budget ?? UnitShapeAssets.Budget(UnitShapeAssets.ShapeLevel.Legacy);
            int triangles = mesh.subMeshCount == 0 ? 0 : (int)(mesh.GetIndexCount(0) / 3);
            if (mesh.subMeshCount != 1 || triangles < range.x || triangles > range.y)
            {
                problems.Add($"{triangles} triangles in {mesh.subMeshCount} submesh(es); the budget is one mesh of {range.x}-{range.y}");
            }

            if (mesh.indexFormat != IndexFormat.UInt16)
            {
                problems.Add("must use 16-bit indices");
            }

            Vector3[] vertices = mesh.vertices;
            Color32[] colors = mesh.colors32;
            if (colors.Length != vertices.Length)
            {
                problems.Add("every vertex needs a color (tint mask in the alpha)");
                return problems;
            }

            int body = 0;
            int fixedColor = 0;
            int eyes = 0;
            float eyesZ = 0f;
            float markerFront = float.NegativeInfinity;
            for (int i = 0; i < vertices.Length; i++)
            {
                Color32 c = colors[i];
                if (c.a == 255)
                {
                    body++;
                }
                else if (c.a == 0)
                {
                    fixedColor++;
                }

                if (Same(c, UnitShapeRecipes.Eyes))
                {
                    eyes++;
                    eyesZ += vertices[i].z;
                }

                if (frontMarker.HasValue && Same(c, frontMarker.Value))
                {
                    markerFront = Mathf.Max(markerFront, vertices[i].z);
                }
            }

            if (body + fixedColor != vertices.Length)
            {
                problems.Add("tint mask: vertex alpha must be 255 (body) or 0 (fixed color)");
            }

            if (body == 0 || fixedColor == 0)
            {
                problems.Add($"tint mask: needs body ({body}) and fixed-color ({fixedColor}) vertices");
            }

            if (Mathf.Abs(mesh.bounds.min.y) > GroundTolerance)
            {
                problems.Add($"must rest on the ground (lowest point y = {mesh.bounds.min.y:0.####})");
            }

            if (hasEyes && (eyes == 0 || eyesZ / eyes <= 0.1f))
            {
                problems.Add("the eyes (fixed dark color) must be on the +Z front");
            }
            else if (!hasEyes && eyes > 0)
            {
                problems.Add("the variant without eyes has eye-colored vertices");
            }

            if (frontMarker.HasValue && !(markerFront > UnitShapeRecipes.Radius * 0.9f))
            {
                problems.Add("the front detail (eyes, beak) must stick out toward +Z");
            }

            return problems;
        }

        /// <summary>Triangles of the first submesh.</summary>
        public static int Triangles(Mesh mesh)
        {
            return mesh.subMeshCount == 0 ? 0 : (int)(mesh.GetIndexCount(0) / 3);
        }

        private static bool Same(Color32 a, Color32 b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }
    }
}
