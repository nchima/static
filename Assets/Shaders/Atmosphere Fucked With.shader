Shader "Custom/Atmosphere Fucked With" {
     Properties {
         _Color ("Color", Color) = (1,1,1,1)
		 _SecondColor ("Second Color", Color) = (0, 0, 0, 1)
         _Size ("Atmosphere Size Multiplier", Range(0,16)) = 4
         _Rim ("Fade Power", Range(0,8)) = 4
		 _MainTex ("Main Texture", 2D) = "white" {}
     }
     SubShader {
         Tags { "RenderType"="Transparent" }
         LOD 200
 
         Cull Front
         
         CGPROGRAM
         // Physically based Standard lighting model, and enable shadows on all light types
         #pragma surface surf Lambert fullforwardshadows alpha:fade
         #pragma vertex vert
 
         // Use shader model 3.0 target, to get nicer looking lighting
         #pragma target 3.0
 
 
         struct Input {
             float3 viewDir;
			 float2 uv_MainTex;
         };
 
         half _Size;
         half _Rim;
         fixed4 _Color;
		 fixed4 _SecondColor;
 
         void vert (inout appdata_full v) {
             v.vertex.xyz += v.vertex.xyz * _Size / 10;
             v.normal *= -1;
         }

		 float nrand(float2 n)
		 {
			 float scale = 0.1;
			 return frac(sin(dot(n.xy, float2(12.9898, 78.233)))* 43758.453) * (1+scale) - scale;
		 }
 
         void surf (Input IN, inout SurfaceOutput o) {
             half rim = saturate (dot (normalize (IN.viewDir), normalize (o.Normal)));
 
             // Albedo comes from a texture tinted by color
             fixed4 c = _Color;
             o.Emission = c.rgb;
			 o.Alpha = 1;
             half testy = lerp (0, 1, pow (rim, _Rim));
			 if(testy - nrand(IN.uv_MainTex) < -0.1) {
				clip (testy + 0.5 - nrand(IN.uv_MainTex));
				o.Emission = _SecondColor;
			 }
			 
         }
         ENDCG
     }
     FallBack "Diffuse"
 }
 