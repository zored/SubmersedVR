Shader "Hidden/ZoredPdaHandReflectionMask" {
 Properties { _Color ("Color", Color) = (1,1,1,1) _HandCenter ("Center", Vector) = (0,0,0,1) _HandRadius ("Radius", Float) = 0.2 }
 SubShader { Tags { "RenderType"="Opaque" } Pass {
 Cull Off ZWrite On CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 fixed4 _Color; float4 _HandCenter; float _HandRadius;
 struct v2f { float4 pos:SV_POSITION; float3 world:TEXCOORD0; };
 v2f vert(float4 v:POSITION) { v2f o; o.pos=UnityObjectToClipPos(v); o.world=mul(unity_ObjectToWorld,v).xyz; return o; }
 fixed4 frag(v2f i):SV_Target { clip(_HandRadius-distance(i.world,_HandCenter.xyz)); return _Color; }
 ENDCG } } Fallback Off
}