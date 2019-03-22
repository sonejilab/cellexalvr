Shader "Custom/Pulse"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Tint("Tint", Color) = (1,1,0,1)
		_PulseSpeed("Pulse Speed", float) = 1
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _Tint;
		float _PulseSpeed;

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			fixed3 tintedColor = lerp(_Color.rgb, _Tint.rgb, (sin(_Time.w * _PulseSpeed) + 1) / 2);
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * fixed4(tintedColor, 1);
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
