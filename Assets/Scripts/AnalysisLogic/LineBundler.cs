using UnityEngine;
using System.Collections;
using CellexalVR.AnalysisObjects;
using System.Collections.Generic;
using CellexalVR.General;
using System.Linq;

namespace CellexalVR.AnalysisLogic
{
    /// <summary>
    /// Class that handles the lines drawn between cells in different graphs and the bundling of them. 
    /// </summary>
    public class LineBundler : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject lineBetweenTwoGraphPointsPrefab;
        public GameObject pointClusterPrefab;
        public GameObject clusterDebugBox;

        private GraphManager graphManager;
        private bool linesBundled;
        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            graphManager = referenceManager.graphManager;
        }

        /// <summary>
        /// Draws lines between the graph that was selected from to points in other graphs that share the same cell label.
        /// </summary>
        /// <param name="selection"> The graphpoints to draw the lines from. </param>
        public IEnumerator DrawLinesBetweenGraphPoints(Selection selection)
        {
            ClearLinesBetweenGraphPoints();
            var fromGraph = selection[0].parent;
            var graphsToDrawBetween = graphManager.originalGraphs.Union(graphManager.facsGraphs.Union(graphManager.attributeSubGraphs)).ToList();
            foreach (Graph toGraph in graphsToDrawBetween.FindAll(x => x != fromGraph))
            {
                Graph newGraph = referenceManager.graphGenerator.CreateGraph(GraphGenerator.GraphType.BETWEEN);
                GraphBetweenGraphs gbg = newGraph.gameObject.AddComponent<GraphBetweenGraphs>();
                if (clusterDebugBox)
                {
                    gbg.clusterDebugBox = clusterDebugBox;
                }
                gbg.graph1 = fromGraph;
                gbg.graph2 = toGraph;
                gbg.referenceManager = referenceManager;
                gbg.lineBetweenTwoGraphPointsPrefab = lineBetweenTwoGraphPointsPrefab;
                gbg.pointClusterPrefab = pointClusterPrefab;
                gbg.CreateGraphBetweenGraphs(selection, newGraph, fromGraph, toGraph);
                while (referenceManager.graphGenerator.isCreating)
                {
                    yield return null;
                }
                if (gbg)
                {
                    linesBundled = selection.size > 500;
                    StartCoroutine(gbg.ClusterLines(bundle: linesBundled));
                }
                Interaction.GraphInteract graphInteract = gbg.GetComponent<Interaction.GraphInteract>();
                graphInteract.RegisterColliders();
                graphInteract.trackPosition = false;
            }

            CellexalEvents.LinesBetweenGraphsDrawn.Invoke();
        }

        public void BundleAllLines()
        {
            foreach (Graph g in graphManager.Graphs)
            {
                foreach (GameObject obj in g.ctcGraphs)
                {
                    GraphBetweenGraphs gbg = obj.GetComponent<GraphBetweenGraphs>();
                    if (gbg.gameObject.activeSelf)
                    {
                        if (!linesBundled)
                        {
                            gbg.RemoveClusters();
                        }
                        gbg.RemoveLines();
                        StartCoroutine(gbg.ClusterLines(bundle: !linesBundled));
                    }
                }
            }
            linesBundled = !linesBundled;
        }


        /// <summary>
        /// Removes all lines between graphs.
        /// </summary>
        public void ClearLinesBetweenGraphPoints()
        {
            graphManager.ClearLinesBetweenGraphs();
            CellexalEvents.LinesBetweenGraphsCleared.Invoke();
        }

    }
}
