// Sky of the studio kit's toon look (URP skybox): a vertical two-color gradient in screen space, top of the screen to
// the bottom, whatever the camera tilt (like a flat background behind the scene). Colors are the globals
// _StudioKitSkyTop / _StudioKitSkyBottom written by ToonPaletteApplier as raw sRGB: the blend happens in sRGB (like a
// canvas gradient) and the result is converted to linear for the linear color space. Not fogged, no texture.
Shader "StudioKit/SkyGradientScreen"
{
    Properties
    {
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off

        Pass
        {
            Name "SkyGradient"

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            half4 _StudioKitSkyTop;
            half4 _StudioKitSkyBottom;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                // Clip-space y and w, divided per pixel (the perspective-correct interpolation keeps y/w exact).
                float2 clipYW : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.clipYW = output.positionCS.yw;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // _ProjectionParams.x is -1 when the projection is flipped (rendering into a texture on a top-left
                // origin API): this undoes it, so 1 is always the top of the screen.
                float up = saturate(input.clipYW.x / input.clipYW.y * _ProjectionParams.x * 0.5 + 0.5);
                half3 color = lerp(_StudioKitSkyBottom.rgb, _StudioKitSkyTop.rgb, up);
            #if !defined(UNITY_COLORSPACE_GAMMA)
                color = SRGBToLinear(color);
            #endif
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
