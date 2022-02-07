Shader "Tutorial/048_Instancing"
{
	//show values to edit in inspector
	Properties
	{
		[PerRendererData] _Color("Color", Color) = (0, 0, 0, 1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		//the material is completely non-transparent and is rendered at the same time as the other opaque geometry
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
			"IgnoreProjector" = "True"
			"RenderPipeline" = "UniversalPipeline"
			
		}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Tags {"LightMode" = "UniversalForward"}
			CGPROGRAM
			//allow instancing
			#pragma target 4.5
			#pragma multi_compile_instancing
			#pragma multi_compile_fwdbase 
			#pragma multi_compile_shadowcaster

			//shader functions
			#pragma vertex vert
			#pragma fragment frag

			//use unity shader library
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			//#include "UnityStandardBRDF.cginc"
			//#include "UnityStandardUtils.cginc"
			#include "UnityPBSLighting.cginc"

			//per vertex data that comes from the model/parameters
			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			//per vertex data that gets passed from the vertex to the fragment function
			struct v2f
			{
				float4 position : SV_POSITION;
				float2  uv          : TEXCOORD0;
				float3  lightDir    : TEXCOORD1;
				float3  normal		: TEXCOORD2;
				float3  worldPos     : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO //Insert
				LIGHTING_COORDS(3, 4)
			};

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
			UNITY_INSTANCING_BUFFER_END(Props)

			sampler2D _MainTex;

			v2f vert(appdata v)
			{
				v2f o;

				//setup instance id

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				//calculate the position in clip space to render the object
				o.uv = v.texcoord.xy;
				o.position = UnityObjectToClipPos(v.vertex);
				o.lightDir = ObjSpaceLightDir(v.vertex);
				o.normal = v.normal;
				return o;
			}

			//UNITY_DECLARE_SCREENSPACE_TEXTURE(_ScreenTex); //Insert
			fixed4 frag(v2f i) : COLOR
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert
				//fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_ScreenTex, i.uv); //Insert
				//setup instance id
				UNITY_SETUP_INSTANCE_ID(i);
				fixed4 color = fixed4(tex2D(_MainTex, i.uv.xy).rgb * UNITY_ACCESS_INSTANCED_PROP(Props, _Color).rgb, 1); //UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				//color.rgb /= 10;
				i.lightDir = normalize(i.lightDir);
				fixed diff = saturate(dot(i.normal, i.lightDir));
				fixed atten = LIGHT_ATTENUATION(i);
				//get _Color Property from buffer
				//Return the color the Object is rendered in
				color.rgb += (color.rgb * _LightColor0.rgb * diff) *(atten * 4); // Diffuse and specular.
				//color.rgb = float3(1, 0, 0);
				//color.a = _Transparancy;
				return color;
			}

		ENDCG
		}
	}
}