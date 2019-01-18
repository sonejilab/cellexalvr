Shader "Custom/CircleClip"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags 
		{ 
			"RenderType"="Opaque"
		}
		LOD 200

		CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma vertex vert
			#pragma fragment frag

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			struct Input
			{
				float4 pos : POSITION;
			};

			struct v2f
			{
				float4 pos : POSITION;
			};

			fixed4 _Color;

			v2f vert (Input IN)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(IN.pos);
				return OUT;
			}

			fixed4 frag(v2f) : SV_TARGET
			{
				return _Color;
			}

		ENDCG
	}
	FallBack "Diffuse"
}
