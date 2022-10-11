using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{
    /// <summary>
    /// A tool that draws a series of lines along the average velocities of small sections of a <see cref="Graph"/>.
    /// </summary>
    public class VelocityPathTool : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public LineRenderer linePrefab;
        public Material materialPrefab;
        public int maxPathLength = 20;
        public new GameObject collider;

        private Graph touchingGraph;
        private Graph previousTouchingGraph;
        private int nGraphCollidersInside = 0;
        private VelocityPathNode pathRoot;
        private List<LineRenderer> currentPath = new List<LineRenderer>();
        private List<Vector3> currentPathCenters = new List<Vector3>();
        private List<LineRenderer> previousPath = new List<LineRenderer>();
        private int frameCount = 0;
        private bool active = false;
        private List<Material> materials = new List<Material>();
        private Dictionary<Graph.GraphPoint, Vector3> velocities = new Dictionary<Graph.GraphPoint, Vector3>();
        private bool calculatingPath = false;


        private void Awake()
        {
            CellexalEvents.ConfigLoaded.AddListener(OnConfigLoaded);
        }

        private void OnConfigLoaded()
        {
            materials.Clear();
            int halfNbrOfExpressionColors = CellexalConfig.Config.GraphNumberOfExpressionColors / 2;

            Color[] lowMidExpressionColors = Extensions.Extensions.InterpolateColors(CellexalConfig.Config.GraphLowExpressionColor, CellexalConfig.Config.GraphMidExpressionColor, halfNbrOfExpressionColors);
            Color[] midHighExpressionColors = Extensions.Extensions.InterpolateColors(CellexalConfig.Config.GraphMidExpressionColor, CellexalConfig.Config.GraphHighExpressionColor, CellexalConfig.Config.GraphNumberOfExpressionColors - halfNbrOfExpressionColors + 1);
            foreach (Color col in lowMidExpressionColors)
            {
                Material newMat = new Material(materialPrefab);
                newMat.color = col;
                materials.Add(newMat);
            }
            foreach (Color col in midHighExpressionColors)
            {
                Material newMat = new Material(materialPrefab);
                newMat.color = col;
                materials.Add(newMat);
            }
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.F1))
            //{
            //    active = !active;
            //    referenceManager.velocityGenerator.ToggleGraphPoints();
            //referenceManager.selectionToolCollider.transform.parent = null;
            //referenceManager.controllerModelSwitcher.DesiredModel = Interaction.ControllerModelSwitcher.Model.SelectionTool;
            //referenceManager.controllerModelSwitcher.ActivateDesiredTool();
            //referenceManager.selectionToolCollider.transform.position = new Vector3(-0.6989f, 1.1888f, -0.0261f);
            //}

            if (active && touchingGraph && touchingGraph.velocityParticleEmitter)
            {
                if (frameCount >= maxPathLength && !calculatingPath)
                {
                    foreach (var line in previousPath)
                    {
                        Destroy(line.gameObject);
                    }
                    previousPath.Clear();
                    previousPath.AddRange(currentPath);
                    currentPath.Clear();
                    currentPathCenters.Clear();
                    frameCount = 0;
                    Vector3 selectionToolPosition = touchingGraph.transform.InverseTransformPoint(transform.position);
                    StartCoroutine(CalculatePath(touchingGraph.MinkowskiDetection(selectionToolPosition, 0.03f, -1)));
                }
                frameCount++;
            }
            else
            {
                frameCount = maxPathLength;
                foreach (var line in previousPath)
                {
                    Destroy(line.gameObject);
                }
                foreach (var line in currentPath)
                {
                    Destroy(line.gameObject);
                }
                previousPath.Clear();
                currentPath.Clear();
                currentPathCenters.Clear();
                StopAllCoroutines();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Graph"))
            {
                Graph graph = other.GetComponent<Graph>();
                if (graph)
                {
                    nGraphCollidersInside++;
                    if (nGraphCollidersInside == 1)
                    {
                        touchingGraph = graph;
                        if (previousTouchingGraph != touchingGraph)
                        {
                            velocities = touchingGraph.velocityParticleEmitter.Velocities;
                        }
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Graph"))
            {
                Graph graph = other.GetComponent<Graph>();
                if (graph == touchingGraph)
                {
                    nGraphCollidersInside--;
                    if (nGraphCollidersInside == 0)
                    {
                        touchingGraph = null;
                        previousTouchingGraph = touchingGraph;
                    }
                }
            }
        }

        /// <summary>
        /// Toggles the <see cref="VelocityPathTool"/> on or off.
        /// </summary>
        public void ToggleActive()
        {
            active = !active;
            collider.SetActive(active);
        }

        /// <summary>
        /// Helper struct for saving information about the current path of velocities we are tracking. Each <see cref="VelocityPathNode"/> represents a sphere and the <see cref="Graph.GraphPoint"/> inside it.
        /// </summary>
        private struct VelocityPathNode
        {
            /// <summary>
            /// The center of this path node.
            /// </summary>
            public Vector3 center;
            /// <summary>
            /// The radius of this path node.
            /// </summary>
            public float radius;
            /// <summary>
            /// A list of all children nodes that this node connects to, being the nodes that were created from tracking the velocities that this node contains.
            /// </summary>
            public List<VelocityPathNode> nextNodes;
            /// <summary>
            /// The points that are inside this node.
            /// </summary>
            public List<Graph.GraphPoint> points;
            /// <summary>
            /// The <see cref="LineRenderer"/> that draws this node's segment.
            /// </summary>
            public LineRenderer renderer;

            public VelocityPathNode(Vector3 center, float radius)
            {
                this.center = center;
                this.radius = radius;
                this.nextNodes = new List<VelocityPathNode>(8);
                this.points = new List<Graph.GraphPoint>();
                this.renderer = null;

            }
            public void AddNextNode(VelocityPathNode newNode)
            {
                nextNodes.Add(newNode);
            }
        }

        /// <summary>
        /// Helper class for the <see cref="KMeans(Cluster, Vector3, float, int)"/> method. Represents a cluster that points are moved to/from according to K-Means clustering.
        /// </summary>
        private class Cluster
        {
            public Vector3 center;
            public List<Graph.GraphPoint> points;
            public List<Vector3> projectedPositions;

            public Cluster()
            {
                points = new List<Graph.GraphPoint>();
                projectedPositions = new List<Vector3>();
            }

            /// <summary>
            /// Calculates the total variance of a cluster.
            /// </summary>
            /// <param name="points">The points the cluster contains.</param>
            /// <param name="center">The center of the cluster.</param>
            /// <returns>The total variance.</returns>
            public static float ClusterVariance(List<Vector3> points, Vector3 center)
            {
                float variance = 0f;
                foreach (Vector3 point in points)
                {
                    variance += (point - center).sqrMagnitude;
                }
                variance /= points.Count;
                return variance;
            }

            public override string ToString()
            {
                return $"center: {center}, n_points: {points.Count}, n_projected_pos: {projectedPositions.Count}";
            }
        }

        /// <summary>
        /// Approximates a new <see cref="VelocityPathNode"/> from a selection of points.
        /// </summary>
        /// <param name="selection">The selection to approximate the node from.</param>
        /// <returns>A new <see cref="VelocityPathNode"/> with it's center at the mean position of the points.</returns>
        private VelocityPathNode CalculateNodeFromSelection(List<Graph.GraphPoint> selection)
        {
            float minX = 1f, maxX = -1f, minY = 1f, maxY = -1f, minZ = 1f, maxZ = -1f;
            foreach (Graph.GraphPoint gp in selection)
            {
                if (gp.Position.x < minX)
                    minX = gp.Position.x;
                if (gp.Position.x > maxX)
                    maxX = gp.Position.x;
                if (gp.Position.y < minY)
                    minY = gp.Position.y;
                if (gp.Position.y > maxY)
                    maxY = gp.Position.y;
                if (gp.Position.z < minZ)
                    minZ = gp.Position.z;
                if (gp.Position.z > maxZ)
                    maxZ = gp.Position.z;
            }
            Vector3 halfExtents = new Vector3(maxX - minX, maxY - minY, maxZ - minZ) / 2f;
            float sphereRadius = (halfExtents.x + halfExtents.y + halfExtents.z) / 3f;
            Vector3 centerPos = new Vector3(minX + halfExtents.x, minY + halfExtents.y, minZ + halfExtents.z);
            return new VelocityPathNode(centerPos, sphereRadius) { points = selection };
        }

        /// <summary>
        /// Follows the average of some points' velocities and returns a new position to track from next iteration.
        /// </summary>
        /// <param name="candidates">The group of points whose velocities to follow.</param>
        /// <param name="center">The center point of the group.</param>
        /// <param name="sphereRadius">The radius of the group, to be passed to the next group.</param>
        /// <param name="squaredThreshold">A threshold of velocities to ignore, squared.</param>
        /// <returns></returns>
        private VelocityPathNode NextNodeFromAverageVelocities(List<Graph.GraphPoint> candidates, Vector3 center, float sphereRadius, float squaredThreshold, float stepThreshold)
        {
            float averageX = 0f;
            float averageY = 0f;
            float averageZ = 0f;
            int count = 0;

            foreach (Graph.GraphPoint point in candidates)
            {
                Vector3 velocity = velocities[point];
                if (velocity.sqrMagnitude >= squaredThreshold)
                {
                    averageX += velocity.x;
                    averageY += velocity.y;
                    averageZ += velocity.z;
                    count++;
                }
            }

            averageX /= count;
            averageY /= count;
            averageZ /= count;
            Vector3 average = new Vector3(averageX, averageY, averageZ);
            if (average.magnitude < stepThreshold)
            {
                return new VelocityPathNode(center, sphereRadius);
            }
            if (average.magnitude < sphereRadius)
            {
                average *= sphereRadius / average.magnitude;
            }

            return new VelocityPathNode(center + average, sphereRadius);
        }

        /// <summary>
        /// Follows a line and returns the point where it intersects a sphere.
        /// </summary>
        /// <param name="sphereCenter">The center of the sphere.</param>
        /// <param name="sphereRadius">The radius of the sphere.</param>
        /// <param name="lineOrigin">The origin of the line.</param>
        /// <param name="lineDir">The direction of the line.</param>
        /// <returns>A <see cref="Vector3"/> of the point where the line intersects the sphere.</returns>
        private Vector3 FindLineSphereIntersection(Vector3 sphereCenter, float sphereRadius, Vector3 lineOrigin, Vector3 lineDir)
        {
            Vector3 c = sphereCenter;
            float r = sphereRadius;
            Vector3 o = lineOrigin;
            Vector3 u = lineDir;

            // quadratic formula components qfa * d^2 + qfb * d + qfc = 0
            // d being the unknown we are solving the equation for
            float qfa = u.sqrMagnitude;
            float qfb = 2 * Vector3.Dot(u, o - c);
            float qfc = (o - c).sqrMagnitude - r * r;


            float underSqrt = qfb * qfb - 4 * qfa * qfc;
            if (underSqrt > 0)
            {
                float dPlus = (-qfb + Mathf.Sqrt(underSqrt)) / (2 * qfa);
                return lineOrigin + dPlus * lineDir;
            }
            else
            {
                float dMinus = (-qfb - Mathf.Sqrt(-underSqrt)) / (2 * qfa);
                return lineOrigin + dMinus * lineDir;
            }
        }

        /// <summary>
        /// Returns the cost to move a point from a cluster. This is what we try to maximize each iteration of the K-Means algorithm.
        /// </summary>
        /// <param name="pointPosition">The position of the point to evaluate.</param>
        /// <param name="currentCluster">The cluster the point is currently a part of.</param>
        /// <param name="candidateCluster">The cluster the point could be moved to.</param>
        /// <returns>The difference in cost between keeping the point in it's current cluster, or move it to the candidate cluster.</returns>
        private float CostDifference(Vector3 pointPosition, Cluster currentCluster, Cluster candidateCluster)
        {
            Vector3 currentClusterCenter = currentCluster.center;
            Vector3 candidateClusterCenter = candidateCluster.center;

            float currentClusterCardinality = currentCluster.points.Count;
            float candidateClusterCardinality = candidateCluster.points.Count;
            float candidateToPointSqrMagnitude = (candidateClusterCenter.x - pointPosition.x) * (candidateClusterCenter.x - pointPosition.x)
                                                 + (candidateClusterCenter.y - pointPosition.y) * (candidateClusterCenter.y - pointPosition.y)
                                                 + (candidateClusterCenter.z - pointPosition.z) * (candidateClusterCenter.z - pointPosition.z);
            float currentToPointSqrMagnitude = (currentClusterCenter.x - pointPosition.x) * (currentClusterCenter.x - pointPosition.x)
                                               + (currentClusterCenter.y - pointPosition.y) * (currentClusterCenter.y - pointPosition.y)
                                               + (currentClusterCenter.z - pointPosition.z) * (currentClusterCenter.z - pointPosition.z);

            float candidateCost = (candidateClusterCardinality / (candidateClusterCardinality + 1)) * candidateToPointSqrMagnitude;
            float currentCost = (currentClusterCardinality / (currentClusterCardinality - 1)) * currentToPointSqrMagnitude;
            return candidateCost - currentCost;
        }

        /// <summary>
        /// Calculates the center of a cluster's points.
        /// </summary>
        /// <param name="cluster">The cluster evaluate.</param>
        /// <param name="useProjectedPositions">True if this method should use the points projected positions (see <see cref="FindLineSphereIntersection(Vector3, float, Vector3, Vector3)"/>), false if it should use the points actual positions.</param>
        /// <returns>The mean position of the points.</returns>
        private Vector3 CalculateBarycenter(Cluster cluster, bool useProjectedPositions = true)
        {
            float averageX = 0f;
            float averageY = 0f;
            float averageZ = 0f;
            int nPoints = cluster.points.Count;
            if (useProjectedPositions)
            {
                foreach (Vector3 pos in cluster.projectedPositions)
                {
                    averageX += pos.x;
                    averageY += pos.y;
                    averageZ += pos.z;
                }
            }
            else
            {
                foreach (Graph.GraphPoint pos in cluster.points)
                {
                    averageX += pos.Position.x;
                    averageY += pos.Position.y;
                    averageZ += pos.Position.z;
                }
            }
            averageX /= nPoints;
            averageY /= nPoints;
            averageZ /= nPoints;
            return new Vector3(averageX, averageY, averageZ);
        }

        /// <summary>
        /// Finds the cluster that a position is clusest to.
        /// </summary>
        /// <param name="pointPosition">The position of the point.</param>
        /// <param name="clusters">A list of clusters to evaluate.</param>
        /// <returns>An index in the list <paramref name="clusters"/>, which the point is closest to.</returns>
        private int ClosestTo(Vector3 pointPosition, List<Cluster> clusters)
        {
            int minIndex = -1;
            float minDistance = float.MaxValue;
            for (int i = 0; i < clusters.Count; ++i)
            {
                float distance = (pointPosition - clusters[i].center).sqrMagnitude;
                if (distance < minDistance)
                {
                    minIndex = i;
                    minDistance = distance;
                }
            }
            return minIndex;
        }

        /// <summary>
        /// A rather naïve implementation of the K-Means algorithm, with k=2.
        /// </summary>
        /// <param name="candidates">A <see cref="Cluster"/> containing all the <see cref="Graph.GraphPoint"/> that should be clustered.</param>
        /// <param name="maxIterations">The maximum amount of iteration to run.</param>
        /// <returns>A list of <see cref="Cluster"/> containing the resulting clustering.</returns>
        private List<Cluster> KMeans(Cluster candidates, int maxIterations = 1000)
        {
            int k = 2;
            List<Cluster> clusters = new List<Cluster>();

            // choose k random points to serve as centroids for our clusters
            int randomIndex = (int)(UnityEngine.Random.value * (candidates.points.Count - 1) / k);
            for (int i = 0; i < k; ++i)
            {
                clusters.Add(new Cluster()
                {
                    center = candidates.projectedPositions[randomIndex * (i + 1)]
                });
            }

            // populate initial clusters with the points closest to each
            for (int i = 0; i < candidates.points.Count; ++i)
            {
                int minDistanceIndex = ClosestTo(candidates.projectedPositions[i], clusters);
                clusters[minDistanceIndex].points.Add(candidates.points[i]);
                clusters[minDistanceIndex].projectedPositions.Add(candidates.projectedPositions[i]);

            }
            // look for points to improve
            for (int iteration = 0; iteration < maxIterations; ++iteration)
            {

                float maximum_cost = 0;
                int max_current_index = -1;
                int max_candidate_index = -1;
                int max_point_index = -1;
                for (int current = 0; current < k; ++current)
                {
                    // if current == 0 then candidate = 1 else candidate = 0 since k = 2
                    int candidate = (current - 1) * -1;
                    Cluster currentCluster = clusters[current];
                    Cluster candidateCluster = clusters[candidate];

                    float n_points_to_iterate_over_multiplier = 0.25f;
                    int n_points_to_iterate_over = Math.Max((int)(1 / n_points_to_iterate_over_multiplier), (int)(currentCluster.points.Count * n_points_to_iterate_over_multiplier));
                    if (n_points_to_iterate_over > currentCluster.points.Count)
                    {
                        n_points_to_iterate_over = currentCluster.points.Count;
                    }
                    for (int i = 0; i < n_points_to_iterate_over; ++i)
                    {
                        Graph.GraphPoint point = currentCluster.points[i];

                        float cost = CostDifference(point.Position, currentCluster, candidateCluster);
                        if (cost < maximum_cost)
                        {
                            maximum_cost = cost;
                            max_current_index = current;
                            max_candidate_index = candidate;
                            max_point_index = i;
                        }
                    }
                }
                if (maximum_cost < 0)
                {
                    print(maximum_cost);
                    clusters[max_candidate_index].points.Add(clusters[max_current_index].points[max_point_index]);
                    clusters[max_candidate_index].projectedPositions.Add(clusters[max_current_index].projectedPositions[max_point_index]);

                    clusters[max_current_index].points.RemoveAt(max_point_index);
                    clusters[max_current_index].projectedPositions.RemoveAt(max_point_index);

                    clusters[max_candidate_index].center = CalculateBarycenter(clusters[max_candidate_index]);
                    clusters[max_current_index].center = CalculateBarycenter(clusters[max_current_index]);

                    if (clusters[max_current_index].points.Count == 0)
                    {
                        return clusters;
                    }
                }
                else
                {
                    return clusters;
                }
            }
            return clusters;
        }

        /// <summary>
        /// Finds the next node(s) using K-Means clustering to decide wether the candidate points' velocities branch into two directions or not.
        /// </summary>
        /// <param name="candidates">The candidate points to find the next node(s) from.</param>
        /// <param name="sphereRadius">The radius of the sphere that is used to find candidates.</param>
        /// <param name="squaredThreshold">A threshold of velocities to ignore.</param>
        /// <returns>A list of <see cref="VelocityPathNode"/> to evaluate later.</returns>
        private List<VelocityPathNode> NextNodesFromKMeans(List<Graph.GraphPoint> candidates, float sphereRadius, float squaredThreshold, float stepThreshold)
        {
            Cluster startingCluster = new Cluster() { points = candidates };
            Vector3 barycenter = CalculateBarycenter(startingCluster, useProjectedPositions: false);
            List<Vector3> projectedPositions = new List<Vector3>();
            int startingNumberOfPoints = candidates.Count;
            for (int i = 0; i < candidates.Count; ++i)
            {
                var point = candidates[i];
                if (velocities[point].sqrMagnitude > squaredThreshold)
                {
                    projectedPositions.Add(FindLineSphereIntersection(barycenter, sphereRadius, point.Position, velocities[point]));
                }
                else
                {
                    candidates.RemoveAt(i);
                    i--;
                }
            }

            startingCluster.center = barycenter;
            startingCluster.projectedPositions = projectedPositions;

            List<Cluster> clusters = KMeans(startingCluster);
            float twoClusterVariance = 0f;
            if (clusters[0].points.Count == 0 || clusters[1].points.Count == 0)
            {
                // prediction: one cluster
                VelocityPathNode newNode = NextNodeFromAverageVelocities(candidates, barycenter, sphereRadius, squaredThreshold, stepThreshold);
                return new List<VelocityPathNode>() { newNode };
            }

            foreach (Cluster cluster in clusters)
            {
                twoClusterVariance += Cluster.ClusterVariance(cluster.projectedPositions, CalculateBarycenter(cluster));
                // always move the next cluster's center atleast 1 radius away
                Vector3 distance = startingCluster.center - cluster.center;
                if (distance.magnitude < sphereRadius)
                {
                    distance *= sphereRadius / distance.magnitude;
                    cluster.center = startingCluster.center + distance;
                }

            }
            float oneClusterVariance = Cluster.ClusterVariance(projectedPositions, CalculateBarycenter(startingCluster));
            if (oneClusterVariance < twoClusterVariance)
            {
                // prediction: one cluster
                VelocityPathNode newNode = NextNodeFromAverageVelocities(candidates, barycenter, sphereRadius, squaredThreshold, stepThreshold);
                return new List<VelocityPathNode>() { newNode };
            }
            else
            {
                // prediction: two clusters
                VelocityPathNode newNode1 = NextNodeFromAverageVelocities(clusters[0].points, CalculateBarycenter(clusters[0], false), sphereRadius, squaredThreshold, stepThreshold);
                VelocityPathNode newNode2 = NextNodeFromAverageVelocities(clusters[1].points, CalculateBarycenter(clusters[1], false), sphereRadius, squaredThreshold, stepThreshold);
                return new List<VelocityPathNode>() { newNode1, newNode2 };
            }

        }

        /// <summary>
        /// Coroutine that tracks velocities along their average vector in an attempt to create a path along them.
        /// </summary>
        /// <param name="selection">An initial selection of points to start the path at.</param>
        /// <param name="threshold">A threshold of velocites to ignore.</param>
        /// <param name="stepThreshold">A threshold of how far each iteration must at least move the <see cref="VelocityPathNode"/> between iterations.</param>
        public IEnumerator CalculatePath(List<Graph.GraphPoint> selection, float threshold = 0.002f, float stepThreshold = 0.000005f)
        {
            if (selection.Count == 0)
            {
                //throw new ArgumentException("Can not calculate path from empty selection");
                yield break;
            }

            Graph graph = selection[0].parent;
            if (!graph.velocityParticleEmitter)
            {
                // throw new Exception($"Velocity not loaded on graph {graph.GraphName}");
                yield break;
            }
            calculatingPath = true;
            VelocityPathNode currentPathNode = CalculateNodeFromSelection(selection);
            float squaredThreshold = threshold * threshold;
            Queue<VelocityPathNode> candidateQueue = new Queue<VelocityPathNode>();
            candidateQueue.Enqueue(currentPathNode);
            int iteration = 0;
            graph.octreeRoot.Group = -1;
            currentPathCenters.Add(currentPathNode.center);
            foreach (Graph.GraphPoint point in selection)
            {
                point.node.Group = 0;
            }

            pathRoot = currentPathNode;

            while (candidateQueue.Count > 0)
            {
                // choose next node based on average velocity
                currentPathNode = candidateQueue.Dequeue();
                VelocityPathNode nextNode = NextNodeFromAverageVelocities(currentPathNode.points, currentPathNode.center, 0.035f, squaredThreshold, stepThreshold);
                Vector3 average = currentPathNode.center - nextNode.center;
                if (average.magnitude < stepThreshold)
                {
                    calculatingPath = false;
                    yield break;
                }
                currentPathNode.AddNextNode(nextNode);
                yield return null;

                /*
                // select new node(s) based on K-Means clustering using k = 2 vs k = 1
                currentPathNode = candidateQueue.Dequeue();
                List<VelocityPathNode> nextNodes = NextNodesFromKMeans(currentPathNode.points, currentPathNode.radius, squaredThreshold);
                foreach (VelocityPathNode nextNode in nextNodes)
                {
                    var nextCandidates = graph.MinkowskiDetection(nextNode.center, nextNode.radius, -1);
                    nextNode.points.AddRange(nextCandidates);

                    graph.ResetColors();
                    foreach (var gp in nextCandidates)
                    {
                        graph.ColorGraphPointSelectionColor(gp, 1, false);
                    }

                    if (nextCandidates.Count > 1)
                    {
                        candidateQueue.Enqueue(nextNode);
                        currentPathNode.AddNextNode(nextNode);

                        // draw line between current node and the next candidate(s)
                        var newLine = Instantiate(linePrefab, graph.transform);
                        newLine.enabled = true;
                        newLine.SetPositions(new Vector3[] { currentPathNode.center, nextNode.center });
                        newLine.sharedMaterial = materials[Math.Min((int)((currentPathNode.center - nextNode.center).magnitude / 0.002f), materials.Count - 1)];
                        currentPath.Add(newLine);
                    }
                    yield return null;
                }

                 */
                nextNode.points.AddRange(graph.MinkowskiDetection(nextNode.center, nextNode.radius, iteration));
                if (nextNode.points.Count > 0)
                {
                    candidateQueue.Enqueue(nextNode);
                    currentPathCenters.Add(nextNode.center);
                    var newLine = Instantiate(linePrefab, graph.transform);
                    newLine.enabled = true;
                    newLine.SetPositions(new Vector3[] { currentPathCenters[iteration], currentPathCenters[iteration + 1] });
                    newLine.sharedMaterial = materials[Math.Min((int)(average.magnitude / 0.002f), materials.Count - 1)];
                    currentPath.Add(newLine);
                }
                else
                {
                    calculatingPath = false;
                    yield break;
                }
                iteration++;
                if (iteration >= maxPathLength)
                {
                    calculatingPath = false;
                    yield break;
                }
            }
            calculatingPath = false;
        }
    }
}
