Shader "Hidden/SobelOutline"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _NormalTex("Normals", 2D) = "white" {}
        _DepthTex("Depth", 2D) = "White" {}
    }
    
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform float _OutlineThickness;
            uniform float _OutlineDepthMultiplier;
            uniform float _OutlineDepthBias;
            uniform float _OutlineDepthScale;
            uniform float _OutlineNormalMultiplier;
            uniform float _OutlineNormalBias;
            uniform float4 _OutlineColor;

            sampler2D _MainTex;
            sampler2D _NormalTex;
            sampler2D _DepthTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 offset = float3((1.0 / _ScreenParams.x), (1.0 / _ScreenParams.y), 0.0) * _OutlineThickness;
                float3 sceneColor = tex2D(_MainTex, i.uv).rgb;

                float sobelDepth = SobelSampleDepth(_DepthTex,  i.uv, offset) / _OutlineDepthScale * 100;
                sobelDepth = pow(saturate(sobelDepth) * _OutlineDepthMultiplier, _OutlineDepthBias);

                float3 sobelNormalVec = SobelSample(_NormalTex, i.uv, offset).rgb;
                float sobelNormal = sobelNormalVec.x + sobelNormalVec.y + sobelNormalVec.z;
                sobelNormal = pow(sobelNormal * _OutlineNormalMultiplier, _OutlineNormalBias);

                float sobelOutline = saturate(max(sobelDepth, sobelNormal));
                float3 outlineColor = lerp(sceneColor, _OutlineColor.rgb, _OutlineColor.a);

                float3 color = sceneColor;
                if (sobelOutline > 0.5f)
                    color = outlineColor;

                float AvgColor = AverageColor(_MainTex, i.uv, offset);
                color.r = (1 - color.r) * AvgColor + color.r * (1 - AvgColor);

                return float4(color.r, color.r, color.r, 1.0);
            }

            ENDCG

            CGINCLUDE
                
            float AverageColor(sampler2D t, float2 uv, float3 offset)
            {
                float4 pixelCenter = tex2D(t, uv);
                float4 pixelLeft = tex2D(t, uv - offset.xz);
                //float4 pixelRight = tex2D(t, uv + offset.xz);
                //float4 pixelUp = tex2D(t, uv + offset.zy);
                float4 pixelDown = tex2D(t, uv - offset.zy);

                return max(max(pixelCenter.g, pixelLeft.g), pixelDown.g);
            }

            float4 SobelSample(sampler2D t, float2 uv, float3 offset)
            {
                float4 pixelCenter = tex2D(t, uv);
                float4 pixelLeft = tex2D(t, uv - offset.xz);
                //float4 pixelRight = tex2D(t, uv + offset.xz);
                //float4 pixelUp = tex2D(t, uv + offset.zy);
                float4 pixelDown = tex2D(t, uv - offset.zy);

                return abs(pixelLeft - pixelCenter) +
                    //abs(pixelRight - pixelCenter) +
                    //abs(pixelUp - pixelCenter) +
                    abs(pixelDown - pixelCenter);
            }

            float SobelDepth(float ldc, float ldl/*, float ldr, float ldu*/, float ldd)
            {
                return abs(ldl - ldc) +
                    //abs(ldr - ldc) +
                    //abs(ldu - ldc) +
                    abs(ldd - ldc);
            }

            float SobelSampleDepth(sampler2D t, float2 uv, float3 offset)
            {
                float pixelCenter = tex2D(t, uv).r;
                float pixelLeft = tex2D(t, uv - offset.xz).r;
                //float pixelRight = tex2D(t, uv + offset.xz).r;
                //float pixelUp = tex2D(t, uv + offset.zy).r;
                float pixelDown = tex2D(t, uv - offset.zy).r;

                return SobelDepth(pixelCenter, pixelLeft/*, pixelRight, pixelUp*/, pixelDown);
            }
            ENDCG
        }
    }
}