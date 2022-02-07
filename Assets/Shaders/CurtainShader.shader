Shader "Custom/CurtainShader"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		_XGridSize("XGridSize", int) = 40
		_YGridSize("YGridSize", int) = 10

		_MainColor("MainColor", Color) = (1,1,1,1)
		_GradientColor("GradientColor", Color) = (1,1,1,1)
		_StripeColor("StripeColor", Color) = (1,1,1,1)
		_BottomStripe("BottomStripe", Range(0,1)) = 0

		_Clip("Clip", Range(0,1)) = 1
		_LightPoint("LightPointPosition", Vector) = (0, 0, 0, 0)
		_LightColor("LightColor", Color) = (0, 0, 0, 0)
	}

	SubShader
	{
		Cull Off
		Pass
		{
			//Tags {"LightMode" = "ForwardBase"}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			//#include "UnityLightingCommon.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				fixed4 diff : COLOR0;
				float3 worldNormal : TEXCOORD1;
				float3 worldPosition : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			int _XGridSize;
			int _YGridSize;
			float4 _MainColor;
			float4 _GradientColor;
			float4 _StripeColor;
			float _Clip;
			float _BottomStripe;
			float4 _LightPoint;
			float4 _LightColor;

			v2f vert(appdata v)
			{
				v2f o;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.uv.x = (v.color.x * 255) / _XGridSize;
				o.uv.y = (v.color.y * 255) / _YGridSize;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//half nl = max(0, dot(o.worldNormal, _WorldSpaceLightPos0.xyz));
				//o.diff = nl * _LightColor0;
				//o.diff.rgb += ShadeSH9(half4(o.worldNormal, 1));
				o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET
			{
				//fixed4 result = tex2D(_MainTex, i.uv);
				if (i.uv.y < _Clip)
				{
					clip(-1.0);
				}
				fixed4 bottomStripe = _StripeColor;
				fixed4 result = _MainColor;
				//result = lerp(_GradientColor, result, i.uv.y);

				//if (i.uv.y < _BottomStripe)
				//{
				//	bottomStripe = lerp(bottomStripe, _GradientColor, i.uv.y * 20);
				//	result = bottomStripe;
				//}
				if (i.uv.y > 0.97)
				{
					bottomStripe = lerp(result, bottomStripe, i.uv.y * 2);
					result = bottomStripe;
				}

				fixed3 lightDifference = i.worldPosition - _LightPoint.xyz;
				fixed3 lightDirection = normalize(lightDifference);
				fixed intensity = 2 * dot(lightDirection, i.worldNormal);
				result *= pow(intensity, 6) * _LightColor; 
				return result;
			}
			ENDCG
		}
	}
}