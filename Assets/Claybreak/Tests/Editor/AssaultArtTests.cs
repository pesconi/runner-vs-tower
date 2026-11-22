using Claybreak.Editor;
using JungleDegree.StudioKit.Toon;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.10 (D28 = A): the committed assault materials are toon and take their colors from the palette's structure and
    /// defense colors (weights in 0..1 adding up to at most 1, the rest white); no Simple Lit is left; the toon primitives
    /// carry white vertex colors. Reads the assets as they are (does not rebuild them).
    /// </summary>
    public sealed class AssaultArtTests
    {
        private static readonly string[] ToonRoles =
        {
            "Stone", "StoneDark", "StoneLight", "StoneWarm", "Iron", "IronWarm", "Wood", "Banner", "Terracotta", "ClayDark",
            "Moss", "Bone", "Wall", "Health", "Line", "Crate", "Glow", "GlowHot", "WoodDark", "Thatch", "Lantern", "Brick",
            "Soot", "Steel", "Frost", "Basalt", "Ice", "ShieldCore",
        };

        [Test]
        public void AssaultMaterialsAreToonWithValidWeights()
        {
            foreach (string role in ToonRoles)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>($"{ArtPalette.MaterialFolder}/{role}.mat");
                Assert.IsNotNull(material, role + " (run Claybreak > Build Scene)");
                Assert.AreEqual(ToonShaderIds.ToonLitShader, material.shader.name, role);
            }

            int toon = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { ArtPalette.MaterialFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                Assert.AreNotEqual("Universal Render Pipeline/Simple Lit", material.shader.name, path);
                if (material.shader.name != ToonShaderIds.ToonLitShader || material.renderQueue >= (int)RenderQueue.Transparent)
                {
                    continue;
                }

                Vector4 w = material.GetVector(ToonShaderIds.StructureWeights);
                for (int i = 0; i < 4; i++)
                {
                    Assert.That(w[i], Is.InRange(0f, 1f), $"{path} weight {i}");
                }

                Assert.LessOrEqual(w.x + w.y + w.z + w.w, 1.0001f, path);
                Assert.That(material.GetFloat(ToonShaderIds.Emission), Is.InRange(0f, 1f), path);
                Assert.That(material.GetFloat(ToonShaderIds.FogAmount), Is.InRange(0f, 1f), path);
                Assert.IsFalse(material.IsKeywordEnabled(ToonShaderIds.InstancesKeyword), path);
                Assert.IsFalse(material.IsKeywordEnabled(ToonShaderIds.SlotsKeyword), path);
                toon++;
            }

            Assert.GreaterOrEqual(toon, ToonRoles.Length);
        }

        [Test]
        public void ToonPrimitivesHaveWhiteVertexColors()
        {
            foreach (PrimitiveType type in new[] { PrimitiveType.Cube, PrimitiveType.Cylinder, PrimitiveType.Sphere })
            {
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(ToonPrimitives.PathOf(type));
                Assert.IsNotNull(mesh, type + " (run Claybreak > Build Scene)");
                Color32[] colors = mesh.colors32;
                Assert.AreEqual(mesh.vertexCount, colors.Length, type.ToString());
                foreach (Color32 c in colors)
                {
                    Assert.AreEqual(new Color32(255, 255, 255, 255), c, type.ToString());
                }
            }
        }
    }
}
