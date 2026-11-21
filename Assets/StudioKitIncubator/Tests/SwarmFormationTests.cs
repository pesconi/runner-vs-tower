using System;
using JungleDegree.StudioKit.Swarm;
using NUnit.Framework;
using Unity.Mathematics;

namespace JungleDegree.StudioKit.Tests
{
    /// <summary>
    /// The sunflower formation, its follow and the drag against a literal copy of the reference prototype's frame loop
    /// (docs/reference/crowd-proto.html, frame() and the pointer handlers; its camera looks toward −Z, the kit's run
    /// goes toward +Z, which mirrors only the depth sign and does not touch these formulas).
    /// </summary>
    public sealed class SwarmFormationTests
    {
        private const float Track = 6f;

        [Test]
        public void OffsetsMatchTheReference()
        {
            Span<float2> table = new float2[2000];
            SunflowerFormation.FillOffsets(table, 0.25f, 1.35f);
            for (int i = 0; i < table.Length; i++)
            {
                // const GA=2.39996,SP=.25; r=Math.sqrt(i+.5)*SP; ox=cos(i*GA)*r; oz=sin(i*GA)*r*1.35
                double r = Math.Sqrt(i + 0.5) * 0.25;
                double ox = Math.Cos(i * 2.39996) * r;
                double oz = Math.Sin(i * 2.39996) * r * 1.35;
                Assert.AreEqual(ox, table[i].x, 2e-5, $"unit {i} x");
                Assert.AreEqual(oz, table[i].y, 2e-5, $"unit {i} z");
                Assert.AreEqual(table[i], SunflowerFormation.Offset(i, 0.25f, 1.35f));
            }
        }

        [Test]
        public void PaceFollowFactorAndSlotMatchTheReference()
        {
            for (int i = 0; i < 40; i++)
            {
                Assert.AreEqual(.8f + .4f * ((i * 7) % 10) / 10f, SunflowerFormation.Pace(i, 0.4f), 1e-6f);
            }

            Assert.AreEqual(1f - Math.Exp(-0.016 * 7), SunflowerFormation.FollowFactor(7f, 0.016f), 1e-6);
            Assert.AreEqual(Track - .2f, SunflowerFormation.SlotX(5.9f, 1f, 0f, Track, 0.2f), 1e-6f, "clamped right");
            Assert.AreEqual(-Track + .2f, SunflowerFormation.SlotX(-5.9f, -1f, 0f, Track, 0.2f), 1e-6f, "clamped left");
            Assert.AreEqual(1.5f, SunflowerFormation.SlotX(1f, 0.5f, 0f, Track, 0.2f), 1e-6f, "inside the lane");
            Assert.AreEqual(10f + Track - .2f, SunflowerFormation.SlotX(20f, 0f, 10f, Track, 0.2f), 1e-6f, "lane off the origin");
        }

        [Test]
        public void SimulationFollowsLikeTheReferenceFrameByFrame()
        {
            const int capacity = 600;
            SwarmFormationSettings formation = SwarmFormationSettings.Default;
            using var simulation = new SwarmSimulation(capacity, formation, 3);
            simulation.SetLane(0f, Track);
            var reference = new Reference(capacity);

            simulation.SetCount(10);
            reference.SetCount(10);
            float[] deltas = { 1f / 60f, 1f / 30f, 0.05f, 1f / 120f };
            for (int frame = 0; frame < 240; frame++)
            {
                if (frame == 20)
                {
                    simulation.SteerTo(5.5f);
                    reference.TargetX = 5.5f;
                }

                if (frame == 60)
                {
                    simulation.SetCount(400);
                    reference.SetCount(400);
                }

                if (frame == 100)
                {
                    simulation.SteerTo(-5.5f);
                    reference.TargetX = -5.5f;
                }

                if (frame == 160)
                {
                    simulation.SetCount(600);
                    reference.SetCount(600);
                    simulation.SteerTo(0.75f);
                    reference.TargetX = 0.75f;
                }

                float dt = deltas[frame % deltas.Length];
                simulation.Schedule(dt, frame * dt, SwarmMotionSettings.Default, new float3(1f));
                reference.Frame(dt);
                simulation.Complete();
                Assert.AreEqual(reference.Cx, simulation.CenterX, 1e-4f, $"center, frame {frame}");
                for (int i = 0; i < reference.N; i += 7)
                {
                    float2 p = simulation.GroundPosition(i);
                    Assert.AreEqual(reference.Ux[i], p.x, 1e-3f, $"unit {i} x, frame {frame}");
                    Assert.AreEqual(reference.Uz[i], p.y, 1e-3f, $"unit {i} z, frame {frame}");
                }
            }
        }

        [Test]
        public void UnitsNeverLeaveTheLane()
        {
            using var simulation = new SwarmSimulation(1000, SwarmFormationSettings.Default, 5);
            simulation.SetLane(2f, Track);
            simulation.PlaceCenter(2f);
            simulation.SetCount(1000);
            for (int frame = 0; frame < 120; frame++)
            {
                simulation.SteerTo(frame < 60 ? 100f : -100f);
                simulation.Schedule(0.05f, frame * 0.05f, SwarmMotionSettings.Default, new float3(1f));
                simulation.Complete();
                for (int i = 0; i < 1000; i += 3)
                {
                    float x = simulation.GroundPosition(i).x;
                    Assert.That(x, Is.InRange(2f - Track + 0.2f - 1e-4f, 2f + Track - 0.2f + 1e-4f), $"unit {i}, frame {frame}");
                }
            }
        }

