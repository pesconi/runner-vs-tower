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
        public float FogStart = 110f;
        public float FogEnd = 150f;

        public string[] Hex => new[]
        {
            SkyTop, SkyBottom, Fog, Ground, Track, TrackStripe, Crowd, FoliageDark, FoliageLight, Trunk, Rock, UiAccent, UiInk,
            TowerLight, TowerDark, TowerDetail, Defense,
        };
    }

    /// <summary>
    /// The world palettes (RT-6t.6a) as code, like the rest of the content: the scene builder rewrites the assets from here
    /// on every build, so a palette change is a reviewed edit of this file. Worlds 1–3 are the user's palettes with the
    /// exact values (docs/direcao-visual-crowd.md section 1.1); world 4 is the provisional "Aurora" of section 5.1. The
    /// tower and defense colors are ours, provisional [P]: on screen since RT-6t.10 (towers, bosses, defenses, projectiles,
    /// crates, through ArtPalette's structure weights), Pastel's tower light/dark darkened there so the towers read against
    /// its pink sky. Fog (RT-6t.13, user 2026-11-22: "remove it or leave it near zero"): 110–150 m in every world, clear
    /// over everything the player looks at and complete before the camera far clip, so the horizon does not cut sharp.
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
                SkyTop = "#ffd9cf", SkyBottom = "#cfe8ff", Fog = "#f3e3ea", Ground = "#bfe6cf",
                Track = "#fff4ea", TrackStripe = "#ffe3d1", Crowd = "#ff8fab",
                FoliageDark = "#7cc6a4", FoliageLight = "#a8dcbf", Trunk = "#c99076", Rock = "#e7d3f0",
                UiAccent = "#ff6f91", UiInk = "#3b2f4a",
                TowerLight = "#f1d2e1", TowerDark = "#ab86b8", TowerDetail = "#ff6f91", Defense = "#7d6b91",
            },
            new PaletteDefinition
            {
                Name = "PorDoSol",
                SkyTop = "#3a3270", SkyBottom = "#f7a26b", Fog = "#e9967a", Ground = "#7a5482",
                Track = "#f6caa6", TrackStripe = "#eeb790", Crowd = "#fff1d6",
                FoliageDark = "#463a78", FoliageLight = "#5b4a8c", Trunk = "#2f2650", Rock = "#a36b8f",
                UiAccent = "#f26a5a", UiInk = "#2d2448",
                TowerLight = "#d9a6b0", TowerDark = "#5b4170", TowerDetail = "#f26a5a", Defense = "#3a2d5c",
            },
            new PaletteDefinition
            {
                Name = "Doce",
                SkyTop = "#8fd3ff", SkyBottom = "#eef9ff", Fog = "#dff2ff", Ground = "#93d99a",
                Track = "#fffae6", TrackStripe = "#ffedb0", Crowd = "#ffcf3a",
                FoliageDark = "#45b07c", FoliageLight = "#6fcf8f", Trunk = "#b07a55", Rock = "#f4b4c8",
                UiAccent = "#2fa7e6", UiInk = "#24435a",
                TowerLight = "#fff0f5", TowerDark = "#e58fae", TowerDetail = "#2fa7e6", Defense = "#5a6fb0",
            },
            new PaletteDefinition
            {
                Name = "Aurora",
                SkyTop = "#2c3e66", SkyBottom = "#9ad0c2", Fog = "#a9cfc4", Ground = "#5e8f88",
                Track = "#f2efe4", TrackStripe = "#e3dcc8", Crowd = "#c9f2a0",
                FoliageDark = "#2f6f73", FoliageLight = "#4f9a8f", Trunk = "#3d3b52", Rock = "#c3b5e0",
                UiAccent = "#ff8a5c", UiInk = "#22313f",
                TowerLight = "#dfe7f0", TowerDark = "#4c5f86", TowerDetail = "#ff8a5c", Defense = "#2c3e66",
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
