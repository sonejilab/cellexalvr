using UnityEngine;

/// <summary>
/// This class represents a button that toggle all arcs between all networks on a graph skeleton.
/// </summary>
public class ToggleAllArcsButton : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public bool toggleToState;

    private SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
    private new Renderer renderer;
    private bool controllerInside = false;
    private Color color;
    private NetworkCenter[] networks;

    void Awake()
    {
        renderer = GetComponent<Renderer>();
        color = renderer.material.color;
    }

    private void Start()
    {
        rightController = referenceManager.rightController;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            foreach (NetworkCenter network in networks)
            {
                network.SetCombinedArcsVisible(false);
                network.SetArcsVisible(toggleToState);
            }
        }
    }

    /// <summary>
    /// Sets the networks that this button should deal with.
    /// </summary>
    public void SetNetworks(NetworkCenter[] networks)
    {
        //color = network.GetComponent<Renderer>().material.color;
        //GetComponent<Renderer>().material.color = color;
        this.networks = new NetworkCenter[networks.Length];
        networks.CopyTo(this.networks, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
        {
            renderer.material.color = Color.white;
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
        {
            renderer.material.color = color;
            controllerInside = false;
        }
    }
}

