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
    /// <summary>
    /// Represents a graph that is created between to graphs when using cell to cell tracking.
    /// A selection is made in one graph and the respective points are found in another.
    /// This graph is created where each points position is in the middle of the two corresponding points.
    /// Lines goes from points in one graph through this graph and finally to the other graph.
    /// Graphpoints in this graph behave just as in normal graphs and can be recoloured in every way.
    /// </summary>
    public class GraphBetweenGraphs : MonoBehaviour
    {
        public Graph graph1, graph2;
        public ReferenceManager referenceManager;
        public GameObject lineBetweenTwoGraphPointsPrefab;
        public GameObject clusterDebugBox;


        private GameObject velocityParticleSystemPrefab;
        private Graph graph;
        private Transform t1, t2;
        private List<LineBetweenTwoPoints> lines = new List<LineBetweenTwoPoints>();
        private LineBetweenTwoPoints firstLine;
        private List<LineBetweenTwoPoints> orderedLines = new List<LineBetweenTwoPoints>();
        private Dictionary<Graph.GraphPoint, Vector3> velocitiesFromGraph = new Dictionary<Graph.GraphPoint, Vector3>();
        private Dictionary<Graph.GraphPoint, Vector3> velocitiesMidGraph = new Dictionary<Graph.GraphPoint, Vector3>();
        private Dictionary<Graph.GraphPoint, Vector3> velocitiesToGraph = new Dictionary<Graph.GraphPoint, Vector3>();
        private GameObject velocityParticleSystemFromGraph;
        private GameObject velocityParticleSystemMidGraph;
        private GameObject velocityParticleSystemToGraph;
        // Use this for initialization
        void Start()
        {
            t1 = graph1.transform;
            t2 = graph2.transform;
            velocityParticleSystemPrefab = referenceManager.velocityGenerator.particleSystemPrefab;
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
        public IEnumerator ClusterLines(List<Graph.GraphPoint> points, Graph fromGraph, Graph toGraph, int clusterSize = 50,
                                    float neighbourDistance = 0.05f, float kernelBandwidth = 1.0f)
        {
            List<Graph.GraphPoint> toGraphpoints = new List<Graph.GraphPoint>();
            foreach (Graph.GraphPoint point in points)
            {
                toGraphpoints.Add(toGraph.FindGraphPoint(point.Label));
                graph.FindGraphPoint(point.Label).RecolorSelectionColor(point.Group, false);
            }
            HashSet<Graph.GraphPoint> prevjoinedclusters = new HashSet<Graph.GraphPoint>();
            var centroids = MeanShiftClustering(points, neighbourDistance: neighbourDistance, kernelBandwidth: kernelBandwidth);
            yield return null;
            var toGraphCentroids = MeanShiftClustering(toGraphpoints, neighbourDistance: neighbourDistance, kernelBandwidth: kernelBandwidth);
            yield return null;
            List<Tuple<HashSet<Graph.GraphPoint>, Vector3>> clusters = AssignPointsToClusters(centroids, points, neighbourDistance);
            yield return null;
            List<Tuple<HashSet<Graph.GraphPoint>, Vector3>> toGraphClusters = AssignPointsToClusters(toGraphCentroids, toGraphpoints, neighbourDistance);
            for (int i = 0; i < clusters.Count; i++)
            {
                var fromCluster = clusters[i];
                for (int j = 0; j < toGraphClusters.Count; j++)
                {
                    var toCluster = toGraphClusters[j];
                    if (!(fromCluster.Item1.Count > clusterSize && toCluster.Item1.Count > clusterSize))
                    {
                        continue;
                    }
                    var joinedCluster = from gpfrom in fromCluster.Item1
                                        join gpto in toCluster.Item1 on gpfrom.Label equals gpto.Label
                                        select gpfrom;
                    if (joinedCluster.ToList().Count > clusterSize)
                    {
                        prevjoinedclusters.UnionWith(joinedCluster);
                        AddCentroidLine(fromGraph, toGraph, joinedCluster);
                    }
                }
                yield return null;
            }

            yield return null;
            var pointsOutsideClusters = points.Except(prevjoinedclusters);
            foreach (Graph.GraphPoint point in pointsOutsideClusters)
            {
                AddLine(fromGraph, toGraph, point);
            }
            AddParticles(fromGraph, toGraph);

        }

        /// <summary>
        /// Mean shift clustering to find clusters with high density. Read more about it here https://en.wikipedia.org/wiki/Mean_shift.
        /// With some modifications to suit the needs of this application.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="iterations"></param>
        /// <param name="neighbourDistance"></param>
        /// <param name="kernelBandwidth"></param>
        /// <returns></returns>
        public List<Vector3> MeanShiftClustering(List<Graph.GraphPoint> points, int iterations = 10, float neighbourDistance = 0.05f, float kernelBandwidth = 2.5f)
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
                    // We dont want all the centroids to converge.
                    // It is better to keep more clusters to be able to bundle more lines together.
                    if (neighbours.Count > 50)
                    {
                        continue;
                    }
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
            List<Graph.GraphPoint> gps = new List<Graph.GraphPoint>(points);
            List<Tuple<HashSet<Graph.GraphPoint>, Vector3>> clusters = new List<Tuple<HashSet<Graph.GraphPoint>, Vector3>>();
            List<Vector3> previousClusters = new List<Vector3>();
            foreach (Vector3 centroid in centroids)
            {
                if (previousClusters.Any(x => Vector3.Distance(centroid, x) < (distance / 4)))
                {
                    continue;
                }
                var neighbours = GetNeighbours(gps, centroid, distance);
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
                    gps.Remove(gp);
                }
                clusters.Add(new Tuple<HashSet<Graph.GraphPoint>, Vector3>(cluster, centroid));
                previousClusters.Add(centroid);
                //Draw centroid boxes
                //GameObject obj = Instantiate(clusterDebugBox, gps[0].parent.transform);
                //obj.transform.localPosition = centroid;

            }
            return clusters;

        }

        /// <summary>
        /// Adds a line that goes from a centroid in one graph to centroid in another.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="fromCluster"></param>
        /// <param name="toCluster"></param>
        /// <param name="joinedCluster"></param>
        private void AddCentroidLine(Graph from, Graph to, IEnumerable<Graph.GraphPoint> joinedCluster)
        {
            HashSet<Graph.GraphPoint> fromCluster = new HashSet<Graph.GraphPoint>();
            HashSet<Graph.GraphPoint> midCluster = new HashSet<Graph.GraphPoint>();
            HashSet<Graph.GraphPoint> toCluster = new HashSet<Graph.GraphPoint>();
            HashSet<Vector3> prevAddedHulls = new HashSet<Vector3>();
            foreach (Graph.GraphPoint gp in joinedCluster)
            {
                fromCluster.Add(from.FindGraphPoint(gp.Label));
                midCluster.Add(graph.FindGraphPoint(gp.Label));
                toCluster.Add(to.FindGraphPoint(gp.Label));
            }
            LineBetweenTwoPoints line = Instantiate(lineBetweenTwoGraphPointsPrefab).GetComponent<LineBetweenTwoPoints>();
            Vector3 fromCentroid = CalculateCentroid(fromCluster);
            Vector3 midCentroid = CalculateCentroid(midCluster);
            Vector3 toCentroid = CalculateCentroid(toCluster);
            Vector3 fromClusterHull = CalculateClusterHull(fromCluster, fromCentroid);
            Vector3 midClusterHull = CalculateClusterHull(midCluster, midCentroid);
            Vector3 toClusterHull = CalculateClusterHull(toCluster, toCentroid);
            if (!prevAddedHulls.Contains(fromClusterHull))
            {
                line.fromClusterHull = fromClusterHull;
                prevAddedHulls.Add(fromClusterHull);
            }
            if (!prevAddedHulls.Contains(midClusterHull))
            {
                line.midClusterHull = midClusterHull;
                prevAddedHulls.Add(midClusterHull);
            }
            if (!prevAddedHulls.Contains(toClusterHull))
            {
                line.toClusterHull = toClusterHull;
                prevAddedHulls.Add(toClusterHull);
            }
            line.t1 = from.transform;
            line.t2 = to.transform;
            line.t3 = graph.transform;
            line.centroids = true;
            line.fromGraphCentroid = fromCentroid;
            line.midGraphCentroid = midCentroid;
            line.toGraphCentroid = toCentroid;
            line.fromPointCluster = fromCluster;
            line.midPointCluster = midCluster;
            line.toPointCluster = toCluster;

            line.selectionManager = referenceManager.selectionManager;
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            Color color = from.FindGraphPoint(joinedCluster.ToList()[0].Label).GetColor();
            line.LineColor = new Color(color.r, color.g, color.b, 0.2f);
            lineRenderer.startColor = lineRenderer.endColor = line.LineColor;
            lines.Add(line);
            //line.transform.parent = graph.lineParent.transform;
            line.gameObject.SetActive(true);


            Vector3 dir;
            foreach (Graph.GraphPoint gp in fromCluster)
            {
                dir = fromCentroid - gp.Position;
                velocitiesFromGraph[gp] = dir / 5f;
            }

            foreach (Graph.GraphPoint gp in toCluster)
            {
                dir = toCentroid - gp.Position;
                velocitiesToGraph[gp] = dir / 5f;
            }
            foreach (Graph.GraphPoint gp in midCluster)
            {
                dir = midCentroid - gp.Position;
                velocitiesMidGraph[gp] = dir / 5f;
            }

        }

        private void AddParticles(Graph from, Graph to)
        {
            velocityParticleSystemFromGraph = Instantiate(velocityParticleSystemPrefab, from.transform);
            VelocityParticleEmitter emitter = velocityParticleSystemFromGraph.GetComponent<VelocityParticleEmitter>();
            emitter.referenceManager = referenceManager;
            emitter.graph = from;
            emitter.Velocities = velocitiesFromGraph;
            emitter.particleMaterial = referenceManager.velocityGenerator.standardMaterial;
            emitter.UseGraphPointColors = true;
            emitter.ChangeFrequency(4.0f);
            ParticleSystem.TrailModule trailModule = velocityParticleSystemFromGraph.GetComponent<ParticleSystem>().trails;
            trailModule.enabled = true;
            trailModule.lifetime = 2.0f;
            trailModule.widthOverTrail = 0.010f;

            velocityParticleSystemMidGraph = Instantiate(velocityParticleSystemPrefab, graph.transform);
            velocityParticleSystemMidGraph.transform.localScale /= 2;
            VelocityParticleEmitter emitterMidGraph = velocityParticleSystemMidGraph.GetComponent<VelocityParticleEmitter>();
            emitterMidGraph.referenceManager = referenceManager;
            emitterMidGraph.graph = graph;
            emitterMidGraph.Velocities = velocitiesMidGraph;
            emitterMidGraph.particleMaterial = referenceManager.velocityGenerator.standardMaterial;
            emitterMidGraph.UseGraphPointColors = true;
            emitterMidGraph.ChangeFrequency(4.0f);
            ParticleSystem.TrailModule trailModuleMidGraph = velocityParticleSystemMidGraph.GetComponent<ParticleSystem>().trails;
            trailModuleMidGraph.enabled = true;
            trailModuleMidGraph.lifetime = 2.0f;
            trailModuleMidGraph.widthOverTrail = 0.010f;

            velocityParticleSystemToGraph = Instantiate(velocityParticleSystemPrefab, to.transform);
            VelocityParticleEmitter emitterToGraph = velocityParticleSystemToGraph.GetComponent<VelocityParticleEmitter>();
            emitterToGraph.referenceManager = referenceManager;
            emitterToGraph.graph = to;
            emitterToGraph.Velocities = velocitiesToGraph;
            emitterToGraph.particleMaterial = referenceManager.velocityGenerator.standardMaterial;
            emitterToGraph.UseGraphPointColors = true;
            emitterToGraph.ChangeFrequency(4.0f);
            ParticleSystem.TrailModule trailModuleToGraph = velocityParticleSystemToGraph.GetComponent<ParticleSystem>().trails;
            trailModuleToGraph.enabled = true;
            trailModuleToGraph.lifetime = 2.0f;
            trailModuleToGraph.widthOverTrail = 0.010f;
            //trailModuleToGraph.dieWithParticles = false;

            //velocityParticleSystemFromGraph.transform.parent = graph.transform;
            //velocityParticleSystemToGraph.transform.parent = graph.transform;

        }

        private Vector3 CalculateCentroid(HashSet<Graph.GraphPoint> cluster)
        {
            Vector3 centroid = new Vector3();
            Transform parent;
            foreach (Graph.GraphPoint gp in cluster)
            {
                centroid += gp.Position;
                parent = gp.parent.transform;
            }
            centroid /= cluster.Count;
            //GameObject obj = Instantiate(clusterDebugBox, parent);
            //obj.transform.localPosition = centroid;
            return centroid;
        }

        public Vector3 CalculateClusterHull(HashSet<Graph.GraphPoint> cluster, Vector3 centroid)
        {
            float distance;
            float maxDistance = 0f;
            foreach (Graph.GraphPoint gp in cluster)
            {
                distance = Vector3.Distance(centroid, gp.Position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }
            return new Vector3(maxDistance, maxDistance, maxDistance);
        }
        /// <summary>
        /// Adds line that goes from a point in one graph to respective point in another.
        /// </summary>
        /// <param name="from">Graph that lines goes from.</param>
        /// <param name="to">Graph that line goes to.</param>
        /// <param name="point">The graphpoint that line goes between.</param>
        private void AddLine(Graph from, Graph to, Graph.GraphPoint point)
        {
            Color color = point.GetColor();
            var sourceCell = from.points[point.Label];
            var targetCell = to.points[point.Label];
            LineBetweenTwoPoints line = Instantiate(lineBetweenTwoGraphPointsPrefab, graph.lineParent.transform).GetComponent<LineBetweenTwoPoints>();
            line.t1 = sourceCell.parent.transform;
            line.t2 = targetCell.parent.transform;
            line.graphPoint1 = sourceCell;
            line.graphPoint2 = targetCell;
            //var midPosition = (line.t1.TransformPoint(sourceCell.Position) + line.t2.TransformPoint(targetCell.Position)) / 2f;
            var gp = graph.FindGraphPoint(point.Label);
            line.graphPoint3 = gp;
            line.t3 = gp.parent.transform;
            line.selectionManager = referenceManager.selectionManager;
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            lineRenderer.startColor = lineRenderer.endColor = new Color(color.r, color.g, color.b, 0.1f);
            lines.Add(line);
            //line.transform.parent = graph.lineParent.transform;
            line.gameObject.SetActive(true);

        }

        public void RemoveGraph()
        {
            Destroy(velocityParticleSystemFromGraph.gameObject);
            Destroy(velocityParticleSystemMidGraph.gameObject);
            Destroy(velocityParticleSystemToGraph.gameObject);
            Destroy(this.gameObject);
        }
    }
}
