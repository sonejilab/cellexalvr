using UnityEngine;

public class ToggleAllArcsButton : MonoBehaviour
{
    public SteamVR_TrackedObject rightController;
    public bool toggleToState;
    private SteamVR_Controller.Device device;
    private new Renderer renderer;
    private bool controllerInside = false;
    private Color color;
    private NetworkCenter[] networks;

    void Awake()
    {
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            foreach (NetworkCenter network in networks)
                network.SetArcsVisible(toggleToState);
        }
    }

    public void SetNetworks(NetworkCenter[] networks)
    {
        //color = network.GetComponent<Renderer>().material.color;
        //GetComponent<Renderer>().material.color = color;
        color = GetComponent<Renderer>().material.color;
        this.networks = networks;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
        {
            renderer.material.color = Color.white;
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
        {
            renderer.material.color = color;
            controllerInside = false;
        }
    }
}

