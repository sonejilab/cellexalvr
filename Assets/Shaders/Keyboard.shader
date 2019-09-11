Shader "Custom/Keyboard"
{
    Properties
    {
        _NonFadedTex("Main Texture", 2D) = "white" {}
        _FadedTex("Faded Texture", 2D) = "white" {}
        _PulseColor("Pulse Color", Color) = (1, 1, 1, 1)
        _LaserHitColor("Laser Hit Color", Color) = (1, 1, 1, 1)
        _LaserHitThickness("Laser Hit Thickness", float) = 0.1
        _FadeRadius("Texture fade radius", float) = 5.0
		_PulseCoords("Pulse and Laser Coords", Vector) = (0, 0, 0, 0)
		_PulseStartTime("Pulse Start Time", float) = 0.0
        _PulseDuration("Pulse Duration", float) = 1.0
        _PulseMaxRadius("Pulse Max Radius", float) = 1.0
        _PulseThickness("Pulse Thickness", float) = 0.1
        _ScaleCorrection("Scale Correction", Vector) = (1.0, 1.0, 1.0, 1.0)
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
                float2 keyboardPos : TEXCOORD1;
            };
            
            struct v2f
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                float2 keyboardPos : TEXCOORD1;
            };

            sampler2D _NonFadedTex;
            sampler2D _FadedTex;
            fixed4 _PulseColor;
            fixed4 _LaserHitColor;
            float _LaserHitThickness;
            float _FadeRadius;
            float _PulseDuration;
            float _PulseMaxRadius;
            float _PulseThickness;
            float4 _ScaleCorrection;
            // goes from 0 to _PulseDuration when a pulse plays
            float _PulseStartTime;
            // xy is the uv2 coords that the last pulse played at, zw is the uv2 coords of the last laser hit
            float4 _PulseCoords;

            v2f vert (Input IN)
            {
                v2f OUT;
                OUT.position = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.uv;
                OUT.keyboardPos = IN.keyboardPos;
                return OUT;
            }

            fixed4 frag (v2f IN) : SV_TARGET
            {
                // the uv2 coordinates we are at
                float2 keyboardPos = IN.keyboardPos;
                // the uv2 coordinates that the laser hit the keyboard at
                float2 laserCoords = _PulseCoords.zw;
                fixed4 nonFadedColor = tex2D(_NonFadedTex, IN.uv);
                fixed4 fadedColor = tex2D(_FadedTex, IN.uv);

                // the distance between us and where the laser hit
                float laserHitDist = length((laserCoords - keyboardPos) * _ScaleCorrection.xy);

                fixed4 finalColor = lerp(nonFadedColor, fadedColor, saturate(laserHitDist / _FadeRadius));
				/*
				// debug stuff
				fixed4 finalColor = fixed4(1,1,1,1);
				if (IN.keyboardPos.x <= 0.99 && IN.keyboardPos.x >= 0.01 && IN.keyboardPos.y <= 0.99 && IN.keyboardPos.y >= 0.01)
				{
					//finalColor = fixed4(int2(IN.keyboardPos.x * 10, IN.keyboardPos.y * 10) / 10.0 , 0, 1);
					//finalColor = fixed4(GammaToLinearSpace(fixed3(IN.keyboardPos.x , IN.keyboardPos.y , 0)), 1);
					//finalColor = fixed4(IN.uv.x , IN.uv.y , 0, 1);
				}
				//else
				//{
				//	finalColor = fixed4(1,1,1,1);
				//}
				*/
                // draw the shiny area where the laser hits, or just finalColor if we are too far away
                finalColor = lerp(_LaserHitColor, finalColor, saturate(laserHitDist / _LaserHitThickness));
                float pulseTime = _PulseStartTime;
                if (pulseTime < _PulseDuration)
                {
                    // correct the pos to match the scale.
                    float2 pos = (keyboardPos - _PulseCoords.xy) * _ScaleCorrection.xy;

                    // the distances to the inner and outer radii
                    float innerRadiusDist = length(pos) / _PulseMaxRadius - pulseTime;
                    float outerRadiusDist = innerRadiusDist - _PulseThickness;
                    if (outerRadiusDist < 0 && innerRadiusDist > 0)
                    {
                        // fade the color based on whatever edge of the pulse we are closest
                        float minRadius = min(innerRadiusDist, -outerRadiusDist) * 3;
                        float t = saturate((minRadius) / _PulseThickness * (_PulseDuration - pulseTime) * 2);
                        finalColor = lerp(finalColor, _PulseColor, t);
                    }
                }
                return finalColor;
            }

            ENDCG
        }
    }
}
