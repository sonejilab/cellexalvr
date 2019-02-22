Shader "Custom/ColorGradientThreeColors" {
	Properties {
		_Color1 ("Color 1", Color) = (1,1,1,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Color3 ("Color 3", Color) = (1,1,1,1)
		_NColors("Number of colors", int) = 3
	}

	SubShader {
		Pass{
			Tags { "RenderType"="Opaque" }
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
					float4 texcoord : TEXCOORD0;
				};

				struct v2f 
				{
					float4 pos : POSITION;
					float2 uv : TEXCOORD0;
				};

				fixed4 _Color1;
				fixed4 _Color2;
				fixed4 _Color3;
				int _NColors;

				v2f vert(Input IN)
				{
					v2f OUT;
					OUT.pos = UnityObjectToClipPos(IN.vertex);
					OUT.uv = IN.texcoord;
					return OUT;
				}

				fixed4 frag(v2f IN) : SV_TARGET
				{
					int halfNColors = _NColors / 2;
					int otherNHalfColors = _NColors - halfNColors;
					int block = floor(IN.uv.y * _NColors);
					if (block < halfNColors)
					{
						float divider = halfNColors;
						float areaUV = (block / divider);
						float4 finalColor = lerp(_Color1, _Color2, areaUV);
						return finalColor;
					}
					else
					{
						block -= halfNColors;
						float divider = otherNHalfColors - 1;
						float areaUV = (block / divider);
						float4 finalColor = lerp(_Color2, _Color3, areaUV);
						return finalColor;
					}
				}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
