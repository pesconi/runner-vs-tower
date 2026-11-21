// Direction v2 prototype (art review before RT-9): the shared "soft toy" lighting.
// One directional light, wrapped diffuse, a two-color sky/ground ambient, a tight plus a broad specular lobe
// (glossy toy plastic), a sky-tinted fresnel rim and linear fog. No shadow maps, no textures, no extra passes:
// the cost per pixel is a handful of ALU ops over the crowd shader the game already runs.
// The look of each direction is set by globals (DirectionV2Prototype): _ToyGloss (0 matte .. 1 glossy),
// _ToyClay (0 smooth .. 1 hand-pressed clay lumps), _ToyBands (0 soft .. 1 two-tone flat), _ToyRim.
#ifndef SOFT_TOY_LIGHTING_INCLUDED
#define SOFT_TOY_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

half4 _ToySkyColor;
half4 _ToyGroundColor;
half _ToyGloss;
half _ToyClay;
half _ToyBands;
half _ToyRim;

float ToyHash(float3 p)
{
    p = frac(p * 0.3183099 + 0.1);
    p *= 17.0;
    return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
}

float ToyNoise(float3 x)
{
    float3 i = floor(x);
    float3 f = frac(x);
    f = f * f * (3.0 - 2.0 * f);
    return lerp(lerp(lerp(ToyHash(i), ToyHash(i + float3(1, 0, 0)), f.x),
                     lerp(ToyHash(i + float3(0, 1, 0)), ToyHash(i + float3(1, 1, 0)), f.x), f.y),
                lerp(lerp(ToyHash(i + float3(0, 0, 1)), ToyHash(i + float3(1, 0, 1)), f.x),
                     lerp(ToyHash(i + float3(0, 1, 1)), ToyHash(i + float3(1, 1, 1)), f.x), f.y), f.z);
}

// albedo: surface color; gloss: per-vertex gloss mask (vertex alpha); positionWS for the clay lumps.
half3 ToyShade(half3 albedo, float3 normalWS, float3 viewDirWS, float3 positionWS, half gloss)
{
    float3 n = normalize(normalWS);
    if (_ToyClay > 0)
    {
        float3 q = positionWS * 7.0;
        float3 bump = float3(ToyNoise(q), ToyNoise(q + 17.3), ToyNoise(q + 41.7)) - 0.5;
        n = normalize(n + bump * 0.35 * _ToyClay);
    }

    float3 v = normalize(viewDirWS);
    Light light = GetMainLight();
    half ndl = dot(n, light.direction);
    half wrapped = saturate((ndl + 0.55h) / 1.55h);
    half banded = lerp(0.62h, 1.0h, smoothstep(0.42h, 0.5h, wrapped));
    half diffuseTerm = lerp(wrapped, banded, _ToyBands);
    half3 ambient = lerp(_ToyGroundColor.rgb, _ToySkyColor.rgb, n.y * 0.5h + 0.5h);
    half3 color = albedo * (ambient + light.color * diffuseTerm);

    float3 h = normalize(light.direction + v);
    half nh = saturate(dot(n, h));
    half specular = (pow(nh, 64.0h) * 0.9h + pow(nh, 8.0h) * 0.12h) * gloss * _ToyGloss;
    half fresnel = pow(1.0h - saturate(dot(n, v)), 3.0h);
    color += light.color * specular;
    color += _ToySkyColor.rgb * fresnel * _ToyRim * (0.4h + 0.6h * gloss);
    return color;
}

#endif
