using System;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The generated crowd shape meshes (RT-6t.2; RT-6t.20 added Coelhinho, Sapinho and Pinguim), written as mesh assets under
    /// <see cref="Folder"/>. Legacy shapes come from <see cref="UnitShapeRecipes"/>, each with and without eyes. RT-6v3.4a: the
    /// Bibopzi comes in three levels written by Blender as simple data (<see cref="SimpleMeshData"/>, <c>Tools/blender</c>):
    /// <see cref="ShapeLevel.Crowd"/> (the 300 units, asset "Bibopzi", same GUID as the RT-6t.22 recipe it replaces),
    /// <see cref="ShapeLevel.Detail"/> (the leader) and <see cref="ShapeLevel.Reserve"/> (the gate's fallback).
    /// Deterministic and idempotent (assets keep their GUIDs). Runs inside <see cref="ToonLabBuilder.Build"/> (so inside the
    /// scene builder too). The validator checks every entry (<see cref="ClaybreakValidation"/>).
    /// </summary>
    public static class UnitShapeAssets
    {
        public const string Folder = "Assets/Claybreak/Content/Generated/Units";

        /// <summary>Which budget a shape mesh answers to (direcao-visual-v3.md 3.1 and the style guide's class table).</summary>
        public enum ShapeLevel
        {
            /// <summary>The RT-6t recipes: 200-500 triangles, flat low poly.</summary>
            Legacy,
            /// <summary>v3 crowd unit: the guide's Unit range (800-1200).</summary>
            Crowd,
            /// <summary>v3 detailed: the guide's Leader range (3000-5000).</summary>
            Detail,
            /// <summary>v3 reserve (~500, smooth): the guide's Unit reserve range.</summary>
            Reserve,
        }

        /// <summary>Triangle range of a level (the v3 ones mirror the style guide; StyleGuideTests checks they agree).</summary>
        public static Vector2Int Budget(ShapeLevel level)
        {
            switch (level)
            {
                case ShapeLevel.Crowd: return new Vector2Int(800, 1200);
                case ShapeLevel.Detail: return new Vector2Int(3000, 5000);
                case ShapeLevel.Reserve: return new Vector2Int(450, 550);
                default: return new Vector2Int(UnitShapeRecipes.MinimumTriangles, UnitShapeRecipes.MaximumTriangles);
            }
        }

        public sealed class Entry
        {
            public Entry(string id, bool eyes, Func<Mesh> build, Color32? frontMarker)
            {
                Id = id;
                HasEyes = eyes;
                Build = build;
                FrontMarker = frontMarker;
                Level = ShapeLevel.Legacy;
            }

            /// <summary>A shape written by Blender as simple data (<paramref name="data"/>, project-relative); always with eyes.</summary>
            public Entry(string id, ShapeLevel level, string data, Color32? frontMarker)
            {
                Id = id;
                HasEyes = true;
                Level = level;
                DataPath = data;
                FrontMarker = frontMarker;
                Build = () =>
                {
                    Mesh mesh = SimpleMeshData.Read(data).ToMesh();
                    mesh.name = id;
                    return mesh;
                };
            }

            /// <summary>Asset name (also the mesh name).</summary>
            public string Id { get; }
            public bool HasEyes { get; }
            public Func<Mesh> Build { get; }
            public ShapeLevel Level { get; }

            /// <summary>The simple-data file of a Blender shape (project-relative), null for a recipe.</summary>
            public string DataPath { get; }

            /// <summary>Fixed color whose frontmost vertex must stick out on +Z (null = symmetric shape, nothing to check).</summary>
            public Color32? FrontMarker { get; }
            public string Path => Folder + "/" + Id + ".asset";
            public Vector2Int Budget => UnitShapeAssets.Budget(Level);
        }

        private const string BibopziData = SimpleMeshData.OutputFolder + "/bibopzi_";

        /// <summary>Lab order: shape × eyes (the first is the provisional default while D25 is open).</summary>
        public static readonly Entry[] All =
        {
            new Entry("Bolinha", true, () => UnitShapeRecipes.Bolinha(true), UnitShapeRecipes.Eyes),
            new Entry("BolinhaNoEyes", false, () => UnitShapeRecipes.Bolinha(false), null),
            new Entry("Pintinho", true, () => UnitShapeRecipes.Pintinho(true), UnitShapeRecipes.Orange),
            new Entry("PintinhoNoEyes", false, () => UnitShapeRecipes.Pintinho(false), UnitShapeRecipes.Orange),
            // RT-6t.20: the three shapes sold in the shop (D31 A).
            new Entry("Coelhinho", true, () => UnitShapeRecipes.Coelhinho(true), UnitShapeRecipes.Eyes),
            new Entry("CoelhinhoNoEyes", false, () => UnitShapeRecipes.Coelhinho(false), null),
            new Entry("Sapinho", true, () => UnitShapeRecipes.Sapinho(true), UnitShapeRecipes.Eyes),
            new Entry("SapinhoNoEyes", false, () => UnitShapeRecipes.Sapinho(false), null),
            new Entry("Pinguim", true, () => UnitShapeRecipes.Pinguim(true), UnitShapeRecipes.Orange),
            new Entry("PinguimNoEyes", false, () => UnitShapeRecipes.Pinguim(false), UnitShapeRecipes.Orange),
            // 2026-11-25: Bibopzi, the default shape (user's decision); its feet stick out at the front. RT-6v3.4a: from Blender,
            // three levels (the eyeless variant of the RT-6t.22 recipe is gone: D25 A settled on eyes, the reserve is a level).
            new Entry("Bibopzi", ShapeLevel.Crowd, BibopziData + "crowd.json", UnitShapeRecipes.BibopziOrange),
            new Entry("BibopziDetail", ShapeLevel.Detail, BibopziData + "detail.json", UnitShapeRecipes.BibopziOrange),
            new Entry("BibopziReserve", ShapeLevel.Reserve, BibopziData + "reserve.json", UnitShapeRecipes.BibopziOrange),
        };

        /// <summary>The generated entry called <paramref name="id"/> (an unknown id is a programming error).</summary>
        public static Entry Named(string id)
        {
            return Array.Find(All, entry => entry.Id == id) ?? throw new ArgumentException($"no unit shape '{id}'", nameof(id));
        }

        /// <summary>The entry whose asset is at <paramref name="path"/>, or null.</summary>
        public static Entry AtPath(string path)
        {
            return Array.Find(All, entry => entry.Path == path);
        }

        [MenuItem("Claybreak/Build Unit Shapes")]
        public static void Build()
        {
            EnsureFolder(Folder);
            foreach (Entry entry in All)
            {
                Save(entry.Build(), entry.Path);
            }

            AssetDatabase.SaveAssets();
        }

        public static Mesh Load(Entry entry)
        {
            return AssetDatabase.LoadAssetAtPath<Mesh>(entry.Path);
        }

        private static void Save(Mesh mesh, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return;
            }

            existing.Clear();
            EditorUtility.CopySerialized(mesh, existing);
            UnityEngine.Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}
