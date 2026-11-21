// RT-6v: opaque glossy toy surface for the world's props, towers, gates and track (direction v2).
// Color = _BaseColor, times the vertex color when _VertexColor = 1 (merged meshes paint each part in its rgb and
// keep its gloss in the alpha); gloss = _Gloss times that alpha. Lighting in ClayToyLighting.hlsl.
Shader "Claybreak/Toy"
{
    Properties
    {
        _BaseColor("Color", Color) = (1, 1, 1, 1)
        [Toggle] _VertexColor("Use Vertex Color (rgb color, alpha gloss)", Float) = 1
        _Gloss("Gloss", Range(0, 1.5)) = 1
        _Rim("Rim", Range(0, 1)) = 0.6
        _Wrap("Light Wrap", Range(0, 1)) = 0.55
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "ClayToyForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "ClayToyLighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _VertexColor;
                half _Gloss;
                half _Rim;
                half _Wrap;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                half4 color : TEXCOORD2;
                half fogFactor : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                output.color = lerp(half4(1, 1, 1, 1), input.color, _VertexColor);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 albedo = input.color.rgb * _BaseColor.rgb;
                half3 color = ToyShade(albedo, input.normalWS, input.viewDirWS, _Gloss * input.color.a, _Rim, _Wrap);
                return half4(MixFog(color, input.fogFactor), 1.0h);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
