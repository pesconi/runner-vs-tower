using System;
using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Claybreak.Editor
{
    /// <summary>
    /// Copies of Unity's cube, cylinder and sphere with white vertex colors (alpha 1 = takes the tint) for
    /// <c>StudioKit/ToonLit</c>, which reads the vertex color: Unity's own primitives have none, and a missing stream is
    /// not guaranteed to read white on every graphics API (RT-6t.10). Same vertices, normals and triangles; assets in
    /// Generated, rewritten in place so references keep their ids.
    /// </summary>
    public static class ToonPrimitives
    {
        public const string Folder = ClaybreakSceneBuilder.Root + "/Generated";

        private static readonly PrimitiveType[] Types = { PrimitiveType.Cube, PrimitiveType.Cylinder, PrimitiveType.Sphere };

        public static string PathOf(PrimitiveType type) => $"{Folder}/Toon{type}.asset";

        /// <summary>Writes the three meshes.</summary>
        public static void BuildAssets()
        {
            ClaybreakSceneBuilder.EnsureFolder(Folder);
            foreach (PrimitiveType type in Types)
            {
                GameObject primitive = GameObject.CreatePrimitive(type);
                Mesh source = primitive.GetComponent<MeshFilter>().sharedMesh;
                string path = PathOf(type);
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                bool created = mesh == null;
                if (created)
                {
                    mesh = new Mesh();
                }

                mesh.Clear();
                mesh.name = "Toon" + type;
                mesh.vertices = source.vertices;
                mesh.normals = source.normals;
                mesh.triangles = source.triangles;
                var colors = new Color32[source.vertexCount];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color32(255, 255, 255, 255);
                }

                mesh.colors32 = colors;
                mesh.RecalculateBounds();
                Object.DestroyImmediate(primitive);
                if (created)
                {
                    AssetDatabase.CreateAsset(mesh, path);
                }
                else
                {
                    EditorUtility.SetDirty(mesh);
                }
            }
        }

        /// <summary>The toon copy of <paramref name="type"/> (built on demand).</summary>
        public static Mesh Get(PrimitiveType type)
        {
            if (Array.IndexOf(Types, type) < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, "no toon copy of this primitive");
            }

            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(PathOf(type));
            if (mesh == null)
            {
                BuildAssets();
                mesh = AssetDatabase.LoadAssetAtPath<Mesh>(PathOf(type));
            }

            return mesh;
        }

        /// <summary>True when <paramref name="material"/> draws with <c>StudioKit/ToonLit</c> or its vinyl sibling (needs vertex colors).</summary>
        public static bool IsToon(Material material)
        {
            return material != null && material.shader != null && WorldScenery.IsToonFamily(material.shader.name);
        }
    }
}
