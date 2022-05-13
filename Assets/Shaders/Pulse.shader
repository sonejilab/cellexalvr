Shader "Custom/Pulse"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Tint("Tint", Color) = (1,1,0,1)
		_Toggle("Toggle", int) = -1
	}
	SubShader
	{
		Pass
		{

			Tags { "RenderType" = "Transparent" }
			LOD 200

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			fixed4 _Color;
			fixed4 _Tint;
			float _Toggle;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f o) : SV_TARGET
			{
				fixed4 col = _Color;
				if (_Toggle > 0)
				{
					float pos = (_Time.x * 10 % 1);
					float dist = length(o.uv.y - pos);
					col = lerp(_Color, _Tint, 0.2 - dist * 2);
				}
				return col;
			}
			ENDCG
		}
	}
}
