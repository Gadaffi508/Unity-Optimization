Shader "Hidden/SPD/DebugWorldPos"{
SubShader{ Tags{ "RenderType"="Opaque" } ZWrite On Cull Back
Pass{
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct appdata{ float4 vertex:POSITION; };
struct v2f{ float4 pos:SV_POSITION; float3 wp: TEXCOORD0; };
v2f vert(appdata v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.wp = mul(unity_ObjectToWorld, v.vertex).xyz; return o; }
fixed4 frag(v2f i):SV_Target{
    float3 c = frac(i.wp * 0.1); // 10 dünya biriminde sarmal renk
    return fixed4(c,1);
}
ENDCG
}}}