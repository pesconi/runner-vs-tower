using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Editor
{
    /// <summary>
    /// A mesh written as plain versioned data by the Blender scripts of <c>Tools/blender</c> (RT-6v3.4a, format
    /// <see cref="Format"/>): positions, normals, UV, UV2 (the kit's animated-layout marks), a palette index per vertex (linear
    /// color, tint flag = takes the skin) and triangle indices, already in Unity's frame and winding. Reading it and turning it
    /// into a mesh needs no Blender; <see cref="Problems"/> also checks that the data was written by the scripts as they are in
    /// the repository (their hash), so a script edited without re-running Blender fails the validator.
    /// </summary>
    public sealed class SimpleMeshData
    {
        public const string Format = "jungledegree.simple-mesh/1";

        /// <summary>The Blender scripts and their output, relative to the project root.</summary>
        public const string ScriptsFolder = "Tools/blender";
        public const string OutputFolder = ScriptsFolder + "/out";

        [Serializable]
        public sealed class PaletteColor
        {
            public string name;
            public string srgb;
            public float[] linear;
            public bool tint;
        }

        [Serializable]
        public sealed class Generator
        {
            public string[] scripts;
            public string sourceSha256;
            public string blender;
        }

        [Serializable]
        public sealed class Meta
        {
            public float headTop;
            public float springTop;
            public float[] keyPivot;
            public float[] keyAxis;
            public float[] faceBox;
        }

        [Serializable]
        private sealed class Raw
        {
            public string format;
            public string name;
            public string level;
            public Generator generator;
            public string colorSpace;
            public PaletteColor[] palette;
            public Meta meta;
            public int vertexCount;
            public int triangleCount;
            public float[] positions;
            public float[] normals;
            public float[] uv;
            public float[] uv2;
            public int[] colors;
            public int[] indices;
        }

        private readonly Raw raw;

        private SimpleMeshData(string path, Raw raw)
        {
            Path = path;
            this.raw = raw;
        }

        /// <summary>Project-relative path of the file.</summary>
        public string Path { get; }
        public string Name => raw.name;
        public string Level => raw.level;
        public Generator Source => raw.generator;
        public Meta Landmarks => raw.meta;
        public PaletteColor[] Palette => raw.palette;
        public int VertexCount => raw.vertexCount;
        public int TriangleCount => raw.triangleCount;

        /// <summary>The project root (the folder that holds Assets).</summary>
        public static string ProjectRoot => System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, ".."));

        /// <summary>Reads <paramref name="path"/> (project-relative). Throws when the file is missing or not this format.</summary>
        public static SimpleMeshData Read(string path)
        {
            string full = System.IO.Path.Combine(ProjectRoot, path);
            var raw = JsonUtility.FromJson<Raw>(File.ReadAllText(full));
            if (raw == null || raw.format != Format)
            {
                throw new InvalidDataException($"{path} is not a {Format} file");
            }

            return new SimpleMeshData(path, raw);
        }

        /// <summary>
        /// SHA-256 of the named scripts (line endings normalized to LF, as the Python side does), or null if one is missing.
        /// </summary>
        public static string SourceHash(IEnumerable<string> scripts)
        {
            using (SHA256 sha = SHA256.Create())
            {
                var all = new List<byte>();
                foreach (string script in scripts)
                {
                    string full = System.IO.Path.Combine(ProjectRoot, ScriptsFolder, script);
                    if (!File.Exists(full))
                    {
                        return null;
                    }

                    byte[] bytes = File.ReadAllBytes(full);
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (bytes[i] == (byte)'\r' && i + 1 < bytes.Length && bytes[i + 1] == (byte)'\n')
                        {
                            continue;
                        }

                        all.Add(bytes[i]);
                    }
                }

                byte[] hash = sha.ComputeHash(all.ToArray());
                var text = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    text.Append(b.ToString("x2"));
                }

                return text.ToString();
            }
        }

        /// <summary>The 8-bit sRGB color of a palette entry (what the game's vertex colors hold) and its tint mask in alpha.</summary>
        public static Color32 ToColor32(PaletteColor color)
        {
            byte Channel(int i) => (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.LinearToGammaSpace(color.linear[i]) * 255f), 0, 255);
            return new Color32(Channel(0), Channel(1), Channel(2), color.tint ? (byte)255 : (byte)0);
        }

        /// <summary>What is wrong with the data: format, sizes, indices, palette, and a source hash that no longer matches the scripts.</summary>
        public List<string> Problems()
        {
            var problems = new List<string>();
            int n = raw.vertexCount;
            if (raw.colorSpace != "linear")
            {
                problems.Add($"color space '{raw.colorSpace}', expected linear");
            }

            if (n <= 0 || n > 65535 || Size(raw.positions) != 3 * n || Size(raw.normals) != 3 * n || Size(raw.uv) != 2 * n
                || Size(raw.uv2) != 4 * n || Size(raw.colors) != n)
            {
                problems.Add($"vertex arrays do not match {n} vertices of 16-bit indices");
                return problems;
            }

            if (Size(raw.indices) != 3 * raw.triangleCount || raw.triangleCount == 0)
            {
                problems.Add($"{Size(raw.indices)} indices for {raw.triangleCount} triangles");
            }
            else if (Array.Exists(raw.indices, i => i < 0 || i >= n))
            {
                problems.Add("an index points outside the vertices");
            }

            if (raw.palette == null || raw.palette.Length == 0)
            {
                problems.Add("no palette");
            }
            else
            {
                foreach (PaletteColor color in raw.palette)
                {
                    if (color.linear == null || color.linear.Length != 3 || !ColorUtility.TryParseHtmlString(color.srgb, out Color sheet)
                        || !SameRgb(ToColor32(color), sheet))
                    {
                        problems.Add($"palette color '{color.name}' does not round-trip to its sRGB {color.srgb}");
                    }
                }

                if (Array.Exists(raw.colors, c => c < 0 || c >= raw.palette.Length))
                {
                    problems.Add("a vertex color points outside the palette");
                }
            }

            string expected = raw.generator?.scripts == null ? null : SourceHash(raw.generator.scripts);
            if (expected == null || expected != raw.generator.sourceSha256)
            {
                problems.Add($"{Path} was written by other scripts than those in {ScriptsFolder} (run Tools/blender/run.sh)");
            }

            return problems;
        }

        /// <summary>The mesh: one submesh, 16-bit indices, vertex colors with the tint mask in alpha, UV and the UV2 marks.</summary>
        public Mesh ToMesh()
        {
            int n = raw.vertexCount;
            var vertices = new Vector3[n];
            var normals = new Vector3[n];
            var uv = new Vector2[n];
            var marks = new List<Vector4>(n);
            var colors = new Color32[n];
            var palette = Array.ConvertAll(raw.palette, ToColor32);
            for (int i = 0; i < n; i++)
            {
                vertices[i] = new Vector3(raw.positions[3 * i], raw.positions[3 * i + 1], raw.positions[3 * i + 2]);
                normals[i] = new Vector3(raw.normals[3 * i], raw.normals[3 * i + 1], raw.normals[3 * i + 2]).normalized;
                uv[i] = new Vector2(raw.uv[2 * i], raw.uv[2 * i + 1]);
                marks.Add(new Vector4(raw.uv2[4 * i], raw.uv2[4 * i + 1], raw.uv2[4 * i + 2], raw.uv2[4 * i + 3]));
                colors[i] = palette[raw.colors[i]];
            }

            var mesh = new Mesh { name = raw.name, indexFormat = IndexFormat.UInt16 };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.SetUVs(1, marks);
            mesh.colors32 = colors;
            mesh.SetTriangles(raw.indices, 0, true);
            return mesh;
        }

        /// <summary>The key's spin pivot and axis (meta), for the kit's <c>ToonVinylMotion</c>.</summary>
        public Vector3 KeyPivot => ToVector(raw.meta.keyPivot);
        public Vector3 KeyAxis => ToVector(raw.meta.keyAxis);

        private static Vector3 ToVector(float[] v) => v != null && v.Length == 3 ? new Vector3(v[0], v[1], v[2]) : Vector3.zero;

        private static int Size<T>(T[] array) => array?.Length ?? -1;

        private static bool SameRgb(Color32 a, Color b)
        {
            Color32 c = b;
            return a.r == c.r && a.g == c.g && a.b == c.b;
        }
    }
}
