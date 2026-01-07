Shader "Unlit/CustomLighted"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _InvertColor("InvertColor", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _LightTex;
            uniform float _LightTexSize;

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
                float4 initialVertex : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Color;

            float _LightTop;
            float _LightLeft;
            float _LightFront;
            float _InvertColor;
            float _LightBaseRange;

            sampler2D _PerlinTex;
            float4 _PerlinTex_ST;
            float _NoiseTextureScale;
            float _NoiseAmplitude;
            float _NoiseTime;
            float _NoiseOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.initialVertex = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            float noise(float2 uv, float time)
            {
                float2 offset1 = float2(time, time);
                float offset2 = float2(-time / 2, time * 1.5);

                float4 col1 = tex2D(_PerlinTex, uv * _NoiseTextureScale + offset1);
                float4 col2 = tex2D(_PerlinTex, uv * _NoiseTextureScale + offset2);

                return (col1.r - col2.r) * _NoiseAmplitude;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 lightUV = i.initialVertex.xz / _LightTexSize;
                lightUV.x = clamp(lightUV.x, 0, 1);
                lightUV.y = clamp(lightUV.y, 0, 1);

                float4 light = tex2D(_LightTex, lightUV);

                float noiseOffset = noise(lightUV, _NoiseTime);
                float minLight = 0.5 - _LightBaseRange / 2 + noiseOffset;
                float maxLight = 0.5 + _LightBaseRange / 2 + noiseOffset;
                light = (light - minLight) / (maxLight - minLight);
                light = clamp(light, 0, 1);

                float4 col = tex2D(_MainTex, i.uv);
                col *= _Color;

				float a = abs(dot(i.normal, float3(0, 1, 0))) * _LightTop;
				float b = abs(dot(i.normal, float3(1, 0, 0))) * _LightLeft;
				float c = abs(dot(i.normal, float3(0, 0, 1))) * _LightFront;

				float mul = sqrt(a*a + b * b + c * c);

				clip(col.a - 0.5);

                return float4((col * light * mul).r, _InvertColor, 0, col.a);
            }
            ENDCG
        }
    }

    Fallback "Standard"
}
