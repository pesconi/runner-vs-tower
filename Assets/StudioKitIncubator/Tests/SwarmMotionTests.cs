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
        public void SimulationWritesOneInstancePerUnitWithTheHop()
        {
            using var simulation = new SwarmSimulation(128, SwarmFormationSettings.Default, 7);
            simulation.PlaceCenter(1f);
            simulation.CenterZ = 2f;
            simulation.SetCount(1000);
            Assert.AreEqual(128, simulation.Count, "the count is clamped to the capacity");
            simulation.SetCount(100);
            SwarmMotionSettings motion = SwarmMotionSettings.Default;
            // Zero step: the units stay in their slots (no lane), only the hop is written.
            simulation.Schedule(0f, 0.37f, motion, new float3(1f, 0.5f, 0.25f));
            simulation.Complete();
            for (int i = 0; i < simulation.Count; i++)
            {
                ToonInstance instance = simulation.Instances[i];
                float2 ground = new float2(1f, 2f) + SunflowerFormation.Offset(i, 0.25f, 1.35f);
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
    }
}
