Shader "Custom/WatercolorShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _PaperTex ("Paper Texture", 2D) = "white" {}
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.1
        _EdgeDarken ("Edge Darken", Range(0, 1)) = 0.3
        _TimeScale ("Time Scale", Range(0, 1)) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
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
            sampler2D _PaperTex;
            float4 _Color;
            float _DistortionStrength;
            float _EdgeDarken;
            float _TimeScale;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float timeOffset = _Time.y * _TimeScale;
                float2 offset = float2(sin(i.uv.y * 20.0 + timeOffset) * _DistortionStrength, cos(i.uv.x * 20.0 + timeOffset) * _DistortionStrength);
                float2 uvDistorted = i.uv + offset;

                fixed4 col = tex2D(_MainTex, uvDistorted) * _Color;
                fixed4 paper = tex2D(_PaperTex, i.uv);

                float edge = smoothstep(0.1, 0.5, length(i.uv - 0.5)) * _EdgeDarken;
                col.rgb *= (1.0 - edge);

                col.rgb = lerp(col.rgb, paper.rgb, 0.3);
                return col;
            }
            ENDCG
        }
    }
}
