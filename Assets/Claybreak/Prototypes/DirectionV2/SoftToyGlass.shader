// Direction v2 prototype: candy glass for the gate panels. Tinted, see-through in the middle, brighter at
// grazing angles, with the same glossy highlight as the toys. One transparent pass, no grab texture.
Shader "Claybreak/Prototype/SoftToyGlass"
{
    Properties
    {
        _BaseColor("Color", Color) = (0.3, 0.85, 1, 0.55)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "SoftToyGlass"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "SoftToyLighting.hlsl"

            half4 _BaseColor;

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
                float3 positionWS : TEXCOORD2;
                half fogFactor : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 color = ToyShade(_BaseColor.rgb, input.normalWS, input.viewDirWS, input.positionWS, 1.0h);
                half fresnel = pow(1.0h - saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS))), 2.0h);
                half alpha = saturate(_BaseColor.a + fresnel * 0.35h);
                return half4(MixFog(color + fresnel * 0.25h, input.fogFactor), alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
