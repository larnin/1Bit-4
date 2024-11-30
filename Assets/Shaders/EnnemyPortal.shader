Shader "Unlit/EnnemyPortal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PerlinTex("Noise", 2D) = "white" {}
        _LightTop("LightTop", float) = 1
        _LightLeft("LightLeft", float) = 0.8
        _LightFront("LightFront", float) = 0.2
        _CircleFrequency("CircleFrequency", float) = 1
        _CircleTime("CircleTimeSpeed", float) = 1
        _TextureOffsetSpeed("TextureOffsetSpeed", float) = 1
        _NoiseTextureScale("NoiseTextureScale", float) = 1
        _NoiseScale("NoiseScale", float) = 1
        _ColorStep("ColorStep", float) = 0.5
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _PerlinTex;
            float4 _PerlinTex_ST;

            float _LightTop;
            float _LightLeft;
            float _LightFront;

            float _CircleFrequency;
            float _CircleTime;
            float _TextureOffsetSpeed;
            float _NoiseTextureScale;
            float _NoiseScale;
            float _ColorStep;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                return o;
            }

            float circle(float2 uv)
            {
                float2 pos = uv - float2(0.5, 0.5);

                float d = sqrt(pos.x * pos.x + pos.y * pos.y);

                return d;
            }

            float noise(float2 uv)
            {
                float2 offset1 = float2(_Time.y, -_Time.y);
                float offset2 = float2(-_Time.y / 2, _Time.y * 1.5);

                float4 col1 = tex2D(_PerlinTex, uv * _NoiseTextureScale + offset1 * _TextureOffsetSpeed);
                float4 col2 = tex2D(_PerlinTex, uv * _NoiseTextureScale + offset2 * _TextureOffsetSpeed);

                return col1.r * col2.r;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                float n = noise(i.uv);

                float d = sin(circle(i.uv) * _CircleFrequency + _Time.y * _CircleTime + n * _NoiseScale) * 0.5 + 0.5;
                if (d < _ColorStep)
                    d = 1;
                else d = 0;
                col *= d;

				float a = abs(dot(i.normal, float3(0, 1, 0))) * _LightTop;
				float b = abs(dot(i.normal, float3(1, 0, 0))) * _LightLeft;
				float c = abs(dot(i.normal, float3(0, 0, 1))) * _LightFront;

                float mul = sqrt(a*a + b*b + c*c);

                return col * mul;
            }
            ENDCG
        }
    }
}
