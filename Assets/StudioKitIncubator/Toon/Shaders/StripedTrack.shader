// Striped track of the studio kit's toon look (URP): two colors in bands across the track, procedural from the world
// Z (no texture): a band of _StripeColor then a band of _TrackColor, each half of _StripePeriod meters. The stripes are
// fixed in the world, so a crowd running forward sees them roll by with no per-frame update. Unlit like the reference
// track (exact palette colors), with linear fog; band edges are filtered by their screen footprint (no shimmer far off).
Shader "StudioKit/StripedTrack"
{
    Properties
    {
        _TrackColor ("Track", Color) = (1, 1, 1, 1)
        _StripeColor ("Stripe", Color) = (0.9, 0.9, 0.9, 1)
        _StripePeriod ("Stripe period (m)", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "StripedTrack"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TrackColor;
                half4 _StripeColor;
                float _StripePeriod;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float bands : TEXCOORD0;
                half fogFactor : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.bands = positionWS.z / max(_StripePeriod, 0.01);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // The stripe covers the first half of each period: distance from its middle, 0 there and 0.5 in the
                // middle of the track band; the edge (0.25) is smoothed over one pixel of band coordinate.
                float fromStripe = abs(frac(input.bands + 0.25) - 0.5);
                float pixel = max(fwidth(input.bands), 1e-5);
                half stripe = (half)saturate((0.25 - fromStripe) / pixel + 0.5);
                half3 color = lerp(_TrackColor.rgb, _StripeColor.rgb, stripe);
                return half4(MixFog(color, input.fogFactor), 1.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
