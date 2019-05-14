using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using TMPro;
using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.AnalysisLogic;
using CellexalVR.DesktopUI;
using System.Threading;

namespace CellexalVR.AnalysisObjects
{
    /// <summary>
    /// A graph that contain one or more <see cref="CombinedCluster"/> that in turn contain one or more <see cref="CombinedGraphPoint"/>.
    /// </summary>
    public class GraphGenerator : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject graphpointsPrefab;
        public GameObject graphPrefab;
        public GameObject AxesPrefab;
        public Material graphPointMaterialPrefab;
        public GameObject skeletonPrefab;
        public Mesh graphPointMesh;
        public string DirectoryName { get; set; }
        public bool isCreating;
        public bool addingToExisting;
        public enum GraphType { MDS, FACS, ATTRIBUTE };

        private GraphType graphType;
        private Graph newGraph;
        private GraphManager graphManager;
        private int nbrOfClusters;
        private int nbrOfMaxPointsPerClusters;
        //private Color[] graphpointColors;
        public Texture2D graphPointColors;
        private Vector3[] startPositions =  {   new Vector3(-0.2f, 1.1f, -0.95f),
                                            new Vector3(-0.9f, 1.1f, -0.4f),
                                            new Vector3(-0.9f, 1.1f, 0.4f),
                                            new Vector3(-0.2f, 1.1f, 0.95f),
                                            new Vector3(0.8f, 1.1f, -0.4f),
                                            new Vector3(0.5f, 1.1f, 0.7f)
                                        };
        public int graphCount;

        private GameManager gameManager;
        //private Graph subGraph;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Awake()
        {
            graphManager = referenceManager.graphManager;
            gameManager = referenceManager.gameManager;
            CellexalEvents.ConfigLoaded.AddListener(CreateShaderColors);
        }

        private void Update()
        {
            // foreach (CombinedGraph graph in graphManager.graphs)
            // {
            //     graph.combinedGraphPointClusters[0].GetComponent<MeshRenderer>().sharedMaterial.SetColorArray("_ExpressionColors", graphpointColors);
            //     graph.combinedGraphPointClusters[0].GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_GraphpointColorTex", graphPointColors);
            // }
        }

        public Graph CreateGraph(GraphType type)
        {
            graphType = type;
            newGraph = Instantiate(graphPrefab).GetComponent<Graph>();
            //graphManager.SetGraphStartPosition();
            newGraph.transform.position = startPositions[graphCount % 6];
            newGraph.referenceManager = referenceManager;
            newGraph.GetComponent<GraphInteract>().referenceManager = referenceManager;
            isCreating = true;
            graphCount++;
            return newGraph;
        }

