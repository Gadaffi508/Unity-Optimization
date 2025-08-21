Shader "Hidden/SPD/DebugNormals"{
SubShader{ Tags{ "RenderType"="Opaque" } ZWrite On Cull Back
Pass{
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct appdata{ float4 vertex:POSITION; float3 normal:NORMAL; };
struct v2f{ float4 pos:SV_POSITION; float3 n: TEXCOORD0; };
v2f vert(appdata v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.n = UnityObjectToWorldNormal(v.normal); return o; }
fixed4 frag(v2f i):SV_Target{ return fixed4(0.5 + 0.5*normalize(i.n), 1); }
ENDCG
}}}