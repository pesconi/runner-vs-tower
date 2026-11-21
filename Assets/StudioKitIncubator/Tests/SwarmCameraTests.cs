using System;
using JungleDegree.StudioKit.Swarm;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace JungleDegree.StudioKit.Tests
{
    /// <summary>
    /// The swarm camera against the reference prototype's frame() (camD, position, lookAt, shake; its run goes toward
    /// −Z, the kit's toward +Z, so the depth sign is mirrored) and the portrait fit checked with Unity's own projection.
    /// </summary>
    public sealed class SwarmCameraTests
    {
        private const float Track = 6f;

        [Test]
        public void DistanceMatchesTheReference()
        {
            int[] counts = { 0, 1, 10, 100, 484, 1000, 2000 };
            foreach (int n in counts)
            {
                Assert.AreEqual(1 + Math.Sqrt(n) / 22, SwarmCameraMath.TargetDistance(n, 22f), 1e-5, $"n={n}");
            }

            // camD+=((1+Math.sqrt(n)/22)-camD)*(1-Math.exp(-dt*2)), frame by frame
            double camD = 1;
            float distance = 1f;
            for (int frame = 0; frame < 300; frame++)
            {
                int n = frame < 100 ? 10 : 1000;
                const float dt = 1f / 60f;
                camD += (1 + Math.Sqrt(n) / 22 - camD) * (1 - Math.Exp(-dt * 2));
                distance = SwarmCameraMath.Smooth(distance, SwarmCameraMath.TargetDistance(n, 22f), 2f, dt);
                Assert.AreEqual(camD, distance, 1e-4, $"frame {frame}");
            }
        }

        [Test]
        public void PoseMatchesTheReferenceMirroredToPlusZ()
        {
            SwarmCameraSettings settings = SwarmCameraSettings.Default;
            float[] centers = { -5.5f, 0f, 2.25f };
            float[] distances = { 1f, 1.14f, 2.44f };
            foreach (float cx in centers)
            {
                foreach (float camD in distances)
                {
                    // camera.position.set(cx*.35, 3.2+4.2*camD, 5.5+5.5*camD); camera.lookAt(cx*.35, 0, -5)
                    SwarmCameraPose pose = SwarmCameraMath.Pose(settings, 0f, cx, 0f, camD);
                    AssertNear(new float3(cx * .35f, 3.2f + 4.2f * camD, -(5.5f + 5.5f * camD)), pose.Position);
                    AssertNear(new float3(cx * .35f, 0f, 5f), pose.Target);
                    Assert.AreEqual(50f, pose.VerticalFov);
                }
            }

            SwarmCameraPose moved = SwarmCameraMath.Pose(settings, 10f, 12f, 30f, 1f);
            AssertNear(new float3(10f + 2f * .35f, 7.4f, 30f - 11f), moved.Position);
            AssertNear(new float3(10.7f, 0f, 35f), moved.Target);
        }

        [Test]
        public void ShakeMatchesTheReference()
        {
            // shake=.35 on a bad gate; shake=Math.max(0,shake-dt); sh=shake*.6; jitter (Math.random()-.5)*sh
            Assert.AreEqual(0.35f, SwarmCameraSettings.Default.ShakeDuration);
            AssertNear(new float2(0.5f * 0.35f * 0.6f, -0.5f * 0.35f * 0.6f), SwarmCameraMath.ShakeJitter(0.35f, 0.6f, new float2(1f, 0f)));
            AssertNear(float2.zero, SwarmCameraMath.ShakeJitter(0f, 0.6f, new float2(1f, 0f)));
            AssertNear(float2.zero, SwarmCameraMath.ShakeJitter(-1f, 0.6f, new float2(1f, 0f)));
            AssertNear(float2.zero, SwarmCameraMath.ShakeJitter(0.2f, 0.6f, new float2(0.5f, 0.5f)));
        }

        [Test]
        public void WideScreensKeepTheReferenceFraming()
        {
            SwarmCameraSettings settings = SwarmCameraSettings.Default;
            foreach (int n in new[] { 10, 100, 1000 })
            {
                SwarmCameraPose pose = SwarmCameraMath.Pose(settings, 0f, 0f, 0f, SwarmCameraMath.TargetDistance(n, 22f));
                SwarmCameraPose fitted = SwarmCameraMath.FitAcross(pose, settings, 16f / 9f, 0f, Track + settings.FrameMargin, 0f);
                Assert.AreEqual(50f, fitted.VerticalFov, $"n={n}");
                AssertNear(pose.Position, fitted.Position);
            }
        }

        [Test]
        public void PortraitScreensFitTheWholeTrack()
        {
            SwarmCameraSettings settings = SwarmCameraSettings.Default;
            float halfWidth = Track + settings.FrameMargin;
            float[] aspects = { 9f / 16f, 1080f / 2354f, 3f / 4f };
            foreach (float aspect in aspects)
            {
                foreach (int n in new[] { 10, 100, 1000, 2000 })
                {
                    foreach (float cx in new[] { -5.5f, 0f, 5.5f })
                    {
                        string label = $"aspect {aspect:0.###}, n={n}, cx={cx}";
                        SwarmCameraPose pose = SwarmCameraMath.Pose(settings, 0f, cx, 0f, SwarmCameraMath.TargetDistance(n, 22f));
                        SwarmCameraPose fitted = SwarmCameraMath.FitAcross(pose, settings, aspect, 0f, halfWidth, 0f);
                        Assert.That(fitted.VerticalFov, Is.InRange(settings.VerticalFov, settings.MaxVerticalFov), label);
                        float3 view = math.normalize(fitted.Target - fitted.Position);
                        AssertNear(math.normalize(pose.Target - pose.Position), view, 1e-4f);
                        float left = ScreenX(fitted, aspect, new Vector3(-halfWidth, 0f, 0f));
                        float right = ScreenX(fitted, aspect, new Vector3(halfWidth, 0f, 0f));
                        Assert.That(left, Is.GreaterThanOrEqualTo(-1.001f), label + " left edge");
                        Assert.That(right, Is.LessThanOrEqualTo(1.001f), label + " right edge");
                        if (fitted.VerticalFov > settings.VerticalFov)
                        {
                            Assert.AreEqual(1f, Math.Max(-left, right), 2e-3f, label + ": the fit is tight (no wasted zoom)");
                        }
                    }
                }
            }
        }

        [Test]
        public void SteeringAcrossTheLaneDoesNotZoom()
        {
            SwarmCameraSettings settings = SwarmCameraSettings.Default;
            float halfWidth = Track + settings.FrameMargin;
            foreach (float aspect in new[] { 9f / 16f, 1080f / 2354f, 16f / 9f })
            {
                foreach (int n in new[] { 10, 1000 })
                {
                    float d = SwarmCameraMath.TargetDistance(n, 22f);
                    SwarmCameraPose centered = SwarmCameraMath.FitLane(settings, aspect, 0f, Track, 0f, 0f, d);
                    for (float cx = -Track; cx <= Track; cx += 1.5f)
                    {
                        string label = $"aspect {aspect:0.###}, n={n}, cx={cx}";
                        SwarmCameraPose pose = SwarmCameraMath.FitLane(settings, aspect, 0f, Track, cx, 0f, d);
                        Assert.AreEqual(centered.VerticalFov, pose.VerticalFov, 1e-4f, label);
                        Assert.AreEqual(centered.Position.y, pose.Position.y, 1e-4f, label + " height");
                        Assert.AreEqual(centered.Position.z, pose.Position.z, 1e-4f, label + " back");
                        Assert.AreEqual(cx * settings.FollowAcross, pose.Position.x, 1e-4f, label + " follows across");
                        Assert.That(ScreenX(pose, aspect, new Vector3(-halfWidth, 0f, 0f)), Is.GreaterThanOrEqualTo(-1.001f), label);
                        Assert.That(ScreenX(pose, aspect, new Vector3(halfWidth, 0f, 0f)), Is.LessThanOrEqualTo(1.001f), label);
                    }
                }
            }

            SwarmCameraPose noLane = SwarmCameraMath.FitLane(settings, 9f / 16f, 0f, 0f, 2f, 0f, 1f);
            AssertNear(SwarmCameraMath.Pose(settings, 0f, 2f, 0f, 1f).Position, noLane.Position);
        }

        [Test]
        public void ThePortraitCameraStillBacksAwayAsTheCrowdGrows()
        {
            SwarmCameraSettings settings = SwarmCameraSettings.Default;
            float halfWidth = Track + settings.FrameMargin;
            foreach (float aspect in new[] { 9f / 16f, 1080f / 2354f })
            {
                float previous = 0f;
                float extra = float.NaN;
                foreach (int n in new[] { 0, 10, 100, 300, 1000, 2000 })
                {
                    string label = $"aspect {aspect:0.###}, n={n}";
                    float d = SwarmCameraMath.TargetDistance(n, 22f);
                    SwarmCameraPose reference = SwarmCameraMath.Pose(settings, 0f, 0f, 0f, d);
                    SwarmCameraPose pose = SwarmCameraMath.FitLane(settings, aspect, 0f, Track, 0f, 0f, d);
                    float length = math.distance(pose.Position, pose.Target);
                    float added = length - math.distance(reference.Position, reference.Target);
                    Assert.Greater(length, previous, label + ": farther than with fewer units");
                    Assert.Greater(added, 0f, label + ": portrait needs more room than the reference");
                    if (!float.IsNaN(extra))
                    {
                        Assert.AreEqual(extra, added, 1e-3f, label + ": the fit adds a fixed distance");
                    }

                    extra = added;
                    previous = length;
                    Assert.AreEqual(length / math.distance(reference.Position, reference.Target),
                        SwarmCameraMath.FramingScale(settings, pose, d), 1e-4f, label);
                    foreach (float cx in new[] { -Track, 0f, Track })
                    {
                        SwarmCameraPose steered = SwarmCameraMath.FitLane(settings, aspect, 0f, Track, cx, 0f, d);
                        Assert.That(ScreenX(steered, aspect, new Vector3(-halfWidth, 0f, 0f)), Is.GreaterThanOrEqualTo(-1.001f), label);
                        Assert.That(ScreenX(steered, aspect, new Vector3(halfWidth, 0f, 0f)), Is.LessThanOrEqualTo(1.001f), label);
                    }
                }
            }

            SwarmCameraPose wide = SwarmCameraMath.FitLane(settings, 16f / 9f, 0f, Track, 3f, 0f, 1.5f);
            AssertNear(SwarmCameraMath.Pose(settings, 0f, 3f, 0f, 1.5f).Position, wide.Position);
            Assert.AreEqual(50f, wide.VerticalFov);
            Assert.AreEqual(1f, SwarmCameraMath.FramingScale(settings, wide, 1.5f), 1e-5f, "wide screens keep the reference framing");
        }

        [Test]
        public void ThePortraitFitOpensTheFieldOfViewBeforeBackingAway()
        {
            SwarmCameraSettings settings = SwarmCameraSettings.Default;
            SwarmCameraPose pose = SwarmCameraMath.Pose(settings, 0f, 0f, 0f, 1f);
            SwarmCameraPose fitted = SwarmCameraMath.FitAcross(pose, settings, 9f / 16f, 0f, Track + settings.FrameMargin, 0f);
            Assert.AreEqual(settings.MaxVerticalFov, fitted.VerticalFov, 1e-4f);
            Assert.Greater(math.distance(fitted.Position, fitted.Target), math.distance(pose.Position, pose.Target));

            settings.MaxVerticalFov = 100f;
            SwarmCameraPose widened = SwarmCameraMath.FitAcross(pose, settings, 9f / 16f, 0f, Track + settings.FrameMargin, 0f);
            Assert.That(widened.VerticalFov, Is.GreaterThan(50f).And.LessThan(100f));
            AssertNear(pose.Position, widened.Position);

            SwarmCameraPose noLane = SwarmCameraMath.FitAcross(pose, settings, 9f / 16f, 0f, 0f, 0f);
            Assert.AreEqual(50f, noLane.VerticalFov);
        }

        [Test]
        public void TheProfileAssetDefaultsAreTheReference()
        {
            var profile = ScriptableObject.CreateInstance<SwarmCameraProfile>();
            try
            {
                SwarmCameraSettings fromAsset = profile.Settings;
                SwarmCameraSettings reference = SwarmCameraSettings.Default;
                Assert.AreEqual(reference.Growth, fromAsset.Growth);
                Assert.AreEqual(reference.DistanceSharpness, fromAsset.DistanceSharpness);
                Assert.AreEqual(reference.BaseHeight, fromAsset.BaseHeight);
                Assert.AreEqual(reference.HeightPerDistance, fromAsset.HeightPerDistance);
                Assert.AreEqual(reference.BaseBack, fromAsset.BaseBack);
                Assert.AreEqual(reference.BackPerDistance, fromAsset.BackPerDistance);
                Assert.AreEqual(reference.LookAhead, fromAsset.LookAhead);
                Assert.AreEqual(reference.FollowAcross, fromAsset.FollowAcross);
                Assert.AreEqual(reference.VerticalFov, fromAsset.VerticalFov);
                Assert.AreEqual(reference.MaxVerticalFov, fromAsset.MaxVerticalFov);
                Assert.AreEqual(reference.FrameMargin, fromAsset.FrameMargin);
                Assert.AreEqual(reference.FitAhead, fromAsset.FitAhead);
                Assert.AreEqual(reference.ShakeDuration, fromAsset.ShakeDuration);
                Assert.AreEqual(reference.ShakeAmplitude, fromAsset.ShakeAmplitude);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        // Normalized screen x (−1 left edge, +1 right edge) of a world point, with Unity's camera conventions.
        private static float ScreenX(SwarmCameraPose pose, float aspect, Vector3 point)
        {
            Vector3 position = pose.Position;
            var rotation = Quaternion.LookRotation((Vector3)pose.Target - position);
            Matrix4x4 worldToCamera = Matrix4x4.TRS(position, rotation, new Vector3(1f, 1f, -1f)).inverse;
            Matrix4x4 projection = Matrix4x4.Perspective(pose.VerticalFov, aspect, 0.3f, 200f);
            Vector4 clip = projection * worldToCamera * new Vector4(point.x, point.y, point.z, 1f);
            Assert.Greater(clip.w, 0f, "the point is behind the camera");
            return clip.x / clip.w;
        }

        private static void AssertNear(float3 expected, float3 actual, float tolerance = 1e-4f)
        {
            Assert.AreEqual(expected.x, actual.x, tolerance);
            Assert.AreEqual(expected.y, actual.y, tolerance);
            Assert.AreEqual(expected.z, actual.z, tolerance);
        }

        private static void AssertNear(float2 expected, float2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, 1e-5f);
            Assert.AreEqual(expected.y, actual.y, 1e-5f);
        }
    }
}
