Shader "Unlit/LaserRay"
{
    Properties
    {
        _PerlinTex("Noise", 2D) = "white" {}
        _PerlinAmplitude("Noise Amplitude", float) = 1
        _PerlinLength("Noise Length", float) = 1
        _PerlinWidth("Noise Width", float) = 1
        _PerlinSpeedLength("Noise Speed Length", float) = 1
        _PerlinSpeedWidth("Noise Speed Width", float) = 1
        _WhiteWidth("White width", float) = 1
        _BlackWidth("Black Width", float) = 1
    }
        SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" }
        LOD 100

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

            sampler2D _PerlinTex;
            float4 _PerlinTex_ST;
            float _PerlinAmplitude;
            float _PerlinLength;
            float _PerlinWidth;
            float _PerlinSpeedLength;
            float _PerlinSpeedWidth;
            float _WhiteWidth;
            float _BlackWidth;
            float _LengthRatio;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = v.normal;
                return o;
            }

            float noise(float2 uv, float timeX, float timeY)
            {
                float2 offset1 = float2(timeX, timeY * 1.1);
                float2 offset2 = float2(timeX * 1.1, -timeY * 0.8);

                float2 scale = float2(_PerlinWidth, _PerlinLength / _LengthRatio);

                float4 col1 = tex2D(_PerlinTex, uv * scale + offset1);
                float4 col2 = tex2D(_PerlinTex, uv * scale + offset2);

                return (col1.r - col2.r) * _PerlinAmplitude;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = abs((i.uv.y - 0.5) * 2) / _Intensity;
                dist += noise(i.uv, _PerlinSpeedWidth * _Time.y, _PerlinSpeedLength * _Time.y);

                float color = 0;
                float alpha = 1;
                if (dist < _WhiteWidth)
                    color = 1;
                if (dist > _BlackWidth)
                    alpha = 0;

                clip(alpha - 0.5);
                return float4(color, 0, 1, alpha);
            }
            ENDCG
        }
    }

    Fallback "Standard"
}
