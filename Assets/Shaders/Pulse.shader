Shader "Custom/Pulse"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Tint("Tint", Color) = (1,1,0,1)
		_PulseSpeed("Pulse Speed", float) = 1
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
			float _PulseSpeed;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f o) : SV_TARGET
			{
				fixed4 col;
				float intensity = clamp(sin(_Time.y), 0, 1);
				col = lerp(_Color, _Tint, intensity);
				return col;
			}
			ENDCG
		}
	}
}
