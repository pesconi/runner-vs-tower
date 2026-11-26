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
        /// <summary>RT-6v3.3: the vinyl surface the material holds (gloss, highlight size, rim, top gloss), when it has one.</summary>
        public bool vinyl;
        public Vector4 surface;
        public float darken;
        public bool translucent;
    }

    /// <summary>RT-6v3.3: the light and post assets a world really shows (what the scene's <see cref="WorldScenery"/> points at).</summary>
    public struct WorldLookFact
    {
        public int world;
        public ToonVinylLook light;
        public UnityEngine.Rendering.VolumeProfile post;
        public float fogStart;
        public float fogEnd;
        public Color fog;
    }

    /// <summary>
    /// Which budget a mesh answers to (RT-6v3.4a): its class's state (legacy ceiling or v3 range), or, for a mesh already
    /// remade in v3 inside a class that is still legacy (the Bibopzi levels before the other shapes, 6v3.4c), the v3 range or
    /// the class's reserve range.
    /// </summary>
    public enum MeshTier
    {
        FollowsClass,
        V3,
        Reserve,
    }

    /// <summary>A mesh (or a scene object's meshes) of a class, with what the style rules measure.</summary>
    public struct MeshFact
    {
        public string name;
        public StyleClass styleClass;
        public MeshTier tier;
        public int triangles;
        public int drawCalls;
        /// <summary>Largest angle (degrees) between two neighbouring faces across a hard edge over the meshes of the object.</summary>
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
        /// <summary>Approximate volume: RMS in dBFS of the samples above -40 dBFS (same definition as Tools/audio/audio_metrics.py).</summary>
        public float loudnessDb;
        public float seconds;
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
        /// <summary>RT-6v3.3: per world, the light and post of the scene and the fog of the palette (empty before the first build).</summary>
        public List<WorldLookFact> Looks = new List<WorldLookFact>();
        /// <summary>RT-6v3.3: true when the scene's game canvas is a screen-space overlay (the UI never goes through the post).</summary>
        public bool UiOutsidePost = true;
        /// <summary>RT-6v3.3: true when the game camera renders the post stack.</summary>
        public bool CameraPost = true;
        /// <summary>RT-6v3.3: the contrast the last style capture measured (null when there is none).</summary>
        public ContrastMeasures Measures;

        /// <summary>The measures of <see cref="StyleContrast.MeasuresPath"/>, or null.</summary>
        public static ContrastMeasures LoadMeasures()
        {
            string path = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? ".", StyleContrast.MeasuresPath);
            return File.Exists(path) ? JsonUtility.FromJson<ContrastMeasures>(File.ReadAllText(path)) : null;
        }
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
            facts.Measures = LoadMeasures();
            GatherMaterials(facts);
            GatherMeshAssets(facts);
            GatherScene(facts);
            GatherColors(facts);
            GatherClips(facts);
            return facts;
        }

        private static readonly int VinylGlossId = Shader.PropertyToID("_VinylGloss");
        private static readonly int VinylSpecSizeId = Shader.PropertyToID("_VinylSpecSize");
        private static readonly int VinylRimId = Shader.PropertyToID("_VinylRim");
        private static readonly int VinylTopGlossId = Shader.PropertyToID("_VinylTopGloss");
        private static readonly int VinylDarkenId = Shader.PropertyToID("_VinylDarken");

        private static void GatherMaterials(StyleFacts facts)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { ClaybreakSceneBuilder.Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material != null)
                {
                    var fact = new MaterialFact { name = Path.GetFileNameWithoutExtension(path), shader = material.shader != null ? material.shader.name : null };
                    if (material.HasProperty(VinylGlossId))
                    {
                        fact.vinyl = true;
                        fact.surface = new Vector4(material.GetFloat(VinylGlossId), material.GetFloat(VinylSpecSizeId),
                            material.GetFloat(VinylRimId), material.GetFloat(VinylTopGlossId));
                        fact.darken = material.GetFloat(VinylDarkenId);
                        fact.translucent = material.renderQueue >= (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    }

                    facts.Materials.Add(fact);
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
                // RT-6v3.4a: the detailed Bibopzi is what the leader wears.
                return name == "LeaderCrown" || UnitShapeAssets.AtPath(path)?.Level == UnitShapeAssets.ShapeLevel.Detail
                    ? StyleClass.Leader
                    : StyleClass.Unit;
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
                        tier = TierOf(path),
                        triangles = Triangles(mesh),
                        drawCalls = 1,
                        maxEdgeAngle = MaxEdgeAngle(mesh),
                    });
                }
            }
        }

        // RT-6v3.4a: the shapes written by Blender answer to the v3 budgets before their class migrates.
        private static MeshTier TierOf(string path)
        {
            switch (UnitShapeAssets.AtPath(path)?.Level)
            {
                case UnitShapeAssets.ShapeLevel.Crowd:
                case UnitShapeAssets.ShapeLevel.Detail:
                    return MeshTier.V3;
                case UnitShapeAssets.ShapeLevel.Reserve:
                    return MeshTier.Reserve;
                default:
                    return MeshTier.FollowsClass;
            }
        }

        // The scene-built classes: each tower body (boss bodies are the Boss class), each defense root and each gate pair.
        // RT-6v3.3: what each world shows (light, post, fog) and whether the UI and the camera are set for the post.
        private static void GatherLooks(StyleFacts facts)
        {
            var scenery = UnityEngine.Object.FindFirstObjectByType<WorldScenery>(FindObjectsInactive.Include);
            PaletteDefinition[] definitions = PaletteContent.Palettes;
            for (int w = 0; w < definitions.Length; w++)
            {
                ToonPalette palette = PaletteContent.Load(w);
                facts.Looks.Add(new WorldLookFact
                {
                    world = w,
                    light = scenery != null ? scenery.LightOf(w) : null,
                    post = scenery != null ? scenery.PostOf(w) : null,
                    fogStart = palette != null ? palette.FogStart : 0f,
                    fogEnd = palette != null ? palette.FogEnd : 0f,
                    fog = palette != null ? palette.Fog : Color.clear,
                });
            }

            GameObject ui = GameObject.Find("/Ui");
            Canvas canvas = ui != null ? ui.GetComponent<Canvas>() : null;
            facts.UiOutsidePost = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay;
            Camera main = null;
            foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (camera.CompareTag("MainCamera"))
                {
                    main = camera;
                }
            }

            var data = main != null ? main.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() : null;
            facts.CameraPost = data != null && data.renderPostProcessing;
        }

        private static void GatherScene(StyleFacts facts)
        {
            if (EditorSceneManager.GetActiveScene().path != ClaybreakSceneBuilder.ScenePath)
            {
                EditorSceneManager.OpenScene(ClaybreakSceneBuilder.ScenePath, OpenSceneMode.Single);
            }

            GatherLooks(facts);
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
                WavMetrics(file, out float peak, out float loudness, out float seconds);
                facts.Clips.Add(new ClipFact
                {
                    name = name, peakDb = peak, loudnessDb = loudness, seconds = seconds, licensed = licenses.Contains($"`{name}.wav`")
                });
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
        /// The largest angle, in degrees, between the normals of two faces that share a HARD edge: vertices welded by position (so
        /// the split vertices of flat shading do not hide the edge), and the edge counts when the vertex normals on its two sides
        /// differ (RT-6v3.4a: an edge drawn with one shared normal is shaded round, it does not read as an unbevelled corner; a
        /// flat-shaded edge does). 0 for a mesh with no hard edge; a cube is 90, a smooth sphere 0.
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

            Vector3[] vertexNormals = mesh.normals;
            bool hasNormals = vertexNormals.Length == vertices.Length;
            var faces = new Dictionary<long, (Vector3 normal, int from, int to)>();
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
                        int rawFrom = triangles[t + e];
                        int rawTo = triangles[t + (e + 1) % 3];
                        int from = ids[rawFrom];
                        int to = ids[rawTo];
                        if (from == to)
                        {
                            continue;
                        }

                        // The raw vertices in welded order (low id first), so both sides of the edge compare end to end.
                        (int low, int high) raw = from < to ? (rawFrom, rawTo) : (rawTo, rawFrom);
                        long key = from < to ? ((long)from << 32) | (uint)to : ((long)to << 32) | (uint)from;
                        if (faces.TryGetValue(key, out var other))
                        {
                            bool hard = !hasNormals || Vector3.Angle(vertexNormals[other.from], vertexNormals[raw.low]) > 1f
                                || Vector3.Angle(vertexNormals[other.to], vertexNormals[raw.high]) > 1f;
                            if (hard)
                            {
                                widest = Mathf.Max(widest, Vector3.Angle(other.normal, normal));
                            }

                            faces[key] = (normal, raw.low, raw.high);
                        }
                        else
                        {
                            faces.Add(key, (normal, raw.low, raw.high));
                        }
                    }
                }
            }

            return widest;
        }

        /// <summary>The peak of a 16-bit PCM WAV file in dBFS (read from the file, so no import setting changes it); -120 for silence or a format it does not read.</summary>
        public static float WavPeakDb(string path)
        {
            WavMetrics(path, out float peak, out _, out _);
            return peak;
        }

        /// <summary>Peak (dBFS), approximate volume (RMS dBFS over samples above -40 dBFS) and duration of a 16-bit mono WAV.</summary>
        public static void WavMetrics(string path, out float peakDb, out float loudnessDb, out float seconds)
        {
            peakDb = -120f;
            loudnessDb = -120f;
            seconds = 0f;
            byte[] bytes = File.ReadAllBytes(path);
            int position = 12;
            int bits = 0;
            int rate = 44100;
            while (position + 8 <= bytes.Length)
            {
                string id = System.Text.Encoding.ASCII.GetString(bytes, position, 4);
                int size = BitConverter.ToInt32(bytes, position + 4);
                int body = position + 8;
                if (id == "fmt ")
                {
                    bits = BitConverter.ToInt16(bytes, body + 14);
                    rate = BitConverter.ToInt32(bytes, body + 4);
                }
                else if (id == "data")
                {
                    if (bits != 16)
                    {
                        return;
                    }

                    int end = Math.Min(bytes.Length, body + size);
                    int peak = 0;
                    double sum = 0;
                    long active = 0;
                    const int Gate = 328; // 10^(-40/20) of full scale (32768)
                    for (int i = body; i + 1 < end; i += 2)
                    {
                        int v = Math.Abs((int)BitConverter.ToInt16(bytes, i));
                        peak = Math.Max(peak, v);
                        if (v > Gate)
                        {
                            sum += (double)v * v;
                            active++;
                        }
                    }

                    seconds = (end - body) / 2f / rate;
                    peakDb = peak == 0 ? -120f : 20f * Mathf.Log10(peak / 32768f);
                    loudnessDb = active == 0 ? -120f : 10f * Mathf.Log10((float)(sum / active) / (32768f * 32768f));
                    return;
                }

                position = body + size + (size & 1);
            }
        }
    }
}
