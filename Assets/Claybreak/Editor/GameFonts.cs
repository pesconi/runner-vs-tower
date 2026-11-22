using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Claybreak.Editor
{
    /// <summary>
    /// The game's type (RT-6t.9): Baloo 2 ExtraBold (SIL OFL 1.1, from github.com/google/fonts, instanced to weight 800
    /// by <c>Tools/fonts/baloo2-extrabold.py</c>; register in docs/audio-licenses.md). The TTF itself is the dynamic font
    /// of every <c>UI.Text</c> and world <c>TextMesh</c>; the outlined texts (gate labels, army counter, floating number)
    /// use a static TextMeshPro SDF asset baked from it with a fixed character set, and four outline presets sharing its
    /// atlas. Built once and kept (rebuilt only when missing or when the character set no longer fits).
    /// </summary>
    public static class GameFonts
    {
        public const string Folder = ClaybreakSceneBuilder.Root + "/Fonts";
        public const string TtfPath = Folder + "/Baloo2-ExtraBold.ttf";
        public const string SdfPath = Folder + "/Baloo2-ExtraBold SDF.asset";
        public const string GateGoodPath = Folder + "/Baloo2 Gate Good.mat";
        public const string GateBadPath = Folder + "/Baloo2 Gate Bad.mat";
        public const string FloatPath = Folder + "/Baloo2 Float.mat";
        public const string CounterPath = Folder + "/Baloo2 Counter.mat";

        /// <summary>
        /// Every character the outlined texts may draw: printable ASCII, the true math signs (minus, times, divide) and
        /// the Portuguese accents (a localized HUD later reuses the same atlas).
        /// </summary>
        public static readonly string Characters = BuildCharacters();

        // The prototype's colors (docs/direcao-visual-crowd.md items 19-21).
        public static readonly Color GoodOutline = Hex("#1f7a55");
        public static readonly Color BadOutline = Hex("#a52a3a");
        public static readonly Color CounterOutline = Hex("#3b2f4a");

        private const int SamplingPointSize = 64;
        private const int AtlasPadding = 8;
        private const int AtlasSize = 1024;

        public static Font Ttf()
        {
            return AssetDatabase.LoadAssetAtPath<Font>(TtfPath) ?? throw new InvalidOperationException(TtfPath + " is missing.");
        }

        public static TMP_FontAsset Sdf()
        {
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SdfPath) ?? throw new InvalidOperationException(SdfPath + " is missing (build the scene).");
        }

        public static Material Preset(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(path) ?? throw new InvalidOperationException(path + " is missing (build the scene).");
        }

        /// <summary>Bakes the SDF asset when missing or incomplete, then (re)writes the four outline presets.</summary>
        public static void BuildAssets()
        {
            TMP_FontAsset sdf = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SdfPath);
            if (sdf == null || MissingCharacters(sdf).Length > 0)
            {
                if (sdf != null)
                {
                    AssetDatabase.DeleteAsset(SdfPath);
                }

                sdf = Bake();
            }

            WritePreset(GateGoodPath, sdf, Color.white, GoodOutline, 0.28f);
            WritePreset(GateBadPath, sdf, Color.white, BadOutline, 0.28f);
            WritePreset(FloatPath, sdf, Color.white, Color.white, 0.18f);
            WritePreset(CounterPath, sdf, Color.white, CounterOutline, 0.22f);
            AssetDatabase.SaveAssets();
        }

        /// <summary>Characters of <see cref="Characters"/> the asset cannot draw (empty = complete).</summary>
        public static string MissingCharacters(TMP_FontAsset sdf)
        {
            var missing = new System.Text.StringBuilder();
            foreach (char c in Characters)
            {
                if (c != ' ' && (sdf.characterLookupTable == null || !sdf.characterLookupTable.ContainsKey(c)))
                {
                    missing.Append(c);
                }
            }

            return missing.ToString();
        }

        private static TMP_FontAsset Bake()
        {
            Font ttf = Ttf();
            TMP_FontAsset sdf = TMP_FontAsset.CreateFontAsset(ttf, SamplingPointSize, AtlasPadding, GlyphRenderMode.SDFAA,
                AtlasSize, AtlasSize, AtlasPopulationMode.Dynamic, false);
            if (sdf == null)
            {
                throw new InvalidOperationException("TextMeshPro could not load " + TtfPath + ".");
            }

            if (!sdf.TryAddCharacters(Characters, out string missing) && !string.IsNullOrEmpty(missing.Trim()))
            {
                throw new InvalidOperationException("Baloo 2 SDF atlas could not take: " + missing);
            }

            // Static: the atlas is final and nothing is rasterized at run time (no per-frame work, no allocation); the build
            // only clears dynamic atlases, so this one ships as baked.
            sdf.atlasPopulationMode = AtlasPopulationMode.Static;
            sdf.name = "Baloo2-ExtraBold SDF";
            AssetDatabase.CreateAsset(sdf, SdfPath);
            Texture2D atlas = sdf.atlasTexture;
            atlas.name = "Baloo2-ExtraBold Atlas";
            AssetDatabase.AddObjectToAsset(atlas, sdf);
            sdf.material.name = "Baloo2-ExtraBold Material";
            AssetDatabase.AddObjectToAsset(sdf.material, sdf);
            EditorUtility.SetDirty(sdf);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(SdfPath);
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SdfPath);
        }

        private static void WritePreset(string path, TMP_FontAsset sdf, Color face, Color outline, float width)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(sdf.material);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = sdf.material.shader;
                material.CopyPropertiesFromMaterial(sdf.material);
            }

            material.name = System.IO.Path.GetFileNameWithoutExtension(path);
            material.SetColor(ShaderUtilities.ID_FaceColor, face);
            material.SetColor(ShaderUtilities.ID_OutlineColor, outline);
            material.SetFloat(ShaderUtilities.ID_OutlineWidth, width);
            material.EnableKeyword(ShaderUtilities.Keyword_Outline);
            EditorUtility.SetDirty(material);
        }

        private static string BuildCharacters()
        {
            var characters = new List<char>();
            for (char c = ' '; c <= '~'; c++)
            {
                characters.Add(c);
            }

            characters.AddRange("−×÷·…"
                + "ÁÂÃÀÇÉÊÍÓÔÕÚ"
                + "áâãàçéêíóôõú");
            return new string(characters.ToArray());
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }
    }
}
