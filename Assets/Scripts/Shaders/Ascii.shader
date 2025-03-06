Shader "Custom/ASCIIShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ASCIIChars ("ASCII Characters", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _ASCIIChars;
            float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float GetLuminance(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float lum = GetLuminance(col.rgb);

                float charIndex = floor(lum * 16); // Assuming 16 grayscale ASCII characters
                float2 charUV = float2(charIndex / 16.0, 0.5); // Sample from ASCII texture

                fixed4 asciiCol = tex2D(_ASCIIChars, charUV);
                return asciiCol;
            }
            ENDCG
        }
    }
}
