Shader "VolumeRendering/ImageStackShader"
{
    Properties
    {
        _DataTex("Data Texture (Generated)", 3D) = "" {}
        _GradientTex("Gradient Texture (Generated)", 3D) = "" {}
        _NoiseTex("Noise Texture (Generated)", 2D) = "white" {}
        _TFTex("Transfer Function Texture (Generated)", 2D) = "" {}
        _MinVal("Min val", Range(0.0, 1.0)) = 0.0
        _MaxVal("Max val", Range(0.0, 1.0)) = 1.0
        _Alpha("Alpha", float) = 0.02
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline"}
            LOD 100
            Cull Front
            ZTest LEqual
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                //#pragma multi_compile MODE_DVR MODE_MIP MODE_SURF
                #pragma multi_compile __ TF2D_ON
                #pragma multi_compile __ CUTOUT_PLANE CUTOUT_BOX_INCL CUTOUT_BOX_EXCL
                #pragma multi_compile __ LIGHTING_ON
                #pragma multi_compile DEPTHWRITE_ON DEPTHWRITE_OFF
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                #define CUTOUT_ON CUTOUT_PLANE || CUTOUT_BOX_INCL || CUTOUT_BOX_EXCL

                struct vert_in
                {
                    float4 vertex : POSITION;
                    float4 normal : NORMAL;
                    float2 uv : TEXCOORD0;
                };

                struct frag_in
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float3 vertexLocal : TEXCOORD1;
                    float3 normal : NORMAL;
                    float3  worldPos : TEXCOORD2;
                };

                struct frag_out
                {
                    float4 colour : SV_TARGET;
    #if DEPTHWRITE_ON
                    float depth : SV_DEPTH;
    #endif
                };

                sampler3D _DataTex;
                sampler3D _GradientTex;
                sampler2D _NoiseTex;
                sampler2D _TFTex;

                float _MinVal;
                float _MaxVal;
                float _Alpha;
                uniform float4x4 _BoxMatrix;

                float4x4 _CrossSectionMatrix;
    #if CUTOUT_ON
    #endif

                // Gets the colour from a 1D Transfer Function (x = density)
                float4 getTF1DColour(float density)
                {
                    return tex2Dlod(_TFTex, float4(density, 0.0f, 0.0f, 0.0f));
                }

                // Gets the colour from a 2D Transfer Function (x = density, y = gradient magnitude)
                float4 getTF2DColour(float density, float gradientMagnitude)
                {
                    return tex2Dlod(_TFTex, float4(density, gradientMagnitude, 0.0f, 0.0f));
                }

                // Gets the density at the specified position
                float getDensity(float3 pos)
                {
                    return tex3Dlod(_DataTex, float4(pos.x, pos.y, pos.z, 0.0f));
                }

                // Gets the gradient at the specified position
                float3 getGradient(float3 pos)
                {
                    return tex3Dlod(_GradientTex, float4(pos.x, pos.y, pos.z, 0.0f)).rgb;
                }

                // Performs lighting calculations, and returns a modified colour.
                float3 calculateLighting(float3 col, float3 normal, float3 lightDir, float3 eyeDir, float specularIntensity)
                {
                    float ndotl = max(lerp(0.0f, 1.5f, dot(normal, lightDir)), 0.5f); // modified, to avoid volume becoming too dark
                    float3 diffuse = ndotl * col;
                    float3 v = eyeDir;
                    float3 r = normalize(reflect(-lightDir, normal));
                    float rdotv = max(dot(r, v), 0.0);
                    float3 specular = pow(rdotv, 32.0f) * float3(1.0f, 1.0f, 1.0f) * specularIntensity;
                    return diffuse + specular;
                }

                // Converts local position to depth value
                float localToDepth(float3 localPos)
                {
                    float4 clipPos = UnityObjectToClipPos(float4(localPos, 1.0f));

    #if defined(SHADER_API_GLCORE) || defined(SHADER_API_OPENGL) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
                    return (clipPos.z / clipPos.w) * 0.5 + 0.5;
    #else
                    return clipPos.z / clipPos.w;
    #endif
                }

                bool IsCutout(float3 currPos)
                {
                    // Move the reference in the middle of the mesh, like the pivot
                    float3 pos = currPos - float3(0.5f, 0.5f, 0.5f);;

                    // Convert from model space to plane's vector space
                    float3 planeSpacePos = mul(_CrossSectionMatrix, float4(pos, 1.0f));

                    return !(planeSpacePos.x >= -0.5f && planeSpacePos.x <= 0.5f && planeSpacePos.y >= -0.5f && planeSpacePos.y <= 0.5f && planeSpacePos.z >= -0.5f && planeSpacePos.z <= 0.5f);
                    return planeSpacePos.x >= -0.5f && planeSpacePos.x <= 0.5f && planeSpacePos.y >= -0.5f && planeSpacePos.y <= 0.5f && planeSpacePos.z >= -0.5f && planeSpacePos.z <= 0.5f;

                    return false;
                }

                float4 BlendUnder(float4 color, float4 newColor)
                {
                    color.rgb += (1.0 - color.a) * newColor.a * newColor.rgb;
                    color.a += (1.0 - color.a) * newColor.a;
                    return color;
                }

                float isInsideBox(float4 pos)
                {
                    if (pos.x < -.5 || pos.x > .5)
                        return 1;
                    if (pos.y < -.5 || pos.y > .5)
                        return 1;
                    if (pos.z < -.5 || pos.z > .5)
                        return 1;
                    return -1;
                }

                frag_in vert_main(vert_in v)
                {
                    frag_in o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    o.vertexLocal = v.vertex;
                    o.normal = UnityObjectToWorldNormal(v.normal);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    return o;
                }

                // Direct Volume Rendering
                frag_out frag_dvr(frag_in i)
                {
                    #define NUM_STEPS 128

                    const float stepSize = 1.732f/*greatest distance in box*/ / NUM_STEPS;

                    float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                    float3 lightDir = normalize(ObjSpaceViewDir(float4(float3(0.0f, 0.0f, 0.0f), 0.0f)));
                    float3 rayDir = ObjSpaceViewDir(float4(i.vertexLocal, 0.0f));
                    rayDir = normalize(rayDir);

                    // Create a small random offset in order to remove artifacts
                    rayStartPos = rayStartPos + (2.0f * rayDir / NUM_STEPS) * tex2D(_NoiseTex, float2(i.uv.x, i.uv.y)).r;

                    float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
                    uint iDepth = 0;
                    for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                    {
                        const float t = iStep * stepSize;
                        const float3 currPos = rayStartPos + rayDir * t;
                        if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f) // TODO: avoid branch?
                            break;

                        // Perform slice culling (cross section plane)
    #ifdef CUTOUT_ON
                        if (IsCutout(currPos))
                            continue;
    #endif

                        // Get the dansity/sample value of the current position
                        //const float density = getDensity(currPos);

                        // Calculate gradient (needed for lighting and 2D transfer functions)
                        //float3 gradient = getGradient(currPos);
    #if defined(TF2D_ON) || defined(LIGHTING_ON)
    #endif

                        // Apply transfer function
                        //float mag = length(gradient) / 1.75f;
                        //float4 src = getTF2DColour(0.1f, mag);
                        //float4 src = float4(0.0f, 0.0f, 0.0f, 0.15f);
    #if TF2D_ON
    #else
    #endif
                        //src = getTF1DColour(density);

                        // Apply lighting
                        //src.rgb = calculateLighting(src.rgb, normalize(gradient), lightDir, rayDir, 0.3f);
    #ifdef LIGHTING_ON
    #endif

                        //if (density < _MinVal || density > _MaxVal)
                        //    src.a = 0.0f;

                        float4 sampledColor = tex3D(_DataTex, currPos);// + float3(0.5f, 0.5f, 0.5f));
                        sampledColor.a *= _Alpha;

                        col.rgb += (1.0 - col.a) * sampledColor.a * sampledColor.rgb;
                        col.a += (1.0 - col.a) * sampledColor.a;

                        if (col.a > 0.15f)
                            iDepth = iStep;

                        if (col.a > 1.0f)
                            break;
                    }

                    // Write fragment output
                    frag_out output;
                    output.colour = col;
                    //output.depth = localToDepth(rayStartPos + rayDir * (iDepth * stepSize) - float3(0.5f, 0.5f, 0.5f));
                    output.depth = 1;
                    
                    //if (iDepth != 0)
                    //else
    #if DEPTHWRITE_ON
    #endif
                    return output;
                }

                //Maximum Intensity Projection mode
                frag_out frag_mip(frag_in i)
                {
                    #define NUM_STEPS 128
                    const float stepSize = 1.732f/*greatest distance in box*/ / NUM_STEPS;

                    float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                    float3 rayDir = ObjSpaceViewDir(float4(i.vertexLocal, 0.0f));
                    rayDir = normalize(rayDir);
                    float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
                    uint iDepth = 0;
                    float maxDensity = 0.0f;
                    for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                    {
                        const float t = iStep * stepSize;
                        const float3 currPos = rayStartPos + rayDir * t;
                        // Stop when we are outside the box
                        if (currPos.x < -0.0001f || currPos.x >= 1.0001f || currPos.y < -0.0001f || currPos.y > 1.0001f || currPos.z < -0.0001f || currPos.z > 1.0001f) // TODO: avoid branch?
                            break;

    #ifdef CUTOUT_ON
                        if (IsCutout(currPos))
                            continue;
    #endif

                        const float density = getDensity(currPos);
                        if (density > _MinVal && density < _MaxVal)
                            maxDensity = max(density, maxDensity);

                        float4 sampledColor = tex3D(_DataTex, currPos);// + float3(0.5f, 0.5f, 0.5f));
                        sampledColor.a *= _Alpha;

                        col.rgb += (1.0 - col.a) * sampledColor.a * sampledColor.rgb;
                        col.a += (1.0 - col.a) * sampledColor.a;
                    }

                    // Write fragment output
                    frag_out output;
                    output.colour = float4(col.rgb, maxDensity); // maximum intensity
    #if DEPTHWRITE_ON
                    output.depth = localToDepth(i.vertexLocal);
    #endif
                    //output.depth = 1;
                    return output;
                }

                // Surface rendering mode
                // Draws the first point (closest to camera) with a density within the user-defined thresholds.
                frag_out frag_surf(frag_in i)
                {
    #define NUM_STEPS 128
    #define EPSILON 0.00001f
                    const float stepSize = 1.732f/*greatest distance in box*/ / NUM_STEPS;

                    float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                    float3 rayDir = normalize(ObjSpaceViewDir(float4(i.vertexLocal, 0.0f)));
                    // Start from the end, tand trace towards the vertex
                    rayStartPos += rayDir * stepSize * NUM_STEPS;
                    rayDir = -rayDir;

                    // Create a small random offset in order to remove artifacts
                    rayStartPos = rayStartPos + (2.0f * rayDir / NUM_STEPS) * tex2D(_NoiseTex, float2(i.uv.x, i.uv.y)).r;
                    float4 col = float4(0,0,0,0);
                    for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                    {
                        const float t = iStep * stepSize;
                        const float3 currPos = rayStartPos + rayDir * t;
                        if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f)
                            continue;

                        float3 wPos = float4(i.worldPos.x, i.worldPos.y, i.worldPos.z, 1.0f);
                        float4 relpos_box1 = mul(_BoxMatrix, wPos);


                        //if (!IsCutout(currPos))
                        //    continue;

                        const float density = getDensity(currPos);
                        if (density > _MinVal && density < _MaxVal)
                        {
                            float4 sampledColor = tex3D(_DataTex, currPos);// +float3(0.5f, 0.5f, 0.5f));
                            sampledColor.a *= _Alpha;
                            col = BlendUnder(col, sampledColor);
                        }
                        

                        //    float3 normal = normalize(getGradient(currPos));
                        //    //col = getTF1DColour(density);
                        //    float4 sampledColor = tex3D(_DataTex, currPos);
                        //    col.rgb = calculateLighting(col.rgb, normal, -rayDir, -rayDir, 0.15);
                        //    col.a = sampledColor.a;
                        //    //col.rgb += (1.0 - col.a) * sampledColor.a * sampledColor.rgb;
                        //    //col.a += (1.0 - col.a) * sampledColor.a;
                        //    //col.a = _Alpha;
                        //    break;
                    }

                    // Write fragment output
                    frag_out output;
                    output.colour = col;
                    output.depth = localToDepth(rayStartPos + rayDir * (iStep * stepSize) - float3(0.5f, 0.5f, 0.5f));

                    return output;
                }

                frag_in vert(vert_in v)
                {
                    return vert_main(v);
                }

                frag_out frag(frag_in i)
                {
                    //return frag_dvr(i);

                    //return frag_mip(i);

                    return frag_surf(i);

                }

                ENDCG
            }
        }
}