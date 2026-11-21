// RT-6v: tinted toy glass for the gate panels (direction v2). See-through in the middle, brighter and more opaque
// at grazing angles, with the toy highlight. One transparent pass, no grab texture.
Shader "Claybreak/ToyGlass"
{
    Properties
    {
        _BaseColor("Color (alpha = opacity facing the camera)", Color) = (0.1, 0.65, 1, 0.66)
        _Gloss("Gloss", Range(0, 1.5)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        Pass
        {
            Name "ClayToyGlass"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "ClayToyLighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Gloss;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 color = ToyShade(_BaseColor.rgb, input.normalWS, input.viewDirWS, _Gloss, 0.6h, 0.55h);
                half fresnel = pow(1.0h - saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS))), 2.0h);
                half alpha = saturate(_BaseColor.a + fresnel * 0.35h);
                return half4(MixFog(color + fresnel * 0.25h, input.fogFactor), alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
