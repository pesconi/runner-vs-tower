using System.Collections.Generic;
using Claybreak.Editor;
using JungleDegree.StudioKit.Toon;
using NUnit.Framework;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6v3.4a: the three Bibopzi levels written by Blender (Tools/blender/bibopzi.py) and read from their simple data, so
    /// none of this needs Blender installed. Landmarks on every level (direcao-visual-v3.md 3.1 and D36: spring topknot,
    /// gear on HIS right temple, cream belly, loose hands, bigger eyes, wind-up key on the back), the UV2 marks the kit's
    /// animated layout reads, axis, scale, winding and the budgets of the style guide.
    /// </summary>
    public sealed class BibopziLevelTests
    {
        private static readonly string[] Levels = { "detail", "crowd", "reserve" };
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

        [TestCase("detail")]
        [TestCase("crowd")]
        [TestCase("reserve")]
        public void TheDataIsTheScriptsCurrentOutput(string level)
        {
            CollectionAssert.IsEmpty(Data(level).Problems());
        }

        [TestCase("detail")]
        [TestCase("crowd")]
        [TestCase("reserve")]
        public void BibopziHasItsLandmarks(string level)
        {
            SimpleMeshData data = Data(level);
            Mesh mesh = Keep(data.ToMesh());
            Vector3[] v = mesh.vertices;
            Color32[] c = mesh.colors32;
            var marks = new List<Vector4>();
            mesh.GetUVs(1, marks);
            Color32 orange = UnitShapeRecipes.BibopziOrange;
            float headTop = UnitShapeRecipes.BibopziHeadTop;
            Assert.AreEqual(headTop, data.Landmarks.headTop, 1e-4f, "the script's head top is the game's crown seat");

            Assert.AreEqual(0f, mesh.bounds.min.y, 1e-4f, "stands on the ground");

            // Spring: orange, out of the head, its weight 0 at the head growing to 1 at the tip.
            float springTop = float.NegativeInfinity;
            float topWeight = 0f;
            var spring = new List<(float y, float w)>();
            for (int i = 0; i < v.Length; i++)
            {
                if (marks[i].x > 0f)
                {
                    Assert.IsTrue(Same(c[i], orange), "only the spring has a spring weight");
                    spring.Add((v[i].y, marks[i].x));
                    if (v[i].y > springTop)
                    {
                        springTop = v[i].y;
                        topWeight = marks[i].x;
                    }
                }
            }

            Assert.Greater(springTop, headTop + 0.1f, "the spring stands out of the head");
            Assert.AreEqual(1f, topWeight, 1e-3f, "the tip swings the most");
            foreach (var low in spring)
            {
                foreach (var high in spring)
                {
                    if (high.y > low.y + 0.005f)
                    {
                        Assert.LessOrEqual(low.w, high.w + 1e-3f, "the spring weight grows with the height");
                    }
                }
            }

            // Gear: the unmarked orange around the temple height, all on +X (his right).
            Vector3 gear = CentroidWhere(v, c, orange, (p, i) => marks[i] == Vector4.zero && p.y > headTop * 0.6f && p.y < headTop);
            Assert.Greater(gear.x, 0.1f, "the gear is on his right temple (+X, the viewer's left)");

            // Belly: cream, on the front, well below the eyes; eyes centered on the front.
            Vector3 belly = CentroidWhere(v, c, UnitShapeRecipes.Cream, (p, i) => true);
            Vector3 eyes = CentroidWhere(v, c, UnitShapeRecipes.Eyes, (p, i) => true);
            Assert.Greater(belly.z, 0.1f);
            Assert.Less(belly.y, eyes.y - 0.15f);
            Assert.AreEqual(0f, eyes.x, 2e-3f, "both eyes, centered");

            // Eyes (D36): bigger than the RT-6t.22 recipe's (0.08 m wide whites, a quarter of the head's width): 28% or more.
            float whiteLeft = float.PositiveInfinity;
            float whiteRight = float.NegativeInfinity;
            for (int i = 0; i < v.Length; i++)
            {
                if (Same(c[i], UnitShapeRecipes.White) && v[i].x > 0f)
                {
                    whiteLeft = Mathf.Min(whiteLeft, v[i].x);
                    whiteRight = Mathf.Max(whiteRight, v[i].x);
                }
            }

            float eyeWidth = whiteRight - whiteLeft;
            Assert.Greater(eyeWidth, 0.09f, "the eye whites grew (readable at 40 px)");
            Assert.Greater(eyeWidth, 0.28f * 2f * BodyHalfWidthNear(v, c, eyes.y), "an eye takes more of the head than the RT-6t.22 recipe (0.25)");

            // Hands: loose (a gap to the body), orange, and marked +1 on his right (+X) and -1 on his left for the kit.
            float bodyHalf = 0f;
            for (int i = 0; i < v.Length; i++)
            {
                if (c[i].a == 255)
                {
                    bodyHalf = Mathf.Max(bodyHalf, Mathf.Abs(v[i].x));
                }
            }

            int right = 0;
            int left = 0;
            for (int i = 0; i < v.Length; i++)
            {
                if (marks[i].y != 0f)
                {
                    Assert.IsTrue(Same(c[i], orange), "only the hands are hands");
                    Assert.AreEqual(Mathf.Sign(v[i].x), marks[i].y, "hand side = the side of the body");
                    Assert.Greater(Mathf.Abs(v[i].x) - bodyHalf, 0.01f, "the hands float loose, no arms");
                    if (marks[i].y > 0f)
                    {
                        right++;
                    }
                    else
                    {
                        left++;
                    }
                }
            }

            Assert.Greater(right, 0);
            Assert.Greater(left, 0);

            // Key (D36): orange, on the back, centered, a bow wide enough to read from behind, spinning about the script's
            // axis through a pivot behind the back.
            float back = 0f;
            for (int i = 0; i < v.Length; i++)
            {
                if (c[i].a == 255)
                {
                    back = Mathf.Min(back, v[i].z);
                }
            }

            Vector3 key = CentroidWhere(v, c, orange, (p, i) => marks[i].z == 1f);
            float keyHalfWidth = 0f;
            for (int i = 0; i < v.Length; i++)
            {
                if (marks[i].z > 0f)
                {
                    Assert.AreEqual(1f, marks[i].z, "the key turns whole");
                    Assert.IsTrue(Same(c[i], orange), "only the key spins");
                    keyHalfWidth = Mathf.Max(keyHalfWidth, Mathf.Abs(v[i].x));
                }
            }

            Assert.Less(key.z, back, "the key sticks out of the back");
            Assert.AreEqual(0f, key.x, 2e-3f, "the key is centered on the back");
            Assert.Greater(keyHalfWidth, 0.08f, "the bow is wide enough to read from the run camera");
            Assert.Less(data.KeyPivot.z, back, "the key turns behind the back");
            Assert.AreEqual(Vector3.forward, data.KeyAxis, "the key turns about the shaft (+Z)");
            Assert.IsNull(new ToonVinylMotion(new Vector3(0f, -0.12f, -0.1f), 0.07f, 0.04f, new Vector2(0.12f, 0.1f),
                data.KeyAxis, data.KeyPivot, 1).Validate(), "the kit accepts the key");

            // A bean: wider at the base than at the head.
            Assert.Greater(BodyHalfWidthNear(v, c, headTop * 0.3f), BodyHalfWidthNear(v, c, headTop * 0.85f) + 0.02f);

            UnitShapeAssets.Entry entry = System.Array.Find(UnitShapeAssets.All, e => e.DataPath == data.Path);
            Assert.IsNotNull(entry, data.Path);
            CollectionAssert.IsEmpty(UnitShapeRules.Check(mesh, true, orange, entry.Budget));
        }

        // Same character on every level: the same height, width and depth within two centimeters (axis and scale of the data;
        // the reserve's six-sided hands, feet and key lobes reach a little less far).
        [Test]
        public void TheLevelsShareAxisAndScale()
        {
            Bounds detail = Keep(Data("detail").ToMesh()).bounds;
            foreach (string level in Levels)
            {
                Bounds bounds = Keep(Data(level).ToMesh()).bounds;
                Assert.AreEqual(0f, bounds.min.y, 1e-4f, level);
                Assert.AreEqual(0f, bounds.center.x, 0.01f, level + " is centered across");
                Assert.That(bounds.max.y, Is.InRange(0.74f, 0.8f), level + " height (spring ball on top)");
                Assert.That(bounds.size.x, Is.InRange(0.55f, 0.62f), level + " width (hands)");
                Assert.AreEqual(detail.size.y, bounds.size.y, 0.02f, level);
                Assert.AreEqual(detail.size.x, bounds.size.x, 0.02f, level);
                Assert.AreEqual(detail.size.z, bounds.size.z, 0.02f, level);
            }
        }

        // Unity's front faces are clockwise (the script reverses Blender's winding): every face looks the way of its normals.
        [TestCase("detail")]
        [TestCase("crowd")]
        [TestCase("reserve")]
        public void TheFacesLookOutward(string level)
        {
            Mesh mesh = Keep(Data(level).ToMesh());
            Vector3[] v = mesh.vertices;
            Vector3[] n = mesh.normals;
            int[] t = mesh.triangles;
            int wrong = 0;
            int counted = 0;
            for (int i = 0; i < t.Length; i += 3)
            {
                Vector3 face = Vector3.Cross(v[t[i + 1]] - v[t[i]], v[t[i + 2]] - v[t[i]]);
                if (face.sqrMagnitude < 1e-14f)
                {
                    continue;
                }

                counted++;
                if (Vector3.Dot(face, n[t[i]] + n[t[i + 1]] + n[t[i + 2]]) <= 0f)
                {
                    wrong++;
                }
            }

            Assert.Greater(counted, 0);
            Assert.LessOrEqual(wrong, counted / 200, $"{wrong} of {counted} faces look inward");
        }

        // The marks the kit reads: spring 0..1, hand -1/0/+1, spin 0/1, face 0 until the atlas (RT-6v3.4b); UV0 in 0..1.
        [TestCase("detail")]
        [TestCase("crowd")]
        [TestCase("reserve")]
        public void TheMarksAndUvAreInRange(string level)
        {
            Mesh mesh = Keep(Data(level).ToMesh());
            var marks = new List<Vector4>();
            var uv = new List<Vector2>();
            mesh.GetUVs(1, marks);
            mesh.GetUVs(0, uv);
            Assert.AreEqual(mesh.vertexCount, marks.Count);
            Assert.AreEqual(mesh.vertexCount, uv.Count);
            foreach (Vector4 m in marks)
            {
                Assert.That(m.x, Is.InRange(0f, 1f));
                Assert.That(m.y == -1f || m.y == 0f || m.y == 1f, Is.True, $"hand side {m.y}");
                Assert.That(m.z == 0f || m.z == 1f, Is.True, $"spin weight {m.z}");
                Assert.AreEqual(0f, m.w, "no face mask before the atlas");
            }

            foreach (Vector2 p in uv)
            {
                Assert.That(p.x, Is.InRange(0f, 1f));
                Assert.That(p.y, Is.InRange(0f, 1f));
            }
        }

        // D36: smooth normals and round corners; the guide's edge rule (hard edges) and the level budgets agree with the guide.
        [TestCase("detail")]
        [TestCase("crowd")]
        [TestCase("reserve")]
        public void TheLevelsAreSmooth(string level)
        {
            Mesh mesh = Keep(Data(level).ToMesh());
            Assert.LessOrEqual(StyleFacts.MaxEdgeAngle(mesh), 60f);
        }

        [Test]
        public void TheLevelBudgetsAreTheGuides()
        {
            StyleGuide guide = StyleGuideContent.Create();
            ClassStyle unit = guide.ClassOf(StyleClass.Unit);
            ClassStyle leader = guide.ClassOf(StyleClass.Leader);
            Assert.AreEqual(new Vector2Int(unit.v3MinTriangles, unit.v3MaxTriangles), UnitShapeAssets.Budget(UnitShapeAssets.ShapeLevel.Crowd));
            Assert.AreEqual(new Vector2Int(leader.v3MinTriangles, leader.v3MaxTriangles), UnitShapeAssets.Budget(UnitShapeAssets.ShapeLevel.Detail));
            Assert.AreEqual(new Vector2Int(unit.reserveMinTriangles, unit.reserveMaxTriangles), UnitShapeAssets.Budget(UnitShapeAssets.ShapeLevel.Reserve));
            Object.DestroyImmediate(guide);
        }

        // The crowd wears the crowd level (same asset id, so the catalog keeps its reference) and the leader the detailed one.
        [Test]
        public void TheCrowdWearsTheCrowdLevelAndTheLeaderTheDetail()
        {
            WorldContent.ShapeDefinition bibopzi = WorldContent.Shapes[0];
            Assert.AreEqual(UnitShapeAssets.ShapeLevel.Crowd, bibopzi.Asset.Level);
            Assert.AreEqual(UnitShapeAssets.ShapeLevel.Detail, bibopzi.LeaderAsset.Level);
            for (int i = 1; i < WorldContent.Shapes.Length; i++)
            {
                Assert.IsNull(WorldContent.Shapes[i].LeaderAsset, "only the Bibopzi has a detailed level (docs 3.1)");
            }
        }

        private static SimpleMeshData Data(string level)
        {
            return SimpleMeshData.Read($"{SimpleMeshData.OutputFolder}/bibopzi_{level}.json");
        }

        private Mesh Keep(Mesh mesh)
        {
            made.Add(mesh);
            return mesh;
        }

        private static bool Same(Color32 a, Color32 b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
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

        private static Vector3 CentroidWhere(Vector3[] v, Color32[] c, Color32 color, System.Func<Vector3, int, bool> where)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            for (int i = 0; i < v.Length; i++)
            {
                if (Same(c[i], color) && where(v[i], i))
                {
                    sum += v[i];
                    count++;
                }
            }

            Assert.Greater(count, 0, "no vertex of that color there");
            return sum / count;
        }
    }
}
