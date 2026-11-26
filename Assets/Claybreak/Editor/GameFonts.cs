using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
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
        /// <summary>RT-6t.17: menu text on a colored face (white with the ink outline; the vertex color tints the face).</summary>
        public const string UiPath = Folder + "/Baloo2 Ui.mat";
        /// <summary>RT-6t.17: menu text on a light card (no outline; the vertex color is the ink).</summary>
        public const string InkPath = Folder + "/Baloo2 Ink.mat";
        /// <summary>RT-6t.17: the inline icons of the menus (star, coin: <see cref="UiIcons"/>), one TMP sprite asset.</summary>
        public const string IconsTexturePath = Folder + "/GameIcons.png";
        public const string IconsPath = Folder + "/GameIcons.asset";
        private const int IconCell = 128;

        /// <summary>
        /// Every character the outlined texts may draw: printable ASCII, the true math signs (minus, times, divide) and
        /// the Portuguese accents (a localized HUD later reuses the same atlas).
        /// </summary>
        public static readonly string Characters = BuildCharacters();

        // The prototype's colors (docs/direcao-visual-crowd.md items 19-21).
        public static readonly Color GoodOutline = Hex("#0f3d2b");
        public static readonly Color BadOutline = Hex("#4f0d19");
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

            // RT-6v3.3: darker and thicker outlines (review: the white number alone is 1.8:1 on a light pink panel), so the gate numbers stay legible over every world's track after the
            // post (measured fill against outline in the style capture, at least 3:1).
            WritePreset(GateGoodPath, sdf, Color.white, GoodOutline, 0.4f);
            WritePreset(GateBadPath, sdf, Color.white, BadOutline, 0.4f);
            WritePreset(FloatPath, sdf, Color.white, Color.white, 0.18f);
            WritePreset(CounterPath, sdf, Color.white, CounterOutline, 0.22f);
            WritePreset(UiPath, sdf, Color.white, CounterOutline, 0.2f);
            WritePreset(InkPath, sdf, Color.white, CounterOutline, 0f);
            BuildIcons();
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
            if (width > 0f)
            {
                material.EnableKeyword(ShaderUtilities.Keyword_Outline);
            }
            else
            {
                material.DisableKeyword(ShaderUtilities.Keyword_Outline);
            }
            EditorUtility.SetDirty(material);
        }

        private static string BuildCharacters()
        {
            var characters = new List<char>();
            for (char c = ' '; c <= '~'; c++)
            {
                characters.Add(c);
            }

            // RT-6t.17: the menus are TextMeshPro too (cent sign and the play arrow of the buttons).
            characters.AddRange("−×÷·…¢▶"
                + "ÁÂÃÀÇÉÊÍÓÔÕÚ"
                + "áâãàçéêíóôõú");
            return new string(characters.ToArray());
        }

        public static TMP_SpriteAsset Icons()
        {
            return AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(IconsPath) ?? throw new InvalidOperationException(IconsPath + " is missing (build the scene).");
        }

        // The icon sheet (one 128 px cell per icon, drawn here, antialiased by 4x4 supersampling) and its TMP sprite asset.
        // Same content on every build, so the files only change when an icon does.
        private static void BuildIcons()
        {
            string[] names = { UiIcons.StarName, UiIcons.CoinName };
            var texture = new Texture2D(IconCell * names.Length, IconCell, TextureFormat.RGBA32, false);
            var pixels = new Color32[texture.width * texture.height];
            for (int icon = 0; icon < names.Length; icon++)
            {
                for (int y = 0; y < IconCell; y++)
                {
                    for (int x = 0; x < IconCell; x++)
                    {
                        Color sum = Color.clear;
                        for (int sy = 0; sy < 4; sy++)
                        {
                            for (int sx = 0; sx < 4; sx++)
                            {
                                var point = new Vector2(x + (sx + 0.5f) / 4f, y + (sy + 0.5f) / 4f);
                                Color sample = icon == 0 ? StarPixel(point) : CoinPixel(point);
                                sum += new Color(sample.r * sample.a, sample.g * sample.a, sample.b * sample.a, sample.a);
                            }
                        }

                        // Back to straight alpha from the coverage-weighted sum.
                        float alpha = sum.a / 16f;
                        Color color = sum.a > 0f ? new Color(sum.r / sum.a, sum.g / sum.a, sum.b / sum.a, alpha) : Color.clear;
                        pixels[y * texture.width + icon * IconCell + x] = color;
                    }
                }
            }

            texture.SetPixels32(pixels);
            byte[] png = texture.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(texture);
            if (!System.IO.File.Exists(IconsTexturePath) || !System.Linq.Enumerable.SequenceEqual(System.IO.File.ReadAllBytes(IconsTexturePath), png))
            {
                System.IO.File.WriteAllBytes(IconsTexturePath, png);
                AssetDatabase.ImportAsset(IconsTexturePath);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(IconsTexturePath);
            if (importer.mipmapEnabled || !importer.alphaIsTransparency || importer.textureType != TextureImporterType.Default
                || importer.wrapMode != TextureWrapMode.Clamp || importer.npotScale != TextureImporterNPOTScale.None)
            {
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.SaveAndReimport();
            }

            Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(IconsTexturePath);
            TMP_SpriteAsset asset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(IconsPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
                asset.name = "GameIcons";
                AssetDatabase.CreateAsset(asset, IconsPath);
            }

            // The version marks the tables as current before anything reads them (an empty one makes TMP "upgrade" them from
            // the legacy sprite list, wiping them).
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("m_Version").stringValue = "1.1.0";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            asset.spriteInfoList ??= new List<TMP_Sprite>();

            if (asset.material == null)
            {
                var material = new Material(Shader.Find("TextMeshPro/Sprite")) { name = "GameIcons Material" };
                AssetDatabase.AddObjectToAsset(material, asset);
                asset.material = material;
            }

            asset.material.SetTexture(ShaderUtilities.ID_MainTex, sheet);
            asset.spriteSheet = sheet;
            asset.hashCode = TMP_TextUtilities.GetSimpleHashCode(asset.name);
            asset.spriteGlyphTable.Clear();
            asset.spriteCharacterTable.Clear();
            for (int i = 0; i < names.Length; i++)
            {
                // The sprite asset has no face of its own: TMP scales the icon to the font's ascent; it sinks a little
                // under the baseline so it sits centered on capitals.
                var glyph = new TMP_SpriteGlyph((uint)i, new GlyphMetrics(IconCell, IconCell, 0f, IconCell * 0.84f, IconCell * 1.08f),
                    new GlyphRect(i * IconCell, 0, IconCell, IconCell), 0.86f, 0);
                asset.spriteGlyphTable.Add(glyph);
                asset.spriteCharacterTable.Add(new TMP_SpriteCharacter(0xFFFE, asset, glyph) { name = names[i] });
            }

            asset.UpdateLookupTables();
            EditorUtility.SetDirty(asset);
        }

        private static readonly Color IconInk = new Color(0.231f, 0.184f, 0.29f, 1f);
        private static readonly Color IconGold = new Color(1f, 0.81f, 0.227f, 1f);
        private static readonly Color IconRim = new Color(0.93f, 0.6f, 0.12f, 1f);
        private static readonly Color IconShine = new Color(1f, 0.92f, 0.6f, 1f);

        // A five-point star, gold over an ink rim.
        private static Color StarPixel(Vector2 point)
        {
            var center = new Vector2(IconCell * 0.5f, IconCell * 0.47f);
            if (InStar(point, center, 62f, 28f))
            {
                return InStar(point, center, 50f, 22f) ? IconGold : IconInk;
            }

            return Color.clear;
        }

        // A coin: ink rim, orange edge, gold face with a lighter mark.
        private static Color CoinPixel(Vector2 point)
        {
            float distance = Vector2.Distance(point, new Vector2(IconCell * 0.5f, IconCell * 0.5f));
            if (distance > 58f)
            {
                return Color.clear;
            }

            if (distance > 50f)
            {
                return IconInk;
            }

            if (distance > 40f)
            {
                return IconRim;
            }

            var mark = new Vector2(point.x - IconCell * 0.5f, point.y - IconCell * 0.5f);
            return Mathf.Abs(mark.x) < 8f && Mathf.Abs(mark.y) < 24f ? IconShine : IconGold;
        }

        private static bool InStar(Vector2 point, Vector2 center, float outer, float inner)
        {
            var polygon = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float angle = Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                float radius = i % 2 == 0 ? outer : inner;
                polygon[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if ((polygon[i].y > point.y) != (polygon[j].y > point.y)
                    && point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
                {
                    inside = !inside;
                }
            }

            return inside;
        }


        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }
    }
}
