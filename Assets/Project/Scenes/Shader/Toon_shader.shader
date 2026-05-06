Shader "Custom/Toon_Shader"
{
    Properties
    {
        _Color ("Base Color", Color) = (0,1,1,1)

        _Dark ("Dark", Range(0,1)) = 0.2
        _Mid ("Mid", Range(0,1)) = 0.6
        _Bright ("Bright", Range(0,2)) = 1.0

        _DarkPortion ("Dark Portion", Range(0,1)) = 0.49
        _MidPortion ("Mid Portion", Range(0,1)) = 0.49
        _BrightPortion ("Bright Portion", Range(0,1)) = 0.02

        // ✅ SHADOW OVERLAY (NEW)
        _ShadowColor ("Shadow Color", Color) = (0.05, 0.1, 0.2, 1)
        _ShadowStrength ("Shadow Strength", Range(0,2)) = 0.7

        // Rim
        _RimColor ("Rim Color", Color) = (0.2, 1, 0.8, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
        _RimStrength ("Rim Strength", Range(0,2)) = 0.8

        // Outline
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0.001, 0.05)) = 0.01
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        // ========================
        // OUTLINE
        // ========================
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="Always" }

            Cull Front
            ZWrite On
            ZTest Less

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _OutlineWidth;
            float4 _OutlineColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float3 normal = normalize(v.normal);
                float3 pos = v.vertex.xyz + normal * _OutlineWidth;
                o.pos = UnityObjectToClipPos(float4(pos,1));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        // ========================
        // MAIN PASS
        // ========================
        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float4 _Color;

            float _Dark, _Mid, _Bright;
            float _DarkPortion, _MidPortion, _BrightPortion;

            float4 _ShadowColor;
            float _ShadowStrength;

            float4 _RimColor;
            float _RimPower, _RimStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.normalWS);

                // LIGHT
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = saturate(dot(normal, lightDir));

                // =====================
                // TOON BASE
                // =====================
                float t1 = _DarkPortion;
                float t2 = _DarkPortion + _MidPortion;

                float toon;

                if (NdotL < t1)
                    toon = _Dark;
                else if (NdotL < t2)
                    toon = _Mid;
                else
                    toon = _Bright;

                float3 baseColor = _Color.rgb * toon;

                // =====================
                // SHADOW OVERLAY (KEY FIX)
                // =====================
                float shadowMask = step(NdotL, _DarkPortion);

                
                float3 shadowOverlay = _ShadowColor.rgb * _ShadowStrength * shadowMask;

                
                float3 colorWithShadow = baseColor + shadowOverlay;

                // =====================
                // VIEW
                // =====================
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

                // =====================
                // RIM
                // =====================
                float rim = 1.0 - saturate(dot(viewDir, normal));
                rim = pow(rim, _RimPower);
                rim *= (1.0 - NdotL);

                float3 rimColor = _RimColor.rgb * rim * _RimStrength;

                // =====================
                // FINAL
                // =====================
                float3 finalColor = colorWithShadow + rimColor;

                return float4(finalColor, 1);
            }

            ENDCG
        }
    }
}