// Blob shadows of an instanced toon draw (studio kit): a flat disc under every instance, drawn as one second call (or one
// per block on the fallback route) that reads the same instances as the bodies (ToonInstanceData.hlsl, both routes). The
// disc keeps the instance ground position (x, z), lies at _GroundHeight and is scaled by the owner data in tint.w (the
// swarm writes the shadow scale there: it shrinks as the unit goes up and pops with its birth; 0 = no shadow).
// Transparent, no depth write. Needs GPU instancing on (fallback route).
Shader "StudioKit/BlobShadowInstanced"
{
    Properties
    {
        _ShadowColor ("Color (alpha = opacity)", Color) = (0.169, 0.133, 0.2, 0.18)
        _GroundHeight ("Ground height", Float) = 0.01
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-10" "IgnoreProjector" = "True" }

        Pass
        {
            Name "BlobShadow"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma target 4.5 STUDIOKIT_TOON_BUFFER
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ STUDIOKIT_TOON_BUFFER
            #pragma multi_compile_instancing
            // 256 matrices = 16 KB, the smallest OpenGL ES 3 uniform block (see ToonLit and ToonInstanceDraw.ChunkSize).
            #pragma instancing_options assumeuniformscaling nolightprobe nolightmap nolodfade forcemaxcount:256

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../../Toon/Shaders/ToonInstanceData.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShadowColor;
                float _GroundHeight;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half fogFactor : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                TOON_SETUP_INSTANCE(input);
                ToonInstanceData instance = ToonLoadInstance(input.instanceID);
                float scale = max(instance.tint.w, 0.0);
                float3 positionWS = float3(instance.row0.w + input.positionOS.x * scale, _GroundHeight,
                    instance.row2.w + input.positionOS.z * scale);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(MixFog(_ShadowColor.rgb, input.fogFactor), _ShadowColor.a);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
