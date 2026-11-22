using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The toon scenery of every world (RT-6t.8), after the reference prototype (docs/reference/crowd-proto.html): a
    /// striped track (<c>StudioKit/StripedTrack</c>), the ground and two side rails in one mesh, and recycled trees and
    /// rocks on both sides (<see cref="InstancedProps"/>, one draw per kind). Every mesh is one-hot vertex colors over the
    /// three slots of <c>StudioKit/ToonLit</c>, so the same meshes take any world's <see cref="ToonPalette"/>;
    /// <see cref="WorldScenery"/> applies the palette and the world's density at run time. Meshes and materials are
    /// generated assets (idempotent, GUIDs kept); the scene builder and the captures place them.
    /// </summary>
    public static class SceneryArt
    {
        public const string Folder = "Assets/Claybreak/Content/Generated/Scenery";
        public const string TreePath = Folder + "/Tree.asset";
        public const string RockPath = Folder + "/Rock.asset";
        public const string PropsMaterialPath = Folder + "/PropsToon.mat";
        public const string GroundMaterialPath = Folder + "/GroundToon.mat";
        public const string TrackMaterialPath = Folder + "/StripedTrack.mat";

        /// <summary>Layers of the props, in this order (<see cref="WorldScenery"/> maps the palette onto them).</summary>
        public const int TreeLayer = 0;
        public const int RockLayer = 1;

        /// <summary>One band of each color, 2 m each (the reference: a 64 px texture half striped, 40 times over 160 m).</summary>
        public const float StripePeriod = 4f;

        /// <summary>Rails beside the track (the reference: 0.3 m square, 0.45 m out from the track edge).</summary>
        public const float RailSize = 0.3f;
        public const float RailOffset = 0.45f;

        /// <summary>
        /// The props start this far out from the track edge: the reference keeps 2.5 m; 3.5 m here keeps the widest tree
        /// (cone radius 1 at scale 1.5) clear of the towers, which reach 5.7 m from the center.
        /// </summary>
        public const float PropMarginFromTrack = 3.5f;

        /// <summary>Scale range of the props (the reference: 0.7 to 1.5).</summary>
        public static readonly Vector2 PropScale = new Vector2(0.7f, 1.5f);

        /// <summary>Tallest prop at its largest scale (the tree's top cone ends at 2.9 m).</summary>
        public const float PropHeight = 2.9f * 1.5f;

        // One-hot slot weights (alpha 0: the instance tint is not used).
        private static readonly Color32 Slot0 = new Color32(255, 0, 0, 0);
        private static readonly Color32 Slot1 = new Color32(0, 255, 0, 0);
        private static readonly Color32 Slot2 = new Color32(0, 0, 255, 0);

        /// <summary>
        /// Tree of the reference (<c>treeGeo()</c>): a trunk (slot 0), a wide cone (slot 1, the dark foliage) and a smaller
        /// cone above it (slot 2, the light foliage). The reference trunk tapers from 0.2 to 0.15; a straight 0.18 here
        /// (the kit's cylinder has one radius). 7-sided cones, 6-sided trunk: 50 triangles.
        /// </summary>
        public static Mesh Tree()
        {
            return new LowPolyMeshBuilder()
                .AddCylinder(LowPolyMeshBuilder.Place(new Vector3(0f, 0.35f, 0f), new Vector3(0.18f, 0.7f, 0.18f)), 6, Slot0)
                .AddCone(LowPolyMeshBuilder.Place(new Vector3(0f, 0.5f, 0f), new Vector3(1f, 1.8f, 1f)), 7, Slot1)
                .AddCone(LowPolyMeshBuilder.Place(new Vector3(0f, 1.5f, 0f), new Vector3(0.75f, 1.4f, 0.75f)), 7, Slot2)
                .ToMesh("Tree");
        }

        /// <summary>
        /// Rock of the reference (<c>rockGeo()</c>): a flattened icosahedron of radius 0.6 (slot 0, the rock) and a smaller
        /// one at its side (slot 1, the light foliage, like moss), both split once and flat-shaded: 160 triangles. The
        /// reference's side stone is at z = +0.3 behind its front; −0.3 here (the mirror of three.js −Z front).
        /// </summary>
        public static Mesh Rock()
        {
            return new LowPolyMeshBuilder()
                .AddIcosahedron(LowPolyMeshBuilder.Place(new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.36f, 0.6f)), Slot0, 1)
                .AddIcosahedron(LowPolyMeshBuilder.Place(new Vector3(0.6f, 0.2f, -0.3f), new Vector3(0.4f, 0.24f, 0.4f)), Slot1, 1)
                .ToMesh("Rock");
        }

        /// <summary>
        /// The ground (slot 0, top 2 cm under the track like the reference) from <paramref name="fromZ"/> to
        /// <paramref name="toZ"/>, 300 m wide, and the two rails beside the track (slot 1, the stripe color) along the track,
        /// in one static mesh: one draw.
        /// </summary>
        public static Mesh GroundAndRails(float trackHalfWidth, float trackStart, float trackEnd, float fromZ, float toZ)
        {
            var builder = new LowPolyMeshBuilder();
            builder.AddBox(LowPolyMeshBuilder.Place(new Vector3(0f, -0.07f, (fromZ + toZ) * 0.5f), new Vector3(300f, 0.1f, toZ - fromZ)), Slot0);
            float railX = trackHalfWidth + RailOffset;
            for (int side = -1; side <= 1; side += 2)
            {
                builder.AddBox(LowPolyMeshBuilder.Place(new Vector3(side * railX, RailSize * 0.5f, (trackStart + trackEnd) * 0.5f),
                    new Vector3(RailSize, RailSize, trackEnd - trackStart)), Slot1);
            }

            return builder.ToMesh("GroundAndRails");
        }

        /// <summary>Writes the prop meshes and the three materials (instanced props, static ground, striped track).</summary>
        public static void BuildAssets()
        {
            EnsureFolder(Folder);
            SaveMesh(Tree(), TreePath);
            SaveMesh(Rock(), RockPath);
            Shader toon = Find(ToonShaderIds.ToonLitShader);
            SaveMaterial(PropsMaterialPath, toon, material =>
            {
                material.EnableKeyword(ToonShaderIds.InstancesKeyword);
                material.EnableKeyword(ToonShaderIds.SlotsKeyword);
                material.enableInstancing = false;
            });
            SaveMaterial(GroundMaterialPath, toon, material =>
            {
                material.DisableKeyword(ToonShaderIds.InstancesKeyword);
                material.EnableKeyword(ToonShaderIds.SlotsKeyword);
            });
            SaveMaterial(TrackMaterialPath, Find(ToonShaderIds.StripedTrackShader), material =>
            {
                material.SetFloat(ToonShaderIds.StripePeriod, StripePeriod);
            });
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// The world-independent scenery objects: the striped track (a flat box, top at y = 0, the width of the play area:
        /// its bounds are what the gates and the assault read), the ground with its rails (mesh saved at
        /// <paramref name="groundMeshPath"/>; null keeps it in the scene only, for throwaway capture scenes), and the props following <paramref name="anchor"/> with
        /// <paramref name="treeCapacity"/> trees and <paramref name="rockCapacity"/> rocks preallocated (none shown until a
        /// world is applied).
        /// </summary>
        public static SceneryParts Create(Transform parent, float trackHalfWidth, float trackStart, float trackEnd, Transform anchor,
            int treeCapacity, int rockCapacity, string groundMeshPath)
        {
            BuildAssets();
            float length = trackEnd - trackStart;
            Renderer track = Prim.Box(parent, "Track", new Vector3(0f, -0.1f, (trackStart + trackEnd) * 0.5f),
                new Vector3(trackHalfWidth * 2f, 0.2f, length), Load<Material>(TrackMaterialPath));

            Mesh groundMesh = GroundAndRails(trackHalfWidth, trackStart, trackEnd, trackStart - 60f, trackEnd + 150f);
            if (groundMeshPath != null)
            {
                groundMesh = SaveMesh(groundMesh, groundMeshPath);
            }
            else
            {
                groundMesh.hideFlags = HideFlags.DontSave;
            }

            var groundObject = new GameObject("GroundAndRails", typeof(MeshFilter), typeof(MeshRenderer));
            groundObject.transform.SetParent(parent, false);
            groundObject.GetComponent<MeshFilter>().sharedMesh = groundMesh;
            var ground = groundObject.GetComponent<MeshRenderer>();
            ground.sharedMaterial = Load<Material>(GroundMaterialPath);
            ground.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ground.receiveShadows = false;

            var propsObject = new GameObject("SideProps");
            propsObject.transform.SetParent(parent, false);
            InstancedProps props = propsObject.AddComponent<InstancedProps>();
            var so = new SerializedObject(props);
            so.FindProperty("anchor").objectReferenceValue = anchor;
            so.FindProperty("material").objectReferenceValue = Load<Material>(PropsMaterialPath);
            so.FindProperty("boundsHeight").floatValue = PropHeight + 1f;
            SerializedProperty scatter = so.FindProperty("scatter");
            scatter.FindPropertyRelative("Behind").floatValue = PropScatterSettings.Default.Behind;
            scatter.FindPropertyRelative("Length").floatValue = PropScatterSettings.Default.Length;
            scatter.FindPropertyRelative("CenterX").floatValue = 0f;
            scatter.FindPropertyRelative("InnerX").floatValue = trackHalfWidth + PropMarginFromTrack;
            scatter.FindPropertyRelative("BandWidth").floatValue = PropScatterSettings.Default.BandWidth;
            SerializedProperty layers = so.FindProperty("layers");
            layers.arraySize = 2;
            SetLayer(layers.GetArrayElementAtIndex(TreeLayer), Load<Mesh>(TreePath), treeCapacity);
            SetLayer(layers.GetArrayElementAtIndex(RockLayer), Load<Mesh>(RockPath), rockCapacity);
            so.ApplyModifiedPropertiesWithoutUndo();
            return new SceneryParts(track, ground, props);
        }

        private static void SetLayer(SerializedProperty layer, Mesh mesh, int capacity)
        {
            layer.FindPropertyRelative("Mesh").objectReferenceValue = mesh;
            layer.FindPropertyRelative("Capacity").intValue = capacity;
            layer.FindPropertyRelative("Count").intValue = 0;
            layer.FindPropertyRelative("Scale").vector2Value = PropScale;
            layer.FindPropertyRelative("Slot0").colorValue = Color.white;
            layer.FindPropertyRelative("Slot1").colorValue = Color.white;
            layer.FindPropertyRelative("Slot2").colorValue = Color.white;
        }

        private static Mesh SaveMesh(Mesh mesh, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }

            existing.Clear();
            EditorUtility.CopySerialized(mesh, existing);
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static void SaveMaterial(string path, Shader shader, System.Action<Material> configure)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            configure(material);
            EditorUtility.SetDirty(material);
        }

        private static T Load<T>(string path) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new System.InvalidOperationException("missing " + path);
            }

            return asset;
        }

        private static Shader Find(string name)
        {
            Shader shader = Shader.Find(name);
            if (shader == null)
            {
                throw new System.InvalidOperationException("shader not found: " + name);
            }

            return shader;
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

    /// <summary>The scenery objects of one scene, as <see cref="SceneryArt.Create"/> made them.</summary>
    public readonly struct SceneryParts
    {
        public SceneryParts(Renderer track, Renderer ground, InstancedProps props)
        {
            Track = track;
            Ground = ground;
            Props = props;
        }

        public Renderer Track { get; }
        public Renderer Ground { get; }
        public InstancedProps Props { get; }
    }
}
