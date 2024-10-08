Shader "Unlit/TransparentFresnel"
{
    Properties
    {
        _InnerColor("Inner Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _RimColor("Rim Color", Color) = (0.26,0.19,0.16,0.0)
        _RimPower("Rim Power", Range(0.5,8.0)) = 3.0
    }
    SubShader
    {
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off

        CGPROGRAM
        #pragma surface surf Unlit alpha

        half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
        {
            half4 c;
            c.rgb = s.Albedo;
            c.a = s.Alpha;
            return c;
        }

        struct Input
        {
            float3 viewDir;
        };

        float4 _InnerColor;
        float4 _RimColor;
        float _RimPower;

        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _InnerColor.rgb;
            half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            half rimPower = pow(rim, _RimPower);
            o.Emission = _RimColor.rgb * rimPower;
            o.Alpha = rimPower* _RimColor.a + _InnerColor.a * (1 - _RimColor.a);
        }
        ENDCG
    }
}
