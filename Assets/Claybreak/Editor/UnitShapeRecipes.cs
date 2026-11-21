using JungleDegree.StudioKit.Toon;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// Recipes of this game's crowd shapes, built with the kit's <see cref="LowPolyMeshBuilder"/> (one mesh, vertex
    /// colors; alpha 255 = body, takes the palette or skin tint; alpha 0 = fixed color). Front is +Z (Unity), the
    /// mirror of the three.js reference (−Z). RT-6t.1 has only the provisional Bolinha at the 500-triangle ceiling;
    /// the final Bolinha and the Pintinho are RT-6t.2.
    /// </summary>
    public static class UnitShapeRecipes
    {
        /// <summary>Body radius of every unit shape (reference R = 0.22).</summary>
        public const float Radius = 0.22f;
        public const int MinimumTriangles = 200;
        public const int MaximumTriangles = 500;

        public static readonly Color32 Body = new Color32(255, 255, 255, 255);
        public static readonly Color32 Eyes = new Color32(0x2b, 0x22, 0x33, 0);
        public static readonly Color32 Leaf = new Color32(0x5c, 0xc9, 0x8a, 0);

        /// <summary>
        /// Provisional Bolinha: ball squashed to 0.9 in Y, two eyes on the front, a sprout (stem and two leaves) on top.
        /// 474 triangles: body 16×11 (320), eyes 6×4 (2 × 36), stem 5 sides (10), leaves 6×4 (2 × 36).
        /// </summary>
        public static Mesh ProvisionalBolinha()
        {
            const float r = Radius;
            var builder = new LowPolyMeshBuilder();
            builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, r * 0.9f, 0f), new Vector3(r, r * 0.9f, r)), 16, 11, Body);
            for (int side = -1; side <= 1; side += 2)
            {
                builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(side * 0.08f, r * 1.05f, r * 0.86f), Vector3.one * 0.035f), 6, 4, Eyes);
            }

            builder.AddCylinder(LowPolyMeshBuilder.Place(new Vector3(0f, r * 1.8f + 0.04f, 0f), new Vector3(0.012f, 0.08f, 0.012f)), 5, Leaf, false);
            for (int side = -1; side <= 1; side += 2)
            {
                builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(side * 0.045f, r * 1.8f + 0.085f, 0f), new Vector3(0.05f, 0.015f, 0.03f)), 6, 4, Leaf);
            }

            return builder.ToMesh("BolinhaProvisional");
        }

        /// <summary>Blob shadow disc under a unit: radius 0.95 R, 12 sides, facing up.</summary>
        public static Mesh ShadowDisc()
        {
            return new LowPolyMeshBuilder()
                .AddDisc(LowPolyMeshBuilder.Place(Vector3.zero, new Vector3(Radius * 0.95f, 1f, Radius * 0.95f)), 12, Body)
                .ToMesh("BlobShadowDisc");
        }
    }
}
