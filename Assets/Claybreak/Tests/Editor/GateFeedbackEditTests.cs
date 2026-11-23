using System.IO;
using System.Security.Cryptography;
using Claybreak.Editor;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>RT-6t.9: the float and pop curves follow the prototype; the Baloo 2 font assets are the registered ones.</summary>
    public sealed class GateFeedbackEditTests
    {
        [Test]
        public void FloatingNumberFollowsThePrototypeKeyframes()
        {
            FloatingNumbers.Pose(0f, out float rise, out float scale, out float alpha);
            Assert.AreEqual(0f, rise, 1e-4f);
            Assert.AreEqual(0.4f, scale, 1e-4f);
            Assert.AreEqual(0f, alpha, 1e-4f);

            FloatingNumbers.Pose(0.2f, out rise, out scale, out alpha);
            Assert.AreEqual(20f, rise, 1e-3f);
            Assert.AreEqual(1.25f, scale, 1e-3f);
            Assert.AreEqual(1f, alpha, 1e-3f);

            FloatingNumbers.Pose(1f, out rise, out scale, out alpha);
            Assert.AreEqual(110f, rise, 1e-3f);
            Assert.AreEqual(1f, scale, 1e-3f);
            Assert.AreEqual(0f, alpha, 1e-3f);

            float last = -1f;
            for (float t = 0f; t <= 1f; t += 0.05f)
            {
                FloatingNumbers.Pose(t, out rise, out _, out _);
                Assert.GreaterOrEqual(rise, last, "the number went down");
                last = rise;
            }
        }

        [Test]
        public void CounterPopsTo118AndBackIn030Seconds()
        {
            Assert.AreEqual(1f, ClaybreakHud.CounterPop(0f), 1e-4f);
            Assert.AreEqual(ClaybreakHud.PopScale, ClaybreakHud.CounterPop(ClaybreakHud.PopHalfSeconds), 1e-4f);
            Assert.AreEqual(1f, ClaybreakHud.CounterPop(2f * ClaybreakHud.PopHalfSeconds), 1e-4f);
            Assert.AreEqual(1f, ClaybreakHud.CounterPop(5f), 1e-4f);
            Assert.Greater(ClaybreakHud.CounterPop(0.05f), 1f);
        }

        [Test]
        public void BalooIsTheRegisteredFileAndDrawsTheMathSignsAndAccents()
        {
            // docs/audio-licenses.md, section 2, and Tools/fonts/baloo2-extrabold.py.
            Assert.AreEqual("73e6a48f3fce3efdd0dea8f4f444279e87763be97be07310739b56cefe29d3e2", Sha256(GameFonts.TtfPath));
            Assert.AreEqual("ad09b05dc8bc678c9daf7c4c5f7ef1f55e5726127f4330b2e98e40b9dffcb860", Sha256(GameFonts.Folder + "/Baloo2-OFL.txt"));

            TMP_FontAsset sdf = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GameFonts.SdfPath);
            Assert.IsNotNull(sdf, "build the scene first");
            Assert.AreEqual(AtlasPopulationMode.Static, sdf.atlasPopulationMode);
            Assert.AreEqual(string.Empty, GameFonts.MissingCharacters(sdf));
            foreach (char c in "+−×÷çãõéêÁÇÃ")
            {
                Assert.IsTrue(sdf.characterLookupTable.ContainsKey(c), "missing U+" + ((int)c).ToString("X4"));
            }

            foreach (string preset in new[] { GameFonts.GateGoodPath, GameFonts.GateBadPath, GameFonts.FloatPath, GameFonts.CounterPath })
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(preset);
                Assert.IsNotNull(material, preset);
                Assert.IsTrue(material.IsKeywordEnabled(ShaderUtilities.Keyword_Outline), preset + " has no outline");
                Assert.AreEqual(sdf.atlasTexture, material.GetTexture(ShaderUtilities.ID_MainTex), preset + " is not on the atlas");
            }

            AssertColor(GameFonts.GoodOutline, AssetDatabase.LoadAssetAtPath<Material>(GameFonts.GateGoodPath).GetColor(ShaderUtilities.ID_OutlineColor));
            AssertColor(GameFonts.BadOutline, AssetDatabase.LoadAssetAtPath<Material>(GameFonts.GateBadPath).GetColor(ShaderUtilities.ID_OutlineColor));
        }

        [Test]
        public void MenuTextHasItsPresetsAndTheInlineIcons()
        {
            // RT-6t.17: the menus are TextMeshPro on the same atlas; the font has no star, so star and coin are sprites.
            TMP_FontAsset sdf = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GameFonts.SdfPath);
            Assert.IsNotNull(sdf, "build the scene first");
            foreach (char c in "¢▶·")
            {
                Assert.IsTrue(sdf.characterLookupTable.ContainsKey(c), "missing U+" + ((int)c).ToString("X4"));
            }

            Material ui = AssetDatabase.LoadAssetAtPath<Material>(GameFonts.UiPath);
            Material ink = AssetDatabase.LoadAssetAtPath<Material>(GameFonts.InkPath);
            Assert.IsNotNull(ui);
            Assert.IsNotNull(ink);
            Assert.IsTrue(ui.IsKeywordEnabled(ShaderUtilities.Keyword_Outline), "menu text on buttons has no outline");
            Assert.IsFalse(ink.IsKeywordEnabled(ShaderUtilities.Keyword_Outline), "ink text on cards should not be outlined");
            Assert.AreEqual(sdf.atlasTexture, ui.GetTexture(ShaderUtilities.ID_MainTex));
            Assert.AreEqual(sdf.atlasTexture, ink.GetTexture(ShaderUtilities.ID_MainTex));
            AssertColor(GameFonts.CounterOutline, ui.GetColor(ShaderUtilities.ID_OutlineColor));

            TMP_SpriteAsset icons = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(GameFonts.IconsPath);
            Assert.IsNotNull(icons, "build the scene first");
            Assert.AreEqual("1.1.0", icons.version);
            Assert.IsNotNull(icons.spriteSheet);
            Assert.IsNotNull(icons.material);
            foreach (string name in new[] { Claybreak.UiIcons.StarName, Claybreak.UiIcons.CoinName })
            {
                Assert.GreaterOrEqual(icons.GetSpriteIndexFromName(name), 0, "no icon named " + name);
            }
        }

        private static void AssertColor(Color expected, Color actual)
        {
            Assert.AreEqual(expected.r, actual.r, 1e-3f);
            Assert.AreEqual(expected.g, actual.g, 1e-3f);
            Assert.AreEqual(expected.b, actual.b, 1e-3f);
        }

        private static string Sha256(string assetPath)
        {
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(File.ReadAllBytes(assetPath));
            return System.BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
