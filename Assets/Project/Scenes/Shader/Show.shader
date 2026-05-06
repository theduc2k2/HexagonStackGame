Shader "Custom/URP/EnergyTube"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.35, 0.08, 0.9, 0.15)
        _GlowColor("Glow Color", Color) = (1.0, 0.45, 1.0, 1.0)
        _CoreColor("Core Color", Color) = (0.65, 0.2, 1.0, 1.0)

        _MainTex("Noise Texture", 2D) = "white" {}
        _NoiseStrength("Noise Strength", Range(0, 3)) = 1.0
        _VerticalSpeed("Vertical Speed", Range(-10, 10)) = 1.5
        _DistortAmount("Distort Amount", Range(0, 1)) = 0.08

        _RingCount("Ring Count", Range(1, 10)) = 3
        _RingThickness("Ring Thickness", Range(0.001, 0.2)) = 0.035
        _RingIntensity("Ring Intensity", Range(0, 10)) = 3.0
        _RingScrollSpeed("Ring Scroll Speed", Range(-5, 5)) = 0.8

        _LineIntensity("Vertical Line Intensity", Range(0, 10)) = 2.0
        _LineTiling("Vertical Line Tiling", Range(1, 100)) = 20.0
        _LineSpeed("Vertical Line Speed", Range(-10, 10)) = 2.5

        _FresnelPower("Fresnel Power", Range(0.1, 10)) = 4.0
        _FresnelIntensity("Fresnel Intensity", Range(0, 10)) = 3.0

        _Opacity("Opacity", Range(0, 1)) = 0.75
        _EmissionStrength("Emission Strength", Range(0, 20)) = 5.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha One
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _GlowColor;
                float4 _CoreColor;
                float4 _MainTex_ST;

                float _NoiseStrength;
                float _VerticalSpeed;
                float _DistortAmount;

                float _RingCount;
                float _RingThickness;
                float _RingIntensity;
                float _RingScrollSpeed;

                float _LineIntensity;
                float _LineTiling;
                float _LineSpeed;

                float _FresnelPower;
                float _FresnelIntensity;

                float _Opacity;
                float _EmissionStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float3 viewDirWS   : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = normalize(normalInputs.normalWS);
                OUT.viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                return OUT;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float verticalStreaks(float2 uv, float timeVal)
            {
                float xVal = uv.x * _LineTiling;

                float id = floor(xVal);
                float localX = frac(xVal) - 0.5;

                float rnd = hash21(float2(id, 7.13));
                float width = lerp(0.015, 0.12, rnd);
                float offset = sin(timeVal * (_LineSpeed * (0.5 + rnd)) + id * 3.17) * 0.15;

                float lineMask = smoothstep(width, 0.0, abs(localX + offset));

                float pulse = sin((uv.y + timeVal * _LineSpeed) * 20.0 + id * 4.0) * 0.5 + 0.5;
                pulse = pow(pulse, 3.0);

                return lineMask * pulse;
            }

            float ringMask(float y, float count, float thickness, float scrollTime)
            {
                float v = frac(y * count + scrollTime);
                float distToCenter = abs(v - 0.5);
                return smoothstep(thickness, 0.0, distToCenter);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float timeVal = _Time.y;

                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);

                float fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower);

                float2 noiseUV1 = uv + float2(0.0, timeVal * _VerticalSpeed);
                float2 noiseUV2 = uv * 1.7 + float2(0.13, -timeVal * (_VerticalSpeed * 0.6));

                float noise1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, noiseUV1).r;
                float noise2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, noiseUV2).g;
                float noise = lerp(noise1, noise2, 0.5);

                float distort = (noise - 0.5) * _DistortAmount;

                float2 streakUV = uv;
                streakUV.x += distort;

                float streaks = verticalStreaks(streakUV, timeVal) * _LineIntensity;
                streaks *= lerp(0.5, 1.2, noise) * _NoiseStrength;

                float rings = 0.0;
                rings += ringMask(uv.y,        _RingCount, _RingThickness,        timeVal * _RingScrollSpeed);
                rings += ringMask(uv.y + 0.21, _RingCount, _RingThickness * 0.75, timeVal * (_RingScrollSpeed * 1.15)) * 0.6;
                rings += ringMask(uv.y + 0.43, _RingCount, _RingThickness * 0.55, timeVal * (_RingScrollSpeed * 1.3)) * 0.35;
                rings *= _RingIntensity;

                float plasma = sin(uv.y * 30.0 + timeVal * 4.0 + noise * 6.0) * 0.5 + 0.5;
                plasma *= sin(uv.y * 8.0 - timeVal * 2.0) * 0.5 + 0.5;
                plasma = pow(plasma, 2.0);

                float energy = 0.0;
                energy += streaks;
                energy += rings;
                energy += plasma * 1.3;
                energy += fresnel * _FresnelIntensity;

                float3 baseCol = _BaseColor.rgb;
                float3 glowCol = lerp(_CoreColor.rgb, _GlowColor.rgb, saturate(energy * 0.35));

                float3 finalCol = baseCol + glowCol * energy * _EmissionStrength;
                float alpha = _Opacity * saturate(0.2 + fresnel * 0.8 + plasma * 0.4 + rings * 0.25);

                return half4(finalCol, alpha);
            }
            ENDHLSL
        }
    }
}