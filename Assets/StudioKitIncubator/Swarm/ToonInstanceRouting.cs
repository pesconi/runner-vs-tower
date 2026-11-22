using UnityEngine;
using UnityEngine.Rendering;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>How instanced toon units reach the vertex shader.</summary>
    public enum ToonInstanceRoute
    {
        /// <summary>One structured buffer read by <c>SV_InstanceID</c>, one call for any count (<c>RenderMeshPrimitives</c>).</summary>
        Buffer,

        /// <summary>
        /// Fallback for devices that cannot read a structured buffer in the vertex shader (many OpenGL ES GPUs): matrices
        /// and tints in blocks of <see cref="ToonInstanceDraw.ChunkSize"/>, one <c>RenderMeshInstanced</c> call per block.
        /// </summary>
        Chunks,
    }

    /// <summary>
    /// Picks the instanced draw route once for the device (<see cref="SystemInfo"/>) and keeps the global shader keyword
    /// <see cref="BufferKeyword"/> in step with it: on, the instanced variants of <c>StudioKit/ToonLit</c> and
    /// <c>StudioKit/BlobShadowInstanced</c> read the structured buffer; off, they read the per-block matrices and tints
    /// (their own variant, which also compiles for OpenGL ES 3.0). Tests and measurement builds can force a route.
    /// </summary>
    public static class ToonInstanceRouting
    {
        /// <summary>Global keyword of the instanced toon shaders: the structured buffer route.</summary>
        public const string BufferKeyword = "STUDIOKIT_TOON_BUFFER";

        private static bool detected;
        private static ToonInstanceRoute detectedRoute;
        private static ToonInstanceRoute? forced;
        private static ToonInstanceRoute? applied;

        /// <summary>
        /// True when this device can read a structured buffer in the vertex shader (instancing, at least one vertex
        /// buffer input, shader model 4.5 = OpenGL ES 3.1 or better).
        /// </summary>
        public static bool DeviceReadsBuffers => SystemInfo.supportsInstancing && SystemInfo.maxComputeBufferInputsVertex > 0
            && SystemInfo.graphicsShaderLevel >= 45;

        /// <summary>The route this device takes on its own (read once).</summary>
        public static ToonInstanceRoute Detected
        {
            get
            {
                if (!detected)
                {
                    detectedRoute = DeviceReadsBuffers ? ToonInstanceRoute.Buffer : ToonInstanceRoute.Chunks;
                    detected = true;
                }

                return detectedRoute;
            }
        }

        /// <summary>
        /// A route imposed over the detected one (null = none): tests comparing the routes and measurement builds testing
        /// the fallback. Forcing <see cref="ToonInstanceRoute.Buffer"/> on a device that cannot read buffers draws nothing
        /// right. Renderers switch to it on their next frame (that frame allocates the other route).
        /// </summary>
        public static ToonInstanceRoute? Forced
        {
            get => forced;
            set
            {
                forced = value;
                Apply();
            }
        }

        /// <summary>The route in use: <see cref="Forced"/> if set, otherwise <see cref="Detected"/>.</summary>
        public static ToonInstanceRoute Current => forced ?? Detected;

        /// <summary>Sets the global keyword for <see cref="Current"/> (and logs the route when it changes); returns it.</summary>
        public static ToonInstanceRoute Apply()
        {
            ToonInstanceRoute route = Current;
            if (route == ToonInstanceRoute.Buffer)
            {
                Shader.EnableKeyword(BufferKeyword);
            }
            else
            {
                Shader.DisableKeyword(BufferKeyword);
            }

            if (applied != route)
            {
                applied = route;
                Debug.Log($"[ToonRoute] route={route} forced={(forced.HasValue ? "yes" : "no")} api={SystemInfo.graphicsDeviceType} "
                    + $"gpu=\"{SystemInfo.graphicsDeviceName}\" shaderLevel={SystemInfo.graphicsShaderLevel} "
                    + $"vertexBuffers={SystemInfo.maxComputeBufferInputsVertex} instancing={SystemInfo.supportsInstancing}");
            }

            return route;
        }

        /// <summary>True without a graphics device (batchmode -nographics): nothing is drawn.</summary>
        public static bool Headless => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
    }
}
