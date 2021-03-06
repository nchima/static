Shader "Doom/Default" 
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf SimpleSpecular vertex:vert fullforwardshadows
		#pragma target 3.0

		struct Input 
		{
			float2 uv_MainTex;
			float3 vertexColor;
		};

		struct v2f 
		{
			float4 pos : SV_POSITION;
			fixed4 color : COLOR;
		};

		struct SurfaceOutputCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			half Specular;
			fixed Gloss;
			fixed Alpha;
			fixed3 VertexColor;
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.vertexColor = v.color;
		}

		sampler2D _MainTex;

		fixed4 _Color;

		uniform half4 _AMBIENTLIGHT = half4(1, 1, 1, 1);
		
		half4 LightingSimpleSpecular(SurfaceOutputCustom s, half3 lightDir, half3 viewDir, half atten)
		{
			half4 c;

			half diff = max(0, dot(s.Normal, lightDir));

			c.rgb = s.Albedo * (_LightColor0.rgb * diff * atten + _AMBIENTLIGHT * s.VertexColor * _Color);
			c.a = s.Alpha;
			return c;
		}

		void surf(Input IN, inout SurfaceOutputCustom o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.VertexColor = IN.vertexColor;
			o.Alpha = c.a;
		}

		ENDCG
	}

	FallBack "Diffuse"
}