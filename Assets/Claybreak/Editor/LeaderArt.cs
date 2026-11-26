using System;
using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Claybreak.Editor
{
    /// <summary>
    /// The crowd leader's crown (RT-6t.13): a low-poly band with five points and a white jewel on each, about 1 m across
    /// with its base at y = 0 (<see cref="CrowdLeader"/> scales it to the unit and sets it on the head). The band and points
    /// are white with vertex alpha 1, so they take the crown color (the world's UI accent) as the instance tint; the jewels
    /// keep their white (alpha 0). Drawn instanced with <c>StudioKit/ToonLit</c> and a little emission, so the crown reads on
    /// every world's light. Assets in Generated/Units, rewritten in place so references keep their ids.
    /// </summary>
    public static class LeaderArt
    {
        public const string CrownMeshPath = UnitShapeAssets.Folder + "/LeaderCrown.asset";
        public const string CrownMaterialPath = UnitShapeAssets.Folder + "/LeaderCrown.mat";

        /// <summary>Points on the crown.</summary>
        public const int Points = 5;

        /// <summary>Emission of the crown (0 lit, 1 unlit).</summary>
        public const float CrownEmission = 0.5f;

        public static void BuildAssets()
        {
            SaveMesh(Crown(), CrownMeshPath);
            Shader toon = Shader.Find(ToonShaderIds.ToonLitShader) ?? throw new InvalidOperationException("missing " + ToonShaderIds.ToonLitShader);
            var material = AssetDatabase.LoadAssetAtPath<Material>(CrownMaterialPath);
            if (material == null)
            {
                material = new Material(toon);
                AssetDatabase.CreateAsset(material, CrownMaterialPath);
            }

            material.shader = StyleGuideContent.KeepVinyl(material, toon);
            material.EnableKeyword(ToonShaderIds.InstancesKeyword);
            material.DisableKeyword(ToonShaderIds.SlotsKeyword);
            material.SetColor(ToonShaderIds.TintColor, Color.white);
            material.SetFloat(ToonShaderIds.Emission, CrownEmission);
            material.enableInstancing = true; // the fallback route draws it with RenderMeshInstanced (RT-6t.11)
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
        }

        /// <summary>The crown mesh: band (radius 0.5, 0.32 high), five cone points around its top edge, a jewel on each.</summary>
        public static Mesh Crown()
        {
            var tinted = new Color32(255, 255, 255, 255);
            var jewel = new Color32(255, 255, 255, 0);
            var builder = new LowPolyMeshBuilder();
            const float radius = 0.5f;
            const float band = 0.32f;
            builder.AddCylinder(LowPolyMeshBuilder.Place(new Vector3(0f, band * 0.5f, 0f), new Vector3(radius, band, radius)), 10, tinted);
            for (int i = 0; i < Points; i++)
            {
                float angle = i * Mathf.PI * 2f / Points;
                var around = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * (radius * 0.82f);
                builder.AddCone(LowPolyMeshBuilder.Place(around + new Vector3(0f, band, 0f), new Vector3(0.17f, 0.3f, 0.17f)), 6, tinted, false);
                builder.AddSphere(LowPolyMeshBuilder.Place(around * 1.1f + new Vector3(0f, band * 0.55f, 0f), Vector3.one * 0.09f), 6, 4, jewel);
            }

            return builder.ToMesh("LeaderCrown");
        }

        private static void SaveMesh(Mesh mesh, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return;
            }

            existing.Clear();
            EditorUtility.CopySerialized(mesh, existing);
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
        }
    }
}
