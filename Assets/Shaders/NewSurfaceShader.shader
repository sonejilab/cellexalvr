// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/NewSurfaceShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _GraphpointColorTex("Graphpoint Colors", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _GraphpointColorTex;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            float3 coords = LinearToGammaSpace(tex2D(_MainTex, IN.uv_MainTex));
            float2 colorTexUV = float2(coords.x + 1/512, 0.5);
            fixed4 c = tex2D(_GraphpointColorTex, colorTexUV);
            o.Albedo = c.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
