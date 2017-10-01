Shader "Custom/ZWriteOnly"
{
	Properties
	{
	}

	SubShader
	{
		Tags{ "Queue" = "Opaque" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		LOD 200

		Pass
		{
			ZWrite On
			ColorMask 0
		}
	}
}	