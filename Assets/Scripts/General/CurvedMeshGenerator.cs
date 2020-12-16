using System;
using System.Collections.Generic;
using UnityEngine;
using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.PDFViewer;

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
        private MeshFilter filter;

        // private Mesh mesh;
        private InteractableObjectBasic interactableObject;

        private Vector3[] vertices;
        private Color32[] desktopUV;
        private Vector2[] uvs;
        private List<GameObject> meshNodes = new List<GameObject>();
        private List<Vector3> meshNodePositions = new List<Vector3>();
        private List<GameObject> splitMeshes = new List<GameObject>();
        private Vector3[] tempPositions;
        private Quaternion[] tempRotations;
        private Vector3[] tempScales;

        private bool updatingMesh;
        private float tempCurvatureXValue;
        private MeshCollider collider;
        private PDFMesh pdfMesh;

        [SerializeField, Range(0f, 2f)] public float curvatureX = 0.3f;

        public float CurvatureX
        {
            get => curvatureX;
            set { curvatureX = 1 / value; }
        }

        [SerializeField, Range(0f, 2f)] public float curvatureY = 0f;

        public float CurvatureY
        {
            get => curvatureY;
            set => curvatureY = 1 / value;
        }

        [SerializeField, Range(0f, 5f)] public float radius = 1f;

        public float Radius => radius;


        private GameObject child;
        private Vector3 scale;
        private float curvatureXLastFrame;
        private float curvatureYLastFrame;
        private float radiusLastFrame;
        private int nrOfPagesLastFrame;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Awake()
        {
            curvatureXLastFrame = curvatureX;
            curvatureYLastFrame = curvatureY;
            radiusLastFrame = radius;
            // nrOfPagesLastFrame = nrOfPages;
            scale = Vector3.one;
            pdfMesh = GetComponentInParent<PDFMesh>();
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            // Generate nodes for bezier curves that make up the structure of the mesh.
            // CellexalEvents.GraphsLoaded.AddListener(() => GenerateNodes(PDFMesh.ViewingMode.PocketMovable));
            // Generate the mesh.
            // StartCoroutine(UpdateMesh());
            // CellexalEvents.GraphsLoaded.AddListener(GenerateMeshes);
            // CellexalEvents.GraphsLoaded.AddListener(() =>
            // StartCoroutine(pdfMesh.ShowMultiplePagesCoroutine(pdfMesh.currentPage, pdfMesh.currentNrOfPages)));
            // GenerateMeshes();
            // UpdateMesh();
            // Generate(); if generating a cube use this one.
        }

        public void GenerateNodes(PDFMesh.ViewingMode viewingMode, float r = 1.0f, float curvature = 0.3f)
        {
            if (r == 0) r = 1;
            if (curvature == 0) curvature = 0.3f; 
            foreach (GameObject bn in meshNodes)
            {
                Destroy(bn);
            }

            meshNodes.Clear();
            meshNodePositions.Clear();

            if (viewingMode == PDFMesh.ViewingMode.CurvedStationary)
            {
                xSize = 40;
                ySize = 10;
                GenerateCurvedNodes(r, curvature);
                GenerateMeshes();
                GetComponent<InteractableObjectBasic>().isGrabbable = false;
            }

            else
            {
                xSize = 7;
                ySize = 10;
                GenerateStraightNodes();
                GenerateMeshes();
            }

            if (!showNodes) return;
            foreach (Vector3 p in meshNodePositions)
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
                meshNodes.Add(bezierNode);
            }
        }


        private void GenerateCurvedNodes(float r = 1.0f, float curvature = 0.3f)
        {
            double yAngle = -Math.PI;
            for (int y = 0; y < ySize; y++)
            {
                float yPos = (float) Math.PI * y / (float) (ySize); //(float) (Math.Cos(yAngle)) + 1;
                float zPos = (float) (Math.Sin(yAngle)) * curvatureY;
                Vector3 p2 = new Vector3(0, yPos, zPos);
                for (int x = 0; x < xSize; x++)
                {
                    float xPos = p2.x + (float) (r * Math.Cos((x + xSize / 2) * -Math.PI * curvature / xSize));
                    yPos = p2.y;
                    zPos = p2.z + (float) (r * Math.Sin(
                        (x + xSize / 2) * -Math.PI * curvature / xSize)); // * curvatureX);

                    Vector3 p = new Vector3(xPos, yPos, zPos);
                    meshNodePositions.Add(p);
                }

                yAngle -= Math.PI / (ySize - 1);
            }
        }

        private void GenerateStraightNodes()
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    Vector3 p = new Vector3(x / 10f, y / 10f, 0);
                    meshNodePositions.Add(p);
                }
            }
        }

        private void LateUpdate()
        {
            if (GetComponentInChildren<MeshDeformer>() == null) return;
            tempPositions = new Vector3[splitMeshes.Count];
            tempRotations = new Quaternion[splitMeshes.Count];
            tempScales = new Vector3[splitMeshes.Count];
            // for (int i = 0; i < splitMeshes.Count; i++)
            // {
            //     tempPositions[i] = splitMeshes[i].transform.localPosition;
            //     tempRotations[i] = splitMeshes[i].transform.localRotation;
            //     tempScales[i] = splitMeshes[i].transform.localScale;
            // }

            if (updatingMesh) return;
            if (radiusLastFrame != radius)
            {
                GenerateNodes(PDFMesh.ViewingMode.CurvedStationary);
                GenerateMeshes();
                radiusLastFrame = radius;
            }

            else if (nrOfPagesLastFrame != pdfMesh.currentNrOfPages)
            {
                GenerateNodes(PDFMesh.ViewingMode.CurvedStationary);
                GenerateMeshes();
                nrOfPagesLastFrame = pdfMesh.currentNrOfPages;
            }

            else if (curvatureYLastFrame != curvatureY)
            {
                GenerateNodes(PDFMesh.ViewingMode.CurvedStationary);
                GenerateMeshes();
                curvatureYLastFrame = curvatureY;
            }

            else if (curvatureXLastFrame != curvatureX)
            {
                GenerateNodes(PDFMesh.ViewingMode.CurvedStationary);
                GenerateMeshes();
                curvatureXLastFrame = curvatureX;
            }

            if (splitMeshes.Count != tempPositions.Length) return;
            // for (int i = 0; i < splitMeshes.Count; i++)
            // {
            //     splitMeshes[i].transform.localPosition = tempPositions[i];
            //     splitMeshes[i].transform.localRotation = tempRotations[i];
            //     splitMeshes[i].transform.localScale = tempScales[i];
            // }
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


        public void GenerateMeshes()
        {
            if (child != null)
            {
                scale = child.transform.localScale;
            }


            foreach (GameObject obj in splitMeshes)
            {
                Destroy(obj);
            }

            splitMeshes.Clear();
            for (int i = 0; i < verticalSplit; i++)
            {
                for (int j = 0; j < horizontalSplit; j++)
                {
                    GetComponent<MeshDeformer>().referenceManager = referenceManager;
                    transform.localPosition = Vector3.zero;
                    Mesh mesh = GenerateMesh(i * xSize / verticalSplit, (i + 1) * xSize / verticalSplit,
                        j * ySize / horizontalSplit, (j + 1) * ySize / horizontalSplit);
                    GetComponent<MeshFilter>().mesh = mesh;
                    Renderer renderer = GetComponent<MeshRenderer>();
                    renderer.material = material;
                    renderer.material.SetFloat("_XGridSize", xSize - 1);
                    renderer.material.SetFloat("_YGridSize", ySize - 1);
                    MeshCollider mc = GetComponent<MeshCollider>();
                    mc.sharedMesh = mesh;
                    mc.gameObject.layer = LayerMask.NameToLayer("GraphLayer");
                    transform.localScale = scale;
                }
            }
        }


        private Mesh GenerateMesh(int xStart = 0, int xEnd = 0, int yStart = 0, int yEnd = 0)
        {
            if (xEnd == 0) xEnd = xSize;
            if (yEnd == 0) yEnd = ySize;
            updatingMesh = true;

            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.name = "Procedural Curved Mesh";

            vertices = new Vector3[(xEnd - xStart + 1) * (yEnd - yStart + 1)];
            uvs = new Vector2[vertices.Length];
            desktopUV = new Color32[vertices.Length];
            // Vector4[] tangents = new Vector4[vertices.Length];
            // Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
            int z = 1;
            // for (int v = 0, y = yStart, k = 0; y <= yEnd; y++, k += 3)
            // {
            // float tY = (float) y / (float) (ySize);
            // Vector3 pY1 = CalculateBezierPoint(tY, bezierNodes[0], bezierNodes[4],
            //     bezierNodes[8], bezierNodes[12]);
            // Vector3 pY2 = CalculateBezierPoint(tY, bezierNodes[1], bezierNodes[5],
            //     bezierNodes[9], bezierNodes[13]);
            // Vector3 pY3 = CalculateBezierPoint(tY, bezierNodes[2], bezierNodes[6],
            //     bezierNodes[10], bezierNodes[14]);
            // Vector3 pY4 = CalculateBezierPoint(tY, bezierNodes[3], bezierNodes[7],
            //     bezierNodes[11], bezierNodes[15]);
            // for (int x = xStart; x <= xEnd; x++, v++)
            // {
            // float t = (float) x / (float) (xSize);
            // Vector3 p0 = new Vector3(bezierNodes[0].x, pY1.y, pY1.z);
            // Vector3 p1 = new Vector3(bezierNodes[1].x, pY2.y, pY2.z);
            // Vector3 p2 = new Vector3(bezierNodes[2].x, pY3.y, pY3.z);
            // Vector3 p3 = new Vector3(bezierNodes[3].x, pY4.y, pY4.z);
            // Vector3 p = CalculateBezierPoint(t, p0, p1, p2, p3);
            // print($"y+k: {y+k}, y+k+1: {y+k+1}, y+k+2: {y+k+2}, y+k+3: {y+k+3}");
            // vertices[v] = p;
            // vertices[v].x += 0.5f; // easier to deal with if it goes from 0 to 1;
            // uvs[i] = new Vector2((float) x / xSize, (float) y / ySize);
            // desktopUV[v] = new Color32((byte) x, (byte) y, (byte) z, 0);
            // tangents[i] = tangent;
            //     }
            // }

            for (int y = 0, v = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++, v++)
                {
                    SetVertex(v, x, y, 1);
                }
            }

            mesh.vertices = vertices;
            mesh.colors32 = desktopUV;

            // mesh.uv = uvs;
            // mesh.tangents = tangents;


            int ti = 0;
            int yMax = yEnd - yStart;
            int xMax = xEnd - xStart;

            int[] triangles = new int[yMax * xMax * 6];
            for (int vi = 0, y = yStart;
                y < yEnd - 1;
                y++, vi++)
            {
                for (int x = xStart; x < xEnd - 1; x++, vi++)
                {
                    ti = SetQuad(triangles, ti, vi, vi + xMax, vi + 1, vi + xMax + 1);
                }
            }

            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            updatingMesh = false;
            return mesh;
        }

        private void SetVertex(int i, int x, int y, int z)
        {
            Vector3 node = meshNodePositions[i];
            vertices[i] = new Vector3(node.x, node.y, node.z);
            desktopUV[i] = new Color32((byte) x, (byte) y, (byte) z, 0);
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