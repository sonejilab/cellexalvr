using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using CellexalVR.DesktopUI;
using CellexalVR.General;

namespace CellexalVR
{
    public class CurvedMeshGenerator : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject bezierNodePrefab;
        public bool showNodes;
        public int xSize = 4;
        public int ySize = 4;
        public int zSize = 2;
        public Material material;
        public int verticalSplit = 1;
        public int horizontalSplit = 1;
        public GameObject meshObjPrefab;


        private MeshFilter filter;

        // private Mesh mesh;
        private VRTK_InteractableObject interactableObj;

        private Vector3[] vertices;
        private Color32[] desktopUV;
        private Vector2[] uvs;
        private List<GameObject> bezierNodesObjects = new List<GameObject>();
        private List<Vector3> bezierNodes = new List<Vector3>();
        private List<GameObject> splitMeshes = new List<GameObject>();
        private Vector3[] tempPositions;
        private Quaternion[] tempRotations;
        private Vector3[] tempScales;

        private bool updatingMesh;
        private float tempCurvatureXValue;
        private MeshCollider collider;

        [SerializeField, Range(0f, 1f)] private float curvatureX = 0f;

        public float CurvatureX
        {
            get => curvatureX;
            set
            {
                curvatureX = 1 / value;


                // scale.y += 0
            }
        }

        [SerializeField, Range(0f, 1f)] private float curvatureY = 0f;

        public float CurvatureY
        {
            get => curvatureY;
            set => curvatureY = 1 / value;
        }

        private float curvatureXLastFrame = 0f;
        private float curvatureYLastFrame = 0f;
        
        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Awake()
        {
            // Generate nodes for bezier curves that make up the structure of the mesh.
            GenerateBezierNodes();
            // Generate the mesh.
            // StartCoroutine(UpdateMesh());
            GenerateMeshes();
            // UpdateMesh();
            // Generate(); if generating a cube use this one.
        }

        private void Update()
        {
            // bool changed = false;
            // foreach (GameObject obj in bezierNodes.Where(obj => obj.transform.hasChanged))
            // {
            //     obj.transform.hasChanged = false;
            //     changed = true;
            // }
            //
            // if (changed) UpdateMesh();
        }


        private void GenerateBezierNodes()
        {
            foreach (GameObject bn in bezierNodesObjects)
            {
                Destroy(bn);
            }

            bezierNodesObjects.Clear();
            bezierNodes.Clear();

            double yAngle = -Math.PI;
            float radius = 1f;
            float radiusInc = 0.1f * curvatureY;
            for (int k = 0; k < 4; k++)
            {
                float yPos = k / (float) (4); //(float) (Math.Cos(yAngle)) + 1;
                float zPos = (float) (Math.Sin(yAngle)) * curvatureY;
                Vector3 p2 = new Vector3(0, yPos, zPos);
                double angle = -(Math.PI);
                for (int m = 0; m < 4; m++)
                {
                    float xPos = p2.x + (float) Math.Cos(angle); // * radius;
                    yPos = p2.y;
                    zPos = p2.z + (float) (Math.Sin(angle) * curvatureX);

                    Vector3 p = new Vector3(xPos, yPos, zPos);
                    angle -= (Math.PI) / 3;
                    bezierNodes.Add(p);
                    if (showNodes)
                    {
                        GameObject
                            bezierNode =
                                Instantiate(bezierNodePrefab, transform); // = Instantiate(cubePrefab, transform);
                        bezierNode.transform.localPosition = p;
                        bezierNode.transform.localScale =
                            new Vector3(bezierNode.transform.localScale.x / transform.localScale.x,
                                bezierNode.transform.localScale.y, bezierNode.transform.localScale.z);
                        bezierNode.GetComponent<MeshRenderer>().enabled = showNodes;
                        bezierNode.transform.hasChanged = false;
                        if (k == ySize / 2)
                        {
                            bezierNode.GetComponent<Renderer>().material.color = Color.red;
                        }

                        bezierNodesObjects.Add(bezierNode);
                    }
                }

                yAngle -= Math.PI / (3);

                // if (k < ySize / 2)
                // {
                //     radiusInc -= 0.01f * curvatureY; // * (k % (ySize / 2));
                //     radius += radiusInc;
                // }
                //
                // else if (k == ySize / 2)
                // {
                //     radius += radiusInc;
                // }
                //
                // else
                // {
                //     radiusInc += 0.01f * curvatureY; // * (k % (ySize/2));
                //     radius -= radiusInc;
                // }

                // if (k < (ySize / 3))
                // {
                //     radiusInc -= 0.01f * curvatureY * (k % (ySize/2));
                //     radius += radiusInc;
                // }
                //
                //
                // else if (k > 2 * (ySize / 3))
                // {
                //     radiusInc += 0.01f * curvatureY * (k % (ySize/2));
                //     radius -= radiusInc;
                // }
            }
        }

