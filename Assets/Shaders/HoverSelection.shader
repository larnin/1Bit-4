Shader "Unlit/HoverSelection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FallOff("Falloff", Range(0, 1)) = 0.2
        _Alpha("Alpha", Range(0, 1)) = 1
        _LineHeight("Line Height", Range(0, 1)) = 0.1
        _LineFrequency("Line Frequency", float) = 1
        _LineCurve("Line Curve", float) = 0.2
        _LineSpeed("Line Speed", float) = 1
    }
        SubShader
    {
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _FallOff;
            float _Alpha;
            float _LineHeight;
            float _LineFrequency;
            float _LineCurve;
            float _LineSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float Alpha = (1 - i.uv.x) / _FallOff;
                if (Alpha > 1)
                    Alpha = 1;
                Alpha *= _Alpha;

                float v = (1 - i.uv.x) + _Time.y * _LineSpeed;
                float offset = i.uv.y;
                if (offset > 0.5)
                    offset = 1 - offset;
                v += offset * _LineCurve;

                float v2 = fmod(v, 1 / _LineFrequency); 
                v = fmod(v, 2 / _LineFrequency);

                if (v2 > _LineHeight / _LineFrequency)
                    Alpha = 0;
                else
                {
                    if (v >= 0.5f)
                        v2 = 0;
                    else v2 = 1;
                }

                fixed4 col = fixed4(v2, v2, v2, Alpha);

                return col;
            }
            ENDCG
        }
    }
}
