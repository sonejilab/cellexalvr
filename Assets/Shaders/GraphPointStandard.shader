// This is the shader used to render graphpoints. It is basically a very slimmed version Unity's standard shader.

Shader "Custom/GraphPointStandard" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" { }
        _Color ("Main Color", Color) = (0.5, 0.5, 0.5, 1)
    }

    SubShader {
        Tags {
            "Queue" = "Geometry"
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
            "ForceNoShadowCasting" = "True"
        }
        Pass {
        Tags { "LightMode" = "ForwardBase" }
            //Blend One One
            //Fog { Color (0,0,0,0) }
            ZWrite On
            ZTest LEqual
            CGPROGRAM
                #pragma target 3.0
                #pragma vertex vertBase
                #pragma fragment fragBase
                #include "UnityStandardCoreForward.cginc"
            ENDCG
        }

        Pass {
        Tags { "LightMode" = "ForwardAdd" }
            Blend One One
            Fog { Color (0,0,0,0) }
            ZWrite On
            ZTest LEqual
            CGPROGRAM
                #pragma target 3.0
                #pragma vertex vertAdd
                #pragma fragment fragAdd
                #include "UnityStandardCoreForward.cginc"
            ENDCG
        }
    }
    Fallback "Diffuse"
}
