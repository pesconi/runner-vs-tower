using System;
using JungleDegree.StudioKit.Toon;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// Draws up to a fixed number of <see cref="ToonInstance"/>s of one mesh, and optionally their blob shadows, by the
    /// route of the device (<see cref="ToonInstanceRouting"/>):
    /// <list type="bullet">
    /// <item><see cref="ToonInstanceRoute.Buffer"/>: one structured buffer, one call for the bodies and one for the shadows
    /// (<c>RenderMeshPrimitives</c>), both reading the buffer.</item>
    /// <item><see cref="ToonInstanceRoute.Chunks"/>: a Burst job writes the object-to-world matrix and the tint of every
    /// instance in one pass; each block of <see cref="ChunkSize"/> is one <c>RenderMeshInstanced</c> call with its tints
    /// in its own property block, and the shadows are drawn from the same matrices and blocks.</item>
    /// </list>
    /// Both routes run the same shader math (<c>ToonInstanceData.hlsl</c>), so they draw the same picture. Everything is
    /// allocated by the constructor (scene load) and freed by <see cref="Dispose"/>; nothing is allocated per frame.
    /// The materials must have GPU instancing on (<see cref="ValidateMaterial"/>).
    /// </summary>
    public sealed class ToonInstanceDraw : IDisposable
    {
        /// <summary>
        /// Instances per call of the chunks route. Equal to <c>forcemaxcount</c> in the instancing options of the toon
        /// shaders: 256 matrices fill the 16 KB uniform block that every OpenGL ES 3 device has. No margin is left: it fits
        /// only because the instancing options (assumeuniformscaling, nolightprobe, nolightmap, nolodfade) keep the
        /// world-to-object matrix and the other per-instance built-ins out of the block. Adding one means a smaller block.
        /// </summary>
        public const int ChunkSize = 256;

        /// <summary>Per-instance tint of the chunks route (rgb linear tint, w owner data), an instanced shader property.</summary>
        public static readonly int InstanceTint = Shader.PropertyToID("_ToonInstanceTint");

        private const int JobBatch = 64;

        private readonly int capacity;
        private readonly bool shadows;
        private GraphicsBuffer buffer;
        private RenderParams bodyParams;
        private RenderParams shadowParams;
        private NativeArray<float4x4> matrices;
        private NativeArray<float4> tints;
        private Vector4[][] chunkTints;
        private RenderParams[] chunkBodies;
        private RenderParams[] chunkShadows;

        /// <summary>
        /// Allocates the route <see cref="ToonInstanceRouting.Current"/> for <paramref name="capacity"/> instances of
        /// <paramref name="body"/> (<c>StudioKit/ToonLit</c>, instances keyword on) and, if not null,
        /// <paramref name="shadow"/> (<c>StudioKit/BlobShadowInstanced</c>), drawn on <paramref name="layer"/>.
        /// </summary>
        public ToonInstanceDraw(Material body, Material shadow, int capacity, int layer)
        {
            this.capacity = Mathf.Max(1, capacity);
            shadows = shadow != null;
            Route = ToonInstanceRouting.Apply();
            if (Route == ToonInstanceRoute.Buffer)
            {
                buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.capacity, ToonInstance.Stride);
                var block = new MaterialPropertyBlock();
                block.SetBuffer(ToonShaderIds.Instances, buffer);
                bodyParams = Params(body, block, layer);
                shadowParams = shadows ? Params(shadow, block, layer) : default;
                return;
            }

            int chunks = ChunksFor(this.capacity);
            matrices = new NativeArray<float4x4>(this.capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            tints = new NativeArray<float4>(this.capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            chunkTints = new Vector4[chunks][];
            chunkBodies = new RenderParams[chunks];
            chunkShadows = shadows ? new RenderParams[chunks] : null;
            for (int c = 0; c < chunks; c++)
            {
                // The array length is fixed by the first set: always the full block.
                chunkTints[c] = new Vector4[ChunkSize];
                var block = new MaterialPropertyBlock();
                block.SetVectorArray(InstanceTint, chunkTints[c]);
                chunkBodies[c] = Params(body, block, layer);
                if (shadows)
                {
                    chunkShadows[c] = Params(shadow, block, layer);
                }
            }
        }

        /// <summary>The route this draw was allocated for.</summary>
        public ToonInstanceRoute Route { get; }

        public int Capacity => capacity;

        /// <summary>True when this draw no longer matches <see cref="ToonInstanceRouting.Current"/> (a route was forced).</summary>
        public bool Stale => Route != ToonInstanceRouting.Current;

        /// <summary>Calls that drawing <paramref name="count"/> instances takes on <paramref name="route"/> (0 for none).</summary>
        public static int Calls(ToonInstanceRoute route, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            return route == ToonInstanceRoute.Buffer ? 1 : ChunksFor(count);
        }

        /// <summary>Blocks of <see cref="ChunkSize"/> needed for <paramref name="count"/> instances.</summary>
        public static int ChunksFor(int count)
        {
            return (Mathf.Max(0, count) + ChunkSize - 1) / ChunkSize;
        }

        /// <summary>Null when <paramref name="material"/> can be drawn by both routes, otherwise what is wrong.</summary>
        public static string ValidateMaterial(Material material)
        {
            if (material == null)
            {
                return "no material";
            }

            return material.enableInstancing ? null : $"{material.name} must have GPU instancing on (the chunks route needs it)";
        }

        /// <summary>The object-to-world matrix of <paramref name="instance"/> (rows 0–2 of it, then 0 0 0 1).</summary>
        public static float4x4 Matrix(in ToonInstance instance)
        {
            return ToonInstanceChunkJob.Matrix(instance);
        }

        /// <summary>Sets a property shared by every instance (slot colors...) on both bodies and shadows.</summary>
        public void SetVector(int id, Vector4 value)
        {
            if (Route == ToonInstanceRoute.Buffer)
            {
                bodyParams.matProps.SetVector(id, value);
                return;
            }

            foreach (RenderParams chunk in chunkBodies)
            {
                chunk.matProps.SetVector(id, value);
            }
        }

        /// <summary>Uploads the first <paramref name="count"/> instances (completes any job writing them first).</summary>
        public void Upload(NativeArray<ToonInstance> instances, int count)
        {
            count = Mathf.Clamp(count, 0, Mathf.Min(capacity, instances.Length));
            if (count == 0)
            {
                return;
            }

            if (Route == ToonInstanceRoute.Buffer)
            {
                buffer.SetData(instances, 0, 0, count);
                return;
            }

            var job = new ToonInstanceChunkJob { Instances = instances, Matrices = matrices, Tints = tints };
            if (count <= ChunkSize)
            {
                job.Run(count); // a small layer (props): not worth waking the workers
            }
            else
            {
                job.Schedule(count, JobBatch).Complete();
            }
            NativeArray<Vector4> source = tints.Reinterpret<Vector4>();
            for (int c = 0, start = 0; start < count; c++, start += ChunkSize)
            {
                NativeArray<Vector4>.Copy(source, start, chunkTints[c], 0, Mathf.Min(ChunkSize, count - start));
                chunkBodies[c].matProps.SetVectorArray(InstanceTint, chunkTints[c]);
            }
        }

        /// <summary>Draws the first <paramref name="count"/> uploaded instances of <paramref name="mesh"/>.</summary>
        public void Draw(Mesh mesh, int count, Bounds bounds, Camera camera = null)
        {
            count = Mathf.Min(count, capacity);
            if (count <= 0)
            {
                return;
            }

            if (Route == ToonInstanceRoute.Buffer)
            {
                bodyParams.worldBounds = bounds;
                bodyParams.camera = camera;
                Graphics.RenderMeshPrimitives(bodyParams, mesh, 0, count);
                return;
            }

            DrawChunks(chunkBodies, mesh, count, bounds, camera);
        }

        /// <summary>Draws the blob shadows of the first <paramref name="count"/> uploaded instances (nothing without a shadow material).</summary>
        public void DrawShadows(Mesh mesh, int count, Bounds bounds, Camera camera = null)
        {
            count = Mathf.Min(count, capacity);
            if (!shadows || count <= 0)
            {
                return;
            }

            if (Route == ToonInstanceRoute.Buffer)
            {
                shadowParams.worldBounds = bounds;
                shadowParams.camera = camera;
                Graphics.RenderMeshPrimitives(shadowParams, mesh, 0, count);
                return;
            }

            DrawChunks(chunkShadows, mesh, count, bounds, camera);
        }

        public void Dispose()
        {
            buffer?.Release();
            buffer = null;
            if (matrices.IsCreated)
            {
                matrices.Dispose();
                tints.Dispose();
            }

            chunkTints = null;
            chunkBodies = null;
            chunkShadows = null;
        }

        private void DrawChunks(RenderParams[] chunks, Mesh mesh, int count, Bounds bounds, Camera camera)
        {
            NativeArray<Matrix4x4> all = matrices.Reinterpret<Matrix4x4>();
            for (int c = 0, start = 0; start < count; c++, start += ChunkSize)
            {
                RenderParams chunk = chunks[c];
                chunk.worldBounds = bounds;
                chunk.camera = camera;
                int n = Mathf.Min(ChunkSize, count - start);
                Graphics.RenderMeshInstanced(chunk, mesh, 0, all.GetSubArray(start, n), n);
            }
        }

        private static RenderParams Params(Material material, MaterialPropertyBlock block, int layer)
        {
            return new RenderParams(material)
            {
                matProps = block,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
                layer = layer,
            };
        }
    }

    /// <summary>
    /// Chunks route: the matrix and the tint of every instance, written in one pass. The blob shadows are drawn from the same
    /// matrices (ground x, z) and tints (w = shadow scale), so the bodies and their shadows come out of this one job.
    /// </summary>
    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct ToonInstanceChunkJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ToonInstance> Instances;
        [WriteOnly] public NativeArray<float4x4> Matrices;
        [WriteOnly] public NativeArray<float4> Tints;

        public void Execute(int index)
        {
            ToonInstance instance = Instances[index];
            Matrices[index] = Matrix(instance);
            Tints[index] = instance.Tint;
        }

        /// <summary>The object-to-world matrix of <paramref name="instance"/> (rows 0–2 of it, then 0 0 0 1).</summary>
        public static float4x4 Matrix(in ToonInstance instance)
        {
            return new float4x4(
                new float4(instance.Row0.x, instance.Row1.x, instance.Row2.x, 0f),
                new float4(instance.Row0.y, instance.Row1.y, instance.Row2.y, 0f),
                new float4(instance.Row0.z, instance.Row1.z, instance.Row2.z, 0f),
                new float4(instance.Row0.w, instance.Row1.w, instance.Row2.w, 1f));
        }
    }
}
