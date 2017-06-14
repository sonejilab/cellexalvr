Shader "Depth Mask" {
	Properties
	{
		_MainTex("Base (RGB) Alpha (A)", 2D) = "white" {}
		_Cutoff("Base Alpha cutoff", Range(0,.9)) = .5
	}
	SubShader{
		Tags{ "Queue" = "Geometry-10" }
		Lighting Off
		ZTest Always
		ZWrite On
		ColorMask 0
		Pass{
			AlphaTest GEqual[_Cutoff]
			Blend SrcAlpha OneMinusSrcAlpha
			SetTexture[_MainTex]{
				combine texture * primary, texture
			}
		}
	}
}