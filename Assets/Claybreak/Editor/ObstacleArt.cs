using System;
using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Claybreak.Editor
{
    /// <summary>
    /// The run obstacles' art (RT-6t.18): one low-poly toon mesh per <see cref="ObstacleKind"/>, modeled around the anchor
    /// <see cref="ObstacleMotion"/> moves (the log hangs from its pivot, the blade and the drum turn about their axle, the
    /// hammer stands on its sole, the pit lies flat at the hole's size) and to its sizes, plus their one instanced material.
    /// Body parts are white with vertex alpha 1 and take the world palette's defense color as the instance tint; spikes,
    /// blades, iron, wood and the pit keep their own colors (alpha 0). Assets in Generated/Obstacles, rewritten in place so
    /// references keep their ids.
    /// </summary>
    public static class ObstacleArt
    {
        public const string Folder = "Assets/Claybreak/Content/Generated/Obstacles";
        public const string MaterialPath = Folder + "/ObstacleToon.mat";

        /// <summary>A little emission so the obstacles read on every world's light.</summary>
        public const float Emission = 0.08f;

        private static readonly Color32 Body = new Color32(255, 255, 255, 255);
        private static readonly Color32 Bone = new Color32(240, 232, 212, 0);
        private static readonly Color32 Steel = new Color32(206, 212, 222, 0);
        private static readonly Color32 SteelEdge = new Color32(240, 244, 250, 0);
        private static readonly Color32 Iron = new Color32(70, 72, 84, 0);
        private static readonly Color32 Wood = new Color32(122, 82, 50, 0);
        private static readonly Color32 WoodEnd = new Color32(214, 170, 112, 0);
        private static readonly Color32 Danger = new Color32(232, 64, 52, 0);
        private static readonly Color32 PitRim = new Color32(78, 64, 58, 0);
        private static readonly Color32 PitWall = new Color32(34, 26, 26, 0);
        private static readonly Color32 PitFloor = new Color32(12, 9, 10, 0);

        /// <summary>The mesh path of <paramref name="kind"/>.</summary>
        public static string MeshPath(ObstacleKind kind) => $"{Folder}/Obstacle{kind}.asset";

        /// <summary>Writes the five meshes and the material.</summary>
        public static void BuildAssets()
        {
            ClaybreakSceneBuilder.EnsureFolder(Folder);
            foreach (ObstacleKind kind in Enum.GetValues(typeof(ObstacleKind)))
            {
                SaveMesh(Build(kind), MeshPath(kind));
            }

            Shader toon = Shader.Find(ToonShaderIds.ToonLitShader) ?? throw new InvalidOperationException("missing " + ToonShaderIds.ToonLitShader);
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(toon);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            material.shader = StyleGuideContent.KeepVinyl(material, toon);
            material.EnableKeyword(ToonShaderIds.InstancesKeyword);
            material.DisableKeyword(ToonShaderIds.SlotsKeyword);
            material.SetColor(ToonShaderIds.TintColor, Color.white);
            material.SetFloat(ToonShaderIds.Emission, Emission);
            material.enableInstancing = true; // the fallback route draws it with RenderMeshInstanced (RT-6t.11)
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
        }

        /// <summary>The meshes in <see cref="ObstacleKind"/> order (what the run course draws).</summary>
        public static Mesh[] LoadMeshes()
        {
            var meshes = new Mesh[ObstacleMotion.KindCount];
            for (int k = 0; k < meshes.Length; k++)
            {
                meshes[k] = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath((ObstacleKind)k));
            }

            return meshes;
        }

        public static Mesh Build(ObstacleKind kind)
        {
            var builder = new LowPolyMeshBuilder();
            switch (kind)
            {
                case ObstacleKind.SwingingLog:
                    SwingingLog(builder);
                    break;
                case ObstacleKind.Saw:
                    Saw(builder);
                    break;
                case ObstacleKind.Hammer:
                    Hammer(builder);
                    break;
                case ObstacleKind.Hole:
                    Pit(builder);
                    break;
                default:
                    Roller(builder);
                    break;
            }

            Mesh mesh = builder.ToMesh("Obstacle" + kind);
            mesh.RecalculateBounds();
            return mesh;
        }

        // Pivot at the origin; the rod runs up out of view and down to the log, which lies along the run at -arm.
        private static void SwingingLog(LowPolyMeshBuilder b)
        {
            const float above = 6f;
            float arm = ObstacleMotion.SwingArm;
            float r = ObstacleMotion.LogRadius;
            float length = ObstacleMotion.LogLength;
            float rodBottom = -arm + r * 0.6f;
            b.AddCylinder(LowPolyMeshBuilder.Place(new Vector3(0f, 0.5f * (above + rodBottom), 0f), new Vector3(0.08f, above - rodBottom, 0.08f)), 6, Iron);
            b.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, -arm + r * 0.9f, 0f), new Vector3(0.2f, 0.16f, 0.2f)), 8, 4, Iron);
            var center = new Vector3(0f, -arm, 0f);
            b.AddCylinder(LowPolyMeshBuilder.Place(center, new Vector3(90f, 0f, 0f), new Vector3(r, length, r)), 12, Body);
            for (int end = -1; end <= 1; end += 2)
            {
                b.AddCylinder(LowPolyMeshBuilder.Place(center + new Vector3(0f, 0f, end * length * 0.5f), new Vector3(90f, 0f, 0f),
                    new Vector3(r * 0.82f, 0.04f, r * 0.82f)), 12, WoodEnd);
            }

            for (int ring = -1; ring <= 1; ring++)
            {
                for (int i = 0; i < 6; i++)
                {
                    float a = (i + (ring == 0 ? 0.5f : 0f)) * Mathf.PI * 2f / 6f;
                    var direction = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
                    Spike(b, center + new Vector3(0f, 0f, ring * length * 0.33f) + direction * (r - 0.06f), direction, 0.13f, 0.45f, Bone);
                }
            }
        }

        // Hub at the origin; the blade stands across the track (in the x-y plane) and turns about z.
        private static void Saw(LowPolyMeshBuilder b)
        {
            float r = ObstacleMotion.SawRadius;
            b.AddCylinder(LowPolyMeshBuilder.Place(Vector3.zero, new Vector3(90f, 0f, 0f), new Vector3(r - 0.08f, 0.1f, r - 0.08f)), 20, Steel);
            b.AddCylinder(LowPolyMeshBuilder.Place(Vector3.zero, new Vector3(90f, 0f, 0f), new Vector3(r * 0.62f, 0.12f, r * 0.62f)), 16, Danger);
            const int teeth = 14;
            for (int i = 0; i < teeth; i++)
            {
                float a = i * Mathf.PI * 2f / teeth;
                var direction = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
                // Teeth lean forward along the spin, as a saw's do.
                var lean = new Vector3(-direction.y, direction.x, 0f);
                Spike(b, direction * (r - 0.14f), (direction + lean * 0.45f).normalized, 0.1f, 0.26f, SteelEdge);
            }

            b.AddCylinder(LowPolyMeshBuilder.Place(Vector3.zero, new Vector3(90f, 0f, 0f), new Vector3(0.3f, 0.3f, 0.3f)), 10, Body);
            b.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, 0f, -0.16f), Vector3.one * 0.12f), 8, 4, Iron);
            b.AddSphere(LowPolyMeshBuilder.Place(new Vector3(0f, 0f, 0.16f), Vector3.one * 0.12f), 8, 4, Iron);
        }

        // Sole at the origin: the head over half the track, iron bands and sole, the handle up out of view.
        private static void Hammer(LowPolyMeshBuilder b)
        {
            float width = ObstacleMotion.HammerHalfWidth * 2f;
            float depth = ObstacleMotion.HammerHalfDepth * 2f;
            float height = ObstacleMotion.HammerHeadHeight;
            b.AddBox(LowPolyMeshBuilder.Place(new Vector3(0f, height * 0.5f + 0.06f, 0f), new Vector3(width, height - 0.12f, depth)), Body);
            b.AddBox(LowPolyMeshBuilder.Place(new Vector3(0f, 0.06f, 0f), new Vector3(width + 0.06f, 0.12f, depth + 0.06f)), Iron);
            for (int side = -1; side <= 1; side += 2)
            {
                b.AddBox(LowPolyMeshBuilder.Place(new Vector3(side * width * 0.36f, height * 0.5f + 0.06f, 0f),
                    new Vector3(0.24f, height - 0.06f, depth + 0.08f)), Iron);
                b.AddBox(LowPolyMeshBuilder.Place(new Vector3(side * (width * 0.5f + 0.02f), height * 0.5f + 0.06f, 0f),
                    new Vector3(0.06f, height * 0.6f, depth * 0.6f)), Danger);
            }

            const float handle = 14f;
            b.AddCylinder(LowPolyMeshBuilder.Place(new Vector3(0f, height + handle * 0.5f, 0f), new Vector3(0.22f, handle, 0.22f)), 8, Wood);
            b.AddCylinder(LowPolyMeshBuilder.Place(new Vector3(0f, height + 0.05f, 0f), new Vector3(0.36f, 0.14f, 0.36f)), 8, Iron);
        }

        // The pit at the hole's size, flat on the track: a rim, a dark wall band and a black floor, a hair above the track.
        private static void Pit(LowPolyMeshBuilder b)
        {
            float w = ObstacleMotion.HoleHalfWidth * 2f;
            float d = ObstacleMotion.HoleHalfDepth * 2f;
            const float rim = 0.16f;
            b.AddBox(LowPolyMeshBuilder.Place(new Vector3(0f, 0.006f, 0f), new Vector3(w - rim, 0.01f, d - rim)), PitWall);
            b.AddBox(LowPolyMeshBuilder.Place(new Vector3(0f, 0.008f, 0f), new Vector3(w - rim * 2.4f, 0.01f, d - rim * 2.4f)), PitFloor);
            for (int side = -1; side <= 1; side += 2)
            {
                b.AddBox(LowPolyMeshBuilder.Place(new Vector3(0f, 0.012f, side * (d - rim) * 0.5f), new Vector3(w, 0.02f, rim)), PitRim);
                b.AddBox(LowPolyMeshBuilder.Place(new Vector3(side * (w - rim) * 0.5f, 0.012f, 0f), new Vector3(rim, 0.02f, d)), PitRim);
            }
        }

        // Axle at the origin, along x: the drum, iron end plates and four rings of spikes.
        private static void Roller(LowPolyMeshBuilder b)
        {
            float r = ObstacleMotion.RollerRadius;
            float length = ObstacleMotion.RollerHalfLength * 2f;
            b.AddCylinder(LowPolyMeshBuilder.Place(Vector3.zero, new Vector3(0f, 0f, 90f), new Vector3(r, length, r)), 14, Body);
            for (int end = -1; end <= 1; end += 2)
            {
                b.AddCylinder(LowPolyMeshBuilder.Place(new Vector3(end * length * 0.5f, 0f, 0f), new Vector3(0f, 0f, 90f),
                    new Vector3(r * 1.1f, 0.1f, r * 1.1f)), 14, Iron);
            }

            float[] rings = { -0.8f, -0.27f, 0.27f, 0.8f };
            for (int ring = 0; ring < rings.Length; ring++)
            {
                for (int i = 0; i < 8; i++)
                {
                    float a = (i + (ring % 2) * 0.5f) * Mathf.PI * 2f / 8f;
                    var direction = new Vector3(0f, Mathf.Cos(a), Mathf.Sin(a));
                    Spike(b, new Vector3(rings[ring] * ObstacleMotion.RollerHalfLength, 0f, 0f) + direction * (r - 0.06f), direction, 0.12f, 0.36f, Bone);
                }
            }
        }

        // A cone from base to tip along direction.
        private static void Spike(LowPolyMeshBuilder b, Vector3 basePoint, Vector3 direction, float radius, float length, Color32 color)
        {
            Matrix4x4 placement = Matrix4x4.TRS(basePoint, Quaternion.FromToRotation(Vector3.up, direction), new Vector3(radius, length, radius));
            b.AddCone(placement, 6, color);
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
