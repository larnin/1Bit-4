Shader "UI/Fade"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_PerlinTex("Noise", 2D) = "white" {}
		_PerlinScale("NoiseScale", float) = 1
		_PerlinAmplitude("NoiseAmplitude", float) = 1
		_CircleAmplitude("CircleAmplitude", float) = 1
		_Percent("Percent", Range(0.0, 1.0)) = 1
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

			float4 _Color;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _PerlinTex;
			float4 _PerlinTex_ST;

			float _PerlinScale;
			float _PerlinAmplitude;
			float _CircleAmplitude;

			float _Percent;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = v.normal;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 pos = i.uv - float2(0.5, 0.5);

				float distToCenter = sqrt(pos.x * pos.x + pos.y * pos.y) * sqrt(2) * _CircleAmplitude;

				float min = 0 - _PerlinAmplitude;
				float max = _CircleAmplitude + _PerlinAmplitude;

				float4 noiseCol = tex2D(_PerlinTex, i.uv * _PerlinScale);
				float noiseValue = (noiseCol.r + noiseCol.g + noiseCol.b) / 3 * _PerlinAmplitude;

				float scaledPercent = _Percent * (max - min) + min;

				float value = distToCenter + noiseValue - scaledPercent;
				if (value < 0)
					value = 0;
				if (value > 1)
					value = 1;
				
				float4 col = _Color * value;

				return col;
			}
			ENDCG
		}
    }
}
