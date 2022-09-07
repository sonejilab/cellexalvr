using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{
    public class VelocityPathTool : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public LineRenderer linePrefab;
        public Material materialPrefab;

        private VelocityPathNode pathRoot;
        private List<LineRenderer> currentPath = new List<LineRenderer>();
        private List<Vector3> currentPathCenters = new List<Vector3>();
        private List<LineRenderer> previousPath = new List<LineRenderer>();
        private int frameCount = 0;
        private int maxPathLength = 25;
        private bool active = false;
        private List<Material> materials = new List<Material>();
        private Graph currentGraph;
        private Dictionary<Graph.GraphPoint, Vector3> velocities = new Dictionary<Graph.GraphPoint, Vector3>();


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
            if (Input.GetKeyDown(KeyCode.F1))
            {
                active = !active;
                referenceManager.selectionToolCollider.transform.parent = null;
                referenceManager.controllerModelSwitcher.DesiredModel = Interaction.ControllerModelSwitcher.Model.SelectionTool;
                referenceManager.controllerModelSwitcher.ActivateDesiredTool();
                referenceManager.selectionToolCollider.transform.position = new Vector3(-0.7442f, 1.0642f, 0.0797f);
                referenceManager.velocityGenerator.ToggleGraphPoints();
            }

            if (active)
            {
                if (frameCount >= maxPathLength)
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
                    //print(referenceManager.selectionToolCollider.touchingGraphs.Count);
                    if (referenceManager.selectionToolCollider.touchingGraphs.Count > 0)
                    {
                        Graph touchingGraph = referenceManager.selectionToolCollider.touchingGraphs[0];
                        if (touchingGraph != currentGraph)
                        {
                            velocities = touchingGraph.velocityParticleEmitter.Velocities;
                        }
                        //print(touchingGraph);
                        Vector3 selectionToolPosition = touchingGraph.transform.InverseTransformPoint(referenceManager.selectionToolCollider.transform.position);
                        StartCoroutine(CalculatePath(touchingGraph.MinkowskiDetection(selectionToolPosition, 0.05f, -1)));
                    }
                }
                frameCount++;
            }
        }

        private struct VelocityPathNode
        {
            public Vector3 center;
            public float radius;
            public List<VelocityPathNode> nextNodes;
            public List<Graph.GraphPoint> points;
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

            public float ClusterVariance()
            {
                return ClusterVariance(points, center);
            }

            public static float ClusterVariance(List<Graph.GraphPoint> points, Vector3 center)
            {
                float variance = 0f;
                foreach (Graph.GraphPoint point in points)
                {
                    variance += (point.Position - center).sqrMagnitude;
                }
                variance /= points.Count;
                return variance;
            }

            public override string ToString()
            {
                return $"center: {center}, n_points: {points.Count}, n_projected_pos: {projectedPositions.Count}";
            }
        }

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
            //print($"sphere stats: x: {minX}, {maxX}, y: {minY}, {maxY}, z: {minZ}, {maxZ}, halfextents: {halfExtents}, radius: {sphereRadius}, center: {centerPos}");
            return new VelocityPathNode(centerPos, sphereRadius) { points = selection };
        }

        private VelocityPathNode NextNodeFromAverageVelocities(List<Graph.GraphPoint> candidates, float squaredThreshold)
        {
            VelocityPathNode pathNode = CalculateNodeFromSelection(candidates);
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
            if (average.magnitude < pathNode.radius)
            {
                average *= pathNode.radius / average.magnitude;
            }
            pathNode.center += average;
            return pathNode;
        }

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

        private float CostDifference(Vector3 pointPosition, Cluster currentCluster, Vector3 currentClusterCenter, Cluster candidateCluster, Vector3 candidateClusterCenter)
        {
            float currentClusterCardinality = currentCluster.points.Count;
            float candidateClusterCardinality = candidateCluster.points.Count;
            float candidateCost = (candidateClusterCardinality / (candidateClusterCardinality + 1)) * (candidateClusterCenter - pointPosition).sqrMagnitude;
            float currentCost = (currentClusterCardinality / (currentClusterCardinality - 1)) * (currentClusterCenter - pointPosition).sqrMagnitude;
            return candidateCost - currentCost;
        }

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



        private List<Cluster> KMeans(Cluster candidates, Vector3 sphereCenter, float sphereRadius, int maxIterations = 500)
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
                Graph.GraphPoint point = candidates.points[i];
                int minDistanceIndex = ClosestTo(point.Position, clusters);
                clusters[minDistanceIndex].points.Add(point);
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
                    //float n_points_to_iterate_over_multiplier = 1f;
                    //int n_points_to_iterate_over = Math.Max((int)(1 / n_points_to_iterate_over_multiplier), (int)(clusters[current].points.Count * n_points_to_iterate_over_multiplier));

                    for (int i = 0; i < clusters[current].points.Count; ++i)
                    {
                        var point = clusters[current].points[i];

                        for (int candidate = 0; candidate < k; ++candidate)
                        {
                            if (candidate == current)
                            {
                                continue;
                            }
                            float cost = CostDifference(point.Position, clusters[current], clusters[current].center, clusters[candidate], clusters[candidate].center);
                            if (cost < maximum_cost)
                            {
                                maximum_cost = cost;
                                max_current_index = current;
                                max_candidate_index = candidate;
                                max_point_index = i;
                            }
                        }
                    }
                }
                if (maximum_cost < 0)
                {
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

        private List<VelocityPathNode> NextNodesFromKMeans(List<Graph.GraphPoint> candidates)
        {
            Cluster startingCluster = new Cluster() { points = candidates };
            Vector3 barycenter = CalculateBarycenter(startingCluster, useProjectedPositions: false);
            float sphereRadius = 0.005f;
            List<Vector3> projectedPositions = new List<Vector3>();
            for (int i = 0; i < candidates.Count; ++i)
            {
                projectedPositions.Add(FindLineSphereIntersection(barycenter, sphereRadius, candidates[i].Position, velocities[candidates[i]]));
            }
            startingCluster.center = barycenter;
            startingCluster.projectedPositions = projectedPositions;

            List<Cluster> clusters = KMeans(startingCluster, barycenter, sphereRadius);
            float twoClusterVariance = 0f;
            foreach (Cluster cluster in clusters)
            {
                twoClusterVariance += cluster.ClusterVariance();
            }
            float oneClusterVariance = Cluster.ClusterVariance(candidates, barycenter);
            if (twoClusterVariance < oneClusterVariance)
            {
                // prediction: one cluster
                VelocityPathNode newNode = new VelocityPathNode(barycenter, sphereRadius);
                return new List<VelocityPathNode>() { newNode };
            }
            else
            {
                // prediction: two clusters
                VelocityPathNode newNode1 = new VelocityPathNode(clusters[0].center, sphereRadius);
                VelocityPathNode newNode2 = new VelocityPathNode(clusters[1].center, sphereRadius);
                return new List<VelocityPathNode>() { newNode1, newNode2 };
            }

        }

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

            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();
            VelocityPathNode currentPathNode = CalculateNodeFromSelection(selection);
            //print($"original selection contains: {selection.Count} cells");
            //print($"center: {sphere.center}, {sphere.radius}");
            float squaredThreshold = threshold * threshold;
            Queue<VelocityPathNode> candidateQueue = new Queue<VelocityPathNode>();
            //List<Graph.GraphPoint> candidates = new List<Graph.GraphPoint>();
            //candidates.AddRange(selection);
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
#if UNITY_EDITOR
                //gizmoSphereCenters.Add(graph.transform.TransformPoint(pathNode.center));
                //gizmoSphereRadiuses.Add(pathNode.radius);
                //gizmoSphereColours.Add(CellexalConfig.Config.SelectionToolColors[iteration % CellexalConfig.Config.SelectionToolColors.Length]);
#endif

                /*
                // choose next node based on average velocity
                VelocityPathNode nextNode = NextNodeFromAverageVelocities(candidates, squaredThreshold);
                Vector3 average = currentPathNode.center - nextNode.center;
                if (average.magnitude < stepThreshold)
                {
                    //print($"average: {average.magnitude}. average below step threshold, breaking out of loop");
                    yield break;
                }
                currentPathNode.AddNextNode(nextNode);
                currentPathNode = nextNode;
                */

                // select new node(s) based on K-Means clustering using k = 2 vs k = 1
                currentPathNode = candidateQueue.Dequeue();
                List<VelocityPathNode> nextNodes = NextNodesFromKMeans(currentPathNode.points);
                foreach (VelocityPathNode nextNode in nextNodes)
                {
                    var nextCandidates = graph.MinkowskiDetection(nextNode.center, nextNode.radius, -1);
                    nextNode.points.AddRange(nextCandidates);
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
                }
                //candidates.Clear();
                // grow sphere slightly to make up for CalculateSphereFromSelection making it slightly smaller
                //candidates.AddRange(graph.MinkowskiDetection(currentPathNode.center, currentPathNode.radius * 1.1f, iteration));
                //foreach (Graph.GraphPoint point in candidates)
                //{
                //    referenceManager.selectionManager.AddGraphpointToSelection(point, iteration, false);
                //}
                //if (candidates.Count > 0)
                //{
                //    currentPathNode = CalculateNodeFromSelection(candidates);
                //    currentPathCenters.Add(currentPathNode.center);
                //    var newLine = Instantiate(linePrefab, graph.transform);
                //    newLine.enabled = true;
                //    newLine.SetPositions(new Vector3[] { currentPathCenters[iteration], currentPathCenters[iteration + 1] });
                //    newLine.sharedMaterial = materials[Math.Min((int)(average.magnitude / 0.002f), materials.Count - 1)];
                //    currentPath.Add(newLine);
                //}
                //print($"found {candidates.Count} candidates");
                //print($"center: {sphere.center}, radius: {sphere.radius}, moved: {average.magnitude}");
                iteration++;
                if (iteration >= maxPathLength)
                {
                    yield break;
                }
                yield return null;
            }
            //stopwatch.Stop();
            //print($"loop: {stopwatch.Elapsed}");

        }

#if UNITY_EDITOR
        private List<Vector3> gizmoSphereCenters = new List<Vector3>();
        private List<float> gizmoSphereRadiuses = new List<float>();
        private List<Color> gizmoSphereColours = new List<Color>();

        private void OnDrawGizmos()
        {
            for (int i = 0; i < gizmoSphereCenters.Count; ++i)
            {
                Gizmos.color = gizmoSphereColours[i];
                Gizmos.DrawWireSphere(gizmoSphereCenters[i], gizmoSphereRadiuses[i]);
                if (i < gizmoSphereCenters.Count - 1)
                {
                    Gizmos.DrawLine(gizmoSphereCenters[i], gizmoSphereCenters[i + 1]);
                }
            }
        }
#endif
    }
}
