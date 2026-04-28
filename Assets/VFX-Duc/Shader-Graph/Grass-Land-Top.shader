Shader "Custom/Grass-Land-Top"
{
    Properties
    {
        [Header(Base Colors)]
        _GrassColorA ("Grass Color A", Color) = (0.42, 0.78, 0.12, 1)
        _GrassColorB ("Grass Color B", Color) = (0.56, 0.90, 0.18, 1)
        _GrassColorC ("Grass Highlight", Color) = (0.75, 1.00, 0.35, 1)

        [Header(Variation)]
        _NoiseScale ("Noise Scale", Range(0.1, 20)) = 4.0
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.35
        _ColorBlendSharpness ("Color Blend Sharpness", Range(0.2, 4)) = 1.5

        [Header(Toon Lighting)]
        _ShadowTint ("Shadow Tint", Color) = (0.22, 0.42, 0.06, 1)
        _LightThreshold ("Light Threshold", Range(0,1)) = 0.5
        _LightSmooth ("Light Smooth", Range(0.001,0.3)) = 0.06
        _AmbientStrength ("Ambient Strength", Range(0,1)) = 0.22

        [Header(Top Surface Style)]
        _TopLightBoost ("Top Light Boost", Range(0.5, 2)) = 1.15
        _CenterHighlight ("Center Highlight", Range(0, 1)) = 0.18
        _EdgeDarken ("Edge Darken", Range(0, 1)) = 0.2

        [Header(Color Punch)]
        _Brightness ("Brightness", Range(0.5, 2)) = 1.05
        _Saturation ("Saturation", Range(0, 3)) = 1.35
        _Contrast ("Contrast", Range(0.5, 2)) = 1.18

        [Header(Optional Texture)]
        _DetailTex ("Detail Tex (optional)", 2D) = "white" {}
        _DetailStrength ("Detail Strength", Range(0,1)) = 0.18
        _DetailTiling ("Detail Tiling", Range(0.5, 20)) = 6.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
            "RenderType"="Opaque"
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                half fogFactor     : TEXCOORD4;
            };

            TEXTURE2D(_DetailTex);
            SAMPLER(sampler_DetailTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _GrassColorA;
                half4 _GrassColorB;
                half4 _GrassColorC;

                half4 _ShadowTint;
                half _NoiseScale;
                half _NoiseStrength;
                half _ColorBlendSharpness;

                half _LightThreshold;
                half _LightSmooth;
                half _AmbientStrength;

                half _TopLightBoost;
                half _CenterHighlight;
                half _EdgeDarken;

                half _Brightness;
                half _Saturation;
                half _Contrast;

                half _DetailStrength;
                half _DetailTiling;
            CBUFFER_END

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float3 ApplySaturation(float3 color, float saturation)
            {
                float luma = dot(color, float3(0.299, 0.587, 0.114));
                return lerp(luma.xxx, color, saturation);
            }

            float3 ApplyContrast(float3 color, float contrast)
            {
                return ((color - 0.5) * contrast) + 0.5;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalize(normalInputs.normalWS);
                output.uv = input.uv;
                output.shadowCoord = TransformWorldToShadowCoord(positionInputs.positionWS);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);

                // Noise theo world-space để map lớn nhìn tự nhiên hơn
                float2 noiseUV = input.positionWS.xz * _NoiseScale * 0.1;
                float n = noise2D(noiseUV);
                n = pow(saturate(n), _ColorBlendSharpness);

                float3 grassBase = lerp(_GrassColorA.rgb, _GrassColorB.rgb, n);

                float fineNoise = noise2D(noiseUV * 2.7 + 11.3);
                grassBase = lerp(grassBase, _GrassColorC.rgb, fineNoise * _NoiseStrength * 0.6);

                // Detail tex optional
                float2 detailUV = input.uv * _DetailTiling;
                float3 detailSample = SAMPLE_TEXTURE2D(_DetailTex, sampler_DetailTex, detailUV).rgb;
                float detailMask = dot(detailSample, float3(0.3333, 0.3333, 0.3333));
                grassBase *= lerp(1.0, detailMask * 1.15, _DetailStrength);

                // Top-facing boost: mặt hướng lên thì sáng đẹp hơn
                float upMask = saturate(normalWS.y);
                grassBase *= lerp(1.0, _TopLightBoost, upMask);

                // Fake center highlight / edge darken theo normal
                float edgeMask = 1.0 - upMask;
                grassBase *= (1.0 - edgeMask * _EdgeDarken);
                grassBase = lerp(grassBase, grassBase * 1.12 + _GrassColorC.rgb * 0.12, _CenterHighlight * upMask);

                // Main light
                Light mainLight = GetMainLight(input.shadowCoord);
                float NdotL = saturate(dot(normalWS, mainLight.direction));

                // Toon light band
                float toonBand = smoothstep(_LightThreshold - _LightSmooth, _LightThreshold + _LightSmooth, NdotL);

                // Shadow attenuation từ URP
                float shadowAtten = mainLight.shadowAttenuation;

                // Ánh sáng tổng hợp toon + shadow
                float litTerm = toonBand * shadowAtten;

                float3 shadowCol = grassBase * _ShadowTint.rgb;
                float3 litCol = grassBase * mainLight.color.rgb;

                // Ambient
                float3 ambient = grassBase * _AmbientStrength;

                float3 finalCol = lerp(shadowCol, litCol, litTerm) + ambient;

                // Thêm chút highlight để kiểu tycoon/cartoon nổi hơn
                float topSpark = saturate(upMask * 0.85 + fineNoise * 0.25);
                finalCol += _GrassColorC.rgb * topSpark * 0.08;

                // Punch màu
                finalCol *= _Brightness;
                finalCol = ApplySaturation(finalCol, _Saturation);
                finalCol = ApplyContrast(finalCol, _Contrast);
                finalCol = saturate(finalCol);

                finalCol = MixFog(finalCol, input.fogFactor);

                return half4(finalCol, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                float3 positionWS = positionInputs.positionWS;
                float3 normalWS = normalize(normalInputs.normalWS);

                float3 lightDirWS = normalize(_LightDirection);
                float3 biasedPositionWS = ApplyShadowBias(positionWS, normalWS, lightDirWS);

                output.positionCS = TransformWorldToHClip(biasedPositionWS);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack Off
}