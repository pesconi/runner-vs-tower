using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The Bibopzi's face and motion on the crowd material (RT-6v3.4b, <c>docs/direcao-visual-v3.md</c> 3.2, 3.3 and D36).
    /// The atlas is drawn by <c>Tools/art/bibopzi_faces.py</c> (8 cells, see <see cref="FaceExpression"/>) and imported here
    /// with mipmaps; the kit's <see cref="ToonFaceAtlas"/> paints it on the masked front of the Blender levels, and
    /// <see cref="ToonVinylMotion"/> moves the spring, the hands and the wind-up key (marks in UV2) in the vertex shader. The
    /// crowd, the leader, the units in flight and the obstacle deaths share the material; the blob shadow takes the same
    /// animated layout (the kit reads 80-byte instances for both).
    /// </summary>
    public static class BibopziFaceArt
    {
        public const string AtlasPath = "Assets/Claybreak/Content/Style/BibopziFaces.png";
        public const string ScriptPath = "Tools/art/bibopzi_faces.py";
        /// <summary>A cell is 128 × 256 px: the face box is 0.4 m × 0.7 m (the whole body), about 330–370 px per meter both ways.</summary>
        public const int CellWidth = 128;
        public const int CellHeight = 256;

        /// <summary>Share of a cell the shader skips on every side (mipmaps never mix two cells); the drawing keeps 0.08 empty.</summary>
        public const float Inset = 0.06f;

        /// <summary>The script's empty band (share of a cell).</summary>
        public const float DrawnMargin = 0.08f;

        /// <summary>The face projection of the mesh (x min, y min, x max, y max; Tools/blender/bibopzi.py FACE_BOX).</summary>
        public static readonly float[] FaceBox = { -0.2f, -0.02f, 0.2f, 0.68f };

        /// <summary>
        /// The motion (object space of the 0.78 m Bibopzi): the spring tip trails 7 cm down and 9 cm back 0.08 s late; the
        /// hands swing 2.5 cm against the hop and open 9 cm out and 10 cm up (fable review: visible from the run camera); the
        /// key turns once per two hops about the shaft (1.3 turns a second at the Bibopzi's hop).
        /// </summary>
        public static ToonVinylMotion Motion(Vector3 keyPivot, Vector3 keyAxis)
        {
            return new ToonVinylMotion(new Vector3(0f, -0.07f, -0.09f), 0.08f, 0.025f, new Vector2(0.09f, 0.1f), keyAxis, keyPivot, 1);
        }

        /// <summary>The motion with the key of the crowd level's data.</summary>
        public static ToonVinylMotion Motion()
        {
            SimpleMeshData data = SimpleMeshData.Read(UnitShapeAssets.Named("Bibopzi").DataPath);
            return Motion(data.KeyPivot, data.KeyAxis);
        }

        public static Texture2D LoadAtlas() => AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);

        public static ToonFaceAtlas Atlas(Texture2D texture) => new ToonFaceAtlas(texture, FaceRules.AtlasColumns, FaceRules.AtlasRows, Inset);

        /// <summary>Imports the atlas (mipmaps, clamp, trilinear; ASTC 4×4 on Android) and puts face and motion on the crowd materials.</summary>
        public static void Apply()
        {
            ConfigureImporter();
            Texture2D atlas = LoadAtlas() ?? throw new InvalidOperationException($"missing {AtlasPath} (python -X utf8 {ScriptPath})");
            Material crowd = Load<Material>(ToonLabBuilder.SwarmMaterialPath);
            Material shadow = Load<Material>(ToonLabBuilder.ShadowMaterialPath);
            if (crowd.shader.name != ToonShaderIds.ToonVinylShader)
            {
                throw new InvalidOperationException($"{ToonLabBuilder.SwarmMaterialPath} must be on {ToonShaderIds.ToonVinylShader} first (StyleGuideContent.ApplySurfaces)");
            }

            Motion().ApplyTo(crowd);
            Atlas(atlas).ApplyTo(crowd);
            // The blob shadow reads the same 80-byte instances (no vertex motion in it, only the layout).
            shadow.EnableKeyword(ToonShaderIds.AnimatedKeyword);
            EditorUtility.SetDirty(crowd);
            EditorUtility.SetDirty(shadow);
            AssetDatabase.SaveAssets();
        }

        public static void ConfigureImporter()
        {
            if (!File.Exists(AtlasPath))
            {
                throw new InvalidOperationException($"missing {AtlasPath} (python -X utf8 {ScriptPath})");
            }

            AssetDatabase.ImportAsset(AtlasPath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            if (ImporterProblem(importer, android) == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.mipmapFilter = TextureImporterMipFilter.BoxFilter;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 1;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 512;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            android.overridden = true;
            android.maxTextureSize = 512;
            android.format = TextureImporterFormat.ASTC_4x4;
            importer.SetPlatformTextureSettings(android);
            importer.SaveAndReimport();
        }

        /// <summary>Null when the atlas is imported as the face needs it.</summary>
        public static string ImporterProblem(TextureImporter importer, TextureImporterPlatformSettings android)
        {
            if (importer == null)
            {
                return $"{AtlasPath} is not imported";
            }

            if (!importer.mipmapEnabled || importer.wrapMode != TextureWrapMode.Clamp || !importer.sRGBTexture
                || !importer.alphaIsTransparency || importer.filterMode != FilterMode.Trilinear
                || importer.textureType != TextureImporterType.Default || importer.npotScale != TextureImporterNPOTScale.None)
            {
                return $"{AtlasPath}: mipmaps, clamp, sRGB, alpha is transparency, trilinear, no NPOT scale";
            }

            return android == null || !android.overridden || android.format != TextureImporterFormat.ASTC_4x4
                ? $"{AtlasPath}: Android ASTC 4x4"
                : null;
        }

        /// <summary>
        /// The problems of the atlas file and of the crowd materials (validator and tests): the file is the script's current
        /// output (hash in its text chunk), 512 × 512 (4 × 2 cells of 128 × 256), every cell draws something and leaves its margin
        /// empty, the importer settings, and the face, motion and layout keywords on the materials.
        /// </summary>
        public static List<string> Problems()
        {
            var problems = new List<string>();
            if (!File.Exists(AtlasPath))
            {
                problems.Add($"missing {AtlasPath} (python -X utf8 {ScriptPath})");
                return problems;
            }

            byte[] png = File.ReadAllBytes(AtlasPath);
            string written = PngText(png, "FACE_SCRIPT_HASH");
            if (written == null || written != ScriptHash())
            {
                problems.Add($"{AtlasPath} is not the current output of {ScriptPath} (run it)");
            }

            problems.AddRange(PixelProblems(ReadPixels(png, out int width, out int height), width, height));
            var importer = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
            string import = ImporterProblem(importer, importer?.GetPlatformTextureSettings("Android"));
            if (import != null)
            {
                problems.Add(import);
            }

            Material crowd = AssetDatabase.LoadAssetAtPath<Material>(ToonLabBuilder.SwarmMaterialPath);
            Material shadow = AssetDatabase.LoadAssetAtPath<Material>(ToonLabBuilder.ShadowMaterialPath);
            if (crowd == null || !crowd.IsKeywordEnabled(ToonShaderIds.AnimatedKeyword) || !crowd.IsKeywordEnabled(ToonShaderIds.FaceKeyword)
                || crowd.GetTexture(ToonShaderIds.VinylFaceAtlas) != LoadAtlas())
            {
                problems.Add($"{ToonLabBuilder.SwarmMaterialPath} needs the animated layout and the face atlas (rebuild the scene)");
            }

            if (shadow == null || !shadow.IsKeywordEnabled(ToonShaderIds.AnimatedKeyword))
            {
                problems.Add($"{ToonLabBuilder.ShadowMaterialPath} needs the animated layout with the crowd's (rebuild the scene)");
            }

            return problems;
        }

        /// <summary>The atlas size and its cells: something drawn, the margin empty.</summary>
        public static List<string> PixelProblems(Color32[] pixels, int width, int height)
        {
            var problems = new List<string>();
            if (width != CellWidth * FaceRules.AtlasColumns || height != CellHeight * FaceRules.AtlasRows)
            {
                problems.Add($"{AtlasPath}: {width}x{height}, expected {CellWidth * FaceRules.AtlasColumns}x{CellHeight * FaceRules.AtlasRows}");
                return problems;
            }

            int marginX = Mathf.RoundToInt(DrawnMargin * CellWidth) - 1;
            int marginY = Mathf.RoundToInt(DrawnMargin * CellHeight) - 1;
            for (int cell = 0; cell < FaceRules.Cells; cell++)
            {
                int drawn = 0;
                int inMargin = 0;
                ForEachPixel(cell, (x, y) =>
                {
                    byte a = pixels[Index(cell, x, y)].a;
                    bool edge = x < marginX || y < marginY || x >= CellWidth - marginX || y >= CellHeight - marginY;
                    if (a > 0 && edge)
                    {
                        inMargin++;
                    }
                    else if (a > 127)
                    {
                        drawn++;
                    }
                });
                if (inMargin > 0 || drawn < CellWidth * CellHeight / 80)
                {
                    problems.Add($"{AtlasPath}: cell {(FaceExpression)cell} must draw its face ({drawn} px) and keep a {marginX} x {marginY} px margin empty ({inMargin} px in it)");
                }
            }

            return problems;
        }

        /// <summary>Index into the atlas pixels (Unity order: row 0 at the bottom) of (x, y) in a cell, y from the cell's top.</summary>
        public static int Index(int cell, int x, int y)
        {
            int column = cell % FaceRules.AtlasColumns;
            int row = cell / FaceRules.AtlasColumns;
            int width = CellWidth * FaceRules.AtlasColumns;
            int top = CellHeight * FaceRules.AtlasRows - 1 - row * CellHeight;
            return (top - y) * width + column * CellWidth + x;
        }

        public static void ForEachPixel(int cell, Action<int, int> visit)
        {
            for (int y = 0; y < CellHeight; y++)
            {
                for (int x = 0; x < CellWidth; x++)
                {
                    visit(x, y);
                }
            }
        }

        /// <summary>The atlas pixels as written (no import), Unity order.</summary>
        public static Color32[] ReadPixels(byte[] png, out int width, out int height)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                texture.LoadImage(png);
                width = texture.width;
                height = texture.height;
                return texture.GetPixels32();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        public static Color32[] ReadPixels(out int width, out int height) => ReadPixels(File.ReadAllBytes(AtlasPath), out width, out height);

        public static string ScriptHash()
        {
            byte[] bytes = File.ReadAllBytes(ScriptPath);
            var text = new List<byte>(bytes.Length);
            for (int i = 0; i < bytes.Length; i++)
            {
                if (!(bytes[i] == '\r' && i + 1 < bytes.Length && bytes[i + 1] == '\n'))
                {
                    text.Add(bytes[i]);
                }
            }

            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(text.ToArray())).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>The value of a PNG tEXt chunk, or null.</summary>
        public static string PngText(byte[] png, string key)
        {
            int at = 8;
            while (at + 8 <= png.Length)
            {
                int length = (png[at] << 24) | (png[at + 1] << 16) | (png[at + 2] << 8) | png[at + 3];
                string type = Encoding.ASCII.GetString(png, at + 4, 4);
                if (length < 0 || at + 12 + length > png.Length)
                {
                    return null;
                }

                if (type == "tEXt")
                {
                    string text = Encoding.ASCII.GetString(png, at + 8, length);
                    int zero = text.IndexOf('\0');
                    if (zero > 0 && text.Substring(0, zero) == key)
                    {
                        return text.Substring(zero + 1);
                    }
                }

                if (type == "IEND")
                {
                    return null;
                }

                at += 12 + length;
            }

            return null;
        }

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("missing " + path);
        }
    }
}
