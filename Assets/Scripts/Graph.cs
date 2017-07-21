using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// This class represents a graph consisting of multiple GraphPoints.
/// </summary>
public class Graph : MonoBehaviour
{
    public GraphPoint graphpoint;
    public SelectionToolHandler selectionToolHandler;
    public GameObject skeletonPrefab;
    public string GraphName { get; set; }
    public string DirectoryName { get; set; }
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
        points = new List<GraphPoint>(1000);
        areaSize = new Vector3(1, 1, 1);
        minAreaValues = transform.position;
        defaultPos = transform.position;
    }

    public Vector3 ScaleCoordinates(float x, float y, float z)
    {
        // Scales the sphere coordinates to fit inside the graph's bounds.
        Vector3 scaledCoordinates = new Vector3(x, y, z);
        scaledCoordinates -= minCoordValues;
        scaledCoordinates.x /= (diffCoordValues.x);
        scaledCoordinates.y /= (diffCoordValues.y);
        scaledCoordinates.z /= (diffCoordValues.z);
        scaledCoordinates.Scale(areaSize);
        scaledCoordinates += minAreaValues;
        return scaledCoordinates;
    }

    /// <summary>
    /// Adds a GraphPoint to this graph.
    /// x, y and z coordinates will be scaled to fit the graph's size.
    /// </summary>
    /// <param name="cell"> The cell object the GraphPoint should represent. </param>
    /// <param name="x"> The x-coordinate. </param>
    /// <param name="y"> The y-coordinate. </param>
    /// <param name="z"> The z-coordinate. </param>
    public void AddGraphPoint(Cell cell, float x, float y, float z)
    {
        Vector3 scaledCoordinates = ScaleCoordinates(x, y, z);
        newGraphpoint = Instantiate(graphpoint);
        newGraphpoint.gameObject.SetActive(true);
        newGraphpoint.SetCoordinates(cell, scaledCoordinates.x, scaledCoordinates.y, scaledCoordinates.z, areaSize);
        newGraphpoint.transform.parent = transform;
        newGraphpoint.transform.localPosition = new Vector3(scaledCoordinates.x, scaledCoordinates.y, scaledCoordinates.z);
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

    internal bool Ready()
    {
        return points != null;
    }

    /// <summary>
    /// Creates a convex hull of this graph.
    /// Not really a convex hull though.
    /// More like a skeleton really.
    /// </summary>
    /// <returns> A reference to the created convexhull </returns>
    public GameObject CreateConvexHull()
    {
        string path = @"C:\Users\vrproject\Documents\vrJeans\Assets\Data\" + DirectoryName + @"\" + GraphName + ".hull";
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
        convexHull.gameObject.transform.localScale = transform.localScale;
        convexHull.gameObject.GetComponent<MeshCollider>().sharedMesh = convexHull.mesh;
        convexHull.mesh.RecalculateBounds();
        convexHull.mesh.RecalculateNormals();
        return convexHull.gameObject;
    }

    /// <summary>
    /// Resets this graphs position, scale and color.
    /// </summary>
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
}
