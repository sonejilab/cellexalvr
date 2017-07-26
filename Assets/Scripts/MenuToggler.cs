using UnityEngine;

public class MenuToggler : MonoBehaviour
{

    public SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;
    public ControllerModelSwitcher menuController;
    public GameObject menu;
    public Collider boxCollider;
    public SteamVR_TrackedObject rightController;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            menu.SetActive(!menu.activeSelf);
            boxCollider.enabled = !boxCollider.enabled;
            menuController.SwitchToDesiredModel();
        }
    }

    public void RotateMenuLeft()
    {
        if (menu.activeSelf)
        {
            menu.GetComponent<RotateMenu>().RotateLeft();
        }
    }

    public void RotateMenuRight()
    {
        if (menu.activeSelf)
        {
            menu.GetComponent<RotateMenu>().RotateRight();
        }
    }

}
