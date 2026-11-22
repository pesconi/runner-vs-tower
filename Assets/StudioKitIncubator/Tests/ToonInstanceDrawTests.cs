using System.IO;
using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace JungleDegree.StudioKit.Tests
{
    /// <summary>RT-6t.11: the fallback route of the instanced toon draws (blocks of matrices for OpenGL ES devices).</summary>
    public sealed class ToonInstanceDrawTests
    {
        [TearDown]
        public void TearDown()
        {
            ToonInstanceRouting.Forced = null;
        }

        [Test]
        public void TheMatrixIsTheInstanceTransform()
        {
            ToonInstance instance = Sample(3);
            Matrix4x4 matrix = ToonInstanceDraw.Matrix(instance);
            var p = new Vector3(0.3f, -1.2f, 2.5f);
            Vector3 moved = matrix.MultiplyPoint3x4(p);
            var p4 = new float4(p.x, p.y, p.z, 1f);
            Assert.AreEqual(math.dot(instance.Row0, p4), moved.x, 1e-5f);
            Assert.AreEqual(math.dot(instance.Row1, p4), moved.y, 1e-5f);
            Assert.AreEqual(math.dot(instance.Row2, p4), moved.z, 1e-5f);
            Assert.AreEqual(new Vector4(0f, 0f, 0f, 1f), matrix.GetRow(3));
            Assert.AreEqual((Vector4)instance.Row1, matrix.GetRow(1), "row i of the matrix is row i of the instance");
        }

        [Test]
        public void TheChunkJobWritesEveryMatrixAndTintInOnePass()
        {
            const int count = 600;
            var instances = new NativeArray<ToonInstance>(count, Allocator.TempJob);
            var matrices = new NativeArray<float4x4>(count, Allocator.TempJob);
            var tints = new NativeArray<float4>(count, Allocator.TempJob);
            try
            {
                for (int i = 0; i < count; i++)
                {
                    instances[i] = Sample(i);
                }

                new ToonInstanceChunkJob { Instances = instances, Matrices = matrices, Tints = tints }.Schedule(count, 64).Complete();
                for (int i = 0; i < count; i += 37)
                {
                    Assert.AreEqual(ToonInstanceDraw.Matrix(instances[i]), matrices[i]);
                    Assert.AreEqual(instances[i].Tint, tints[i], "tint rgb and the shadow scale in w");
                }
            }
            finally
            {
                instances.Dispose();
                matrices.Dispose();
                tints.Dispose();
            }
        }

        [Test]
        public void TheFallbackTakesOneCallPerBlock()
        {
            Assert.AreEqual(256, ToonInstanceDraw.ChunkSize);
            Assert.AreEqual(0, ToonInstanceDraw.Calls(ToonInstanceRoute.Chunks, 0));
            Assert.AreEqual(1, ToonInstanceDraw.Calls(ToonInstanceRoute.Chunks, 256));
            Assert.AreEqual(2, ToonInstanceDraw.Calls(ToonInstanceRoute.Chunks, 257));
            Assert.AreEqual(4, ToonInstanceDraw.Calls(ToonInstanceRoute.Chunks, 1000));
            Assert.AreEqual(6, ToonInstanceDraw.Calls(ToonInstanceRoute.Chunks, 1512), "1000 live + 512 dying");
            Assert.AreEqual(0, ToonInstanceDraw.Calls(ToonInstanceRoute.Buffer, 0));
            Assert.AreEqual(1, ToonInstanceDraw.Calls(ToonInstanceRoute.Buffer, 1512));
        }

        [Test]
        public void TheShadersInstanceInBlocksOfTheChunkSize()
        {
            foreach (string name in new[] { ToonShaderIds.ToonLitShader, "StudioKit/BlobShadowInstanced" })
            {
                Shader shader = Shader.Find(name);
                Assert.IsNotNull(shader, name);
                Assert.IsFalse(ShaderUtil.ShaderHasError(shader), name + " has compile errors");
                string source = File.ReadAllText(AssetDatabase.GetAssetPath(shader));
                StringAssert.Contains("forcemaxcount:" + ToonInstanceDraw.ChunkSize, source, name);
                StringAssert.Contains("multi_compile _ " + ToonInstanceRouting.BufferKeyword, source, name);
            }
        }

        [Test]
        public void AForcedRouteWinsAndSetsTheGlobalKeyword()
        {
            ToonInstanceRouting.Forced = ToonInstanceRoute.Chunks;
            Assert.AreEqual(ToonInstanceRoute.Chunks, ToonInstanceRouting.Current);
            Assert.IsFalse(Shader.IsKeywordEnabled(ToonInstanceRouting.BufferKeyword));
            ToonInstanceRouting.Forced = ToonInstanceRoute.Buffer;
            Assert.AreEqual(ToonInstanceRoute.Buffer, ToonInstanceRouting.Current);
            Assert.IsTrue(Shader.IsKeywordEnabled(ToonInstanceRouting.BufferKeyword));
            ToonInstanceRouting.Forced = null;
            Assert.AreEqual(ToonInstanceRouting.Detected, ToonInstanceRouting.Current);
            Assert.AreEqual(ToonInstanceRouting.Detected == ToonInstanceRoute.Buffer, Shader.IsKeywordEnabled(ToonInstanceRouting.BufferKeyword));
        }

        [Test]
        public void MaterialsNeedInstancingForTheFallback()
        {
            var material = new Material(Shader.Find(ToonShaderIds.ToonLitShader)) { name = "toon" };
            try
            {
                StringAssert.Contains("instancing", ToonInstanceDraw.ValidateMaterial(material));
                material.enableInstancing = true;
                Assert.IsNull(ToonInstanceDraw.ValidateMaterial(material));
                Assert.IsNotNull(ToonInstanceDraw.ValidateMaterial(null));
            }
            finally
            {
                Object.DestroyImmediate(material);
            }
        }

        // A squashed, leaning, moved unit with a tint and a shadow scale.
        private static ToonInstance Sample(int seed)
        {
            var instance = new ToonInstance { Tint = new float4(0.2f + seed * 0.001f, 0.5f, 0.9f, 0.7f) };
            SwarmAnimation.Compose(ref instance, new float3(seed * 0.1f, 0.3f, -seed * 0.05f), 0.12f, 0.08f, 0.9f, 1.2f);
            return instance;
        }
    }
}
