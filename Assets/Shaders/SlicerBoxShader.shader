Shader "Custom/SlicerBoxShader"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_BaseColor("Color", Color) = (1, 0.5, 0, 0.5)
		_WaveCoords("Wave Coords", Vector) = (0.5, 0.5, 0, 0)
		_WaveColor("Wave Color", Color) = (1, 1, 1, 1)
		_WaveToggle("Wave Toggle", int) = 1
		_WaveAxis("Wave Axis", int) = 1
	}

		SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderPipeline" = "UniversalPipeline"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100
		ZWrite Off
		Tags
		{
			"LightMode" = "UniversalForward"
		}

		HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		CBUFFER_START(UnityPerMaterial)
			float4 _BaseColor;
			uniform float4x4 _BoxMatrix;
			float3 _WaveCoords;
			float4 _WaveColor;
			int _WaveToggle;
			int _WaveAxis;
		CBUFFER_END

		TEXTURE2D(_MainTex);
		SAMPLER(sampler_MainTex);

		struct VertexInput
		{
			float4 position : POSITION;
			float2 uv : TEXCOORD0;

			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct VertexOutput
		{
			float4 position : SV_POSITION;
			float2 uv	: TEXCOORD0;

			float3 worldPos : TEXCOORD1;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		ENDHLSL
		Pass //Normal Render
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			VertexOutput vert(VertexInput i)
			{
				VertexOutput o;

				UNITY_SETUP_INSTANCE_ID(i);
				#if defined(UNITY_COMPILER_HLSL)
				#define UNITY_INITIALIZE_OUTPUT(type,name) name = (type)0;
				#else
				#define UNITY_INITIALIZE_OUTPUT(type,name)
				#endif
				UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.position = TransformObjectToHClip(i.position.xyz);
				o.worldPos = TransformObjectToWorld(i.position);
				o.uv = i.uv;
				return o;
			}


			//UNITY_DECLARE_SCREENSPACE_TEXTURE(_ScreenTex); //Insert
			float4 frag(VertexOutput i) : SV_TARGET
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert
				float4 col = _BaseColor;
				float4 textureCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				if (_WaveToggle > 0)
				{
					float4 wpos = float4(i.worldPos.x, i.worldPos.y, i.worldPos.z, 1);
					float4 relPos = mul(_BoxMatrix, wpos);
					float dist;
					if (_WaveAxis == 0)
					{
						dist = abs(relPos.x - _WaveCoords.x);
					}
					if (_WaveAxis == 1)
					{
						dist = abs(relPos.y - _WaveCoords.y);
					}
					if (_WaveAxis == 2)
					{
						dist = abs(relPos.z - _WaveCoords.z);
					}

					col = lerp(col, _WaveColor, (1-dist));
				}
				col.a = textureCol.r;
				return col;
			}

			ENDHLSL
		}
	}
}

