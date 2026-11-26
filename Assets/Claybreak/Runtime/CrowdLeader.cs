using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak
{
    /// <summary>
    /// The crowd's leader (RT-6t.13, user decision 2026-11-22): one unit of the crowd's shape and color, <see cref="Scale"/>
    /// times bigger, with a crown in the world's UI color, drawn at the front of the formation. It is drawn through the same
    /// instanced pipeline as the crowd (<see cref="ToonInstanceDraw"/>: one instance for the body and its blob shadow, one
    /// for the crown), so there is no object per unit and nothing is created while playing. It bounces and leans like the
    /// units (<see cref="SwarmMotion"/>, <see cref="SwarmAnimation"/>). Where it stands is decided by <see cref="RunCrowd"/>
    /// (<see cref="Place"/>); it is not part of the army.
    /// </summary>
    public sealed class CrowdLeader : MonoBehaviour
    {
        /// <summary>The leader's size against a crowd unit (the approved proposal: about 1.6×).</summary>
        public const float Scale = 1.6f;

        [Tooltip("The crowd: the leader wears its shape and tint and draws on its layer.")]
        [SerializeField] private SwarmRenderer swarm;
        [Tooltip("StudioKit/ToonLit with the instances keyword (the crowd's material).")]
        [SerializeField] private Material bodyMaterial;
        [Tooltip("StudioKit/BlobShadowInstanced (the crowd's shadow).")]
        [SerializeField] private Material shadowMaterial;
        [SerializeField] private Mesh shadowMesh;
        [Tooltip("The crown: white vertices with alpha 1 (they take the crown color), base at y = 0, about 1 m across.")]
        [SerializeField] private Mesh crownMesh;
        [Tooltip("StudioKit/ToonLit with the instances keyword, a little emission so the crown reads on every world.")]
        [SerializeField] private Material crownMaterial;
        [Tooltip("Crown width against the unit's width.")]
        [Min(0.05f)] [SerializeField] private float crownWidth = 0.85f;
        [Tooltip("How far the crown sinks into the head (share of the unit's height).")]
        [Range(0f, 0.5f)] [SerializeField] private float crownSink = 0.12f;
        [SerializeField] private SwarmMotionSettings motion = SwarmMotionSettings.Default;
        [SerializeField] private SwarmAnimationSettings unitAnimation = SwarmAnimationSettings.Default;

        private ToonInstanceDraw bodyDraw;
        private ToonInstanceDraw crownDraw;
        private NativeArray<ToonInstance> body;
        private NativeArray<ToonInstance> crown;
        private bool headless;
        private Vector3 ground;
        private float lastX;
        private bool placed;
        private Color crownColor = Color.white;
        private Mesh measuredMesh;
        private float unitHeight = 1f;
        private float unitWidth = 1f;
        private float crownSeat;
        private Mesh leaderMesh;

        /// <summary>The hop the leader draws with now (the worn shape's, RT-6t.20).</summary>
        public SwarmMotionSettings Motion => motion;

        /// <summary>
        /// The worn shape's hop and crown seat (RT-6t.20): the leader hops like its crowd; <paramref name="seat"/> is the
        /// crown's height on the unit (m), 0 = the top of the mesh. RT-6v3.4a: <paramref name="mesh"/> is the shape's detailed
        /// level (null = the crowd's mesh).
        /// </summary>
        public void UseShape(in SwarmMotionSettings hop, float seat, Mesh mesh = null)
        {
            motion = hop;
            crownSeat = Mathf.Max(0f, seat);
            leaderMesh = mesh;
        }

        /// <summary>The mesh the leader draws now: the worn shape's detailed level, or the crowd's.</summary>
        public Mesh BodyMesh => leaderMesh != null ? leaderMesh : swarm.UnitMesh;

        /// <summary>Where the leader stands on the ground now.</summary>
        public Vector3 Ground => ground;

        /// <summary>The crown color (the world's UI accent).</summary>
        public Color CrownColor => crownColor;

        /// <summary>Instanced draws of the last frame (body, shadow, crown).</summary>
        public int DrawCalls { get; private set; }

        public string Validate()
        {
            if (swarm == null || bodyMaterial == null || crownMesh == null || crownMaterial == null)
            {
                return "CrowdLeader needs the crowd, its material, the crown mesh and the crown material.";
            }

            if (!bodyMaterial.IsKeywordEnabled(ToonShaderIds.InstancesKeyword) || !crownMaterial.IsKeywordEnabled(ToonShaderIds.InstancesKeyword))
            {
                return $"CrowdLeader: the body and crown materials must have {ToonShaderIds.InstancesKeyword} on.";
            }

            return ToonInstanceDraw.ValidateMaterial(bodyMaterial) ?? ToonInstanceDraw.ValidateMaterial(crownMaterial)
                ?? (shadowMaterial != null ? ToonInstanceDraw.ValidateMaterial(shadowMaterial) : null);
        }

        /// <summary>Puts the leader on the ground at <paramref name="position"/>; <paramref name="snap"/> skips the lean.</summary>
        public void Place(Vector3 position, bool snap = false)
        {
            ground = position;
            if (snap || !placed)
            {
                lastX = position.x;
                placed = true;
            }
        }

        /// <summary>The crown color (the world's UI accent); a few property writes, never per frame.</summary>
        public void WearCrown(Color color)
        {
            crownColor = color;
        }

        private void OnEnable()
        {
            string error = Validate();
            if (error != null)
            {
                Debug.LogError("[Leader] " + error, this);
                enabled = false;
                return;
            }

            body = new NativeArray<ToonInstance>(1, Allocator.Persistent);
            crown = new NativeArray<ToonInstance>(1, Allocator.Persistent);
            headless = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
            if (!headless)
            {
                CreateDraws();
            }
        }

        private void OnDisable()
        {
            bodyDraw?.Dispose();
            crownDraw?.Dispose();
            bodyDraw = null;
            crownDraw = null;
            if (body.IsCreated)
            {
                body.Dispose();
            }

            if (crown.IsCreated)
            {
                crown.Dispose();
            }

            DrawCalls = 0;
        }

        // The leader reads the crowd's shape and tint, not its instances: no ordering with the swarm's own LateUpdate.
        private void LateUpdate()
        {
            if (headless || !placed)
            {
                return;
            }

            if (bodyDraw.Stale)
            {
                // A route was forced (tests, measurement builds): the other route's draws.
                bodyDraw.Dispose();
                crownDraw.Dispose();
                CreateDraws();
            }

            Mesh unit = BodyMesh;
            if (unit != measuredMesh)
            {
                Bounds bounds = unit.bounds;
                unitHeight = bounds.max.y;
                unitWidth = Mathf.Max(bounds.size.x, bounds.size.z);
                measuredMesh = unit;
            }

            float deltaTime = Mathf.Max(Time.deltaTime, 1e-4f);
            float acrossSpeed = (ground.x - lastX) / deltaTime;
            lastX = ground.x;
            Compose(Time.time, acrossSpeed);
            bodyDraw.Upload(body, 1);
            crownDraw.Upload(crown, 1);
            var drawBounds = new Bounds(ground + new Vector3(0f, unitHeight * Scale, 0f), new Vector3(3f, unitHeight * Scale * 2f + 2f, 3f));
            bodyDraw.Draw(unit, 1, drawBounds);
            int calls = 1;
            if (shadowMaterial != null && shadowMesh != null)
            {
                bodyDraw.DrawShadows(shadowMesh, 1, drawBounds);
                calls++;
            }

            crownDraw.Draw(crownMesh, 1, drawBounds);
            DrawCalls = calls + 1;
        }

        // Body: the unit's bounce, squash, lean and tilt at the leader's scale; crown: on top of the head, carried by the
        // body's transform (same lean, tilt and squash).
        private void Compose(float time, float acrossSpeed)
        {
            float bounce = SwarmMotion.Bounce(time, 0f, motion.Frequency);
            float sy = SwarmMotion.SquashY(bounce, motion);
            float sxz = SwarmMotion.SquashXZ(sy);
            float tilt = SwarmAnimation.Tilt(acrossSpeed, unitAnimation);
            Color skin = swarm.Tint.linear;
            var instance = new ToonInstance
            {
                Tint = new float4(skin.r, skin.g, skin.b, Scale * SwarmAnimation.ShadowScale(bounce, 1f, unitAnimation)),
            };
            var position = new float3(ground.x, bounce * motion.HopHeight * Scale, ground.z);
            SwarmAnimation.Compose(ref instance, position, motion.ForwardLean, tilt, sxz * Scale, sy * Scale);
            body[0] = instance;

            // Crown = body · T(0, head, 0) · S(width): the body's 3×3 times the crown scale, moved up the body's own axis.
            // float3x3(a, b, c) takes columns: the rows go in transposed.
            float3x3 basis = math.transpose(new float3x3(instance.Row0.xyz, instance.Row1.xyz, instance.Row2.xyz));
            float head = (crownSeat > 0f ? crownSeat : unitHeight) * (1f - crownSink);
            float3 offset = math.mul(basis, new float3(0f, head, 0f));
            float3x3 scaled = math.mul(basis, float3x3.Scale(unitWidth * crownWidth));
            Color accent = crownColor.linear;
            crown[0] = new ToonInstance
            {
                Row0 = new float4(scaled.c0.x, scaled.c1.x, scaled.c2.x, position.x + offset.x),
                Row1 = new float4(scaled.c0.y, scaled.c1.y, scaled.c2.y, position.y + offset.y),
                Row2 = new float4(scaled.c0.z, scaled.c1.z, scaled.c2.z, position.z + offset.z),
                Tint = new float4(accent.r, accent.g, accent.b, 0f),
            };
        }

        private void CreateDraws()
        {
            int layer = swarm.gameObject.layer;
            bodyDraw = new ToonInstanceDraw(bodyMaterial, shadowMaterial, 1, layer);
            crownDraw = new ToonInstanceDraw(crownMaterial, null, 1, layer);
        }
    }
}
