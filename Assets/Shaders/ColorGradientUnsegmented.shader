Shader "Custom/ColorGradientUnsegmented" {
	Properties {
		_Color1 ("Color 1", Color) = (1,1,1,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
	}

	SubShader {
		Pass{
			Tags 
			{ 
				"RenderType" = "Opaque" 
				"DisableBatching" = "True"
			}
			LOD 200

			CGPROGRAM
				// Physically based Standard lighting model, and enable shadows on all light types
				#pragma vertex vert
				#pragma fragment frag

				// Use shader model 3.0 target, to get nicer looking lighting
				#pragma target 3.0
				#include "UnityCG.cginc"

				struct Input {
					float4 vertex : POSITION;
					float4 color : COLOR;
				};

				struct v2f 
				{
					float4 pos : POSITION;
					float4 color : COLOR;
				};

				fixed4 _Color1;
				fixed4 _Color2;

				v2f vert(Input IN)
				{
					v2f OUT;
					OUT.pos = UnityObjectToClipPos(IN.vertex);
					OUT.color =  lerp(_Color1, _Color2, (IN.vertex.z + 1) / 2);
					return OUT;
				}

				fixed4 frag(v2f IN) : SV_TARGET
				{
					// float4 finalColor = lerp(_Color1, _Color2, (IN.originalPos.z + 1) / 2);
					// float4 finalColor = float4(GammaToLinearSpace(lerp(_Color1, _Color2, (IN.originalPos.z + 1) / 2)), 1);
					return IN.color;
				}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
