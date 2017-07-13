using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public GraphPoint graphpoint;
    public SelectionToolHandler selectionToolHandler;
    public GameObject skeletonPrefab;
    public string GraphName { get; set; }
    private GraphPoint newGraphpoint;
    private List<GraphPoint> points;
    private Vector3 maxCoordValues;
    private Vector3 minCoordValues;
    private Vector3 diffCoordValues;
    private Vector3 minAreaValues;
    private Vector3 areaSize;
    private Vector3 defaultPos;
    private Vector3 defaultScale;
    void Start()
    {
        defaultPos = transform.position;
    }

    //Called before any Start()-function. Avoids nullReferenceException in addGraphPoint().
    void Awake()
    {
        points = new List<GraphPoint>(1000);
        minAreaValues = gameObject.transform.position;
        areaSize = new Vector3(1, 1, 1);
    }

    public void AddGraphPoint(Cell cell, float x, float y, float z)
    {

        // Scales the sphere coordinates to fit inside the this.
        Vector3 scaledCoordinates = new Vector3(x, y, z);
        scaledCoordinates -= minCoordValues;
        scaledCoordinates.x /= (diffCoordValues.x);
        scaledCoordinates.y /= (diffCoordValues.y);
        scaledCoordinates.z /= (diffCoordValues.z);
        scaledCoordinates.Scale(areaSize);
        scaledCoordinates += minAreaValues;

        newGraphpoint = Instantiate(graphpoint, new Vector3(scaledCoordinates.x, scaledCoordinates.y, scaledCoordinates.z), Quaternion.identity);
        newGraphpoint.gameObject.SetActive(true);
        newGraphpoint.SetCoordinates(cell, scaledCoordinates.x, scaledCoordinates.y, scaledCoordinates.z, areaSize);
        newGraphpoint.transform.SetParent(this.transform);
        newGraphpoint.SaveParent(this);
        points.Add(newGraphpoint);

        defaultScale = transform.localScale;
    }

    public void SetMinMaxCoords(Vector3 min, Vector3 max)
    {
        minCoordValues = min;
        maxCoordValues = max;
        diffCoordValues = maxCoordValues - minCoordValues;

    }

    public static void AutoWeld(Mesh mesh, float threshold, float bucketStep)
    {
        Vector3[] oldVertices = mesh.vertices;
        Vector3[] newVertices = new Vector3[oldVertices.Length];
        int[] old2new = new int[oldVertices.Length];
        int newSize = 0;

        // Find AABB
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < oldVertices.Length; i++)
        {
            if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
            if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
            if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
            if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
            if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
            if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
        }

        // Make cubic buckets, each with dimensions "bucketStep"
        int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
        int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
        int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
        List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

        // Make new vertices
        for (int i = 0; i < oldVertices.Length; i++)
        {
            // Determine which bucket it belongs to
            int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
            int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
            int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

            // Check to see if it's already been added
            if (buckets[x, y, z] == null)
                buckets[x, y, z] = new List<int>(); // Make buckets lazily

            for (int j = 0; j < buckets[x, y, z].Count; j++)
            {
                Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                if (Vector3.SqrMagnitude(to) < threshold)
                {
                    old2new[i] = buckets[x, y, z][j];
                    goto skip; // Skip to next old vertex if this one is already there
                }
            }

            // Add new vertex
            newVertices[newSize] = oldVertices[i];
            buckets[x, y, z].Add(newSize);
            old2new[i] = newSize;
            newSize++;

            skip:;
        }

        // Make new triangles
        int[] oldTris = mesh.triangles;
        int[] newTris = new int[oldTris.Length];
        for (int i = 0; i < oldTris.Length; i++)
        {
            newTris[i] = old2new[oldTris[i]];
        }

        Vector3[] finalVertices = new Vector3[newSize];
        for (int i = 0; i < newSize; i++)
            finalVertices[i] = newVertices[i];

        mesh.Clear();
        mesh.vertices = finalVertices;
        mesh.triangles = newTris;
        mesh.RecalculateNormals();
    }

    //public void CreateConvexHull(int[] xcoords, int[] ycoords, int[] zcoords)
    public void CreateConvexHull()
    {
        // TODO this file path should really not be hardcoded
        string path = @"C:\Users\vrproject\Documents\vrJeans\Assets\Data\Jensen_10X\" + GraphName + ".hull";
        string[] lines = File.ReadAllLines(path);

        int[] xcoords = new int[lines.Length];
        int[] ycoords = new int[lines.Length];
        int[] zcoords = new int[lines.Length];
        Vector3[] vertices = new Vector3[points.Count];
        int[] triangles = new int[lines.Length * 3];

        for (int i = 0; i < points.Count; ++i)
        {
            vertices[i] = points[i].transform.localPosition;
        }

        var trianglesIndex = 0;
        for (int i = 0; i < lines.Length; ++i)
        {
            string[] coords = lines[i].Split(null);
            triangles[trianglesIndex++] = int.Parse(coords[1]) - 1;
            triangles[trianglesIndex++] = int.Parse(coords[2]) - 1;
            triangles[trianglesIndex++] = int.Parse(coords[3]) - 1;
        }

        var convexHull = Instantiate(skeletonPrefab).GetComponent<MeshFilter>();
        convexHull.mesh = new Mesh()
        {
            vertices = vertices,
            triangles = triangles
        };
        convexHull.gameObject.transform.position = transform.position;
        convexHull.gameObject.transform.rotation = transform.rotation;
        convexHull.gameObject.GetComponent<MeshCollider>().sharedMesh = convexHull.mesh;
        convexHull.mesh.RecalculateBounds();
        convexHull.mesh.RecalculateNormals();
    }

    /* public void CreateConvexHull()
     {
         int scaleFactor = 10;
         Vector3 clonePos = new Vector3(0, .5f, -3.5f);
         MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
         CombineInstance[] combine = new CombineInstance[100];
         GameObject skeleton = Instantiate(skeletonPrefab);
         var skeletonMeshFilter = skeleton.GetComponent<MeshFilter>();
         skeletonMeshFilter.mesh = new Mesh();
         int i = 0;
         while (i < meshFilters.Length)
         {
             for (int j = 0; j < 100 && i < meshFilters.Length; ++j, ++i)
             {
                 var newMesh = Instantiate(meshFilters[i]);
                 newMesh.mesh = meshFilters[i].mesh;
                 newMesh.transform.localScale = meshFilters[i].transform.localScale * scaleFactor;
                 combine[j].mesh = newMesh.mesh;
                 combine[j].transform = newMesh.transform.localToWorldMatrix;
                 Destroy(newMesh.gameObject);
             }
             // print("combined meshes " + i);
             //skeletonMeshFilter.mesh = new Mesh
             skeletonMeshFilter.mesh.CombineMeshes(combine);
             Instantiate(skeletonMeshFilter, clonePos += new Vector3(0,0,-1f), Quaternion.identity, null).name = i + "";
             // Destroy(tmpSkeleton.gameObject);
             AutoWeld(skeletonMeshFilter.mesh, 0.001f, 1);
         }
         skeleton.transform.position = transform.position;
         skeleton.transform.rotation = transform.rotation;
         skeleton.transform.parent = transform;
     }*/

    public void ResetGraph()
    {
        transform.position = defaultPos;
        transform.localScale = defaultScale;
        foreach (GraphPoint point in points)
        {
            point.gameObject.SetActive(true);
            point.ResetCoords();
            if (point.GetComponent<Rigidbody>() != null)
            {
                point.GetComponent<Collider>().isTrigger = true;
                point.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }

    // Makes scaling of sub-graphs work better
    public void LimitGraphArea(ArrayList points)
    {
        maxCoordValues.x = maxCoordValues.y = maxCoordValues.z = -1000000.0f;
        minCoordValues.x = minCoordValues.y = minCoordValues.z = 1000000.0f;
        foreach (Collider col in points)
        {
            if (col.gameObject.activeSelf)
            {
                Vector3 coordinates = col.transform.position;
                if (coordinates.x > maxCoordValues.x)
                {
                    maxCoordValues.x = coordinates.x;
                }
                if (coordinates.x < minCoordValues.x)
                {
                    minCoordValues.x = coordinates.x;
                }
                if (coordinates.y > maxCoordValues.y)
                {
                    maxCoordValues.y = coordinates.y;
                }
                if (coordinates.y < minCoordValues.y)
                {
                    minCoordValues.y = coordinates.y;
                }
                if (coordinates.z > maxCoordValues.z)
                {
                    maxCoordValues.z = coordinates.z;
                }
                if (coordinates.z < minCoordValues.z)
                {
                    minCoordValues.z = coordinates.z;
                }
            }
        }
        Vector3 newCenter = Vector3.Lerp(minCoordValues, maxCoordValues, (minCoordValues - maxCoordValues).magnitude / 2);
        transform.position = newCenter;
    }

}
