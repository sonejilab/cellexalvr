using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.AnalysisLogic
{
    public class VelocityPathTool : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        public class VelocityPathNode
        {
            public Vector3 center;
            public float extents;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                CalculatePath(referenceManager.selectionManager.GetCurrentSelection());
            }
        }

        public List<VelocityPathNode> CalculatePath(List<Graph.GraphPoint> selection, float threshold = 0f)
        {
            if (selection.Count == 0)
            {
                throw new ArgumentException("Can not calculate path from empty selection");
            }

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            float minX = selection.Min((gp) => gp.Position.x);
            float maxX = selection.Max((gp) => gp.Position.x);
            float minY = selection.Min((gp) => gp.Position.y);
            float maxY = selection.Max((gp) => gp.Position.y);
            float minZ = selection.Min((gp) => gp.Position.z);
            float maxZ = selection.Max((gp) => gp.Position.z);
            stopwatch.Stop();
            print($"linq: {stopwatch.Elapsed}");

            stopwatch.Restart();
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
            stopwatch.Stop();
            print($"foreach: {stopwatch.Elapsed}");
            float extents = (maxX - minX + maxY - minY + maxZ - minZ) / 2f;
            print($"extents: {extents}");
            stopwatch.Restart();
            Graph graph = selection[0].parent;
            float squaredThreshold = threshold * threshold;
            List<VelocityPathNode> path = new List<VelocityPathNode>();
            List<Graph.GraphPoint> candidates = new List<Graph.GraphPoint>();
            Dictionary<Graph.GraphPoint, Vector3> velocities = graph.velocityParticleEmitter.Velocities;
            do
            {
                float averageX = 0f;
                float averageY = 0f;
                float averageZ = 0f;

                foreach (Graph.GraphPoint point in selection)
                {
                    Vector3 velocity = velocities[point];
                    if (velocity.sqrMagnitude >= squaredThreshold)
                    {
                        averageX += velocity.x;
                        averageY += velocity.y;
                        averageZ += velocity.z;
                    }
                }

                averageX /= velocities.Count;
                averageY /= velocities.Count;
                averageZ /= velocities.Count;
                Vector3 average = new Vector3(averageX, averageY, averageZ);
                candidates.AddRange(graph.MinkowskiDetection)
            } while (candidates.Count > 0);
            stopwatch.Stop();
            print($"loop: {stopwatch.Elapsed}");
            return path;
        }
    }
}
