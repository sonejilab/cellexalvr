using UnityEngine;

public class DeleteTool : MonoBehaviour
{

    public SteamVR_TrackedObject rightController;
    public MinimizedObjectHandler jail;

    private bool controllerInside = false;
    private GameObject collidingWith;
    private int numberColliders;

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
                // the collider is a graphpoint
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