        [Test]
        public void NewUnitsAppearInTheirSlotAtTheCurrentCenter()
        {
            using var simulation = new SwarmSimulation(100, SwarmFormationSettings.Default, 1);
            simulation.SetLane(0f, Track);
            simulation.PlaceCenter(3f);
            simulation.CenterZ = 4f;
            simulation.SetCount(100);
            for (int i = 0; i < 100; i++)
            {
                float2 offset = SunflowerFormation.Offset(i, 0.25f, 1.35f);
                float2 p = simulation.GroundPosition(i);
                Assert.AreEqual(math.clamp(3f + offset.x, -Track + 0.2f, Track - 0.2f), p.x, 1e-6f);
                Assert.AreEqual(4f + offset.y, p.y, 1e-6f);
            }
        }

        [Test]
        public void DragMatchesTheReference()
        {
            SwarmDragSettings drag = SwarmDragSettings.Default;
            float[] moves = { 0f, 40f, -300f, 540f, 1080f, -2000f };
            foreach (float move in moves)
            {
                // targetX=drag.t+(e.clientX-drag.x)/innerWidth*16; clamp to ±(TRACK−.5)
                float expected = Math.Max(-Track + .5f, Math.Min(Track - .5f, 1.25f + move / 1080f * 16f));
                Assert.AreEqual(expected, SwarmSteering.DragTarget(1.25f, 200f, 200f + move, 1080f, drag, 0f, Track), 1e-5f);
            }

            // ArrowLeft/ArrowRight: ±1.5, clamped
            Assert.AreEqual(1.5f, SwarmSteering.KeyTarget(0f, 1, drag, 0f, Track), 1e-6f);
            Assert.AreEqual(Track - .5f, SwarmSteering.KeyTarget(5f, 1, drag, 0f, Track), 1e-6f);
            Assert.AreEqual(-Track + .5f, SwarmSteering.KeyTarget(-5f, -1, drag, 0f, Track), 1e-6f);
            Assert.AreEqual(7f, SwarmSteering.Clamp(7f, drag, 0f, 0f), 1e-6f, "no lane: unchanged");
        }

        [Test]
        public void BoundsHoldEveryUnit()
        {
            using var simulation = new SwarmSimulation(1000, SwarmFormationSettings.Default, 1);
            simulation.CenterZ = 3f;
            simulation.SetCount(1000);
            UnityEngine.Bounds free = simulation.GroundBounds(1f);
            simulation.SetLane(0f, Track);
            UnityEngine.Bounds lane = simulation.GroundBounds(1f);
            for (int i = 0; i < 1000; i++)
            {
                float2 p = SunflowerFormation.Offset(i, 0.25f, 1.35f);
                Assert.IsTrue(free.Contains(new UnityEngine.Vector3(p.x, 0.5f, 3f + p.y)), $"unit {i} is outside the bounds");
                Assert.IsTrue(lane.Contains(new UnityEngine.Vector3(math.clamp(p.x, -Track, Track), 0.5f, 3f + p.y)), $"unit {i} is outside the lane bounds");
            }
        }

        // Literal port of the prototype's live crowd (setCount for growth, the cx follow and the unit loop of frame()).
        private sealed class Reference
        {
            public readonly float[] Ux;
            public readonly float[] Uz;
            private readonly float[] ox;
            private readonly float[] oz;
            public int N;
            public float Cx;
            public float TargetX;

            public Reference(int max)
            {
                Ux = new float[max];
                Uz = new float[max];
                ox = new float[max];
                oz = new float[max];
                // JavaScript numbers are doubles; the arrays (Float32Array) store floats.
                const double GA = 2.39996, SP = .25;
                for (int i = 0; i < max; i++)
                {
                    double r = Math.Sqrt(i + .5) * SP;
                    ox[i] = (float)(Math.Cos(i * GA) * r);
                    oz[i] = (float)(Math.Sin(i * GA) * r * 1.35);
                }
            }

            public void SetCount(int m)
            {
                for (int i = N; i < m; i++)
                {
                    Ux[i] = Math.Max(-Track + .2f, Math.Min(Track - .2f, Cx + ox[i]));
                    Uz[i] = oz[i];
                }

                N = m;
            }

            public void Frame(float dt)
            {
                Cx += (TargetX - Cx) * (1 - (float)Math.Exp(-dt * 8));
                float k = 1 - (float)Math.Exp(-dt * 7);
                for (int i = 0; i < N; i++)
                {
                    float tx = Math.Max(-Track + .2f, Math.Min(Track - .2f, Cx + ox[i]));
                    float nx = Ux[i] + (tx - Ux[i]) * k * (.8f + .4f * ((i * 7) % 10) / 10f);
                    Ux[i] = nx;
                    Uz[i] += (oz[i] - Uz[i]) * k;
                }
            }
        }
    }
}
