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

    internal void ShowGraph()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = true;
    }

    internal void HideGraph()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;
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

    public void UpdateStartPosition()
    {
        defaultPos = transform.position;
        defaultScale = transform.localScale;
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
    /// <returns> A reference to the created convex hull or null if no file containing the convex hull information was found. </returns>
    public GameObject CreateConvexHull()
    {

        // Read the .hull file
        // The file format should be
        //  VERTEX_1    VERTEX_2    VERTEX_3
        //  VERTEX_1    VERTEX_2    VERTEX_3
        // ...
        // Each line is 3 integers that corresponds to graphpoints
        // 1 means the graphpoint that was created from the first line in the .mds file
        // 2 means the graphpoint that was created from the second line
        // and so on
        // Each line in the file connects three graphpoints into a triangle
        string path = Directory.GetCurrentDirectory() + @"\Assets\Data\" + DirectoryName + @"\" + GraphName + ".hull";
        string[] lines = File.ReadAllLines(path);
        if (lines.Length == 0)
        {
            print("File " + GraphName + ".hull not found");
            return null;
        }

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
            // subtract 1 because R is 1-indexed
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

        convexHull.transform.position = transform.position;
        // move the convexhull slightly out of the way of the graph
        // in a direction sort of pointing towards the middle
        Vector3 moveDist = new Vector3(.2f, 0, .2f);
        if (transform.position.x > 0) moveDist.x = -.2f;
        if (transform.position.z > 0) moveDist.z = -.2f;
        convexHull.transform.Translate(moveDist);

        convexHull.transform.rotation = transform.rotation;
        convexHull.transform.localScale = transform.localScale;
        convexHull.GetComponent<MeshCollider>().sharedMesh = convexHull.mesh;
        convexHull.mesh.RecalculateBounds();
        convexHull.mesh.RecalculateNormals();
        return convexHull.gameObject;
    }

    /// <summary>
    /// Resets this graphs position, scale and color.
    /// </summary>
    public void ResetGraph()
    {
        transform.localScale = defaultScale;
        transform.position = defaultPos;
        transform.rotation = Quaternion.identity;
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
