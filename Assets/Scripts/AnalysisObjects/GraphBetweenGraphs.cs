using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CellexalVR.General;
using CellexalVR.SceneObjects;
using System.Linq;
using CellexalVR.DesktopUI;
using System;

namespace CellexalVR.AnalysisObjects
{

    public class GraphBetweenGraphs : MonoBehaviour
    {
        public Graph graph1, graph2;
        public ReferenceManager referenceManager;
        public GameObject lineBetweenTwoGraphPointsPrefab;
        public GameObject box;


        private Graph graph;
        private Transform t1, t2;
        private List<LineBetweenTwoPoints> lines = new List<LineBetweenTwoPoints>();
        private LineBetweenTwoPoints firstLine;
        private List<LineBetweenTwoPoints> orderedLines = new List<LineBetweenTwoPoints>();
        // Use this for initialization
        void Start()
        {
            t1 = graph1.transform;
            t2 = graph2.transform;
        }

        // Update is called once per frame
        void Update()
        {
            if (t1.hasChanged || t2.hasChanged)
            {
                transform.position = (t1.position + t2.position) / 2f;
            }
        }

        /// <summary>
        /// Draws lines between graphpoints representing the same cell in to different graphs (i.e share the same label).
        /// </summary>
        /// <param name="points"> The graphpoints to draw the lines from. </param>
        /// /// <param name="newGraph">New graph containing the points between the two graphs. </param>
        /// /// <param name="fromGraph">The graph the lines go from. </param>
        /// /// <param name="toGraph"> The graph to draw lines to. </param>
        public void CreateGraphBetweenGraphs(List<Graph.GraphPoint> points, Graph newGraph, Graph fromGraph, Graph toGraph)
        {
            graph = newGraph;
            newGraph.GraphName = "CTC_" + fromGraph.GraphName + "_" + toGraph.GraphName;
            newGraph.tag = "Untagged";
            newGraph.transform.position = (fromGraph.transform.position + toGraph.transform.position) / 2f;
            newGraph.transform.localScale /= 2;
            newGraph.SetInfoTextVisible(false);
            foreach (Graph.GraphPoint g in points)
            {
                var sourceCell = fromGraph.points[g.Label];
                var targetCell = toGraph.points[g.Label];
                var midPosition = (fromGraph.transform.TransformPoint(sourceCell.Position) + toGraph.transform.TransformPoint(targetCell.Position)) / 2f;
                var gp = referenceManager.graphGenerator.AddGraphPoint(referenceManager.cellManager.GetCell(g.Label), midPosition.x, midPosition.y, midPosition.z);
                //gp.RecolorSelectionColor(g.Group, false);

            }
            referenceManager.graphGenerator.SliceClustering();
            referenceManager.graphManager.Graphs.Add(newGraph);
            fromGraph.CTCGraphs.Add(newGraph.gameObject);
            toGraph.CTCGraphs.Add(newGraph.gameObject);
            if (!(fromGraph.GraphActive && toGraph.GraphActive))
            {
                gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// Main function for clustering the lines. If many lines are present it is convenient to cluster them together for better visibility and less lag.
        /// Start by clustering the two graphs using mean shift clustering. Assign points to the clusters and then check if many points in a cluster in one graph goes to the same cluster in the other.
        /// These are points that should be bundles together. Points that do not cluster in both graphs are not bundled but rendered normally.
        /// </summary>
        /// <param name="points">The selection points.</param>
        /// <param name="fromGraph"> THe graph the points were selected from. The lines goes FROM one graph TO another.</param>
        /// <param name="toGraph">The other graph.</param>
        /// <param name="clusterSize">To be considered a cluster and for the points to be bundled there has to be this many points.</param>
        /// <param name="neighbourDistance">The distance to other points to be considered in the same cluster. </param>
        public void ClusterLines(List<Graph.GraphPoint> points, Graph fromGraph, Graph toGraph, int clusterSize = 20,
                                    float neighbourDistance = 0.05f, float kernelBandwidth = 1.0f)
        {
            List<Graph.GraphPoint> toGraphpoints = new List<Graph.GraphPoint>();
            foreach (Graph.GraphPoint point in points)
            {
                toGraphpoints.Add(toGraph.FindGraphPoint(point.Label));
            }
            var centroids = MeanShiftClustering(points, neighbourDistance: neighbourDistance, kernelBandwidth: kernelBandwidth);
            var toGraphCentroids = MeanShiftClustering(toGraphpoints, neighbourDistance: neighbourDistance, kernelBandwidth: kernelBandwidth);
            List<Tuple<HashSet<Graph.GraphPoint>, Vector3>> clusters = AssignPointsToClusters(centroids, points, neighbourDistance);
            List<Tuple<HashSet<Graph.GraphPoint>, Vector3>> toGraphClusters = AssignPointsToClusters(toGraphCentroids, toGraphpoints, neighbourDistance);
            HashSet<Graph.GraphPoint> prevjoinedclusters = new HashSet<Graph.GraphPoint>();
            for (int i = 0; i < clusters.Count; i++)
            {
                var cluster = clusters[i].Item1;
                for (int j = 0; j < toGraphClusters.Count; j++)
                {
                    var toCluster = toGraphClusters[j].Item1;
                    if (!(cluster.Count > clusterSize && toCluster.Count > clusterSize))
                    {
                        continue;
                    }
                    var joinedCluster = from gpfrom in cluster
                                        join gpto in toCluster on gpfrom.Label equals gpto.Label
                                        select gpfrom;
                    prevjoinedclusters.UnionWith(joinedCluster);
                    if (joinedCluster.ToList().Count > clusterSize)
                    {
                        LineBetweenTwoPoints line = Instantiate(lineBetweenTwoGraphPointsPrefab).GetComponent<LineBetweenTwoPoints>();
                        line.t1 = fromGraph.transform;
                        line.t2 = toGraph.transform;
                        line.t3 = graph.transform;
                        line.centroids = true;
                        line.fromGraphCentroid = clusters[i].Item2;
                        line.toGraphCentroid = toGraphClusters[j].Item2;
                        var midGp = graph.FindGraphPoint(joinedCluster.ToList()[(int)(joinedCluster.ToList().Count / 2)].Label);
                        line.midGraphCentroid = midGp.Position;
                        line.selectionManager = referenceManager.selectionManager;
                        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                        Color color = fromGraph.FindGraphPoint(midGp.Label).GetColor();
                        lineRenderer.startColor = lineRenderer.endColor = new Color(color.r, color.g, color.b, 0.1f);
                        lines.Add(line);
                        line.transform.parent = graph.transform;
                        line.gameObject.SetActive(true);
                    }
                }
            }
            var pointsOutsideClusters = points.Except(prevjoinedclusters);
            foreach (Graph.GraphPoint point in pointsOutsideClusters)
            {
                Color color = point.GetColor();
                var sourceCell = fromGraph.points[point.Label];
                var targetCell = toGraph.points[point.Label];
                LineBetweenTwoPoints line = Instantiate(lineBetweenTwoGraphPointsPrefab).GetComponent<LineBetweenTwoPoints>();
                line.t1 = sourceCell.parent.transform;
                line.t2 = targetCell.parent.transform;
                line.graphPoint1 = sourceCell;
                line.graphPoint2 = targetCell;
                var midPosition = (line.t1.TransformPoint(sourceCell.Position) + line.t2.TransformPoint(targetCell.Position)) / 2f;
                var gp = graph.FindGraphPoint(point.Label);
                line.graphPoint3 = gp;
                line.t3 = gp.parent.transform;
                line.selectionManager = referenceManager.selectionManager;
                LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                lineRenderer.startColor = lineRenderer.endColor = new Color(color.r, color.g, color.b, 0.1f);
                lines.Add(line);
                line.transform.parent = graph.transform;
                line.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Mean shift clustering to find clusters with high density. Read more about it here https://en.wikipedia.org/wiki/Mean_shift.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="iterations"></param>
        /// <param name="neighbourDistance"></param>
        /// <param name="kernelBandwidth"></param>
        /// <returns></returns>
        public List<Vector3> MeanShiftClustering(List<Graph.GraphPoint> points, int iterations = 5, float neighbourDistance = 0.05f, float kernelBandwidth = 2.5f)
        {
            List<Vector3> centroids = new List<Vector3>();
            // Create grid of points that cover the graph area. Graph resides in a cube from -0.5 - 0.5. 
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    for (int k = 0; k < 7; k++)
                    {
                        Vector3 v = new Vector3(-0.5f + (i * 1f / 6), -0.5f + (j * 1f / 6), -0.5f + (k * 1f / 6));
                        centroids.Add(v);
                    }
                }
            }
            List<List<Vector3>> oldPoints = new List<List<Vector3>>();
            Vector3 meanShift;
            for (int n = 0; n < iterations; n++)
            {
                for (int i = 0; i < centroids.Count; i++)
                {
                    Vector3 centroid = centroids[i];
                    // Step 1: Calculate neighbouring points N(x) for each point x
                    var neighbours = GetNeighbours(points, centroid, neighbourDistance);
                    if (neighbours.Count == 0)
                    {
                        centroids.RemoveAt(i);
                        continue;
                    }
                    // Step 2: For each point calculate the mean shift m(x)
                    Vector3 nom = Vector3.zero;
                    float denom = 0.0f;
                    foreach (Graph.GraphPoint neighbour in neighbours)
                    {
                        float distance = Vector3.Distance(neighbour.Position, centroid);
                        float weight = GaussianKernel(distance, kernelBandwidth);
                        nom += weight * neighbour.Position;
                        denom += weight;
                    }
                    meanShift = nom / denom;
                    // Step 3: Update each meanshift for the points x <- m(x)
                    centroids[i] = meanShift;
                }
                oldPoints.Add(centroids);
            }
            return centroids;
        }

        private List<Graph.GraphPoint> GetNeighbours(List<Graph.GraphPoint> points, Vector3 centroid, float distance = 0.15f)
        {
            List<Graph.GraphPoint> neighbours = points.FindAll(x => Vector3.Distance(centroid, x.Position) < distance);
            return neighbours;
        }

        private float GaussianKernel(float distance, float bandwidth)
        {
            return (1 / bandwidth * Mathf.Sqrt(2 * Mathf.PI)) * Mathf.Exp(-0.5f * (Mathf.Pow(distance / bandwidth, 2)));
        }

        private List<Tuple<HashSet<Graph.GraphPoint>, Vector3>> AssignPointsToClusters(List<Vector3> centroids, List<Graph.GraphPoint> points, float distance = 0.10f)
        {
            List<Tuple<HashSet<Graph.GraphPoint>, Vector3>> clusters = new List<Tuple<HashSet<Graph.GraphPoint>, Vector3>>();
            List<Vector3> previousClusters = new List<Vector3>();
            foreach (Vector3 centroid in centroids)
            {
                if (previousClusters.Any(x => Vector3.Distance(centroid, x) < (distance / 4)))
                {
                    continue;
                }
                var neighbours = GetNeighbours(points, centroid, distance);
                if (neighbours.Count == 0)
                {
                    continue;
                }
                HashSet<Graph.GraphPoint> cluster = new HashSet<Graph.GraphPoint>();
                int currentGroup = neighbours[0].Group;
                foreach (Graph.GraphPoint gp in neighbours)
                {
                    if (gp.Group != currentGroup)
                    {
                        break;
                    }
                    cluster.Add(gp);
                }
                clusters.Add(new Tuple<HashSet<Graph.GraphPoint>, Vector3>(cluster, centroid));
                previousClusters.Add(centroid);
                //Draw centroid boxes
                //GameObject obj = Instantiate(box, points[0].parent.transform);
                //obj.transform.localPosition = centroid;

            }
            return clusters;

        }
    }
}
