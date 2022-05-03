Shader "Custom/SlowPulse"
{
    Properties
    {
        _PulseGradient("Pulse Gradient", 2D) = "white" {}
        _MainColor("Main Color", Color) = (0, 0, 0, 1)
        _TimeScaleFactor("Time Scale Factor", float) = 1
        _CoordinateFactor("Coordinate Factor", float) = 1
        _PulseLength("Pulse Length", float) = 10
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _PulseGradient;
            fixed4 _MainColor;
            float _TimeScaleFactor;
            float _CoordinateFactor;
            float _PulseLength;

            v2f vert(Input IN)
            {
                v2f OUT;
                OUT.position = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.uv;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_TARGET
            {
                float timeFactor = _Time.x * _TimeScaleFactor;
                float coordinateFactor = IN.uv.x * _CoordinateFactor;
                float pulseIntensity = clamp(sin(timeFactor + coordinateFactor) * (_PulseLength + 1) - _PulseLength, 0, 1);
                fixed4 pulseColor = tex2D(_PulseGradient, fixed2(0, frac(IN.uv.y+ _Time.y/8)));
                fixed4 color = lerp(_MainColor, pulseColor, pulseIntensity);
                return color;
            }

            ENDCG
        }
    }
}
