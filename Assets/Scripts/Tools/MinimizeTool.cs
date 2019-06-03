using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;

namespace CellexalVR.Tools
{
    /// <summary>
    /// Represents the tool that is used to minimize objects.
    /// Minimized objects are placed on top of the menu.
    /// </summary>

    public class MinimizeTool : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        private SteamVR_TrackedObject rightController;
        private SteamVR_TrackedObject leftController;
        private MinimizedObjectHandler jail;
        private ControllerModelSwitcher controllerModelSwitcher;
        private bool controllerInside = false;
        private GameObject collidingWith;
        private int numberColliders;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        void Start()
        {
            rightController = referenceManager.rightController;
            leftController = referenceManager.leftController;
            jail = referenceManager.minimizedObjectHandler;
            controllerModelSwitcher = referenceManager.controllerModelSwitcher;

        }

        void Update()
        {
            var device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                controllerInside = false;
                if (collidingWith.CompareTag("Graph") || collidingWith.CompareTag("Subgraph"))
                {
                    // the collider is a graphpoint
                    var graph = collidingWith.transform;
                    if (graph == null)
                    {
                        return;
                    }
                    graph.GetComponent<Graph>().HideGraph();
                    string graphName = graph.GetComponent<Graph>().gameObject.name;
                    jail.MinimizeObject(graph.gameObject, graphName);
                    //minimize = true;
                    referenceManager.gameManager.InformMinimizeGraph(graphName);
                }
                if (collidingWith.CompareTag("Network"))
                {
                    if (controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Minimizer)
                        return;
                    var networkHandler = collidingWith.GetComponent<NetworkHandler>();
                    if (networkHandler != null)
                    {
                        networkHandler.HideNetworks();
                        string networkName = collidingWith.gameObject.name;
                        jail.MinimizeObject(collidingWith, networkName);
                        referenceManager.gameManager.InformMinimizeNetwork(networkName);
                    }
                }
                if (collidingWith.CompareTag("HeatBoard"))
                {
                    if (controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Minimizer)
                        return;
                    var heatmap = collidingWith.GetComponent<Heatmap>();
                    if (heatmap != null)
                    {
                        heatmap.HideHeatmap();
                        string heatmapName = heatmap.gameObject.name;
                        jail.MinimizeObject(heatmap.gameObject, heatmapName);
                        controllerInside = false;
                        referenceManager.gameManager.InformMinimizeHeatmap(heatmapName);
                    }
                }
            }

        }

        private void OnTriggerEnter(Collider other)
        {

            numberColliders++;
            if (other.CompareTag("Graph") || other.CompareTag("Subgraph") || other.CompareTag("HeatBoard") || other.CompareTag("Network"))
            {
                GetComponent<Light>().range = 0.08f;
                GetComponent<Light>().intensity = 1.1f;
            }
            collidingWith = other.gameObject;
            controllerInside = true;
        }

        private void OnTriggerExit(Collider other)
        {
            numberColliders--;
            if (other.CompareTag("Graph") || other.CompareTag("Subgraph") || other.CompareTag("HeatBoard") || other.CompareTag("Network"))
            {
                GetComponent<Light>().range = 0.04f;
                GetComponent<Light>().intensity = 0.8f;
            }
            if (numberColliders == 0)
            {
                controllerInside = false;
            }
        }
    }

}