
Shader "Custom/UnderwaterWobble"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaveStrength ("Wave Strength", Float) = 0.02
        _WaveFrequency ("Wave Frequency", Float) = 10.0
        _WaveSpeed ("Wave Speed", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _WaveStrength;
            float _WaveFrequency;
            float _WaveSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _WaveSpeed;
                float2 uv = i.uv;
                uv.y += sin(uv.x * _WaveFrequency + time) * _WaveStrength;
                uv.x += cos(uv.y * _WaveFrequency + time * 0.5) * _WaveStrength * 0.5;
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
