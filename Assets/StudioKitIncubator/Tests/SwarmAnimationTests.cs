using System;
using System.Collections.Generic;
using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace JungleDegree.StudioKit.Tests
{
    /// <summary>
    /// RT-6t.5: the procedural animations against the reference prototype (docs/reference/crowd-proto.html). The
    /// expected values are the prototype's own expressions, translated literally (three.js: front −Z, Euler XYZ); the
    /// kit's front is +Z, so the lean and the X tumble are mirrored.
    /// </summary>
    public sealed class SwarmAnimationTests
    {
        private const float Track = 6f;

        // --- the prototype, literally ---

        private static float ProtoTilt(float uv) => Math.Max(-.5f, Math.Min(.5f, -uv * .06f));

        private static float ProtoBirth(float T, float born)
        {
            float a = (T - born) / .45f;
            return a <= 0 ? 0 : a >= 1 ? 1 : 1 - (float)Math.Pow(2, -10 * a) * (float)Math.Cos(a * Math.PI * 3.3);
        }

        private static float ProtoShadow(float h, float s) => (1 - h * .35f) * s;

        private sealed class ProtoDying
        {
            public float X, Y, Z, Vx, Vy, Vz, T0, R;
        }

        // dying=dying.filter(d=>T-d.t0<1.1); then for each: vy-=22dt; x+=vx dt; y=max(0,y+vy dt); z+=vz dt; s=max(0,1-(T-t0)/1.1)
        private static void ProtoDyingFrame(List<ProtoDying> dying, float T, float dt)
        {
            dying.RemoveAll(d => !(T - d.T0 < 1.1f));
            foreach (ProtoDying d in dying)
            {
                d.Vy -= 22 * dt;
                d.X += d.Vx * dt;
                d.Y = Math.Max(0, d.Y + d.Vy * dt);
                d.Z += d.Vz * dt;
            }
        }

        // Three's object matrix for position, Euler XYZ rotation and scale, with the front mirrored to +Z (the lean and
        // the X tumble change sign; Unity's Euler with no Y turn is the same Rx·Rz product).
        private static Matrix4x4 ProtoMatrix(Vector3 position, float rotationX, float rotationZ, Vector3 scale)
        {
            return Matrix4x4.TRS(position, Quaternion.Euler(-rotationX * Mathf.Rad2Deg, 0f, rotationZ * Mathf.Rad2Deg), scale);
        }

        private static void AssertInstance(Matrix4x4 expected, ToonInstance actual, float tolerance, string what)
        {
            float4[] rows = { actual.Row0, actual.Row1, actual.Row2 };
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    Assert.AreEqual(expected[r, c], rows[r][c], tolerance, $"{what}: m{r}{c}");
                }
            }
        }

        // --- formulas ---

        [Test]
        public void TiltFollowsTheReference()
        {
            SwarmAnimationSettings settings = SwarmAnimationSettings.Default;
            foreach (float uv in new[] { -40f, -8.4f, -1f, 0f, 0.5f, 3f, 8.3f, 25f })
            {
                Assert.AreEqual(ProtoTilt(uv), SwarmAnimation.Tilt(uv, settings), 1e-6f, $"speed {uv}");
            }

            Assert.Less(SwarmAnimation.Tilt(4f, settings), 0f, "moving right tips the top to the right (negative Z turn)");
        }

        [Test]
        public void BirthPopsElasticallyLikeTheReference()
        {
            SwarmAnimationSettings settings = SwarmAnimationSettings.Default;
            float maxScale = 0f;
            for (int k = -5; k <= 60; k++)
            {
                float t = 10f + k * 0.01f;
                float expected = ProtoBirth(t, 10f);
                float actual = SwarmAnimation.BirthScale(t, 10f, settings.BirthDuration);
                Assert.AreEqual(expected, actual, 1e-5f, $"age {k * 0.01f:0.00}");
                maxScale = Math.Max(maxScale, actual);
            }

            Assert.AreEqual(0f, SwarmAnimation.BirthScale(9.9f, 10f, 0.45f), "not started yet");
            Assert.AreEqual(1f, SwarmAnimation.BirthScale(10.5f, 10f, 0.45f), "settled");
            Assert.Greater(maxScale, 1.05f, "the pop overshoots (elastic)");
        }

        [Test]
        public void ShadowShrinksWithTheHopAndPopsWithTheBirth()
        {
            SwarmAnimationSettings settings = SwarmAnimationSettings.Default;
            foreach (float h in new[] { 0f, 0.3f, 1f })
            {
                foreach (float s in new[] { 0f, 0.5f, 1f, 1.2f })
                {
                    Assert.AreEqual(ProtoShadow(h, s), SwarmAnimation.ShadowScale(h, s, settings), 1e-6f, $"h {h}, s {s}");
                }
            }

            Assert.AreEqual(0.65f, SwarmAnimation.ShadowScale(1f, 1f, settings), 1e-6f, "top of the hop: 1 − 0.35");
        }

        [Test]
        public void ComposeIsTheReferenceTransform()
        {
            var position = new float3(1.5f, 0.13f, -2.25f);
            foreach (float tilt in new[] { -0.5f, 0f, 0.31f })
            {
                var instance = new ToonInstance();
                SwarmAnimation.Compose(ref instance, position, 0.12f, tilt, 0.94f, 1.12f);
                // Prototype: rotation.set(-.12, 0, tilt) with the front at −Z → +0.12 about X here.
                Matrix4x4 expected = ProtoMatrix(position, -0.12f, tilt, new Vector3(0.94f, 1.12f, 0.94f));
                AssertInstance(expected, instance, 1e-5f, $"tilt {tilt}");
            }
        }

        [Test]
        public void DeathLaunchIsTheReferenceThrow()
        {
            SwarmDeathSettings death = SwarmDeathSettings.Default;
            var offset = new float2(-0.4f, 0.9f);
            var random = new Unity.Mathematics.Random(11);
            var draws = new Unity.Mathematics.Random(11);
            SwarmDyingUnit unit = SwarmDying.Launch(new float2(2f, -1f), offset, 3.5f, new float3(1f, 0f, 0f), death, ref random);
            // a=atan2(oz,ox); vx=cos(a)*3+(rnd-.5)*3; vy=4+rnd*4; vz=sin(a)*2+(rnd-.5)*2; r=rnd*6 (in that order).
            double a = Math.Atan2(offset.y, offset.x);
            float vx = (float)(Math.Cos(a) * 3 + (draws.NextFloat() - .5) * 3);
            float vy = 4 + draws.NextFloat() * 4;
            float vz = (float)(Math.Sin(a) * 2 + (draws.NextFloat() - .5) * 2);
            float r = draws.NextFloat() * 6;
            Assert.AreEqual(vx, unit.Velocity.x, 1e-5f);
            Assert.AreEqual(vy, unit.Velocity.y, 1e-5f);
            Assert.AreEqual(vz, unit.Velocity.z, 1e-5f);
            Assert.AreEqual(r, unit.Spin, 1e-5f);
            Assert.AreEqual(new float3(2f, 0f, -1f), unit.Position, "from where it stood, on the ground");
            Assert.AreEqual(3.5f, unit.DiedAt);
            Assert.That(unit.Velocity.y, Is.InRange(4f, 8f));
        }

        [Test]
        public void DeathFlightAndShrinkFollowTheReferenceFrameByFrame()
        {
            SwarmDeathSettings death = SwarmDeathSettings.Default;
            var unit = new SwarmDyingUnit { Position = new float3(0.5f, 0f, 1f), Velocity = new float3(2.5f, 7f, -1.2f), DiedAt = 0f, Spin = 4f };
            var proto = new List<ProtoDying> { new ProtoDying { X = 0.5f, Y = 0f, Z = 1f, Vx = 2.5f, Vy = 7f, Vz = -1.2f, T0 = 0f, R = 4f } };
            float T = 0f;
            float[] deltas = { 1f / 60f, 1f / 30f, 0.05f, 1f / 120f };
            for (int frame = 0; frame < 80; frame++)
            {
                float dt = deltas[frame % deltas.Length];
                T += dt;
                ProtoDyingFrame(proto, T, dt);
                bool alive = SwarmDying.Alive(T, unit.DiedAt, death.Lifetime);
                Assert.AreEqual(proto.Count == 1, alive, $"alive, frame {frame}");
                if (!alive)
                {
                    break;
                }

                SwarmDying.Step(ref unit, dt, death.Gravity);
                ProtoDying d = proto[0];
                Assert.AreEqual(d.X, unit.Position.x, 1e-4f, $"x, frame {frame}");
                Assert.AreEqual(d.Y, unit.Position.y, 1e-4f, $"y, frame {frame}");
                Assert.AreEqual(d.Z, unit.Position.z, 1e-4f, $"z, frame {frame}");
                Assert.AreEqual(Math.Max(0, 1 - (T - d.T0) / 1.1f), SwarmDying.Scale(T, unit.DiedAt, death.Lifetime), 1e-5f);
                Assert.GreaterOrEqual(unit.Position.y, 0f, "never below the ground");
            }

            Assert.AreEqual(0, proto.Count, "gone within the lifetime");
        }

        [Test]
        public void DyingTransformTumblesAndShrinksLikeTheReference()
        {
            // dummy.position.set(x,y,z); dummy.rotation.set(r*T,0,r*T*.7); dummy.scale.setScalar(s)
            float T = 2.3f;
            float r = 3.7f;
            float s = SwarmDying.Scale(T, 1.9f, 1.1f);
            float2 tumble = SwarmDying.Tumble(T, r);
            var instance = new ToonInstance();
            var position = new float3(-1f, 0.6f, 4f);
            SwarmAnimation.Compose(ref instance, position, tumble.x, tumble.y, s, s);
            AssertInstance(ProtoMatrix(position, r * T, r * T * .7f, Vector3.one * s), instance, 1e-4f, "dying");
        }

        // --- the simulation ---

        [Test]
        public void RemovedUnitsAreTheOuterRingAndDieInSlotOrder()
        {
            using var simulation = new SwarmSimulation(200, 512, SwarmFormationSettings.Default, 9);
            simulation.SetLane(0f, Track);
            simulation.SetCount(100, 0f, SwarmRemoval.Vanish);
            var before = new float2[100];
            for (int i = 0; i < 100; i++)
            {
                before[i] = simulation.GroundPosition(i);
            }

            simulation.SetCount(70, 0.5f, SwarmRemoval.Die);
            Assert.AreEqual(70, simulation.Count);
            Assert.AreEqual(30, simulation.DyingCount);
            for (int j = 0; j < 30; j++)
            {
                SwarmDyingUnit unit = simulation.DyingUnit(j);
                Assert.AreEqual(before[70 + j].x, unit.Position.x, 1e-6f, $"dying {j} is unit {70 + j}");
                Assert.AreEqual(before[70 + j].y, unit.Position.z, 1e-6f);
                Assert.AreEqual(0.5f, unit.DiedAt);
            }

            // Outer ring: every removed slot is at least as far out as every kept one.
            float keptRadius = SunflowerFormation.Radius(70, 0.25f);
            for (int i = 70; i < 100; i++)
            {
                Assert.GreaterOrEqual(math.length(SunflowerFormation.Offset(i, 0.25f, 1f)), keptRadius - 1e-4f, $"unit {i}");
            }

            simulation.SetCount(60, 0.6f, SwarmRemoval.Vanish);
            Assert.AreEqual(30, simulation.DyingCount, "vanishing units do not die");
        }

        [Test]
        public void TheRingKeepsTheNewestWhenFull()
        {
            using var simulation = new SwarmSimulation(1200, 512, SwarmFormationSettings.Default, 4);
            simulation.SetCount(1100, 0f, SwarmRemoval.Vanish);
            var before = new float2[1100];
            for (int i = 0; i < 1100; i++)
            {
                before[i] = simulation.GroundPosition(i);
            }

            simulation.SetCount(500, 1f, SwarmRemoval.Die);
            Assert.AreEqual(512, simulation.DyingCount, "fixed capacity");
            // Prototype: dying.splice(0, length − cap) drops the first thrown, so the last 512 removed stay.
            Assert.AreEqual(before[1100 - 512].x, simulation.DyingUnit(0).Position.x, 1e-6f);
            Assert.AreEqual(before[1099].x, simulation.DyingUnit(511).Position.x, 1e-6f);

            using var none = new SwarmSimulation(100, SwarmFormationSettings.Default, 4);
            none.SetCount(100, 0f, SwarmRemoval.Vanish);
            none.SetCount(10, 0f, SwarmRemoval.Die);
            Assert.AreEqual(0, none.DyingCount, "no ring: removals vanish");
        }

        [Test]
        public void SimulationFliesTheDyingLikeTheReferenceAndDrawsThemAfterTheLive()
        {
            using var simulation = new SwarmSimulation(300, 512, SwarmFormationSettings.Default, 21);
            simulation.SetLane(0f, Track);
            simulation.CenterZ = 5f;
            simulation.SetCount(300, 0f, SwarmRemoval.Vanish);
            simulation.SetCount(150, 0f, SwarmRemoval.Die);
            var proto = new List<ProtoDying>();
            for (int j = 0; j < simulation.DyingCount; j++)
            {
                SwarmDyingUnit u = simulation.DyingUnit(j);
                proto.Add(new ProtoDying { X = u.Position.x, Y = u.Position.y, Z = u.Position.z, Vx = u.Velocity.x, Vy = u.Velocity.y, Vz = u.Velocity.z, T0 = u.DiedAt, R = u.Spin });
            }

            float T = 0f;
            const float dt = 1f / 60f;
            for (int frame = 0; frame < 75; frame++)
            {
                T += dt;
                ProtoDyingFrame(proto, T, dt);
                simulation.Schedule(dt, T, SwarmMotionSettings.Default, new float3(1f));
                simulation.Complete();
                Assert.AreEqual(proto.Count, simulation.DyingCount, $"dying, frame {frame}");
                Assert.AreEqual(150 + proto.Count, simulation.DrawCount);
                for (int j = 0; j < proto.Count; j += 13)
                {
                    ProtoDying d = proto[j];
                    float s = Math.Max(0, 1 - (T - d.T0) / 1.1f);
                    var position = new Vector3(d.X, d.Y, 5f + d.Z);
                    ToonInstance instance = simulation.Instances[150 + j];
                    AssertInstance(ProtoMatrix(position, d.R * T, d.R * T * .7f, Vector3.one * s), instance, 2e-3f, $"dying {j}, frame {frame}");
                    Assert.AreEqual(0f, instance.Tint.w, "no shadow for the dying");
                    var inside = new Vector3(position.x, Mathf.Max(position.y, 0.01f), position.z);
                    Assert.IsTrue(simulation.GroundBounds(1.5f).Contains(inside), $"bounds hold dying {j}, frame {frame}");
                }
            }

            Assert.AreEqual(0, simulation.DyingCount, "all gone after the lifetime");
            simulation.SetCount(100, T, SwarmRemoval.Die);
            simulation.ClearDying();
            Assert.AreEqual(0, simulation.DyingCount, "a restart clears the ring");
        }

        [Test]
        public void LiveUnitsPopLeanAndCastAShrinkingShadowLikeTheReference()
        {
            SwarmMotionSettings motion = SwarmMotionSettings.Default;
            SwarmAnimationSettings animation = SwarmAnimationSettings.Default;
            using var simulation = new SwarmSimulation(400, 0, SwarmFormationSettings.Default, 8);
            simulation.SetLane(0f, Track);
            simulation.SetCount(400, 0f, SwarmRemoval.Vanish);
            // Frame at the same time: every unit is still before (or at the start of) its pop: nothing drawn, no shadow.
            simulation.Schedule(0f, 0f, motion, new float3(1f));
            simulation.Complete();
            for (int i = 0; i < 400; i += 11)
            {
                Assert.AreEqual(0f, math.length(simulation.Instances[i].Row0.xyz), 1e-6f, $"unit {i} not popped yet");
                Assert.AreEqual(0f, simulation.Instances[i].Tint.w, 1e-6f);
            }

            float T = 0f;
            const float dt = 1f / 60f;
            float maxBirth = 0f;
            simulation.SteerTo(4f);
            for (int frame = 0; frame < 50; frame++)
            {
                var before = new float2[400];
                for (int i = 0; i < 400; i++)
                {
                    before[i] = simulation.GroundPosition(i);
                }

                T += dt;
                simulation.Schedule(dt, T, motion, new float3(1f));
                simulation.Complete();
                for (int i = 0; i < 400; i += 17)
                {
                    float2 after = simulation.GroundPosition(i);
                    float uv = (after.x - before[i].x) / dt;
                    ToonInstance instance = simulation.Instances[i];
                    float h = instance.Row1.w / motion.HopHeight;
                    float sy = .8f + .32f * h;
                    float sx = 1 / (float)Math.Sqrt(sy);
                    // Each unit pops after its own random delay: its birth scale is read back from the drawn squash,
                    // and the rest of the transform must then be the reference one.
                    float birth = math.length(instance.Row0.xyz) > 0f
                        ? math.length(new float3(instance.Row0.y, instance.Row1.y, instance.Row2.y)) / sy
                        : 0f;
                    maxBirth = Math.Max(maxBirth, birth);
                    float born = simulation.BirthTime(i);
                    Assert.That(born, Is.InRange(0f, animation.BirthDelayMax), $"unit {i} pop delay");
                    Assert.AreEqual(ProtoBirth(T, born), birth, 2e-3f, $"unit {i} pop, frame {frame}");
                    var expected = ProtoMatrix(new Vector3(after.x, h * .2f, after.y), -.12f, ProtoTilt(uv),
                        new Vector3(sx * birth, sy * birth, sx * birth));
                    AssertInstance(expected, instance, 2e-3f, $"unit {i}, frame {frame}");
                    Assert.AreEqual(ProtoShadow(h, birth), instance.Tint.w, 2e-3f, $"shadow {i}, frame {frame}");
                    if (T >= animation.BirthDelayMax + animation.BirthDuration)
                    {
                        Assert.AreEqual(1f, birth, 2e-3f, $"unit {i} settled, frame {frame}");
                    }
                }
            }

            Assert.Greater(maxBirth, 1.05f, "the pop overshoots in the simulation too");
        }

        [Test]
        public void BoundsHoldUnitsThrownFarFromTheShrunkFormationAndResetDoesNotPop()
        {
            using var simulation = new SwarmSimulation(1000, 512, SwarmFormationSettings.Default, 13);
            simulation.SetCount(1000, 0f, SwarmRemoval.Vanish);
            simulation.SetCount(10, 0f, SwarmRemoval.Die);
            float T = 0f;
            for (int frame = 0; frame < 60; frame++)
            {
                T += 1f / 60f;
                simulation.Schedule(1f / 60f, T, SwarmMotionSettings.Default, new float3(1f));
                simulation.Complete();
                Bounds bounds = simulation.GroundBounds(1.5f);
                for (int j = 0; j < simulation.DyingCount; j += 7)
                {
                    float3 p = simulation.DyingUnit(j).Position;
                    var inside = new Vector3(p.x, Mathf.Max(p.y, 0.01f), simulation.CenterZ + p.z);
                    Assert.IsTrue(bounds.Contains(inside), $"dying {j}, frame {frame}");
                }
            }

            simulation.Reset(200);
            Assert.AreEqual(200, simulation.Count);
            Assert.AreEqual(0, simulation.DyingCount, "a reset kills nobody");
            simulation.Schedule(0f, T, SwarmMotionSettings.Default, new float3(1f));
            simulation.Complete();
            Assert.AreEqual(200, simulation.DrawCount);
            for (int i = 0; i < 200; i += 9)
            {
                ToonInstance instance = simulation.Instances[i];
                float sy = math.length(new float3(instance.Row0.y, instance.Row1.y, instance.Row2.y));
                float h = instance.Row1.w / SwarmMotionSettings.Default.HopHeight;
                Assert.AreEqual(SwarmMotion.SquashY(h, SwarmMotionSettings.Default), sy, 1e-3f, $"unit {i} full size at once");
            }

            simulation.SetCount(100, T, SwarmRemoval.Die);
            Assert.AreEqual(200, simulation.DrawCount, "the draw keeps the last written frame until the next one");
        }

        // RT-6t.7: a loose unit (not in a formation) is thrown along a direction with its own size; the formation launch
        // is the same throw at size 1 (same draws in the same order).
        [Test]
        public void LooseLaunchThrowsAlongTheDirectionAtItsSize()
        {
            SwarmDeathSettings death = SwarmDeathSettings.Default;
            var a = new Unity.Mathematics.Random(5);
            var b = new Unity.Mathematics.Random(5);
            SwarmDyingUnit formation = SwarmDying.Launch(new float2(1f, 30f), new float2(0f, -2f), 2f, new float3(0.5f), death, ref a);
            SwarmDyingUnit loose = SwarmDying.Launch(new float2(1f, 30f), new float2(0f, -0.1f), 2f, new float3(0.5f), 1.45f, death, ref b);
            Assert.AreEqual(1f, formation.Size, "a formation unit dies at the mesh size");
            Assert.AreEqual(1.45f, loose.Size);
            Assert.AreEqual(formation.Velocity.x, loose.Velocity.x, 1e-5f, "only the direction of the outward vector counts");
            Assert.AreEqual(formation.Velocity.z, loose.Velocity.z, 1e-5f);
            Assert.Less(loose.Velocity.z, -death.OutwardSpeedZ + 0.5f * death.JitterZ + 1e-4f, "thrown backward");
        }

        // The ring alone: oldest first, the oldest makes room when full, the expired leave from the head, the flight
        // writes one instance per unit with the size applied, and bounds grow by the reach.
        [Test]
        public void DyingRingKeepsTheNewestAndShrinksFromTheSize()
        {
            SwarmDeathSettings death = SwarmDeathSettings.Default;
            var random = new Unity.Mathematics.Random(3);
            using var ring = new SwarmDyingRing(4);
            using var instances = new Unity.Collections.NativeArray<ToonInstance>(4, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < 6; i++)
            {
                ring.Push(SwarmDying.Launch(new float2(i, 0f), new float2(1f, 0f), i * 0.1f, new float3(1f), 2f, death, ref random));
            }

            Assert.AreEqual(4, ring.Count);
            Assert.AreEqual(2f, ring[0].Position.x, "the two oldest made room");
            Assert.AreEqual(5f, ring[3].Position.x);

            ring.ScheduleFly(instances, 0f, 0.5f, 0f, death, default).Complete();
            float expected = SwarmDying.Scale(0.5f, ring[0].DiedAt, death.Lifetime) * 2f;
            float drawn = math.length(new float3(instances[0].Row0.x, instances[0].Row1.x, instances[0].Row2.x));
            Assert.AreEqual(expected, drawn, 1e-4f, "the dying unit shrinks from its own size");

            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            ring.Encapsulate(ref bounds, 0f, 1f, death);
            Assert.Greater(bounds.max.x, 5f, "the bounds hold where the units can fly");

            ring.DropExpired(0.3f + death.Lifetime + 1e-3f, death.Lifetime);
            Assert.AreEqual(2, ring.Count, "the units dead longer than the lifetime left");
            ring.Clear();
            Assert.AreEqual(0, ring.Count);

            using var none = new SwarmDyingRing(0);
            none.Push(new SwarmDyingUnit { Size = 1f });
            Assert.AreEqual(0, none.Count, "a ring with no room keeps nothing");
        }
    }
}
