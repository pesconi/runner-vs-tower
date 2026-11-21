// Direction v2 prototype: sky dome with a vertical gradient (horizon to zenith). Drawn on a large inverted
// sphere around the camera; the horizon color is also the fog color, so the ground melts into the sky.
// The same shader with _Blob = 1 draws soft round contact shadows on quads (uv distance from the center).
Shader "Claybreak/Prototype/SoftSky"
{
    Properties
    {
        _Horizon("Horizon", Color) = (0.85, 0.95, 1, 1)
        _Zenith("Zenith", Color) = (0.45, 0.75, 1, 1)
        _Blob("Blob shadow mode", Float) = 0
        _BlobColor("Blob color", Color) = (0.2, 0.1, 0.15, 0.35)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-10" }

        Pass
        {
            Name "SoftSky"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            half4 _Horizon;
            half4 _Zenith;
            half _Blob;
            half4 _BlobColor;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 directionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.directionWS = positionWS - GetCameraPositionWS();
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                if (_Blob > 0.5h)
                {
                    float d = length(input.uv - 0.5) * 2.0;
                    half a = (1.0h - smoothstep(0.25h, 1.0h, d)) * _BlobColor.a;
                    return half4(_BlobColor.rgb, a);
                }

                float y = normalize(input.directionWS).y;
                half t = smoothstep(0.0h, 0.45h, y);
                return half4(lerp(_Horizon.rgb, _Zenith.rgb, t), 1.0h);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
