using Claybreak.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    public sealed class UnitShapeRecipeTests
    {
        [Test]
        public void ProvisionalBolinhaSitsAtTheTriangleCeiling()
        {
            Mesh mesh = UnitShapeRecipes.ProvisionalBolinha();
            int triangles = (int)(mesh.GetIndexCount(0) / 3);
            Assert.AreEqual(474, triangles);
            Assert.LessOrEqual(triangles, UnitShapeRecipes.MaximumTriangles);
            Assert.AreEqual(1, mesh.subMeshCount);
            Assert.AreEqual(IndexFormat.UInt16, mesh.indexFormat);
            Assert.AreEqual(0f, mesh.bounds.min.y, 1e-3f, "the body rests on the ground");
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void BodyTakesTheTintAndDetailsKeepTheirColor()
        {
            Mesh mesh = UnitShapeRecipes.ProvisionalBolinha();
            int tinted = 0;
            int fixedColor = 0;
            foreach (Color32 c in mesh.colors32)
            {
                if (c.a == 255)
                {
                    tinted++;
                }
                else if (c.a == 0)
                {
                    fixedColor++;
                }
            }

            Assert.Greater(tinted, 0);
            Assert.Greater(fixedColor, 0);
            Assert.AreEqual(mesh.vertexCount, tinted + fixedColor);
            Object.DestroyImmediate(mesh);
        }
    }
}
