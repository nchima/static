﻿Shader "Custom/ZWriteOnly"
{
	Properties
	{
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		LOD 200

		Pass
		{
			ZWrite On
			Cull Off
			ColorMask 0
		}
	}
}