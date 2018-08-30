using UnityEngine;

/// <summary>
/// Represents the tool that is used to minimize objects.
/// Minimized objects are placed on top of the menu.
/// </summary>

public class MinimizeTool : MonoBehaviour
{
    public ReferenceManager referenceManager;

    private SteamVR_TrackedObject rightController;
    private MinimizedObjectHandler jail;
    private ControllerModelSwitcher controllerModelSwitcher;
    private bool controllerInside = false;
    private GameObject collidingWith;
    private int numberColliders;

    private void Start()
    {
        rightController = referenceManager.rightController;
        jail = referenceManager.minimizedObjectHandler;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
    }

    private void Update()
    {
        var device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            controllerInside = false;
            if (collidingWith.CompareTag("Graph"))
            {
                // the collider is a graphpoint
                var graph = collidingWith.transform.parent;
                if (graph == null)
                {
                    return;
                }
                graph.GetComponent<Graph>().HideGraph();
                string graphName = graph.GetComponent<Graph>().GraphName;
                jail.MinimizeObject(graph.gameObject, graphName);
                referenceManager.gameManager.InformMinimizeGraph(graphName);
            }
            else if (collidingWith.CompareTag("Network"))
            {
                if (controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Minimizer)
                    return;
                var networkHandler = collidingWith.GetComponent<NetworkHandler>();
                if (networkHandler != null)
                {
                    networkHandler.HideNetworks();
                    string networkName = collidingWith.GetComponent<NetworkHandler>().NetworkHandlerName;
                    jail.MinimizeObject(collidingWith, networkName);
                    referenceManager.gameManager.InformMinimizeNetwork(networkName);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        numberColliders++;
        collidingWith = other.gameObject;
        controllerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        numberColliders--;
        if (numberColliders == 0)
        {
            controllerInside = false;
        }
    }
}

