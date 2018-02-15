Shader "Unlit/Heatmap"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

        SubShader
    {

    Tags{ "RenderType" = "Opaque" }

    CGPROGRAM
        #pragma target 3.0
        #pragma glsl
        #pragma surface surf Lambert vertex:vert

        struct Input {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;

        void vert(inout appdata_full v) {
            float4 tex = tex2Dlod(_MainTex, float4(v.texcoord.xy, 0, 0));
            v.vertex.z -= tex.r / 50;
        }

        void surf(Input IN, inout SurfaceOutput o) {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
        }

    ENDCG
    }
}
