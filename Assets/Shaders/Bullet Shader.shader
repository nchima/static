// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Bullet Shader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Tint("Tint", Color) = (1, 1, 1, 1)
		_AlphaCutoff("Alpha Cutoff", float) = 0.5
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "TransparentCutout"
		}

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Tint;
			float _AlphaCutoff;

			struct Interpolators
			{
				float4 position : SV_POSITION;
				float3 localPosition : TEXCOORD0;
				float2 uv : TEXCOORD1;
				float4 uvProj : TEXCOORD2;
			};

			struct VertexData
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD1;
			};

			Interpolators MyVertexProgram(VertexData v)
			{
				Interpolators i;
				i.localPosition = v.position.xyz;
				i.position = UnityObjectToClipPos(v.position);

				i.uv = TRANSFORM_TEX(v.uv, _MainTex);
				i.uvProj.xy = TRANSFORM_TEX(i.position, _MainTex);
				i.uvProj.zw = i.position.zw;

				return i;
			}

			float4 MyFragmentProgram(Interpolators i) : SV_TARGET
			{
				//float rand = tex2D(_MainTex, float2(i.localPosition.x, i.localPosition.y));
				//return float4(rand, rand, rand, rand);

				i.uvProj /= i.uvProj.w;
				i.uvProj = (i.uvProj + 1) * 0.5;

				half4 color = tex2D(_MainTex, i.uvProj.xy);
				clip (color.a - _AlphaCutoff);

				float brightness = (color.x + color.y + color.z) / 3;
				if (brightness < _AlphaCutoff) discard;
				else color = half4(1, 1, 1, 1);

				return color * _Tint;
			}

			ENDCG
		}
	}
}