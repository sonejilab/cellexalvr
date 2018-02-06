// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/ConvexHull" {
	Properties {
		_Color ("Main Color", Color) = (1,.9,.8,.9)
		_Emission ("Emmisive Color", Color) = (0,0,0,0)
        _Shininess ("Shininess", Range (0.01, 1)) = 0.01
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

	}
	SubShader {
	Material {
			Shininess [_Shininess]
            // Specular [_SpecColor]
            Emission [_Emission]
	}
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		// LOD 200
		
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		/*Pass {
			ZWrite On
			ColorMask 0
		}*/

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		 #pragma surface surf Lambert alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		
		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o) {
			
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = tex2D (_MainTex, IN.uv_MainTex).a;
			// Metallic and smoothness come from slider variables
		}
		ENDCG
	}
	FallBack "Diffuse"
}
