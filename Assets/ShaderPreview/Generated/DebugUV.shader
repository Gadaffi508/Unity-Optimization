Shader "Hidden/SPD/DebugUV"{
SubShader{ Tags{ "RenderType"="Opaque" } ZWrite On Cull Back
Pass{
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct appdata{ float4 vertex:POSITION; float2 uv:TEXCOORD0; };
struct v2f{ float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
v2f vert(appdata v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
fixed4 frag(v2f i):SV_Target{ return fixed4(frac(i.uv),0,1); }
ENDCG
}}}