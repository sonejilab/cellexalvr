using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SQLiter;
using TMPro;
using UnityEngine;
using VRTK;

/// <summary>
/// Represents a graph consisting of multiple GraphPoints.
/// </summary>
public class CombinedGraph : MonoBehaviour
{
    public GameObject skeletonPrefab;
    public string DirectoryName { get; set; }
    public List<GameObject> Lines { get; set; }
    [HideInInspector]
    public GraphManager graphManager;
    public TextMeshPro graphNameText;
    public TextMeshPro graphInfoText;
    public Boolean GraphActive = true;
    public Dictionary<string, CombinedGraphPoint> points = new Dictionary<string, CombinedGraphPoint>();
    private List<Vector3> pointsPositions;
    public ReferenceManager referenceManager;
    private ControllerModelSwitcher controllerModelSwitcher;
    private GameManager gameManager;
    private Vector3 startPosition;

    // For minimization animation
    private bool minimize;
    private bool maximize;
    private Transform target;
    private float speed;
    private float targetMinScale;
    private float targetMaxScale;
    private float shrinkSpeed;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Vector3 originalScale;

    // Debug stuff
    private Vector3 debugGizmosPos;
    private Vector3 debugGizmosMin;
    private Vector3 debugGizmosMax;
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
            this.name = graphName;
            this.gameObject.name = graphName;
        }
    }
    public int textureWidth;
    public int textureHeight;
    public Texture2D texture;
    private bool textureChanged;
    private int nbrOfExpressionColors;
    public Vector3 minCoordValues = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    public Vector3 maxCoordValues = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    public Vector3 diffCoordValues;
    public float longestAxis;
    public Vector3 scaledOffset;
    public int nbrOfClusters;

    private static LayerMask selectionToolLayerMask;

    public OctreeNode octreeRoot;
    private CombinedGraphGenerator combinedGraphGenerator;
    public List<GameObject> combinedGraphPointClusters = new List<GameObject>();
    void Start()
    {
        speed = 1.5f;
        shrinkSpeed = 2f;
        targetMinScale = 0.05f;
        targetMaxScale = 1f;
        originalPos = new Vector3();
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        graphManager = referenceManager.graphManager;
        gameManager = referenceManager.gameManager;
        graphManager = referenceManager.graphManager;
        Lines = new List<GameObject>();
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        combinedGraphGenerator = GetComponent<CombinedGraphGenerator>();
        selectionToolLayerMask = 1 << LayerMask.NameToLayer("SelectionToolLayer");
        startPosition = transform.position;
        nbrOfExpressionColors = CellexalConfig.NumberOfExpressionColors;
    }

    private void Update()
    {
        if (textureChanged && texture != null)
        {
            texture.Apply();
            textureChanged = false;
        }

        if (GetComponent<VRTK_InteractableObject>().IsGrabbed())
        {
            gameManager.InformMoveGraph(GraphName, transform.position, transform.rotation, transform.localScale);
        }
        if (minimize)
        {
            Minimize();
        }
        if (maximize)
        {
            Maximize();
        }
    }

    internal void ShowGraph()
    {
        transform.position = referenceManager.minimizedObjectHandler.transform.position;
        GraphActive = true;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        //foreach (GameObject line in Lines)
        //    line.SetActive(true);
        maximize = true;
    }

    /// <summary>
    /// Animation for showing graph.
    /// </summary>
    void Maximize()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, originalPos, step);
        transform.localScale += Vector3.one * Time.deltaTime * shrinkSpeed;
        transform.Rotate(Vector3.one * Time.deltaTime * -100);
        if (transform.localScale.x >= targetMaxScale)
        {
            transform.localScale = originalScale;
            transform.localPosition = originalPos;
            CellexalLog.Log("Maximized object" + name);
            maximize = false;
            GraphActive = true;
            foreach (Collider c in GetComponentsInChildren<Collider>())
                c.enabled = true;
            //foreach (GameObject line in Lines)
            //    line.SetActive(true);
        }
    }
    /// <summary>
    /// Turns off all renderers and colliders for this graph.
    /// </summary>
    internal void HideGraph()
    {
        GraphActive = false;
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;
        foreach (GameObject line in Lines)
            line.SetActive(false);
        originalPos = transform.position;
        originalScale = transform.localScale;
        minimize = true;
    }

    /// <summary>
    /// Animation for hiding graph.
    /// </summary>
    void Minimize()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, referenceManager.minimizedObjectHandler.transform.position, step);
        transform.localScale -= Vector3.one * Time.deltaTime * shrinkSpeed;
        transform.Rotate(Vector3.one * Time.deltaTime * 100);
        if (transform.localScale.x <= targetMinScale)
        {
            minimize = false;
            GraphActive = false;
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = false;
            foreach (GameObject line in Lines)
                line.SetActive(false);
            referenceManager.minimizeTool.GetComponent<Light>().range = 0.04f;
            referenceManager.minimizeTool.GetComponent<Light>().intensity = 0.8f;
        }
    }

    public Vector3 ScaleCoordinates(Vector3 coords)
    {
        // move one of the graph's corners to origo
        coords -= minCoordValues;

        // uniformly scale all axes down based on the longest axis
        // this makes the longest axis have length 1 and keeps the proportions of the graph
        coords /= longestAxis;

        // move the graph a bit so (0, 0, 0) is the center point
        coords -= scaledOffset;
        return coords;
    }

    public class CombinedGraphPoint
    {
        private static int indexCounter = 0;

        public string Label;
        public Vector3 Position;
        public int index;
        public Vector2Int textureCoord;
        public CombinedGraph parent;
        public int group;
        public bool unconfirmedInSelection;

        public CombinedGraphPoint(string label, float x, float y, float z, CombinedGraph parent)
        {
            Label = label;
            Position = new Vector3(x, y, z);
            this.parent = parent;
            index = indexCounter;
            group = -1;
            unconfirmedInSelection = false;
            indexCounter++;
        }

        public override string ToString()
        {
            return Label;
        }

        public void ScaleCoordinates()
        {
            // move one of the graph's corners to origo
            Position -= parent.minCoordValues;

            // uniformly scale all axes down based on the longest axis
            // this makes the longest axis have length 1 and keeps the proportions of the graph
            Position /= parent.longestAxis;

            // move the graph a bit so (0, 0, 0) is the center point
            Position -= parent.scaledOffset;
        }

        public void SetTextureCoord(Vector2Int newPos)
        {
            textureCoord = newPos;
        }

        public void RecolorGeneExpression(int i, bool outline)
        {
            parent.RecolorGraphPointGeneExpression(this, i, outline);
        }

        public void RecolorSelectionColor(int i, bool outline)
        {
            parent.RecolorGraphPointSelectionColor(this, i, outline);
            group = i;
        }

        public void ResetColor()
        {
            parent.ResetGraphPointColor(this);
        }

        public Color GetColor()
        {
            return parent.GetGraphPointColor(this);
        }
    }

    /// <summary>
    /// Private class to represent one node in the octree used for collision detection
    /// </summary>
    public class OctreeNode
    {
        public OctreeNode parent;
        /// <summary>
        /// Not always of length 8. Empty children are removed when the octree is constructed to save some memory.
        /// </summary>
        public OctreeNode[] children;
        /// <summary>
        /// Null if this is not a leaf. Only leaves represents points in the graph.
        /// </summary>
        public CombinedGraphPoint point;
        public Vector3 pos;
        /// <summary>
        /// Not the geometrical center, just a point inside the node that defines corners for its children, unless this is a leaf node.
        /// </summary>
        public Vector3 center;
        public Vector3 size;
        private int group = -1;
        private bool raycasted;
        /// <summary>
        /// The group that this node belongs to. -1 means no group, 0 or a positive number means some group.
        /// If this is a leaf node, this should be the same as the group that the selection tool has given the <see cref="CombinedGraphPoint"/>.
        /// If this is not a leaf node, this is not -1 if all its children are of that group.
        /// </summary>
        public int Group
        {
            get { return group; }
            set { SetGroup(value); }
        }
        public bool rejected;
        public bool completelyInside;

        public OctreeNode() { }

        /// <summary>
        /// Returns a string representation of this node and all its children. May produce very long strings for very large trees.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            ToStringRec(ref sb);
            return sb.ToString();
        }

        private void ToStringRec(ref StringBuilder sb)
        {
            if (point != null)
            {
                sb.Append(point.Label);
            }
            else
            {
                if (children.Length > 0)
                {
                    sb.Append("(");
                    foreach (var child in children)
                    {
                        child.ToStringRec(ref sb);
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(")");
                }
            }
        }

        private void SetGroup(int group)
        {
            this.group = group;
            foreach (var child in children)
            {
                if (child.group != group)
                    child.SetGroup(group);
            }
            if (parent != null && parent.group != group)
            {
                parent.NotifyGroupChange(group);
            }
        }

        private void NotifyGroupChange(int group)
        {
            int setGroupTo = group;
            // only set this node's group if all children are also of that group
            foreach (var child in children)
            {
                if (child.group != group)
                {
                    // not all children were of that group, set this node's group to -1
                    setGroupTo = -1;
                    break;
                }
            }
            this.group = setGroupTo;
            if (parent != null && parent.group != group)
            {
                parent.NotifyGroupChange(group);
            }
        }

        /// <summary>
        /// Changes the group of this node and all of its children (and grand-children and so on) recursively.
        /// </summary>
        /// <param name="group">The new group.</param>
        public void ChangeGroupRecursively(int group)
        {
            this.group = group;
            foreach (var child in children)
            {
                child.ChangeGroupRecursively(group);
            }
        }

        /// <summary>
        /// Returns the smallest node that contains a point. The retured node might not be a leaf.
        /// </summary>
        /// <param name="point">The point to look for a node around.</param>
        /// <returns>The that contains the point.</returns>
        public OctreeNode NodeContainingPoint(Vector3 point)
        {
            if (children.Length == 0)
            {
                return this;
            }
            foreach (OctreeNode child in children)
            {
                if (child.PointInside(point))
                {
                    return child.NodeContainingPoint(point);
                }
            }
            return this;
        }

        /// <summary>
        /// Checks if a point is inside this node.
        /// </summary>
        public bool PointInside(Vector3 point)
        {
            return point.x >= pos.x && point.x <= pos.x + size.x
                && point.y >= pos.y && point.y <= pos.y + size.y
                && point.z >= pos.z && point.z <= pos.z + size.z;
        }

        /// <summary>
        /// Checks if a point is inside the selection tool by raycasting.
        /// </summary>
        /// <param name="selectionToolCenter">The selection tool's position in world space.</param>
        /// <param name="pointPosWorldSpace">This node's position in world space. (use Transform.TransformPoint(node.splitCenter) )</param>
        /// <returns>True if <paramref name="pointPosWorldSpace"/> is inside the selection tool.</returns>
        public bool PointInsideSelectionTool(Vector3 selectionToolCenter, Vector3 pointPosWorldSpace)
        {
            raycasted = true;
            Vector3 difference = selectionToolCenter - pointPosWorldSpace;
            return !Physics.Raycast(pointPosWorldSpace, difference, difference.magnitude, CombinedGraph.selectionToolLayerMask);
        }

        #region DEBUG_FUNCTIONS
        public void DebugColorLeaves(Color color)
        {
            if (point != null)
            {
                point.RecolorSelectionColor(0, false);
            }
            foreach (var child in children)
            {
                child.DebugColorLeaves(color);
            }
        }

        public void DrawDebugCubesRecursive(Vector3 gameobjectPos, bool onlyLeaves, int i)
        {
            Gizmos.color = GizmoColors(i++);
            if (!onlyLeaves || children.Length == 0 && onlyLeaves)
            {
                Gizmos.DrawWireCube(gameobjectPos + pos + size / 2, size / 0.95f);
            }
            foreach (var child in children)
            {
                child.DrawDebugCubesRecursive(gameobjectPos, onlyLeaves, i);
            }

        }

        public void DrawDebugCube(Vector3 gameobjetcPos)
        {
            if (rejected)
            {

            }
        }

        public void DrawDebugLines(Vector3 gameobjectPos)
        {
            if (children.Length != 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(gameobjectPos + center, 0.03f);
                Gizmos.color = Color.white;
            }
            foreach (var child in children)
            {
                Gizmos.DrawLine(gameobjectPos + center, gameobjectPos + child.center);
                child.DrawDebugLines(gameobjectPos);
            }
        }

        public void DrawDebugRaycasts(Vector3 gameobjectPos, Vector3 selectionToolPos)
        {
            if (raycasted)
            {
                Gizmos.DrawLine(center + gameobjectPos, selectionToolPos);
                raycasted = false;
            }
            else
            {
                foreach (var child in children)
                {
                    child.DrawDebugRaycasts(gameobjectPos, selectionToolPos);
                }
            }
        }

        private Color GizmoColors(int i)
        {
            i = i % 5;
            switch (i)
            {
                case 0:
                    return Color.red;
                case 1:
                    return Color.blue;
                case 2:
                    return Color.green;
                case 3:
                    return Color.cyan;
                case 4:
                    return Color.magenta;
                case 5:
                    return Color.yellow;
                default:
                    return Color.white;
            }
        }
        #endregion
    }

    public void OnDrawGizmos()
    {
        if (octreeRoot != null)
        {
            if (graphManager.drawDebugCubes)
            {
                octreeRoot.DrawDebugCubesRecursive(transform.position, false, 0);
            }
            if (graphManager.drawDebugLines)
            {
                Gizmos.color = Color.white;
                octreeRoot.DrawDebugLines(transform.position);
            }
        }
        if (graphManager.drawSelectionToolDebugLines)
        {
            Gizmos.color = Color.green;
            Vector3 debugGizmosSize = debugGizmosMax - debugGizmosMin;
            DrawDebugCube(Color.green, transform.TransformPoint(debugGizmosMin), transform.TransformPoint(debugGizmosMax));
            Gizmos.DrawSphere(debugGizmosMin, 0.01f);
            Gizmos.DrawSphere(debugGizmosMax, 0.01f);
            //print("debug lines: " + debugGizmosMin.ToString() + " " + debugGizmosMax.ToString());
        }
        if (graphManager.drawDebugRaycast)
        {
            octreeRoot.DrawDebugRaycasts(transform.position, debugGizmosPos);
        }
        if (graphManager.drawDebugRejectionApprovedCubes)
        {
            DrawRejectionApproveCubes(octreeRoot);
        }

    }

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
        CellexalLog.Log("Started reading " + path);
        for (int i = 0; i < points.Count; ++i)
        {
            vertices[i] = pointsPositions[i];
        }

        while (!streamReader.EndOfStream)
        {

            string[] coords = streamReader.ReadLine().Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
            if (coords.Length != 3)
                continue;
            // subtract 1 because R is 1-indexed
            triangles.Add(int.Parse(coords[0]) - 1);
            triangles.Add(int.Parse(coords[1]) - 1);
            triangles.Add(int.Parse(coords[2]) - 1);

        }

        streamReader.Close();
        fileStream.Close();

        var convexHull = Instantiate(skeletonPrefab).GetComponent<MeshFilter>();
        convexHull.gameObject.name = "ConvexHull_" + this.name;
        convexHull.mesh = new Mesh()
        {
            vertices = vertices,
            triangles = triangles.ToArray()
        };
        if (gameManager.multiplayer)
        {
            convexHull.transform.position = new Vector3(0, 1f, 0);
        }
        if (!gameManager.multiplayer)
        {
            convexHull.transform.position = referenceManager.rightController.transform.position;
        }
        // move the convexhull slightly out of the way of the graph
        // in a direction sort of pointing towards the middle.
        // otherwise it lags really bad when the skeleton is first 
        // moved out of the original graph
        //Vector3 moveDist = new Vector3(0f, 0.3f, 0f);
        //if (transform.position.x > 0) moveDist.x = -.2f;
        //if (transform.position.z > 0) moveDist.z = -.2f;
        //convexHull.transform.Translate(moveDist);
        //convexHull.transform.position += referenceManager.rightController.transform.forward * 1f;
        //convexHull.transform.rotation = transform.rotation;
        //convexHull.transform.localScale = transform.localScale;
        convexHull.GetComponent<MeshCollider>().sharedMesh = convexHull.mesh;
        convexHull.mesh.RecalculateBounds();
        convexHull.mesh.RecalculateNormals();
        CellexalLog.Log("Created convex hull with " + vertices.Count() + " vertices");
        return convexHull.gameObject;

    }

    private void DrawDebugCube(Color color, Vector3 min, Vector3 max)
    {
        min += transform.position;
        max += transform.position;
        Gizmos.color = color;
        Vector3[] corners = new Vector3[]
        {
           min,
           new Vector3(max.x, min.y, min.z),
           new Vector3(min.x, max.y, min.z),
           new Vector3(min.x, min.y, max.z),
           new Vector3(max.x, max.y, min.z),
           new Vector3(max.x, min.y, max.z),
           new Vector3(min.x, max.y, max.z),
           max
        };

        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[0], corners[2]);
        Gizmos.DrawLine(corners[0], corners[3]);
        Gizmos.DrawLine(corners[1], corners[4]);
        Gizmos.DrawLine(corners[1], corners[5]);
        Gizmos.DrawLine(corners[2], corners[4]);
        Gizmos.DrawLine(corners[2], corners[6]);
        Gizmos.DrawLine(corners[3], corners[5]);
        Gizmos.DrawLine(corners[3], corners[6]);
        Gizmos.DrawLine(corners[4], corners[7]);
        Gizmos.DrawLine(corners[5], corners[7]);
        Gizmos.DrawLine(corners[6], corners[7]);

    }

    private void DrawRejectionApproveCubes(OctreeNode node)
    {
        if (node.rejected)
        {
            node.rejected = false;
            DrawDebugCube(Color.red, node.pos, node.pos + node.size);
        }
        else if (node.completelyInside)
        {
            node.completelyInside = false;
            DrawDebugCube(Color.green, node.pos, node.pos + node.size);
        }

        foreach (var child in node.children)
        {
            DrawRejectionApproveCubes(child);
        }
    }

    /// <summary>
    /// Tells this graph that all graphpoints are added to this graph and we can update the info text.
    /// </summary>
    public void SetInfoText()
    {
        //graphInfoText.transform.parent.localPosition = ScaleCoordinates(maxCoordValues.x - (maxCoordValues.x - minCoordValues.x) / 2, maxCoordValues.y, maxCoordValues.z);
        graphInfoText.text = "Points: " + points.Count;
        SetInfoTextVisible(true);
    }

    /// <summary>
    /// Set this graph's info panel visible or not visible.
    /// </summary>
    /// <param name="visible"> True for visible, false for invisible </param>
    public void SetInfoTextVisible(bool visible)
    {
        graphInfoText.transform.parent.gameObject.SetActive(visible);
    }

    public void ResetColorsAndPosition()
    {
        ResetColors();
        transform.position = startPosition;
    }

    /// <summary>
    /// Resets the color of all graphpoints in this graph to white.
    /// </summary>
    public void ResetColors()
    {
        for (int i = 0; i < textureWidth; ++i)
        {
            for (int j = 0; j < textureHeight; ++j)
            {
                texture.SetPixel(i, j, Color.white);
            }
        }
        texture.Apply();

        foreach (CombinedGraphPoint p in points.Values)
        {
            p.group = -1;
        }
        octreeRoot.Group = -1;
    }

    public Color GetGraphPointColor(CombinedGraphPoint gp)
    {
        return texture.GetPixel(gp.textureCoord.x, gp.textureCoord.y);
    }

    /// <summary>
    /// Resets this graphs position, scale and color.
    /// </summary>
    //    public void ResetGraph()
    //    {
    //        transform.localScale = defaultScale;
    //        transform.position = defaultPos;
    //        transform.rotation = Quaternion.identity;
    //        foreach (GraphPoint point in points.Values)
    //        {
    //            point.gameObject.SetActive(true);
    //            point.ResetCoords();
    //            point.ResetColor();
    //        }
    //    }

    /// <summary>
    /// Recolors a single graphpoint.
    /// </summary>
    /// <param name="combinedGraphPoint">The graphpoint to recolor.</param>
    /// <param name="color">The graphpoint's new color.</param>
    public void RecolorGraphPointGeneExpression(CombinedGraphPoint combinedGraphPoint, int i, bool outline)
    {
        byte greenChannel = (byte)(outline ? 1 : 0);
        if (i == -1)
        {
            i = 255;
        }
        Color32 finalColor = new Color32((byte)i, greenChannel, 0, 255);
        texture.SetPixels32(combinedGraphPoint.textureCoord.x, combinedGraphPoint.textureCoord.y, 1, 1, new Color32[] { finalColor });
        textureChanged = true;
    }

    public void RecolorGraphPointSelectionColor(CombinedGraphPoint combinedGraphPoint, int i, bool outline)
    {
        byte greenChannel = (byte)(outline ? 1 : 0);
        if (i == -1)
        {
            i = 255;
        }
        byte redChannel = (byte)(nbrOfExpressionColors + i);
        Color32 finalColor = new Color32(redChannel, greenChannel, 0, 255);
        texture.SetPixels32(combinedGraphPoint.textureCoord.x, combinedGraphPoint.textureCoord.y, 1, 1, new Color32[] { finalColor });
        textureChanged = true;
    }

    public void ResetGraphPointColor(CombinedGraphPoint combinedGraphPoint)
    {
        texture.SetPixels32(combinedGraphPoint.textureCoord.x, combinedGraphPoint.textureCoord.y, 1, 1, new Color32[] { new Color32(255, 0, 0, 255) });
        textureChanged = true;
    }

    public CombinedGraphPoint FindGraphPoint(string label)
    {
        return points[label];
    }

    /// <summary>
    /// Color all graphpoints in this graph with the expression of some gene.
    /// </summary>
    /// <param name="expressions">An arraylist with <see cref="CellExpressionPair"/>.</param>
    public void ColorByGeneExpression(ArrayList expressions)
    {

        // Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight);
        // System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);

        //UnityEngine.Color lowColor = CellexalConfig.LowExpressionColor;
        //UnityEngine.Color midColor = CellexalConfig.MidExpressionColor;
        //UnityEngine.Color highColor = CellexalConfig.HighExpressionColor;
        //int numExpressionColors = CellexalConfig.NumberOfExpressionColors;

        //Color[] lowMidExpressionBrushes = CellexalExtensions.Extensions.InterpolateColors(lowColor, midColor, numExpressionColors / 2);
        //Color[] midHighExpressionBrushes = CellexalExtensions.Extensions.InterpolateColors(midColor, highColor, numExpressionColors - numExpressionColors / 2);
        //Color[] expressionBrushes = new Color[numExpressionColors];
        //Array.Copy(lowMidExpressionBrushes, expressionBrushes, numExpressionColors / 2);
        //Array.Copy(midHighExpressionBrushes, 0, expressionBrushes, numExpressionColors / 2, numExpressionColors - numExpressionColors / 2);

        // cells that have 0 (or whatever the lowest is) expression are not in the results
        // fill entire background with the lowest expression color


        for (int i = 0; i < textureWidth; ++i)
        {
            for (int j = 0; j < textureHeight; ++j)
            {
                texture.SetPixel(i, j, Color.black);
            }
        }
        int nbrOfExpressionColors = CellexalConfig.NumberOfExpressionColors;
        Color32[][] colorValues = new Color32[nbrOfExpressionColors][];
        for (byte i = 0; i < nbrOfExpressionColors; ++i)
        {
            colorValues[i] = new Color32[] { new Color32(i, 0, 0, 1) };
        }

        foreach (CellExpressionPair pair in expressions)
        {
            Vector2Int pos = points[pair.Cell].textureCoord;
            int expressionColorIndex = pair.Color;
            float outlined = 0;
            if (pair.Color >= nbrOfExpressionColors)
            {
                outlined = 1 / 255f;
                expressionColorIndex = nbrOfExpressionColors - 1;
            }

            texture.SetPixels32(pos.x, pos.y, 1, 1, colorValues[expressionColorIndex]);
        }

        texture.Apply();
        //var pixels = texture.GetPixels32(0);
        //int[] hist = new int[nbrOfExpressionColors];
        //for (int i = 0; i < pixels.Length; ++i)
        //{
        //    hist[pixels[i].r]++;
        //}

        combinedGraphPointClusters[0].GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
    }

    /// <summary>
    /// Helper method to remove graphpoints from a dictionary.
    /// </summary>
    /// <param name="colliders"> An array with colliders attached to graphpoints. </param>
    /// <param name="set"> A hashset containing graphpoints. </param>
    private void RemoveGraphPointsFromSet(Collider[] colliders, ref HashSet<GraphPoint> set)
    {
        {
            foreach (Collider c in colliders)
            {
                GraphPoint p = c.gameObject.GetComponent<GraphPoint>();
                if (p)
                {
                    set.Remove(p);
                }
            }
        }
    }

    /// <summary>
    /// Helper method to count number of not yet added grapphpoints we collided with.
    /// </summary>
    /// <param name="colliders"> An array of colliders attached to graphpoints. </param>
    /// <param name="points"> A hashset containing graphpoints. </param>
    /// <returns> The number of graphpoints that were present in the dictionary. </returns>
    private int NumberOfNotIncludedColliders(Collider[] colliders, HashSet<GraphPoint> points)
    {
        int total = 0;
        foreach (Collider c in colliders)
        {
            GraphPoint p = c.gameObject.GetComponent<GraphPoint>();
            if (p)
            {
                total += points.Contains(p) ? 1 : 0;
            }
        }
        return total;
    }
    /// <summary>
    /// Finds all <see cref="CombinedGraphPoint"/> that are inside the selection tool. This is done by traversing the generated Octree and dismissing subtrees using Minkowski differences.
    /// Ultimately, raycasting is used to find collisions because the selection tool is not a box.
    /// </summary>
    /// <param name="selectionToolPos">The selection tool's position in world space.</param>
    /// <param name="selectionToolBoundsCenter">The selection tool's bounding box's center in world space.</param>
    /// <param name="selectionToolBoundsExtents">The selection tool's bounding box's extents in world space.</param>
    /// <param name="group">The group that the selection tool is set to color the graphpoints by.</param>
    /// <returns>A <see cref="List{CombinedGraphPoint}"/> with all <see cref="CombinedGraphPoint"/> that are inside the selecion tool.</returns>
    public List<CombinedGraphPoint> MinkowskiDetection(Vector3 selectionToolPos, Vector3 selectionToolBoundsCenter, Vector3 selectionToolBoundsExtents, int group, float ms)
    {
        List<CombinedGraphPoint> result = new List<CombinedGraphPoint>(64);
        //int calls = 0;
        //int callsEntirelyInside = 0;

        // calculate a new (non-minimal) bounding box for the selection tool, in the graph's local space
        Vector3 center = transform.InverseTransformPoint(selectionToolBoundsCenter);

        Vector3 axisX = transform.InverseTransformVector(selectionToolBoundsExtents.x, 0, 0);
        Vector3 axisY = transform.InverseTransformVector(0, selectionToolBoundsExtents.y, 0);
        Vector3 axisZ = transform.InverseTransformVector(0, 0, selectionToolBoundsExtents.z);

        float extentsx = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        float extentsy = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        float extentsz = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        Vector3 extents = new Vector3(extentsx, extentsy, extentsz);
        Vector3 min = center - extents;
        Vector3 max = center + extents;

        //debugGizmosMin = min;
        //debugGizmosMax = max;

        //debugGizmosPos = selectionToolPos;

        MinkowskiDetectionRecursive(selectionToolPos, min, max, octreeRoot, group, ref result, ms);
        return result;
    }

    /// <summary>
    /// Recursive function to traverse the Octree and find collisions with the selection tool.
    /// </summary>
    /// <param name="selectionToolWorldPos">The selection tool's position in world space.</param>
    /// <param name="boundingBoxMin">A <see cref="Vector3"/> comprised of the smallest x, y and z coordinates of the selection tool's bounding box.</param>
    /// <param name="boundingBoxMax">A <see cref="Vector3"/> comprised of the largest x, y and z coordinates of the selection tool's bounding box.</param>
    /// <param name="node">The <see cref="OctreeNode"/> to evaluate.</param>
    /// <param name="group">The group to assign the node.</param>
    /// <param name="result">All leaf nodes found so far.</param>
    private void MinkowskiDetectionRecursive(Vector3 selectionToolWorldPos, Vector3 boundingBoxMin, Vector3 boundingBoxMax, OctreeNode node, int group, ref List<CombinedGraphPoint> result, float ms)
    {
        //print(Time.realtimeSinceStartup);
        if (Time.realtimeSinceStartup - ms > 25)
        {
            print("stopped due to stopwatch - " + (Time.realtimeSinceStartup - ms));
            return;
        }
        //calls++;
        // minkowski difference selection tool bounding box and node
        // take advantage of both being AABB
        // check if result contains (0,0,0)
        if (boundingBoxMin.x - node.pos.x - node.size.x <= 0
            && boundingBoxMax.x - node.pos.x >= 0
            && boundingBoxMin.y - node.pos.y - node.size.y <= 0
            && boundingBoxMax.y - node.pos.y >= 0
            && boundingBoxMin.z - node.pos.z - node.size.z <= 0
            && boundingBoxMax.z - node.pos.z >= 0)
        {
            // check if this node is entirely inside the bounding box
            if (boundingBoxMin.x < node.pos.x && boundingBoxMax.x > node.pos.x + node.size.x &&
                boundingBoxMin.y < node.pos.y && boundingBoxMax.y > node.pos.y + node.size.y &&
                boundingBoxMin.z < node.pos.z && boundingBoxMax.z > node.pos.z + node.size.z)
            {
                // just find the leaves and check if they are inside

                if (node.Group != group)
                {
                    node.completelyInside = true;
                    CheckIfLeavesInside(selectionToolWorldPos, node, group, ref result, ms);
                }
                return;
            }

            // check if this is a leaf node that is inside the selection tool. Can't rely on bounding boxes here, have to raycast to find collisions
            if (node.point != null && node.Group != group && node.PointInsideSelectionTool(selectionToolWorldPos, transform.TransformPoint(node.center)))
            {
                node.Group = group;
                result.Add(node.point);
            }
            else
            {
                // recursion
                foreach (var child in node.children)
                {
                    if (child.Group != group)
                    {
                        MinkowskiDetectionRecursive(selectionToolWorldPos, boundingBoxMin, boundingBoxMax, child, group, ref result, ms);
                    }
                }
            }
        }
        else
        {
            node.rejected = true;
        }
    }

    /// <summary>
    /// Recursive function to traverse the Octree and find collisions with the selection tool.
    /// </summary>
    /// <param name="selectionToolWorldPos">The selection tool's position in world space.</param>
    /// <param name="node">The to evaluate.</param>
    /// <param name="group">The group to assign the node.</param>
    /// <param name="result">All leaf nodes found so far.</param>
    private void CheckIfLeavesInside(Vector3 selectionToolWorldPos, OctreeNode node, int group, ref List<CombinedGraphPoint> result, float ms)
    {
        if (Time.realtimeSinceStartup - ms > 250)
        {
            print("stopped due to stopwatch - " + (Time.realtimeSinceStartup - ms));
            return;
        }
        //callsEntirelyInside++;
        foreach (var child in node.children)
        {
            if (child.Group == group)
            {
                continue;
            }
            if (child.point != null && child.PointInsideSelectionTool(selectionToolWorldPos, transform.TransformPoint(child.center)))
            {
                child.Group = group;
                result.Add(child.point);
            }
            else
            {
                CheckIfLeavesInside(selectionToolWorldPos, child, group, ref result, ms);
            }
        }
    }
}
