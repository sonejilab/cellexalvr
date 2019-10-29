Shader "Custom/Kanelbulle"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (1,1,1,1)
        _Color2 ("Color 2", Color) = (1,1,1,1)
        _Threshold ("Threshold", float) = 0.25
        
    }

    SubShader
    {
        Pass
        {
            
            Tags
            {
                "RenderQueue" = "Opaque"
            }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct Input
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
            };

            fixed4 _Color1;
            fixed4 _Color2;
            float _Threshold;

            v2f vert(Input IN)
            {
                v2f OUT;
                OUT.position = UnityObjectToClipPos(IN.vertex);
                OUT.normal = IN.normal;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_TARGET
            {
                float normalDotObjectZ = dot(IN.normal, float3(0, 0, 1));
                float lengthsMul = length(IN.normal); // times 1
                float cosAngle = saturate(normalDotObjectZ / lengthsMul);
                return lerp(_Color2, _Color1, cosAngle);  
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}