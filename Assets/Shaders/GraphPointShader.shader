// UNITY_SHADER_NO_UPGRADE

Shader "Custom/GraphPointShader" 
{
	Properties 
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)

		_OutlineColor("Outline Color", Color) = (1, 0, 0, 1)
        _OutlineThickness("Thickness", float) = 1
	}

	SubShader 
	{
		Tags
		{
			"RenderType"="Opaque" 
			"RenderQueue"="Opaque"
		}

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
	
			fixed4 _Color;
	
			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			// UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
			// UNITY_INSTANCING_CBUFFER_END
	
			void surf (Input IN, inout SurfaceOutputStandard o) 
			{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
				// Albedo comes from a texture tinted by color
				o.Albedo = c.rgb;
				// Metallic and smoothness come from slider variables
	
				o.Alpha = c.a;
			}
		ENDCG

		// Fill the stencil buffer
     // Fill the stencil buffer
        Pass
        {
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
                ZFail Replace
            }

            ColorMask 0
        }

        // Draw the outline
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off // On (default) = Ignore lights etc. Should this be a property?
            Stencil
            {
                Ref 0
                Comp Equal
            }

            CGPROGRAM
                #pragma vertex vert
                #pragma geometry geom
                #pragma fragment frag

                #include "UnityCG.cginc"
                half4 _OutlineColor;
                float _Thickness;

                struct appdata
                {
                    float4 vertex : POSITION;
                };

                struct v2g
                {
                    float4 pos : SV_POSITION;
                };

                v2g vert(appdata IN)
                {
                    v2g OUT;
                    OUT.pos = UnityObjectToClipPos(IN.vertex);
                    return OUT;
                }

                void geom2(v2g start, v2g end, inout TriangleStream<v2g> triStream)
                {
                    float width = _Thickness / 100;
                    float4 parallel = (end.pos - start.pos) * width;
                    float4 perpendicular = normalize(float4(parallel.y, -parallel.x, 0, 0)) * width;
                    float4 v1 = start.pos - parallel;
                    float4 v2 = end.pos + parallel;
                    v2g OUT;
                    OUT.pos = v1 - perpendicular;
                    triStream.Append(OUT);
                    OUT.pos = v1 + perpendicular;
                    triStream.Append(OUT);
                    OUT.pos = v2 - perpendicular;
                    triStream.Append(OUT);
                    OUT.pos = v2 + perpendicular;
                    triStream.Append(OUT);
                }

                [maxvertexcount(12)]
                void geom(triangle v2g IN[3], inout TriangleStream<v2g> triStream)
                {
                    geom2(IN[0], IN[1], triStream);
                    geom2(IN[1], IN[2], triStream);
                    geom2(IN[2], IN[0], triStream);
                }

                half4 frag(v2g IN) : COLOR
                {
                    _OutlineColor.a = 1;
                    return _OutlineColor;
                }
            ENDCG
        }
	}
	FallBack "Diffuse"
}
