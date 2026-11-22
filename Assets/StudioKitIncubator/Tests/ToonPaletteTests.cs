using JungleDegree.StudioKit.Toon;
using NUnit.Framework;
using UnityEngine;

namespace JungleDegree.StudioKit.Tests
{
    public sealed class ToonPaletteTests
    {
        private ToonPalette palette;

        [SetUp]
        public void SetUp()
        {
            palette = ScriptableObject.CreateInstance<ToonPalette>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(palette);
        }

        [Test]
        public void ADefaultPaletteIsValidAndHasTheReferenceFog()
        {
            Assert.IsNull(palette.Validate());
            Assert.AreEqual(22f, palette.FogStart);
            Assert.AreEqual(75f, palette.FogEnd);
            Assert.AreEqual(ToonPalette.ColorNames.Length, palette.Colors.Length);
        }

        [Test]
        public void AssignKeepsTheColorOrder()
        {
            var colors = new Color[ToonPalette.ColorNames.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(i / 32f, 0.5f, 1f - i / 32f, 1f);
            }

            palette.EditorAssign(colors, 10f, 40f);
            CollectionAssert.AreEqual(colors, palette.Colors);
            Assert.AreEqual(colors[0], palette.SkyTop);
            Assert.AreEqual(colors[6], palette.Crowd);
            Assert.AreEqual(colors[16], palette.Defense);
            Assert.AreEqual(10f, palette.FogStart);
            Assert.AreEqual(40f, palette.FogEnd);
        }

        [Test]
        public void ATransparentColorOrABackwardFogIsRejected()
        {
            Color[] colors = palette.Colors;
            colors[4] = new Color(1f, 1f, 1f, 0.5f);
            palette.EditorAssign(colors, 22f, 75f);
            StringAssert.Contains("track", palette.Validate());

            colors[4] = Color.white;
            palette.EditorAssign(colors, 75f, 22f);
            StringAssert.Contains("fog", palette.Validate());
        }

        [Test]
        public void ContrastMatchesTheWcagAndCieReferences()
        {
            Assert.AreEqual(21f, ToonContrast.Ratio(Color.black, Color.white), 0.01f);
            Assert.AreEqual(1f, ToonContrast.Ratio(Color.gray, Color.gray), 1e-5f);
            Assert.AreEqual(ToonContrast.Ratio(Color.red, Color.blue), ToonContrast.Ratio(Color.blue, Color.red), 1e-5f);
            Assert.AreEqual(0f, ToonContrast.DeltaE(Color.cyan, Color.cyan), 1e-4f);

            // CIELAB of pure sRGB white and red (D65): (100, 0, 0) and about (53.2, 80.1, 67.2).
            Vector3 white = ToonContrast.ToLab(Color.white);
            Assert.AreEqual(100f, white.x, 0.1f);
            Assert.AreEqual(0f, white.y, 0.2f);
            Assert.AreEqual(0f, white.z, 0.2f);
            Vector3 red = ToonContrast.ToLab(Color.red);
            Assert.AreEqual(53.2f, red.x, 0.3f);
            Assert.AreEqual(80.1f, red.y, 0.5f);
            Assert.AreEqual(67.2f, red.z, 0.5f);
            Assert.AreEqual(100f, ToonContrast.DeltaE(Color.black, Color.white), 0.1f);
        }

        [Test]
        public void TheApplierWritesTheSkyAndTheFog()
        {
            Color[] colors = palette.Colors;
            colors[0] = new Color(1f, 0.85f, 0.81f);
            colors[1] = new Color(0.81f, 0.91f, 1f);
            colors[2] = new Color(0.95f, 0.89f, 0.92f);
            palette.EditorAssign(colors, 22f, 75f);
            bool fog = RenderSettings.fog;
            FogMode mode = RenderSettings.fogMode;
            Color fogColor = RenderSettings.fogColor;
            float start = RenderSettings.fogStartDistance;
            float end = RenderSettings.fogEndDistance;
            try
            {
                ToonPaletteApplier.ApplyGlobally(palette, null);
                Assert.IsTrue(RenderSettings.fog);
                Assert.AreEqual(FogMode.Linear, RenderSettings.fogMode);
                Assert.AreEqual(colors[2], RenderSettings.fogColor);
                Assert.AreEqual(22f, RenderSettings.fogStartDistance);
                Assert.AreEqual(75f, RenderSettings.fogEndDistance);
                Vector4 top = Shader.GetGlobalVector(ToonShaderIds.SkyTop);
                Vector4 bottom = Shader.GetGlobalVector(ToonShaderIds.SkyBottom);
                Assert.AreEqual(colors[0].r, top.x, 1e-5f, "the sky goes raw (sRGB), not linear");
                Assert.AreEqual(colors[1].b, bottom.z, 1e-5f);
            }
            finally
            {
                RenderSettings.fog = fog;
                RenderSettings.fogMode = mode;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogStartDistance = start;
                RenderSettings.fogEndDistance = end;
            }
        }
    }
}
