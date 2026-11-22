// Toon lighting of the studio kit (URP): the main directional light through a hard ramp (global
// _StudioKitToonRamp, point filtered) plus a two-tone ambient (ground color from below, sky color from above),
// linear fog, vertex colors. Vertex color alpha is the tint mask: 1 = takes the tint (material _TintColor, or the
// instance tint in the instanced variant), 0 = keeps its own color (eyes, leaves...). No shadows, no PBR.
// Keyword STUDIOKIT_TOON_INSTANCES: the transform and tint come from the instance (ToonInstanceData.hlsl): with the
// global keyword STUDIOKIT_TOON_BUFFER from _ToonInstances[SV_InstanceID] (Graphics.RenderMeshPrimitives, one draw for
// any count; shader model 4.5), without it from Unity's instancing arrays (Graphics.RenderMeshInstanced in blocks of 256,
// the fallback for devices without structured buffers in the vertex shader; OpenGL ES 3.0 is enough). Both routes run
// the same math. Materials drawn instanced need GPU instancing on. Global values come from ToonLookApplier.
// Keyword STUDIOKIT_TOON_SLOTS: the vertex color RGB are weights of three palette slots (_ToonSlot0..2, linear)
// instead of a color, so one mesh takes any palette (props: trunk / foliage / foliage light).
// Structure colors: _StructureWeights (x light, y dark, z detail, w defense) mix the palette's global structure and
// defense colors (ToonPaletteApplier) into the tint; what is left up to 1 stays white, so weights 0 = the plain tint and
// one material follows any palette with no per-world asset. _ToonEmission blends the lit color to the unlit albedo
// (glows); _ToonFogAmount scales the fog (1 = full; structures far down the track keep their colors with less).
Shader "StudioKit/ToonLit"
{
    Properties
    {
        _TintColor ("Tint (where vertex alpha = 1)", Color) = (1, 1, 1, 1)
        [HideInInspector] _ToonSlot0 ("Slot 0 (vertex red)", Color) = (1, 1, 1, 1)
        [HideInInspector] _ToonSlot1 ("Slot 1 (vertex green)", Color) = (1, 1, 1, 1)
        [HideInInspector] _ToonSlot2 ("Slot 2 (vertex blue)", Color) = (1, 1, 1, 1)
        _StructureWeights ("Structure weights (light, dark, detail, defense)", Vector) = (0, 0, 0, 0)
        _ToonEmission ("Emission (0 lit, 1 unlit)", Range(0, 1)) = 0
        _ToonFogAmount ("Fog amount", Range(0, 1)) = 1
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
            #pragma target 3.5
            #pragma target 4.5 STUDIOKIT_TOON_BUFFER
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile_local _ STUDIOKIT_TOON_INSTANCES
            #pragma multi_compile_local _ STUDIOKIT_TOON_SLOTS
            #pragma multi_compile _ STUDIOKIT_TOON_BUFFER
            #pragma multi_compile_instancing
            // 256 object-to-world matrices = 16 KB, the smallest uniform block of OpenGL ES 3 (ToonInstanceDraw.ChunkSize):
            // keep the options that leave the other per-instance built-ins out, or lower the count.
            #pragma instancing_options assumeuniformscaling nolightprobe nolightmap nolodfade forcemaxcount:256

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
            #include "ToonInstanceData.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                half4 _ToonSlot0;
                half4 _ToonSlot1;
                half4 _ToonSlot2;
                half4 _StructureWeights;
                half _ToonEmission;
                half _ToonFogAmount;
            CBUFFER_END

            TEXTURE2D(_StudioKitToonRamp);
            SamplerState studiokit_point_clamp_sampler;
            half4 _StudioKitToonSky;
            half4 _StudioKitToonGround;
            half _StudioKitToonAmbient;
            half _StudioKitToonDirect;
            half4 _StudioKitStructureLight;
            half4 _StudioKitStructureDark;
            half4 _StudioKitStructureDetail;
            half4 _StudioKitDefense;

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
                TOON_SETUP_INSTANCE(input);
                ToonInstanceData instance = ToonLoadInstance(input.instanceID);
                positionWS = ToonInstancePosition(instance, input.positionOS.xyz);
                normalWS = ToonInstanceNormal(instance, input.normalOS);
                tint = instance.tint.rgb;
            #else
                positionWS = TransformObjectToWorld(input.positionOS.xyz);
                normalWS = TransformObjectToWorldNormal(input.normalOS);
                tint = _TintColor.rgb;
            #endif
                // A global never written (edit mode before any palette) is all zero: alpha 0 reads it as white, not black.
                half4 w = _StructureWeights;
                tint *= w.x * lerp(1.0h, _StudioKitStructureLight.rgb, _StudioKitStructureLight.a)
                    + w.y * lerp(1.0h, _StudioKitStructureDark.rgb, _StudioKitStructureDark.a)
                    + w.z * lerp(1.0h, _StudioKitStructureDetail.rgb, _StudioKitStructureDetail.a)
                    + w.w * lerp(1.0h, _StudioKitDefense.rgb, _StudioKitDefense.a)
                    + saturate(1.0h - w.x - w.y - w.z - w.w);
            #if defined(STUDIOKIT_TOON_SLOTS)
                half3 vertexColor = input.color.r * _ToonSlot0.rgb + input.color.g * _ToonSlot1.rgb + input.color.b * _ToonSlot2.rgb;
            #else
                half3 vertexColor = input.color.rgb;
            #if !defined(UNITY_COLORSPACE_GAMMA)
                vertexColor = SRGBToLinear(vertexColor);
            #endif
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
                half3 color = lerp(input.albedo * (direct + ambient), input.albedo, _ToonEmission);
                return half4(lerp(color, MixFog(color, input.fogFactor), _ToonFogAmount), 1.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
