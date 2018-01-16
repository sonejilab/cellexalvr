using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VRTK;

/// <summary>
/// This class represents a graph consisting of multiple GraphPoints.
/// </summary>
public class Graph : MonoBehaviour
{
    public GraphPoint graphpoint;
    public GameObject skeletonPrefab;
    public string DirectoryName { get; set; }
    public List<GameObject> Lines { get; set; }
    [HideInInspector]
    public GraphManager graphManager;
    public TextMesh graphNameText;
    public TextMesh graphInfoText;

    private GraphPoint newGraphpoint;
    public Dictionary<string, GraphPoint> points;
    private List<Vector3> pointsPositions;
    private Vector3 maxCoordValues;
    private Vector3 minCoordValues;
    private Vector3 diffCoordValues;
    private float longestAxis;
    private Vector3 scaledOffset;
    private Vector3 defaultPos;
    private Vector3 defaultScale;
    private ReferenceManager referenceManager;
    private ControllerModelSwitcher controllerModelSwitcher;
    private GameManager gameManager;

    private string graphName;
    /// <summary>
    /// The name of this graph. Should just be the filename that the graph came from.
    /// </summary>
    public string GraphName
    {
        get { return graphName; }
        set
        {
            graphName = value;
            graphNameText.text = value;
        }
    }

    private Material graphPointMaterial; // = Resources.Load("Materials/GraphPointGeneExpression") as Material;

    void Start()
    {
        points = new Dictionary<string, GraphPoint>(1024);
        defaultPos = transform.position;
        pointsPositions = new List<Vector3>();
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        gameManager = referenceManager.gameManager;
        Lines = new List<GameObject>();
        graphPointMaterial = Resources.Load("Materials/GraphPointGeneExpression") as Material;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
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
        foreach (GameObject line in Lines)
            line.SetActive(true);
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
        foreach (GameObject line in Lines)
            line.SetActive(false);
    }

    /// <summary>
    /// Scales three coordinates to fit inside the graphs area. This method requires <see cref="SetMinMaxCoords(Vector3, Vector3)"/> to have been called already.
    /// </summary>
    /// <param name="x"> The x-coordinate. </param>
    /// <param name="y"> The y-coordinate. </param>
    /// <param name="z"> The z-coordinate. </param>
    /// <returns> A Vector3 with the new scaled coordinates. </returns>
    public Vector3 ScaleCoordinates(float x, float y, float z)
    {
        // Scales the sphere coordinates to fit inside the graph's bounds.
        Vector3 scaledCoordinates = new Vector3(x, y, z);

        // move one of the graph's corners to origo
        scaledCoordinates -= minCoordValues;

        // this instead of /= longestaxis puts the graph in a 1x1x1 cube, which is convenient sometimes, but distorts the graph
        //scaledCoordinates.x /= diffCoordValues.x;
        //scaledCoordinates.y /= diffCoordValues.y;
        //scaledCoordinates.z /= diffCoordValues.z;

        // uniformly scale all axes down based on the longest axis
        // this makes the longest axis have length 1 and keeps the proportions of the graph
        scaledCoordinates /= longestAxis;

        // move the graph a bit so (0, 0, 0) is the center point
        scaledCoordinates -= scaledOffset;

        return scaledCoordinates;
    }

    /// <summary>
    /// Saves the default positions of the graphs.
    /// </summary>
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

