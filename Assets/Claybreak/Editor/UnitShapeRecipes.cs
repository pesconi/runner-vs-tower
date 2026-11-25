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
        /// <summary>Fixed white (bunny tail, frog eye bulbs, penguin belly): never takes the skin.</summary>
        public static readonly Color32 White = new Color32(0xfb, 0xf7, 0xf0, 0);

        /// <summary>Where the leader's crown sits on the Coelhinho: on the head between the ears, not on the ear tips.</summary>
        public const float BunnyHeadTop = Radius * 1.9f;

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
                AddEyes(builder, r * 1.05f, EyeDepth);
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
        /// Pintinho: an egg-shaped body (RT-6t.20: taller and narrower than the Bolinha, so the two read apart from afar;
        /// before, a round ball of the Bolinha's size), two eyes, an orange cone beak on the front, a bigger two-ball orange
        /// crest on top and a small body-colored cone tail pointing back and up.
        /// Triangles: body 16×11 (320), eyes (72), beak 8 sides with base (16), crest 6×4 (2 × 36), tail 6 sides with base
        /// (12) = 492; 420 without eyes.
        /// </summary>
        public static Mesh Pintinho(bool eyes)
        {
            const float r = Radius;
            var builder = new LowPolyMeshBuilder();
            builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, r * 1.15f, 0f), new Vector3(r * 0.92f, r * 1.15f, r * 0.95f)), 16, 11, Body);
            if (eyes)
            {
                AddEyes(builder, r * 1.45f, EyeDepth);
            }

            // Reference: cone(0.05, 0.11, 8) at (0, 0.95R, −1.05R), rotated −90° about X (apex to the front); raised with the egg.
            AddCone(builder, new Vector3(0f, r * 1.2f, r * 1.0f), 90f, 0.05f, 0.11f, 8, Orange);
            builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, r * 2.3f + 0.02f, 0f), Vector3.one * 0.06f), SmallSegments, SmallBands, Orange);
            builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, r * 2.22f, -0.065f), Vector3.one * 0.05f), SmallSegments, SmallBands, Orange);
            // Reference: cone(0.06, 0.12, 6) in the body color at (0, 1.2R, 0.95R), rotated 1.1 rad about X (back and up).
            AddCone(builder, new Vector3(0f, r * 1.3f, -r * 0.9f), -1.1f * Mathf.Rad2Deg, 0.06f, 0.12f, 6, Body);
            return builder.ToMesh(eyes ? "Pintinho" : "PintinhoNoEyes");
        }

        /// <summary>
        /// Coelhinho (RT-6t.20): a round body, two long upright ears leaning out a little (from afar, the only shape taller
        /// than wide by its ears), two eyes and a white pompom tail.
        /// Triangles: body 14×10 (252), eyes (72), ears 8×5 (2 × 64), tail 6×4 (36) = 488; 416 without eyes.
        /// </summary>
        public static Mesh Coelhinho(bool eyes)
        {
            const float r = Radius;
            var builder = new LowPolyMeshBuilder();
            builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, r * 0.95f, 0f), new Vector3(r, r * 0.95f, r * 0.95f)), 14, 10, Body);
            if (eyes)
            {
                AddEyes(builder, r * 1.1f, EyeDepth);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(side * 0.075f, BunnyHeadTop + 0.12f, -0.02f),
                    new Vector3(0f, 0f, -side * 12f), new Vector3(0.045f, 0.16f, 0.03f)), 8, 5, Body);
            }

            builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, r * 0.7f, -r * 0.95f), Vector3.one * 0.06f), SmallSegments, SmallBands, White);
            return builder.ToMesh(eyes ? "Coelhinho" : "CoelhinhoNoEyes");
        }

        /// <summary>
        /// Sapinho (RT-6t.20): a flat, wide body (the lowest and widest silhouette) with two white eye bulbs on top at the
        /// front and dark pupils looking forward.
        /// Triangles: body 16×10 (288), eye bulbs 8×5 (2 × 64), pupils 6×4 (2 × 36) = 488; 416 without eyes (the bulbs stay).
        /// </summary>
        public static Mesh Sapinho(bool eyes)
        {
            const float r = Radius;
            const float across = 0.085f;
            const float bulbZ = r * 0.6f;
            var builder = new LowPolyMeshBuilder();
            builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, r * 0.62f, 0f), new Vector3(r * 1.18f, r * 0.62f, r)), 16, 10, Body);
            for (int side = -1; side <= 1; side += 2)
            {
                builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(side * across, r * 1.2f, bulbZ), Vector3.one * 0.065f), 8, 5, White);
            }

            if (eyes)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(side * across, r * 1.25f, bulbZ + 0.05f), Vector3.one * 0.03f),
                        SmallSegments, SmallBands, Eyes);
                }
            }

            return builder.ToMesh(eyes ? "Sapinho" : "SapinhoNoEyes");
        }

        /// <summary>
        /// Pinguim (RT-6t.20): a tall oval body (the tallest, taller than wide without ears), a white belly, two flippers held
        /// well out (the widest shape after the frog), two eyes and an orange beak.
        /// Triangles: body 14×10 (252), belly 8×5 (64), eyes (72), beak 8 sides with base (16), flippers 6×4 (2 × 36) = 476;
        /// 404 without eyes.
        /// </summary>
        public static Mesh Pinguim(bool eyes)
        {
            const float r = Radius;
            var builder = new LowPolyMeshBuilder();
            builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, r * 1.4f, 0f), new Vector3(r * 0.82f, r * 1.4f, r * 0.8f)), 14, 10, Body);
            builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, r * 1.15f, r * 0.4f), new Vector3(r * 0.62f, r * 1.05f, r * 0.55f)), 8, 5, White);
            if (eyes)
            {
                AddEyes(builder, r * 1.95f, r * 0.68f);
            }

            AddCone(builder, new Vector3(0f, r * 1.72f, r * 0.78f), 90f, 0.05f, 0.1f, 8, Orange);
            for (int side = -1; side <= 1; side += 2)
            {
                builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(side * r * 0.85f, r * 1.15f, 0f), new Vector3(0f, 0f, side * 35f),
                    new Vector3(0.025f, 0.12f, 0.06f)), SmallSegments, SmallBands, Body);
            }

            return builder.ToMesh(eyes ? "Pinguim" : "PinguimNoEyes");
        }

        // ---------- Bibopzi (2026-11-25, the studio's character in the "Feijão" direction, the default shape) ----------

        /// <summary>Bibopzi's fixed orange (gear, spring, hands, feet): #FF8A3D of the character sheet.</summary>
        public static readonly Color32 BibopziOrange = new Color32(0xff, 0x8a, 0x3d, 0);
        /// <summary>Bibopzi's cream belly: #FFF1DC, never the skin.</summary>
        public static readonly Color32 Cream = new Color32(0xff, 0xf1, 0xdc, 0);
        /// <summary>Brows and the slot of the screw nose (darker than the eye diamonds, so the eyeless variant has no eye color).</summary>
        public static readonly Color32 Brow = new Color32(0x1d, 0x2b, 0x3a, 0);
        /// <summary>The screw nose.</summary>
        public static readonly Color32 Steel = new Color32(0xc8, 0xce, 0xd6, 0);

        // The bean body: one piece from y = BibopziBottom up BibopziBodyHeight, a superellipse profile (fuller sides than a
        // sphere, round top and bottom), half-width BaseHalfWidth at the base narrowing to HeadHalfWidth at the head (the
        // slightly chamfered shoulder is the step between them), depth BibopziDepth of the width. Proportions from the
        // reference drawing (personagem/ia/2026-11/2026-11-25_bibopzi_direcoes-2d.svg, column A) at 0.0018 m per pixel.
        private const float BibopziBottom = 0.03f;
        private const float BibopziBodyHeight = 0.594f;
        private const float BaseHalfWidth = 0.18f;
        private const float HeadHalfWidth = 0.15f;
        private const float BibopziDepth = 0.82f;
        // Superellipse exponent of the profile (2 = the sphere itself; higher = straighter sides).
        private const float Fullness = 2.78f;

        /// <summary>Top of Bibopzi's head (the leader's crown sits there, the spring goes through it).</summary>
        public const float BibopziHeadTop = BibopziBottom + BibopziBodyHeight;

        /// <summary>
        /// Bibopzi, the baby robot of the studio ("Feijão" direction, D3 of the character, user's pick on 2026-11-25): a
        /// one-piece bean body that takes the skin, a cream belly, two eyes (white with a dark diamond) under brows, a small
        /// screw nose, an orange gear on HIS right temple (+X: the viewer's left from the front) facing out like an ear so the
        /// run camera behind the crowd sees it, an orange spring topknot, two loose orange ball hands (no arms) and two flat
        /// orange feet that stick out at the front. The landmarks (spring, gear, belly, loose hands) keep it apart from other
        /// bean-shaped characters.
        /// Triangles: body 10×9 (160), belly 8×4 (48), eye whites 8-gon discs (2 × 8), diamonds (2 × 4), brows (2 × 4), nose
        /// 6 + slot 4 (10), gear hexagonal hub with caps (24) + 3 tooth bars (36), spring 3 open 4-sided tubes (24) + ball
        /// 6×4 (36), hands 6×4 (2 × 36), feet 6×3 (2 × 24) = 490; 458 without eyes (no whites, diamonds or brows).
        /// </summary>
        public static Mesh Bibopzi(bool eyes)
        {
            var builder = new LowPolyMeshBuilder();
            // Body: a unit sphere bent into the bean afterwards (the kit's builder has no lathe); its vertices come first.
            builder.AddSphere(Matrix4x4.identity, 10, 9, Body);
            int bodyVertices = builder.VertexCount;

            // Belly: a flat ellipsoid sunk into the front, low on the body, bulging a little.
            builder.AddSphere(OnBibopzi(0.31f, FrontAzimuth(0.31f, 0f), -0.026f, new Vector3(0.112f, 0.044f, 0.1f)), 8, 4, Cream);

            if (eyes)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    float eyeAzimuth = FrontAzimuth(0.74f, side * 0.063f);
                    builder.AddDisc(OnBibopzi(0.74f, eyeAzimuth, 0.002f, new Vector3(0.04f, 1f, 0.032f)), 8, White);
                    builder.AddDisc(OnBibopzi(0.735f, eyeAzimuth, 0.004f, new Vector3(0.014f, 1f, 0.02f)), 4, Eyes);
                    builder.AddDisc(OnBibopzi(0.86f, FrontAzimuth(0.86f, side * 0.066f), 0.003f, new Vector3(0.034f, 1f, 0.007f),
                        side * 12f), 4, Brow);
                }
            }

            // Screw nose: a hexagonal head with a slanted slot.
            float noseAzimuth = FrontAzimuth(0.63f, 0f);
            builder.AddDisc(OnBibopzi(0.63f, noseAzimuth, 0.003f, new Vector3(0.017f, 1f, 0.017f)), 6, Steel);
            builder.AddDisc(OnBibopzi(0.63f, noseAzimuth, 0.005f, new Vector3(0.014f, 1f, 0.0035f), 45f), 4, Brow);

            // Gear on his right temple (+X), turned a little to the front: a hexagonal hub and three bars = six teeth.
            const float gearT = 0.73f;
            const float gearAzimuth = 25f;
            builder.AddCylinder(OnBibopzi(gearT, gearAzimuth, 0.014f, new Vector3(0.048f, 0.04f, 0.048f)), 6, BibopziOrange);
            for (int tooth = 0; tooth < 3; tooth++)
            {
                builder.AddBox(OnBibopzi(gearT, gearAzimuth, 0.014f, new Vector3(0.14f, 0.034f, 0.026f), 30f + tooth * 60f), BibopziOrange);
            }

            // Spring topknot: a zigzag of three open tubes out of the head, with a ball on top (front view: an S).
            Vector3[] coil =
            {
                new Vector3(0f, BibopziHeadTop - 0.012f, 0f), new Vector3(0.026f, BibopziHeadTop + 0.038f, 0f),
                new Vector3(-0.022f, BibopziHeadTop + 0.08f, 0f), new Vector3(0.014f, BibopziHeadTop + 0.118f, 0f)
            };
            for (int i = 0; i < coil.Length - 1; i++)
            {
                builder.AddCylinder(Tube(coil[i], coil[i + 1], 0.012f), 4, BibopziOrange, false);
            }

            builder.AddSphere(LowPolyMeshBuilder.Place(coil[coil.Length - 1] + new Vector3(0f, 0.014f, 0f), Vector3.one * 0.024f),
                SmallSegments, SmallBands, BibopziOrange);

            // Loose ball hands beside the base (a clear gap to the body), and flat feet sticking out at the front.
            for (int side = -1; side <= 1; side += 2)
            {
                builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(side * 0.25f, BibopziBottom + 0.38f * BibopziBodyHeight, 0.01f),
                    Vector3.one * 0.045f), SmallSegments, SmallBands, BibopziOrange);
                // Turned a quarter so a ring vertex points straight forward (6 segments have none on +Z otherwise).
                builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(side * 0.08f, 0.032f, 0.12f), new Vector3(0f, 90f, 0f), new Vector3(0.102f, 0.032f, 0.07f)),
                    SmallSegments, 3, BibopziOrange);
            }

            Mesh mesh = builder.ToMesh(eyes ? "Bibopzi" : "BibopziNoEyes");
            BendIntoBean(mesh, bodyVertices);
            return mesh;
        }

        // Moves the first bodyVertices (a unit sphere) onto the bean and gives them the bean's normals.
        private static void BendIntoBean(Mesh mesh, int bodyVertices)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            float squareness = 2f / Fullness;
            for (int i = 0; i < bodyVertices; i++)
            {
                Vector3 unit = vertices[i];
                float across = Mathf.Sqrt(unit.x * unit.x + unit.z * unit.z);
                float t = (Mathf.Sign(unit.y) * Mathf.Pow(Mathf.Abs(unit.y), squareness) + 1f) * 0.5f;
                float radius = Mathf.Pow(across, squareness) * BibopziHalfWidth(t);
                Vector3 p = across > 1e-6f
                    ? new Vector3(unit.x / across * radius, 0f, unit.z / across * radius * BibopziDepth)
                    : Vector3.zero;
                p.y = BibopziBottom + t * BibopziBodyHeight;
                vertices[i] = p;
                normals[i] = BibopziNormal(p);
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.RecalculateBounds();
        }

        private static float BibopziHalfWidth(float t)
        {
            float k = Mathf.Clamp01((t - 0.45f) / 0.4f);
            return Mathf.Lerp(BaseHalfWidth, HeadHalfWidth, k * k * (3f - 2f * k));
        }

        // The bean's surface is where this is 0 (negative inside).
        private static float BibopziField(Vector3 p)
        {
            float t = (p.y - BibopziBottom) / BibopziBodyHeight;
            float w = BibopziHalfWidth(Mathf.Clamp01(t));
            float x = p.x / w;
            float z = p.z / (w * BibopziDepth);
            return Mathf.Pow(Mathf.Sqrt(x * x + z * z), Fullness) + Mathf.Pow(Mathf.Abs(2f * t - 1f), Fullness) - 1f;
        }

        private static Vector3 BibopziNormal(Vector3 p)
        {
            const float h = 1e-4f;
            var gradient = new Vector3(
                BibopziField(p + new Vector3(h, 0f, 0f)) - BibopziField(p - new Vector3(h, 0f, 0f)),
                BibopziField(p + new Vector3(0f, h, 0f)) - BibopziField(p - new Vector3(0f, h, 0f)),
                BibopziField(p + new Vector3(0f, 0f, h)) - BibopziField(p - new Vector3(0f, 0f, h)));
            return gradient.sqrMagnitude > 0f ? gradient.normalized : Vector3.up;
        }

        // The bean's ring radius (before width and depth) at height fraction t.
        private static float BibopziRing(float t)
        {
            return Mathf.Pow(1f - Mathf.Pow(Mathf.Abs(2f * t - 1f), Fullness), 1f / Fullness);
        }

        /// <summary>Point of the bean's surface at height fraction t (0 base, 1 top) and azimuth (0 = +X, 90 = +Z front).</summary>
        private static Vector3 BibopziSurface(float t, float azimuthDegrees)
        {
            float radius = BibopziRing(t) * BibopziHalfWidth(t);
            float a = azimuthDegrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(a) * radius, BibopziBottom + t * BibopziBodyHeight, Mathf.Sin(a) * radius * BibopziDepth);
        }

        // Azimuth of the front surface point at height fraction t that is x across.
        private static float FrontAzimuth(float t, float x)
        {
            return Mathf.Acos(Mathf.Clamp(x / (BibopziRing(t) * BibopziHalfWidth(t)), -1f, 1f)) * Mathf.Rad2Deg;
        }

        // A primitive lying on the bean: local +Y along the surface normal (lift along it), local +X across, local +Z down
        // the surface; spin turns it about the normal (positive raises its +X end).
        private static Matrix4x4 OnBibopzi(float t, float azimuthDegrees, float lift, Vector3 scale, float spinDegrees = 0f)
        {
            Vector3 p = BibopziSurface(t, azimuthDegrees);
            Vector3 n = BibopziNormal(p);
            Quaternion q = Quaternion.LookRotation(n, Vector3.up) * Quaternion.Euler(90f, 0f, 0f) * Quaternion.Euler(0f, spinDegrees, 0f);
            return Matrix4x4.TRS(p + n * lift, q, scale);
        }

        // A tube of the given radius from a to b, a little longer at both ends so the zigzag has no gaps at the corners.
        private static Matrix4x4 Tube(Vector3 a, Vector3 b, float radius)
        {
            Vector3 along = b - a;
            return Matrix4x4.TRS((a + b) * 0.5f, Quaternion.FromToRotation(Vector3.up, along),
                new Vector3(radius, along.magnitude + radius, radius));
        }

        /// <summary>Blob shadow disc under a unit: radius 0.95 R, 12 sides, facing up.</summary>
        public static Mesh ShadowDisc()
        {
            return new LowPolyMeshBuilder()
                .AddDisc(LowPolyMeshBuilder.Place(Vector3.zero, new Vector3(Radius * 0.95f, 1f, Radius * 0.95f)), 12, Body)
                .ToMesh("BlobShadowDisc");
        }

        private static void AddEyes(LowPolyMeshBuilder builder, float height, float depth)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                builder.AddSphere(LowPolyMeshBuilder.Place(new Vector3(side * EyeAcross, height, depth), Vector3.one * EyeRadius),
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
