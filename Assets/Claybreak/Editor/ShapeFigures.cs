using System.IO;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The shop figures of the crowd shapes (RT-6t.20, D31 A): each shape's mesh drawn once in the editor, in a three-quarter
    /// view from a little above, flat-shaded in three toon bands with a dark outline, into two 256 px sprites: the body
    /// layer (white shading; the shop tints it with the army's color) and the detail layer (eyes, beak, belly, outline in
    /// their own colors, drawn over it). A small software rasterizer, so it runs in batchmode without a GPU
    /// (<c>-nographics</c>) and gives the same bytes every time (a rebuild never touches git when nothing changed).
    /// Nothing is drawn in the game: the shop shows two images. RT-6v3.4b: a shape with a detailed level (the Bibopzi) is
    /// drawn from it, and its painted face (UV2.w mask, UV0 in the face cell) takes the neutral cell of the face atlas, the
    /// same face the game shows (direcao-visual-v3.md 3.1: "o detalhado nas telas").
    /// </summary>
    public static class ShapeFigures
    {
        public const string Folder = "Assets/Claybreak/Content/Generated/ShapeFigures";
        public const int Size = 256;

        private const int Supersample = 3;
        private const float Fill = 0.84f;
        private const float YawDegrees = -30f;
        private const float PitchDegrees = 16f;
        private static readonly Vector3 Light = new Vector3(-0.45f, 0.75f, 0.55f).normalized;
        private static readonly Color32 Ink = new Color32(0x2b, 0x22, 0x33, 255);

        public static string BodyPath(string id) => $"{Folder}/{id}-body.png";
        public static string DetailPath(string id) => $"{Folder}/{id}-detail.png";

        /// <summary>Writes (when changed) and imports the two figure sprites of every catalog shape.</summary>
        public static void Build()
        {
            EnsureFolder();
            foreach (WorldContent.ShapeDefinition shape in WorldContent.Shapes)
            {
                Mesh mesh = UnitShapeAssets.Load(shape.LeaderAsset ?? shape.Asset);
                if (mesh == null)
                {
                    throw new System.InvalidOperationException($"shape '{shape.Id}': build the unit shapes first");
                }

                Color32[] atlas = null;
                int atlasWidth = 0;
                int atlasHeight = 0;
                if ((shape.LeaderAsset ?? shape.Asset).PaintedFace)
                {
                    atlas = BibopziFaceArt.ReadPixels(out atlasWidth, out atlasHeight);
                }

                Render(mesh, Size, out Color32[] body, out Color32[] detail, atlas, atlasWidth, atlasHeight);
                Write(BodyPath(shape.Id), body);
                Write(DetailPath(shape.Id), detail);
            }
        }

        public static Sprite LoadBody(string id) => AssetDatabase.LoadAssetAtPath<Sprite>(BodyPath(id));
        public static Sprite LoadDetail(string id) => AssetDatabase.LoadAssetAtPath<Sprite>(DetailPath(id));

        /// <summary>
        /// Draws <paramref name="mesh"/> into two <paramref name="size"/>² layers (row 0 at the bottom, like a texture):
        /// body pixels (vertex alpha 255) as white shading, every other part and the outline in its own color.
        /// </summary>
        public static void Render(Mesh mesh, int size, out Color32[] body, out Color32[] detail, Color32[] faceAtlas = null,
            int atlasWidth = 0, int atlasHeight = 0, FaceExpression face = FaceExpression.Neutral)
        {
            int big = size * Supersample;
            Vector3[] source = mesh.vertices;
            Color32[] colors = mesh.colors32;
            int[] triangles = mesh.triangles;
            var uv = new System.Collections.Generic.List<Vector2>();
            var marks = new System.Collections.Generic.List<Vector4>();
            if (faceAtlas != null)
            {
                mesh.GetUVs(0, uv);
                mesh.GetUVs(1, marks);
                if (uv.Count != source.Length || marks.Count != source.Length)
                {
                    faceAtlas = null;
                }
            }
            // Camera space as Unity's: the camera at −Z looking along +Z (right +X, up +Y). The shape turns half around to
            // face it (its front is +Z), a little more for the three-quarter view, and its top tilts toward the camera.
            Quaternion view = Quaternion.Euler(-PitchDegrees, 0f, 0f) * Quaternion.Euler(0f, 180f + YawDegrees, 0f);
            var points = new Vector3[source.Length];
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < source.Length; i++)
            {
                points[i] = view * source[i];
                min = Vector2.Min(min, points[i]);
                max = Vector2.Max(max, points[i]);
            }

            // Fit the figure in the square, centered (the same margins for every shape: the shop rows line up).
            float scale = Fill * big / Mathf.Max(max.x - min.x, max.y - min.y);
            Vector2 center = (min + max) * 0.5f;
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Vector3((points[i].x - center.x) * scale + big * 0.5f, (points[i].y - center.y) * scale + big * 0.5f,
                    points[i].z);
            }

            // Depth test: nearer the camera is smaller z.
            var depth = new float[big * big];
            var owner = new int[big * big];
            for (int i = 0; i < depth.Length; i++)
            {
                depth[i] = float.MaxValue;
                owner[i] = -1;
            }

            for (int t = 0; t < triangles.Length; t += 3)
            {
                Rasterize(points[triangles[t]], points[triangles[t + 1]], points[triangles[t + 2]], t / 3, big, depth, owner);
            }

            int triangleCount = triangles.Length / 3;
            var shade = new float[triangleCount];
            var isBody = new bool[triangleCount];
            var tone = new Color32[triangleCount];
            for (int f = 0; f < triangleCount; f++)
            {
                Vector3 a = view * source[triangles[f * 3]];
                Vector3 n = Vector3.Cross(view * source[triangles[f * 3 + 1]] - a, view * source[triangles[f * 3 + 2]] - a).normalized;
                if (n.z > 0f)
                {
                    n = -n;
                }

                // Toward the viewer is −Z here; the light comes from the viewer's upper left.
                float lit = Vector3.Dot(new Vector3(n.x, n.y, -n.z), Light);
                shade[f] = lit > 0.55f ? 1f : lit > 0.15f ? 0.86f : 0.72f;
                Color32 c = colors[triangles[f * 3]];
                isBody[f] = c.a == 255;
                tone[f] = c;
            }

            int outline = 2 * Supersample;
            body = new Color32[size * size];
            detail = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector4 bodySum = Vector4.zero;
                    Vector4 detailSum = Vector4.zero;
                    for (int sy = 0; sy < Supersample; sy++)
                    {
                        for (int sx = 0; sx < Supersample; sx++)
                        {
                            int px = x * Supersample + sx;
                            int py = y * Supersample + sy;
                            int f = owner[py * big + px];
                            if (f >= 0)
                            {
                                float s = shade[f];
                                Color32 painted = faceAtlas == null || !isBody[f]
                                    ? default
                                    : Face(points, triangles, f, px + 0.5f, py + 0.5f, uv, marks, faceAtlas, atlasWidth, atlasHeight, face);
                                if (isBody[f] && painted.a > 0)
                                {
                                    float a = painted.a / 255f;
                                    bodySum += new Vector4(s, s, s, 1f) * (1f - a);
                                    detailSum += new Vector4(painted.r / 255f * s, painted.g / 255f * s, painted.b / 255f * s, 1f) * a;
                                }
                                else if (isBody[f])
                                {
                                    bodySum += new Vector4(s, s, s, 1f);
                                }
                                else
                                {
                                    detailSum += new Vector4(tone[f].r / 255f * s, tone[f].g / 255f * s, tone[f].b / 255f * s, 1f);
                                }
                            }
                            else if (Near(owner, big, px, py, outline))
                            {
                                detailSum += new Vector4(Ink.r / 255f, Ink.g / 255f, Ink.b / 255f, 1f);
                            }
                        }
                    }

                    float samples = Supersample * Supersample;
                    body[y * size + x] = Straight(bodySum, samples);
                    detail[y * size + x] = Straight(detailSum, samples);
                }
            }
        }

        // The face atlas at a sample of triangle f: its UV and mask interpolated there, the cell's pixel (nearest), the mask
        // times the cell's alpha as alpha. Same mapping as the kit's shader (ToonVertexMotion.FaceUV, inset of BibopziFaceArt).
        private static Color32 Face(Vector3[] points, int[] triangles, int f, float x, float y, System.Collections.Generic.List<Vector2> uv,
            System.Collections.Generic.List<Vector4> marks, Color32[] atlas, int width, int height, FaceExpression face)
        {
            int ia = triangles[f * 3];
            int ib = triangles[f * 3 + 1];
            int ic = triangles[f * 3 + 2];
            Vector3 a = points[ia];
            Vector3 b = points[ib];
            Vector3 c = points[ic];
            var p = new Vector3(x, y, 0f);
            float area = Edge(a, b, c);
            if (Mathf.Abs(area) < 1e-9f)
            {
                return default;
            }

            float w0 = Edge(b, c, p) / area;
            float w1 = Edge(c, a, p) / area;
            float w2 = 1f - w0 - w1;
            float mask = Mathf.Clamp01(w0 * marks[ia].w + w1 * marks[ib].w + w2 * marks[ic].w);
            if (mask <= 0f)
            {
                return default;
            }

            Vector2 local = w0 * uv[ia] + w1 * uv[ib] + w2 * uv[ic];
            Unity.Mathematics.float2 atlasUv = JungleDegree.StudioKit.Toon.ToonVertexMotion.FaceUV(local, (int)face,
                FaceRules.AtlasColumns, FaceRules.AtlasRows, BibopziFaceArt.Inset);
            int px = Mathf.Clamp(Mathf.FloorToInt(atlasUv.x * width), 0, width - 1);
            int py = Mathf.Clamp(Mathf.FloorToInt(atlasUv.y * height), 0, height - 1);
            Color32 texel = atlas[py * width + px];
            texel.a = (byte)Mathf.RoundToInt(texel.a * mask);
            return texel;
        }

        // Fills the pixels whose centers are inside the triangle (either winding), nearest surface first.
        private static void Rasterize(Vector3 a, Vector3 b, Vector3 c, int face, int big, float[] depth, int[] owner)
        {
            float area = Edge(a, b, c);
            if (Mathf.Abs(area) < 1e-9f)
            {
                return;
            }

            int x0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
            int x1 = Mathf.Min(big - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
            int y1 = Mathf.Min(big - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    var p = new Vector3(x + 0.5f, y + 0.5f, 0f);
                    float w0 = Edge(b, c, p) / area;
                    float w1 = Edge(c, a, p) / area;
                    float w2 = Edge(a, b, p) / area;
                    if (w0 < 0f || w1 < 0f || w2 < 0f)
                    {
                        continue;
                    }

                    float z = w0 * a.z + w1 * b.z + w2 * c.z;
                    int index = y * big + x;
                    if (z < depth[index])
                    {
                        depth[index] = z;
                        owner[index] = face;
                    }
                }
            }
        }

        private static float Edge(Vector3 a, Vector3 b, Vector3 p)
        {
            return (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
        }

        private static bool Near(int[] owner, int big, int px, int py, int radius)
        {
            int r2 = radius * radius;
            for (int dy = -radius; dy <= radius; dy++)
            {
                int y = py + dy;
                if (y < 0 || y >= big)
                {
                    continue;
                }

                for (int dx = -radius; dx <= radius; dx++)
                {
                    int x = px + dx;
                    if (x >= 0 && x < big && dx * dx + dy * dy <= r2 && owner[y * big + x] >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static Color32 Straight(Vector4 premultiplied, float samples)
        {
            if (premultiplied.w <= 0f)
            {
                return new Color32(0, 0, 0, 0);
            }

            float a = premultiplied.w / samples;
            Vector3 rgb = new Vector3(premultiplied.x, premultiplied.y, premultiplied.z) / premultiplied.w;
            return new Color32((byte)Mathf.RoundToInt(rgb.x * 255f), (byte)Mathf.RoundToInt(rgb.y * 255f),
                (byte)Mathf.RoundToInt(rgb.z * 255f), (byte)Mathf.RoundToInt(a * 255f));
        }

        private static void Write(string path, Color32[] pixels)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            byte[] png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);
            if (!File.Exists(path) || !SameBytes(File.ReadAllBytes(path), png))
            {
                File.WriteAllBytes(path, png);
                AssetDatabase.ImportAsset(path);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.textureType != TextureImporterType.Sprite || importer.mipmapEnabled
                || importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static bool SameBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(Folder).Replace('\\', '/'), Path.GetFileName(Folder));
            }
        }
    }
}
