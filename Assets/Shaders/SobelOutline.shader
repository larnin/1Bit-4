Shader "PostProcessing/SobelOutline"
{
    HLSLINCLUDE
#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    TEXTURE2D_SAMPLER2D(_NormalTex, sampler_NormalTex);
    TEXTURE2D_SAMPLER2D(_DepthTex, sampler_DepthTex);

    float _OutlineThickness;
    float _OutlineDepthMultiplier;
    float _OutlineDepthBias;
    float _OutlineDepthScale;
    float _OutlineNormalMultiplier;
    float _OutlineNormalBias;
    float4 _OutlineColor;

    float4 SobelSample(Texture2D t, SamplerState s, float2 uv, float3 offset)
    {
        float4 pixelCenter = t.Sample(s, uv);
        float4 pixelLeft = t.Sample(s, uv - offset.xz);
        float4 pixelRight = t.Sample(s, uv + offset.xz);
        float4 pixelUp = t.Sample(s, uv + offset.zy);
        float4 pixelDown = t.Sample(s, uv - offset.zy);

        return abs(pixelLeft - pixelCenter) +
            //abs(pixelRight - pixelCenter) +
            //abs(pixelUp - pixelCenter) +
            abs(pixelDown - pixelCenter);
    }

    float SobelDepth(float ldc, float ldl, float ldr, float ldu, float ldd)
    {
        return abs(ldl - ldc) +
            //abs(ldr - ldc) +
            //abs(ldu - ldc) +
            abs(ldd - ldc);
    }

    float SobelSampleDepth(Texture2D t, SamplerState s, float2 uv, float3 offset)
    {
        float pixelCenter = t.Sample(s, uv).r;
        float pixelLeft = t.Sample(s, uv - offset.xz).r;
        float pixelRight = t.Sample(s, uv + offset.xz).r;
        float pixelUp = t.Sample(s, uv + offset.zy).r;
        float pixelDown = t.Sample(s, uv - offset.zy).r;

        return SobelDepth(pixelCenter, pixelLeft, pixelRight, pixelUp, pixelDown);
    }

    float4 FragMain(VaryingsDefault i) : SV_Target
    {
        float3 offset = float3((1.0 / _ScreenParams.x), (1.0 / _ScreenParams.y), 0.0) * _OutlineThickness;
        float3 sceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord).rgb;

        float sobelDepth = SobelSampleDepth(_DepthTex, sampler_DepthTex, i.texcoord.xy, offset) / _OutlineDepthScale * 100;
        sobelDepth = pow(saturate(sobelDepth) * _OutlineDepthMultiplier, _OutlineDepthBias);

        float3 sobelNormalVec = SobelSample(_NormalTex, sampler_NormalTex, i.texcoord.xy, offset).rgb;
        float sobelNormal = sobelNormalVec.x + sobelNormalVec.y + sobelNormalVec.z;
        sobelNormal = pow(sobelNormal * _OutlineNormalMultiplier, _OutlineNormalBias);

        float sobelOutline = saturate(max(sobelDepth, sobelNormal));
        if (sobelOutline < 0.5)
            sobelOutline = 0;
        else sobelOutline = 1;

        float3 outlineColor = lerp(sceneColor, _OutlineColor.rgb, _OutlineColor.a);
        float3 color = lerp(sceneColor, outlineColor, sobelOutline);

        return float4(color, 1.0);
    }
        ENDHLSL

        SubShader
    {
        Cull Off ZWrite Off ZTest Always

            Pass
        {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragMain
            ENDHLSL
        }
    }
}