Shader "Custom/Static Pulse"
{
	Properties
	{
		_MainColor("Main Color", Color) = (1,1,1,1)
		_PulseColor("Pulse Color", Color) = (1,1,0,1)
	}
	SubShader
	{
		Tags 
		{ 
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
			"RenderPipeline" = "UniversalPipeline"
		}

		LOD 200
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : POSITION;
			};

			fixed4 _MainColor;
			fixed4 _PulseColor;

			v2f vert(appdata v)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(v.vertex);
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_TARGET
			{
				float time = (sin(_Time.z) + 1) / 2;
				fixed4 color = lerp(_MainColor, _PulseColor, time);
				return color;
			}
			ENDCG
		}
	}
}
