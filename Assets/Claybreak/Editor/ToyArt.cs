using System;
using JungleDegree.StudioKit.Editor;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// RT-6v (direction v2, D24 = A "Brinquedo Brilhante"): the colors of the glossy toy look. sRGB values as an
    /// artist picks them; <see cref="ToyArt.Paint"/> converts them for vertex colors. World 1 (sunny valley) first;
    /// worlds 2–4 get theirs in RT-6v.2. No candy pink, no faces (D17, D24).
    /// </summary>
    public static class ToyPalette
    {
        // Army (shop default skin and the theme color): cobalt blue and gold.
        public static readonly Color Cobalt = Hex("2F6BFF");
        public static readonly Color Gold = Hex("FFC83A");
        public static readonly Color Navy = Hex("1C2C70");
        public static readonly Color Cream = Hex("FFF3DA");

        // World 1: the sunny valley.
        public static readonly Color Zenith = Hex("63BEFF");
        public static readonly Color Horizon = Hex("E6F6FF");
        public static readonly Color AmbientSky = Hex("A7C4EC");
        public static readonly Color AmbientGround = Hex("8E6E5E");
        public static readonly Color Sun = Hex("FFF2DE");
        public static readonly Color TrackTop = Hex("FFD996");
        public static readonly Color TrackSide = Hex("F2BE78");
        public static readonly Color Curb = Hex("F0784E");
        public static readonly Color Grass = Hex("79D896");
        public static readonly Color GrassFar = Hex("8FE0A0");
        public static readonly Color Trunk = Hex("C98A5E");
        public static readonly Color Pebble = Hex("F3A57C");
        public static readonly Color FlowerCenter = Hex("FFB83D");
        public static readonly Color[] Hills = { Hex("57CF9C"), Hex("97E187"), Hex("3DB7A4") };
        public static readonly Color[] Leaves = { Hex("4FC98A"), Hex("2FB5A8"), Hex("FFA43A") };
        public static readonly Color[] Petals = { Hex("FFFFFF"), Hex("FFE066"), Hex("FFFFFF") };
        public static readonly Color Cloud = Hex("FFFFFF");
        public static readonly Color ShadowTint = Hex("5A3A50");

        // Enemy forts of world 1: coral and cream with navy roofs and a dark cannon.
        public static readonly Color FortMain = Hex("F2664E");
        public static readonly Color FortTrim = Hex("FFEBD2");
        public static readonly Color FortRoof = Hex("33366E");
        public static readonly Color FortWindow = Hex("232048");
        public static readonly Color FortDoor = Hex("6E3A2C");
        public static readonly Color Cannon = Hex("2A2C45");
        public static readonly Color BossGold = Hex("FFB21E");

        // Gates (every world): white frame, gold knobs, blue (good) and coral-red (bad) glass, navy number shadow.
        public static readonly Color GateFrame = Hex("FFFFFF");
        public static readonly Color GlassGood = new Color(0.1f, 0.65f, 1f, 0.62f);
        public static readonly Color GlassBad = new Color(1f, 0.24f, 0.36f, 0.62f);
        public static readonly Color LabelShadow = new Color(0.08f, 0.1f, 0.3f, 1f);

        public static Color Hex(string rgb)
        {
            if (!ColorUtility.TryParseHtmlString("#" + rgb, out Color color))
            {
                throw new ArgumentException("not a color: " + rgb, nameof(rgb));
            }

            return color;
        }
    }

    /// <summary>
    /// RT-6v: the materials and mesh assets of the toy look. Materials live in Materials/Toy/, meshes built with the
    /// kit's <see cref="SoftMeshBuilder"/> in Generated/Toy/ (rebuilt in place, so their GUIDs stay).
    /// </summary>
    public static class ToyArt
    {
        public const string ToyShader = "Claybreak/Toy";
        public const string GlassShader = "Claybreak/ToyGlass";
        public const string SkyShader = "StudioKit/SkyGradient";
        public const string ContactShadowShader = "StudioKit/SoftContactShadow";
        public const string MeshFolder = ClaybreakSceneBuilder.Root + "/Generated/Toy";

        /// <summary>A vertex color for the toy shader: linear rgb (the project renders in linear space) and gloss in alpha.</summary>
        public static Color Paint(Color srgb, float gloss = 1f)
        {
            Color linear = srgb.linear;
            return new Color(linear.r, linear.g, linear.b, Mathf.Clamp01(gloss));
        }

        /// <summary>A toy material painted by the vertex colors of its mesh.</summary>
        public static Material Painted(string name, float gloss = 1f, float rim = 0.6f)
        {
            return Toy(name, Color.white, true, gloss, rim);
        }

        /// <summary>A toy material of one color (meshes without vertex colors: the track).</summary>
        public static Material Solid(string name, Color color, float gloss = 0.4f, float rim = 0.3f)
        {
            return Toy(name, color, false, gloss, rim);
        }

        public static Material Glass(string name, Color color)
        {
            Material material = ArtPalette.LoadOrCreate(name, GlassShader);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Gloss", 1f);
            material.renderQueue = -1;
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Material Sky(string name, Color horizon, Color zenith)
        {
            Material material = ArtPalette.LoadOrCreate("Toy/" + name, SkyShader);
            material.SetColor("_Horizon", horizon);
            material.SetColor("_Zenith", zenith);
            material.SetColor("_Ground", horizon);
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Material ContactShadow(string name, Color tint)
        {
            Material material = ArtPalette.LoadOrCreate("Toy/" + name, ContactShadowShader);
            material.SetColor("_ShadowColor", tint);
            material.SetFloat("_Softness", 0.8f);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Saves <paramref name="mesh"/> as Generated/Toy/&lt;name&gt;.asset, over the old one (same GUID), and returns the asset.</summary>
        public static Mesh SaveMesh(Mesh mesh, string name)
        {
            return SaveMeshAt(mesh, MeshFolder + "/" + name + ".asset");
        }

        public static Mesh SaveMeshAt(Mesh mesh, string path)
        {
            ClaybreakSceneBuilder.EnsureFolder(path.Substring(0, path.LastIndexOf('/')));
            mesh.name = System.IO.Path.GetFileNameWithoutExtension(path);
            mesh.RecalculateBounds();
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }

            EditorUtility.CopySerialized(mesh, existing);
            UnityEngine.Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        /// <summary>A child with a mesh and a material (no collider, no shadows: rule 2.2.7, GDD 9).</summary>
        public static MeshRenderer Place(Transform parent, string name, Mesh mesh, Material material, Vector3 position = default)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return renderer;
        }

        private static Material Toy(string name, Color color, bool vertexColor, float gloss, float rim)
        {
            Material material = ArtPalette.LoadOrCreate("Toy/" + name, ToyShader);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_VertexColor", vertexColor ? 1f : 0f);
            material.SetFloat("_Gloss", gloss);
            material.SetFloat("_Rim", rim);
            material.SetFloat("_Wrap", 0.55f);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
