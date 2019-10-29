Shader "Custom/Velocity"
{
	Properties
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_VelocityData("Velocity Data", 2D) = "white" {}
		_GraphPointPosData("Graph Point Pos Data", 2D) = "white" {}
        _VelocityT("Velocity t", Range(0, 1)) = 0.0 // how far the velocity has gone so far
	}
	SubShader
	{
		Pass
		{
			Tags 
			{ 
				"RenderType" = "Transparent" 
			}
			LOD 200
			Cull Off

			CGPROGRAM
				// Physically based Standard lighting model, and enable shadows on all light types
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct vertex
				{
					float4 position : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f 
				{
					float4 position : SV_POSITION;
					float2 uv : TEXCOORD0;
				};
				
				sampler2D _MainTex;
				sampler2D _VelocityData;
				sampler2D _GraphPointPosData;

				v2f vert(vertex IN)
				{ 
					v2f OUT;
					OUT.position = UnityObjectToClipPos(IN.position);
					OUT.uv = IN.uv;
					
					return OUT;
				}

				fixed4 frag(v2f IN) : SV_TARGET
				{
					fixed4 color = tex2D(_VelocityData, IN.uv);
					
					return color;
				}

			ENDCG
		}
    }
    Fallback "Diffuse"
}
