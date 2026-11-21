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

StructuredBuffer<ToonInstanceData> _ToonInstances;

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
