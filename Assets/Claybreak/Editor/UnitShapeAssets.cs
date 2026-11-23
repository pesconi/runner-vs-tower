using System;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The generated crowd shape meshes (RT-6t.2; RT-6t.20 added Coelhinho, Sapinho and Pinguim), each with and without eyes, written
    /// as mesh assets under <see cref="Folder"/> from <see cref="UnitShapeRecipes"/>. Deterministic and idempotent (assets
    /// keep their GUIDs). Runs inside <see cref="ToonLabBuilder.Build"/> (so inside the scene builder too).
    /// The validator checks every entry (<see cref="ClaybreakValidation"/>).
    /// </summary>
    public static class UnitShapeAssets
    {
        public const string Folder = "Assets/Claybreak/Content/Generated/Units";

        public sealed class Entry
        {
            public Entry(string id, bool eyes, Func<Mesh> build, Color32? frontMarker)
            {
                Id = id;
                HasEyes = eyes;
                Build = build;
                FrontMarker = frontMarker;
            }

            /// <summary>Asset name (also the mesh name).</summary>
            public string Id { get; }
            public bool HasEyes { get; }
            public Func<Mesh> Build { get; }

            /// <summary>Fixed color whose frontmost vertex must stick out on +Z (null = symmetric shape, nothing to check).</summary>
            public Color32? FrontMarker { get; }
            public string Path => Folder + "/" + Id + ".asset";
        }

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
        };

        /// <summary>The generated entry called <paramref name="id"/> (an unknown id is a programming error).</summary>
        public static Entry Named(string id)
        {
            return Array.Find(All, entry => entry.Id == id) ?? throw new ArgumentException($"no unit shape '{id}'", nameof(id));
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
