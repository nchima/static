Shader "Custom/ColorMaskBlack"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BrightnessCutoff ("Brightness Cutoff", Range(0, 1)) = 0.5
		_FinalColor ("Final Color", Color) = (0, 0, 0, 1)
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

			float _BrightnessCutoff;
			fixed4 _FinalColor;
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				
				float brightness = 0.299*col.r + 0.587*col.g + 0.114*col.b;
				clip(_BrightnessCutoff - (1 - brightness));
				col = _FinalColor;

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}
			ENDCG
		}
	}
}
