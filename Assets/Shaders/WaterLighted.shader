Shader "Unlit/WaterLighted"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _InvertColor("InvertColor", Range(0, 1)) = 0
        _ShoreTex("Shore", 2D) = "white" {}
        _ShoreWaveStep("ShoreWaveStep", float) = 0.5
        _ShoreWaveSize("ShoreWaveSize", float) = 1
        _ShoreWaveSpeed("ShoreWaveSpeed", float) = 1
        _ShoreWaveNoiseTextureScale("ShoreWaveNoiseTextureScale", float) = 1
        _ShoreWaveNoiseScale("ShoreWaveNoiseScale", float) = 1
        _ShoreWaveNoiseSpeed("ShorewaveNoiseSpeed", float) = 1
        _ShoreWaveOffset("ShoreWaveOffset", float) = 1
        _ShoreWaveFallOff("ShoreWaveFallOff", float) = 0
        _ShoreBorderDistance("ShoreBorderDistance", float) = 1
        _ShoreBorderNoiseTextureScale("ShoreBorderNoiseTextureScale", float) = 1
        _ShoreBorderNoiseScale("ShoreBorderNoiseScale", float) = 1
        _ShoreBorderNoiseSpeed("ShoreBorderNoiseSpeed", float) = 1
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
                float2 shoreUV : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _ShoreTex;
            float4 _ShoreTex_ST;
            float _ShoreWaveStep;
            float _ShoreWaveSize;
            float _ShoreWaveSpeed;
            float _ShoreWaveNoiseTextureScale;
            float _ShoreWaveNoiseScale;
            float _ShoreWaveNoiseSpeed;
            float _ShoreWaveOffset;
            float _ShoreWaveFallOff;
            float _ShoreBorderDistance;
            float _ShoreBorderNoiseTextureScale;
            float _ShoreBorderNoiseScale;
            float _ShoreBorderNoiseSpeed;

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
                o.shoreUV = TRANSFORM_TEX(v.uv, _ShoreTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            float noise(float2 uv, float time, float scale, float amplitude)
            {
                float2 offset1 = float2(time, time);
                float offset2 = float2(-time / 2 + 1, time * 1.53 - 1.37);

                float4 col1 = tex2D(_PerlinTex, uv * scale + offset1);
                float4 col2 = tex2D(_PerlinTex, uv * scale + offset2);

                return (col1.r - col2.r) * amplitude;
            }

            float noiseAdd(float2 uv, float time, float scale, float amplitude)
            {
                float2 offset1 = float2(time, time);
                float offset2 = float2(-time / 2 + 1, time * 1.53 - 1.37);

                float4 col1 = tex2D(_PerlinTex, uv * scale + offset1);
                float4 col2 = tex2D(_PerlinTex, uv * scale + offset2);

                return (col1.r + col2.r) * amplitude / 2;
            }

            float Shore(float2 uv, float2 worldPos, float time)
            {
                float n = noise(worldPos, time * _ShoreBorderNoiseSpeed, _ShoreBorderNoiseTextureScale, _ShoreBorderNoiseScale) + _ShoreBorderDistance;
                float v = tex2D(_ShoreTex, uv).r;
                float border = step(1 - v, n);

                float wn = noiseAdd(worldPos, time * _ShoreWaveNoiseSpeed, _ShoreWaveNoiseTextureScale, _ShoreWaveNoiseScale);
                float w = (1 + cos(time * _ShoreWaveSpeed + v * _ShoreWaveSize + wn )) / 2;
                
                float fallOff = _ShoreWaveFallOff + n;
                float offset = v - fallOff;
                offset = clamp(offset, 0, _ShoreWaveOffset);
                offset /= _ShoreWaveOffset;
                float wave = w * offset > _ShoreWaveStep;

                return max(border, wave);
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 lightUV = i.initialVertex.xz / _LightTexSize;
                lightUV.x = clamp(lightUV.x, 0, 1);
                lightUV.y = clamp(lightUV.y, 0, 1);

                float4 light = tex2D(_LightTex, lightUV);

                float noiseOffset = noise(lightUV, _NoiseTime, _NoiseTextureScale, _NoiseAmplitude);
                float minLight = 0.5 - _LightBaseRange / 2 + noiseOffset;
                float maxLight = 0.5 + _LightBaseRange / 2 + noiseOffset;
                light = (light - minLight) / (maxLight - minLight);
                light = clamp(light, 0, 1);

                float4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                col.r *= Shore(i.shoreUV, lightUV, _Time.x);

				float a = abs(dot(i.normal, float3(0, 1, 0))) * _LightTop;
				float b = abs(dot(i.normal, float3(1, 0, 0))) * _LightLeft;
				float c = abs(dot(i.normal, float3(0, 0, 1))) * _LightFront;

				float mul = sqrt(a*a + b * b + c * c);

				clip(col.a - 0.5);

                return float4((col * mul).r, _InvertColor, light.r, col.a);
            }
            ENDCG
        }
    }

    Fallback "Standard"
}
