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
            const float r = UnitShapeRecipes.Radius;
            Mesh mesh = Keep(UnitShapeRecipes.Pintinho(true));
            Vector3[] vertices = mesh.vertices;
            Color32[] colors = mesh.colors32;

            // Beak apex: reference center (0, 0.95R, −1.05R) + half its height (0.055) toward the front.
            Vector3 beakTip = Extreme(vertices, colors, UnitShapeRecipes.Orange, v => v.z);
            Assert.AreEqual(0f, beakTip.x, 1e-4f);
            Assert.AreEqual(r * 0.95f, beakTip.y, 1e-4f);
            Assert.AreEqual(r * 1.05f + 0.055f, beakTip.z, 1e-4f);

            // Crest top: reference ball 0.045 at (0, 2.02R, 0).
            Vector3 crestTop = Extreme(vertices, colors, UnitShapeRecipes.Orange, v => v.y);
            Assert.AreEqual(r * 2.02f + 0.045f, crestTop.y, 1e-4f);

            // Tail (body color, behind the ball): apex at reference (0, 1.2R, 0.95R) + 0.06 along the axis rotated
            // 1.1 rad about X, mirrored: (0, cos 1.1, −sin 1.1).
            Vector3 tailTip = Extreme(vertices, colors, UnitShapeRecipes.Body, v => -v.z);
            Assert.AreEqual(r * 1.2f + 0.06f * Mathf.Cos(1.1f), tailTip.y, 1e-4f);
            Assert.AreEqual(-(r * 0.95f + 0.06f * Mathf.Sin(1.1f)), tailTip.z, 1e-4f);

            // Eyes: reference centers (±0.08, 1.15R, −0.86R); both balls together are centered across.
            Vector3 eyes = Centroid(vertices, colors, UnitShapeRecipes.Eyes);
            Assert.AreEqual(0f, eyes.x, 1e-4f);
            Assert.AreEqual(r * 1.15f, eyes.y, 1e-4f);
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
