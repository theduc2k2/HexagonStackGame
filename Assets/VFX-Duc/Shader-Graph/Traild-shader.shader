Shader "Custom/URP/EnergyTrailSphere_LongOuterTrails"
{
    Properties
    {
        [Header(Main)]
        _Tint("Tint", Color) = (0.75, 0.92, 1.0, 1.0)
        _Intensity("Intensity", Range(0, 10)) = 3.0
        _Opacity("Opacity", Range(0, 3)) = 1.0

        [Header(Rim)]
        _RimPower("Rim Power", Range(0.1, 12)) = 4.0
        _RimIntensity("Rim Intensity", Range(0, 6)) = 1.8

        [Header(Volume)]
        _Steps("Raymarch Steps", Range(16, 128)) = 80
        _SphereRadius("Sphere Radius", Range(0.1, 2.0)) = 0.5
        _VolumeDensity("Volume Density", Range(0, 5)) = 1.1

        [Header(Trails)]
        _TrailCount("Trail Count", Range(1, 12)) = 5
        _TrailScale("Trail Scale", Range(0.5, 20)) = 7.5
        _TrailThinness("Trail Thinness", Range(2, 80)) = 52.0
        _TrailBrightness("Trail Brightness", Range(0, 8)) = 4.0
        _TrailLength("Trail Length", Range(0.1, 8.0)) = 2.8
        _TrailCurvature("Trail Curvature", Range(0.1, 8.0)) = 2.2

        [Header(Motion)]
        _FlowSpeed("Flow Speed", Range(0, 5)) = 1.3
        _InwardStrength("Inward Strength", Range(0, 5)) = 1.6
        _SpiralStrength("Spiral Strength", Range(0, 5)) = 2.4
        _NoiseWarp("Noise Warp", Range(0, 3)) = 0.25

        [Header(Outer Shell)]
        _OuterShellStart("Outer Shell Start", Range(0, 1)) = 0.55
        _OuterShellEnd("Outer Shell End", Range(0, 1)) = 1.0
        _OuterTrailBoost("Outer Trail Boost", Range(0, 5)) = 2.0

        [Header(Core)]
        _CoreRadius("Core Radius", Range(0.01, 1.0)) = 0.10
        _CoreGlow("Core Glow", Range(0, 8)) = 2.2
        _CoreBlend("Core Blend", Range(0, 5)) = 1.2
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

            Blend One One
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 viewDirWS  : TEXCOORD2;
                float3 positionOS : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Tint;
                float _Intensity;
                float _Opacity;

                float _RimPower;
                float _RimIntensity;

                float _Steps;
                float _SphereRadius;
                float _VolumeDensity;

                float _TrailCount;
                float _TrailScale;
                float _TrailThinness;
                float _TrailBrightness;
                float _TrailLength;
                float _TrailCurvature;

                float _FlowSpeed;
                float _InwardStrength;
                float _SpiralStrength;
                float _NoiseWarp;

                float _OuterShellStart;
                float _OuterShellEnd;
                float _OuterTrailBoost;

                float _CoreRadius;
                float _CoreGlow;
                float _CoreBlend;
            CBUFFER_END

            float hash31(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            float noise3D(float3 x)
            {
                float3 p = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);

                float n000 = hash31(p + float3(0,0,0));
                float n100 = hash31(p + float3(1,0,0));
                float n010 = hash31(p + float3(0,1,0));
                float n110 = hash31(p + float3(1,1,0));
                float n001 = hash31(p + float3(0,0,1));
                float n101 = hash31(p + float3(1,0,1));
                float n011 = hash31(p + float3(0,1,1));
                float n111 = hash31(p + float3(1,1,1));

                float n00 = lerp(n000, n100, f.x);
                float n10 = lerp(n010, n110, f.x);
                float n01 = lerp(n001, n101, f.x);
                float n11 = lerp(n011, n111, f.x);

                float n0 = lerp(n00, n10, f.y);
                float n1 = lerp(n01, n11, f.y);

                return lerp(n0, n1, f.z);
            }

            float3 rotateY(float3 p, float a)
            {
                float s = sin(a);
                float c = cos(a);
                return float3(c * p.x + s * p.z, p.y, -s * p.x + c * p.z);
            }

            float3 rotateX(float3 p, float a)
            {
                float s = sin(a);
                float c = cos(a);
                return float3(p.x, c * p.y - s * p.z, s * p.y + c * p.z);
            }

            float3 rotateZ(float3 p, float a)
            {
                float s = sin(a);
                float c = cos(a);
                return float3(c * p.x - s * p.y, s * p.x + c * p.y, p.z);
            }

            float trailLayer(float3 p, float seed, float timeVal)
            {
                float dist = length(p);
                float radial01 = saturate(dist / _SphereRadius);
                float inner01 = 1.0 - radial01;

                float3 dirToCenter = normalize(-p + 1e-5);

                // tọa độ cầu
                float azimuth = atan2(p.z, p.x);
                float polar = atan2(length(p.xz), p.y + 1e-5);

                // mask tập trung mạnh ở outer shell
                float outerMask = smoothstep(_OuterShellStart, _OuterShellEnd, radial01);

                // warp nhẹ cho trail đỡ đều quá
                float n = noise3D(p * _TrailScale + float3(seed * 7.13, seed * 11.7, timeVal * 0.35));
                float n2 = noise3D(p * (_TrailScale * 0.6) - float3(seed * 3.1, timeVal * 0.42, seed * 5.6));

                float swirlTime = timeVal * _SpiralStrength * (0.7 + seed * 0.35);
                float inwardTime = timeVal * _InwardStrength * (0.5 + seed * 0.25);

                // quỹ đạo dài uốn lượn quanh cầu
                float waveA =
                    sin(azimuth * (3.0 + seed * 2.0)
                    + polar * _TrailCurvature * (1.2 + seed)
                    - swirlTime
                    + n * 2.2);

                float waveB =
                    sin(azimuth * (5.5 + seed * 1.2)
                    - polar * (_TrailCurvature * 0.85)
                    - swirlTime * 1.15
                    + n2 * 1.8);

                // thành phần hút vào tâm
                float radialBands =
                    sin((radial01 * _TrailLength * 8.0)
                    - inwardTime * 2.0
                    + azimuth * 1.8
                    + polar * 0.9);

                float combined = waveA * 0.9 + waveB * 0.7 + radialBands * 0.55;

                // trail sắc và dài
                float lines = pow(saturate(1.0 - abs(combined) * 0.72), _TrailThinness);

                // ưu tiên hiện trail ở outer shell
                lines *= lerp(0.15, 1.0, outerMask) * (1.0 + outerMask * _OuterTrailBoost);

                // vẫn cho nó tồn tại khi trôi vào trong
                lines *= lerp(1.0, 0.45, inner01 * 0.35);

                // kéo trail theo hướng tâm để không bị đứng im thành texture
                float forwardStreak =
                    0.65 + 0.35 * sin(
                        dot(dirToCenter, p) * 18.0
                        - inwardTime * 3.0
                        + azimuth * 2.4
                        + seed * 8.0
                    );

                return lines * forwardStreak;
            }

            float sampleEnergy(float3 p, float timeVal)
            {
                float dist = length(p);
                float radial01 = saturate(dist / _SphereRadius);
                float insideMask = saturate(1.0 - radial01);

                // uốn cả field để trail nhìn trôi dài hơn
                float3 dirToCenter = normalize(-p + 1e-5);
                float3 tangent = normalize(cross(dirToCenter, float3(0.0, 1.0, 0.0)) + 1e-5);

                p += tangent * sin(timeVal * 0.9 + dist * 8.0) * 0.05 * _SpiralStrength;
                p += dirToCenter * timeVal * 0.08 * _InwardStrength;

                p = rotateY(p, timeVal * 0.18);
                p = rotateX(p, timeVal * 0.11);

                float accum = 0.0;

                [unroll(12)]
                for (int i = 0; i < 12; i++)
                {
                    if (i >= (int)_TrailCount) break;
                    float seed = 0.19 + i * 0.173;
                    accum += trailLayer(p, seed, timeVal);
                }

                accum /= max(_TrailCount, 1.0);

                float coreMask = 1.0 - smoothstep(_CoreRadius, _SphereRadius, dist);
                float coreGlow = pow(coreMask, 2.5) * _CoreGlow;

                return (accum * _TrailBrightness + coreGlow * _CoreBlend) * insideMask;
            }

            bool raySphere(float3 ro, float3 rd, float radius, out float t0, out float t1)
            {
                float b = dot(ro, rd);
                float c = dot(ro, ro) - radius * radius;
                float h = b * b - c;
                if (h < 0.0)
                {
                    t0 = 0;
                    t1 = 0;
                    return false;
                }

                h = sqrt(h);
                t0 = -b - h;
                t1 = -b + h;
                return true;
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
                float3 viewDirWS = normalize(IN.viewDirWS);
                float3 normalWS = normalize(IN.normalWS);

                float3 camPosWS = GetCameraPositionWS();
                float3 roOS = TransformWorldToObject(camPosWS);
                float3 hitOS = IN.positionOS;
                float3 rdOS = normalize(hitOS - roOS);

                float t0, t1;
                if (!raySphere(roOS, rdOS, _SphereRadius, t0, t1))
                    return 0;

                t0 = max(t0, 0.0);
                float segLen = max(0.0001, t1 - t0);
                int steps = (int)_Steps;
                float stepSize = segLen / steps;

                float timeVal = _Time.y * _FlowSpeed;

                float3 accumColor = 0.0;
                float accumAlpha = 0.0;

                [loop]
                for (int i = 0; i < 128; i++)
                {
                    if (i >= steps) break;

                    float3 samplePos = roOS + rdOS * (t0 + stepSize * i);
                    float e = sampleEnergy(samplePos, timeVal) * _VolumeDensity;

                    float dist = length(samplePos);
                    float coreMask = 1.0 - smoothstep(_CoreRadius, _SphereRadius, dist);

                    float3 col = _Tint.rgb * e * (0.85 + coreMask * 0.9);

                    accumColor += col * stepSize;
                    accumAlpha += e * 0.03 * stepSize;
                }

                float ndv = saturate(dot(normalWS, normalize(viewDirWS)));
                float rim = pow(1.0 - ndv, _RimPower) * _RimIntensity;

                float3 finalCol =
                    accumColor * _Intensity * _Opacity +
                    _Tint.rgb * rim * 0.55;

                float finalAlpha = saturate(accumAlpha + rim * 0.15) * _Opacity;

                return half4(finalCol, finalAlpha);
            }
            ENDHLSL
        }
    }
}