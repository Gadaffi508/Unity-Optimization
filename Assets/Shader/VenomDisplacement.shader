
Shader "Custom/VenomDisplacement"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _NoiseScale("Noise Scale", Float) = 5
        _DisplacementStrength("Displacement Strength", Float) = 0.5
        _Speed("Animation Speed", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _NoiseScale;
            float _DisplacementStrength;
            float _Speed;
            float4 _MainTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float noise(float3 p)
            {
                return frac(sin(dot(p.xyz, float3(12.9898, 78.233, 45.164))) * 43758.5453);
            }

            float3 displace(float3 position, float3 normal, float time)
            {
                float3 worldPos = position * 0.2f * _NoiseScale + time * _Speed * 0.001;
                float n = noise(worldPos);
                return position + normal * (n * _DisplacementStrength) * 0.2f;
            }

            v2f vert(appdata v)
            {
                v2f o;
                float time = _Time.y * 0.2f;
                float3 displaced = displace(v.vertex.xyz, v.normal, time);
                o.pos = UnityObjectToClipPos(float4(displaced, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