        /// <summary>
        /// Creates the texture that the CombinedGraph shader uses to fetch colors for the graphpoints.
        /// </summary>
        public void CreateShaderColors()
        {
            Color lowColor = CellexalConfig.Config.GraphLowExpressionColor;
            Color midColor = CellexalConfig.Config.GraphMidExpressionColor;
            Color highColor = CellexalConfig.Config.GraphHighExpressionColor;
            int nbrOfExpressionColors = CellexalConfig.Config.GraphNumberOfExpressionColors;
            int nbrOfSelectionColors = CellexalConfig.Config.SelectionToolColors.Length;

            if (nbrOfExpressionColors + nbrOfSelectionColors > 255)
            {
                nbrOfExpressionColors = 255 - nbrOfSelectionColors;
                CellexalLog.Log(string.Format("ERROR: Can not have more than 255 total expression and selection colors. Reducing expression colors to {0}. Change NumberOfExpressionColors and SelectionToolColors in the config.txt.", nbrOfExpressionColors));
            }
            else if (nbrOfExpressionColors < 3)
            {
                CellexalLog.Log("ERROR: Can not have less than 3 gene expression colors. Increasing to 3. Change NumberOfExpressionColors in the config.txt.");
                nbrOfExpressionColors = 3;
            }
            int halfNbrOfExpressionColors = nbrOfExpressionColors / 2;

            Color[] lowMidExpressionColors = Extensions.Extensions.InterpolateColors(lowColor, midColor, halfNbrOfExpressionColors);
            Color[] midHighExpressionColors = Extensions.Extensions.InterpolateColors(midColor, highColor, nbrOfExpressionColors - halfNbrOfExpressionColors);


            //graphpointColors = new Color[256];
            //Array.Copy(lowMidExpressionColors, graphpointColors, halfNbrOfExpressionColors);
            //Array.Copy(midHighExpressionColors, 0, graphpointColors, halfNbrOfExpressionColors, nbrOfExpressionColors - halfNbrOfExpressionColors);
            //Array.Copy(CellexalConfig.SelectionToolColors, 0, graphpointColors, nbrOfExpressionColors, nbrOfSelectionColors);

            //// reservered colors
            //graphpointColors[255] = Color.white;

            graphPointColors = new Texture2D(256, 1, TextureFormat.ARGB32, false);
            int pixel = 0;
            for (int i = 0; i < halfNbrOfExpressionColors; ++i)
            {
                graphPointColors.SetPixel(pixel, 0, lowMidExpressionColors[i]);
                pixel++;
            }
            for (int i = 0; i < nbrOfExpressionColors - halfNbrOfExpressionColors; ++i)
            {
                graphPointColors.SetPixel(pixel, 0, midHighExpressionColors[i]);
                pixel++;
            }
            for (int i = 0; i < nbrOfSelectionColors; ++i)
            {
                graphPointColors.SetPixel(pixel, 0, CellexalConfig.Config.SelectionToolColors[i]);
                pixel++;
            }

            graphPointColors.SetPixel(255, 0, CellexalConfig.Config.GraphDefaultColor);
            graphPointColors.filterMode = FilterMode.Point;
            graphPointColors.Apply();

            foreach (Graph graph in graphManager.Graphs)
            {
                graph.graphPointClusters[0].GetComponent<Renderer>().sharedMaterial.SetTexture("_GraphpointColorTex", graphPointColors);
            }
        }

        /// <summary>
        /// Adds a graphpoint to this graph. The coordinates should be scaled later with <see cref="ScaleAllCoordinates"/>.
        /// </summary>
        /// <param name="label">The graphpoint's (cell's) name.</param>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="z">The z-coordinate.</param>
        public void AddGraphPoint(Cell cell, float x, float y, float z)
        {
            Graph.GraphPoint gp = new Graph.GraphPoint(cell.Label, x, y, z, newGraph);
            newGraph.points[cell.Label] = gp;
            newGraph.pointsPositions.Add(new Vector3(x, y, z));
            cell.AddGraphPoint(gp);
            UpdateMinMaxCoords(x, y, z);
            //print("adding points to - " + newGraph.gameObject.name + " - " + newGraph.points.Count);
        }

        /// <summary>
        /// The squared euclidean distance between two vectors.
        /// </summary>
        private float SquaredEuclideanDistance(Vector3 v1, Vector3 v2)
        {
            float distance = Vector3.Distance(v1, v2);
            return distance * distance;
        }

