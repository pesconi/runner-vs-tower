#ifndef STUDIOKIT_TOON_INSTANCE_DATA_INCLUDED
#define STUDIOKIT_TOON_INSTANCE_DATA_INCLUDED

// One instance of the instanced toon draws; mirrors JungleDegree.StudioKit.Toon.ToonInstance (64 bytes).
// row0..row2: object-to-world transform (3x4, translation in w). tint.rgb: linear tint; tint.w: owner data.
struct ToonInstanceData
{
    float4 row0;
    float4 row1;
    float4 row2;
    float4 tint;
};

// Two routes (ToonInstanceRouting, global keyword STUDIOKIT_TOON_BUFFER):
// - buffer: every instance in one structured buffer, read by SV_InstanceID (RenderMeshPrimitives, one call);
// - chunks (fallback for devices without structured buffers in the vertex shader): Unity's instancing arrays, filled
//   by RenderMeshInstanced in blocks of 256 (ToonInstanceDraw.ChunkSize = forcemaxcount): the matrix is the transform,
//   _ToonInstanceTint the tint. Include after Core.hlsl, with the instancing pragmas of the toon shaders.
#if defined(STUDIOKIT_TOON_BUFFER)
StructuredBuffer<ToonInstanceData> _ToonInstances;
#define TOON_SETUP_INSTANCE(input)
#else
UNITY_INSTANCING_BUFFER_START(ToonInstanceProps)
    UNITY_DEFINE_INSTANCED_PROP(float4, _ToonInstanceTint)
UNITY_INSTANCING_BUFFER_END(ToonInstanceProps)
#define TOON_SETUP_INSTANCE(input) UNITY_SETUP_INSTANCE_ID(input)
#endif

// The instance drawn by this vertex, the same data on both routes (TOON_SETUP_INSTANCE first).
ToonInstanceData ToonLoadInstance(uint instanceID)
{
#if defined(STUDIOKIT_TOON_BUFFER)
    return _ToonInstances[instanceID];
#else
    float4x4 m = GetObjectToWorldMatrix();
    ToonInstanceData instance;
    instance.row0 = m[0];
    instance.row1 = m[1];
    instance.row2 = m[2];
    instance.tint = UNITY_ACCESS_INSTANCED_PROP(ToonInstanceProps, _ToonInstanceTint);
    return instance;
#endif
}

float3 ToonInstancePosition(ToonInstanceData instance, float3 positionOS)
{
    float4 p = float4(positionOS, 1.0);
    return float3(dot(instance.row0, p), dot(instance.row1, p), dot(instance.row2, p));
}

// Normal through the inverse transpose of the 3x3 part: for M = R * S, M * (n / s^2) = R * (n / s), and s^2 is
// the squared length of each column of M.
float3 ToonInstanceNormal(ToonInstanceData instance, float3 normalOS)
{
    float3 columnLengthSq = instance.row0.xyz * instance.row0.xyz + instance.row1.xyz * instance.row1.xyz
        + instance.row2.xyz * instance.row2.xyz;
    float3 n = normalOS / max(columnLengthSq, 1e-6);
    return normalize(float3(dot(instance.row0.xyz, n), dot(instance.row1.xyz, n), dot(instance.row2.xyz, n)));
}

#endif
