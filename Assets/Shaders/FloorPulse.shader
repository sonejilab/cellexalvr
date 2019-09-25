Shader "Custom/FloorPulse"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _RotationTex("Rotating Texture", 2D) = "white" {}
        _Tint("Tint Color", Color) = (1, 1, 1, 1)
        _FadedTex("Faded Texture", 2D) = "white" {}
        _LaserHitColor("Laser Hit Color", Color) = (1, 1, 1, 1)
        _FadeRadius("Texture fade radius", float) = 5.0
        _WaveColor("Wave Color", Color) = (1, 1, 1, 1)
		_WaveCoords("Wave and Laser Coords", Vector) = (0.5, 0.5, 0, 0)
		_WaveStartTime("Wave Start Time", float) = 0.0
        _WaveSpeed("Wave Speed", float) = 1.0
        _WaveDuration("Wave Duration", float) = 1.0
        _WaveMaxRadius("Wave Max Radius", float) = 1.0
        _WaveThickness("Wave Thickness", float) = 0.1

        _PulseToggle("Pulse Toggle", float) = 0.0
        _PulseSpeed("Pulse Speed", float) = 1.0
        _PulseInnerRadius("Pulse Inner Radius", float) = 0.01
        _PulseOuterRadius("Pulse Outer Radius", float) = 0.057
        _PulseFade("Pulse Fade Thickness", float) = 0.057
        _PulseRotationSpeed("Pulse Rotation Speed", float) = 0.0

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

            sampler2D _MainTex;
            sampler2D _RotationTex;
            fixed4 _Tint;
            fixed4 _WaveColor;
            float _WaveSpeed;
            float _WaveMaxRadius;
            float _WaveDuration;
            float _WaveThickness;
            // goes from 0 to _PulseDuration when a pulse plays
            float _WaveStartTime;
            // xy is the uv2 coords that the last pulse played at, zw is the uv2 coords of the last laser hit
            float4 _WaveCoords;
            float _PulseFade;

            float _PulseToggle;
            float _PulseSpeed;
            float _PulseInnerRadius;
            float _PulseOuterRadius;
            float _PulseRotationSpeed;

            v2f vert (Input IN)
            {
                v2f OUT;
                OUT.position = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.uv;
                return OUT;
            }

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

            fixed4 frag(v2f IN) : SV_TARGET
            {
                fixed4 finalColor = tex2D(_MainTex, IN.uv) * _Tint;
                float waveTime = _WaveStartTime;
                float pulseToggle = _PulseToggle;
                if (waveTime < _WaveDuration)
                {
                    float dist = length(IN.uv - _WaveCoords.xy);
                    // the distances to the inner and outer radii
                    float innerRadiusDist = dist / _WaveMaxRadius - waveTime;
                    float outerRadiusDist = innerRadiusDist - _WaveThickness;
                    if (outerRadiusDist < 0 && innerRadiusDist > 0)
                    {
                        // fade the color based on whatever edge of the pulse we are closest
                        float minRadius = min(innerRadiusDist, -outerRadiusDist) * 3;
                        float t = saturate((minRadius) / _WaveThickness * (_WaveDuration - waveTime) * 2);
                        finalColor = lerp(finalColor, _WaveColor, t);
                    }
                }
                
                if (pulseToggle > 0.0)
                {
                    float pulseDist = length(IN.uv - _WaveCoords.xy);
                    float outerRadiusDist = _PulseOuterRadius;
                    float innerRadiusDist = _PulseInnerRadius;
                    if (pulseDist < outerRadiusDist && pulseDist > innerRadiusDist)
                    {
                        IN.uv -=0.5;
                        float s = sin ( _PulseRotationSpeed * _Time );
                        float c = cos ( _PulseRotationSpeed * _Time );
                        float2x2 rotationMatrix = float2x2( c, -s, s, c);
                        rotationMatrix *=0.5;
                        rotationMatrix +=0.5;
                        rotationMatrix = rotationMatrix * 2-1;
                        IN.uv = mul ( IN.uv, rotationMatrix );
                        IN.uv += 0.5;
                        finalColor = tex2D(_RotationTex, IN.uv);
                        float amountRotated = finalColor.r;
                        finalColor *= _WaveColor;
                        finalColor = lerp(finalColor, tex2D(_MainTex, IN.uv) * _Tint , 1 - amountRotated);
                        // finalColor = lerp(finalColor, _WaveColor, (sin(_Time.w * _PulseSpeed) + 1) / 2);
                        finalColor = lerp(finalColor, tex2D(_MainTex, IN.uv) * _Tint, saturate(pulseDist / _PulseFade));
                    }
                }
                // fixed4 textureColor = tex2D(_MainTex, IN.uv) * _Tint;
                // fixed3 tintedColor = lerp(_PulseColor.rgb, textureColor.rgb, (sin(_Time.w * _PulseSpeed) + 1) / 2);
                                // party
                // float time = _Time.z * 2;
                // float4 sinTime = _SinTime;
                // float2 pos = IN.uv * 30;
                // float2 seed = pos.x * pos.y;
                // // magic function i found on the internet
                // float noise = frac(sin(dot(seed ,float2(12.9898,78.233))) * 43758.5453);
                // float hue = (sin(pos.x + time + sinTime.w) + sin(pos.y + time + sinTime.z));
                // // hue = (hue + 6) / 12;
                // hue = (hue + 3) / 6;
                // hue *= saturate(noise + 0.5);
                // return float4(hsv_to_rgb(float3(hue, 1.0, 1.0)).rgb, 1.0);
                return finalColor;
            }

            ENDCG
        }
    }
}