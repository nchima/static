Shader "Unlit/Flash And Clip Black Pixels"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FlashColor1 ("Flash Color 1", Color) = (1, 1, 1, 1)
		_FlashColor2 ("Flash Color 2", Color) = (1, 1, 1, 1)
		_FlashColor3 ("Flash Color 3", Color) = (1, 1, 1, 1)
		_FlashColor4 ("Flash Color 4", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
		ZWrite On
		Cull Off

		Pass
		{
			//Stencil
			//{
			//	Ref 1
			//	Comp Equal
			//}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 _FlashColor1;
			fixed4 _FlashColor2;
			fixed4 _FlashColor3;
			fixed4 _FlashColor4;

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
			
				clip(col.a - 1);
				clip((col.r + col.g + col.b) - 0.01);

			    float3 COLOR_MASKS[4];
				COLOR_MASKS[0] = _FlashColor1;
				COLOR_MASKS[1] = _FlashColor2;
				COLOR_MASKS[2] = _FlashColor3;
				COLOR_MASKS[3] = _FlashColor4;
			    
   				int index = int(fmod(_Time.y*60.0, 4.0));
    			col.rgb = COLOR_MASKS[index];
        
				return col;
			}
			ENDCG
		}
	}
}
