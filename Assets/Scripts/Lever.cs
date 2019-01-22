using UnityEngine;

public class Lever : MonoBehaviour
{
    public ReferenceManager referenceManager;

    private bool controllerInside = false;
    private SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            PullLever();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Controller"))
        {
            controllerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Controller"))
        {
            controllerInside = false;
        }
    }

    private void OnDisable()
    {
        controllerInside = false;
    }

    public void PullLever()
    {
        referenceManager.loaderController.LoadAllCells();
    }
}