        newGraphpoint.GetComponent<Renderer>().sharedMaterial = graphManager.defaultGraphPointMaterial;
        defaultScale = transform.localScale;
    }

    /// <summary>
    /// Sets the maximum and minumum coordinates that this graph should use.
    /// These argument Vector3s can be seen as two opposite corners of a cuboid that the graph will the be rescaled to fit inside.
    /// </summary>
    /// <param name="min"> The minimum coordinates. </param>
    /// <param name="max"> The maximum coordinates. </param>
    public void SetMinMaxCoords(Vector3 min, Vector3 max)
    {
        minCoordValues = min;
        maxCoordValues = max;
        diffCoordValues = maxCoordValues - minCoordValues;
        longestAxis = Math.Max(Math.Max(diffCoordValues.x, diffCoordValues.y), diffCoordValues.z);
        scaledOffset = (diffCoordValues / longestAxis) / 2;
        //GetComponent<BoxCollider>().size = new Vector3(diffCoordValues.x, diffCoordValues.y, diffCoordValues.z) / longestAxis;
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
        // One problem is that the lines are always ordered numerically so when unity is figuring out 
        // which way of the triangle is in and which is out, it's pretty much random what the result is.
        // The "solution" was to place a shader which does not cull the backside of the triangles, so 
        // both sides are always rendered.
        string path = Directory.GetCurrentDirectory() + @"\Data\" + DirectoryName + @"\" + GraphName + ".hull";
        FileStream fileStream = new FileStream(path, FileMode.Open);
        StreamReader streamReader = new StreamReader(fileStream);

        Vector3[] vertices = new Vector3[points.Count];
        List<int> triangles = new List<int>();

        for (int i = 0; i < points.Count; ++i)
        {
            vertices[i] = pointsPositions[i];
        }

        while (!streamReader.EndOfStream)
        {

            string[] coords = streamReader.ReadLine().Split(null);
            if (coords.Length < 4)
                continue;
            // subtract 1 because R is 1-indexed
            triangles.Add(int.Parse(coords[1]) - 1);
            triangles.Add(int.Parse(coords[2]) - 1);
            triangles.Add(int.Parse(coords[3]) - 1);
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

    /// <summary>
    /// Tells this graph that all graphpoints are added to this graph and we can update the info text.
    /// </summary>
    public void SetInfoText()
    {
        graphInfoText.transform.parent.localPosition = ScaleCoordinates(maxCoordValues.x - (maxCoordValues.x - minCoordValues.x) / 2, maxCoordValues.y, maxCoordValues.z);
        graphInfoText.text = "Points: " + points.Count;
        // the info panels should only be shown when the help tool is activated
        SetInfoTextVisible(controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.HelpTool);
    }

    /// <summary>
    /// Set this graph's info panel visible or not visible.
    /// </summary>
    /// <param name="visible"> True for visible, false for invisible </param>
    public void SetInfoTextVisible(bool visible)
    {
        graphInfoText.transform.parent.gameObject.SetActive(visible);
    }

    /// <summary>
    /// Resets all graphpoint's color to white.
    /// </summary>
    public void ResetGraphColors()
    {
        foreach (GraphPoint point in points.Values)
        {
            point.gameObject.SetActive(true);
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
            point.ResetColor();
        }
    }

    /// <summary>
    /// Adds many boxcolliders to this graph. The idea is that when grabbing graphs we do not want to collide with all the small colliders on the graphpoints, so we put many boxcolliders that cover the graph instead.
    /// </summary>
    public void CreateColliders()
    {
        // maximum number of times we allow colliders to grow in size
        int maxColliderIncreaseIterations = 10;
        // how many more graphpoints there must be for it to be worth exctending a collider
        float extensionThreshold = 0.1f;
        // copy points dictionary
        Dictionary<string, GraphPoint> notIncluded = new Dictionary<string, GraphPoint>(points);

        LayerMask layerMask = LayerMask.NameToLayer("GraphLayer");

        while (notIncluded.Count > 0)
        {
            // get any graphpoint
            GraphPoint point = notIncluded.Values.First();
            Vector3 pointPosition = point.transform.position;
            Vector3 halfExtents = new Vector3(0.01f, 0.01f, 0.01f);
            Vector3 oldHalfExtents = halfExtents;

            for (int j = 0; j < maxColliderIncreaseIterations; ++j)
            {
                // find the graphspoints it is near
                Collider[] collidesWith = Physics.OverlapBox(point.transform.position, halfExtents, Quaternion.identity, ~layerMask, QueryTriggerInteraction.Collide);
                // should we increase the size?

                // centers for new boxes to check
                Vector3[] newVectors = {
                    new Vector3(pointPosition.x + halfExtents.x, pointPosition.y, pointPosition.z),
                    new Vector3(pointPosition.x - halfExtents.x, pointPosition.y, pointPosition.z),
                    new Vector3(pointPosition.x, pointPosition.y + halfExtents.y, pointPosition.z),
                    new Vector3(pointPosition.x, pointPosition.y - halfExtents.y, pointPosition.z),
                    new Vector3(pointPosition.x, pointPosition.y, pointPosition.z + halfExtents.z),
                    new Vector3(pointPosition.x, pointPosition.y, pointPosition.z - halfExtents.z)
                };

                // halfextents for new boxes
                Vector3[] newHalfExtents = {
                    new Vector3(oldHalfExtents.x, halfExtents.y + oldHalfExtents.y * 2, halfExtents.z + oldHalfExtents.z * 2),
                    new Vector3(halfExtents.x + oldHalfExtents.x * 2, oldHalfExtents.y, halfExtents.z + oldHalfExtents.z * 2),
                    new Vector3(halfExtents.x + oldHalfExtents.x * 2, halfExtents.y + oldHalfExtents.y * 2, oldHalfExtents.z),
                };

                // check how many colliders there are surrounding us
                Collider[] collidesWithx1 = Physics.OverlapBox(newVectors[0], newHalfExtents[0], Quaternion.identity, ~layerMask, QueryTriggerInteraction.Collide);
                Collider[] collidesWithx2 = Physics.OverlapBox(newVectors[1], newHalfExtents[0], Quaternion.identity, ~layerMask, QueryTriggerInteraction.Collide);
                int totalCollidersx = NumberOfNotIncludedColliders(collidesWithx1, notIncluded) + NumberOfNotIncludedColliders(collidesWithx2, notIncluded);

                Collider[] collidesWithy1 = Physics.OverlapBox(newVectors[2], newHalfExtents[1], Quaternion.identity, ~layerMask, QueryTriggerInteraction.Collide);
                Collider[] collidesWithy2 = Physics.OverlapBox(newVectors[3], newHalfExtents[1], Quaternion.identity, ~layerMask, QueryTriggerInteraction.Collide);
                int totalCollidersy = NumberOfNotIncludedColliders(collidesWithy1, notIncluded) + NumberOfNotIncludedColliders(collidesWithy2, notIncluded);

                Collider[] collidesWithz1 = Physics.OverlapBox(newVectors[4], newHalfExtents[2], Quaternion.identity, ~layerMask, QueryTriggerInteraction.Collide);
                Collider[] collidesWithz2 = Physics.OverlapBox(newVectors[5], newHalfExtents[2], Quaternion.identity, ~layerMask, QueryTriggerInteraction.Collide);
                int totalCollidersz = NumberOfNotIncludedColliders(collidesWithz1, notIncluded) + NumberOfNotIncludedColliders(collidesWithz2, notIncluded);
                bool extended = false;
                // increase the halfextents if it seems worth it
                if (totalCollidersx > (int)(collidesWith.Length * extensionThreshold))
                {
                    halfExtents.x += oldHalfExtents.x;
                    extended = true;
                }
                if (totalCollidersy > (int)(collidesWith.Length * extensionThreshold))
                {
                    halfExtents.y += oldHalfExtents.y;
                    extended = true;
                }
                if (totalCollidersz > (int)(collidesWith.Length * extensionThreshold))
                {
                    halfExtents.z += oldHalfExtents.z;
                    extended = true;
                }
                // remove all the graphpoints that collide with this new collider
                collidesWith = Physics.OverlapBox(point.transform.position, halfExtents, Quaternion.identity, ~layerMask, QueryTriggerInteraction.Collide);
                RemoveGraphPointsFromDictionary(collidesWith, ref notIncluded);
                if (!extended) break;
            }
            // add the collider
            BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
            newCollider.center = transform.InverseTransformPoint(pointPosition);
            newCollider.size = halfExtents * 2;
        }
    }

    /// <summary>
    /// Helper method to remove graphpoints from a dictionary.
    /// </summary>
    /// <param name="colliders"> An array with colliders attached to graphpoints. </param>
    /// <param name="dict"> A dictionary containing graphpoints. </param>
    private void RemoveGraphPointsFromDictionary(Collider[] colliders, ref Dictionary<string, GraphPoint> dict)
    {
        {
            foreach (Collider c in colliders)
            {
                GraphPoint p = c.gameObject.GetComponent<GraphPoint>();
                if (p)
                {
                    dict.Remove(p.label);
                }
            }
        }
    }

    /// <summary>
    /// Helper method to count number of not yet added grapphpoints we collided with.
    /// </summary>
    /// <param name="colliders"> An array of colliders attached to graphpoints. </param>
    /// <param name="dict"> A dictionary containing graphpoints. </param>
    /// <returns> The number of graphpoints that were present in the dictionary. </returns>
    private int NumberOfNotIncludedColliders(Collider[] colliders, Dictionary<string, GraphPoint> dict)
    {
        int total = 0;
        foreach (Collider c in colliders)
        {
            GraphPoint p = c.gameObject.GetComponent<GraphPoint>();
            if (p)
            {
                total += dict.ContainsKey(p.label) ? 1 : 0;
            }
        }
        return total;
    }
}
