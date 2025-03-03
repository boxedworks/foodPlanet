Shader "Hidden/EdgeDetection"
{
    Properties
    {
        _EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)
        _Threshold ("Edge Threshold", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // The texture samplers
            TEXTURE2D(_CameraDepthTexture);
            TEXTURE2D(_CameraNormalsTexture);
            SAMPLER(sampler_CameraDepthTexture);
            SAMPLER(sampler_CameraNormalsTexture);

            // Edge and threshold properties
            float4 _EdgeColor;
            float _Threshold;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            // Sample depth from the depth texture
            float SampleDepth(float2 uv)
            {
                float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
                return LinearEyeDepth(rawDepth, _ZBufferParams);
            }

            // Sample normals from the normal texture
            float3 SampleNormal(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv).rgb;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Compute pixel size
                float2 texelSize = 1.0 / _ScreenParams.xy;

                // Depth and normal at the current pixel
                float depthCenter = SampleDepth(IN.uv);
                float3 normalCenter = SampleNormal(IN.uv);

                // Sample depth and normals in the neighboring pixels
                float depthLeft = SampleDepth(IN.uv + float2(-texelSize.x, 0));
                float depthRight = SampleDepth(IN.uv + float2(texelSize.x, 0));
                float depthUp = SampleDepth(IN.uv + float2(0, texelSize.y));
                float depthDown = SampleDepth(IN.uv + float2(0, -texelSize.y));

                float3 normalLeft = SampleNormal(IN.uv + float2(-texelSize.x, 0));
                float3 normalRight = SampleNormal(IN.uv + float2(texelSize.x, 0));
                float3 normalUp = SampleNormal(IN.uv + float2(0, texelSize.y));
                float3 normalDown = SampleNormal(IN.uv + float2(0, -texelSize.y));

                // Edge detection based on depth and normal differences
                float depthEdge = abs(depthLeft - depthRight) + abs(depthUp - depthDown);
                float normalEdge = length(normalLeft - normalRight) + length(normalUp - normalDown);

                // Combine the depth and normal edges
                float edge = depthEdge + normalEdge;

                // Return the edge color if the edge is above the threshold
                return edge > _Threshold ? _EdgeColor : float4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}
