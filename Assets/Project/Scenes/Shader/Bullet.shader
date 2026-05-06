Shader "Custom/URP/EnergyConeProceduralRings"
{
    Properties
    {
        [Header(Color)]
        _BaseColor("Base Color", Color) = (0.05, 0.7, 1.0, 1.0)
        _RingColor("Ring Color", Color) = (0.7, 1.0, 1.0, 1.0)
        _Intensity("Intensity", Range(0, 10)) = 3
        _Opacity("Opacity", Range(0, 5)) = 1.2

        [Header(Fill)]
        _FillAmount("Fill Amount", Range(0, 1)) = 0.7
        _FillSoftness("Fill Softness", Range(0.001, 0.25)) = 0.05
        _ConeMinY("Cone Min Y", Float) = -0.5
        _ConeMaxY("Cone Max Y", Float) = 0.5

        [Header(Rings)]
        _RingCount("Ring Count", Range(1, 80)) = 18
        _RingThickness("Ring Thickness", Range(0.001, 0.2)) = 0.03
        _RingSoftness("Ring Softness", Range(0.001, 0.1)) = 0.01
        _RingSpeed("Ring Speed", Float) = 2.5
        _RingStrength("Ring Strength", Range(0, 5)) = 2.0

        [Header(Ring Motion)]
        _TwistAmount("Twist Amount", Range(0, 20)) = 4.0
        _WaveAmount("Wave Amount", Range(0, 0.3)) = 0.06
        _WaveFreq("Wave Frequency", Range(0, 30)) = 10.0

        [Header(Fresnel)]
        _FresnelPower("Fresnel Power", Range(0.1, 8)) = 2.5
        _FresnelStrength("Fresnel Strength", Range(0, 5)) = 1.2

        [Header(Shape Fade)]
        _TopBoost("Top Boost", Range(0, 3)) = 1.5
        _BottomFade("Bottom Fade", Range(0, 3)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            Blend One One
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RingColor;
                float _Intensity;
                float _Opacity;

                float _FillAmount;
                float _FillSoftness;
                float _ConeMinY;
                float _ConeMaxY;

                float _RingCount;
                float _RingThickness;
                float _RingSoftness;
                float _RingSpeed;
                float _RingStrength;

                float _TwistAmount;
                float _WaveAmount;
                float _WaveFreq;

                float _FresnelPower;
                float _FresnelStrength;

                float _TopBoost;
                float _BottomFade;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 positionOS : TEXCOORD3;
            };

            float SafeInverseLerp(float a, float b, float v)
            {
                return saturate((v - a) / max(0.0001, b - a));
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = normalize(normalInputs.normalWS);
                OUT.viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);
                OUT.positionOS = IN.positionOS.xyz;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(IN.viewDirWS);

                // 0 = đáy, 1 = đỉnh
                float h = SafeInverseLerp(_ConeMinY, _ConeMaxY, IN.positionOS.y);

                // fill từ đỉnh xuống
                float fromTip = 1.0 - h;
                float fillMask = smoothstep(_FillAmount + _FillSoftness, _FillAmount - _FillSoftness, fromTip);

                // góc quanh trục Y
                float angle = atan2(IN.positionOS.x, IN.positionOS.z);
                float u = angle / (2.0 * 3.14159265) + 0.5;

                // tạo twist + wave để ring không đứng im cứng ngắc
                float twistedU = u + h * _TwistAmount + _Time.y * _RingSpeed;
                float wave = sin(twistedU * 6.2831853 + h * _WaveFreq - _Time.y * _RingSpeed * 2.0) * _WaveAmount;

                // trục tạo ring mỏng theo chiều dọc
                float ringCoord = frac((h + wave) * _RingCount - _Time.y * _RingSpeed);

                // ring mỏng ở giữa mỗi cell
                float distToCenter = abs(ringCoord - 0.5);

                float ringMask = 1.0 - smoothstep(
                    _RingThickness,
                    _RingThickness + _RingSoftness,
                    distToCenter
                );

                // tăng/giảm sáng theo góc nhìn
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower) * _FresnelStrength;

                // vùng gần đỉnh sáng hơn
                float topBoost = lerp(1.0 / max(0.001, _BottomFade), _TopBoost, h);

                float baseEnergy = fillMask * topBoost;
                float ringEnergy = ringMask * fillMask * _RingStrength;
                float edgeEnergy = fresnel * fillMask;

                float energy = baseEnergy * 0.35 + ringEnergy + edgeEnergy;

                float3 color = 0;
                color += _BaseColor.rgb * baseEnergy * 0.5;
                color += _RingColor.rgb * ringEnergy;
                color += _RingColor.rgb * edgeEnergy;

                color *= _Intensity;

                float alpha = saturate(energy * _Opacity);

                return half4(color * alpha, alpha);
            }
            ENDHLSL
        }
    }
}