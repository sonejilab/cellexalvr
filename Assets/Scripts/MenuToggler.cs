using UnityEngine;

public class MenuToggler : MonoBehaviour
{

    public SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;
    public ControllerModelSwitcher menuController;
    public GameObject menu;
    public Collider collider;

    // Use this for initialization
    void Start()
    {
        device = SteamVR_Controller.Input((int)trackedObject.index);
    }

    // Update is called once per frame
    void Update()
    {
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            menu.SetActive(!menu.activeSelf);
            collider.enabled = !collider.enabled;
        }
    }

    public void rotateMenuLeft()
    {
        if (menu.activeSelf)
        {
            menu.GetComponent<RotateMenu>().RotateLeft();
        }
    }

    public void rotateMenuRight()
    {
        if (menu.activeSelf)
        {
            menu.GetComponent<RotateMenu>().RotateRight();
        }
    }

}
