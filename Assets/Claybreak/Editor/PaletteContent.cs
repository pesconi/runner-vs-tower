using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>One world palette as written in the art direction (sRGB hex), in <see cref="ToonPalette.ColorNames"/> order.</summary>
    public sealed class PaletteDefinition
    {
        public string Name;
        public string SkyTop, SkyBottom, Fog, Ground, Track, TrackStripe, Crowd, FoliageDark, FoliageLight, Trunk, Rock,
            UiAccent, UiInk, TowerLight, TowerDark, TowerDetail, Defense;
        // Tower* fill the palette's structure colors (the towers are this game's structures).
        // RT-6v3.3 (D36): colored depth fog, from the middle of the play area to just past the camera far clip (170 m): the far
        // ground meets the fog color at the horizon, the towers keep most of their colors (structures take a third of it) and
        // the props come in at the horizon almost fully fogged (review round 1).
        public float FogStart = 40f;
        public float FogEnd = 180f;

        public string[] Hex => new[]
        {
            SkyTop, SkyBottom, Fog, Ground, Track, TrackStripe, Crowd, FoliageDark, FoliageLight, Trunk, Rock, UiAccent, UiInk,
            TowerLight, TowerDark, TowerDetail, Defense,
        };
    }

    /// <summary>
    /// The world palettes as code, like the rest of the content: the scene builder rewrites the assets from here on every build,
    /// so a palette change is a reviewed edit of this file. RT-6v3.3 (D36): every world is the "night factory" of the A/M
    /// concept screens (dark blue sky in a gradient, clear structures, a glow) with its own accents, and follows the D36
    /// contrast logic, checked by the style validator: the crowd is the most saturated color (CIELAB chroma 81–108), the
    /// track is neutral (chroma ≤ 15) and far from the crowd in value (|ΔL*| ≥ 30: light track in worlds 1–3, dark in world 4
    /// for the light lime crowd), the defenses sit across the hue wheel from the crowd (≥ 115°). The fog is on again
    /// (D36 revises RT-6t.13), in the color of the world's horizon (the sky bottom), so the far ground melts into the sky. Asset names stay the first palettes' (stable ids).
    /// </summary>
    public static class PaletteContent
    {
        public const string Folder = WorldContent.DataFolder + "/Palettes";

        /// <summary>One palette per world, in catalog order.</summary>
        public static PaletteDefinition[] Palettes => new[]
        {
            new PaletteDefinition
            {
                Name = "Pastel",
                SkyTop = "#233764", SkyBottom = "#3a5a79", Fog = "#3a5a79", Ground = "#2f6f7a",
                Track = "#e9e9ec", TrackStripe = "#dbdade", Crowd = "#ff2e9a",
                FoliageDark = "#1f5d6b", FoliageLight = "#3f8f97", Trunk = "#3a3f66", Rock = "#c9c3e6",
                UiAccent = "#ff6f91", UiInk = "#3b2f4a",
                TowerLight = "#f2f4fa", TowerDark = "#3b4a7a", TowerDetail = "#39e6ff", Defense = "#0d5a55",
            },
            new PaletteDefinition
            {
                Name = "PorDoSol",
                SkyTop = "#2e2458", SkyBottom = "#5a4a7e", Fog = "#5a4a7e", Ground = "#2e4f5e",
                Track = "#efebe7", TrackStripe = "#e2ddd8", Crowd = "#f0600a",
                FoliageDark = "#2b2552", FoliageLight = "#4a3f7a", Trunk = "#241d40", Rock = "#9b7fb8",
                UiAccent = "#f26a5a", UiInk = "#2d2448",
                TowerLight = "#f3eef8", TowerDark = "#4a3a78", TowerDetail = "#ffb347", Defense = "#2a52b8",
            },
            new PaletteDefinition
            {
                Name = "Doce",
                SkyTop = "#173a5c", SkyBottom = "#2f6a80", Fog = "#2f6a80", Ground = "#2c7a6a",
                Track = "#f0efe9", TrackStripe = "#e3e1d8", Crowd = "#a03cff",
                FoliageDark = "#1d5f55", FoliageLight = "#3d9a7f", Trunk = "#2f3b52", Rock = "#f2b8d0",
                UiAccent = "#2fa7e6", UiInk = "#24435a",
                TowerLight = "#fbf7ea", TowerDark = "#3a4f6e", TowerDetail = "#5ff2c2", Defense = "#b07a12",
            },
            new PaletteDefinition
            {
                Name = "Aurora",
                SkyTop = "#152a52", SkyBottom = "#2f5a7e", Fog = "#2f5a7e", Ground = "#24586a",
                Track = "#23273a", TrackStripe = "#2f3446", Crowd = "#9be21c",
                FoliageDark = "#1e5a66", FoliageLight = "#2f7f86", Trunk = "#2a2d44", Rock = "#b8c8ef",
                UiAccent = "#ff8a5c", UiInk = "#22313f",
                TowerLight = "#e8f1fb", TowerDark = "#33406a", TowerDetail = "#6af0ff", Defense = "#7a2f9e",
            },
        };

        public static string PalettePath(int world) => $"{Folder}/{Palettes[world].Name}.asset";

        /// <summary>The palette asset of a world (catalog index), or null before the first build.</summary>
        public static ToonPalette Load(int world) => AssetDatabase.LoadAssetAtPath<ToonPalette>(PalettePath(world));

        /// <summary>Writes every palette asset from <see cref="Palettes"/>.</summary>
        public static void Write()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
            {
                AssetDatabase.CreateFolder(WorldContent.DataFolder, "Palettes");
            }

            PaletteDefinition[] definitions = Palettes;
            for (int w = 0; w < definitions.Length; w++)
            {
                string path = PalettePath(w);
                var palette = AssetDatabase.LoadAssetAtPath<ToonPalette>(path);
                if (palette == null)
                {
                    palette = ScriptableObject.CreateInstance<ToonPalette>();
                    AssetDatabase.CreateAsset(palette, path);
                }

                palette.EditorAssign(Colors(definitions[w]), definitions[w].FogStart, definitions[w].FogEnd);
                EditorUtility.SetDirty(palette);
            }
        }

        /// <summary>The colors of a definition, parsed from its hex values.</summary>
        public static Color[] Colors(PaletteDefinition definition)
        {
            string[] hex = definition.Hex;
            var colors = new Color[hex.Length];
            for (int i = 0; i < hex.Length; i++)
            {
                if (!ColorUtility.TryParseHtmlString(hex[i], out colors[i]))
                {
                    throw new System.FormatException($"palette {definition.Name}: '{hex[i]}' ({ToonPalette.ColorNames[i]}) is not a color");
                }
            }

            return colors;
        }
    }
}