        private void LateUpdate()
        {
            tempPositions = new Vector3[splitMeshes.Count];
            tempRotations = new Quaternion[splitMeshes.Count];
            tempScales = new Vector3[splitMeshes.Count];
            for (int i = 0; i < splitMeshes.Count; i++)
            {
                tempPositions[i] = splitMeshes[i].transform.localPosition;
                tempRotations[i] = splitMeshes[i].transform.localRotation;
                tempScales[i] = splitMeshes[i].transform.localScale;
            }
            if (updatingMesh) return;
            if (curvatureXLastFrame != curvatureX)
            {
                // Vector3 scale = transform.localScale;
                // float diffX = curvatureX - curvatureXLastFrame;
                // scale.y += diffX / 2;
                // scale.x -= diffX / 2;
                // transform.localScale = scale;
                GenerateBezierNodes();
                GenerateMeshes();
                curvatureXLastFrame = curvatureX;
            }
            else if (curvatureYLastFrame != curvatureY)
            {
                // Vector3 scale = transform.localScale;
                // float diffY = curvatureY - curvatureYLastFrame;
                // scale.x += diffY;
                // transform.localScale = scale;
                GenerateBezierNodes();
                GenerateMeshes();
                curvatureYLastFrame = curvatureY;
            }

            for (int i = 0; i < splitMeshes.Count; i++)
            {
                splitMeshes[i].transform.localPosition = tempPositions[i];
                splitMeshes[i].transform.localRotation = tempRotations[i];
                splitMeshes[i].transform.localScale = tempScales[i];
            }
        }


