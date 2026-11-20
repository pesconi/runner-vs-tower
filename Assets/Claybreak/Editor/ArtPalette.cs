using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Editor
{
    /// <summary>
    /// Generated flat materials (URP Simple Lit; glow and translucent ones on URP Unlit). Adult palette
    /// (GDD 9): warm darks, no candy colors. One shared instance per build; materials are assets, reused.
    /// </summary>
    public sealed class ArtPalette
    {
        public const string MaterialFolder = ClaybreakSceneBuilder.Root + "/Materials";

        public readonly Material Ground = Solid("Ground", new Color(0.34f, 0.27f, 0.24f));
        public readonly Material Field = Solid("Field", new Color(0.2f, 0.17f, 0.17f));
        public readonly Material Stone = Solid("Stone", new Color(0.46f, 0.42f, 0.42f));
        public readonly Material StoneDark = Solid("StoneDark", new Color(0.24f, 0.21f, 0.22f));
        public readonly Material StoneWarm = Solid("StoneWarm", new Color(0.58f, 0.5f, 0.44f));
        public readonly Material Iron = Solid("Iron", new Color(0.12f, 0.12f, 0.13f));
        public readonly Material IronWarm = Solid("IronWarm", new Color(0.22f, 0.19f, 0.18f));
        public readonly Material Wood = Solid("Wood", new Color(0.3f, 0.19f, 0.12f));
        public readonly Material Banner = Solid("Banner", new Color(0.55f, 0.1f, 0.08f));
        public readonly Material Terracotta = Solid("Terracotta", new Color(0.66f, 0.36f, 0.24f));
        public readonly Material ClayDark = Solid("ClayDark", new Color(0.4f, 0.2f, 0.14f));
        public readonly Material Moss = Solid("Moss", new Color(0.26f, 0.32f, 0.16f));
        public readonly Material Bone = Solid("Bone", new Color(0.82f, 0.76f, 0.64f));
        public readonly Material Wall = Solid("Wall", new Color(0.55f, 0.36f, 0.24f));
        public readonly Material Health = Solid("Health", new Color(0.86f, 0.2f, 0.16f));
        public readonly Material Line = Solid("Line", new Color(0.95f, 0.92f, 0.86f));
        public readonly Material Crate = Solid("Crate", new Color(0.45f, 0.13f, 0.1f));
        public readonly Material Glow = Unlit("Glow", new Color(1f, 0.55f, 0.15f));
        public readonly Material GlowHot = Unlit("GlowHot", new Color(1f, 0.82f, 0.4f));
        public readonly Material GateGood = Clear("GateGood", new Color(0.2f, 0.75f, 0.95f, 0.55f));
        public readonly Material GateBad = Clear("GateBad", new Color(0.92f, 0.25f, 0.2f, 0.55f));
        public readonly Material Marker = Clear("LaneMarker", new Color(1f, 0.8f, 0.25f, 0.35f));
        public readonly Material Shadow = Clear("Shadow", new Color(0f, 0f, 0f, 0.5f));
        public readonly Material Danger = Clear("Danger", new Color(0.95f, 0.3f, 0.15f, 0.35f));

        private static Material Solid(string name, Color color)
        {
            Material material = LoadOrCreate(name, "Universal Render Pipeline/Simple Lit");
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material Unlit(string name, Color color)
        {
            Material material = LoadOrCreate(name, "Universal Render Pipeline/Unlit");
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material Clear(string name, Color color)
        {
            Material material = LoadOrCreate(name, "Universal Render Pipeline/Unlit");
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreate(string name, string shaderName)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Shader shader = Shader.Find(shaderName) ?? throw new InvalidOperationException("Shader not found: " + shaderName);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                ClaybreakSceneBuilder.EnsureFolder(MaterialFolder);
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            return material;
        }
    }
}
