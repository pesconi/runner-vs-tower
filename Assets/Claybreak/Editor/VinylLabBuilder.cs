using Claybreak.Lab;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Editor
{
    /// <summary>
    /// RT-6v3.2 laboratory assets of the vinyl directions (decision D36), next to the toon lab and outside the game content
    /// (<c>Assets/Claybreak/Lab/Generated</c>, so the style guide and the game scene never see them): the smoothed Bibopzi
    /// (normals smoothed under 50°, then one PN subdivision: 4× the triangles), the instanced material on
    /// <c>Claybreak/VinylLab</c> (it carries the shader and its instancing variants into the measurement player) and one post
    /// profile per direction (<see cref="VinylDirection.FillProfile"/>). <see cref="Wire"/> puts them into the toon lab scene
    /// with a global volume that the lab turns on only for <c>--ez post true</c>. Runs inside <see cref="ToonLabBuilder.Build"/>.
    /// </summary>
    public static class VinylLabBuilder
    {
        public const string Folder = ToonLabBuilder.GeneratedFolder;
        public const string UnitPath = Folder + "/BibopziVinyl.asset";
        public const string MaterialPath = Folder + "/VinylCrowd.mat";
        public const string BibopziPath = UnitShapeAssets.Folder + "/Bibopzi.asset";
        public const float SmoothAngle = 50f;

        public static string ProfilePath(int direction)
        {
            return $"{Folder}/VinylPost{VinylDirection.All[direction].id}.asset";
        }

        public static void BuildAssets()
        {
            ToonLabBuilder.EnsureFolder(Folder);
            Mesh bibopzi = ToonLabBuilder.Load<Mesh>(BibopziPath);
            Mesh smooth = VinylMeshSmoothing.SmoothNormals(bibopzi, SmoothAngle);
            Mesh unit = VinylMeshSmoothing.Subdivide(smooth);
            Object.DestroyImmediate(smooth);
            unit.name = "BibopziVinyl";
            ToonLabBuilder.SaveMesh(unit, UnitPath);
            Shader vinyl = Shader.Find(VinylDirection.ShaderName);
            if (vinyl == null)
            {
                throw new System.InvalidOperationException("shader not found: " + VinylDirection.ShaderName);
            }

            ToonLabBuilder.SaveMaterial(MaterialPath, vinyl, material =>
            {
                material.EnableKeyword("STUDIOKIT_TOON_INSTANCES");
                material.SetColor("_TintColor", Color.white);
                material.SetFloat(VinylDirection.Hero, 1f);
                material.enableInstancing = true;
            });
            for (int i = 0; i < VinylDirection.All.Length; i++)
            {
                SaveProfile(VinylDirection.All[i], ProfilePath(i));
            }

            Debug.Log($"[VinylLab] Bibopzi {bibopzi.triangles.Length / 3} tris -> vinyl unit {ToonLabBuilder.Load<Mesh>(UnitPath).triangles.Length / 3} tris");
        }

        private static void SaveProfile(VinylDirection direction, string path)
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            direction.FillProfile(profile);
            foreach (VolumeComponent component in profile.components)
            {
                if (!AssetDatabase.Contains(component))
                {
                    component.name = component.GetType().Name;
                    component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(component, profile);
                }

                EditorUtility.SetDirty(component);
            }

            // Sub-assets left behind by a removed override (bloom off) are deleted with it.
            foreach (Object sub in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (sub is VolumeComponent component && !profile.components.Contains(component))
                {
                    Object.DestroyImmediate(component, true);
                }
            }

            EditorUtility.SetDirty(profile);
        }

        /// <summary>Gives the toon lab its vinyl references and a disabled global volume.</summary>
        public static void Wire(ToonLab lab, Light sun)
        {
            var volumeObject = new GameObject("VinylVolume");
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.enabled = false;
            var so = new SerializedObject(lab);
            so.FindProperty("vinylMaterial").objectReferenceValue = ToonLabBuilder.Load<Material>(MaterialPath);
            so.FindProperty("vinylUnit").objectReferenceValue = ToonLabBuilder.Load<Mesh>(UnitPath);
            SerializedProperty profiles = so.FindProperty("vinylProfiles");
            profiles.arraySize = VinylDirection.All.Length;
            for (int i = 0; i < VinylDirection.All.Length; i++)
            {
                profiles.GetArrayElementAtIndex(i).objectReferenceValue = ToonLabBuilder.Load<VolumeProfile>(ProfilePath(i));
            }

            so.FindProperty("vinylVolume").objectReferenceValue = volume;
            so.FindProperty("sun").objectReferenceValue = sun;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
