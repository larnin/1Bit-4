Shader "Unlit/NormalGeneration"
{
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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 col = i.normal * 0.5 + float3(0.5, 0.5, 0.5);
                return float4(col, 1.0);
            }
            ENDCG
        }
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

			sampler2D _MainTex;
			float4 _MainTex_ST;

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

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal; 
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 colTex = tex2D(_MainTex, i.uv);

				float3 col = i.normal * 0.5 + float3(0.5, 0.5, 0.5);
				clip(colTex.a - 0.5);
				return float4(col, colTex.a);
			}
			ENDCG
		}
	}

    Fallback "Standard"
}
