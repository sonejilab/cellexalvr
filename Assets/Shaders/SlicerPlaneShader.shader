Shader "Custom/SlicerPlaneShader"
{
	Properties
	{
		_BaseColor("Color", Color) = (1, 0.5, 0, 0.5)
		_SliceOffset("Toggle", float) = 0.0
	}

		SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderPipeline" = "UniversalPipeline"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Tags
		{
			"LightMode" = "UniversalForward"
		}

		HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		CBUFFER_START(UnityPerMaterial)
			float4 _BaseColor;
			uniform float4x4 _BoxMatrix;
			float _SliceOffset;
		CBUFFER_END


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
				o.uv = i.uv;
				o.worldPos = TransformObjectToWorld(i.position);
				return o;
			}


			float isInsideBox(float4 pos)
			{
				if (pos.x < -.5 || pos.x > .5)
					return 1;
				if (pos.y < -.5 || pos.y > .5)
					return 1;
				if (pos.z < -.5 || pos.z > .5)
					return 1;
				return -1;
			}

			float clip_fragment(float inside_first)
			{
				if (inside_first >= 0)
					return -1;
				return 1;
			}

			//UNITY_DECLARE_SCREENSPACE_TEXTURE(_ScreenTex); //Insert
			float4 frag(VertexOutput i) : SV_TARGET
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert
				float4 col = _BaseColor;
				float4 wpos = float4(i.worldPos.x, i.worldPos.y, i.worldPos.z, 1);
				float4 relpos_box1 = mul(_BoxMatrix, wpos);
				float do_clip = clip_fragment(isInsideBox(relpos_box1));
				clip(do_clip);
				float xPos = abs(relpos_box1.x);
				float yPos = abs(relpos_box1.y);
				float zPos = abs(relpos_box1.z);
				float threshold = 0.49 - _SliceOffset;
				if (xPos < threshold && yPos < threshold && zPos < threshold)
				{
					col.a = 0.05;
				}
				else
				{
					col.a = 1;
				}
				return col;
			}

			ENDHLSL
		}
	}
		Fallback "Diffuse"
}

//#pragma target 4.5
//#pragma vertex vert
//#pragma fragment frag 
//#pragma multi_compile_fwdbase                       // This line tells Unity to compile this pass for forward base.
//#pragma multi_compile _ LOD_FADE_CROSSFADE


//#include "AutoLight.cginc"
//#include "UnityCG.cginc"
//#include "UnityPBSLighting.cginc"

//struct vertex_input
//{
//	float4 vertex : POSITION;
//	float3 normal : NORMAL;
//	float4 color : COLOR;
//	UNITY_VERTEX_INPUT_INSTANCE_ID //Insert  
//};

//struct vertex_output
//{
//	float4  pos         : SV_POSITION;
//	float2  uv          : TEXCOORD0;
//	float3  lightDir    : TEXCOORD1;
//	float3  normal		: TEXCOORD2;
//	float3  worldPos     : TEXCOORD3;
//	UNITY_VERTEX_OUTPUT_STEREO //Insert
//	LIGHTING_COORDS(3,4)                            // Macro to send shadow & attenuation to the vertex shader.
//};

//vertex_output vert(vertex_input v)
//{
//	vertex_output o;
//	UNITY_SETUP_INSTANCE_ID(v); //Insert
//	UNITY_INITIALIZE_OUTPUT(vertex_output, o); //Insert
//	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert
//	o.pos = UnityObjectToClipPos(v.vertex);
//	o.uv.x = (v.color.x * 255);
//	o.uv.y = (v.color.y * 255);
//	o.lightDir = ObjSpaceLightDir(v.vertex);
//	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
//	o.normal = v.normal;

//	return o;
//}


//sampler2D _MainTex;
//sampler2D _GraphpointColorTex;
//float4 _MainTex_ST;
////fixed4 _LightColor0;
//float _Transparancy;
//float _Cutoff;
//float4 _PlanePos;
//uniform float4x4 _BoxMatrix;
//uniform float4x4 _BoxMatrix2;
//float _Culling;
//half4 _Tint;
//float _Smoothness;
//float _Metallic;
//float _PulseSpeed;
//float4 _Color;


//float isInsideBox(float4 pos)
//{
//	if (pos.x < -.5 || pos.x > .5)
//		return 1;
//	if (pos.y < -.5 || pos.y > .5)
//		return 1;
//	if (pos.z < -.5 || pos.z > .5)
//		return 1;
//	return -1;
//}

//float clip_fragment(float inside_first)
//{
//	if (inside_first >= 0)
//		return -1;
//	return 1;
//}

//UNITY_DECLARE_SCREENSPACE_TEXTURE(_ScreenTex); //Insert
//fixed4 frag(vertex_output i) : COLOR
//{
//	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert
//	fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_ScreenTex, i.uv); //Insert
//	float4 wpos = float4(i.worldPos.x, i.worldPos.y, i.worldPos.z, 1);
//	float4 relpos_box1 = mul(_BoxMatrix, wpos);
//	float do_clip = clip_fragment(isInsideBox(relpos_box1));
//	clip(do_clip);
//	col = _Color;
//	float xPos = abs(relpos_box1.x);
//	float yPos = abs(relpos_box1.y);
//	float zPos = abs(relpos_box1.z);
//	if (xPos < 0.49 && yPos < 0.49 && zPos < 0.49)
//	{
//		col.a = 0.05;
//	}
//	else
//	{
//		col.a = 1;
//	}
//	return col;
//}