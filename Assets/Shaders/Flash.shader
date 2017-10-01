Shader "Unlit/Flash"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
		ZWrite Off
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
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
			
				clip(col.a - 1);

			    float3 COLOR_MASKS[8];
				COLOR_MASKS[0] = float3( 0.0, 0.0, 0.0 );
				COLOR_MASKS[1] = float3( 0.5, 0.5, 0.5 );
				COLOR_MASKS[2] = float3( 1.0, 0.5, 0.5 );
				COLOR_MASKS[3] = float3( 0.5, 1.0, 0.5 );
				COLOR_MASKS[4] = float3( 0.5, 0.5, 1.0 );
				COLOR_MASKS[5] = float3( 0.5, 1.0, 1.0 );
				COLOR_MASKS[6] = float3( 1.0, 0.5, 1.0 );
				COLOR_MASKS[7] = float3( 1.0, 1.0, 0.5 );
			    
			   
   				int index = int(fmod(_Time.y*60.0,7.0));
    			col.rgb = COLOR_MASKS[index];
        

				return col;
			}
			ENDCG
		}
	}
}
