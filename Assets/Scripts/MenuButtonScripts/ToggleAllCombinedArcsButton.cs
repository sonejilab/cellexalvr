using UnityEngine;

public class ToggleAllCombinedArcsButton : MonoBehaviour
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
        color = renderer.material.color;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            foreach (NetworkCenter network in networks)
            {
                network.SetArcsVisible(false);
                network.SetCombinedArcsVisible(toggleToState);
            }
        }
    }

    public void SetNetworks(NetworkCenter[] networks)
    {
        //color = network.GetComponent<Renderer>().material.color;
        //GetComponent<Renderer>().material.color = color;
        this.networks = networks;
    }

    public void SetCombinedArcsVisible(bool visible)
    {
        foreach (NetworkCenter network in networks)
        {
            network.SetCombinedArcsVisible(visible);
        }
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

