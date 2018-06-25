Shader "Custom/GeneDistancePlot" {
    Properties{
        _WireThickness("Wire Thickness", RANGE(0, 800)) = 100
        _WireSmoothness("Wire Smoothness", RANGE(0, 20)) = 3
        _WireColor("Wire Color", Color) = (0.0, 1.0, 0.0, 1.0)
        _BaseColor("Base Color", Color) = (0.0, 0.0, 0.0, 1.0)

        _LowColor("Low Color", Color) = (0.0, 0.0, 1.0)
        _MedColor("Medium Color", Color) = (1.0, 1.0, 0.0)
        _HighColor("High Color", Color) = (1.0, 0.0, 0.0)
    }

        SubShader{
            Tags {
                "RenderType" = "Opaque"
            }

            Pass{
                CGPROGRAM
                #pragma vertex vert
                #pragma geometry geom
                #pragma fragment frag

                #include "UnityCG.cginc"

                uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
                uniform float _WireThickness;
                uniform float _WireSmoothness;
                uniform float4 _WireColor;
                uniform float4 _BaseColor;

                uniform float3 _LowColor;
                uniform float3 _MedColor;
                uniform float3 _HighColor;

                struct appdata
                {
                    float3 vertex : POSITION;
                    float3 normal : NORMAL;
                    float2 texcoord0 : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2g
                {
                    float3 normal : NORMAL;
                    float4 projectionSpaceVertex : SV_POSITION;
                    float2 uv0 : TEXCOORD0;
                    float4 worldSpacePosition : TEXCOORD1;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                struct g2f
                {
                    float3 normal : NORMAL;
                    float4 projectionSpaceVertex : SV_POSITION;
                    float2 uv0 : TEXCOORD0;
                    float4 worldSpacePosition : TEXCOORD1;
                    float4 dist : TEXCOORD2;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                v2g vert(appdata v)
                {
                    v2g o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.projectionSpaceVertex = UnityObjectToClipPos(v.vertex);
                    o.worldSpacePosition = mul(unity_ObjectToWorld, v.vertex);
                    o.uv0 = v.texcoord0;

                    // set color based on the height of the vertex
                    //o.color = float4(v.texcoord0.y, 0, 1 - v.texcoord0.y, 0);

                    o.normal = v.normal * 0.5 + 0.5;
                    return o;
                }

                [maxvertexcount(3)]
                void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream)
                {
                    float2 p0 = i[0].projectionSpaceVertex.xy / i[0].projectionSpaceVertex.w;
                    float2 p1 = i[1].projectionSpaceVertex.xy / i[1].projectionSpaceVertex.w;
                    float2 p2 = i[2].projectionSpaceVertex.xy / i[2].projectionSpaceVertex.w;


                    float2 edge0 = p2 - p1;
                    float2 edge1 = p2 - p0;
                    float2 edge2 = p1 - p0;



                    // To find the distance to the opposite edge, we take the
                    // formula for finding the area of a triangle Area = Base/2 * Height, 
                    // and solve for the Height = (Area * 2)/Base.
                    // We can get the area of a triangle by taking its cross product
                    // divided by 2.  However we can avoid dividing our area/base by 2
                    // since our cross product will already be double our area.
                    float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);
                    float wireThickness = 800 - _WireThickness;

                    g2f o;

                    o.uv0 = i[0].uv0;
                    o.worldSpacePosition = i[0].worldSpacePosition;
                    o.projectionSpaceVertex = i[0].projectionSpaceVertex;
                    o.normal = i[0].normal;
                    o.dist.xyz = float3((area / length(edge0)), 0.0, 0.0) * o.projectionSpaceVertex.w * wireThickness;
                    o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                    UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[0], o);
                    triangleStream.Append(o);

                    o.uv0 = i[1].uv0;
                    o.worldSpacePosition = i[1].worldSpacePosition;
                    o.projectionSpaceVertex = i[1].projectionSpaceVertex;
                    o.normal = i[1].normal;
                    o.dist.xyz = float3(0.0, (area / length(edge1)), 0.0) * o.projectionSpaceVertex.w * wireThickness;
                    o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                    UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[1], o);
                    triangleStream.Append(o);

                    o.uv0 = i[2].uv0;
                    o.worldSpacePosition = i[2].worldSpacePosition;
                    o.projectionSpaceVertex = i[2].projectionSpaceVertex;
                    o.normal = i[2].normal;
                    o.dist.xyz = float3(0.0, 0.0, (area / length(edge2))) * o.projectionSpaceVertex.w * wireThickness;
                    o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                    UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[2], o);
                    triangleStream.Append(o);
                }

                fixed4 frag(g2f i) : SV_Target
                {
                    float minDistanceToEdge = min(i.dist[0], min(i.dist[1], i.dist[2])) * i.dist[3];

                    float4 baseColor = _BaseColor * tex2D(_MainTex, i.uv0);

                    float4 desiredColor;

                    if (i.uv0.y < 0.5)
                    {
                        desiredColor = fixed4(lerp(_LowColor, _MedColor, i.uv0.y * 2.0), 1);
                    }
                    else
                    {
                        desiredColor = fixed4(lerp(_MedColor, _HighColor, (i.uv0.y - 0.5) * 2.0), 1);
                    }
                    return desiredColor;
                    // Early out if we know we are not on a line segment.
                    if (minDistanceToEdge > 0.9 || i.normal.y < 0.999999)
                    {
                        return desiredColor;
                    }

                    // Smooth our line out
                    float t = exp2(_WireSmoothness * -1.0 * minDistanceToEdge * minDistanceToEdge);
                    fixed4 finalColor = lerp(desiredColor, _WireColor, t);
                    finalColor.a = t;

                    return finalColor;
                }
                ENDCG
            }
    }

        FallBack "Diffuse"
}
