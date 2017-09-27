using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRTK;

/// <summary>
/// This class represents a graph consisting of multiple GraphPoints.
/// </summary>
public class Graph : MonoBehaviour
{
    public GraphPoint graphpoint;
    public GameObject skeletonPrefab;
    public string GraphName { get; set; }
    public string DirectoryName { get; set; }

    private GraphPoint newGraphpoint;
    public Dictionary<string, GraphPoint> points;
    private List<Vector3> pointsPositions;
    private Vector3 maxCoordValues;
    private Vector3 minCoordValues;
    private Vector3 diffCoordValues;
    private Vector3 defaultPos;
    private Vector3 defaultScale;
    private ReferenceManager referenceManager;
    private GameManager gameManager;

    void Start()
    {
        points = new Dictionary<string, GraphPoint>(1000);
        defaultPos = transform.position;
        pointsPositions = new List<Vector3>();
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        gameManager = referenceManager.gameManager;
    }

    private void Update()
    {
        if (GetComponent<VRTK_InteractableObject>().enabled)
        {
            gameManager.InformMoveGraph(GraphName, transform.position, transform.rotation, transform.localScale);
        }

    }
    /// <summary>
    /// Turns on all renderers and colliders for this graph.
    /// </summary>
    internal void ShowGraph()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = true;
    }

    /// <summary>
    /// Turns off all renderers and colliders for this graph.
    /// </summary>
    internal void HideGraph()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;
    }

    /// <summary>
    /// Scales three coordinates to fit inside the graphs area. This method requires SetMinMax() to have been called already.
    /// </summary>
    /// <param name="x"> The x-coordinate. </param>
    /// <param name="y"> The y-coordinate. </param>
    /// <param name="z"> The z-coordinate. </param>
    /// <returns> A Vector3 with the new scaled coordinates. </returns>
    public Vector3 ScaleCoordinates(float x, float y, float z)
    {
        // Scales the sphere coordinates to fit inside the graph's bounds.
        Vector3 scaledCoordinates = new Vector3(x, y, z);

        // move one of the graph's centers to origo
        scaledCoordinates -= minCoordValues;

        // find the longest axis
        float longestAxis = Math.Max(Math.Max(diffCoordValues.x, diffCoordValues.y), diffCoordValues.z);
        // this instead of /= longestaxis puts the graph in a 1x1x1 cube, which is convenient sometimes, but distorts the graph
        //scaledCoordinates.x /= diffCoordValues.x;
        //scaledCoordinates.y /= diffCoordValues.y;
        //scaledCoordinates.z /= diffCoordValues.z;

        // uniformly scale all axes down based on the longest axis
        // this makes the longest axis have length 1 and keeps the proportions of the graph
        scaledCoordinates.x /= longestAxis;
        scaledCoordinates.y /= longestAxis;
        scaledCoordinates.z /= longestAxis;

        // move the graph a bit so (0, 0, 0) is the center point
        scaledCoordinates.x -= (diffCoordValues.x / longestAxis) / 2;
        scaledCoordinates.y -= (diffCoordValues.y / longestAxis) / 2;
        scaledCoordinates.z -= (diffCoordValues.z / longestAxis) / 2;

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
        newGraphpoint.SetCoordinates(cell, scaledCoordinates.x, scaledCoordinates.y, scaledCoordinates.z);
        newGraphpoint.transform.parent = transform;
        newGraphpoint.transform.localPosition = new Vector3(scaledCoordinates.x, scaledCoordinates.y, scaledCoordinates.z);
        newGraphpoint.SaveParent(this);
        points[newGraphpoint.Label] = newGraphpoint;
        pointsPositions.Add(newGraphpoint.transform.localPosition);

        defaultScale = transform.localScale;
    }

    /// <summary>
    /// Sets the maximum and minumum coordinates that this graph should use.
    /// These argument Vector3s can be seen as two opposite corners of a cuboid that the graph will the be rescaled to fit inside.
    /// </summary>
    /// <param name="min"> The 3 minimun</param>
    /// <param name="max"></param>
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
        /// The file format should be
        ///  VERTEX_1    VERTEX_2    VERTEX_3
        ///  VERTEX_1    VERTEX_2    VERTEX_3
        /// ...
        /// Each line is 3 integers that corresponds to graphpoints
        /// 1 means the graphpoint that was created from the first line in the .mds file
        /// 2 means the graphpoint that was created from the second line
        /// and so on
        /// Each line in the file connects three graphpoints into a triangle
        /// One problem is that the lines are always ordered numerically so when unity is figuring out 
        /// which way of the triangle is in and which is out, it's pretty much random what the result is.
        /// The "solution" was to place a shader which does not cull the backside of the triangles, so 
        /// both sides are always rendered.
        string path = Directory.GetCurrentDirectory() + @"\Data\" + DirectoryName + @"\" + GraphName + ".hull";
        FileStream fileStream = new FileStream(path, FileMode.Open);
        StreamReader streamReader = new StreamReader(fileStream);

        Vector3[] vertices = new Vector3[points.Count];
        List<int> triangles = new List<int>();

        for (int i = 0; i < points.Count; ++i)
        {
            vertices[i] = pointsPositions[i];
        }

        var trianglesIndex = 0;
        while (!streamReader.EndOfStream)
        {

            string[] coords = streamReader.ReadLine().Split(null);
            if (coords.Length < 4)
                continue;
            // subtract 1 because R is 1-indexed
            triangles[trianglesIndex++] = int.Parse(coords[1]) - 1;
            triangles[trianglesIndex++] = int.Parse(coords[2]) - 1;
            triangles[trianglesIndex++] = int.Parse(coords[3]) - 1;
        }

        streamReader.Close();
        fileStream.Close();

        var convexHull = Instantiate(skeletonPrefab).GetComponent<MeshFilter>();
        convexHull.mesh = new Mesh()
        {
            vertices = vertices,
            triangles = triangles.ToArray()
        };

        convexHull.transform.position = transform.position;
        // move the convexhull slightly out of the way of the graph
        // in a direction sort of pointing towards the middle.
        // otherwise it lags really bad when the skeleton is first 
        // moved out of the original graph
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

    public void ResetGraphColors()
    {
        //bool graphPointActive;
        foreach (GraphPoint point in points.Values)
        {
            //point.gameObject.SetActive(true);
            point.ResetColor();
        }
    }

    /// <summary>
    /// Resets this graphs position, scale and color.
    /// </summary>
    public void ResetGraph()
    {
        transform.localScale = defaultScale;
        transform.position = defaultPos;
        transform.rotation = Quaternion.identity;
        foreach (GraphPoint point in points.Values)
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
