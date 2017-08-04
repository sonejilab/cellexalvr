using UnityEngine;

public class MenuToggler : MonoBehaviour
{

    private SteamVR_Controller.Device device;
    public GameObject menu;
    public Collider boxCollider;
    public SteamVR_TrackedObject leftController;
    public ControllerModelSwitcher menuSwitcher;
    public bool MenuActive { get; set; }

    private void Start()
    {
        MenuActive = menu.activeSelf;
    }

    // Update is called once per frame
    void Update()
    {
        device = SteamVR_Controller.Input((int)leftController.index);
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            MenuActive = !MenuActive;
            menu.SetActive(MenuActive);
            boxCollider.enabled = MenuActive;
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
