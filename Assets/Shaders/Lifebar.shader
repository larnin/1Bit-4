Shader "Unlit/Lifebar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BackColor("BackColor", Color) = (1, 1, 1, 1)
        _FillColor("FillColor", Color) = (1, 1, 1, 1)
        _FillPercent("FillPercent", Range(0.0, 1.0)) = 0.5 
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _BackColor;
            float4 _FillColor;
            float _FillPercent;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col.g = 0;
                col.b = 0;

                float xValue = i.uv.x;
                if (i.uv.y < 0.5)
                {
                    if (xValue < 0.5)
                        xValue = 0;
                    else xValue = 1;
                }
                fixed4 mulCol = xValue <= _FillPercent ? _FillColor : _BackColor;

            //float4 mulCol = i.uv.y * _FillColor + (1 - i.uv.y) * _BackColor;
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col * mulCol;
            }
            ENDCG
        }
    }
}
