using UnityEngine;

/// <summary>
/// The class represents an object that temporarily holds another object while it is minimized.
/// </summary>
public class MinimizedObjectContainer : MonoBehaviour
{

    private SteamVR_TrackedObject rightController;
    public GameObject MinimizedObject { get; set; }
    public MinimizedObjectHandler Handler { get; set; }
    public int SpaceX { get; set; }
    public int SpaceY { get; set; }

    private bool controllerInside = false;


    private void Start()
    {
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
    }

    private void Update()
    {
        var device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (MinimizedObject.CompareTag("Graph"))
            {
                MinimizedObject.GetComponent<Graph>().ShowGraph();
            }
            if (MinimizedObject.CompareTag("Network"))
            {
                MinimizedObject.GetComponent<NetworkHandler>().ShowNetworks();
            }
            Handler.ContainerRemoved(this);
            Destroy(gameObject);
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
            controllerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
            controllerInside = false;
    }
}