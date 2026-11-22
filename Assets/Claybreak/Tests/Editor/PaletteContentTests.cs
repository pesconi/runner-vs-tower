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
        private static readonly string[][] Approved =
        {
            new[] { "#ffd9cf", "#cfe8ff", "#f3e3ea", "#bfe6cf", "#fff4ea", "#ffe3d1", "#ff8fab", "#7cc6a4", "#a8dcbf", "#c99076", "#e7d3f0", "#ff6f91", "#3b2f4a" },
            new[] { "#3a3270", "#f7a26b", "#e9967a", "#7a5482", "#f6caa6", "#eeb790", "#fff1d6", "#463a78", "#5b4a8c", "#2f2650", "#a36b8f", "#f26a5a", "#2d2448" },
            new[] { "#8fd3ff", "#eef9ff", "#dff2ff", "#93d99a", "#fffae6", "#ffedb0", "#ffcf3a", "#45b07c", "#6fcf8f", "#b07a55", "#f4b4c8", "#2fa7e6", "#24435a" },
            new[] { "#2c3e66", "#9ad0c2", "#a9cfc4", "#5e8f88", "#f2efe4", "#e3dcc8", "#c9f2a0", "#2f6f73", "#4f9a8f", "#3d3b52", "#c3b5e0", "#ff8a5c", "#22313f" },
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

                Assert.AreEqual(22f, palettes[w].FogStart);
                Assert.AreEqual(75f, palettes[w].FogEnd);
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
