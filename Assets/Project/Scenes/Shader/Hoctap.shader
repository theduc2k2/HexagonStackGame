Shader "Unlit/Hoctap"
{
    Properties
    {
      _Color("Test Color", color) = (1,1,1,1)
      _MainTexture("Main Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100 // mức độ chi tiết của shader, càng cao thì càng chi tiết nhưng cũng tốn tài nguyên hơn

        Pass // một pass là một lần vẽ, có thể có nhiều pass để tạo ra hiệu ứng phức tạp hơn
        {
            CGPROGRAM 
            #pragma vertex vert // định nghĩa hàm vertex shader , nó chạy trên mọi đỉnh của các đối tượng , có nhiệm vụ biến đổi vị trí của các đỉnh từ không gian đối tượng sang không gian màn hình
            #pragma fragment frag // định nghĩa hàm fragment shader , để xử lí từng pixel màu trên màn hình hoặc gameobject
            

            #include "UnityCG.cginc"

            struct appdata // đối tượng dạng dữ liệu lưới hoặc object, chứa thông tin về vị trí , màu sắc , tọa độ UV của các đỉnh
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f //Vertex to Fragment, chứa thông tin về vị trí của đỉnh sau khi đã được biến đổi sang không gian
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uv *= 2.0; // scale UV coordinates by 2
                o.uv.x += 0.5;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv; 
                
                fixed4 textureColor = tex2D(_MainTexture, uv);
                
                // sample the texture
                
                return textureColor*_Color; // trả về màu sắc cuối cùng của pixel, kết hợp giữa màu sắc của texture và màu sắc được định nghĩa trong shader
                
            }
            ENDCG
        }
    }
}