        public void AddAxes(Graph graph, string[] axisNames)
        {
            graph.axisNames = axisNames;
            Vector3 position = graph.ScaleCoordinates(graph.minCoordValues);
            GameObject axes = Instantiate(AxesPrefab, graph.transform);
            axes.transform.localPosition = position - (Vector3.one * 0.01f);
            Vector3 size = graph.GetComponent<BoxCollider>().size;
            float longestAx = Mathf.Max(Mathf.Max(size.x, size.y), size.z);
            axes.transform.localScale = Vector3.one * (longestAx * 0.6f);
            if (graphType == GraphType.FACS)
            {
                axes.GetComponent<AxesArrow>().SetColors(axisNames, graph.minCoordValues, graph.maxCoordValues);
                axes.SetActive(true);
            }
            else
            {
                axes.SetActive(false);
            }
            TextMeshPro[] texts = axes.GetComponentsInChildren<TextMeshPro>();
            graph.axes = axes;
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].text = axisNames[i];
            }
        }

        /// <summary>
        /// Divides the graph into clusters. The graph starts out as one large cluster and is recursively divided into smaller and smaller clusters until all clusters can be rendered in Unity using a single mesh.
        /// </summary>
        public void SliceClustering(Dictionary<string, Graph.GraphPoint> points = null)
        {
            ScaleAllCoordinates();

            // meshes in unity can have a max of 65534 vertices
            int maxVerticesPerMesh = 65534;
            nbrOfMaxPointsPerClusters = maxVerticesPerMesh / graphPointMesh.vertexCount;
            // place all points in one big cluster
            var firstCluster = new HashSet<Graph.GraphPoint>();
            if (points == null)
            {
                foreach (var point in newGraph.points.Values)
                {
                    firstCluster.Add(point);
                }
            }
            else
            {
                print("slice with points " + points.Values.Count);
                foreach (var point in points.Values)
                {
                    firstCluster.Add(point);
                }

            }
            //firstCluster.IntersectWith(newGraph.points.Values);

            List<HashSet<Graph.GraphPoint>> clusters = new List<HashSet<Graph.GraphPoint>>();
            clusters = SplitCluster(firstCluster);
            MakeMeshes(clusters);

            //if (graphType == GraphType.ATTRIBUTE)
            //{
            //}
            //else
            //{
            //    MakeMeshes(clusters);
            //}
            //CreateColliders();
        }


        /// <summary>
        /// Helper method for clustering. Splits the first cluster.
        /// </summary>
        /// <param name="cluster">A cluster containing all the points in the graph.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="HashSet{T}"/> that each contain one cluster.</returns>
        private List<HashSet<Graph.GraphPoint>> SplitCluster(HashSet<Graph.GraphPoint> cluster)
        {
            var clusters = new List<HashSet<Graph.GraphPoint>>();
            newGraph.octreeRoot = new Graph.OctreeNode();
            newGraph.octreeRoot.pos = new Vector3(-0.5f, -0.5f, -0.5f);
            newGraph.octreeRoot.size = new Vector3(1f, 1f, 1f);
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            clusters = SplitClusterRecursive(cluster, newGraph.octreeRoot, true);
            nbrOfClusters = clusters.Count;
            newGraph.GetComponent<BoxCollider>().size = newGraph.octreeRoot.size;
            stopwatch.Stop();
            CellexalLog.Log(string.Format("clustered {0} in {1}. nbr of clusters: {2}", newGraph.GraphName, stopwatch.Elapsed.ToString(), newGraph.nbrOfClusters));
            return clusters;

        }

        /// <summary>
        /// Helper method for clustering. Divides one cluster into up to eight smaller clusters if it is too large and returns the non-empty new clusters.
        /// </summary>
        /// <param name="cluster">The cluster to split.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="HashSet{T}"/> that each contain one cluster.</returns>
        private List<HashSet<Graph.GraphPoint>> SplitClusterRecursive(HashSet<Graph.GraphPoint> cluster, Graph.OctreeNode node, bool addClusters)
        {
            float graphpointMeshSize = graphPointMesh.bounds.size.x;
            float graphpointMeshExtent = graphpointMeshSize / 2f;
            if (cluster.Count == 1)
            {
                node.children = new Graph.OctreeNode[0];
                var point = cluster.First();
                node.point = point;
                point.node = node;
                node.size = graphPointMesh.bounds.size;
                //node.size = new Vector3(0.03f, 0.03f, 0.03f);
                node.pos = point.Position - node.size / 2;
                node.center = point.Position;
                return null;
            }

            var result = new List<HashSet<Graph.GraphPoint>>();
            if (cluster.Count <= nbrOfMaxPointsPerClusters && addClusters)
            {
                result.Add(cluster);
                addClusters = false;
            }

            // cluster is too big, split it
            // calculate center
            Vector3 splitCenter = Vector3.zero;
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;
            foreach (var point in cluster)
            {
                Vector3 pos = point.Position;
                splitCenter += pos;
                if (pos.x < minX)
                    minX = pos.x;
                if (pos.x > maxX)
                    maxX = pos.x;
                if (pos.y < minY)
                    minY = pos.y;
                if (pos.y > maxY)
                    maxY = pos.y;
                if (pos.z < minZ)
                    minZ = pos.z;
                if (pos.z > maxZ)
                    maxZ = pos.z;
            }
            splitCenter /= cluster.Count;
            node.center = splitCenter;
            node.pos = new Vector3(minX - graphpointMeshExtent, minY - graphpointMeshExtent, minZ - graphpointMeshExtent);
            Vector3 nodePos = node.pos;
            node.size = new Vector3(maxX - minX + graphpointMeshSize, maxY - minY + graphpointMeshSize, maxZ - minZ + graphpointMeshSize);
            Vector3 nodeSize = node.size;


            // initialise new clusters
            List<HashSet<Graph.GraphPoint>> newClusters = new List<HashSet<Graph.GraphPoint>>(8);
            for (int i = 0; i < 8; ++i)
            {
                newClusters.Add(new HashSet<Graph.GraphPoint>());
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

            node.children = new Graph.OctreeNode[newClusters.Count((HashSet<Graph.GraphPoint> c) => c.Count > 0)];
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
                    var newOctreeNode = new Graph.OctreeNode();
                    newOctreeNode.parent = node;
                    newOctreeNode.pos = newOctreePos[j];
                    newOctreeNode.size = newOctreeSizes[j];
                    node.children[i] = newOctreeNode;
                }
            }
            //print(newClusters.Count);
            //call recursively for each new cluster
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
            newGraph.maxCoordValues += graphPointMesh.bounds.size;
            newGraph.minCoordValues -= graphPointMesh.bounds.size;
            newGraph.diffCoordValues = newGraph.maxCoordValues - newGraph.minCoordValues;
            switch (graphType)
            {
                case GraphType.MDS:
                case GraphType.ATTRIBUTE:
                    newGraph.longestAxis = Mathf.Max(newGraph.diffCoordValues.x, newGraph.diffCoordValues.y, newGraph.diffCoordValues.z);
                    break;
                case GraphType.FACS:
                    newGraph.longestAxis = Mathf.Max(newGraph.diffCoordValues.x, newGraph.diffCoordValues.y, newGraph.diffCoordValues.z) * 2;
                    break;

            }
            // making the largest axis longer by the length of two graphpoint meshes makes no part of the graphpoints peek out of the 1x1x1 meter bounding cube when positioned close to the borders
            //var graphPointMeshBounds = graphPointMesh.bounds;
            //float longestAxisGraphPointMesh = Mathf.Max(graphPointMeshBounds.size.x, graphPointMeshBounds.size.y, graphPointMeshBounds.size.z);
            //longestAxis += longestAxisGraphPointMesh;
            newGraph.scaledOffset = (newGraph.diffCoordValues / newGraph.longestAxis) / 2;

            newGraph.points.Values.All((Graph.GraphPoint p) => { p.ScaleCoordinates(); return true; });
        }

        /// <summary>
        /// Creates the meshes from the clusters given by <see cref="SplitCluster(HashSet{CombinedGraphPoint})"/>.
        /// </summary>
        private void MakeMeshes(List<HashSet<Graph.GraphPoint>> clusters)
        {
            StartCoroutine(MakeMeshesCoroutine(clusters));
        }

        /// <summary>
        /// Coroutine that makes some meshes every frame. Uses the new job system to do things parallel.
        /// </summary>
        private IEnumerator MakeMeshesCoroutine(List<HashSet<Graph.GraphPoint>> clusters, BooleanExpression.Expr expr = null)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            newGraph.graphPointClusters = new List<GameObject>(nbrOfClusters);
            int graphPointMeshVertexCount = graphPointMesh.vertexCount;
            int graphPointMeshTriangleCount = graphPointMesh.triangles.Length;
            Material graphPointMaterial = Instantiate(graphPointMaterialPrefab);
            //List<Vector3> graphpointMeshNormals = new List<Vector3>(graphPointMeshVertexCount);
            //graphPointMesh.GetNormals(graphpointMeshNormals);

            NativeArray<Vector3> graphPointMeshVertices = new NativeArray<Vector3>(graphPointMesh.vertices, Allocator.TempJob);
            NativeArray<int> graphPointMeshTriangles = new NativeArray<int>(graphPointMesh.triangles, Allocator.TempJob);
            NativeArray<int> clusterOffsets = new NativeArray<int>(nbrOfClusters, Allocator.TempJob);
            NativeArray<Vector3> positions = new NativeArray<Vector3>(newGraph.points.Count, Allocator.TempJob);
            NativeArray<Vector3> resultVertices = new NativeArray<Vector3>(newGraph.points.Count * graphPointMeshVertexCount, Allocator.TempJob);
            NativeArray<int> resultTriangles = new NativeArray<int>(newGraph.points.Count * graphPointMeshTriangleCount, Allocator.TempJob);
            NativeArray<Vector2> resultUVs = new NativeArray<Vector2>(newGraph.points.Count * graphPointMeshVertexCount, Allocator.TempJob);

            int totalNumberOfPoints = newGraph.points.Count;
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
            int maximumItemsPerFrame = CellexalConfig.Config.GraphClustersPerFrameStartCount;
            int maximumItemsPerFrameInc = CellexalConfig.Config.GraphClustersPerFrameIncrement;
            float maximumDeltaTime = 0.05f;

            for (int i = 0; i < nbrOfClusters; ++i)
            {
                var newPart = Instantiate(graphpointsPrefab, newGraph.transform);
                //clusters[i].All((CombinedGraph.CombinedGraphPoint point) => (point.cluster = newPart));
                var newMesh = new Mesh();
                int clusterOffset = clusterOffsets[i];
                int clusterSize = 0;
                if (i < clusterOffsets.Length - 1)
                {
                    clusterSize = clusterOffsets[i + 1] - clusterOffsets[i];
                }
                else
                {
                    clusterSize = newGraph.points.Count - clusterOffsets[i];
                }
                int nbrOfVerticesInCluster = clusterSize * graphPointMeshVertexCount;
                int nbrOfTrianglesInCluster = clusterSize * graphPointMeshTriangleCount;
                int vertexOffset = clusterOffset * graphPointMeshVertexCount;
                int triangleOffset = clusterOffset * graphPointMeshTriangleCount;

                // copy the vertices, uvs and triangles to the new mesh
                newMesh.vertices = new NativeSlice<Vector3>(job.resultVertices, vertexOffset, nbrOfVerticesInCluster).ToArray();
                newMesh.uv = new NativeSlice<Vector2>(job.resultUVs, vertexOffset, nbrOfVerticesInCluster).ToArray();
                newMesh.triangles = new NativeSlice<int>(job.resultTriangles, triangleOffset, nbrOfTrianglesInCluster).ToArray();

                //List<Vector3> newMeshNormals = new List<Vector3>(clusterSize * graphpointMeshNormals.Count);
                //for (int j = 0; j < clusterSize; ++j)
                //{
                //    newMeshNormals.AddRange(graphpointMeshNormals);
                //}

                //newMesh.SetNormals(newMeshNormals);
                //newMesh.vertices = new Vector3[nbrOfVerticesInCluster];
                //newMesh.uv = new Vector2[nbrOfVerticesInCluster];
                //newMesh.triangles = new int[nbrOfTrianglesInCluster];

                //job.resultVertices.CopySliceTo(newMesh.vertices, vertexOffset, nbrOfVerticesInCluster);
                //job.resultUVs.CopySliceTo(newMesh.uv, vertexOffset, nbrOfVerticesInCluster);
                //job.resultTriangles.CopySliceTo(newMesh.triangles, triangleOffset, nbrOfTrianglesInCluster);

                newMesh.RecalculateBounds();
                newMesh.RecalculateNormals();

                newPart.GetComponent<MeshFilter>().mesh = newMesh;
                newGraph.graphPointClusters.Add(newPart);
                newPart.GetComponent<Renderer>().sharedMaterial = graphPointMaterial;

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

            newGraph.textureWidth = nbrOfMaxPointsPerClusters;
            newGraph.textureHeight = nbrOfClusters;
            Texture2D texture = new Texture2D(newGraph.textureWidth, newGraph.textureHeight, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Point;

            for (int i = 0; i < texture.width; ++i)
            {
                for (int j = 0; j < texture.height; ++j)
                {
                    texture.SetPixel(i, j, Color.red);
                }
            }
            texture.Apply();
            newGraph.texture = texture;
            var sharedMaterial = newGraph.graphPointClusters[0].GetComponent<Renderer>().sharedMaterial;
            sharedMaterial.mainTexture = newGraph.texture;

            Shader graphpointShader = sharedMaterial.shader;

            sharedMaterial.SetTexture("_GraphpointColorTex", graphPointColors);
            //sharedMaterial.SetColorArray("_ExpressionColors", graphpointColors);

            stopwatch.Stop();
            CellexalLog.Log(string.Format("made meshes for {0} in {1}", newGraph.GraphName, stopwatch.Elapsed.ToString()));
            isCreating = false;
            CellexalEvents.GraphsLoaded.Invoke();
            //yield break;
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
            if (x < newGraph.minCoordValues.x)
                newGraph.minCoordValues.x = x;
            if (y < newGraph.minCoordValues.y)
                newGraph.minCoordValues.y = y;
            if (z < newGraph.minCoordValues.z)
                newGraph.minCoordValues.z = z;
            if (x > newGraph.maxCoordValues.x)
                newGraph.maxCoordValues.x = x;
            if (y > newGraph.maxCoordValues.y)
                newGraph.maxCoordValues.y = y;
            if (z > newGraph.maxCoordValues.z)
                newGraph.maxCoordValues.z = z;
        }

        [ConsoleCommand("graphGenerator", "csg")]
        public void CreateSubGraphs()
        {
            List<string> attr = new List<string>
        {
            "celltype@Caudal.Mesoderm"
        };
            CreateSubGraphs(attr);
        }

        public void CreateSubGraphs(List<string> attributes)
        {
            BooleanExpression.Expr expr = new BooleanExpression.ValueExpr(attributes[0]);

            string name = attributes[0];
            if (name.Contains('@'))
            {
                name = name.Split('@')[1];
            }
            for (int i = 1; i < attributes.Count; i++)
            {
                if (attributes[i].Contains('@'))
                {
                    name += " - " + attributes[i].Split('@')[1];
                }
                else
                {
                    name += " - " + attributes[i];
                }
                BooleanExpression.Expr tempExpr = expr;
                expr = new BooleanExpression.OrExpr(tempExpr,
                                                    new BooleanExpression.ValueExpr(attributes[i]));
            }
            foreach (Graph g in graphManager.originalGraphs)
            {
                
                StartCoroutine(CreateSubGraphsCoroutine(expr, attributes, g, name));
            }
        }


        private IEnumerator CreateSubGraphsCoroutine(BooleanExpression.Expr expr, List<string> attributes, Graph g, string name)
        {
            while (isCreating)
            {
                yield return null;
            }
            var subGraph = CreateGraph(GraphType.ATTRIBUTE);
            subGraph.GraphName = name;
            subGraph.tag = "Subgraph";

            StartCoroutine(g.CreateGraphSkeleton(true));
            while (g.convexHull.activeSelf == false)
            {
                yield return null;
            }
            GameObject skeleton = g.convexHull;
            skeleton.transform.parent = subGraph.gameObject.transform;
            skeleton.transform.localPosition = Vector3.zero;

            List<Cell> subset = referenceManager.cellManager.SubSet(expr);

            Graph graph = g;
            foreach (Cell cell in subset)
            {
                var point = graph.FindGraphPoint(cell.Label).Position;
                AddGraphPoint(cell, point.x, point.y, point.z);
            }
            subGraph.maxCoordValues = graph.ScaleCoordinates(graph.maxCoordValues);
            subGraph.minCoordValues = graph.ScaleCoordinates(graph.minCoordValues);
            SliceClustering();
            subGraph.GetComponent<BoxCollider>().size = graph.GetComponent<BoxCollider>().size;
 
            while (isCreating)
            {
                yield return null;
            }


            //}
            foreach (string attribute in attributes)
            {
                referenceManager.cellManager.ColorByAttribute(attribute, true, true);
            }
            graphManager.Graphs.Add(subGraph);
            string[] axes = new string[3];
            axes = g.axisNames.ToArray();
            AddAxes(subGraph, axes);
        }

        //[ConsoleCommand("graphGenerator", "asg")]
        //public void AddToGraph()
        //{
        //    StartCoroutine(AddToSubGraphCoroutine("celltype@Epiblast"));
        //}

        //public void AddToGraph(string expr)
        //{
        //    StartCoroutine(AddToSubGraphCoroutine(expr));
        //}

        //private IEnumerator AddToSubGraphCoroutine(string expr)
        //{
        //    isCreating = true;
        //    List<Cell> subset = referenceManager.cellManager.SubSet(new BooleanExpression.ValueExpr(expr));
        //    print("subset size " + subset.Count);
        //    //foreach (CombinedGraph graph in graphManager.Graphs)
        //    //{
        //    Graph graph = graphManager.Graphs[0];
        //    subGraph.minCoordValues = graph.ScaleCoordinates(graph.minCoordValues);
        //    subGraph.maxCoordValues = graph.ScaleCoordinates(graph.maxCoordValues);
        //    Dictionary<string, Graph.GraphPoint> newPoints = new Dictionary<string, Graph.GraphPoint>();
        //    foreach (Cell cell in subset)
        //    {
        //        var point = graph.FindGraphPoint(cell.Label).Position;
        //        AddGraphPoint(cell, point.x, point.y, point.z);
        //        newPoints[cell.Label] = new Graph.GraphPoint(cell.Label, point.x, point.y, point.z, subGraph);
        //    }
        //    print("new points size " + newPoints.Count);
        //    SliceClustering(newPoints);
        //    while (isCreating)
        //    {
        //        yield return null;
        //    }
        //    //}
        //    referenceManager.cellManager.ColorByAttribute(expr, true);
        //}

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
            HashSet<Graph.GraphPoint> notIncluded = new HashSet<Graph.GraphPoint>(newGraph.points.Values);

            LayerMask layerMask = 1 << LayerMask.NameToLayer("GraphPointLayer");

            while (notIncluded.Count > (newGraph.points.Count / 100))
            {
                // get any graphpoint
                Graph.GraphPoint point = notIncluded.First();
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
        /// Helper method to remove graphpoints from a dictionary.
        /// </summary>
        /// <param name="colliders"> An array with colliders attached to graphpoints. </param>
        /// <param name="set"> A hashset containing graphpoints. </param>
        private void RemoveGraphPointsFromSet(Collider[] colliders, ref HashSet<Graph.GraphPoint> set)
        {
            foreach (Collider c in colliders)
            {
                Graph.GraphPoint p = c.gameObject.GetComponent<Graph.GraphPoint>();
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
        private int NumberOfNotIncludedColliders(Collider[] colliders, HashSet<Graph.GraphPoint> points)
        {
            int total = 0;
            foreach (Collider c in colliders)
            {
                Graph.GraphPoint p = c.gameObject.GetComponent<Graph.GraphPoint>();
                if (p != null)
                {
                    total += points.Contains(p) ? 1 : 0;
                }
            }
            return total;
        }
    }
}