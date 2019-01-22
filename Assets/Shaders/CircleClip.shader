Shader "Custom/CircleClip"
{
    Properties
	{
        _Color ("Color", Color) = (1,1,1,1)
        _OuterClipRadius("Outer Clip Radius", float) = 1.000056
        _InnerClipRadius("Inner Clip Radius", float) = 1.000062
    }

    SubShader 
    {
        Pass {
            Tags
            {
                "Queue" = "Geometry"
                "RenderType"="Opaque"
            }
            LOD 200
            Cull Off

            CGPROGRAM
                // Physically based Standard lighting model, and enable shadows on all light types
                #pragma vertex vert
                #pragma fragment frag

                // Use shader model 3.0 target, to get nicer looking lighting
                #pragma target 3.0

                struct vertex
                {
                    float4 pos : POSITION;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float4 vertexPos : TEXCOORD0;
                };

                fixed4 _Color;
                float _OuterClipRadius;
                float _InnerClipRadius;

                v2f vert (vertex IN)
                {
                    v2f OUT;
                    OUT.pos = UnityObjectToClipPos(IN.pos);
                    OUT.vertexPos = IN.pos;
                    return OUT;
                }

                fixed4 frag(v2f IN) : SV_TARGET
                {
                    float radius = length(IN.vertexPos);
                    // clip(radius - _OuterClipRadius);
                    // clip(_InnerClipRadius - radius);
                    fixed4 color = _Color;
                    return color;
                }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
