Shader "Custom/GeneExpressionGradient" {
    Properties{
        _LowColor("Low Color", Color) = (0.0, 0.0, 1.0)
        _MedColor("Medium Color", Color) = (1.0, 1.0, 0.0)
        _HighColor("High Color", Color) = (1.0, 0.0, 0.0)
    }
        SubShader{
            Tags{ "RenderType" = "Opaque" }
            //LOD 200
            Pass{
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc" 

                uniform float3 _LowColor;
                uniform float3 _MedColor;
                uniform float3 _HighColor;
                float _Expressions[100];

                struct v2f {
                    float4 vertex : SV_POSITION;
                    float4 texcoord : TEXCOORD0;
                };

                v2f vert(appdata_full v) {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = v.texcoord;

                    return o;
                }

                fixed4 frag(v2f i) : SV_TARGET{
                    float expr = _Expressions[i.texcoord.y * 100];
                    fixed4 finalcolor;
                    if (expr < 0.5)
                    {
                        finalcolor = fixed4(lerp(_LowColor, _MedColor, expr * 2.0), 1);
                    }
                    else
                    {
                        finalcolor = fixed4(lerp(_MedColor, _HighColor, (expr - 0.5) * 2.0), 1);
                    }
                    return finalcolor;
                }

                ENDCG
            }
    }
        FallBack "Diffuse"
}
