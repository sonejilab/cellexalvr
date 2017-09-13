using UnityEngine;

/// <summary>
/// This class represents a button that toggles arcs between networks.
/// </summary>
public class ToggleArcsButton : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public bool toggleToState;

    private SteamVR_TrackedObject rightController;
    [HideInInspector]
    public ToggleAllCombinedArcsButton combinedNetworksButton;
    private SteamVR_Controller.Device device;
    private new Renderer renderer;
    private bool controllerInside = false;
    private Color color;
    private NetworkCenter network;

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
            combinedNetworksButton.SetCombinedArcsVisible(false);
            network.SetArcsVisible(toggleToState);
        }
    }

    /// <summary>
    /// Sets which network's arcs this button should toggle.
    /// </summary>
    public void SetNetwork(NetworkCenter network)
    {
        this.network = network;
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

