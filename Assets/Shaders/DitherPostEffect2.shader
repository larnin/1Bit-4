Shader "Hidden/DitherPostEffect2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform float _DarkLevel = 0;
			uniform float _LightLevel = 1;

			uniform float4 _DarkColor;
			uniform float4 _LightColor;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);

				float level = col.r;

				if (level <= _DarkLevel)
					return _DarkColor;
				if (level >= _LightLevel)
					return _LightColor;

				level -= _DarkLevel;
				level /= (_LightLevel - _DarkLevel);
				
				/*float randPos = hash(i.vertex.xy, _Time.y);
				if (level > randPos)
					level = 1;
				else level = 0;*/

				level *= 7;
				float2 tempPos = i.vertex.xy;

				int2 pos = int2(floor(tempPos.x), floor(tempPos.y));

				if (level < 1)
					level = ditherLevel1(pos);
				else if (level < 2)
					level = ditherLevel2(pos);
				else if (level < 3)
					level = ditherLevel3(pos);
				else if (level < 4)
					level = ditherLevel4(pos);
				else if(level < 5)
					level = 1 - ditherLevel3(pos);
				else if(level < 6)
					level = 1 - ditherLevel2(pos);
				else level = 1 - ditherLevel1(pos);

				return _LightColor * level + _DarkColor * (1 - level);
			}

			ENDCG
			
			CGINCLUDE

			float hash(float2 uv, float seed)
			{
				return frac(sin(7.289 * uv.x + 11.23 * uv.y + 18.747 * seed) * 23758.5453);
			}

			float ditherLevel1(int2 pos)
			{
				return (pos.x + pos.y / 2) % 4 == 0 && pos.y % 4 == 0;
			}

			float ditherLevel2(int2 pos)
			{
				return (pos.x + pos.y) % 4 == 0 && (pos.x - pos.y) % 4 == 0;
			}

			float ditherLevel3(int2 pos)
			{
				return (pos.x + pos.y) % 4 == 0 || (pos.x - pos.y) % 4 == 0;
			}

			float ditherLevel4(int2 pos)
			{
				return (pos.x + pos.y) % 2;
			}

			float4 colorToHSV(float4 col)
			{
				float4 hsv = float4(0, 0, 0, col.a);

				float minValue = min(col.r, min(col.g, col.b));
				float maxValue = max(col.r, max(col.g, col.b));

				hsv.z = maxValue;

				float delta = maxValue - minValue;
				if (delta < 0.00001)
				{
					hsv.x = 0;
					hsv.y = 0;
					return hsv;
				}

				if (maxValue > 0)
					hsv.y = (delta / maxValue);
				else
				{
					hsv.x = 0;
					hsv.y = 0;
				}
				
				if (col.r >= maxValue)
					hsv.x = (col.g - col.b) / delta;
				else if (col.g >= maxValue)
					hsv.x = 2 + (col.b - col.r) / delta;
				else hsv.x = 4 + (col.r - col.g) / delta;

				hsv.x *= 60;

				if (hsv.x < 0)
					hsv.x += 360;

				return hsv;
			}

			float4 HSVToColor(float4 hsv)
			{
				float4 col = hsv.z;

				float varH = hsv.x * 6 / 360;
				float varI = floor(varH);
				float var1 = hsv.z * (1 - hsv.y);
				float var2 = hsv.z * (1 - hsv.y * (varH - varI));
				float var3 = hsv.z * (1 - hsv.y * (1 - (varH - varI)));

				if (varI == 0) { col = float4(hsv.z, var3, var1, hsv.a); }
				else if (varI == 1) { col = float4(var2, hsv.z, var1, hsv.a); }
				else if (varI == 2) { col = float4(var1, hsv.z, var3, hsv.a); }
				else if (varI == 3) { col = float4(var1, var2, hsv.z, hsv.a); }
				else if (varI == 4) { col = float4(var3, var1, hsv.z, hsv.a); }
				else { col = float4(hsv.z, var1, var2, hsv.a); }

				return col;
			}

			ENDCG
		}
    }
}
