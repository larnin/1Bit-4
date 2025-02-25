Shader "Projector/PlaceBuildingDecal" 
{
	Properties
	{
		_ShadowTex("Cookie", 2D) = "gray" {}
	}

	Subshader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		Pass 
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 uvShadow : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			float4x4 unity_Projector;

			v2f vert(float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.uvShadow = mul(unity_Projector, vertex);
				return o;
			}

			sampler2D _ShadowTex;
			sampler2D _FalloffTex;

			float4 frag(v2f i) : SV_Target
			{
				float4 texS = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));

				float x = 2 * (i.uvShadow.x - 0.5);
				float y = 2 * (i.uvShadow.y - 0.5);

				float dist = sqrt((x * x) + (y * y));

				float mul = 1;
				if (dist >= 1)
					mul = 0;
				else if (dist >= 0.8)
					mul = 1 - (dist - 0.8) / 0.2;

				if (texS.r > 0.1f && texS.r < 0.9f) 
					return float4(0, 0, 0, mul);
				return float4(0, 0, 0, 0);
			}
			ENDCG
		}
	}
}