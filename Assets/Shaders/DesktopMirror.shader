// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.
Shader "Custom/Desktop Mirror" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_XGridSize ("XGridSize", int) = 20
		_YGridSize ("YGridSize", int) = 20
	    _OffsetX ("OffsetX", float) = 0
	    _OffsetY ("OffsetY", float) = 0
	    _OffsetZ ("OffsetZ", float) = 0
	    _Scale ("Scale", float) = 1
	}
	SubShader {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
        }
        Blend SrcAlpha OneMinusSrcAlpha 
        ZWrite Off
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert alpha fullforwardshadows vertex:vert 
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float3 desktopUV;
			float distance;
		};
		
		struct vertex_input {
		    float4 vertex : POSITION;
		    float4 color : COLOR;
		    float4 pos : SV_POSITION;
		    float3 worldPos : TEXCOORD3;
		    float3 normal : NORMAL;
		};
		
		
		//struct vertex_output {
		//    float4 vertex : POSITION;
		//    float4 pos : SV_POSITION;
		//    float3 worldPos : TEXCOORD3;
		//    float3 normal : NORMAL;
		//    float4 color : COLOR;
		//};

		half _Glossiness;
		half _Metallic;
		int _XGridSize;
		int _YGridSize;
		fixed4 _Color;
		half _OffsetX;
		half _OffsetY;
		half _OffsetZ;
		half _Scale;
		float _Alpha;
		float2 _MouseCursor;
		
		//vertex_output vert (inout vertex_input v) {
			//o.desktopUV.x = v.vertex.x / _XGridSize;
			//o.desktopUV.y = v.vertex.y / _YGridSize;
			//o.desktopUV = v.color.xy * 255 / _XGridSize;
			//vertex_output vo;
			
			//vo.color.xy = v.color.xy;
			
            //vo.pos = UnityObjectToClipPos(v.vertex);
            //o.lightDir = ObjSpaceLightDir(v.vertex);
            //vo.worldPos = mul(unity_ObjectToWorld, v.vertex);
            //vo.normal = v.normal;
            
			//o.desktopUV.a = 0.2f;
		//}
		
		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.desktopUV.x = (v.color.x * 255) / _XGridSize;
			o.desktopUV.y = (v.color.y * 255) / _YGridSize;
			
			//if (v.xy == _MouseCursor)
			//{
			//    o.desktopUV.xy = (0,0);
			//}
			
		    if (_Alpha > 0) {
    			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    			float val = sqrt(pow(worldPos.x - _OffsetX, 2) + pow(worldPos.y - _OffsetY, 2) + pow(worldPos.z - _OffsetZ, 2));
    			if (val < 0.1) {
    			    o.distance = val / 5;
    			}
    			else {
    			    o.distance = val;
    			}
		    }
		    else {
		        o.distance = 1;
		    }
		    
		    	
			//half offsetvert = (v.vertex.x * v.vertex.x) + (v.vertex.y * v.vertex.y);
			//half value = sin(_Time.w * 0.5 + offsetvert + (v.vertex.x * _OffsetX) + (v.vertex.z * _OffsetY));
			//v.vertex.x += value;
			
		}
		

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.desktopUV) * _Color;
			o.Albedo = c.rgb;
			//o.Metallic = _Metallic;
			//o.Smoothness = _Glossiness;
			//o.Alpha = IN.color.a;
			//half offsetvert = (IN.desktopUV.x * IN.desktopUV.x) + (IN.desktopUV.y * IN.desktopUV.y);
			//half offsetvert = (_OffsetX) + (_OffsetY);//IN.desktopUV.x * IN.desktopUV.x) + (IN.desktopUV.y * IN.desktopUV.y);
			//half value = offsetvert + (IN.desktopUV.x * _OffsetX) + (IN.desktopUV.y * _OffsetY);
			//o.Alpha += value;
			o.Alpha += IN.distance;
			
		}
		ENDCG
	} 
	FallBack "Diffuse"
}

