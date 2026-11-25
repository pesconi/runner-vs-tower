using System;
using System.Collections.Generic;
using System.IO;
using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>A material asset and the shader it really uses.</summary>
    public struct MaterialFact
    {
        public string name;
        public string shader;
    }

    /// <summary>A mesh (or a scene object's meshes) of a class, with what the style rules measure.</summary>
    public struct MeshFact
    {
        public string name;
        public StyleClass styleClass;
        public int triangles;
        public int drawCalls;
        /// <summary>Largest angle (degrees) between two neighbouring faces over the meshes of the object.</summary>
        public float maxEdgeAngle;
    }

    /// <summary>An actual color of the game (world index, -1 for the colors shared by every world) against its guide role.</summary>
    public struct ColorFact
    {
        public int world;
        public string role;
        public Color color;
    }

    /// <summary>A clip file of the game.</summary>
    public struct ClipFact
    {
        public string name;
        /// <summary>Peak of the file in dBFS (-120 for silence).</summary>
        public float peakDb;
        /// <summary>True when docs/audio-licenses.md lists the file.</summary>
        public bool licensed;
    }

    /// <summary>
    /// What the game really has, gathered for the style rules (RT-6v3.1): the material assets and their shaders, the meshes by class
    /// (assets and scene-built objects), the palette colors in play, the clip files and the capacity of each effect's pool. Kept
    /// apart from the rules so the tests can feed the validator facts the game does not have.
    /// </summary>
    public sealed class StyleFacts
    {
        public List<MaterialFact> Materials = new List<MaterialFact>();
        public List<MeshFact> Meshes = new List<MeshFact>();
        public List<ColorFact> Colors = new List<ColorFact>();
        public List<ClipFact> Clips = new List<ClipFact>();
        public Dictionary<StyleEffect, int> EffectCapacity = new Dictionary<StyleEffect, int>();
        /// <summary>Mesh assets under the generated folder that no class owns (the shared primitives are listed apart).</summary>
        public List<string> UnclassifiedMeshes = new List<string>();

        private const string GeneratedFolder = ClaybreakSceneBuilder.Root + "/Generated";
        // Building blocks of the scene-built art: the classes that use them are measured through the scene objects.
        private static readonly HashSet<string> SharedPrimitives = new HashSet<string> { "ToonCube", "ToonCylinder", "ToonSphere", "BlobShadowDisc" };

        /// <summary>The facts of the game as built now (opens the game scene for the scene-built classes).</summary>
        public static StyleFacts Gather()
        {
            var facts = new StyleFacts();
            GatherMaterials(facts);
            GatherMeshAssets(facts);
            GatherScene(facts);
            GatherColors(facts);
            GatherClips(facts);
            return facts;
        }

        private static void GatherMaterials(StyleFacts facts)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { ClaybreakSceneBuilder.Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material != null)
                {
                    facts.Materials.Add(new MaterialFact { name = Path.GetFileNameWithoutExtension(path), shader = material.shader != null ? material.shader.name : null });
                }
            }
        }

        /// <summary>The class that owns a generated mesh asset, or null when it is a shared primitive; throws nothing: unknown meshes are reported.</summary>
        public static StyleClass? ClassOfMeshAsset(string path, out bool known)
        {
            known = true;
            string name = Path.GetFileNameWithoutExtension(path);
            if (SharedPrimitives.Contains(name))
            {
                return null;
            }

            if (path.StartsWith(GeneratedFolder + "/Units/", StringComparison.Ordinal))
            {
                return name == "LeaderCrown" ? StyleClass.Leader : StyleClass.Unit;
            }

            if (path.StartsWith(GeneratedFolder + "/Obstacles/", StringComparison.Ordinal))
            {
                return StyleClass.Obstacle;
            }

            if (path.StartsWith(GeneratedFolder + "/Scenery/", StringComparison.Ordinal))
            {
                return name == "GroundAndRails" ? StyleClass.Floor : StyleClass.Scenery;
            }

            if (name == "GatePost")
            {
                return StyleClass.Gate;
            }

            known = false;
            return null;
        }

        private static void GatherMeshAssets(StyleFacts facts)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Mesh", new[] { GeneratedFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                StyleClass? owner = ClassOfMeshAsset(path, out bool known);
                if (!known)
                {
                    facts.UnclassifiedMeshes.Add(path);
                    continue;
                }

                if (owner == null)
                {
                    continue;
                }

                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh != null)
                {
                    facts.Meshes.Add(new MeshFact
                    {
                        name = Path.GetFileNameWithoutExtension(path),
                        styleClass = owner.Value,
                        triangles = Triangles(mesh),
                        drawCalls = 1,
                        maxEdgeAngle = MaxEdgeAngle(mesh),
                    });
                }
            }
        }

        // The scene-built classes: each tower body (boss bodies are the Boss class), each defense root and each gate pair.
        private static void GatherScene(StyleFacts facts)
        {
            if (EditorSceneManager.GetActiveScene().path != ClaybreakSceneBuilder.ScenePath)
            {
                EditorSceneManager.OpenScene(ClaybreakSceneBuilder.ScenePath, OpenSceneMode.Single);
            }

            var tower = UnityEngine.Object.FindFirstObjectByType<AssaultTower>(FindObjectsInactive.Include);
            if (tower != null)
            {
                foreach (Transform child in tower.transform)
                {
                    if (child.name.EndsWith("Body", StringComparison.Ordinal))
                    {
                        bool boss = child.name.Contains("Boss");
                        facts.Meshes.Add(Measure(child, child.name, boss ? StyleClass.Boss : StyleClass.Tower));
                    }
                }
            }

            foreach (string defense in new[] { "Wall", "Shield", "Sweeper" })
            {
                GameObject root = GameObject.Find(defense);
                if (root != null)
                {
                    facts.Meshes.Add(Measure(root.transform, defense, StyleClass.Enemy));
                }
            }

            // The cannon is a pool: its stones and landing shadows are many identical objects, each measured as one (the first of each).
            GameObject cannon = GameObject.Find("CannonDefense");
            if (cannon != null)
            {
                foreach (string part in new[] { "Stone1", "Shadow1" })
                {
                    Transform piece = cannon.transform.Find(part);
                    if (piece != null)
                    {
                        facts.Meshes.Add(Measure(piece, "Cannon" + part.Replace("1", string.Empty), StyleClass.Enemy));
                    }
                }
            }

            GameObject gates = GameObject.Find("RunGates");
            if (gates != null)
            {
                foreach (Transform pair in gates.transform)
                {
                    if (pair.name.StartsWith("Pair", StringComparison.Ordinal))
                    {
                        facts.Meshes.Add(Measure(pair, pair.name, StyleClass.Gate));
                    }
                }
            }

            // The pools the effects are drawn from.
            var numbers = UnityEngine.Object.FindFirstObjectByType<FloatingNumbers>(FindObjectsInactive.Include);
            facts.EffectCapacity[StyleEffect.FloatingNumber] = numbers != null ? numbers.Capacity : 0;
            facts.EffectCapacity[StyleEffect.CounterPop] = 1;
            facts.EffectCapacity[StyleEffect.UnitDeath] = Math.Max(ClaybreakSceneBuilder.FlightDyingCapacity, ClaybreakSceneBuilder.ObstacleDyingCapacity);
            facts.EffectCapacity[StyleEffect.CameraShake] = 1;
            facts.EffectCapacity[StyleEffect.ProjectileImpact] = ClaybreakSceneBuilder.ProjectilePoolSize;
            facts.EffectCapacity[StyleEffect.TowerBreak] = AssaultTower.BlocksPerBody;
        }

        private static MeshFact Measure(Transform root, string name, StyleClass styleClass)
        {
            int triangles = 0;
            int calls = 0;
            float edge = 0f;
            var seen = new HashSet<Mesh>();
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                triangles += Triangles(mesh);
                calls++;
                if (seen.Add(mesh))
                {
                    edge = Mathf.Max(edge, MaxEdgeAngle(mesh));
                }
            }

            return new MeshFact { name = name, styleClass = styleClass, triangles = triangles, drawCalls = calls, maxEdgeAngle = edge };
        }

        private static void GatherColors(StyleFacts facts)
        {
            PaletteDefinition[] definitions = PaletteContent.Palettes;
            string[] names = ToonPalette.ColorNames;
            for (int w = 0; w < definitions.Length; w++)
            {
                ToonPalette palette = PaletteContent.Load(w);
                if (palette == null)
                {
                    continue;
                }

                Color[] colors = palette.Colors;
                for (int i = 0; i < names.Length; i++)
                {
                    facts.Colors.Add(new ColorFact { world = w, role = names[i], color = colors[i] });
                }
            }

            AddMaterialColor(facts, StyleGuideContent.GateGoodRole, "GateGood");
            AddMaterialColor(facts, StyleGuideContent.GateBadRole, "GateBad");
        }

        private static void AddMaterialColor(StyleFacts facts, string role, string materialName)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>($"{ArtPalette.MaterialFolder}/{materialName}.mat");
            if (material != null)
            {
                Color color = material.GetColor("_BaseColor");
                color.a = 1f;
                facts.Colors.Add(new ColorFact { world = -1, role = role, color = color });
            }
        }

        private static void GatherClips(StyleFacts facts)
        {
            string licenses = File.Exists(ClaybreakAudio.LicensesPath) ? File.ReadAllText(ClaybreakAudio.LicensesPath) : string.Empty;
            if (!Directory.Exists(ClaybreakAudio.Folder))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(ClaybreakAudio.Folder, "*.wav"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                facts.Clips.Add(new ClipFact { name = name, peakDb = WavPeakDb(file), licensed = licenses.Contains($"`{name}.wav`") });
            }
        }

        // ---------- measures ----------

        public static int Triangles(Mesh mesh)
        {
            long indices = 0;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                if (mesh.GetTopology(i) == MeshTopology.Triangles)
                {
                    indices += mesh.GetIndexCount(i);
                }
            }

            return (int)(indices / 3);
        }

        /// <summary>
        /// The largest angle, in degrees, between the normals of two faces that share an edge (vertices welded by position, so the
        /// split vertices of flat shading do not hide the edge). 0 for a mesh with no shared edge; a cube is 90, a sphere a few degrees.
        /// </summary>
        public static float MaxEdgeAngle(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            var weld = new Dictionary<Vector3Int, int>();
            var ids = new int[vertices.Length];
            for (int v = 0; v < vertices.Length; v++)
            {
                var key = new Vector3Int(Mathf.RoundToInt(vertices[v].x * 1000f), Mathf.RoundToInt(vertices[v].y * 1000f), Mathf.RoundToInt(vertices[v].z * 1000f));
                if (!weld.TryGetValue(key, out int id))
                {
                    id = weld.Count;
                    weld.Add(key, id);
                }

                ids[v] = id;
            }

            var faces = new Dictionary<long, Vector3>();
            float widest = 0f;
            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                if (mesh.GetTopology(s) != MeshTopology.Triangles)
                {
                    continue;
                }

                int[] triangles = mesh.GetTriangles(s);
                for (int t = 0; t + 2 < triangles.Length; t += 3)
                {
                    Vector3 a = vertices[triangles[t]];
                    Vector3 b = vertices[triangles[t + 1]];
                    Vector3 c = vertices[triangles[t + 2]];
                    Vector3 normal = Vector3.Cross(b - a, c - a);
                    if (normal.sqrMagnitude < 1e-12f)
                    {
                        continue;
                    }

                    normal.Normalize();
                    for (int e = 0; e < 3; e++)
                    {
                        int from = ids[triangles[t + e]];
                        int to = ids[triangles[t + (e + 1) % 3]];
                        if (from == to)
                        {
                            continue;
                        }

                        long key = from < to ? ((long)from << 32) | (uint)to : ((long)to << 32) | (uint)from;
                        if (faces.TryGetValue(key, out Vector3 other))
                        {
                            widest = Mathf.Max(widest, Vector3.Angle(other, normal));
                            faces[key] = normal;
                        }
                        else
                        {
                            faces.Add(key, normal);
                        }
                    }
                }
            }

            return widest;
        }

        /// <summary>The peak of a 16-bit PCM WAV file in dBFS (read from the file, so no import setting changes it); -120 for silence or a format it does not read.</summary>
        public static float WavPeakDb(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            int position = 12;
            int bits = 0;
            while (position + 8 <= bytes.Length)
            {
                string id = System.Text.Encoding.ASCII.GetString(bytes, position, 4);
                int size = BitConverter.ToInt32(bytes, position + 4);
                int body = position + 8;
                if (id == "fmt ")
                {
                    bits = BitConverter.ToInt16(bytes, body + 14);
                }
                else if (id == "data")
                {
                    if (bits != 16)
                    {
                        return -120f;
                    }

                    int end = Math.Min(bytes.Length, body + size);
                    int peak = 0;
                    for (int i = body; i + 1 < end; i += 2)
                    {
                        peak = Math.Max(peak, Math.Abs((int)BitConverter.ToInt16(bytes, i)));
                    }

                    return peak == 0 ? -120f : 20f * Mathf.Log10(peak / 32768f);
                }

                position = body + size + (size & 1);
            }

            return -120f;
        }
    }
}
