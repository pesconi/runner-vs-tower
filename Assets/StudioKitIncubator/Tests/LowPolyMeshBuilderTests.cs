using JungleDegree.StudioKit.Toon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace JungleDegree.StudioKit.Tests
{
    public sealed class LowPolyMeshBuilderTests
    {
        private static readonly Color32 Red = new Color32(255, 0, 0, 255);

        [Test]
        public void PrimitivesHaveTheDocumentedTriangleCounts()
        {
            Matrix4x4 unit = Matrix4x4.identity;
            Assert.AreEqual(2 * 16 * 10, new LowPolyMeshBuilder().AddSphere(unit, 16, 11, Red).TriangleCount);
            Assert.AreEqual(8, new LowPolyMeshBuilder().AddCone(unit, 8, Red, false).TriangleCount);
            Assert.AreEqual(16, new LowPolyMeshBuilder().AddCone(unit, 8, Red).TriangleCount);
            Assert.AreEqual(10, new LowPolyMeshBuilder().AddCylinder(unit, 5, Red, false).TriangleCount);
            Assert.AreEqual(20, new LowPolyMeshBuilder().AddCylinder(unit, 5, Red).TriangleCount);
            Assert.AreEqual(12, new LowPolyMeshBuilder().AddDisc(unit, 12, Red).TriangleCount);
            Assert.AreEqual(12, new LowPolyMeshBuilder().AddBox(unit, Red).TriangleCount);
            Assert.AreEqual(20, new LowPolyMeshBuilder().AddIcosahedron(unit, Red).TriangleCount);
            Assert.AreEqual(80, new LowPolyMeshBuilder().AddIcosahedron(unit, Red, 1).TriangleCount);
            Assert.AreEqual(180, new LowPolyMeshBuilder().AddIcosahedron(unit, Red, 2).TriangleCount);
        }

        [Test]
        public void SplitIcosahedronStaysOnTheUnitSphere()
        {
            Mesh mesh = new LowPolyMeshBuilder().AddIcosahedron(Matrix4x4.identity, Red, 2).ToMesh("icosphere");
            foreach (Vector3 vertex in mesh.vertices)
            {
                Assert.AreEqual(1f, vertex.magnitude, 1e-4f);
            }

            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void EveryTriangleFacesOutward()
        {
            Matrix4x4[] placements =
            {
                Matrix4x4.identity,
                LowPolyMeshBuilder.Place(new Vector3(1f, 2f, 3f), new Vector3(30f, 45f, 10f), new Vector3(0.5f, 2f, 1f)),
                LowPolyMeshBuilder.Place(Vector3.zero, new Vector3(-1f, 1f, 1f)),
            };
            foreach (Matrix4x4 placement in placements)
            {
                var builder = new LowPolyMeshBuilder()
                    .AddSphere(placement, 8, 6, Red)
                    .AddCone(placement, 7, Red)
                    .AddCylinder(placement, 6, Red)
                    .AddDisc(placement, 9, Red)
                    .AddBox(placement, Red)
                    .AddIcosahedron(placement, Red)
                    .AddIcosahedron(placement, Red, 1);
                AssertFacesOutward(builder.ToMesh("test"));
            }
        }

        [Test]
        public void MeshIsSixteenBitWithOneColorPerVertex()
        {
            var color = new Color32(10, 20, 30, 0);
            Mesh mesh = new LowPolyMeshBuilder().AddSphere(Matrix4x4.identity, 6, 4, color).ToMesh("colors");
            Assert.AreEqual(IndexFormat.UInt16, mesh.indexFormat);
            Assert.AreEqual(mesh.vertexCount, mesh.colors32.Length);
            foreach (Color32 c in mesh.colors32)
            {
                Assert.AreEqual(color, c);
            }

            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void PlacementScalesTheUnitShape()
        {
            Mesh mesh = new LowPolyMeshBuilder()
                .AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, 1f, 0f), new Vector3(2f, 0.5f, 3f)), 12, 8, Red)
                .ToMesh("ellipsoid");
            Bounds bounds = mesh.bounds;
            Assert.AreEqual(1f, bounds.center.y, 1e-4f);
            Assert.AreEqual(0.5f, bounds.extents.y, 1e-4f, "the poles sit exactly at the y radius");
            Assert.AreEqual(3f, bounds.extents.z, 0.05f);
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void RampTextureIsLinearPointAndClamped()
        {
            Texture2D ramp = ToonLook.CreateRampTexture(new[] { 110f / 255f, 185f / 255f, 1f });
            Assert.AreEqual(3, ramp.width);
            Assert.AreEqual(FilterMode.Point, ramp.filterMode);
            Assert.AreEqual(TextureWrapMode.Clamp, ramp.wrapMode);
            Object.DestroyImmediate(ramp);
        }

        [Test]
        public void LookRejectsDecreasingRamp()
        {
            ToonLook look = ScriptableObject.CreateInstance<ToonLook>();
            Assert.IsNull(look.Validate(), "the default look is valid");
            look.RampLevels[1] = 0.1f;
            Assert.IsNotNull(look.Validate());
            Object.DestroyImmediate(look);
        }

        private static void AssertFacesOutward(Mesh mesh)
        {
            Vector3[] v = mesh.vertices;
            Vector3[] n = mesh.normals;
            int[] t = mesh.triangles;
            for (int i = 0; i < t.Length; i += 3)
            {
                Vector3 face = Vector3.Cross(v[t[i + 1]] - v[t[i]], v[t[i + 2]] - v[t[i]]);
                Vector3 vertexNormals = n[t[i]] + n[t[i + 1]] + n[t[i + 2]];
                Assert.Greater(Vector3.Dot(face, vertexNormals), 0f, $"triangle {i / 3} is wound inward");
            }

            Object.DestroyImmediate(mesh);
        }
    }
}
