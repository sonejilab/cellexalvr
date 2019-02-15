Shader "UI/SatValBox"
{
    Properties
    {
        _Hue("Hue", Range(0, 0.999999)) = 0
    }

    SubShader
    {
        Pass 
        {
            Tags 
            {
                "RenderType" = "Opaque"
                "Queue" = "Geometry"
                // "DisableBatching" = "true"
            }

            CGPROGRAM
                #pragma target 3.0
                #pragma vertex vert 
                #pragma fragment frag
                
                #include "UnityCG.cginc"

                float _Hue;

                struct Input {
                    float4 vertex : POSITION;
                    float4 texcoord : TEXCOORD0;
                };

                struct v2f 
                {
                    float4 pos : POSITION;
                    float2 uv : TEXCOORD0;
                };

                float3 hsv_to_rgb(float3 HSV)
                {
                    float3 RGB = HSV.z;
            
                    float var_h = HSV.x * 6;
                    float var_i = floor(var_h);
                    float var_1 = HSV.z * (1.0 - HSV.y);
                    float var_2 = HSV.z * (1.0 - HSV.y * (var_h-var_i));
                    float var_3 = HSV.z * (1.0 - HSV.y * (1-(var_h-var_i)));
                    if      (var_i == 0) { RGB = float3(HSV.z, var_3, var_1); }
                    else if (var_i == 1) { RGB = float3(var_2, HSV.z, var_1); }
                    else if (var_i == 2) { RGB = float3(var_1, HSV.z, var_3); }
                    else if (var_i == 3) { RGB = float3(var_1, var_2, HSV.z); }
                    else if (var_i == 4) { RGB = float3(var_3, var_1, HSV.z); }
                    else                 { RGB = float3(HSV.z, var_1, var_2); }
                
                    return (RGB);
                }

                v2f vert(Input IN)
                {
                    v2f OUT;
                    OUT.pos = UnityObjectToClipPos(IN.vertex);
                    OUT.uv = IN.texcoord.xy;
                    return OUT;
                }

                fixed4 frag(v2f IN) : SV_TARGET
                {
                    float3 hsv = float3(_Hue, (IN.uv.x), (IN.uv.y));

                    return fixed4(GammaToLinearSpace(hsv_to_rgb(hsv).xyz), 1);
                }

            ENDCG
        }
    }
    Fallback "Diffuse"
}
