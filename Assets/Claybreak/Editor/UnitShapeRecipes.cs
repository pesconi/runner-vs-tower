using JungleDegree.StudioKit.Toon;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// Recipes of this game's crowd shapes (RT-6t.2), built with the kit's <see cref="LowPolyMeshBuilder"/>: one mesh,
    /// vertex colors, alpha 255 = body (takes the palette or skin tint), alpha 0 = fixed color (eyes, leaves, beak).
    /// They follow <c>unitGeo()</c> of the reference prototype (docs/reference/crowd-proto.html) with fewer segments to
    /// stay within 200–500 triangles. Front is +Z (Unity), the mirror of the three.js reference (−Z): a reference point
    /// (x, y, z) is (x, y, −z) here, and a reference rotation about X keeps its axis with the opposite sign.
    /// Each shape also has a variant without eyes (decision D25, still open: both are kept ready).
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
        public static readonly Color32 Orange = new Color32(0xff, 0x9f, 0x43, 0);

        // Detail resolution (the reference uses 8×6 for the small spheres; 6×4 keeps the budget and reads the same at
        // crowd distance).
        private const int SmallSegments = 6;
        private const int SmallBands = 4;
        private const float EyeRadius = 0.035f;
        private const float EyeAcross = 0.08f;
        private const float EyeDepth = Radius * 0.86f;

        /// <summary>
        /// Bolinha: ball squashed to 0.9 in Y, two eyes on the front, a sprout (stem and two leaves) on top.
        /// Triangles: body 16×11 (320), eyes 6×4 (2 × 36), stem 5 sides with caps (20), leaves 6×4 (2 × 36) = 484;
        /// 412 without eyes.
        /// </summary>
        public static Mesh Bolinha(bool eyes)
        {
            const float r = Radius;
            var builder = new LowPolyMeshBuilder();
            builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, r * 0.9f, 0f), new Vector3(r, r * 0.9f, r)), 16, 11, Body);
            if (eyes)
            {
                AddEyes(builder, r * 1.05f);
            }

            builder.AddCylinder(LowPolyMeshBuilder.Place(new Vector3(0f, r * 1.8f + 0.04f, 0f), new Vector3(0.012f, 0.08f, 0.012f)), 5, Leaf);
            for (int side = -1; side <= 1; side += 2)
            {
                builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(side * 0.045f, r * 1.8f + 0.085f, 0f), new Vector3(0.05f, 0.015f, 0.03f)),
                    SmallSegments, SmallBands, Leaf);
            }

            return builder.ToMesh(eyes ? "Bolinha" : "BolinhaNoEyes");
        }

        /// <summary>
        /// Pintinho: round ball, two eyes, an orange cone beak on the front, a two-ball orange crest on top and a small
        /// body-colored cone tail pointing back and up.
        /// Triangles: body 16×11 (320), eyes (72), beak 8 sides with base (16), crest 6×4 (2 × 36), tail 6 sides with base
        /// (12) = 492; 420 without eyes.
        /// </summary>
        public static Mesh Pintinho(bool eyes)
        {
            const float r = Radius;
            var builder = new LowPolyMeshBuilder();
            builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, r, 0f), Vector3.one * r), 16, 11, Body);
            if (eyes)
            {
                AddEyes(builder, r * 1.15f);
            }

            // Reference: cone(0.05, 0.11, 8) at (0, 0.95R, −1.05R), rotated −90° about X (apex to the front).
            AddCone(builder, new Vector3(0f, r * 0.95f, r * 1.05f), 90f, 0.05f, 0.11f, 8, Orange);
            builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, r * 2.02f, 0f), Vector3.one * 0.045f), SmallSegments, SmallBands, Orange);
            builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, r * 1.95f, -0.06f), Vector3.one * 0.04f), SmallSegments, SmallBands, Orange);
            // Reference: cone(0.06, 0.12, 6) in the body color at (0, 1.2R, 0.95R), rotated 1.1 rad about X (back and up).
            AddCone(builder, new Vector3(0f, r * 1.2f, -r * 0.95f), -1.1f * Mathf.Rad2Deg, 0.06f, 0.12f, 6, Body);
            return builder.ToMesh(eyes ? "Pintinho" : "PintinhoNoEyes");
        }

        /// <summary>Blob shadow disc under a unit: radius 0.95 R, 12 sides, facing up.</summary>
        public static Mesh ShadowDisc()
        {
            return new LowPolyMeshBuilder()
                .AddDisc(LowPolyMeshBuilder.Place(Vector3.zero, new Vector3(Radius * 0.95f, 1f, Radius * 0.95f)), 12, Body)
                .ToMesh("BlobShadowDisc");
        }

        private static void AddEyes(LowPolyMeshBuilder builder, float height)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(side * EyeAcross, height, EyeDepth), Vector3.one * EyeRadius),
                    SmallSegments, SmallBands, Eyes);
            }
        }

        // A three.js cone is centered on its height; the builder's has its base at y = 0. The axis (+Y) is turned by
        // <paramref name="pitchDegrees"/> about X, so the base sits half a height behind the center along it.
        private static void AddCone(LowPolyMeshBuilder builder, Vector3 center, float pitchDegrees, float radius, float height,
            int segments, Color32 color)
        {
            Quaternion pitch = Quaternion.Euler(pitchDegrees, 0f, 0f);
            Vector3 baseCenter = center - pitch * Vector3.up * (height * 0.5f);
            builder.AddCone(LowPolyMeshBuilder.Place(baseCenter, new Vector3(pitchDegrees, 0f, 0f), new Vector3(radius, height, radius)), segments, color);
        }
    }
}
