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
        [SerializeField] private GameObject tip;
        [SerializeField] private Color activeColor;
        [SerializeField] private Color inactiveColor;

        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.Interaction.Toolkit.ActionBasedController leftController;
        private UnityEngine.XR.InputDevice device;
        private MinimizedObjectHandler jail;
        private ControllerModelSwitcher controllerModelSwitcher;
        private bool controllerInside = false;
        private GameObject collidingWith;
        private int numberColliders;
        private Light triggerLight => GetComponent<Light>();


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
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
            tip.GetComponent<Renderer>().material.color = inactiveColor;
        }

        private void OnTriggerClick()
        {
            // Open XR
            //device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside)
            {
                controllerInside = false;
                if (collidingWith.CompareTag("Graph") || collidingWith.CompareTag("SubGraph")
                    || collidingWith.CompareTag("FacsGraph"))
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
                    referenceManager.multiuserMessageSender.SendMessageMinimizeGraph(graphName);
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
                        referenceManager.multiuserMessageSender.SendMessageMinimizeNetwork(networkName);
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
                        referenceManager.multiuserMessageSender.SendMessageMinimizeHeatmap(heatmapName);
                    }
                }
            }
            tip.GetComponent<Renderer>().material.color = inactiveColor;

        }

        private void OnTriggerEnter(Collider other)
        {
            numberColliders++;
            if (other.CompareTag("Graph") || other.CompareTag("SubGraph") || other.CompareTag("FacsGraph")
                || other.CompareTag("HeatBoard") || other.CompareTag("Network"))
            {
                tip.GetComponent<Renderer>().material.color = activeColor;
            }
            collidingWith = other.gameObject;
            controllerInside = true;
        }

        private void OnTriggerExit(Collider other)
        {
            numberColliders--;
            if (other.CompareTag("Graph") || other.CompareTag("SubGraph") || other.CompareTag("FacsGraph")
                || other.CompareTag("HeatBoard") || other.CompareTag("Network"))
            {
                tip.GetComponent<Renderer>().material.color = inactiveColor;
            }
            if (numberColliders == 0)
            {
                controllerInside = false;
            }
        }
    }

}