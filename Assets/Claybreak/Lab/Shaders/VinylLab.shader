// RT-6v3.2 laboratory shader of the "premium vinyl toy" direction, LOCAL TO THIS GAME (never shipped in the game scene,
// never in the kit). It is a drop-in sibling of StudioKit/ToonLit: the same properties, the same keywords
// (STUDIOKIT_TOON_INSTANCES, STUDIOKIT_TOON_SLOTS, STUDIOKIT_TOON_BUFFER) and the same instance data on both routes
// (ToonInstanceData.hlsl of the kit), so a ToonLit material switched to this shader keeps working unchanged. It exists to
// make the concept screens and to measure the per-pixel cost on the phone before decision D36; the shader that ships
// comes from delivery K.v3a in the kit with the chosen parameters.
// Lighting: a SOFT ramp (smoothstep of N.L with _VinylSoftness, mixed with half-Lambert) instead of the hard 3-band ramp;
// shadows tinted warm (_VinylShadowTint) instead of darkened grey; a soft specular blob (Blinn-Phong pushed through a
// smoothstep, the "vinyl highlight"); a rim light on the edges (_VinylRim, colored); the kit's two-tone ambient.
// Gloss: highlight and rim are scaled by lerp(_VinylSceneGloss, 1, _VinylHero): the hero materials (crowd, leader) get
// them in full, the scenery only as much as the direction says (direction B: matte scenery, glossy character), and never on
// scenery faces that look up (ground and track would glare at grazing angles).
// All _Vinyl* values except _VinylHero are globals written by Claybreak.Lab.VinylDirection.
Shader "Claybreak/VinylLab"
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
        _VinylHero ("Hero gloss (1 = character, 0 = scenery)", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "VinylForward"
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
            #pragma instancing_options assumeuniformscaling nolightprobe nolightmap nolodfade forcemaxcount:256

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
            #include "Packages/com.jungledegree.studiokit/Shared/Shaders/ToonInstanceData.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                half4 _ToonSlot0;
                half4 _ToonSlot1;
                half4 _ToonSlot2;
                half4 _StructureWeights;
                half _ToonEmission;
                half _ToonFogAmount;
                half _VinylHero;
            CBUFFER_END

            // The kit's look and palette globals (ToonLookApplier, ToonPaletteApplier).
            half4 _StudioKitToonSky;
            half4 _StudioKitToonGround;
            half _StudioKitToonAmbient;
            half _StudioKitToonDirect;
            half4 _StudioKitStructureLight;
            half4 _StudioKitStructureDark;
            half4 _StudioKitStructureDetail;
            half4 _StudioKitDefense;

            // The direction's globals (VinylDirection.Apply).
            half _VinylSoftness;      // half width of the light/shadow transition in N.L (0.02 hard .. 0.5 very soft)
            half _VinylLambert;       // share of plain half-Lambert mixed into the ramp (form shading inside the lit side)
            half4 _VinylShadowTint;   // color the albedo takes in full shadow (warm, not grey)
            half _VinylSpec;          // strength of the highlight
            half _VinylSpecSize;      // 0 small and sharp .. 1 broad
            half4 _VinylRimColor;     // rim light color (rgb) and strength (a)
            half _VinylRimWidth;      // exponent of the rim falloff (higher = thinner)
            half _VinylSceneGloss;    // how much highlight and rim the non-hero materials get
            half4 _VinylAmbientTint;  // multiplies the kit's ambient (warm or cool fill)

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
                float3 positionWS : TEXCOORD3;
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
                output.positionWS = positionWS;
                output.albedo = lerp(vertexColor, vertexColor * tint, input.color.a);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 n = normalize(input.normalWS);
                half3 v = normalize(GetWorldSpaceViewDir(input.positionWS));
                Light light = GetMainLight();
                half ndl = dot(n, light.direction);
                // Soft ramp: a smooth step around the terminator, mixed with half-Lambert for form inside the light.
                half band = smoothstep(-_VinylSoftness, _VinylSoftness, ndl);
                half lit = lerp(band, saturate(ndl * 0.5h + 0.5h), _VinylLambert);
                half3 shade = lerp(_VinylShadowTint.rgb, half3(1.0h, 1.0h, 1.0h), lit);
                half3 direct = shade * light.color * _StudioKitToonDirect;
                half3 ambient = lerp(_StudioKitToonGround.rgb, _StudioKitToonSky.rgb, n.y * 0.5h + 0.5h)
                    * _StudioKitToonAmbient * _VinylAmbientTint.rgb;
                half3 color = input.albedo * (direct + ambient);
                // Scenery: no highlight or rim on faces that look up (ground, track, roofs seen at grazing angles glare and
                // wash the frame, as the first concept run showed); sides keep them. The hero keeps them everywhere.
                half upward = saturate(n.y * 1.25h - 0.25h);
                half gloss = lerp(_VinylSceneGloss * (1.0h - upward), 1.0h, _VinylHero);
                // Vinyl highlight: Blinn-Phong pushed through a smooth step, a soft-edged blob rather than a hot pin.
                half3 h = normalize(light.direction + v);
                half ndh = saturate(dot(n, h));
                half power = lerp(96.0h, 12.0h, _VinylSpecSize);
                half spec = smoothstep(0.35h, 0.65h, pow(ndh, power)) * _VinylSpec * gloss * band;
                // Rim: thin bright edge, stronger on the lit side, never on faces turned to the camera.
                half fresnel = pow(1.0h - saturate(dot(n, v)), _VinylRimWidth);
                half rim = fresnel * _VinylRimColor.a * gloss * lerp(0.45h, 1.0h, band);
                color += spec * light.color + rim * _VinylRimColor.rgb;
                color = lerp(color, input.albedo, _ToonEmission);
                return half4(lerp(color, MixFog(color, input.fogFactor), _ToonFogAmount), 1.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
