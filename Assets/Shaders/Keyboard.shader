Shader "Custom/Keyboard"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _PulseColor("Pulse Color", Color) = (1,1,1,1)
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
                
            };
            
            struct v2f{
                float4 position : POSITION;
            };

            fixed4 _Color;

            v2f vert (Input IN)
            {
                v2f OUT;
                OUT.position = UnityObjectToClipPos(IN.vertex);
                return OUT;
            }

            fixed4 frag (v2f IN) : SV_TARGET
            {
                return _Color;
            }

            ENDCG
        }
    }
}