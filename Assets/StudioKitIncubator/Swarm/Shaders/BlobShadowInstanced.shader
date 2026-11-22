// Blob shadows of an instanced toon draw (studio kit): a flat disc under every instance of _ToonInstances,
// drawn as one second call that reads the same buffer as the bodies. The disc keeps the instance ground
// position (x, z), lies at _GroundHeight and is scaled by the owner data in tint.w (the swarm writes the shadow
// scale there: it shrinks as the unit goes up and pops with its birth; 0 = no shadow). Transparent, no depth write.
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
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

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
                ToonInstanceData instance = _ToonInstances[input.instanceID];
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
