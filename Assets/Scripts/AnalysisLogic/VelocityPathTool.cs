using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.AnalysisLogic
{
    public class VelocityPathTool : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public LineRenderer linePrefab;
        public Material materialPrefab;

        private List<LineRenderer> currentPath = new List<LineRenderer>();
        private List<Vector3> currentPathCenters = new List<Vector3>();
        private List<LineRenderer> previousPath = new List<LineRenderer>();
        private int frameCount = 0;
        private int maxPathLength = 15;
        private bool active = false;
        private List<Material> materials = new List<Material>();

        private void Awake()
        {
            CellexalEvents.ConfigLoaded.AddListener(OnConfigLoaded);
        }

        private void OnConfigLoaded()
        {
            materials.Clear();
            int halfNbrOfExpressionColors = CellexalConfig.Config.GraphNumberOfExpressionColors / 2;

            Color[] lowMidExpressionColors = CellexalVR.Extensions.Extensions.InterpolateColors(CellexalConfig.Config.GraphLowExpressionColor, CellexalConfig.Config.GraphMidExpressionColor, halfNbrOfExpressionColors);
            Color[] midHighExpressionColors = CellexalVR.Extensions.Extensions.InterpolateColors(CellexalConfig.Config.GraphMidExpressionColor, CellexalConfig.Config.GraphHighExpressionColor, CellexalConfig.Config.GraphNumberOfExpressionColors - halfNbrOfExpressionColors + 1);
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
                        //print(touchingGraph);
                        Vector3 selectionToolPosition = touchingGraph.transform.InverseTransformPoint(referenceManager.selectionToolCollider.transform.position);
                        StartCoroutine(CalculatePath(touchingGraph.MinkowskiDetection(selectionToolPosition, 0.05f, -1)));
                    }
                }
                frameCount++;
            }
        }

        private struct Sphere
        {
            public Vector3 center;
            public float radius;

            public Sphere(Vector3 center, float radius)
            {
                this.center = center;
                this.radius = radius;
            }
        }
        private Sphere CalculateSphereFromSelection(List<Graph.GraphPoint> selection)
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
            return new Sphere(centerPos, sphereRadius);
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
            Sphere pathNode = CalculateSphereFromSelection(selection);
            //print($"original selection contains: {selection.Count} cells");
            //print($"center: {sphere.center}, {sphere.radius}");
            float squaredThreshold = threshold * threshold;
            List<Graph.GraphPoint> candidates = new List<Graph.GraphPoint>();
            candidates.AddRange(selection);
            Dictionary<Graph.GraphPoint, Vector3> velocities = graph.velocityParticleEmitter.Velocities;
            int iteration = 0;
            graph.octreeRoot.Group = -1;
            currentPathCenters.Add(pathNode.center);
            foreach (Graph.GraphPoint point in selection)
            {
                point.node.Group = 0;
            }

            while (candidates.Count > 0)
            {
#if UNITY_EDITOR
                //gizmoSphereCenters.Add(graph.transform.TransformPoint(pathNode.center));
                //gizmoSphereRadiuses.Add(pathNode.radius);
                //gizmoSphereColours.Add(CellexalConfig.Config.SelectionToolColors[iteration % CellexalConfig.Config.SelectionToolColors.Length]);
#endif
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
                if (average.magnitude < stepThreshold)
                {
                    //print($"average: {average.magnitude}. average below step threshold, breaking out of loop");
                    break;
                }
                candidates.Clear();
                // grow sphere slightly to make up for CalculateSphereFromSelection making it slightly smaller
                candidates.AddRange(graph.MinkowskiDetection(pathNode.center, pathNode.radius * 1.1f, iteration));
                //foreach (Graph.GraphPoint point in candidates)
                //{
                //    referenceManager.selectionManager.AddGraphpointToSelection(point, iteration, false);
                //}
                pathNode = CalculateSphereFromSelection(candidates);
                if (candidates.Count > 0)
                {
                    currentPathCenters.Add(pathNode.center);
                    var newLine = Instantiate(linePrefab, graph.transform);
                    newLine.enabled = true;
                    newLine.SetPositions(new Vector3[] { currentPathCenters[iteration], currentPathCenters[iteration + 1] });
                    newLine.sharedMaterial = materials[Math.Min((int)(average.magnitude / 0.002f), materials.Count - 1)];
                    currentPath.Add(newLine);
                }
                //print($"found {candidates.Count} candidates");
                //print($"center: {sphere.center}, radius: {sphere.radius}, moved: {average.magnitude}");
                iteration++;
                if (iteration >= maxPathLength)
                {
                    break;
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
