// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/Dennis2" {
	Properties {
		
		[PerRendererData] _Color ("Color", Color) = (1,1,1,1)
		[PerRendererData] _Emission ("Emission", Color) = (0,0,0,0)
		[PerRendererData] _Explode ("Explode", float) = 0
		[PerRendererData] _RandomSeed ("Random Seed", Range(0,10)) = 0
		[PerRendererData] _TextureOffset ("Texture Offset", Vector) = (0,0,0,0)
		_ExplodeDistance ("Explode Distance", Range(0.1, 100)) = 2.0
		_ExplodeCutoff ("Explode Alpha Cutoff", Range(0,1)) = 0.5
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NoiseTex ("Noise (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		LOD 200
		Cull Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert alpha:fade
		#include "UnityCG.cginc"
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		            

		sampler2D _MainTex;
		sampler2D _NoiseTex;
		float4 _NoiseTex_ST;
		struct Input {
			float2 uv_MainTex : TEXCOORD0;
			float4 color : COLOR;
		};

		half _Glossiness;
		half _Metallic;
		float _ExplodeDistance;
		float _ExplodeCutoff;


		// This adds instancing support for this shader so we can batch all the objects. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Emission)
			UNITY_DEFINE_INSTANCED_PROP(float, _Explode)
			UNITY_DEFINE_INSTANCED_PROP(float, _RandomSeed)
			UNITY_DEFINE_INSTANCED_PROP(float4, _TextureOffset)
		UNITY_INSTANCING_CBUFFER_END

		//just generates a nice Perlin noise from a texture. It's offset using the _RandomSeed instance property (hopefully different for every object).
		float noise( in float3 x )
		{
		    x += float3(UNITY_ACCESS_INSTANCED_PROP(_RandomSeed),0,0);
		    float3 p = floor(x);
		    float3 f = frac(x);
			f = f*f*(3.0-2.0*f);
			float2 uv = TRANSFORM_TEX((p.xy+float2(37.0,17.0)*p.z) + f.xy,_NoiseTex); 
			uv += UNITY_ACCESS_INSTANCED_PROP(_TextureOffset).xy;
			float2 rg = tex2Dlod( _NoiseTex, float4((uv+float2(0.5,0.5))/1.0,0,0)).yx;
			return lerp( rg.x, rg.y, f.z );
		
		}
	
		void vert (inout appdata_full v) {
			float3 oldPos = v.vertex.xyz; 

			//extrude the verts along the surface normal, using the noise texture and the _Explode and _ExplodeDistance properties
        	v.vertex.xyz += v.normal * pow(noise(float3(v.vertex.xyz)),1)*UNITY_ACCESS_INSTANCED_PROP(_Explode)*_ExplodeDistance;

        	//measure how far we moved the vertex... we'll use this to set the pixel transparent for verts that moved a long way
        	float dist = length(oldPos-v.vertex.xyz)/_ExplodeCutoff;
        	v.color = float4(1,1,1, 1-clamp(dist,0,1)); //store the distance in the vertex color, for convenience
     	}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			
			if (IN.color.a < 0.5) discard; //don't draw the pixel if it got moved a lot by the explosion 

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(_Color);
			o.Albedo = c.rgb; //use the per-instance color and the main texture to set the albedo color

			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;

			o.Emission = UNITY_ACCESS_INSTANCED_PROP(_Emission); //set the emission color using the material property block
			 
			o.Alpha =  c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
