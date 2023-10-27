﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.AnalysisLogic;
using CellexalVR.DesktopUI;
using CellexalVR.Extensions;
using CellexalVR.Spatial;
using TMPro;
using UnityEngine.Rendering;
using CellexalVR.AnalysisLogic;

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
        public GameObject spatialSlicePrefab;
        public GameObject AxesPrefabColoured;
        public GameObject AxesPrefabUncoloured;
        public Material graphPointMaterialPrefab;
        public GameObject skeletonPrefab;
        public Mesh graphpointStandardQStandardSzMesh;
        public Mesh graphpointStandardQSmallSzMesh;
        public Mesh graphpointStandardQLargeSzMesh;
        public Mesh graphpointLowQStandardSzMesh;
        public Mesh graphpointLowQSmallSzMesh;
        public Mesh graphpointLowQLargeSzMesh;
        public Mesh meshToUse;
        public string DirectoryName { get; set; }
        public bool isCreating;
        public int nrOfLODGroups = 1;

        public enum GraphType
        {
            MDS,
            FACS,
            ATTRIBUTE,
            BETWEEN,
            SPATIAL
        };

        public Color[] geneExpressionColors;
        public Texture2D graphPointColors;
        public int graphCount;
        public Graph newGraph;

        private GraphType graphType;
        private SpatialGraph spatialGraph;
        private GraphManager graphManager;
        private int nbrOfClusters;
        private int nbrOfMaxPointsPerClusters;

        private Vector3[] startPositions =
        {
            new Vector3(-0.2f, 1.1f, -0.95f),
            new Vector3(-0.9f, 1.1f, -0.4f),
            new Vector3(-0.9f, 1.1f, 0.4f),
            new Vector3(-0.2f, 1.1f, 0.95f),
            new Vector3(0.8f, 1.1f, -0.4f),
            new Vector3(0.5f, 1.1f, 0.7f)
        };

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
            CellexalEvents.ConfigLoaded.AddListener(CreateShaderColors);
            //CellexalEvents.ConfigLoaded.AddListener(RescaleGraphpointMeshes);
        }

        /// <summary>
        /// Creates a new graph of the given type.
        /// </summary>
        /// <param name="type">The type of graph to create.</param>
        /// <returns>The new graph.</returns>
        public Graph CreateGraph(GraphType type)
        {
            SetMeshToUse(type);
            if (type == GraphType.SPATIAL)
            {
                // meshToUse = graphpointStandardQLargeSzMesh;
                newGraph = Instantiate(spatialSlicePrefab).GetComponent<Graph>();
            }
            else
            {
                newGraph = Instantiate(graphPrefab).GetComponent<Graph>();
                newGraph.GetComponent<GraphInteract>().referenceManager = referenceManager;
            }

            //graphManager.SetGraphStartPosition();
            newGraph.transform.position = startPositions[graphCount % 6];
            newGraph.referenceManager = referenceManager;
            isCreating = true;
            graphCount++;
            return newGraph;
        }

        /// <summary>
        /// Helper function to set the current mesh used to create the graphs based on the selection in the config.
        /// </summary>
        private void SetMeshToUse(GraphType type)
        {
            graphType = type;
            if (type == GraphType.BETWEEN)
            {
                meshToUse = graphpointLowQLargeSzMesh;
            }
            else
                switch (CellexalConfig.Config.GraphPointQuality)
                {
                    case "Standard" when CellexalConfig.Config.GraphPointSize == "Standard":
                        meshToUse = graphpointStandardQStandardSzMesh;
                        break;
                    case "Low" when CellexalConfig.Config.GraphPointSize == "Standard":
                        meshToUse = graphpointLowQStandardSzMesh;
                        break;
                    case "Standard" when CellexalConfig.Config.GraphPointSize == "Small":
                    case "Low" when CellexalConfig.Config.GraphPointSize == "Small":
                        meshToUse = graphpointLowQSmallSzMesh;
                        break;
                    case "Standard" when CellexalConfig.Config.GraphPointSize == "Large":
                        meshToUse = graphpointStandardQLargeSzMesh;
                        break;
                    default:
                        meshToUse = graphpointLowQLargeSzMesh;
                        break;
                }
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

            if (nbrOfExpressionColors + nbrOfSelectionColors > 254)
            {
                nbrOfSelectionColors = 254 - nbrOfExpressionColors;
                CellexalLog.Log(string.Format("ERROR: Can not have more than 254 total expression and selection colors. Reducing selection colors to {0}. Change NumberOfExpressionColors and SelectionToolColors in the settings menu or config.xml.", nbrOfExpressionColors));
            }
            else if (nbrOfExpressionColors < 3)
            {
                CellexalLog.Log("ERROR: Can not have less than 3 gene expression colors. Increasing to 3. Change NumberOfExpressionColors in the settings menu or config.xml.");
                nbrOfExpressionColors = 3;
            }
            int halfNbrOfExpressionColors = nbrOfExpressionColors / 2;

            Color[] lowMidExpressionColors = Extensions.Extensions.InterpolateColors(lowColor, midColor, halfNbrOfExpressionColors);
            Color[] midHighExpressionColors = Extensions.Extensions.InterpolateColors(midColor, highColor, nbrOfExpressionColors - halfNbrOfExpressionColors + 1);

            geneExpressionColors = new Color[CellexalConfig.Config.GraphNumberOfExpressionColors + 1];
            geneExpressionColors[0] = CellexalConfig.Config.GraphZeroExpressionColor;
            Array.Copy(lowMidExpressionColors, 0, geneExpressionColors, 1, lowMidExpressionColors.Length);
            Array.Copy(midHighExpressionColors, 1, geneExpressionColors, 1 + lowMidExpressionColors.Length, midHighExpressionColors.Length - 1);

            //// reservered colors
            //graphpointColors[255] = Color.white;

            graphPointColors = new Texture2D(256, 1, TextureFormat.RGBA32, false);
            int pixel = 0;
            for (int i = 0; i < halfNbrOfExpressionColors; ++i)
            {
                graphPointColors.SetPixel(pixel, 0, lowMidExpressionColors[i]);
                pixel++;
            }

            for (int i = 1; i < nbrOfExpressionColors - halfNbrOfExpressionColors + 1; ++i)
            {
                graphPointColors.SetPixel(pixel, 0, midHighExpressionColors[i]);
                pixel++;
            }

            for (int i = 0; i < nbrOfSelectionColors; ++i)
            {
                graphPointColors.SetPixel(pixel, 0, CellexalConfig.Config.SelectionToolColors[i]);
                pixel++;
            }

            // Setting a block of colours because when going from linear to gamma space in the shader could cause rounding errors.
            //graphPointColors.SetPixel(255, 0, CellexalConfig.Config.GraphDefaultColor);
            //graphPointColors.SetPixel(254, 0, CellexalConfig.Config.GraphDefaultColor);
            //graphPointColors.SetPixel(253, 0, CellexalConfig.Config.GraphDefaultColor);       
            //graphPointColors.SetPixel(252, 0, CellexalConfig.Config.GraphZeroExpressionColor);
            //graphPointColors.SetPixel(251, 0, CellexalConfig.Config.GraphZeroExpressionColor);
            //graphPointColors.SetPixel(250, 0, CellexalConfig.Config.GraphZeroExpressionColor);

            graphPointColors.SetPixel(255, 0, CellexalConfig.Config.GraphDefaultColor);
            graphPointColors.SetPixel(254, 0, CellexalConfig.Config.GraphZeroExpressionColor);

            graphPointColors.filterMode = FilterMode.Point;
            graphPointColors.Apply();

            graphPointMaterialPrefab.SetTexture("_GraphpointColorTex", graphPointColors);
            foreach (Graph graph in graphManager.Graphs)
            {
                foreach (List<GameObject> lodGroup in graph.lodGroupClusters.Values)
                {
                    if (lodGroup.Count > 0)
                    {
                        lodGroup[0].GetComponent<Renderer>().sharedMaterial
                            .SetTexture("_GraphpointColorTex", graphPointColors);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a graphpoint to this graph. The coordinates should be scaled later with <see cref="ScaleAllCoordinates"/>.
        /// </summary>
        /// <param name="label">The graphpoint's (cell's) name.</param>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="z">The z-coordinate.</param>
        public Graph.GraphPoint AddGraphPoint(Cell cell, float x, float y, float z, Graph graph = null)
        {
            Graph.GraphPoint gp;
            if (graph != null)
            {
                if (graph.points.ContainsKey(cell.Label))
                    return null;

                gp = new Graph.GraphPoint(cell.Label, x, y, z, graph);
                newGraph.points[cell.Label] = gp;
                cell.AddGraphPoint(gp);
            }

            else
            {
                if (newGraph.points.ContainsKey(cell.Label))
                    return null;

                gp = new Graph.GraphPoint(cell.Label, x, y, z, newGraph);
                newGraph.points[cell.Label] = gp;
                cell.AddGraphPoint(gp);
                UpdateMinMaxCoords(x, y, z);
            }

            //print("adding points to - " + newGraph.gameObject.name + " - " + newGraph.points.Count);
            //CellexalLog.Log("Added graphpoint: " + cell.Label + " " + x + " " + y + " " + z);
            return gp;
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
        /// Adds arrows representing the axes of the graph.
        /// </summary>
        /// <param name="graph">The graph to add axes to.</param>
        /// <param name="axisNames">The labels on the axes.</param>
        public void AddAxes(Graph graph, string[] axisNames)
        {
            graph.axisNames = axisNames;
            Vector3 position = graph.ScaleCoordinates(graph.minCoordValues);
            GameObject axes;
            if (graphType == GraphType.FACS)
            {
                axes = Instantiate(AxesPrefabColoured, graph.transform);
                axes.GetComponent<AxesArrow>().SetColors(axisNames, graph.minCoordValues, graph.maxCoordValues);
                axes.SetActive(true);
            }
            else
            {
                axes = Instantiate(AxesPrefabUncoloured, graph.transform);
                axes.SetActive(false);
            }

            axes.transform.localPosition = position - (Vector3.one * 0.01f);
            Vector3 size = graph.ScaleCoordinates(graph.maxCoordValues);
            float longestAx = Mathf.Max(Mathf.Max(size.x, size.y), size.z);
            axes.transform.localScale = Vector3.one * longestAx;
            TextMeshPro[] texts = axes.GetComponentsInChildren<TextMeshPro>();
            graph.axes = axes;
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].text = axisNames[i];
            }
        }

        /// <summary>
        /// Helper function to add level of detail group when building graphs.
        /// </summary>
        public void AddLODGroup(Graph combGraph, int i, GraphSlice slice = null)
        {
            GameObject lodGroup = new GameObject();
            // if (slice != null)
            // {
            //     parent = slice.transform;
            //     slice.LODGroupParents.Add(lodGroup);
            // }

            Transform parent = combGraph.transform;
            combGraph.lodGroupParents.Add(lodGroup);

            lodGroup.transform.parent = parent;
            lodGroup.transform.localPosition = Vector3.zero;
            lodGroup.transform.localScale = Vector3.one;
            lodGroup.gameObject.name = $"LODGroup{i}";
            if (i > 0)
            {
                meshToUse = referenceManager.graphGenerator.graphpointLowQLargeSzMesh;
                // referenceManager.graphGenerator.UpdateCoords();
            }

            else
            {
                SetMeshToUse(graphType);
            }


            lodGroup.transform.localRotation = Quaternion.identity;
        }

        public IEnumerator SliceClusteringLOD(int lodGroups, Dictionary<string, Graph.GraphPoint> points = null,
            GraphSlice slice = null, bool scale = true)
        {
            ScaleAllCoordinates();
            newGraph.lodGroups = lodGroups;
            newGraph.textures = new Texture2D[lodGroups];

            if (slice != null)
            {
                // slice.textures = new Texture2D[lodGroups];
            }

            for (int i = 0; i < lodGroups; i++)
            {
                isCreating = true;

                if (lodGroups > 0)
                {
                    AddLODGroup(newGraph, i, slice);
                }


                SliceClustering(points, lodGroup: i, slice: slice);

                while (isCreating)
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Divides the graph into clusters. The graph starts out as one large cluster and is recursively divided into smaller and smaller clusters until all clusters can be rendered in Unity using a single mesh.
        /// </summary>
        public void SliceClustering(Dictionary<string, Graph.GraphPoint> points = null, int lodGroup = 0,
            GraphSlice slice = null)
        {
            // meshes in unity can have a max of 65535 vertices

            // int maxVerticesPerMesh = 250000;
            int maxVerticesPerMesh = 16000 * meshToUse.vertexCount;
            // int maxVerticesPerMesh = 200000;
            nbrOfMaxPointsPerClusters = maxVerticesPerMesh / meshToUse.vertexCount;
            // nbrOfMaxPointsPerClusters = SystemInfo.maxTextureSize;
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
                foreach (var point in points.Values)
                {
                    firstCluster.Add(point);
                }
            }
            //firstCluster.IntersectWith(newGraph.points.Values);

            List<HashSet<Graph.GraphPoint>> clusters = SplitCluster(firstCluster, slice);

            try
            {
                MakeMeshes(clusters, lodGroup: lodGroup, points: points, slice: slice);
            }
            catch (Exception e)
            {
                GC.Collect();
                UnityEngine.Resources.UnloadUnusedAssets();
            }
        }


        /// <summary>
        /// Helper method for clustering. Splits the first cluster. This will remove duplicate points that end up on the same position.
        /// </summary>
        /// <param name="cluster">A cluster containing all the points in the graph.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="HashSet{T}"/> that each contain one cluster.</returns>
        private List<HashSet<Graph.GraphPoint>> SplitCluster(HashSet<Graph.GraphPoint> cluster, GraphSlice slice = null)
        {
            Graph.OctreeNode on;
            // on = new Graph.OctreeNode();
            // on.pos = new Vector3(-0.5f, -0.5f, -0.5f);
            // on.size = new Vector3(1f, 1f, 1f);
            // if (slice != null)
            // {
            //     slice.octreeRoot = new Graph.OctreeNode();
            //     slice.octreeRoot = new Graph.OctreeNode();
            //     slice.octreeRoot.pos = new Vector3(-0.5f, -0.5f, -0.5f);
            //     slice.octreeRoot.size = new Vector3(1f, 1f, 1f);
            //     on = slice.octreeRoot;
            // }
            // else
            // {
            newGraph.octreeRoot = new Graph.OctreeNode();
            newGraph.octreeRoot = new Graph.OctreeNode();
            newGraph.octreeRoot.pos = new Vector3(-0.5f, -0.5f, -0.5f);
            newGraph.octreeRoot.size = new Vector3(1f, 1f, 1f);
            on = newGraph.octreeRoot;
            // }

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            List<Graph.GraphPoint> removedDuplicates = new List<Graph.GraphPoint>();
            List<HashSet<Graph.GraphPoint>> clusters =
                SplitClusterRecursive(cluster, on, true, ref removedDuplicates, slice);
            // remove the duplicates
            foreach (var c in clusters)
            {
                foreach (var gp in removedDuplicates)
                {
                    c.Remove(gp);
                    referenceManager.cellManager.GetCell(gp.Label).GraphPoints.Remove(gp);
                }
            }

            // add colliders if they are not already added.
            if (newGraph.GetComponent<BoxCollider>() == null)
            {
                foreach (Graph.OctreeNode node in on.children)
                {
                    BoxCollider collider;
                    if (slice != null)
                    {
                        collider = slice.gameObject.AddComponent<BoxCollider>();
                    }
                    else
                    {
                        collider = newGraph.gameObject.AddComponent<BoxCollider>();
                    }

                    collider.center = node.pos + node.size / 2f;
                    collider.size = node.size;
                }
            }
            newGraph.GetComponent<GraphInteract>().RegisterColliders();

            // if (slice?.LODGroupClusters[0]?.Count > 0)
            // {
            //     nbrOfClusters = slice.LODGroupClusters[0].Count;
            //     print(nbrOfClusters);
            // }
            //
            // else
            // {
            // }
            // if (nbrOfClustersInOrg > 0)
            // {
            //     nbrOfClusters = nbrOfClustersInOrg;
            // }
            //
            // else
            // {
            // }
            nbrOfClusters = clusters.Count;
            // print($"cluster: {clusters.Count}, max p : {nbrOfMaxPointsPerClusters}, points: {slice.points.Count}");

            stopwatch.Stop();
            CellexalLog.Log(string.Format("clustered {0} in {1}. nbr of clusters: {2}", newGraph.GraphName,
                stopwatch.Elapsed.ToString(), nbrOfClusters));
            return clusters;
        }


        /// <summary>
        /// Helper method for clustering. Divides one cluster into up to eight smaller clusters if it is too large and returns the non-empty new clusters.
        /// </summary>
        /// <param name="cluster">The cluster to split.</param>
        /// <param name="node">The current Octree node to add points and children to.</param>
        /// <param name="addClusters">True if we are yet to add clusters to return, the result is used for generating meshes.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="HashSet{T}"/> that each contain one cluster.</returns>
        private List<HashSet<Graph.GraphPoint>> SplitClusterRecursive(HashSet<Graph.GraphPoint> cluster,
            Graph.OctreeNode node, bool addClusters, ref List<Graph.GraphPoint> removedDuplicates,
            GraphSlice slice = null)
        {
            //removedDuplicates = new List<Graph.GraphPoint>();
            var result = new List<HashSet<Graph.GraphPoint>>();
            float graphpointMeshSize = meshToUse.bounds.size.x;
            float graphpointMeshExtent = graphpointMeshSize / 2f;
            Graph.GraphPoint firstPoint = cluster.First();
            // if the cluster size is 1, add one last octreenode and return. recursion stops here.
            if (cluster.Count == 1)
            {
                node.children = new Graph.OctreeNode[0];
                node.point = firstPoint;
                firstPoint.node = node;
                node.size = meshToUse.bounds.size;
                node.pos = firstPoint.Position - node.size / 2;
                node.center = firstPoint.Position;
                if (addClusters)
                {
                    result.Add(cluster);
                    return result;
                }
                else
                {
                    return null;
                }
            }

            if (cluster.Count <= nbrOfMaxPointsPerClusters && addClusters)
            {
                result.Add(cluster);
                addClusters = false;
            }

            // cluster is too big, split it
            // calculate center
            Vector3 splitCenter = Vector3.zero;
            Vector3 firstPointPosition = firstPoint.Position;
            float minX = firstPointPosition.x;
            float minY = firstPointPosition.y;
            float minZ = firstPointPosition.z;
            float maxX = firstPointPosition.x;
            float maxY = firstPointPosition.y;
            float maxZ = firstPointPosition.z;
            foreach (var point in cluster)
            {
                Vector3 pos = point.Position;
                splitCenter += pos;
                if (pos.x < minX)
                    minX = pos.x;
                else if (pos.x > maxX)
                    maxX = pos.x;
                if (pos.y < minY)
                    minY = pos.y;
                else if (pos.y > maxY)
                    maxY = pos.y;
                if (pos.z < minZ)
                    minZ = pos.z;
                else if (pos.z > maxZ)
                    maxZ = pos.z;
            }

            splitCenter /= cluster.Count;
            node.center = splitCenter;
            node.pos = new Vector3(minX - graphpointMeshExtent, minY - graphpointMeshExtent,
                minZ - graphpointMeshExtent);
            Vector3 nodePos = node.pos;
            node.size = new Vector3(maxX - minX + graphpointMeshSize, maxY - minY + graphpointMeshSize,
                maxZ - minZ + graphpointMeshSize);
            Vector3 nodeSize = node.size;

            // nodeSize.magnitude < 0.000031622776
            //if (new Vector3(maxX - minX, maxY - minY, maxZ - minZ).sqrMagnitude < 0.0000001f)
            if (minX == maxX && minY == maxY && minZ == maxZ)
            {
                CellexalLog.Log("Removed " + (cluster.Count - 1) + " duplicate point(s) at position " + V2S(node.pos));
                foreach (var gp in cluster)
                {
                    if (gp != firstPoint)
                    {
                        if (slice != null)
                        {
                            // slice.points.Remove(gp.Label);
                        }

                        else
                        {
                            newGraph.points.Remove(gp.Label);
                        }

                        removedDuplicates.Add(gp);
                    }
                }

                cluster.Clear();
                cluster.Add(firstPoint);

                return SplitClusterRecursive(cluster, node, addClusters, ref removedDuplicates, slice);
            }

            int nodeDepth = 0;
            var parentNode = node;
            while (parentNode.parent != null)
            {
                parentNode = parentNode.parent;
                nodeDepth++;
            }

            if (nodeDepth > 25f)
            {
                CellexalLog.Log("Too many iterations reached when clustering, bailing out");
                //CellexalLog.Log("points in cluster: " + cluster.Count + " depth: " + nodeDepth);
                //cluster.All((gp) =>
                //{
                //    CellexalLog.Log(GraphGenerator.V2S(gp.Position) + " " + gp.Label);
                //    return true;
                //});
                return result;
            }

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
                new Vector3(splitCenter.x,  splitCenter.y,  nodePos.z),
                new Vector3(nodePos.x,      nodePos.y ,     splitCenter.z),
                new Vector3(splitCenter.x,  nodePos.y,      splitCenter.z),
                new Vector3(nodePos.x,      splitCenter.y,  splitCenter.z),
                splitCenter
            };

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
            //call recursively for each new cluster
            for (int i = 0; i < newClusters.Count; ++i)
            {
                var returnedClusters = SplitClusterRecursive(newClusters[i], node.children[i], addClusters, ref removedDuplicates);
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
            newGraph.maxCoordValues += meshToUse.bounds.size;
            newGraph.minCoordValues -= meshToUse.bounds.size;
            newGraph.diffCoordValues = newGraph.maxCoordValues - newGraph.minCoordValues;
            switch (graphType)
            {
                case GraphType.MDS:
                case GraphType.BETWEEN:
                case GraphType.SPATIAL:
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
        private void MakeMeshes(List<HashSet<Graph.GraphPoint>> clusters, int lodGroup = 0,
            Dictionary<string, Graph.GraphPoint> points = null, GraphSlice slice = null)
        {
            if (points == null)
            {
                StartCoroutine(MakeMeshesCoroutine(clusters, lodGroup: lodGroup, points: newGraph.points));
            }

            else
            {
                StartCoroutine(MakeMeshesCoroutine(clusters, points: points, lodGroup: lodGroup, slice: slice));
            }
        }



        /// <summary>
        /// Coroutine that makes some meshes every frame. Uses the new job system to do things parallel.
        /// </summary>
        private IEnumerator MakeMeshesCoroutine(List<HashSet<Graph.GraphPoint>> clusters,
            Dictionary<string, Graph.GraphPoint> points,
            BooleanExpression.Expr expr = null, int lodGroup = 0, GraphSlice slice = null)
        {
            newGraph.lodGroupClusters[lodGroup] = new List<GameObject>(nbrOfClusters);
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            int graphPointMeshVertexCount = meshToUse.vertexCount;
            int graphPointMeshTriangleCount = meshToUse.triangles.Length;
            Material graphPointMaterial = Instantiate(graphPointMaterialPrefab);

            // arrays used by the combine meshes job
            NativeArray<Vector3> graphPointMeshVertices =
                new NativeArray<Vector3>(meshToUse.vertices, Allocator.TempJob);
            NativeArray<int> graphPointMeshTriangles = new NativeArray<int>(meshToUse.triangles, Allocator.TempJob);
            NativeArray<int> clusterOffsets = new NativeArray<int>(nbrOfClusters, Allocator.TempJob);
            NativeArray<Vector3> positions = new NativeArray<Vector3>(points.Count, Allocator.TempJob);
            NativeArray<Vector3> resultVertices =
                new NativeArray<Vector3>(points.Count * graphPointMeshVertexCount, Allocator.TempJob);
            NativeArray<int> resultTriangles =
                new NativeArray<int>(points.Count * graphPointMeshTriangleCount, Allocator.TempJob);
            NativeArray<Vector2> resultUVs =
                new NativeArray<Vector2>(points.Count * graphPointMeshVertexCount, Allocator.TempJob);

            // set up cluster offsets
            clusterOffsets[0] = 0;
            // if (nbrOfClusters > clusterOffsets.Length)
            // {
            //     print("mismatch make new");
            //     clusterOffsets = new NativeArray<int>(nbrOfClusters, Allocator.TempJob);
            // }

            for (int i = 0; i < nbrOfClusters; ++i)
            {
                var cluster = clusters[i];
                int clusterOffset = cluster.Count;
                if (i < clusterOffsets.Length - 1)
                {
                    clusterOffsets[i + 1] = clusterOffsets[i] + cluster.Count;
                }

                int j = 0;
                foreach (var point in cluster)
                {
                    positions[clusterOffsets[i] + j] = point.Position;
                    point.SetTextureCoord(new Vector2Int(j, i), lodGroup);

                    //debugPoints.Remove(point);

                    j++;
                }
            }


            // create a job to create and merge the meshes
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

            // instantiate everything
            int itemsThisFrame = 0;
            int maximumItemsPerFrame = CellexalConfig.Config.GraphClustersPerFrameStartCount;
            int maximumItemsPerFrameInc = CellexalConfig.Config.GraphClustersPerFrameIncrement;
            float maximumDeltaTime = 0.05f;

            for (int i = 0; i < nbrOfClusters; ++i)
            {
                GameObject newPart;
                // newPart = Instantiate(graphpointsPrefab, newGraph.transform);
                // print($"group {lodGroups[lodGroup]}");

                newPart = Instantiate(graphpointsPrefab, newGraph.lodGroupParents[lodGroup].transform);
                if (newGraph.GetComponent<GraphSlice>() != null)
                {
                    newPart.SetActive(false);
                }

                var newMesh = new Mesh();
                newMesh.indexFormat = IndexFormat.UInt32;
                int clusterOffset = clusterOffsets[i];
                int clusterSize = 0;
                if (i < clusterOffsets.Length - 1)
                {
                    clusterSize = clusterOffsets[i + 1] - clusterOffset;
                }
                else
                {
                    clusterSize = points.Count - clusterOffset;
                }

                int nbrOfVerticesInCluster = clusterSize * graphPointMeshVertexCount;
                int nbrOfTrianglesInCluster = clusterSize * graphPointMeshTriangleCount;
                int vertexOffset = clusterOffset * graphPointMeshVertexCount;
                int triangleOffset = clusterOffset * graphPointMeshTriangleCount;

                // copy the vertices, uvs and triangles to the new mesh
                newMesh.vertices =
                    new NativeSlice<Vector3>(job.resultVertices, vertexOffset, nbrOfVerticesInCluster)
                        .ToArray();
                newMesh.uv = new NativeSlice<Vector2>(job.resultUVs, vertexOffset, nbrOfVerticesInCluster)
                    .ToArray();
                newMesh.triangles =
                    new NativeSlice<int>(job.resultTriangles, triangleOffset, nbrOfTrianglesInCluster)
                        .ToArray();

                newMesh.RecalculateBounds();
                newMesh.RecalculateNormals();

                newPart.GetComponent<MeshFilter>().mesh = newMesh;

                newGraph.lodGroupClusters[lodGroup].Add(newPart);
                newPart.GetComponent<Renderer>().sharedMaterial = graphPointMaterial;
                //newPart.GetComponent<Renderer>().sharedMaterials = new Material[] { graphPointMaterial, graphPointTransparentMaterial };

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
            job.resultVertices.Dispose();
            job.resultTriangles.Dispose();
            job.resultUVs.Dispose();

            graphPointMeshVertices.Dispose();
            graphPointMeshTriangles.Dispose();

            // graphPointMeshVertices.Dispose();
            // graphPointMeshTriangles.Dispose();
            // clusterOffsets.Dispose();
            // positions.Dispose();
            // resultVertices.Dispose();
            // resultTriangles.Dispose();
            // resultUVs.Dispose();

            // set up the graph's texture
            //print(SystemInfo.maxTextureSize);


            //newGraph.textureHeights[lodGroup] = nbrOfClusters;
            //Texture2D texture = new Texture2D(newGraph.textureWidth, newGraph.textureHeight, TextureFormat.ARGB32, false);
            Texture2D texture = new Texture2D(nbrOfMaxPointsPerClusters, nbrOfClusters,
                TextureFormat.RGBA32,
                false, true);

            texture.filterMode = FilterMode.Point;
            texture.anisoLevel = 0;
            for (int i = 0; i < texture.width; ++i)
            {
                for (int j = 0; j < texture.height; ++j)
                {
                    texture.SetPixel(i, j, Color.red);
                }
            }

            texture.Apply();
            Material sharedMaterial;

            // var sharedMaterial = newGraph.graphPointClusters[0].GetComponent<Renderer>().sharedMaterial;
            newGraph.textureWidths[lodGroup] = nbrOfMaxPointsPerClusters; //16000
            newGraph.textureHeights[lodGroup] = nbrOfClusters; //170000 / 16000;
            newGraph.textures[lodGroup] = texture;
            sharedMaterial = newGraph.lodGroupClusters[lodGroup][0].GetComponent<Renderer>().sharedMaterial;
            sharedMaterial.mainTexture = newGraph.textures[lodGroup];

            Shader graphpointShader = sharedMaterial.shader;
            sharedMaterial.SetTexture("_GraphpointColorTex", graphPointColors);

            isCreating = false;
        }

        public static string V2S(Vector2 v)
        {
            return "(" + v.x + ", " + v.y + ")";
        }

        public static string V2S(Vector3 v)
        {
            return "(" + v.x + ", " + v.y + ", " + v.z + ")";
        }

        public static string V2S(Vector4 v)
        {
            return "(" + v.x + ", " + v.y + ", " + v.z + ", " + v.w + ")";
        }

        /// <summary>
        /// Job that combines the meshes of one cluster.
        /// </summary>
        public struct CombineMeshesJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> positions;
            [ReadOnly] public NativeArray<Vector3> vertices;
            [ReadOnly] public NativeArray<int> triangles;
            [ReadOnly] public NativeArray<int> clusterOffsets;
            [ReadOnly] public int clusterMaxSize;

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
                int clusterOffset = clusterOffsets[index];
                int nbrOfPoints;
                if (index < clusterOffsets.Length - 1)
                {
                    nbrOfPoints = clusterOffsets[index + 1] - clusterOffset;
                }
                else
                {
                    nbrOfPoints = positions.Length - clusterOffset;
                }

                int nbrOfVertices = vertices.Length;
                int nbrOfTriangles = triangles.Length;
                int vertexIndexOffset = clusterOffset * nbrOfVertices;
                int triangleIndexOffset = clusterOffset * nbrOfTriangles;
                float clusterUVY = (index + 0.5f) / clusterOffsets.Length;

                for (int i = 0; i < nbrOfPoints; ++i)
                {
                    Vector3 pointPosition = positions[clusterOffset + i];
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

        public void UpdateCoords()
        {
            newGraph.minCoordValues = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            newGraph.maxCoordValues = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (Graph.GraphPoint point in newGraph.points.Values)
            {
                UpdateMinMaxCoords(point.Position.x, point.Position.y, point.Position.z);
            }
        }

        [ConsoleCommand("graphGenerator", aliases: "csg")]
        public void CreateSubGraphs()
        {
            List<string> attr = new List<string>
            {
                "celltype@Caudal.Mesoderm"
            };
            CreateSubGraphs(attr);
        }


        [ConsoleCommand("graphGenerator", aliases: "cm")]
        public void CreateMeshFromCommandLine()
        {
            //CreateMesh();
        }

        public void CreateMesh()
        {
            MeshGenerator.instance.GenerateMeshes(false);
        }


        /// <summary>
        /// Creates a subgraph based on some attrbiutes.
        /// </summary>
        /// <param name="attributes">The attributes to include in the subgraph.</param>
        public void CreateSubGraphs(List<string> attributes)
        {
            BooleanExpression.Expr expr = new BooleanExpression.AttributeExpr(attributes[0], true);
            string subGraphName = attributes[0];
            // if (subGraphName.Contains('@'))
            // {
            //     subGraphName = subGraphName.Split('@')[1];
            // }

            for (int i = 1; i < attributes.Count; i++)
            {
                // if (attributes[i].Contains('@'))
                // {
                //     subGraphName += " - " + attributes[i].Split('@')[1];
                // }
                // else
                // {
                // }
                subGraphName += " - " + attributes[i];

                BooleanExpression.Expr tempExpr = expr;
                expr = new BooleanExpression.OrExpr(tempExpr, new BooleanExpression.AttributeExpr(attributes[i], true));
            }
            StartCoroutine(CreateSubgraphsCoroutine(expr, attributes, graphManager.originalGraphs, subGraphName));
        }

        private IEnumerator CreateSubgraphsCoroutine(BooleanExpression.Expr expr, List<string> attributes,
            List<Graph> graphs, string subGraphName)
        {
            foreach (Graph g in graphs)
            {
                if (!g.gameObject.activeSelf) continue;
                string fullName = g.name + " - " + subGraphName;
                yield return StartCoroutine(CreateSubGraphsCoroutine(expr, attributes, g, fullName));
            }

            if (!referenceManager.sessionHistoryList.Contains(subGraphName, Definitions.HistoryEvent.ATTRIBUTEGRAPH))
            {
                referenceManager.sessionHistoryList.AddEntry(subGraphName, Definitions.HistoryEvent.ATTRIBUTEGRAPH);
            }

            CellexalEvents.CommandFinished.Invoke(true);
        }


        private IEnumerator CreateSubGraphsCoroutine(BooleanExpression.Expr expr, List<string> attributes, Graph g,
            string subGraphName)
        {
            List<string> attributesToColor = new List<string>(attributes);
            while (isCreating)
            {
                yield return null;
            }

            var subGraph = CreateGraph(GraphType.ATTRIBUTE);
            subGraph.GraphName = subGraphName;
            subGraph.tag = "SubGraph";

            g.CreateGraphSkeleton(true);
            while (MeshGenerator.instance.creatingMesh)
            {
                yield return null;
            }

            GameObject skeleton = g.convexHull;
            skeleton.transform.parent = subGraph.gameObject.transform;
            skeleton.transform.localPosition = Vector3.zero;

            List<Cell> subset = referenceManager.cellManager.SubSet(expr);
            foreach (Cell cell in subset)
            {
                var point = g.FindGraphPoint(cell.Label).Position;
                AddGraphPoint(cell, point.x, point.y, point.z);
            }

            subGraph.maxCoordValues = g.ScaleCoordinates(g.maxCoordValues);
            subGraph.minCoordValues = g.ScaleCoordinates(g.minCoordValues);
            StartCoroutine(SliceClusteringLOD(nrOfLODGroups));
            foreach (BoxCollider col in g.GetComponents<BoxCollider>())
            {
                var newCol = subGraph.gameObject.AddComponent<BoxCollider>();
                newCol.size = col.size;
            }

            while (isCreating)
            {
                yield return null;
            }

            subGraph.CopyAttributeMasks(g);

            foreach (string attribute in attributesToColor)
            {
                subGraph.ColorByAttribute(attribute, true);
            }
            graphManager.Graphs.Add(subGraph);
            graphManager.attributeSubGraphs.Add(subGraph);
            if (g.hasVelocityInfo)
            {
                referenceManager.velocitySubMenu.CreateButton(CellexalUser.DatasetFullPath + @"\" +
                                                              g.GraphName + ".mds", subGraphName);
                subGraph.hasVelocityInfo = true;
            }
        }

        public void UpdateMeshToUse()
        {
            switch (CellexalConfig.Config.GraphPointQuality)
            {
                case "Standard" when CellexalConfig.Config.GraphPointSize == "Standard":
                    meshToUse = graphpointStandardQStandardSzMesh;
                    break;
                case "Low" when CellexalConfig.Config.GraphPointSize == "Standard":
                    meshToUse = graphpointLowQStandardSzMesh;
                    break;
                case "Standard" when CellexalConfig.Config.GraphPointSize == "Small":
                case "Low" when CellexalConfig.Config.GraphPointSize == "Small":
                    meshToUse = graphpointLowQSmallSzMesh;
                    break;
                case "Standard" when CellexalConfig.Config.GraphPointSize == "Large":
                    meshToUse = graphpointStandardQLargeSzMesh;
                    break;
                default:
                    meshToUse = graphpointLowQLargeSzMesh;
                    break;
            }
        }


        /// <summary>
        /// Rebuilding graphs while when mesh has changed such as to another size or quality. The color of the points are kept
        /// </summary>
        /// <returns></returns>
        public IEnumerator RebuildGraphs()
        {
            while (isCreating)
            {
                yield break;
            }

            foreach (Graph graph in graphManager.originalGraphs)
            {
                newGraph = graph;
                Texture2D oldTexture = graph.textures[0];
                graph.minCoordValues = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                graph.maxCoordValues = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                Dictionary<string, Color32> oldTextureColors = new Dictionary<string, Color32>();
                foreach (KeyValuePair<string, Graph.GraphPoint> point in newGraph.points)
                {
                    UpdateMinMaxCoords(point.Value.Position.x, point.Value.Position.y, point.Value.Position.z);
                    oldTextureColors[point.Key] =
                        oldTexture.GetPixel(point.Value.textureCoord[0].x, point.Value.textureCoord[0].y);
                }


                foreach (GameObject obj in newGraph.lodGroupParents)
                {
                    Destroy(obj);
                }

                newGraph.lodGroupParents.Clear();
                newGraph.lodGroupClusters.Clear();

                foreach (BoxCollider bc in graph.GetComponents<BoxCollider>())
                {
                    Destroy(bc);
                }

                //TODO
                //nrOfLODGroups = CellexalConfig.Config.GraphPointQuality == "Standard" ? 2 : 1;
                StartCoroutine(SliceClusteringLOD(nrOfLODGroups));

                while (isCreating)
                {
                    yield return null;
                }

                foreach (KeyValuePair<string, Graph.GraphPoint> point in newGraph.points)
                {
                    Vector2Int pos = point.Value.textureCoord[0];
                    Color32 oldColor = oldTextureColors[point.Key];
                    graph.textures[0].SetPixels32(pos.x, pos.y, 1, 1, new Color32[] { oldColor });
                }

                graph.textures[0].Apply();
                //UpdateLODGroups(graph, nrOfLODGroups);
                //for (int i = 0; i < graph.lodGroupParents.Count; i++)
                //{
                //    graph.lodGroupClusters[i][0].GetComponent<Renderer>().sharedMaterial.mainTexture =
                //        graph.textures[0];
                //}
            }
        }


        public void UpdateLODGroups(Graph graph = null, int nrOfLODGroups = 1, GraphSlice slice = null)
        {
            LODGroup lodGroup;
            if (graph != null)
            {
                lodGroup = graph.GetComponent<LODGroup>();
                if (lodGroup == null)
                {
                    lodGroup = graph.gameObject.AddComponent<LODGroup>();
                }
            }

            else
            {
                lodGroup = slice.GetComponent<LODGroup>();
                if (lodGroup == null)
                {
                    lodGroup = slice.gameObject.AddComponent<LODGroup>();
                }
            }

            LOD[] lods = new LOD[2];
            for (int i = 0; i < nrOfLODGroups; i++)
            {
                Renderer[] renderers;
                renderers = new Renderer[graph.lodGroupClusters[i].Count];
                if (graph != null)
                {
                    for (int j = 0; j < graph.lodGroupClusters[i].Count; j++)
                    {
                        renderers[j] = graph.lodGroupClusters[i][j].GetComponent<Renderer>();
                    }
                }

                else
                {
                    //renderers = new Renderer[slice.lodGroupClusters[i].Count];
                    // for (int j = 0; j < slice.lodGroupClusters[i].Count; j++)
                    // {
                    //     renderers[j] = slice.lodGroupClusters[i][j].GetComponent<Renderer>();
                    // }
                }

                if (i == nrOfLODGroups - 1)
                {
                    lods[i] = new LOD(0f, renderers);
                }

                else
                {
                    lods[i] = new LOD(1f / 1.3f, renderers);
                }
            }

            // GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // g.transform.parent = newGraph.transform;
            // g.transform.localPosition = Vector3.zero;
            // g.layer = LayerMask.NameToLayer("GraphLayer");
            // Renderer[] lastRenderers = new Renderer[1];
            // lastRenderers[0] = g.GetComponent<Renderer>();
            // lastRenderers[0].material = graphPointMaterialPrefab;
            // lods[2] = new LOD(1.0f / 7, lastRenderers);
            lodGroup.fadeMode = LODFadeMode.CrossFade;
            lodGroup.animateCrossFading = true;
            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();
        }
    }
}