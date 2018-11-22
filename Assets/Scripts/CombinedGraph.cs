using SQLiter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using System.Text;

/// <summary>
/// A graph that contain one or more <see cref="CombinedCluster"/> that in turn contain one or more <see cref="CombinedGraphPoint"/>.
/// </summary>
public class CombinedGraph : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public GameObject combinedGraphpointsPrefab;
    public GameObject clusterColliderContainer;
    public Mesh graphPointMesh;
    public Material combinedGraphPointMaterialPrefab;
    public GameObject skeletonPrefab;

    private Material combinedGraphPointMaterial;

    public Dictionary<string, CombinedGraphPoint> points = new Dictionary<string, CombinedGraphPoint>();
    public string GraphName { get; set; }
    public string DirectoryName { get; set; }

    private GraphManager graphManager;
    private List<GameObject> combinedGraphPointClusters = new List<GameObject>();

    private Vector3 minCoordValues = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    private Vector3 maxCoordValues = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    private Vector3 diffCoordValues;
    private float longestAxis;
    private Vector3 scaledOffset;

    private int nbrOfClusters;
    private int nbrOfMaxPointsPerClusters;

    private int textureWidth;
    private int textureHeight;
    private Texture2D texture;
    private bool textureChanged;
    private OctreeNode octreeRoot;
    private static LayerMask selectionToolLayerMask;

    private Vector3 debugGizmosPos;
    private Vector3 debugGizmosMin;
    private Vector3 debugGizmosMax;

    private void Start()
    {
        graphManager = referenceManager.graphManager;
        combinedGraphPointMaterial = Instantiate(combinedGraphPointMaterialPrefab);
        selectionToolLayerMask.value = 1 << LayerMask.NameToLayer("SelectionToolLayer");
    }

    private void Update()
    {
        if (textureChanged && texture != null)
        {
            texture.Apply();
            textureChanged = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SelectionTool"))
        {
            referenceManager.selectionToolHandler.TouchingGraph = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SelectionTool") && referenceManager.selectionToolHandler.TouchingGraph == this)
        {
            referenceManager.selectionToolHandler.TouchingGraph = null;
        }
    }

    /// <summary>
    /// Represents a point in this graph.
    /// </summary>
    public class CombinedGraphPoint
    {
        private static int indexCounter = 0;

        public string Label;
        public Vector3 Position;
        public int index;
        public Vector2Int textureCoord;
        public CombinedGraph parent;
        public Collider collider;
        public int group;

        public CombinedGraphPoint(string label, float x, float y, float z, CombinedGraph parent)
        {
            Label = label;
            Position = new Vector3(x, y, z);
            this.parent = parent;
            index = indexCounter;
            group = -1;

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

        public void Recolor(CombinedGraphPoint graphPoint, Color color, int group)
        {
            parent.RecolorGraphPoint(this, color);
            this.group = group;
        }
    }

    /// <summary>
    /// Private class to represent one node in the octree used for collision detection
    /// </summary>
    private class OctreeNode
    {
        public OctreeNode parent;
        public OctreeNode[] children;
        public CombinedGraphPoint point;
        public Vector3 pos;
        public Vector3 center;
        public Vector3 size;
        private int group = -1;
        private bool raycasted;
        public int Group
        {
            get { return group; }
            set { SetGroup(value); }
        }

        public OctreeNode() { }

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
            foreach (var child in children)
            {
                if (child.group != group)
                    return;
            }
            this.group = group;
            if (parent != null && parent.group != group)
            {
                parent.NotifyGroupChange(group);
            }
        }

        /// <summary>
        /// Returns the smallest node that contains a point, might not be a leaf.
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

        public void DrawDebugCubes(Vector3 gameobjectPos, bool onlyLeaves, int i)
        {
            Gizmos.color = GizmoColors(i++);
            if (!onlyLeaves || children.Length == 0 && onlyLeaves)
            {
                Gizmos.DrawWireCube(gameobjectPos + pos + size / 2, size / 0.95f);
            }
            foreach (var child in children)
            {
                child.DrawDebugCubes(gameobjectPos, onlyLeaves, i);
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
    }

    public void OnDrawGizmos()
    {
        if (octreeRoot != null)
        {
            if (graphManager.drawDebugCubes)
            {
                octreeRoot.DrawDebugCubes(transform.position, false, 0);
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
            Gizmos.DrawWireCube(transform.TransformPoint(debugGizmosMin + debugGizmosSize / 2), debugGizmosSize);
            Gizmos.DrawSphere(debugGizmosMin, 0.01f);
            Gizmos.DrawSphere(debugGizmosMax, 0.01f);
            //print("debug lines: " + debugGizmosMin.ToString() + " " + debugGizmosMax.ToString());
        }
        if (graphManager.drawDebugRaycast)
        {
            octreeRoot.DrawDebugRaycasts(transform.position, debugGizmosPos);
        }
    }

    /// <summary>
    /// Recolors a single graphpoint.
    /// </summary>
    /// <param name="label">The graphpoint's label (the cell's name).</param>
    /// <param name="color">The graphpoint's new color.</param>
    public void RecolorGraphPoint(string label, Color color)
    {
        RecolorGraphPoint(points[label], color);
    }

    /// <summary>
    /// Recolors a single graphpoint.
    /// </summary>
    /// <param name="combinedGraphPoint">The graphpoint to recolor.</param>
    /// <param name="color">The graphpoint's new color.</param>
    public void RecolorGraphPoint(CombinedGraphPoint combinedGraphPoint, Color color)
    {
        texture.SetPixel(combinedGraphPoint.textureCoord.x, combinedGraphPoint.textureCoord.y, color);
        textureChanged = true;
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

    /// <summary>
    /// Adds a graphpoint to this graph. The coordinates should be scaled later with <see cref="ScaleAllCoordinates"/>.
    /// </summary>
    /// <param name="label">The graphpoint's (cell's) name.</param>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    public void AddGraphPoint(string label, float x, float y, float z)
    {
        points[label] = (new CombinedGraphPoint(label, x, y, z, this));
        UpdateMinMaxCoords(x, y, z);
    }

    /// <summary>
    /// Color all graphpoints in this graph with the expression of some gene.
    /// </summary>
    /// <param name="expressions">An arraylist with <see cref="CellExpressionPair"/>.</param>
    public void ColorByGeneExpression(ArrayList expressions)
    {

        // Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight);
        // System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);

        UnityEngine.Color lowColor = CellexalConfig.LowExpressionColor;
        UnityEngine.Color midColor = CellexalConfig.MidExpressionColor;
        UnityEngine.Color highColor = CellexalConfig.HighExpressionColor;
        int numExpressionColors = CellexalConfig.NumberOfExpressionColors;

        Color[] lowMidExpressionBrushes = CellexalExtensions.Extensions.InterpolateColors(lowColor, midColor, numExpressionColors / 2);
        Color[] midHighExpressionBrushes = CellexalExtensions.Extensions.InterpolateColors(midColor, highColor, numExpressionColors - numExpressionColors / 2);
        Color[] expressionBrushes = new Color[numExpressionColors];
        Array.Copy(lowMidExpressionBrushes, expressionBrushes, numExpressionColors / 2);
        Array.Copy(midHighExpressionBrushes, 0, expressionBrushes, numExpressionColors / 2, numExpressionColors - numExpressionColors / 2);

        // cells that have 0 (or whatever the lowest is) expression are not in the results
        // fill entire background with the lowest expression color
        //graphics.FillRectangle(expressionBrushes[0], 0f, 0f, bitmapWidth, bitmapHeight);
        for (int i = 0; i < textureWidth; ++i)
        {
            for (int j = 0; j < textureHeight; ++j)
            {
                texture.SetPixel(i, j, expressionBrushes[0]);
            }
        }

        foreach (CellExpressionPair pair in expressions)
        {
            Vector2Int pos = points[pair.Cell].textureCoord;
            if (pair.Color >= expressionBrushes.Length)
            {
                pair.Color = expressionBrushes.Length - 1;
            }
            texture.SetPixel(pos.x, pos.y, expressionBrushes[pair.Color]);
        }

        texture.Apply();
        combinedGraphPointClusters[0].GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
    }

    /// <summary>
    /// The squared euclidean distance between two vectors.
    /// </summary>
    private float SquaredEuclideanDistance(Vector3 v1, Vector3 v2)
    {
        float distance = Vector3.Distance(v1, v2);
        return distance * distance;
    }

    /// <summary>
    /// Divides the graph into clusters. The graph starts out as one large cluster and is recursively divided into smaller and smaller clusters until all clusters can be rendered in Unity using a single mesh.
    /// </summary>
    public void SliceClustering()
    {
        //    SliceClusteringCoroutine();
        //}

        //private void SliceClusteringCoroutine()
        //{

        ScaleAllCoordinates();

        // unty meshes can have a max of 65534 vertices
        int maxVerticesPerMesh = 65534;
        nbrOfMaxPointsPerClusters = maxVerticesPerMesh / graphPointMesh.vertexCount;
        // place all points in one big cluster
        var firstCluster = new HashSet<CombinedGraphPoint>();
        foreach (var point in points.Values)
        {
            firstCluster.Add(point);
        }

        List<HashSet<CombinedGraphPoint>> clusters = new List<HashSet<CombinedGraphPoint>>();
        clusters = SplitCluster(firstCluster);
        MakeMeshes(clusters);
        //CreateColliders();
    }

    /// <summary>
    /// Helper method for clustering. Splits the first cluster.
    /// </summary>
    /// <param name="cluster">A cluster containing all the points in the graph.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="HashSet{T}"/> that each contain one cluster.</returns>
    private List<HashSet<CombinedGraphPoint>> SplitCluster(HashSet<CombinedGraphPoint> cluster)
    {
        var clusters = new List<HashSet<CombinedGraphPoint>>();
        octreeRoot = new OctreeNode();
        octreeRoot.pos = new Vector3(-0.5f, -0.5f, -0.5f);
        octreeRoot.size = new Vector3(1f, 1f, 1f);
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        clusters = SplitClusterRecursive(cluster, octreeRoot, true);
        nbrOfClusters = clusters.Count;
        stopwatch.Stop();
        CellexalLog.Log(string.Format("clustered {0} in {1}. nbr of clusters: {2}", GraphName, stopwatch.Elapsed.ToString(), nbrOfClusters));
        return clusters;

    }

    /// <summary>
    /// Helper method for clustering. Divides one cluster into up to eight smaller clusters if it is too large and returns the non-empty new clusters.
    /// </summary>
    /// <param name="cluster">The cluster to split.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="HashSet{T}"/> that each contain one cluster.</returns>
    private List<HashSet<CombinedGraphPoint>> SplitClusterRecursive(HashSet<CombinedGraphPoint> cluster, OctreeNode node, bool addClusters)
    {

        if (cluster.Count == 1)
        {
            node.children = new OctreeNode[0];
            var point = cluster.First();
            node.point = point;
            node.size = graphPointMesh.bounds.size;
            //node.size = new Vector3(0.03f, 0.03f, 0.03f);
            node.pos = point.Position - node.size / 2;
            node.center = point.Position;
            return null;
        }

        var result = new List<HashSet<CombinedGraphPoint>>();
        if (cluster.Count <= nbrOfMaxPointsPerClusters && addClusters)
        {
            result.Add(cluster);
            addClusters = false;
        }

        // cluster is too big, split it
        // calculate center
        Vector3 splitCenter = Vector3.zero;
        Vector3 nodePos = node.pos;
        Vector3 nodeSize = node.size;
        foreach (var point in cluster)
        {
            splitCenter += point.Position;
        }
        splitCenter /= cluster.Count;
        node.center = splitCenter;

        // initialise new clusters
        List<HashSet<CombinedGraphPoint>> newClusters = new List<HashSet<CombinedGraphPoint>>(8);
        for (int i = 0; i < 8; ++i)
        {
            newClusters.Add(new HashSet<CombinedGraphPoint>());
        }

        // assign points to the new clusters
        foreach (var point in cluster)
        {
            int chosenClusterIndex = 0;
            Vector3 position = point.Position;
            if (position.x > splitCenter.x)
            {
                chosenClusterIndex += 1;
            }
            if (position.y > splitCenter.y)
            {
                chosenClusterIndex += 2;
            }
            if (position.z > splitCenter.z)
            {
                chosenClusterIndex += 4;
            }
            newClusters[chosenClusterIndex].Add(point);
        }

        node.children = new OctreeNode[newClusters.Count((HashSet<CombinedGraphPoint> c) => c.Count > 0)];
        Vector3[] newOctreePos = new Vector3[] {
            nodePos,
            new Vector3(splitCenter.x,  nodePos.y,      nodePos.z),
            new Vector3(nodePos.x,      splitCenter.y,  nodePos.z),
            new Vector3(splitCenter.x,  splitCenter.y,  node.pos.z),
            new Vector3(nodePos.x,      nodePos.y ,     splitCenter.z),
            new Vector3(splitCenter.x,  nodePos.y,      splitCenter.z),
            new Vector3(nodePos.x,      splitCenter.y,  splitCenter.z),
            splitCenter };

        Vector3[] newOctreeSizes = new Vector3[]
        {
            splitCenter - nodePos,
            new Vector3(nodeSize.x - (splitCenter.x - nodePos.x), splitCenter.y - nodePos.y,                splitCenter.z - nodePos.z),
            new Vector3(splitCenter.x - nodePos.x,                nodeSize.y - (splitCenter.y - nodePos.y), splitCenter.z - nodePos.z),
            new Vector3(nodeSize.x - (splitCenter.x - nodePos.x), nodeSize.y - (splitCenter.y - nodePos.y), splitCenter.z - nodePos.z),
            new Vector3(splitCenter.x - nodePos.x,                splitCenter.y - nodePos.y ,               nodeSize.z - (splitCenter.z - nodePos.z)),
            new Vector3(nodeSize.x - (splitCenter.x - nodePos.x), splitCenter.y - nodePos.y,                nodeSize.z - (splitCenter.z - nodePos.z)),
            new Vector3(splitCenter.x - nodePos.x,                nodeSize.y - (splitCenter.y - nodePos.y), nodeSize.z - (splitCenter.z - nodePos.z)),
            nodeSize - (splitCenter - nodePos)
        };

        for (int i = 0, j = 0; j < 8; ++i, ++j)
        {
            // remove empty clusters
            if (newClusters[i].Count == 0)
            {
                newClusters.RemoveAt(i);
                --i;
            }
            else
            {
                var newOctreeNode = new OctreeNode();
                newOctreeNode.parent = node;
                newOctreeNode.pos = newOctreePos[j];
                newOctreeNode.size = newOctreeSizes[j];
                node.children[i] = newOctreeNode;
            }
        }

        // call recursively for each new cluster
        for (int i = 0; i < newClusters.Count; ++i)
        {
            var returnedClusters = SplitClusterRecursive(newClusters[i], node.children[i], addClusters);
            if (returnedClusters != null)
            {
                result.AddRange(returnedClusters);
            }
        }
        return result;
    }

    /// <summary>
    /// Scales the coordinates of all graphpoints to fit inside a 1x1x1 meter cube. Should be called before <see cref="MakeMeshes(List{HashSet{CombinedGraphPoint}})"/>.
    /// </summary>
    private void ScaleAllCoordinates()
    {
        maxCoordValues += graphPointMesh.bounds.size;
        minCoordValues -= graphPointMesh.bounds.size;
        diffCoordValues = maxCoordValues - minCoordValues;
        longestAxis = Mathf.Max(diffCoordValues.x, diffCoordValues.y, diffCoordValues.z);
        // making the largest axis longer by the length of two graphpoint meshes makes no part of the graphpoints peak out of the 1x1x1 meter bounding cube when positioned close to the borders
        //var graphPointMeshBounds = graphPointMesh.bounds;
        //float longestAxisGraphPointMesh = Mathf.Max(graphPointMeshBounds.size.x, graphPointMeshBounds.size.y, graphPointMeshBounds.size.z);
        //longestAxis += longestAxisGraphPointMesh;
        scaledOffset = (diffCoordValues / longestAxis) / 2;

        points.Values.All((CombinedGraphPoint p) => { p.ScaleCoordinates(); return true; });
    }

    /// <summary>
    /// Creates the meshes from the clusters given by <see cref="SplitCluster(HashSet{CombinedGraphPoint})"/>.
    /// </summary>
    private void MakeMeshes(List<HashSet<CombinedGraphPoint>> clusters)
    {
        StartCoroutine(MakeMeshesCoroutine(clusters));
    }

    /// <summary>
    /// Coroutine that makes some meshes every frame. Uses the new job system to do things parallel.
    /// </summary>
    private IEnumerator MakeMeshesCoroutine(List<HashSet<CombinedGraphPoint>> clusters)
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        List<Mesh> meshes = new List<Mesh>();
        CombineInstance[] combine = new CombineInstance[nbrOfMaxPointsPerClusters];
        CombineInstance emptyCombineInstance = new CombineInstance();
        combinedGraphPointClusters = new List<GameObject>(nbrOfClusters);
        emptyCombineInstance.mesh = new Mesh();
        int graphPointMeshVertexCount = graphPointMesh.vertexCount;
        int graphPointMeshTriangleCount = graphPointMesh.triangles.Length;

        NativeArray<Vector3> graphPointMeshVertices = new NativeArray<Vector3>(graphPointMesh.vertices, Allocator.TempJob);
        NativeArray<int> graphPointMeshTriangles = new NativeArray<int>(graphPointMesh.triangles, Allocator.TempJob);
        NativeArray<int> clusterOffsets = new NativeArray<int>(nbrOfClusters, Allocator.TempJob);
        NativeArray<Vector3> positions = new NativeArray<Vector3>(points.Count, Allocator.TempJob);
        NativeArray<Vector3> resultVertices = new NativeArray<Vector3>(points.Count * graphPointMeshVertexCount, Allocator.TempJob);
        NativeArray<int> resultTriangles = new NativeArray<int>(points.Count * graphPointMeshTriangleCount, Allocator.TempJob);
        NativeArray<Vector2> resultUVs = new NativeArray<Vector2>(points.Count * graphPointMeshVertexCount, Allocator.TempJob);

        int totalNumberOfPoints = points.Count;

        int lastClusterOffset = 0;
        for (int i = 0; i < nbrOfClusters; ++i)
        {
            var cluster = clusters[i];
            int clusterOffset = cluster.Count;
            clusterOffsets[0] = 0;
            if (i < clusterOffsets.Length - 1)
            {
                clusterOffsets[i + 1] = clusterOffsets[i] + cluster.Count;
            }

            int j = 0;
            foreach (var point in cluster)
            {
                positions[clusterOffsets[i] + j] = point.Position;
                point.SetTextureCoord(new Vector2Int(j, i));
                j++;
            }
            lastClusterOffset = clusterOffset;
        }

        var job = new CombineMeshesJob()
        {
            // input
            positions = positions,
            vertices = graphPointMeshVertices,
            triangles = graphPointMeshTriangles,
            clusterOffsets = clusterOffsets,
            clusterMaxSize = nbrOfMaxPointsPerClusters,
            // output
            resultVertices = resultVertices,
            resultTriangles = resultTriangles,
            resultUVs = resultUVs
        };

        var handle = job.Schedule(nbrOfClusters, 1);
        yield return new WaitWhile(() => !handle.IsCompleted);
        handle.Complete();

        int itemsThisFrame = 0;
        int maximumItemsPerFrame = CellexalConfig.GraphClustersPerFrameStartCount;
        int maximumItemsPerFrameInc = CellexalConfig.GraphClustersPerFrameIncrement;
        float maximumDeltaTime = 0.05f;

        for (int i = 0; i < nbrOfClusters; ++i)
        {
            var newPart = Instantiate(combinedGraphpointsPrefab, transform);
            var newMesh = new Mesh();
            int clusterOffset = clusterOffsets[i];
            int clusterSize = 0;
            if (i < clusterOffsets.Length - 1)
            {
                clusterSize = clusterOffsets[i + 1] - clusterOffsets[i];
            }
            else
            {
                clusterSize = points.Count - clusterOffsets[i];
            }
            int nbrOfVerticesInCluster = clusterSize * graphPointMeshVertexCount;
            int nbrOfTrianglesInCluster = clusterSize * graphPointMeshTriangleCount;
            int vertexOffset = clusterOffset * graphPointMeshVertexCount;
            int triangleOffset = clusterOffset * graphPointMeshTriangleCount;

            // copy the vertices, uvs and triangles to the new mesh
            newMesh.vertices = new NativeSlice<Vector3>(job.resultVertices, vertexOffset, nbrOfVerticesInCluster).ToArray();
            newMesh.uv = new NativeSlice<Vector2>(job.resultUVs, vertexOffset, nbrOfVerticesInCluster).ToArray();
            newMesh.triangles = new NativeSlice<int>(job.resultTriangles, triangleOffset, nbrOfTrianglesInCluster).ToArray();

            //newMesh.vertices = new Vector3[nbrOfVerticesInCluster];
            //newMesh.uv = new Vector2[nbrOfVerticesInCluster];
            //newMesh.triangles = new int[nbrOfTrianglesInCluster];

            //job.resultVertices.CopySliceTo(newMesh.vertices, vertexOffset, nbrOfVerticesInCluster);
            //job.resultUVs.CopySliceTo(newMesh.uv, vertexOffset, nbrOfVerticesInCluster);
            //job.resultTriangles.CopySliceTo(newMesh.triangles, triangleOffset, nbrOfTrianglesInCluster);

            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();

            newPart.GetComponent<MeshFilter>().mesh = newMesh;
            combinedGraphPointClusters.Add(newPart);
            newPart.GetComponent<Renderer>().sharedMaterial = combinedGraphPointMaterial;

            itemsThisFrame++;
            if (itemsThisFrame >= maximumItemsPerFrame)
            {
                itemsThisFrame = 0;
                // wait one frame
                yield return null;
                // now is the next frame
                float lastFrame = Time.deltaTime;
                if (lastFrame < maximumDeltaTime)
                {
                    // we had some time over last frame
                    maximumItemsPerFrame += maximumItemsPerFrameInc;
                }
                else if (lastFrame > maximumDeltaTime && maximumItemsPerFrame > maximumItemsPerFrameInc * 2)
                {
                    // we took too much time last frame
                    maximumItemsPerFrame -= maximumItemsPerFrameInc;
                }
            }
        }
        job.positions.Dispose();
        job.clusterOffsets.Dispose();
        //job.vertices.Dispose();
        //job.triangles.Dispose();
        job.resultVertices.Dispose();
        job.resultTriangles.Dispose();
        job.resultUVs.Dispose();

        graphPointMeshVertices.Dispose();
        graphPointMeshTriangles.Dispose();

        textureWidth = nbrOfMaxPointsPerClusters;
        textureHeight = nbrOfClusters;
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
        combinedGraphPointClusters[0].GetComponent<Renderer>().sharedMaterial.mainTexture = texture;

        stopwatch.Stop();
        CellexalLog.Log(string.Format("made meshes for {0} in {1}", GraphName, stopwatch.Elapsed.ToString()));
        yield break;
    }

    /// <summary>
    /// Job that combines the meshes of one cluster.
    /// </summary>
    public struct CombineMeshesJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector3> positions;
        [ReadOnly]
        public NativeArray<Vector3> vertices;
        [ReadOnly]
        public NativeArray<int> triangles;
        [ReadOnly]
        public NativeArray<int> clusterOffsets;
        [ReadOnly]
        public int clusterMaxSize;
        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector3> resultVertices;
        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<int> resultTriangles;
        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector2> resultUVs;

        // each call to execute merges the meshes of one cluster
        public void Execute(int index)
        {
            int nbrOfPoints = 0;
            if (index < clusterOffsets.Length - 1)
            {
                nbrOfPoints = clusterOffsets[index + 1] - clusterOffsets[index];
            }
            else
            {
                nbrOfPoints = positions.Length - clusterOffsets[index];
            }
            int nbrOfVertices = vertices.Length;
            int nbrOfTriangles = triangles.Length;
            int vertexIndexOffset = clusterOffsets[index] * nbrOfVertices;
            int triangleIndexOffset = clusterOffsets[index] * nbrOfTriangles;
            float clusterUVY = (index + 0.5f) / clusterOffsets.Length;

            for (int i = 0; i < nbrOfPoints; ++i)
            {
                Vector3 pointPosition = positions[clusterOffsets[index] + i];
                float pointUVX = (i + 0.5f) / clusterMaxSize;

                for (int j = 0; j < nbrOfVertices; ++j)
                {
                    resultVertices[vertexIndexOffset + j] = vertices[j] + pointPosition;
                    resultUVs[vertexIndexOffset + j] = new Vector2(pointUVX, clusterUVY);
                }

                for (int j = 0; j < nbrOfTriangles; ++j)
                {
                    resultTriangles[triangleIndexOffset + j] = triangles[j] + i * nbrOfVertices;
                }

                vertexIndexOffset += nbrOfVertices;
                triangleIndexOffset += nbrOfTriangles;
            }
        }
    }

    /// <summary>
    /// Updates the min and max coordinates for the <see cref="ScaleAllCoordinates"/>.
    /// </summary>
    /// <param name="x">The x-coordinate of a graphpoint added to this graph.</param>
    /// <param name="y">The y-coordinate of a graphpoint added to this graph.</param>
    /// <param name="z">The z-coordinate of a graphpoint added to this graph.</param>
    private void UpdateMinMaxCoords(float x, float y, float z)
    {
        if (x < minCoordValues.x)
            minCoordValues.x = x;
        if (y < minCoordValues.y)
            minCoordValues.y = y;
        if (z < minCoordValues.z)
            minCoordValues.z = z;
        if (x > maxCoordValues.x)
            maxCoordValues.x = x;
        if (y > maxCoordValues.y)
            maxCoordValues.y = y;
        if (z > maxCoordValues.z)
            maxCoordValues.z = z;
    }

    /// <summary>
    /// Adds many boxcolliders to this graph. The idea is that when grabbing graphs we do not want to collide with all the small colliders on the graphpoints, so we put many boxcolliders that cover the graph instead.
    /// </summary>
    public void CreateColliders()
    {
        // maximum number of times we allow colliders to grow in size
        int maxColliderIncreaseIterations = 10;
        // how many more graphpoints there must be for it to be worth exctending a collider
        float extensionThreshold = 0.1f /*CellexalConfig.GraphGrabbableCollidersExtensionThresehold*/;
        // copy points dictionary
        HashSet<CombinedGraphPoint> notIncluded = new HashSet<CombinedGraphPoint>(points.Values);

        LayerMask layerMask = 1 << LayerMask.NameToLayer("GraphPointLayer");

        while (notIncluded.Count > (points.Count / 100))
        {
            // get any graphpoint
            CombinedGraphPoint point = notIncluded.First();
            Vector3 center = point.Position;
            Vector3 halfExtents = new Vector3(0.01f, 0.01f, 0.01f);
            Vector3 oldHalfExtents = halfExtents;

            for (int j = 0; j < maxColliderIncreaseIterations; ++j)
            {
                // find the graphspoints it is near
                Collider[] collidesWith = Physics.OverlapBox(center, halfExtents, Quaternion.identity, ~layerMask, QueryTriggerInteraction.Collide);
                // should we increase the size?

                // halfextents for new boxes
                Vector3 newHalfExtents = halfExtents + oldHalfExtents;

                // check how many colliders there are surrounding us
                Collider[] collidesWithx1 = Physics.OverlapBox(center, newHalfExtents, Quaternion.identity, ~layerMask, QueryTriggerInteraction.Collide);

                bool extended = false;
                // increase the halfextents if it seems worth it
                int currentlyCollidingWith = (int)(collidesWith.Length * extensionThreshold);
                if (NumberOfNotIncludedColliders(collidesWithx1, notIncluded) > currentlyCollidingWith)
                {
                    halfExtents.x += oldHalfExtents.x;
                    center.x += oldHalfExtents.x / 2f;
                    extended = true;
                }

                // remove all the graphpoints that collide with this new collider
                collidesWith = Physics.OverlapBox(center, halfExtents, Quaternion.identity, ~layerMask, QueryTriggerInteraction.Collide);
                RemoveGraphPointsFromSet(collidesWith, ref notIncluded);
                if (!extended) break;
            }
            // add the collider
            BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
            newCollider.center = transform.InverseTransformPoint(center);
            newCollider.size = halfExtents * 2;
        }
    }

    /// <summary>
    /// Reads the .hull file that belongs to this graph and creates a skeleton that resembles the graph.
    /// </summary>
    /// <returns>The instantiated skeleton <see cref="GameObject"/>.</returns>
    public GameObject CreateGraphSkeleton()
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
        foreach (var point in points.Values)
        {
            vertices[point.index] = point.Position;
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
        var mesh = new Mesh()
        {
            vertices = vertices,
            triangles = triangles.ToArray()
        };
        //var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
        //meshSimplifier.Initialize(mesh);
        //float quality = 255f / mesh.triangles.Length;
        //meshSimplifier.SimplifyMesh(quality);
        //convexHull.mesh = meshSimplifier.ToMesh();

        convexHull.mesh = mesh;
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
        CellexalLog.Log("Created convex hull with " + vertices.Count() + " vertices");
        return convexHull.gameObject;
    }

    /// <summary>
    /// Helper method to remove graphpoints from a dictionary.
    /// </summary>
    /// <param name="colliders"> An array with colliders attached to graphpoints. </param>
    /// <param name="set"> A hashset containing graphpoints. </param>
    private void RemoveGraphPointsFromSet(Collider[] colliders, ref HashSet<CombinedGraphPoint> set)
    {
        foreach (Collider c in colliders)
        {
            CombinedGraphPoint p = c.gameObject.GetComponent<CombinedGraphPoint>();
            if (p != null)
            {
                set.Remove(p);
            }
        }
    }

    /// <summary>
    /// Helper method to count number of not yet added grapphpoints we collided with.
    /// </summary>
    /// <param name="colliders"> An array of colliders attached to graphpoints. </param>
    /// <param name="points"> A hashset containing graphpoints. </param>
    /// <returns> The number of graphpoints that were present in the dictionary. </returns>
    private int NumberOfNotIncludedColliders(Collider[] colliders, HashSet<CombinedGraphPoint> points)
    {
        int total = 0;
        foreach (Collider c in colliders)
        {
            CombinedGraphPoint p = c.gameObject.GetComponent<CombinedGraphPoint>();
            if (p != null)
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
    public List<CombinedGraphPoint> MinkowskiDetection(Vector3 selectionToolPos, Vector3 selectionToolBoundsCenter, Vector3 selectionToolBoundsExtents, int group)
    {
        List<CombinedGraphPoint> result = new List<CombinedGraphPoint>(64);
        int calls = 0;
        int callsEntirelyInside = 0;

        Vector3 center = transform.InverseTransformPoint(selectionToolBoundsCenter);
        Vector3 extents = Vector3.zero;

        Vector3 axisX = transform.InverseTransformVector(selectionToolBoundsExtents.x, 0, 0);
        Vector3 axisY = transform.InverseTransformVector(0, selectionToolBoundsExtents.y, 0);
        Vector3 axisZ = transform.InverseTransformVector(0, 0, selectionToolBoundsExtents.z);

        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        Vector3 min = center - extents;
        Vector3 max = center + extents;

        debugGizmosMin = min;
        debugGizmosMax = max;

        debugGizmosPos = selectionToolPos;

        MinkowskiDetectionRecursive(selectionToolPos, min, max, octreeRoot, group, ref result, ref calls, ref callsEntirelyInside);
        print("minkowski calls:  " + calls + " entirely inside: " + callsEntirelyInside);
        return result;
    }

    /// <summary>
    /// Recursive function to traverse the Octree and find collisions with the selection tool.
    /// </summary>
    /// <param name="selectionToolWorldPos">The selection tool's position in world space.</param>
    /// <param name="boundingBoxMin">A <see cref="Vector3"/> comprised of the smallest x, y and z coordinates of the selection tool's bounding box.</param>
    /// <param name="boundingBoxMax">A <see cref="Vector3"/> comprised of the largest x, y and z coordinates of the selection tool's bounding box.</param>
    /// <param name="node">The to evaluate.</param>
    /// <param name="group">The group to assign the node.</param>
    /// <param name="result">All leaf nodes found so far.</param>
    private void MinkowskiDetectionRecursive(Vector3 selectionToolWorldPos, Vector3 boundingBoxMin, Vector3 boundingBoxMax, OctreeNode node, int group, ref List<CombinedGraphPoint> result, ref int calls, ref int callsEntirelyInside)
    {
        calls++;
        // minkowski difference selection tool and node
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
                    CheckIfLeavesInside(selectionToolWorldPos, node, group, ref result, ref callsEntirelyInside);
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
                        MinkowskiDetectionRecursive(selectionToolWorldPos, boundingBoxMin, boundingBoxMax, child, group, ref result, ref calls, ref callsEntirelyInside);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Recursive function to traverse the Octree and find collisions with the selection tool.
    /// </summary>
    /// <param name="selectionToolWorldPos">The selection tool's position in world space.</param>
    /// <param name="node">The to evaluate.</param>
    /// <param name="group">The group to assign the node.</param>
    /// <param name="result">All leaf nodes found so far.</param>
    private void CheckIfLeavesInside(Vector3 selectionToolWorldPos, OctreeNode node, int group, ref List<CombinedGraphPoint> result, ref int callsEntirelyInside)
    {
        callsEntirelyInside++;
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
                CheckIfLeavesInside(selectionToolWorldPos, child, group, ref result, ref callsEntirelyInside);
            }
        }
    }
}