        private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float t2 = t * t;
            float u2 = u * u;
            float u3 = u2 * u;
            float t3 = t2 * t;
            Vector3 p = u3 * p0;
            p += 3 * u2 * t * p1;
            p += 3 * u * t2 * p2;
            p += t3 * p3;
            return p;
        }

        private void OnDrawGizmos()
        {
            if (vertices == null)
            {
                return;
            }

            Gizmos.color = Color.green;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (i == 0)
                {
                    Gizmos.DrawSphere(transform.TransformPoint(vertices[i]), 0.04f);
                }
                else
                {
                    Gizmos.DrawSphere(transform.TransformPoint(vertices[i]), 0.02f);
                }
            }
        }


        private void GenerateMeshes()
        {
            foreach (GameObject obj in splitMeshes)
            {
                Destroy(obj);
            }

            splitMeshes.Clear();
            for (int i = 0; i < verticalSplit; i++)
            {
                for (int j = 0; j < horizontalSplit; j++)
                {
                    GameObject obj = Instantiate(meshObjPrefab);
                    obj.GetComponent<MeshDeformer>().referenceManager = referenceManager;
                    obj.transform.parent = transform;
                    Vector3 position = obj.transform.localPosition;
                    position.x = 0.05f * i;
                    position.y = 0.05f * j;
                    obj.transform.localPosition = position;
                    obj.transform.localScale = Vector3.one;
                    Mesh mesh = GenerateMesh(obj, i * xSize / verticalSplit, (i + 1) * xSize / verticalSplit,
                        j * ySize / horizontalSplit, (j + 1) * ySize / horizontalSplit);
                    obj.GetComponent<MeshFilter>().mesh = mesh;
                    Renderer renderer = obj.GetComponent<MeshRenderer>();
                    renderer.material = material;
                    renderer.material.SetFloat("_XGridSize", xSize);
                    renderer.material.SetFloat("_YGridSize", ySize);
                    obj.GetComponent<MeshCollider>().sharedMesh = mesh;
                    splitMeshes.Add(obj);
                }
            }

        }

        private Mesh GenerateMesh(GameObject obj, int xStart = 0, int xEnd = 0, int yStart = 0, int yEnd = 0)
        {
            if (xEnd == 0) xEnd = xSize;
            if (yEnd == 0) yEnd = ySize;
            updatingMesh = true;

            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.name = "Procedural Curved Mesh";

            vertices = new Vector3[(xEnd - xStart + 1) * (yEnd - yStart + 1)];
            // Vector2[] uvs = new Vector2[vertices.Length];
            Color32[] desktopUV = new Color32[vertices.Length];
            // Vector4[] tangents = new Vector4[vertices.Length];
            // Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
            int z = 1;
            for (int v = 0, y = yStart, k = 0; y <= yEnd; y++, k += 3)
            {
                float tY = (float) y / (float) (ySize - 1);
                Vector3 pY1 = CalculateBezierPoint(tY, bezierNodes[0], bezierNodes[4],
                    bezierNodes[8], bezierNodes[12]);
                Vector3 pY2 = CalculateBezierPoint(tY, bezierNodes[1], bezierNodes[5],
                    bezierNodes[9], bezierNodes[13]);
                Vector3 pY3 = CalculateBezierPoint(tY, bezierNodes[2], bezierNodes[6],
                    bezierNodes[10], bezierNodes[14]);
                Vector3 pY4 = CalculateBezierPoint(tY, bezierNodes[3], bezierNodes[7],
                    bezierNodes[11], bezierNodes[15]);
                for (int x = xStart; x <= xEnd; x++, v++)
                {
                    float t = (float) x / (float) (xSize - 1);
                    Vector3 p0 = new Vector3(bezierNodes[0].x, pY1.y, pY1.z);
                    Vector3 p1 = new Vector3(bezierNodes[1].x, pY2.y, pY2.z);
                    Vector3 p2 = new Vector3(bezierNodes[2].x, pY3.y, pY3.z);
                    Vector3 p3 = new Vector3(bezierNodes[3].x, pY4.y, pY4.z);
                    Vector3 p = CalculateBezierPoint(t, p0, p1, p2, p3);
                    // print($"y+k: {y+k}, y+k+1: {y+k+1}, y+k+2: {y+k+2}, y+k+3: {y+k+3}");
                    vertices[v] = p;
                    // vertices[v].x += 0.5f; // easier to deal with if it goes from 0 to 1;
                    // uvs[i] = new Vector2((float) x / xSize, (float) y / ySize);
                    desktopUV[v] = new Color32((byte) x, (byte) y, (byte) z, 0);
                    // tangents[i] = tangent;
                }
            }

            mesh.vertices = vertices;
            // GetComponent<MeshDeformer>().UpdateVertices(vertices);
            // mesh.uv = uvs;
            mesh.colors32 = desktopUV;
            // mesh.tangents = tangents;


            int ti = 0;
            int yMax = yEnd - yStart;
            int xMax = xEnd - xStart;
            int[] triangles = new int[yMax * xMax * 6];
            for (int vi = 0, y = yStart; y < yEnd; y++, vi++)
            {
                for (int x = xStart; x < xEnd; x++, vi++)
                {
                    ti = SetQuad(triangles, ti, vi, vi + xMax + 1, vi + 1, vi + xMax + 2);
                }
            }

            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();


            // GetComponent<BoxCollider>().center = new Vector3(0.5f, 0.5f, 0.5f);
            // GetComponent<BoxCollider>().size = transform.localScale;


            // meshCollider = gameObject.AddComponent<MeshCollider>();
            // meshCollider.sharedMesh = null;

            // collider = GetComponent<MeshCollider>();
            // if (!collider)
            // {
            //     collider = gameObject.AddComponent<MeshCollider>();
            // }

            // Vector3 size = collider.bounds.size;
            // size = mesh.bounds.size;
            // Vector3 center = mesh.bounds.center;
            // size.z = 0.05f;
            // collider.size = size;
            // collider.center = center;
            // collider.isTrigger = true;
            // collider.convex = true;
            // meshCollider.sharedMesh = mesh;
            updatingMesh = false;
            return mesh;
        }


        private void Generate()
        {
            // GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            // mesh.name = "Procedural Cube";
            CreateVertices();
            // CreateTriangles();
            gameObject.AddComponent<BoxCollider>();
        }

        private void CreateVertices()
        {
            int cornerVertices = 8;
            int edgeVertices = (xSize + ySize + zSize - 3) * 4;
            int faceVertices = (
                (xSize - 1) * (ySize - 1) +
                (xSize - 1) * (zSize - 1) +
                (ySize - 1) * (zSize - 1)) * 2;
            vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
            desktopUV = new Color32[vertices.Length];
            uvs = new Vector2[vertices.Length];
            int v = 0;

            for (int y = 0; y <= ySize; y++)
            {
                for (int x = 0; x <= xSize; x++)
                {
                    SetVertex(v++, x, y, 0);
                }

                for (int z = 1; z <= zSize; z++)
                {
                    SetVertex(v++, xSize, y, z);
                    // vertices[v++] = new Vector3(xSize, y, z);
                }

                for (int x = xSize - 1; x >= 0; x--)
                {
                    SetVertex(v++, x, y, zSize);
                    // vertices[v++] = new Vector3(x, y, zSize);
                }

                for (int z = zSize - 1; z > 0; z--)
                {
                    SetVertex(v++, 0, y, z);
                    // vertices[v++] = new Vector3(0, y, z);
                }
            }

            for (int z = 1; z < zSize; z++)
            {
                for (int x = 1; x < xSize; x++)
                {
                    SetVertex(v++, x, ySize, z);
                    // vertices[v++] = new Vector3(x, ySize, z);
                }
            }

            for (int z = 1; z < zSize; z++)
            {
                for (int x = 1; x < xSize; x++)
                {
                    SetVertex(v++, x, 0, z);
                    // vertices[v++] = new Vector3(x, 0, z);
                }
            }

            // mesh.vertices = vertices;
            // mesh.colors32 = desktopUV;
            // mesh.uv = uvs;
        }

        private void SetVertex(int i, int x, int y, int z)
        {
            vertices[i] = new Vector3(x, y, z);
            desktopUV[i] = new Color32((byte) x, (byte) y, (byte) z, 0);
            uvs[i] = new Vector2(x, y);
        }

        private void CreateTriangles()
        {
            int[] trianglesZ = new int[(xSize * ySize) * 12];
            int[] trianglesX = new int[(ySize * zSize) * 12];
            int[] trianglesY = new int[(xSize * zSize) * 12];
            int ring = (xSize + zSize) * 2;
            int tZ = 0, tX = 0, tY = 0, v = 0;
            for (int y = 0; y < ySize; y++, v++)
            {
                for (int q = 0; q < xSize; q++, v++)
                {
                    tZ = SetQuad(trianglesZ, tZ, v, v + ring, v + 1, v + ring + 1);
                }

                for (int q = 0; q < zSize; q++, v++)
                {
                    tX = SetQuad(trianglesX, tX, v, v + ring, v + 1, v + ring + 1);
                }

                for (int q = 0; q < xSize; q++, v++)
                {
                    tZ = SetQuad(trianglesZ, tZ, v, v + ring, v + 1, v + ring + 1);
                }

                for (int q = 0; q < zSize - 1; q++, v++)
                {
                    tX = SetQuad(trianglesX, tX, v, v + ring, v + 1, v + ring + 1);
                }

                tX = SetQuad(trianglesX, tX, v, v + ring, v - ring + 1, v + 1);
            }


            // for (int y = 0; y < ySize; y++, v++)
            // {
            //     for (int q = 0; q < xSize; q++, v++)
            //     {
            //         tZ = SetQuad(trianglesZ, tZ, v, v + ring, v + 1, v + ring + 1);
            //     }
            //
            //     for (int q = 0; q < zSize; q++, v++)
            //     {
            //         tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            //     }
            //
            //     for (int q = 0; q < xSize; q++, v++)
            //     {
            //         tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            //     }
            //
            //     for (int q = 0; q < zSize - 1; q++, v++)
            //     {
            //         tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            //     }
            //
            //     tX = SetQuad(trianglesX, tX, v, v + ring, v - ring + 1, v + 1);
            // }

            tY = CreateTopFace(trianglesY, tY, ring);
            tY = CreateBottomFace(trianglesY, tY, ring);
            // mesh.subMeshCount = 3;
            // mesh.SetTriangles(trianglesZ, 0);
            // mesh.SetTriangles(trianglesX, 1);
            // mesh.SetTriangles(trianglesY, 2);
            // mesh.triangles = triangles;
        }

        private int CreateTopFace(int[] triangles, int t, int ring)
        {
            int v = ring * ySize;
            for (int x = 0; x < xSize - 1; x++, v++)
            {
                t = SetQuad(triangles, t, v, v + ring - 1, v + 1, v + ring);
            }

            t = SetQuad(triangles, t, v, v + ring - 1, v + 1, v + 2);

            int vMin = ring * (ySize + 1) - 1;
            int vMid = vMin + 1;
            int vMax = v + 2;
            // int diff = 1;

            for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
            {
                t = SetQuad(triangles, t, vMin, vMin - 1, vMid, vMid + xSize - 1);
                for (int x = 1; x < xSize - 1; x++, vMid++)
                {
                    t = SetQuad(
                        triangles, t,
                        vMid, vMid + xSize - 1, vMid + 1, vMid + xSize);
                }

                t = SetQuad(triangles, t, vMid, vMid + xSize - 1, vMax, vMax + 1);
            }

            int vTop = vMin - 2;
            t = SetQuad(triangles, t, vMin, vMin - 1, vMid, vMin - 2);
            for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vTop, vMid + 1, vTop - 1);
            }

            t = SetQuad(triangles, t, vMid, vTop, vTop - 2, vTop - 1);
            // for (int z = 1; z < zSize - 1; z++, vMin--, vMax++, vMax++)
            // {
            //     t = SetQuad(triangles, t, vMin, vMin - 1, vMid, vMid - xSize + 1);
            //     for (int x = 1; x < xSize - 1; x++, vMid++, diff -= 2)
            //     {
            //         t = SetQuad(triangles, t, vMid, vMid - xSize + diff, vMid + 1, vMid - xSize + (diff - 1));
            //     }
            //
            //     t = SetQuad(triangles, t, vMid, vMid - xSize + diff, vMax, vMax + 1);
            // }


            return t;
        }


        private int CreateBottomFace(int[] triangles, int t, int ring)
        {
            int v = 1;
            int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
            t = SetQuad(triangles, t, ring - 1, 0, vMid, 1);
            for (int x = 1; x < xSize - 1; x++, v++, vMid++)
            {
                t = SetQuad(triangles, t, vMid, v, vMid + 1, v + 1);
            }

            t = SetQuad(triangles, t, vMid, v, v + 2, v + 1);

            int vMin = ring - 2;
            vMid -= xSize - 2;
            int vMax = v + 2;

            for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
            {
                t = SetQuad(triangles, t, vMin, vMin + 1, vMid + xSize - 1, vMid);
                for (int x = 1; x < xSize - 1; x++, vMid++)
                {
                    t = SetQuad(
                        triangles, t,
                        vMid + xSize - 1, vMid, vMid + xSize, vMid + 1);
                }

                t = SetQuad(triangles, t, vMax + 1, vMid + xSize - 1, vMid, vMax);
            }

            int vTop = vMin - 1;
            t = SetQuad(triangles, t, vTop + 1, vTop + 2, vTop, vMid);
            for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
            {
                t = SetQuad(triangles, t, vTop, vMid, vTop - 1, vMid + 1);
            }

            t = SetQuad(triangles, t, vTop, vMid, vTop - 1, vTop - 2);

            return t;
        }

        private static int SetQuad(int[] triangles, int i, int v00, int v01, int v10, int v11)
        {
            triangles[i] = v00;
            triangles[i + 1] = triangles[i + 4] = v01;
            triangles[i + 2] = triangles[i + 3] = v10;
            triangles[i + 5] = v11;
            return i + 6;
        }

    }
}