using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using NUnit.Framework;
using Unity.Mathematics;

namespace JungleDegree.StudioKit.Tests
{
    public sealed class SwarmMotionTests
    {
        [Test]
        public void BounceAndSquashFollowTheReferenceFormulas()
        {
            SwarmMotionSettings motion = SwarmMotionSettings.Default;
            Assert.AreEqual(0f, SwarmMotion.Bounce(0f, 0f, motion.Frequency), 1e-6f);
            float top = math.PI / 2f / motion.Frequency;
            Assert.AreEqual(1f, SwarmMotion.Bounce(top, 0f, motion.Frequency), 1e-5f);
            Assert.AreEqual(0.8f, SwarmMotion.SquashY(0f, motion), 1e-6f);
            Assert.AreEqual(1.12f, SwarmMotion.SquashY(1f, motion), 1e-6f);
            float sy = SwarmMotion.SquashY(0.5f, motion);
            float sxz = SwarmMotion.SquashXZ(sy);
            Assert.AreEqual(1f, sy * sxz * sxz, 1e-4f, "the squash keeps the volume");
        }

        [Test]
        public void SunflowerOffsetsMatchTheReference()
        {
            for (int i = 0; i < 50; i++)
            {
                float radius = math.sqrt(i + 0.5f) * 0.25f;
                float2 expected = new float2(math.cos(i * 2.39996f) * radius, math.sin(i * 2.39996f) * radius * 1.35f);
                float2 actual = SwarmMotion.SunflowerOffset(i, 0.25f, 1.35f);
                Assert.AreEqual(expected.x, actual.x, 1e-5f);
                Assert.AreEqual(expected.y, actual.y, 1e-5f);
            }
        }

        [Test]
        public void SimulationWritesOneInstancePerUnitWithTheHop()
        {
            using var simulation = new SwarmSimulation(128, 0.25f, 1.35f, 7);
            simulation.SetCount(1000, new float2(1f, 2f));
            Assert.AreEqual(128, simulation.Count, "the count is clamped to the capacity");
            simulation.SetCount(100, new float2(1f, 2f));
            SwarmMotionSettings motion = SwarmMotionSettings.Default;
            simulation.Schedule(0.37f, motion, new float3(1f, 0.5f, 0.25f));
            simulation.Complete();
            for (int i = 0; i < simulation.Count; i++)
            {
                ToonInstance instance = simulation.Instances[i];
                float2 ground = new float2(1f, 2f) + SwarmMotion.SunflowerOffset(i, 0.25f, 1.35f);
                Assert.AreEqual(ground.x, instance.Row0.w, 1e-5f);
                Assert.AreEqual(ground.y, instance.Row2.w, 1e-5f);
                float bounce = instance.Tint.w;
                Assert.That(bounce, Is.InRange(0f, 1f));
                Assert.AreEqual(bounce * motion.HopHeight, instance.Row1.w, 1e-5f);
                Assert.AreEqual(1f, instance.Tint.x, 1e-6f);
                float sy = math.length(new float3(instance.Row0.y, instance.Row1.y, instance.Row2.y));
                Assert.AreEqual(SwarmMotion.SquashY(bounce, motion), sy, 1e-3f);
            }
        }

        [Test]
        public void BoundsHoldEveryUnit()
        {
            using var simulation = new SwarmSimulation(1000, 0.25f, 1.35f, 1);
            simulation.SetCount(1000, float2.zero);
            UnityEngine.Bounds bounds = simulation.GroundBounds(1f);
            for (int i = 0; i < 1000; i++)
            {
                float2 p = SwarmMotion.SunflowerOffset(i, 0.25f, 1.35f);
                Assert.IsTrue(bounds.Contains(new UnityEngine.Vector3(p.x, 0.5f, p.y)), $"unit {i} is outside the bounds");
            }
        }
    }
}
