// Toon lighting of the studio kit (URP): the main directional light through a hard ramp (global
// _StudioKitToonRamp, point filtered) plus a two-tone ambient (ground color from below, sky color from above),
// linear fog, vertex colors. Vertex color alpha is the tint mask: 1 = takes the tint (material _TintColor, or the
// instance tint in the instanced variant), 0 = keeps its own color (eyes, leaves...). No shadows, no PBR.
// Keyword STUDIOKIT_TOON_INSTANCES: the transform and tint come from _ToonInstances[SV_InstanceID]
// (Graphics.RenderMeshPrimitives, one draw for any count). Global values come from ToonLookApplier.
Shader "StudioKit/ToonLit"
{
    Properties
    {
        _TintColor ("Tint (where vertex alpha = 1)", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "ToonForward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile_local _ STUDIOKIT_TOON_INSTANCES

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
            #include "ToonInstanceData.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
            CBUFFER_END

            TEXTURE2D(_StudioKitToonRamp);
            SamplerState studiokit_point_clamp_sampler;
            half4 _StudioKitToonSky;
            half4 _StudioKitToonGround;
            half _StudioKitToonAmbient;
            half _StudioKitToonDirect;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                half3 albedo : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS;
                float3 normalWS;
                half3 tint;
            #if defined(STUDIOKIT_TOON_INSTANCES)
                ToonInstanceData instance = _ToonInstances[input.instanceID];
                positionWS = ToonInstancePosition(instance, input.positionOS.xyz);
                normalWS = ToonInstanceNormal(instance, input.normalOS);
                tint = instance.tint.rgb;
            #else
                positionWS = TransformObjectToWorld(input.positionOS.xyz);
                normalWS = TransformObjectToWorldNormal(input.normalOS);
                tint = _TintColor.rgb;
            #endif
                half3 vertexColor = input.color.rgb;
            #if !defined(UNITY_COLORSPACE_GAMMA)
                vertexColor = SRGBToLinear(vertexColor);
            #endif
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = normalWS;
                output.albedo = lerp(vertexColor, vertexColor * tint, input.color.a);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 n = normalize(input.normalWS);
                Light light = GetMainLight();
                half ndl = dot(n, light.direction);
                half ramp = SAMPLE_TEXTURE2D(_StudioKitToonRamp, studiokit_point_clamp_sampler, float2(ndl * 0.5 + 0.5, 0.5)).r;
                half3 direct = ramp * light.color * _StudioKitToonDirect;
                half3 ambient = lerp(_StudioKitToonGround.rgb, _StudioKitToonSky.rgb, n.y * 0.5 + 0.5) * _StudioKitToonAmbient;
                half3 color = input.albedo * (direct + ambient);
                return half4(MixFog(color, input.fogFactor), 1.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
