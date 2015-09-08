//  This file is part of the Structure SDK.
//  Copyright © 2015 Occipital, Inc. All rights reserved.
//  http://structure.io

Shader "StructureAR/Wireframe"
{
	SubShader
	{
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off
			Fog { Mode Off }
			BindChannels
			{
				Bind "vertex", vertex
				Bind "color", color
			}
		}
	}
}
