using System.Collections.Generic;
using Claybreak.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    public sealed class UnitShapeRecipeTests
    {
        private readonly List<Mesh> made = new List<Mesh>();

        [TearDown]
        public void Release()
        {
            foreach (Mesh mesh in made)
            {
                Object.DestroyImmediate(mesh);
            }

            made.Clear();
        }

        [TestCase(true, 484)]
        [TestCase(false, 412)]
        public void BolinhaTriangles(bool eyes, int expected)
        {
            Assert.AreEqual(expected, Triangles(Keep(UnitShapeRecipes.Bolinha(eyes))));
        }

        [TestCase(true, 492)]
        [TestCase(false, 420)]
        public void PintinhoTriangles(bool eyes, int expected)
        {
            Assert.AreEqual(expected, Triangles(Keep(UnitShapeRecipes.Pintinho(eyes))));
        }

        [Test]
        public void EveryShapePassesTheValidatorRules()
        {
            foreach (UnitShapeAssets.Entry entry in UnitShapeAssets.All)
            {
                Mesh mesh = Keep(entry.Build());
                Assert.AreEqual(entry.Id, mesh.name);
                Assert.AreEqual(IndexFormat.UInt16, mesh.indexFormat);
                CollectionAssert.IsEmpty(UnitShapeRules.Check(mesh, entry.HasEyes, entry.FrontMarker), entry.Id);
            }
        }

        [Test]
        public void TheRulesCatchABrokenShape()
        {
            // Eyes declared but missing, and eyes where the variant should have none.
            Assert.IsNotEmpty(UnitShapeRules.Check(Keep(UnitShapeRecipes.Bolinha(false)), true, UnitShapeRecipes.Eyes));
            Assert.IsNotEmpty(UnitShapeRules.Check(Keep(UnitShapeRecipes.Bolinha(true)), false, null));

            // A shape still facing the three.js front (−Z) fails the front rule.
            Mesh backwards = Keep(UnitShapeRecipes.Pintinho(true));
            Vector3[] vertices = backwards.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].z = -vertices[i].z;
            }

            backwards.vertices = vertices;
            backwards.RecalculateBounds();
            List<string> problems = UnitShapeRules.Check(backwards, true, UnitShapeRecipes.Orange);
            Assert.That(problems, Has.Some.Contains("+Z"));

            // Over the budget: the same shape with every triangle drawn twice.
            Mesh big = Keep(UnitShapeRecipes.Bolinha(false));
            int[] triangles = big.triangles;
            var doubled = new int[triangles.Length * 2];
            triangles.CopyTo(doubled, 0);
            triangles.CopyTo(doubled, triangles.Length);
            big.triangles = doubled;
            Assert.That(UnitShapeRules.Check(big, false, null), Has.Some.Contains("budget"));
        }

        [Test]
        public void BodyTakesTheTintAndDetailsKeepTheirColor()
        {
            Mesh mesh = Keep(UnitShapeRecipes.Pintinho(true));
            int tinted = 0;
            int fixedColor = 0;
            foreach (Color32 c in mesh.colors32)
            {
                tinted += c.a == 255 ? 1 : 0;
                fixedColor += c.a == 0 ? 1 : 0;
            }

            Assert.Greater(tinted, 0);
            Assert.Greater(fixedColor, 0);
            Assert.AreEqual(mesh.vertexCount, tinted + fixedColor);
        }

        [Test]
        public void PintinhoMatchesTheReferenceMirroredToPlusZ()
        {
            // RT-6t.20: the egg-shaped Pintinho keeps the reference's parts and their mirror to +Z, raised with the egg.
            const float r = UnitShapeRecipes.Radius;
            Mesh mesh = Keep(UnitShapeRecipes.Pintinho(true));
            Vector3[] vertices = mesh.vertices;
            Color32[] colors = mesh.colors32;

            // Beak apex: center (0, 1.2R, +1.0R) + half its height (0.055) toward the front.
            Vector3 beakTip = Extreme(vertices, colors, UnitShapeRecipes.Orange, v => v.z);
            Assert.AreEqual(0f, beakTip.x, 1e-4f);
            Assert.AreEqual(r * 1.2f, beakTip.y, 1e-4f);
            Assert.AreEqual(r * 1.0f + 0.055f, beakTip.z, 1e-4f);

            // Crest top: ball 0.06 at (0, 2.3R + 0.02, 0).
            Vector3 crestTop = Extreme(vertices, colors, UnitShapeRecipes.Orange, v => v.y);
            Assert.AreEqual(r * 2.3f + 0.02f + 0.06f, crestTop.y, 1e-4f);

            // Tail (body color, behind the egg): apex at (0, 1.3R, −0.9R) + 0.06 along the axis rotated 1.1 rad about X,
            // mirrored: (0, cos 1.1, −sin 1.1).
            Vector3 tailTip = Extreme(vertices, colors, UnitShapeRecipes.Body, v => -v.z);
            Assert.AreEqual(r * 1.3f + 0.06f * Mathf.Cos(1.1f), tailTip.y, 1e-4f);
            Assert.AreEqual(-(r * 0.9f + 0.06f * Mathf.Sin(1.1f)), tailTip.z, 1e-4f);

            // Eyes: centers (±0.08, 1.45R, +0.86R); both balls together are centered across.
            Vector3 eyes = Centroid(vertices, colors, UnitShapeRecipes.Eyes);
            Assert.AreEqual(0f, eyes.x, 1e-4f);
            Assert.AreEqual(r * 1.45f, eyes.y, 1e-4f);
            Assert.AreEqual(r * 0.86f, eyes.z, 1e-4f);
        }

        [Test]
        public void BolinhaMatchesTheReference()
        {
            const float r = UnitShapeRecipes.Radius;
            Mesh mesh = Keep(UnitShapeRecipes.Bolinha(true));
            Assert.AreEqual(0f, mesh.bounds.min.y, 1e-4f);
            Assert.AreEqual(r, mesh.bounds.max.x, 0.005f, "ball radius R across (16×11 has no ring on the equator)");
            Vector3 leafTop = Extreme(mesh.vertices, mesh.colors32, UnitShapeRecipes.Leaf, v => v.y);
            Assert.AreEqual(r * 1.8f + 0.085f + 0.015f, leafTop.y, 1e-4f);
            Vector3 eyes = Centroid(mesh.vertices, mesh.colors32, UnitShapeRecipes.Eyes);
            Assert.AreEqual(r * 1.05f, eyes.y, 1e-4f);
            Assert.AreEqual(r * 0.86f, eyes.z, 1e-4f);
        }

        // 2026-11-25: Bibopzi's landmarks (what keeps it apart from other bean characters) are in the mesh, where the reference
        // drawing has them: the gear on HIS right temple (+X), the spring above the head, the cream belly low on the front,
        // hands loose beside the body, flat feet at the front, a bean wider at the base than at the head.
        [Test]
        public void BibopziHasItsLandmarks()
        {
            Mesh mesh = Keep(UnitShapeRecipes.Bibopzi(true));
            Vector3[] v = mesh.vertices;
            Color32[] c = mesh.colors32;
            Color32 orange = UnitShapeRecipes.BibopziOrange;
            float headTop = UnitShapeRecipes.BibopziHeadTop;

            Assert.AreEqual(0f, mesh.bounds.min.y, 1e-4f, "stands on the ground");
            Vector3 springTop = Extreme(v, c, orange, p => p.y);
            Assert.Greater(springTop.y, headTop + 0.1f, "the spring stands out of the head");

            // Gear: the orange around the temple height, all on +X.
            Vector3 gear = CentroidWhere(v, c, orange, p => p.y > headTop * 0.6f && p.y < headTop);
            Assert.Greater(gear.x, 0.1f, "the gear is on his right temple (+X, the viewer's left)");
            Assert.Less(Extreme(v, c, orange, p => -p.x).x, -0.2f, "the left hand is there too");

            // Belly: cream, on the front, below the eyes.
            Vector3 belly = Centroid(v, c, UnitShapeRecipes.Cream);
            Vector3 eyes = Centroid(v, c, UnitShapeRecipes.Eyes);
            Assert.Greater(belly.z, 0.1f);
            Assert.Less(belly.y, eyes.y - 0.15f);
            Assert.AreEqual(0f, eyes.x, 1e-3f, "both eyes, centered");

            // Hands: a gap between the body and each hand at the hand's height.
            float handInner = Extreme(v, c, orange, p => p.x).x - 0.09f;
            float bodyHalf = 0f;
            for (int i = 0; i < v.Length; i++)
            {
                if (c[i].a == 255)
                {
                    bodyHalf = Mathf.Max(bodyHalf, Mathf.Abs(v[i].x));
                }
            }

            Assert.Greater(handInner - bodyHalf, 0.01f, "the hands float loose, no arms");

            // A bean: wider at the base than at the head.
            Assert.Greater(BodyHalfWidthNear(v, c, headTop * 0.3f), BodyHalfWidthNear(v, c, headTop * 0.85f) + 0.02f);
            Assert.IsEmpty(UnitShapeRules.Check(mesh, true, orange));
        }

        private static float BodyHalfWidthNear(Vector3[] v, Color32[] c, float y)
        {
            float half = 0f;
            for (int i = 0; i < v.Length; i++)
            {
                if (c[i].a == 255 && Mathf.Abs(v[i].y - y) < 0.05f)
                {
                    half = Mathf.Max(half, Mathf.Abs(v[i].x));
                }
            }

            return half;
        }

        private static Vector3 CentroidWhere(Vector3[] vertices, Color32[] colors, Color32 color, System.Func<Vector3, bool> where)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                Color32 c = colors[i];
                if (c.r == color.r && c.g == color.g && c.b == color.b && c.a == color.a && where(vertices[i]))
                {
                    sum += vertices[i];
                    count++;
                }
            }

            Assert.Greater(count, 0, "no vertex of that color there");
            return sum / count;
        }

        private Mesh Keep(Mesh mesh)
        {
            made.Add(mesh);
            return mesh;
        }

        private static int Triangles(Mesh mesh)
        {
            return (int)(mesh.GetIndexCount(0) / 3);
        }

        private static Vector3 Centroid(Vector3[] vertices, Color32[] colors, Color32 color)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                Color32 c = colors[i];
                if (c.r == color.r && c.g == color.g && c.b == color.b && c.a == color.a)
                {
                    sum += vertices[i];
                    count++;
                }
            }

            Assert.Greater(count, 0, "no vertex of that color");
            return sum / count;
        }

        private static Vector3 Extreme(Vector3[] vertices, Color32[] colors, Color32 color, System.Func<Vector3, float> score)
        {
            Vector3 best = Vector3.zero;
            float bestScore = float.NegativeInfinity;
            for (int i = 0; i < vertices.Length; i++)
            {
                Color32 c = colors[i];
                if (c.r == color.r && c.g == color.g && c.b == color.b && c.a == color.a && score(vertices[i]) > bestScore)
                {
                    bestScore = score(vertices[i]);
                    best = vertices[i];
                }
            }

            Assert.IsFalse(float.IsNegativeInfinity(bestScore), "no vertex of that color");
            return best;
        }
    }
}
