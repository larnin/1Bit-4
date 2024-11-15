Shader "Unlit/CustomLighted"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LightTop("LightTop", float) = 1
        _LightLeft("LightLeft", float) = 0.8
        _LightFront("LightFront", float) = 0.2

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

            float _LightTop;
            float _LightLeft;
            float _LightFront;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.initialVertex = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 lightUV = i.initialVertex.xz / _LightTexSize;
                lightUV.x = clamp(lightUV.x, 0, 1);
                lightUV.y = clamp(lightUV.y, 0, 1);

                fixed4 light = tex2D(_LightTex, lightUV);

                fixed4 col = tex2D(_MainTex, i.uv);

                float mul = abs(dot(i.normal, float3(0, 1, 0))) * _LightTop * _LightTop;
                mul += abs(dot(i.normal, float3(1, 0, 0))) * _LightLeft * _LightLeft;
                mul += abs(dot(i.normal, float3(0, 0, 1))) * _LightFront * _LightFront;

                return col * light * mul;
            }
            ENDCG
        }
    }

    Fallback "Standard"
}
