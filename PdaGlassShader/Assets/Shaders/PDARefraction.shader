Shader "Zored/PDARefraction"
{
    Properties
    {
        _Color ("Glass Tint", Color) = (0.44, 0.76, 0.86, 0.22)
        _MainTex ("Original Surface", 2D) = "black" {}
        _Illum ("Original Emission", 2D) = "black" {}
        _Strength ("Refraction Strength", Range(0, 0.03)) = 0.008
        [HideInInspector] _ReflectionLeft ("Reflection Left", 2D) = "black" {}
        [HideInInspector] _ReflectionRight ("Reflection Right", 2D) = "black" {}
        [HideInInspector] _ReflectionEnabled ("Reflection Enabled", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+20" "RenderType"="Transparent" }
        GrabPass { }
        Pass
        {
            Cull Back
            ZWrite Off
            ZTest LEqual
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            fixed4 _Color;
            sampler2D _MainTex;
            sampler2D _Illum;
            float _Strength;
            float4 _FaceBounds, _Ripple0, _Ripple1, _Ripple2, _Ripple3;
            float _RippleTime;
            sampler2D _ReflectionLeft, _ReflectionRight;
            float _ReflectionEnabled;
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_GrabTexture);
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 grabPos : TEXCOORD0;
                float3 viewNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float2 face : TEXCOORD3;
                float4 reflectionPos : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.reflectionPos = ComputeNonStereoScreenPos(o.pos);
                o.viewNormal = mul((float3x3)UNITY_MATRIX_V, UnityObjectToWorldNormal(v.normal));
                o.uv = v.uv;
                o.face = (v.vertex.xz - _FaceBounds.xy) / max(_FaceBounds.zw, float2(0.001, 0.001));
                return o;
            }
            float Ripple(float2 pos, float4 wave)
            {
                float age = max(0.0, _RippleTime - wave.z);
                float r = length(pos - wave.xy);
                float width = saturate(1.0 - abs(r - age * 0.32) * 15.0);
                float ring = width * width * (3.0 - 2.0 * width);
                return ring * saturate(1.0 - age * 0.9) * wave.w;
            }
            float Chip(float2 p, float2 center, float2 size)
            {
                float2 q = abs((p - center) / size);
                return 1.0 - smoothstep(0.65, 1.0, q.x + q.y * 0.8);
            }
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 uv = i.grabPos.xy / i.grabPos.w;
                float3 normal = normalize(i.viewNormal);
                float2 waves = float2(sin(i.uv.x * 14.0 + sin(i.uv.y * 8.0)) + 0.45 * sin(i.uv.y * 19.0), cos(i.uv.y * 13.0 + sin(i.uv.x * 7.0)) + 0.45 * cos(i.uv.x * 17.0));
                float2 face = i.face;
                float edge = min(min(face.x, 1.0 - face.x), min(face.y, 1.0 - face.y));
                float fade = smoothstep(0.0, 0.12, edge);
                float ripple = (Ripple(face, _Ripple0) + Ripple(face, _Ripple1) + Ripple(face, _Ripple2) + Ripple(face, _Ripple3)) * fade;
                float2 offset = (normal.xy + waves * 0.18) * _Strength + waves * ripple * 0.0035;
                float2 blurredUV = uv + offset;
                float2 blur = float2(0.0012, 0.0012);
                fixed3 background = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture, blurredUV).rgb * 0.4;
                background += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture, blurredUV + float2(blur.x, 0)).rgb * 0.15;
                background += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture, blurredUV - float2(blur.x, 0)).rgb * 0.15;
                background += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture, blurredUV + float2(0, blur.y)).rgb * 0.15;
                background += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture, blurredUV - float2(0, blur.y)).rgb * 0.15;
                background *= 0.86;
                fixed3 tint = lerp(background, background * _Color.rgb, saturate(_Color.a));
                fixed3 surface = tex2D(_MainTex, i.uv).rgb;
                fixed3 emission = tex2D(_Illum, i.uv).rgb;
                float fresnel = pow(1.0 - saturate(abs(normal.z)), 3.0);
                float chip = Chip(face, float2(0.035, 0.24), float2(0.012, 0.018));
                chip += Chip(face, float2(0.96, 0.72), float2(0.01, 0.014));
                chip += Chip(face, float2(0.28, 0.965), float2(0.015, 0.009));
                tint += surface * .04 + emission * .08 + fresnel * .06 + chip * 0.07 + ripple * 0.055;
                float2 reflectedUV = i.reflectionPos.xy / i.reflectionPos.w;
                fixed4 reflection;
                #if defined(UNITY_SINGLE_PASS_STEREO) || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
                    if (unity_StereoEyeIndex == 0) reflection = tex2D(_ReflectionLeft, reflectedUV) * 0.5 + (tex2D(_ReflectionLeft, reflectedUV + float2(0.007,0)) + tex2D(_ReflectionLeft, reflectedUV - float2(0.007,0))) * 0.25;
                    else reflection = tex2D(_ReflectionRight, reflectedUV) * 0.5 + (tex2D(_ReflectionRight, reflectedUV + float2(0.007,0)) + tex2D(_ReflectionRight, reflectedUV - float2(0.007,0))) * 0.25;
                #else
                    reflection = tex2D(_ReflectionLeft, reflectedUV) * 0.5 + (tex2D(_ReflectionLeft, reflectedUV + float2(0.007,0)) + tex2D(_ReflectionLeft, reflectedUV - float2(0.007,0))) * 0.25;
                #endif
                tint = lerp(tint, reflection.rgb, saturate(reflection.a * _ReflectionEnabled));
                return fixed4(tint, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
