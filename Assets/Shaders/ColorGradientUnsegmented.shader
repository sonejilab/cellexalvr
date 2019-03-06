Shader "Custom/ColorGradientUnsegmented" {
	Properties {
		_Color1 ("Color 1", Color) = (0,0,1,1)
		_Color2 ("Color 2", Color) = (1,1,0,1)
		_Color3 ("Color 3", Color) = (1,0,0,1)
		_MinVal("Minimum Value", Range(0, 1)) = 0
		_MaxVal("Maximum Value", Range(0, 1)) = 1
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
					float2 uv : TEXCOORD0;
				};

				struct v2f 
				{
					float4 pos : POSITION;
					float2 uv : TEXCOORD0;
				};

				fixed4 _Color1;
				fixed4 _Color2;
				fixed4 _Color3;
				float _MinVal;
				float _MaxVal;

				v2f vert(Input IN)
				{
					v2f OUT;
					OUT.pos = UnityObjectToClipPos(IN.vertex);
					OUT.uv = IN.uv;
					return OUT;
				}

				fixed4 frag(v2f IN) : SV_TARGET
				{
					float value = (IN.uv.y) * (_MaxVal - _MinVal) + _MinVal;
					fixed4 color;
					if (value < 0.5)
					{
						// value is in the range (0, 0.5)
						color = lerp(_Color1, _Color2, value * 2);
					}
					else
					{
						// value is in the range (0.5, 1)
						color = lerp(_Color2, _Color3, value * 2 - 1);
					}
					return color;
				}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
