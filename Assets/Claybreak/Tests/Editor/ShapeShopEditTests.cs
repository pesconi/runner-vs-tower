using System.Collections.Generic;
using Claybreak.Editor;
using JungleDegree.StudioKit.Swarm;
using NUnit.Framework;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.20 (D31 A, D32 A): the three new crowd shapes (budget, outline apart from the others), the hop bridge to the
    /// kit's swarm renderers, the shapes on offer with prices and hops, and the shop figures drawn from the meshes.
    /// </summary>
    public sealed class ShapeShopEditTests
    {
        private readonly List<Object> made = new List<Object>();

        [TearDown]
        public void Release()
        {
            foreach (Object item in made)
            {
                Object.DestroyImmediate(item);
            }

            made.Clear();
        }

        private Mesh Keep(Mesh mesh)
        {
            made.Add(mesh);
            return mesh;
        }

        private static int Triangles(Mesh mesh) => (int)(mesh.GetIndexCount(0) / 3);

        [TestCase(true, 488)]
        [TestCase(false, 416)]
        public void CoelhinhoTriangles(bool eyes, int expected)
        {
            Assert.AreEqual(expected, Triangles(Keep(UnitShapeRecipes.Coelhinho(eyes))));
        }

        [TestCase(true, 488)]
        [TestCase(false, 416)]
        public void SapinhoTriangles(bool eyes, int expected)
        {
            Assert.AreEqual(expected, Triangles(Keep(UnitShapeRecipes.Sapinho(eyes))));
        }

        [TestCase(true, 476)]
        [TestCase(false, 404)]
        public void PinguimTriangles(bool eyes, int expected)
        {
            Assert.AreEqual(expected, Triangles(Keep(UnitShapeRecipes.Pinguim(eyes))));
        }

        // No shape sold is heavier than the Pintinho (492, the device gate's worst case until now): the 3x gate stays valid.
        [Test]
        public void NoShapeOnOfferPassesThePintinhosTriangles()
        {
            foreach (WorldContent.ShapeDefinition shape in WorldContent.Shapes)
            {
                Assert.LessOrEqual(Triangles(Keep(shape.Asset.Build())), 492, shape.Id);
            }
        }

        // The user's criterion (D31 A): each shape reads from afar. At crowd distance mostly the outline is left, so every pair
        // of shapes must differ in height, width or height-to-width by at least 15%.
        [Test]
        public void EveryPairOfShapesHasADifferentOutline()
        {
            var sizes = new List<(string id, Vector3 size)>();
            foreach (WorldContent.ShapeDefinition shape in WorldContent.Shapes)
            {
                sizes.Add((shape.Id, Keep(shape.Asset.Build()).bounds.size));
            }

            for (int a = 0; a < sizes.Count; a++)
            {
                for (int b = a + 1; b < sizes.Count; b++)
                {
                    float height = Ratio(sizes[a].size.y, sizes[b].size.y);
                    float width = Ratio(sizes[a].size.x, sizes[b].size.x);
                    float aspect = Ratio(sizes[a].size.y / sizes[a].size.x, sizes[b].size.y / sizes[b].size.x);
                    float best = Mathf.Max(height, Mathf.Max(width, aspect));
                    Debug.Log($"[ShapeOutline] {sizes[a].id} vs {sizes[b].id}: height x{height:0.00}, width x{width:0.00}, aspect x{aspect:0.00}");
                    Assert.GreaterOrEqual(best, 1.15f, $"{sizes[a].id} and {sizes[b].id} have the same outline ({sizes[a].size} vs {sizes[b].size})");
                }
            }
        }

        private static float Ratio(float a, float b) => Mathf.Max(a, b) / Mathf.Max(1e-4f, Mathf.Min(a, b));

        // Each shape hops its own way (D31 A): no two shapes share the same hop speed and height.
        [Test]
        public void EveryShapeHopsItsOwnWay()
        {
            WorldContent.ShapeDefinition[] shapes = WorldContent.Shapes;
            for (int a = 0; a < shapes.Length; a++)
            {
                for (int b = a + 1; b < shapes.Length; b++)
                {
                    SwarmMotionSettings x = shapes[a].Motion;
                    SwarmMotionSettings y = shapes[b].Motion;
                    bool same = Mathf.Approximately(x.Frequency, y.Frequency) || Mathf.Approximately(x.HopHeight, y.HopHeight);
                    Assert.IsFalse(same, $"{shapes[a].Id} and {shapes[b].Id} hop alike");
                }
            }
        }

        // D32 A: BALL free, CHICK 300, BUNNY 1000, FROGGY 1600, PENGUIN 2400 (total 5300), in that order.
        [Test]
        public void PricesAreTheUsersDecision()
        {
            CollectionAssert.AreEqual(new[] { "ball", "chick", "bunny", "froggy", "penguin" },
                System.Array.ConvertAll(WorldContent.Shapes, s => s.Id));
            CollectionAssert.AreEqual(new[] { 0, 300, 1000, 1600, 2400 }, System.Array.ConvertAll(WorldContent.Shapes, s => s.Price));
        }

        // The bridge writes the kit renderers' serialized hop; it fails if the kit renames the field (the hop would silently stop).
        [Test]
        public void TheHopBridgeReachesBothKitRenderers()
        {
            var hop = new SwarmMotionSettings { Frequency = 6.5f, HopHeight = 0.42f, SquashBase = 0.7f, SquashAmount = 0.5f, ForwardLean = 0.08f };
            var host = new GameObject("bridge");
            made.Add(host);
            host.SetActive(false);
            SwarmRenderer swarm = host.AddComponent<SwarmRenderer>();
            LooseSwarmRenderer loose = host.AddComponent<LooseSwarmRenderer>();
            int capacity = swarm.Capacity;
            SwarmMotionBridge.Apply(swarm, hop);
            SwarmMotionBridge.Apply(loose, hop);
            foreach (MonoBehaviour renderer in new MonoBehaviour[] { swarm, loose })
            {
                SwarmMotionSettings read = SwarmMotionBridge.Read(renderer);
                string name = renderer.GetType().Name;
                Assert.AreEqual(hop.Frequency, read.Frequency, name);
                Assert.AreEqual(hop.HopHeight, read.HopHeight, name);
                Assert.AreEqual(hop.SquashBase, read.SquashBase, name);
                Assert.AreEqual(hop.SquashAmount, read.SquashAmount, name);
                Assert.AreEqual(hop.ForwardLean, read.ForwardLean, name);
                var so = new UnityEditor.SerializedObject(renderer);
                Assert.IsNotNull(so.FindProperty("motion.HopHeight"), $"the kit renamed {name}.motion: the bridge writes nothing");
                Assert.AreEqual(0.42f, so.FindProperty("motion.HopHeight").floatValue, 1e-6f, name);
            }

            Assert.AreEqual(capacity, swarm.Capacity, "only the hop may change");
        }

        // The figure is drawn from the mesh: both layers have pixels, the body layer is grey shading only (the shop tints it),
        // and the same mesh gives the same pixels (a rebuild never churns git).
        [Test]
        public void TheShopFigureIsDrawnFromTheMesh()
        {
            Mesh mesh = Keep(UnitShapeRecipes.Pinguim(true));
            ShapeFigures.Render(mesh, 64, out Color32[] body, out Color32[] detail);
            int bodyPixels = 0;
            int detailPixels = 0;
            foreach (Color32 c in body)
            {
                if (c.a > 0)
                {
                    bodyPixels++;
                    Assert.IsTrue(c.r == c.g && c.g == c.b, "the body layer is grey shading only");
                }
            }

            foreach (Color32 c in detail)
            {
                detailPixels += c.a > 0 ? 1 : 0;
            }

            Assert.Greater(bodyPixels, 64 * 64 / 10, "the body is too small in the figure");
            Assert.Greater(detailPixels, 64 * 4, "no outline or details");
            ShapeFigures.Render(mesh, 64, out Color32[] again, out _);
            CollectionAssert.AreEqual(body, again);
        }
    }
}
