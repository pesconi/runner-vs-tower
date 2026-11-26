using Claybreak.Editor;
using JungleDegree.StudioKit.Toon;
using NUnit.Framework;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.6a: the world palettes are the user's exact values (docs/direcao-visual-crowd.md, section 1.1, copied here as
    /// the record), the provisional fourth one is section 5.1's, and every palette is complete and valid.
    /// </summary>
    public sealed class PaletteContentTests
    {
        // Sky top, sky bottom, fog, ground, track, stripe, crowd, tree (2 tones), trunk, rock, UI accent, UI ink.
        // RT-6v3.3 (D36, "night factory" and the contrast logic of option C) replaced the RT-6t.6a palettes (git history keeps them).
        private static readonly string[][] Approved =
        {
            new[] { "#233764", "#3a5a79", "#3a5a79", "#2f6f7a", "#e9e9ec", "#dbdade", "#ff2e9a", "#1f5d6b", "#3f8f97", "#3a3f66", "#c9c3e6", "#ff6f91", "#3b2f4a" },
            new[] { "#2e2458", "#5a4a7e", "#5a4a7e", "#2e4f5e", "#efebe7", "#e2ddd8", "#f0600a", "#2b2552", "#4a3f7a", "#241d40", "#9b7fb8", "#f26a5a", "#2d2448" },
            new[] { "#173a5c", "#2f6a80", "#2f6a80", "#2c7a6a", "#f0efe9", "#e3e1d8", "#a03cff", "#1d5f55", "#3d9a7f", "#2f3b52", "#f2b8d0", "#2fa7e6", "#24435a" },
            new[] { "#152a52", "#2f5a7e", "#2f5a7e", "#24586a", "#23273a", "#2f3446", "#9be21c", "#1e5a66", "#2f7f86", "#2a2d44", "#b8c8ef", "#ff8a5c", "#22313f" },
        };

        [Test]
        public void OnePalettePerWorldWithTheApprovedValues()
        {
            PaletteDefinition[] palettes = PaletteContent.Palettes;
            Assert.AreEqual(WorldContent.Worlds.Length, palettes.Length);
            CollectionAssert.AreEqual(new[] { "Pastel", "PorDoSol", "Doce", "Aurora" }, System.Array.ConvertAll(palettes, p => p.Name));
            for (int w = 0; w < palettes.Length; w++)
            {
                string[] hex = palettes[w].Hex;
                for (int i = 0; i < Approved[w].Length; i++)
                {
                    Assert.AreEqual(Approved[w][i], hex[i], $"{palettes[w].Name} {ToonPalette.ColorNames[i]}");
                }

                Assert.AreEqual(40f, palettes[w].FogStart);
                Assert.AreEqual(180f, palettes[w].FogEnd);
            }
        }

        [Test]
        public void EveryPaletteParsesAndValidates()
        {
            foreach (PaletteDefinition definition in PaletteContent.Palettes)
            {
                Color[] colors = PaletteContent.Colors(definition);
                var palette = ScriptableObject.CreateInstance<ToonPalette>();
                try
                {
                    palette.EditorAssign(colors, definition.FogStart, definition.FogEnd);
                    Assert.IsNull(palette.Validate(), definition.Name);
                    Assert.AreEqual(definition.Crowd, "#" + ColorUtility.ToHtmlStringRGB(palette.Crowd).ToLowerInvariant());
                }
                finally
                {
                    Object.DestroyImmediate(palette);
                }
            }
        }

        [Test]
        public void TheWorldCrowdColorsAreAllDifferent()
        {
            PaletteDefinition[] palettes = PaletteContent.Palettes;
            for (int a = 0; a < palettes.Length; a++)
            {
                for (int b = a + 1; b < palettes.Length; b++)
                {
                    Assert.AreNotEqual(palettes[a].Crowd, palettes[b].Crowd, $"{palettes[a].Name} and {palettes[b].Name}");
                }
            }
        }
    }
}
