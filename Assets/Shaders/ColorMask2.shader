Shader "Custom/ColorMask2"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color Mask", Color) = (0, 1, 0, 1)
		_AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
	}

	SubShader
	{
		Tags { "RenderType"="AlphaTest" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			float nrand(float2 n)
			{
				float scale = 0.1;
				return frac(sin(dot(n.xy, float2(12.9898, 78.233)))* 43758.453) * (1+scale) - scale;
			}

			fixed4 _Color;
			float _AlphaCutoff;
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				
				col.rgb *= _Color.rgb;

				clip(((col.r + col.g + col.b) / 3) - ((_Color.r + _Color.g + _Color.b) / 3));
				clip(((col.r + col.g + col.b) / 3) - _AlphaCutoff);
				clip(col.a - _AlphaCutoff);

				//clip(col.r - (_Color.r + nrand(i.uv)));
				//clip(col.g - (_Color.g + nrand(i.uv)));
				//clip(col.b - (_Color.b + nrand(i.uv)));
				//clip(col.a - (_Color.a + nrand(i.uv)));

				col.r = _Color.r * 2;
				col.g = _Color.g * 2;
				col.b = _Color.b * 2;
				col.a = _Color.a * 2;

				//col = _Color;

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}
			ENDCG
		}
	}
}
