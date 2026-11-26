using System;
using JungleDegree.StudioKit.Toon;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Claybreak.Editor
{
    /// <summary>
    /// The look of the math gates (RT-6t.9, after docs/reference/crowd-proto.html): translucent panels half the track wide
    /// (<see cref="ArtPalette.GateGood"/> / <see cref="ArtPalette.GateBad"/>, #5ad1a0 / #ff6b81 at 0.45, no depth write),
    /// white posts in <c>StudioKit/ToonLit</c> and big white TextMeshPro labels in Baloo 2 with the side's outline preset.
    /// The posts are a vertex-colored cylinder (vertex alpha 0 = keeps its own white, no tint).
    /// </summary>
    public static class GateArt
    {
        public const string PostMeshPath = ClaybreakSceneBuilder.Root + "/Generated/GatePost.asset";
        public const string PostMaterialPath = ArtPalette.MaterialFolder + "/GatePostToon.mat";

        /// <summary>Post radius (the prototype's) and the label em height (world units; a bit above the prototype's 0.85 so
        /// the numbers read on a 6" portrait screen, checked in the capture).</summary>
        public const float PostRadius = 0.14f;
        public const float LabelEm = 1.05f;

        public static void BuildAssets()
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Mesh source = cylinder.GetComponent<MeshFilter>().sharedMesh;
            var mesh = new Mesh { name = "GatePost" };
            mesh.vertices = source.vertices;
            mesh.normals = source.normals;
            mesh.triangles = source.triangles;
            var colors = new Color[source.vertexCount];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(1f, 1f, 1f, 0f);
            }

            mesh.colors = colors;
            mesh.RecalculateBounds();
            Object.DestroyImmediate(cylinder);

            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(PostMeshPath);
            if (existing == null)
            {
                ClaybreakSceneBuilder.EnsureFolder(PostMeshPath.Substring(0, PostMeshPath.LastIndexOf('/')));
                AssetDatabase.CreateAsset(mesh, PostMeshPath);
            }
            else
            {
                existing.Clear();
                existing.vertices = mesh.vertices;
                existing.normals = mesh.normals;
                existing.triangles = mesh.triangles;
                existing.colors = mesh.colors;
                existing.RecalculateBounds();
                Object.DestroyImmediate(mesh);
                EditorUtility.SetDirty(existing);
            }

            Shader toon = Shader.Find(ToonShaderIds.ToonLitShader) ?? throw new InvalidOperationException("missing " + ToonShaderIds.ToonLitShader);
            var material = AssetDatabase.LoadAssetAtPath<Material>(PostMaterialPath);
            if (material == null)
            {
                material = new Material(toon);
                AssetDatabase.CreateAsset(material, PostMaterialPath);
            }

            material.shader = StyleGuideContent.KeepVinyl(material, toon);
            material.DisableKeyword(ToonShaderIds.InstancesKeyword);
            material.DisableKeyword(ToonShaderIds.SlotsKeyword);
            material.SetColor(ToonShaderIds.TintColor, Color.white);
            EditorUtility.SetDirty(material);
            GameFonts.BuildAssets();
            AssetDatabase.SaveAssets();
        }

        /// <summary>A white post standing on the track at <paramref name="x"/> (local), <paramref name="height"/> tall.</summary>
        public static MeshRenderer Post(Transform parent, string name, float x, float height)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(x, height * 0.5f, 0f);
            go.transform.localScale = new Vector3(PostRadius * 2f, height * 0.5f, PostRadius * 2f);
            go.GetComponent<MeshFilter>().sharedMesh = Load<Mesh>(PostMeshPath);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = Load<Material>(PostMaterialPath);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return renderer;
        }

        /// <summary>A world label (TextMeshPro, Baloo 2 SDF) centered at <paramref name="position"/>, facing the camera (-z).</summary>
        public static TextMeshPro Label(Transform parent, string name, Vector3 position, float em, float width, string preset)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(MeshRenderer), typeof(TextMeshPro));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var text = go.GetComponent<TextMeshPro>();
            text.font = GameFonts.Sdf();
            text.fontSharedMaterial = GameFonts.Preset(preset);
            // TextMeshPro world text: point size 10 is one unit of em.
            text.fontSize = em * 10f;
            text.enableAutoSizing = false;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.color = Color.white;
            text.raycastTarget = false;
            text.rectTransform.sizeDelta = new Vector2(width, em * 1.6f);
            text.text = "+0";
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return text;
        }

        private static T Load<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("missing " + path);
        }
    }
}
