Shader "Unlit/LightCircle"
{
    Properties
    {
        _Border ("Border", Float) = 1
    }
    SubShader
    {
        //Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            uniform float _Size;
            float _Border;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                i.uv.x -= 0.5;
                i.uv.y -= 0.5;
                i.uv *= 2;

                float r = sqrt(i.uv.x * i.uv.x + i.uv.y * i.uv.y);

                float border = _Border / _Size;

                if (r > 1)
                    return float4(1, 1, 1, 0);
                r = 1 - (r - (1 - border)) / border;
                if (r > 1)
                    return float4(1, 1, 1, 1);

                return float4(1, 1, 1, r);
            }
            ENDCG
        }
    }
}
