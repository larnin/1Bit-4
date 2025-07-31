Shader "Custom/SilhouetteUnlit"
{
    Properties
    {
        _SilhouetteColor("Silhouette Color", Color) = (1, 0, 0, 0.5)
        _Color("Color", Color) = (1,1,1,1)
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }

 
        ZWrite On

        Stencil
        {
            Ref 2
            Pass Replace
        }

        CGPROGRAM
        #pragma surface surf nolight alpha:fade noforwardadd nolightmap noambient novertexlights noshadow
        #pragma target 3.0

        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Emission = _Color.rgb;
            o.Alpha = _Color.a;
        }

        float4 Lightingnolight(SurfaceOutput s, float3 lightDir, float atten)
        {
            return float4(s.Emission, s.Alpha);
        }
        ENDCG

        ZWrite Off
        ZTest Greater

        Stencil
        {
            Ref 2
            Comp NotEqual
        }

        CGPROGRAM
        #pragma surface surf nolight alpha:fade noforwardadd nolightmap noambient novertexlights noshadow

        struct Input { float4 color:COLOR; };
        float4 _SilhouetteColor;

        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Emission = _SilhouetteColor.rgb;
            o.Alpha = _SilhouetteColor.a;
        }

        float4 Lightingnolight(SurfaceOutput s, float3 lightDir, float atten)
        {
            return float4(s.Emission, s.Alpha);
        }
        ENDCG
    }
        FallBack "Diffuse"
}