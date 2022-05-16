Shader "Custom/ControllerPulse"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Tint("Tint", Color) = (1,1,0,1)
		_Toggle("Toggle", int) = -1
		_MainTex("MainTex", 2D) = "white" {}
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
			#include "AutoLight.cginc"
			#include "UnityPBSLighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 lightDir : TEXCOORD1;
				float3 normal : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				LIGHTING_COORDS(3, 4)
			};

			fixed4 _Color;
			fixed4 _Tint;
			float _Toggle;
			sampler2D _MainTex;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.lightDir = ObjSpaceLightDir(v.vertex);
				o.normal = v.normal;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET
			{
				//fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 col = _Color;
				if (_Toggle > 0)
				{
					float pos = (_Time.x * 10 % 1);
					float dist = length(i.uv.y - pos);
					col = lerp(float4(0, 0, 0, 0), _Tint, 0.2 - dist * 2);

				}
				return col;
			}
			ENDCG
		}
	}
}
