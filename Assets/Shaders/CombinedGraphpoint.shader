// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader used by the combined graphpoints.
// The idea is to encode rendering information in the main texture.
// The red channel chooses the gene color of the graphpoint, the values [0-x)
// (x is the number of available colors) chooses a color from the
// _ExpressionColors array. The value 255 is reserved for white.
// The green channel values determines the following;
// g == 0:
// 0   < g <= 0.1: pulsating with tint color
// 0.1 < g <= 0.2: shinier appearance (for highlighting)
// 0.2 < g <= 0.5: not used
// 0.5 < g <= 0.7 not used
// 0.7 < g <= 0.9 transparancy
// 0.9 < g <= 1  : party
// The blue channel values determines the following;
// b == 0: not used
// 0   < b <= 0.1: culling
// 0.1 < b <= 0.2: not used
// 0.2 < b <= 0.5: not used
// 0.5 < b <= 0.7 not used
// 0.7 < b <= 0.9 not used
// 0.9 < b <= 1  : not used

Shader "Custom/CombinedGraphpoint"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _GraphpointColorTex("Graphpoint Colors", 2D) = "white" {}
        _OutlineThickness("Thickness", float) = 0.005
        _OutlineColor("OutlineColor", Color) = (1, 1, 1, 1)
		_ThickerOutline("ThicknessMultiplier", float) = 4
        _TestPar("test", float) = 0
        _Transparancy("Transparancy", Range(0.0, 1.0)) = 0.1
        _Culling("Culling", float) = 1
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _Tint("Tint", Color) = (1,1,1,1)
        [Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
        _PulseSpeed ("PulseSpeed", float) = 1.0
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
//        ZWrite Off
        LOD 100
        
        Pass //Normal Render
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
                //HLSLINCLUDE
                //    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl" 
                //    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                //    #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
                //    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                //ENDHLSL
                
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag 
                
                #pragma target 4.5
                //#pragma exclude_renderers d3d11_9x
                #pragma multi_compile_fwdbase                       // This line tells Unity to compile this pass for forward base.
                #pragma multi_compile _ LOD_FADE_CROSSFADE
                //#pragma multi_compile_instancing
                //#pragma multi_compile _ DOTS_INSTANCING_ON
               
               
                #include "AutoLight.cginc"
                #include "UnityCG.cginc"
                //#include "UnityStandardBRDF.cginc"
                //#include "UnityStandardUtils.cginc"
                #include "UnityPBSLighting.cginc"
                
                struct vertex_input
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID //Insert                
                };
                
                
                struct v2f
                {
                    float4  pos         : SV_POSITION;
                    float2  uv          : TEXCOORD0;
                    float3  lightDir    : TEXCOORD1;
                    float3  normal		: TEXCOORD2;
                    float3  worldPos     : TEXCOORD3;
                    UNITY_VERTEX_OUTPUT_STEREO //Insert
                    LIGHTING_COORDS(3,4)                            // Macro to send shadow & attenuation to the vertex shader.
                };
                
                //float _OutlineThickness;
                //float _OutlineColor;
                //vertex_output vert (vertex_input v)
                v2f vert (vertex_input IN)
                {
                   v2f o;
                   UNITY_SETUP_INSTANCE_ID(IN); //Insert
                   UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert
                   UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert
                   o.pos = UnityObjectToClipPos(IN.vertex);
                   o.uv = IN.texcoord.xy;
                   o.lightDir = ObjSpaceLightDir(IN.vertex);
                   o.worldPos = mul(unity_ObjectToWorld, IN.vertex);
                   o.normal = IN.normal;
                   return o;
                }

                sampler2D _MainTex;
                sampler2D _GraphpointColorTex;
                float4 _MainTex_ST;
                //fixed4 _LightColor0;
                float _Transparancy;
                float _Cutoff;
                float4 _PlanePos;
                uniform float4x4 _BoxMatrix;
                uniform float4x4 _BoxMatrix2;
                float _Culling;
                half4 _Tint;
                float _Smoothness;
                float _Metallic;
                float _PulseSpeed;
                
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

                float clip_fragment(float inside_first, float inside_second, float blue_channel)
                {
                    if ((inside_first <= 0 || inside_second <= 0) && !(blue_channel > 0 && blue_channel < 0.1))
                        return -1;
                    return 1;
                }
                
                void ClipLOD (float2 positionCS, float fade) {
                	#if defined(LOD_FADE_CROSSFADE)
                		float dither = (positionCS.xy % 8) / 32;
                		//float dither = InterleavedGradientNoise(positionCS.xy, 0);
                		clip(fade + (fade < 0.0 ? dither : -dither));
                		//positionCS = positionCS % 189;
                        //float x = (34 * positionCS.x + 1) * positionCS.x % 189 + positionCS.y;
                        //x = (34 * x + 1) * x % 189;
                        //x = frac(x / 41) * 2 - 1;
                        //clip(normalize(float2(x - floor(x + 0.5), abs(x) - 0.5)));
                	#endif
                }

                UNITY_DECLARE_SCREENSPACE_TEXTURE(_ScreenTex); //Insert
                fixed4 frag(v2f i) : COLOR
                {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert
                    fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_ScreenTex, i.uv); //Insert
                    ClipLOD(i.pos.xy, unity_LODFade.x);
                    i.lightDir = normalize(i.lightDir);
                    fixed atten = LIGHT_ATTENUATION(i); // Macro to get you the combined shadow & attenuation value.

                    float3 expressionColorData = tex2D(_MainTex, i.uv);
                    // the 255.0 / 256.0 is there to shift the x-coordinate slightly to the left, otherwise the rightmost pixel (which in the _MainTex is red = 255) rolls over to the leftmost
                    float2 colorTexUV = float2(expressionColorData.x * 255.0/256.0, 0.5);

                    float4 color = tex2D(_GraphpointColorTex, colorTexUV);
                    fixed diff = saturate(dot(i.normal, i.lightDir));

                    float4 wpos = float4(i.worldPos.x, i.worldPos.y, i.worldPos.z, 1);
                    float4 relpos_box1 = mul(_BoxMatrix, wpos);
                    float4 relpos_box2 = mul(_BoxMatrix2, wpos);
                    float do_clip = clip_fragment(isInsideBox(relpos_box1), isInsideBox(relpos_box2), expressionColorData.b);
                    clip(do_clip*_Culling);    
                   

                   if (expressionColorData.g > 0.7 && expressionColorData.g < 0.9) //(colorTexUV.x == 254.0/255.0)
                   {
                       //color.rgb = (UNITY_LIGHTMODEL_AMBIENT.rgb * 15 * color.rgb);         // Ambient term. Only do this in Forward Base. It only needs calculating once.
                       color.rgb += (color.rgb * _LightColor0.rgb * diff) * (atten * 4); // Diffuse and specular.
                       color.a = _Transparancy;
                   }
                   
                   else if (expressionColorData.g > 0 && expressionColorData.g <= 0.2)
                   {
                   		i.normal = normalize(i.normal);
                   		float3 lightDir = _WorldSpaceLightPos0.xyz;
                   		float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                   		float3 lightColor = _LightColor0.rgb;
                   		float3 albedo = tex2D(_MainTex, i.uv).rgb;
                        float3 specularTint; // = albedo * _Metallic;
				        float oneMinusReflectivity;// = 1 - _Metallic;
                   		UnityLight light;
				        light.color = lightColor;
				        light.dir = lightDir;
				        light.ndotl = DotClamped(i.normal, lightDir);
				        UnityIndirect indirectLight;
				        indirectLight.diffuse = 0.25;
				        indirectLight.specular = 0.20;
				        //_Metallic = lerp(0.0, 1.0, (sin(_Time.w * _PulseSpeed) + 1) / 2);
				        //_Smoothness = lerp(0.0, 1.0, (sin(_Time.w * _PulseSpeed) + 1) / 2);
                   		albedo = DiffuseAndSpecularFromMetallic(albedo, _Metallic, specularTint, oneMinusReflectivity);
                   		color.rgb += UNITY_BRDF_PBS(albedo, specularTint, oneMinusReflectivity, _Smoothness, i.normal, viewDir, light, indirectLight);
                   }
                   
                   else 
                   {
                       //color.rgb = (UNITY_LIGHTMODEL_AMBIENT.rgb * 15 * color.rgb);         // Ambient term. Only do this in Forward Base. It only needs calculating once.
                       color.rgb += (color.rgb * _LightColor0.rgb * diff) * (atten * 4); // Diffuse and specular.
                       color.a = 1.0;
                   }
                   
                   if (expressionColorData.b > 0.1 && expressionColorData.b <= 0.2)
                   {
			           fixed3 tintedColor = lerp(color.a, _Tint.a, (sin(_Time.w * _PulseSpeed) + 1) / 2);
                       color.a *= tintedColor;
                   }
                    //color.a = color.a + _LightColor0.a * atten;
                   return color;
               }
               
                
               


           ENDCG
        }
    }
    Fallback "Diffuse"
}
