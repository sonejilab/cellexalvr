using UnityEngine;

public class MenuToggler : MonoBehaviour
{

    private SteamVR_Controller.Device device;
    public GameObject menu;
    public Collider boxCollider;
    public SteamVR_TrackedObject leftController;
    public ControllerModelSwitcher menuSwitcher;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        device = SteamVR_Controller.Input((int)leftController.index);
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            menu.SetActive(!menu.activeSelf);
            boxCollider.enabled = !boxCollider.enabled;
            menuSwitcher.SwitchToDesiredModel();
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
