// Shader used by the combined graphpoints.
// The idea is to encode rendering information in the main texture.
// The red channel chooses the gene color of the graphpoint, the values [0-x)
// (x is the number of available colors) chooses a color from the
// _ExpressionColors array. The value 255 is reserved for white.
// The green channel is 0 for no outline, 1 for outline.

Shader "Custom/CombinedGraphpoint" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (1, 0, 0, 1)
        _Thickness("Thickness", float) = 0.005
    }

    SubShader {
        Tags {
            "Queue" = "Geometry"
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
            "ForceNoShadowCasting" = "True"
        }

        // graphpoint pass
        Pass {
            Tags { 
                "LightMode" = "ForwardBase"
            }
            // LOD 200
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            struct v2g {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            fixed4 _Color;
            sampler2D_float _MainTex;
            float4 _ExpressionColors[256];

            half4 _OutlineColor;
            float _Thickness;

            v2g vert (in appdata_base IN) {
                v2g o;
                UNITY_INITIALIZE_OUTPUT(v2g, o);
                float4 uvAndMip = float4(IN.texcoord.x, IN.texcoord.y, 0, 0);
                float3 expressionColorData = LinearToGammaSpace(tex2Dlod(_MainTex, uvAndMip));
                // float3 expressionColorData = LinearToGammaSpace(_Color);
                o.color = _ExpressionColors[round(expressionColorData.x * 255)];

                o.pos = UnityObjectToClipPos(IN.vertex);
                return o;
            }

            fixed4 frag(v2g IN) : SV_TARGET {
                return IN.color;
            } 
            ENDCG
        }

        // Fill the stencil buffer
        Pass {
            Stencil {
                Ref 1
                Comp Always
                Pass Replace
                ZFail Replace
            }
            ColorMask 0
        }

        // outline pass
        Pass {

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On // On (default) = Ignore lights etc. Should this be a property?
            Stencil
            {
                Ref 0
                Comp Equal
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "UnityCG.cginc"


            half4 _OutlineColor;
            float _Thickness;
            sampler2D_float _MainTex;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD;
            };

            struct v2g
            {
                float4 pos : SV_POSITION;
                float3 color : COLOR;
            };

            v2g vert(in appdata_base IN)
            {
                v2g OUT;
                OUT.pos = UnityObjectToClipPos(IN.vertex);
                float4 uvAndMip = float4(IN.texcoord.x, IN.texcoord.y, 0, 0);
                OUT.color = tex2Dlod(_MainTex, uvAndMip);
                // OUT.color = _Color;
                return OUT;
            }

            void geom2(v2g start, v2g end, inout TriangleStream<v2g> triStream)
            {
                float width = _Thickness;// / 100;
                float4 parallel = (end.pos - start.pos) * width;
                float4 perpendicular = normalize(float4(parallel.y, -parallel.x, 0, 0)) * width;
                float4 v1 = start.pos - parallel;
                float4 v2 = end.pos + parallel;
                v2g OUT;
                OUT.color = start.color;
                OUT.pos = v1 - perpendicular;
                triStream.Append(OUT);
                OUT.pos = v1 + perpendicular;
                triStream.Append(OUT);
                OUT.pos = v2 - perpendicular;
                triStream.Append(OUT);
                OUT.pos = v2 + perpendicular;
                triStream.Append(OUT);
            }

            [maxvertexcount(8)]
            void geom(triangle v2g IN[3], inout TriangleStream<v2g> triStream)
            {
                if (!IN[0].color.g == 1) {
                    return;
                }

                geom2(IN[0], IN[1], triStream);
                geom2(IN[1], IN[2], triStream);
                geom2(IN[2], IN[0], triStream);
            }

            fixed4 frag(v2g i) : COLOR {
                _OutlineColor.a = 1;
                return _OutlineColor;
                // return fixed4(0,1,1,1);
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
