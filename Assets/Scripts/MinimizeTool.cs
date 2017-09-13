using UnityEngine;

/// <summary>
/// This class represents the tool that is used to minimize objects.
/// Minimized objects are placed on top of the menu.
/// </summary>

public class MinimizeTool : MonoBehaviour
{
    public ReferenceManager referenceManager;

    private SteamVR_TrackedObject rightController;
    private MinimizedObjectHandler jail;

    private bool controllerInside = false;
    private GameObject collidingWith;
    private int numberColliders;

    private void Start()
    {
        rightController = referenceManager.rightController;
        jail = referenceManager.minimizedObjectHandler;
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
                jail.MinimizeObject(graph.gameObject, graph.GetComponent<Graph>().GraphName);
            }
            else if (collidingWith.CompareTag("Network"))
            {
                collidingWith.GetComponent<NetworkHandler>().HideNetworks();
                jail.MinimizeObject(collidingWith, collidingWith.GetComponent<NetworkHandler>().NetworkName);
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

