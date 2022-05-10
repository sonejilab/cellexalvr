Shader "Custom/MenuCube"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_FrontColor("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Position("Position", float) = 1.0
	}

		SubShader
	{
		Pass
		{
		LOD 200

		CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _Color;
		float4 _FrontColor;
		float _Position;

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
			float3 localPosition : TEXCOORD1;
		};


		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.localPosition = v.vertex.xyz;
			o.uv = v.uv;
			return o;
		}

		fixed4 frag(v2f o) : SV_TARGET
		{
			float time = sin(_Time.y * 2);
			fixed4 texCol = tex2D(_MainTex, o.uv);
			fixed4 mainCol = texCol *_Color;
			fixed4 pulseCol = texCol * _FrontColor;
			float dist = abs(o.localPosition.y - time);
			fixed4 col = lerp(mainCol, pulseCol, (1-dist));
			return col;
		}

		ENDCG
			}

	}
}
