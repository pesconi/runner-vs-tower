using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace JungleDegree.StudioKit.Toon
{
    /// <summary>
    /// One instance drawn by the instanced variant of <c>StudioKit/ToonLit</c> (keyword
    /// <see cref="ToonShaderIds.InstancesKeyword"/>), read in the vertex shader from the structured buffer
    /// <see cref="ToonShaderIds.Instances"/> by <c>SV_InstanceID</c>. Mirrors <c>ToonInstanceData.hlsl</c>.
    /// Rows 0–2 are the object-to-world transform (3×4, translation in <c>w</c>). <see cref="Tint"/>.rgb is the
    /// tint in linear color (applied where the vertex color alpha is 1); <see cref="Tint"/>.w is free for the
    /// owner (the swarm stores the hop lift there, which its blob shadows read).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ToonInstance
    {
        public const int Stride = 64;

        public float4 Row0;
        public float4 Row1;
        public float4 Row2;
        public float4 Tint;

        public float3 Position => new float3(Row0.w, Row1.w, Row2.w);
    }
}
