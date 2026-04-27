Shader "Custom/Blender_Color"
{
    Properties
    {
        // Màu nền chính của thân xe (được set từ material.color trong C#).
        [MainColor] _Color ("Base Color", Color) = (0.24, 0.58, 1.0, 1.0)
        // Màu dự phòng để tương thích với material cũ dùng _MainColor.
        [HideInInspector] _MainColor ("Legacy Base Color", Color) = (0.24, 0.58, 1.0, 1.0)

        [Header(Color Tuning)]
        // Hệ số tăng/giảm độ sáng tổng của màu thân xe.
        _ValueBoost ("Value Boost", Range(0.7, 1.6)) = 1.12
        // Độ đậm màu (cao hơn = rực hơn, saturation cao hơn).
        _Saturation ("Saturation", Range(0.6, 2.2)) = 1.12
        // Độ tương phản tổng thể quanh vùng xám trung gian.
        _Contrast ("Contrast", Range(0.7, 1.8)) = 1.06

        [Header(Toon Blocks)]
        // Số bậc sáng/tối theo kiểu toon.
        _ToonSteps ("Toon Steps", Range(2, 12)) = 3
        // Mức sáng tối thiểu để bóng không bị đen quá.
        _MinLight ("Min Light", Range(0.2, 0.9)) = 0.68
        // Độ mượt kiểu Standard, ảnh hưởng độ sắc và độ rộng highlight.
        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.35
        // Tăng tương phản giữa các mặt khác nhau của khối.
        _FaceContrast ("Face Contrast", Range(0.0, 1.5)) = 0.95
        // Nén vùng mid-tone để tách rõ vùng sáng và tối.
        _MidBandStrength ("Mid Band Strength", Range(0.0, 1.0)) = 0.45
        // Độ đậm tại vùng chuyển mặt/góc cạnh của khối.
        _BlockEdgeContrast ("Block Edge Contrast", Range(0.0, 1.5)) = 0.65
        // Độ rộng vùng chuyển tiếp để nhấn cạnh khối.
        _BlockEdgeWidth ("Block Edge Width", Range(0.02, 0.5)) = 0.18
        // Tăng sáng nhẹ cho mặt hướng lên trên để rõ khối.
        _TopBoost ("Top Boost", Range(0.0, 3.0)) = 0.10
        // Làm tối mặt bên để tách hình rõ hơn.
        _SideDarken ("Side Darken", Range(0.0, 0.7)) = 0.12

        [Header(Shape Clarity)]
        // Độ đậm viền silhouette và góc.
        _EdgeDarken ("Edge Darken", Range(0.0, 0.8)) = 0.12
        // Độ gắt của viền tối (cao hơn = viền sát và sắc hơn).
        _EdgePower ("Edge Power", Range(0.6, 8.0)) = 2.6
        // Cường độ rim light quanh viền đối tượng.
        _RimStrength ("Rim Strength", Range(0.0, 1.5)) = 0.14
        // Độ sắc/giảm dần của rim light.
        _RimPower ("Rim Power", Range(1.0, 8.0)) = 3.6

        [Header(Ambient Shadow)]
        // Cường độ ánh sáng ambient (chỉ lấy độ sáng, không lấy màu ambient).
        _AmbientStrength ("Ambient Strength", Range(0.0, 2.0)) = 1.0
        // Độ ảnh hưởng của bóng đổ từ hệ thống shadow của scene.
        _ShadowStrength ("Shadow Strength", Range(0.0, 1.0)) = 0.32
        // Màu phủ nhẹ cho vùng trong bóng.
        _ShadowTint ("Shadow Tint", Color) = (0.93, 0.95, 0.98, 1.0)

        [Header(Fake Light Direction)]
        // Hướng đèn giả chính để tính toon band và highlight.
        _FakeLightDir ("Fake Light Dir", Vector) = (0.45, 1.0, 0.28, 0.0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 220

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            fixed4 _Color;
            fixed4 _MainColor;

            half _ValueBoost;
            half _Saturation;
            half _Contrast;

            half _ToonSteps;
            half _MinLight;
            half _Smoothness;
            half _FaceContrast;
            half _MidBandStrength;
            half _BlockEdgeContrast;
            half _BlockEdgeWidth;
            half _TopBoost;
            half _SideDarken;

            half _EdgeDarken;
            half _EdgePower;
            half _RimStrength;
            half _RimPower;

            half _AmbientStrength;
            half _ShadowStrength;
            fixed4 _ShadowTint;

            float4 _FakeLightDir;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                half3 worldNormal : TEXCOORD1;
                SHADOW_COORDS(2)
                UNITY_FOG_COORDS(3)
            };

            inline half3 ResolveBaseColor()
            {
                // Lấy màu nền chính từ _Color.
                half3 c = _Color.rgb;
                // Nếu _Color vẫn là trắng mặc định, fallback về _MainColor cũ để tương thích material legacy.
                half isUntouchedColor = step(abs(c.r - 1.0) + abs(c.g - 1.0) + abs(c.b - 1.0), 0.0001);
                return lerp(c, _MainColor.rgb, isUntouchedColor);
            }

            inline half3 TuneColor(half3 c)
            {
                // 1) Nâng/giảm độ sáng tổng.
                c *= _ValueBoost;
                // 2) Tính độ sáng (luma) để điều chỉnh saturation.
                half l = dot(c, half3(0.299, 0.587, 0.114));
                // 3) Tăng/giảm độ đậm màu.
                c = lerp(l.xxx, c, _Saturation);
                // 4) Tăng/giảm tương phản quanh mốc 0.5.
                c = (c - 0.5h) * _Contrast + 0.5h;
                // Chặn màu trong khoảng hợp lệ.
                return saturate(c);
            }

            v2f vert(appdata v)
            {
                v2f o;
                // Biến đổi đỉnh sang clip-space để rasterize.
                o.pos = UnityObjectToClipPos(v.vertex);
                // Lưu world position để tính view direction ở fragment.
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // Đưa normal sang world-space để tính lighting ổn định theo camera.
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                // Truyền toạ độ shadow map.
                TRANSFER_SHADOW(o);
                // Truyền dữ liệu fog.
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Chuẩn hoá các vector cơ bản.
                half3 n = normalize(i.worldNormal);
                half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                half3 fakeLightDir = normalize(_FakeLightDir.xyz);
                half3 halfDir = normalize(viewDir + fakeLightDir);

                // Màu nền sau khi áp tuning.
                half3 baseColor = TuneColor(ResolveBaseColor());
                // Độ sáng màu nền: dùng để xử lý riêng cho màu nhạt/trắng.
                half baseLuma = dot(baseColor, half3(0.299h, 0.587h, 0.114h));
                // paleBoost cao khi màu gần trắng.
                half paleBoost = smoothstep(0.72h, 0.98h, baseLuma);

                // Khối toon cơ bản: chia dải sáng/tối theo hướng đèn giả.
                half ndl = saturate(dot(n, fakeLightDir) * 0.5h + 0.5h);
                half steps = max(2.0h, floor(_ToonSteps + 0.5h));
                half toonHard = floor(ndl * steps) / (steps - 1.0h);
                // Smoothness chỉ ảnh hưởng nhẹ đến chuyển dải để tránh cảm giác "chỉ tăng sáng".
                half smoothAffect = _Smoothness * 0.35h;
                half bandWidth = lerp(0.02h, 0.10h, smoothAffect);
                half toonSoft = smoothstep(0.0h, 1.0h, ndl + (ndl - 0.5h) * bandWidth);
                half toon = lerp(toonHard, toonSoft, smoothAffect);
                half lightTerm = lerp(_MinLight, 1.0h, saturate(toon));

                half3 result = baseColor * lightTerm;

                // Tăng tách mặt khối: tạo band sáng, band tối và band trung gian.
                half highlightBand = smoothstep(0.70h, 0.95h, ndl);
                half shadowBand = 1.0h - smoothstep(0.18h, 0.48h, ndl);
                half midBand = smoothstep(0.32h, 0.50h, ndl) * (1.0h - smoothstep(0.56h, 0.78h, ndl));
                result *= (1.0h + highlightBand * (_FaceContrast * lerp(0.18h, 0.14h, paleBoost)));
                result *= (1.0h - shadowBand * (_FaceContrast * lerp(0.16h, 0.30h, paleBoost)));
                result *= (1.0h - midBand * (_MidBandStrength * lerp(0.18h, 0.24h, paleBoost)));

                // Nhấn cạnh giữa các mặt phẳng: tối nhẹ vùng normal đang giao thoa nhiều trục.
                half3 an = abs(n);
                half maxAxis = max(an.x, max(an.y, an.z));
                half minAxis = min(an.x, min(an.y, an.z));
                half axisGap = maxAxis - minAxis;
                half blockEdgeMask = 1.0h - smoothstep(_BlockEdgeWidth, _BlockEdgeWidth + 0.22h, axisGap);
                result *= (1.0h - blockEdgeMask * _BlockEdgeContrast * lerp(0.55h, 0.85h, 1.0h - paleBoost));

                // Tách mặt trên/mặt bên để form rõ hơn.
                half topMask = saturate(n.y);
                result *= (1.0h + topMask * _TopBoost);

                half sideMask = saturate(1.0h - abs(n.y));
                result *= (1.0h - sideMask * _SideDarken);

                // Làm đậm viền silhouette để dễ đọc hình khối khi nhìn xa.
                half ndv = saturate(dot(n, viewDir));
                half edge = pow(1.0h - ndv, _EdgePower);
                half edgeDarken = lerp(_EdgeDarken, _EdgeDarken * 0.75h, smoothAffect) + paleBoost * 0.08h;
                result *= (1.0h - edge * edgeDarken);

                // Rim light nhẹ để tránh cảm giác bệt ở biên.
                half rim = pow(1.0h - ndv, _RimPower);
                half rimLift = lerp(0.35h, 0.42h, smoothAffect);
                result += baseColor * rim * _RimStrength * rimLift;

                // Specular kiểu smoothness gần Standard (không dùng hệ gloss custom riêng).
                half ndlFace = saturate(dot(n, fakeLightDir));
                half ndh = saturate(dot(n, halfDir));
                half smoothness = saturate(_Smoothness);
                // Hàm phản xạ: mượt hơn khi smoothness cao, nhưng vẫn giữ chất toon.
                half specPower = exp2(2.4h + smoothness * 6.2h);
                half specNorm = (specPower + 2.0h) * 0.125h;
                half spec = pow(ndh, specPower) * specNorm * ndlFace;
                // Màu tối/đậm dễ mất bóng -> tăng nhẹ coat để highlight vẫn thấy.
                half darkBoost = (1.0h - baseLuma);
                half specStrength = lerp(0.05h, 0.34h, smoothness) * lerp(1.0h, 1.35h, darkBoost) * lerp(1.0h, 1.18h, paleBoost);
                half coatMix = lerp(0.35h, 0.62h, darkBoost);
                half3 specColor = lerp(baseColor, half3(1, 1, 1), coatMix);
                // Xe trắng dễ "chìm bóng" -> đẩy phản xạ hơi lạnh để thấy chi tiết tốt hơn.
                specColor = lerp(specColor, half3(0.86h, 0.91h, 0.98h), paleBoost * 0.65h);
                result += specColor * spec * specStrength;

                // Nhận cường độ ambient theo SH, nhưng không nhuộm màu ambient trực tiếp.
                half3 sh = max(ShadeSH9(half4(n, 1.0h)), 0.0h);
                half ambientLuma = dot(sh, half3(0.299h, 0.587h, 0.114h));
                half ambientFactor = lerp(0.78h, 1.08h, saturate(ambientLuma)) * _AmbientStrength;
                result *= clamp(ambientFactor, 0.65h, 1.18h);

                // Nhận shadow attenuation từ scene, chỉ dùng mức độ tối (không lấy màu đèn ngoài).
                half shadowAtten = SHADOW_ATTENUATION(i);
                half shadowMix = lerp(1.0h - _ShadowStrength, 1.0h, shadowAtten);
                half3 shadowTinted = result * _ShadowTint.rgb;
                result = lerp(shadowTinted, result, shadowMix);

                // Kéo nhẹ về baseColor để màu toon giữ độ tươi, tránh bẩn màu do nhiều lớp nhân.
                result = lerp(result, baseColor, 0.08h);

                // Áp fog cuối pipeline.
                UNITY_APPLY_FOG(i.fogCoord, result);
                // Xuất màu cuối, clamp về [0..1].
                return fixed4(saturate(result), 1.0);
            }
            ENDCG
        }

        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }

    FallBack Off
}
