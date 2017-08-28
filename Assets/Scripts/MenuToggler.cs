using UnityEngine;

/// <summary>
/// This class holds the logic for toggling the menu.
/// </summary>
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
        // The menu should be turned off when the program starts
        menu.SetActive(false);
        MenuActive = false;
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

}
