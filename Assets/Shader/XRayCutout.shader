Shader "Custom/XRayOnlySelected"
{
    Properties{
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _XrayColor("XRay Color", Color) = (0.2,0.9,0.85,1)

        _EdgeStrength("Edge Strength", Range(0,3)) = 1.2
        _HoleAlpha("Hole Alpha (min)", Range(0,0.3)) = 0.06

        // (Opsiyonel) Kenar dalgası
        _WaveAmp("Edge Wave Amplitude", Range(0,0.5)) = 0.18
        _WaveFreq("Edge Wave Frequency", Range(0.1,10)) = 3.0
        _WaveSpeed("Edge Wave Speed", Range(0,10)) = 2.0
        _WaveWidth("Edge Wave Width", Range(0.01,0.5)) = 0.12

        // TÜM YÜZEY nabız/dalga
        _PulseAmp("Global Pulse Alpha", Range(0,1)) = 0.35
        _PulseBulge("Global Pulse Bulge", Range(0,0.3)) = 0.05
        _PulseSpeed("Global Pulse Speed", Range(0,10)) = 2.0
    }
    SubShader
    {
        Tags{ "RenderType"="Transparent" "Queue"="Transparent+10" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor, _XrayColor;
        float  _EdgeStrength, _HoleAlpha;
        float  _WaveAmp, _WaveFreq, _WaveSpeed, _WaveWidth;
        float  _PulseAmp, _PulseBulge, _PulseSpeed;
        CBUFFER_END

        // URP zamanı: _TimeParameters.y = t
        float3 _XR_RayOrigin, _XR_RayDir;
        float  _XR_RayLength, _XR_ConeRadiusEnd, _XR_Softness;

        struct A{ float4 positionOS:POSITION; float3 normalOS:NORMAL; };
        struct V{
            float4 positionCS:SV_POSITION;
            float3 ws:TEXCOORD0;
            float3 n:TEXCOORD1;
            float  t:TEXCOORD2;
            float  d:TEXCOORD3;
            float  rad:TEXCOORD4;
        };

        // 0=delik içi, 1=delik dışı
        float RayMask(float3 Pws, out float t, out float d, out float rad)
        {
            float3 ro=_XR_RayOrigin, rd=normalize(_XR_RayDir);
            float3 v=Pws-ro;
            t = saturate( dot(v,rd) / max(dot(rd,rd),1e-4) ); // 0..1
            float3 closest = ro + rd*(t*_XR_RayLength);
            rad = lerp(0.0, _XR_ConeRadiusEnd, t);
            d = distance(Pws, closest);
            return smoothstep(rad, rad - max(_XR_Softness, 1e-3), d); // 0=delik,1=dış
        }

        float Fresnel(float3 n, float3 posWS){
            float3 V = normalize(_WorldSpaceCameraPos - posWS);
            return pow(1.0 - saturate(dot(normalize(n), V)), 4.0);
        }

        // Kenar zarfı (1=kenarda, 0=uzakta)
        float EdgeEnvelope(float d, float rad, float width)
        {
            float x = abs(d - rad);
            float e = 1.0 - smoothstep(0.0, width, x);
            return saturate(pow(e, 0.6));
        }

        V vert(A i){
            V o;
            float3 pws = TransformObjectToWorld(i.positionOS.xyz);
            float3 nws = TransformObjectToWorldNormal(i.normalOS);

            // Ray tabanlı değerler (kenar için lazım)
            float t, d, rad;
            float mask = RayMask(pws, t, d, rad);

            // --- GLOBAL BULGE: tüm yüzeyde nefes alma ---
            float pulse = sin(_TimeParameters.y * _PulseSpeed); // -1..1
            float bulge = _PulseBulge * pulse;                  // -amp..amp
            pws += nws * bulge;

            // (İsteğe bağlı) kenarda ekstra kabarma da istiyorsan uncomment:
            // float env  = EdgeEnvelope(d, rad, _WaveWidth);
            // pws += nws * (env * 0.5 * _PulseBulge * pulse);

            o.positionCS = TransformWorldToHClip(pws);
            o.ws = pws;
            o.n  = nws;
            o.t  = t;   o.d = d;   o.rad = rad;
            return o;
        }
        ENDHLSL

        // Ön yüz (deliği aç + global pulse + opsiyonel kenar dalgası)
        Pass{
            Name "Frontfaces"
            Cull Back
            ZTest LEqual
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            V vert(A i);
            float4 frag(V i):SV_Target{
                float t=i.t, d=i.d, rad=i.rad;
                float mask = RayMask(i.ws, t, d, rad);
                float f = Fresnel(i.n, i.ws);

                // Kenar dalgası (opsiyonel)
                float env   = EdgeEnvelope(d, rad, _WaveWidth);
                float edgeWave = sin((_TimeParameters.y * _WaveSpeed) + d * _WaveFreq) * _WaveAmp * env;

                // Global pulse: tüm yüzeyde alpha nefes alır
                float pulse = 0.5 + 0.5 * sin(_TimeParameters.y * _PulseSpeed); // 0..1
                float pulseMul = lerp(1.0 - _PulseAmp, 1.0, pulse);             // 1-amp .. 1

                float alpha = lerp(_HoleAlpha, 1.0, mask);       // delik içi şeffaf, dışı tam
                alpha = max(alpha, f * _EdgeStrength);           // kenarı koru
                alpha = saturate(alpha * pulseMul + edgeWave);   // global + kenar dalgası

                float4 col = _BaseColor;
                col.a = alpha;
                return col;
            }
            ENDHLSL
        }

        // İç yüz (x-ray rengi + global pulse + opsiyonel kenar dalgası)
        Pass{
            Name "Backfaces"
            Cull Front
            ZTest LEqual
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            V vert(A i);
            float4 frag(V i):SV_Target{
                float t=i.t, d=i.d, rad=i.rad;
                float mask = RayMask(i.ws, t, d, rad);
                if (1.0 - mask <= 0.001) discard; // sadece delikte

                float env   = EdgeEnvelope(d, rad, _WaveWidth);
                float edgeWave = sin((_TimeParameters.y * _WaveSpeed) + d * _WaveFreq) * _WaveAmp * env;

                float pulse = 0.5 + 0.5 * sin(_TimeParameters.y * _PulseSpeed);
                float pulseMul = lerp(1.0 - _PulseAmp, 1.0, pulse);

                float f = Fresnel(-i.n, i.ws);
                float4 c = _XrayColor;
                c.rgb += f * 0.4;
                c.a = saturate((0.45 + f*0.55) * pulseMul + edgeWave);
                return c;
            }
            ENDHLSL
        }
    }
}
