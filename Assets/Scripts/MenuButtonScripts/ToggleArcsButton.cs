using UnityEngine;

public class ToggleArcsButton : MonoBehaviour
{
    public SteamVR_TrackedObject rightController;
    public bool toggleToState;
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

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            network.SetArcsVisible(toggleToState);
        }
    }

    public void SetNetwork(NetworkCenter network)
    {
        //color = network.GetComponent<Renderer>().material.color;
        //GetComponent<Renderer>().material.color = color;
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

