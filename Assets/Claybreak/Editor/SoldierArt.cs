using JungleDegree.StudioKit.Characters;
using JungleDegree.StudioKit.Editor;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// RT-6v (direction v2, D24 = A): the game's own soldier, a glossy bean with a round helmet, a plume, a shield,
    /// two hands and two feet, WITHOUT face or eyes (D17). It is built in code with the kit's
    /// <see cref="SoftMeshBuilder"/> as a <see cref="RunnerBodyFrames"/>: the kit's run cycle
    /// (<see cref="RunnerRunMeshFactory.FrameCount"/> poses), the rest pose and the kit's clips (Idle, Start, Cheer,
    /// Trip), every pose deforming the same vertices, so the kit bakes them into its pose texture and the crowd stays
    /// one instanced draw. The body paints the kit's four color regions in its vertex colors: base (body and
    /// helmet: the shop color), accent (gold brim, plume and hands), detail (navy boots) and the fourth slot (the
    /// shield's cream face). At most <see cref="MaximumVertices"/> vertices (the crowd's vertex cost).
    /// </summary>
    public static class SoldierArt
    {
        public const int MaximumVertices = 600;
        public const string Folder = ClaybreakSceneBuilder.Root + "/Generated/Soldier";
        public const string FramesPath = Folder + "/SoldierFrames.asset";
        public const string SkinPath = Folder + "/SoldierSkin.asset";
        /// <summary>Soldier height relative to the kit's built-in runner (same crowd spacing and colliders).</summary>
        public const float HeightToKitRunner = 0.9f;

        private const float AuthoredHeight = 1.24f;
        private static readonly Color BaseRegion = new Color(1f, 0f, 0f, 0f);
        private static readonly Color AccentRegion = new Color(0f, 1f, 0f, 0f);
        private static readonly Color DetailRegion = new Color(0f, 0f, 1f, 0f);
        private static readonly Color FourthRegion = new Color(0f, 0f, 0f, 1f);

        /// <summary>One pose of the soldier (authored units: 1.24 tall, feet on y = 0, facing +Z).</summary>
        private struct Pose
        {
            public float Bob;
            public float Squash;
            public float Pitch;
            public Vector2 LeftFoot;
            public Vector2 RightFoot;
            public Vector2 LeftHand;
            public Vector2 RightHand;
        }

        /// <summary>Builds (or rebuilds in place) the soldier frames; <paramref name="kitRunner"/> sets the height.</summary>
        public static RunnerBodyFrames Build(RunnerBodyFrames kitRunner)
        {
            float scale = HeightToKitRunner * kitRunner.Idle.bounds.size.y / AuthoredHeight;
            var frames = new Mesh[RunnerRunMeshFactory.FrameCount + 1];
            var anchors = new RunnerBodyAnchorPose[frames.Length];
            for (int f = 0; f < RunnerRunMeshFactory.FrameCount; f++)
            {
                float t = f * Mathf.PI * 2f / RunnerRunMeshFactory.FrameCount;
                frames[f] = Save(Shape(Run(t), scale, out anchors[f]), $"Run{f:00}");
            }

            frames[RunnerRunMeshFactory.IdleFrame] = Save(Shape(default, scale, out anchors[RunnerRunMeshFactory.IdleFrame]), "Rest");
            RunnerBodyClip[] clips =
            {
                Clip("Idle", true, 8, scale, i => new Pose { Squash = 0.03f * Mathf.Sin(i * Mathf.PI * 2f / 8f) }),
                Clip("Start", false, 4, scale, i => new Pose { Squash = 0.08f * (1f - i / 3f), Pitch = 12f * (1f - i / 3f) }),
                Clip("Cheer", true, 8, scale, i =>
                {
                    float hop = Mathf.Sin(i * Mathf.PI / 8f);
                    return new Pose
                    {
                        Bob = 0.16f * hop, Squash = 0.06f * (1f - 2f * hop),
                        LeftHand = new Vector2(0f, 0.3f), RightHand = new Vector2(0f, 0.36f + 0.06f * hop),
                        LeftFoot = new Vector2(0f, 0.04f * hop), RightFoot = new Vector2(0f, 0.04f * hop)
                    };
                }),
                Clip("Trip", false, 6, scale, i =>
                {
                    float fall = i / 5f;
                    return new Pose
                    {
                        Pitch = 55f * fall, Bob = -0.06f * fall, LeftHand = new Vector2(0.2f * fall, 0.1f),
                        RightHand = new Vector2(0.22f * fall, 0.12f), LeftFoot = new Vector2(-0.12f * fall, 0f)
                    };
                })
            };

            var body = AssetDatabase.LoadAssetAtPath<RunnerBodyFrames>(FramesPath);
            if (body == null)
            {
                body = ScriptableObject.CreateInstance<RunnerBodyFrames>();
                AssetDatabase.CreateAsset(body, FramesPath);
            }

            body.EditorSetFrames(frames, anchors, clips);
            // The head (helmet) is above ~0.62 of the height: it lags the lean as one piece (kit Stage M4).
            body.EditorSetShape(new Vector2(-0.2f * scale, -0.1f * scale), 0.4f * scale, true);
            EditorUtility.SetDirty(body);
            if (!body.TryValidate(out string error))
            {
                throw new System.InvalidOperationException("soldier frames: " + error);
            }

            return body;
        }

        /// <summary>The color look the theme gives the shop colors: the shop color is the base, the rest is fixed.</summary>
        public static RunnerSkin BuildSkin(RunnerBodyFrames body)
        {
            var skin = AssetDatabase.LoadAssetAtPath<RunnerSkin>(SkinPath);
            if (skin == null)
            {
                skin = ScriptableObject.CreateInstance<RunnerSkin>();
                AssetDatabase.CreateAsset(skin, SkinPath);
            }

            var so = new SerializedObject(skin);
            so.FindProperty("body").objectReferenceValue = body;
            so.FindProperty("baseColor").colorValue = ToyPalette.Cobalt;
            so.FindProperty("accentColor").colorValue = ToyPalette.Gold;
            so.FindProperty("detailColor").colorValue = ToyPalette.Navy;
            so.FindProperty("eyeColor").colorValue = ToyPalette.Cream;
            so.FindProperty("attachments").arraySize = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skin);
            return skin;
        }

        // The run cycle: two steps per cycle, a bounce on each, feet and hands swinging in opposition.
        private static Pose Run(float t)
        {
            float step = Mathf.Abs(Mathf.Sin(t));
            return new Pose
            {
                Bob = 0.06f * step,
                Squash = -0.04f * step + 0.03f * (1f - step),
                Pitch = 6f,
                LeftFoot = new Vector2(0.17f * Mathf.Sin(t), 0.1f * Mathf.Max(0f, Mathf.Cos(t))),
                RightFoot = new Vector2(-0.17f * Mathf.Sin(t), 0.1f * Mathf.Max(0f, -Mathf.Cos(t))),
                LeftHand = new Vector2(-0.07f * Mathf.Sin(t), 0f),
                RightHand = new Vector2(0.15f * Mathf.Sin(t), 0.04f * step)
            };
        }

        private static RunnerBodyClip Clip(string name, bool loop, int count, float scale, System.Func<int, Pose> pose)
        {
            var clip = new RunnerBodyClip { Name = name, Loop = loop, Frames = new Mesh[count], Anchors = new RunnerBodyAnchorPose[count] };
            for (int i = 0; i < count; i++)
            {
                clip.Frames[i] = Save(Shape(pose(i), scale, out clip.Anchors[i]), $"{name}{i:00}");
            }

            return clip;
        }

        private static Mesh Save(Mesh mesh, string name)
        {
            return ToyArt.SaveMeshAt(mesh, $"{Folder}/Poses/{name}.asset");
        }

        // Every pose adds the same shapes in the same order with the same segment counts: same vertices, new places.
        private static Mesh Shape(Pose pose, float scale, out RunnerBodyAnchorPose anchors)
        {
            var b = new SoftMeshBuilder();
            Matrix4x4 size = Matrix4x4.Scale(Vector3.one * scale);
            Matrix4x4 lean = size * Matrix4x4.Rotate(Quaternion.Euler(pose.Pitch, 0f, 0f));
            float sy = 1f - pose.Squash;
            float sx = 1f + pose.Squash * 0.5f;
            float center = 0.5f + pose.Bob;
            float helmet = center + 0.4f * sy - 0.12f;

            b.Frame = size;
            b.Ellipsoid(new Vector3(-0.13f, 0.075f + pose.LeftFoot.y, pose.LeftFoot.x + 0.03f), new Vector3(0.11f, 0.075f, 0.15f), DetailRegion, 8, 5);
            b.Ellipsoid(new Vector3(0.13f, 0.075f + pose.RightFoot.y, pose.RightFoot.x + 0.03f), new Vector3(0.11f, 0.075f, 0.15f), DetailRegion, 8, 5);

            b.Frame = lean;
            b.Ellipsoid(new Vector3(0f, center, 0f), new Vector3(0.33f * sx, 0.4f * sy, 0.3f * sx), BaseRegion, 12, 8);
            b.Dome(new Vector3(0f, helmet, 0f), 0.31f, 0.3f, BaseRegion, 0f, 12, 5, false);
            b.Ring(new Vector3(0f, helmet, 0f), 0.34f, 0.042f, AccentRegion, 12, 5);
            b.Ellipsoid(new Vector3(0f, helmet + 0.36f, -0.03f), new Vector3(0.045f, 0.11f, 0.09f), AccentRegion, 6, 4);
            b.Ellipsoid(new Vector3(0.36f, center - 0.02f + pose.RightHand.y, 0.04f + pose.RightHand.x), Vector3.one * 0.085f, AccentRegion, 6, 4);
            b.Ellipsoid(new Vector3(-0.36f, center - 0.02f + pose.LeftHand.y, 0.04f + pose.LeftHand.x), Vector3.one * 0.085f, AccentRegion, 6, 4);
            b.Ellipsoid(new Vector3(-0.4f, center + pose.LeftHand.y, 0.06f + pose.LeftHand.x), new Vector3(0.045f, 0.19f, 0.16f), FourthRegion, 10, 5);

            anchors = new RunnerBodyAnchorPose
            {
                HeadPosition = lean.MultiplyPoint3x4(new Vector3(0f, helmet + 0.1f, 0f)),
                HeadRotation = Quaternion.Euler(pose.Pitch, 0f, 0f),
                ChestPosition = lean.MultiplyPoint3x4(new Vector3(0f, center + 0.12f, 0.1f)),
                ChestRotation = Quaternion.Euler(pose.Pitch, 0f, 0f),
                PelvisPosition = lean.MultiplyPoint3x4(new Vector3(0f, center - 0.2f, 0f)),
                PelvisRotation = Quaternion.Euler(pose.Pitch, 0f, 0f)
            };

            if (b.VertexCount > MaximumVertices)
            {
                throw new System.InvalidOperationException($"the soldier has {b.VertexCount} vertices (limit {MaximumVertices}).");
            }

            return b.Build("Soldier");
        }
    }
}
