// RT-6v (direction v2, D24 = A "Brinquedo Brilhante"): the shared light of the glossy toy look.
// One directional light with wrapped diffuse (the shadow side is never black), a two-color ambient (the world's
// sky color from above, a warm ground color from below: globals set by WorldScenery), a tight plus a broad
// specular lobe (glossy plastic) and a sky-tinted rim. No shadow maps, no textures, no extra passes.
#ifndef CLAY_TOY_LIGHTING_INCLUDED
#define CLAY_TOY_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

half4 _ToySkyColor;
half4 _ToyGroundColor;

// gloss: 0 matte .. 1 glossy (material times the vertex alpha); rim: strength of the sky-colored edge light.
half3 ToyShade(half3 albedo, float3 normalWS, float3 viewDirWS, half gloss, half rim, half wrap)
{
    float3 n = normalize(normalWS);
    float3 v = normalize(viewDirWS);
    Light light = GetMainLight();
    half wrapped = saturate((dot(n, light.direction) + wrap) / (1.0h + wrap));
    half3 ambient = lerp(_ToyGroundColor.rgb, _ToySkyColor.rgb, n.y * 0.5h + 0.5h);
    half3 color = albedo * (ambient + light.color * wrapped);

    half nh = saturate(dot(n, normalize(light.direction + v)));
    half specular = (pow(nh, 64.0h) * 0.9h + pow(nh, 8.0h) * 0.12h) * gloss;
    half fresnel = pow(1.0h - saturate(dot(n, v)), 3.0h);
    color += light.color * specular;
    color += _ToySkyColor.rgb * fresnel * rim * (0.4h + 0.6h * saturate(gloss));
    return color;
}

#endif
