using System;
using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Editor
{
    /// <summary>
    /// The generated materials of the assault art (towers, bosses, defenses, projectiles, crates and their glows) and of the
    /// track line. RT-6t.10 (D28 = A, no answer): every opaque one is <c>StudioKit/ToonLit</c> (3-band ramp) whose color is a
    /// mix of the world palette's structure light / dark / detail and defense colors (<see cref="ToonShaderIds.StructureWeights"/>,
    /// globals written by the palette applier), so one material follows the four worlds with nothing per world; the old
    /// role names stay so the tower builders keep their shapes (no remodel, no faces). Glows are the same shader with
    /// emission. Colors that are game signals stay fixed: health bar, shield (cyan), danger strip, gate panels.
    /// The translucent ones stay on URP Unlit. One shared instance per build; materials are assets, reused.
    /// </summary>
    public sealed class ArtPalette
    {
        public const string MaterialFolder = ClaybreakSceneBuilder.Root + "/Materials";

        /// <summary>
        /// Fog taken by the structures (0..1): with the reference fog (22–75 m) a tower at the end of the track would be
        /// washed into the fog color (RT-6t.6a note), so they keep most of their colors. Calibrated on the captures.
        /// </summary>
        public const float StructureFog = 0.35f;

        // Structure weights: x light, y dark, z detail, w defense; what is left up to 1 is white (lightens).
        public readonly Material Stone = Toon("Stone", W(0.5f, 0.5f, 0f, 0f));
        public readonly Material StoneDark = Toon("StoneDark", W(0f, 1f, 0f, 0f));
        public readonly Material StoneLight = Toon("StoneLight", W(1f, 0f, 0f, 0f));
        public readonly Material StoneWarm = Toon("StoneWarm", W(0.6f, 0.25f, 0.15f, 0f));
        public readonly Material Iron = Toon("Iron", W(0f, 0f, 0f, 1f));
        public readonly Material IronWarm = Toon("IronWarm", W(0f, 0.4f, 0f, 0.6f));
        public readonly Material Wood = Toon("Wood", W(0f, 0.7f, 0f, 0.3f));
        public readonly Material Banner = Toon("Banner", W(0f, 0f, 1f, 0f));
        public readonly Material Terracotta = Toon("Terracotta", W(0.2f, 0.4f, 0.4f, 0f));
        public readonly Material ClayDark = Toon("ClayDark", W(0f, 0.6f, 0f, 0.4f));
        public readonly Material Moss = Toon("Moss", W(0f, 0.5f, 0f, 0.5f), shade: 0.9f);
        public readonly Material Bone = Toon("Bone", W(0.85f, 0f, 0f, 0f));
        public readonly Material Wall = Toon("Wall", W(0.45f, 0f, 0f, 0.55f));
        public readonly Material Health = Toon("Health", W(0f, 0f, 0f, 0f), new Color(0.86f, 0.2f, 0.16f), emission: 1f);
        public readonly Material Line = Toon("Line", W(0f, 0f, 0f, 0f), fog: 1f);
        public readonly Material Crate = Toon("Crate", W(0f, 0.25f, 0.75f, 0f));
        public readonly Material Glow = Toon("Glow", W(0f, 0f, 0.65f, 0f), emission: 1f);
        public readonly Material GlowHot = Toon("GlowHot", W(0f, 0f, 0.3f, 0f), emission: 1f);
        // RT-6t.9: the prototype's gate panels, #5ad1a0 (good) and #ff6b81 (bad) at 0.45, no depth write.
        public readonly Material GateGood = Clear("GateGood", new Color(0x5a / 255f, 0xd1 / 255f, 0xa0 / 255f, 0.45f));
        public readonly Material GateBad = Clear("GateBad", new Color(1f, 0x6b / 255f, 0x81 / 255f, 0.45f));
        public readonly Material Marker = Clear("LaneMarker", new Color(1f, 0.8f, 0.25f, 0.35f));
        // The projectile's landing shadow in the art direction's shadow color (#2b2233), darker than the crowd's 0.18:
        // it warns where the stone falls.
        public readonly Material Shadow = Clear("Shadow", new Color(0x2b / 255f, 0x22 / 255f, 0x33 / 255f, 0.4f));
        public readonly Material Danger = Clear("Danger", new Color(0.95f, 0.3f, 0.15f, 0.35f));
        // World 2 towers (RT-6): tarred wood, thatch, swamp lanterns.
        public readonly Material WoodDark = Toon("WoodDark", W(0f, 0.75f, 0f, 0.25f), shade: 0.85f);
        public readonly Material Thatch = Toon("Thatch", W(0.65f, 0.2f, 0.15f, 0f));
        public readonly Material Lantern = Toon("Lantern", W(0f, 0f, 0.45f, 0f), emission: 1f);
        // World 3 towers (RT-6): brick, soot, steel.
        public readonly Material Brick = Toon("Brick", W(0f, 0.55f, 0.45f, 0f));
        public readonly Material Soot = Toon("Soot", W(0f, 0f, 0f, 1f), shade: 0.7f);
        public readonly Material Steel = Toon("Steel", W(0.45f, 0f, 0f, 0.55f), shade: 1.1f);
        // World 4 towers (RT-6): frost, basalt, ice light.
        public readonly Material Frost = Toon("Frost", W(0.9f, 0f, 0f, 0f));
        public readonly Material Basalt = Toon("Basalt", W(0f, 0.4f, 0f, 0.6f));
        public readonly Material Ice = Toon("Ice", W(0.4f, 0f, 0.2f, 0f), emission: 1f);
        public readonly Material ShieldDome = Clear("ShieldDome", new Color(0.35f, 0.85f, 0.95f, 0.22f));
        public readonly Material ShieldCore = Toon("ShieldCore", W(0f, 0f, 0f, 0f), new Color(0.45f, 0.95f, 1f), emission: 1f);

        public ArtPalette()
        {
            ToonPrimitives.BuildAssets();
        }

        private static Vector4 W(float light, float dark, float detail, float defense)
        {
            return new Vector4(light, dark, detail, defense);
        }

        /// <summary>
        /// A <c>StudioKit/ToonLit</c> material: <paramref name="weights"/> of the palette's structure colors times
        /// <paramref name="color"/> (white when omitted) times <paramref name="shade"/>.
        /// </summary>
        private static Material Toon(string name, Vector4 weights, Color? color = null, float shade = 1f, float emission = 0f,
            float fog = StructureFog)
        {
            Material material = LoadOrCreate(name, ToonShaderIds.ToonLitShader);
            material.DisableKeyword(ToonShaderIds.InstancesKeyword);
            material.DisableKeyword(ToonShaderIds.SlotsKeyword);
            Color tint = (color ?? Color.white) * shade;
            tint.a = 1f;
            material.SetColor(ToonShaderIds.TintColor, tint);
            material.SetVector(ToonShaderIds.StructureWeights, weights);
            material.SetFloat(ToonShaderIds.Emission, emission);
            material.SetFloat(ToonShaderIds.FogAmount, fog);
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
                ClaybreakSceneBuilder.EnsureFolder(path.Substring(0, path.LastIndexOf('/')));
                material = new Material(shader) { name = name.Substring(name.LastIndexOf('/') + 1) };
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                // Switching shader keeps the old shader's saved values (Simple Lit's _BaseColor, keywords, disabled passes):
                // overwrite the asset with a fresh material of the new shader, same asset id, so it holds only what that
                // shader reads.
                var clean = new Material(shader);
                EditorUtility.CopySerialized(clean, material);
                UnityEngine.Object.DestroyImmediate(clean);
                material.name = name.Substring(name.LastIndexOf('/') + 1);
            }

            return material;
        }
    }
}
