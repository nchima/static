Shader "Custom Effects/Screen Blur"
{
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader {
		// No culling or depth
		Cull Off
		ZWrite Off
		ZTest Always

		GrabPass {
            "_PR"
        }

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float nrand(float x, float y) {
               return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
            }
            
            sampler2D _MainTex;
            sampler2D _CameraMotionVectorsTexture;
            sampler2D _PR;
            int _Button;

			
			//sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target {
				float2 uvr=round(i.uv*400)/400;

               float n = nrand(_Time.x,uvr.x+uvr.y*_ScreenParams.x);
               //noise float per UV block, changes over time

               float4 mot = tex2D(_CameraMotionVectorsTexture,uvr);

               #if UNITY_UV_STARTS_AT_TOP
               float2 mvuv = float2(i.uv.x-mot.r,1-i.uv.y+mot.g);
               #else
               float2 mvuv = float2(i.uv.x-mot.r,i.uv.y-mot.g);
               #endif
               //fixed4 col = lerp(tex2D(_MainTex,i.uv), tex2D(_PR, mvuv), lerp(round(1-(n)/1.4),1,1));
			   fixed4 col = lerp(tex2D(_MainTex,i.uv), tex2D(_PR, mvuv), 1);
               //button@0=lossy w/ noise, button@1=total loss
               col += mot;

               return col;
			}
			ENDCG
		}
	}
}