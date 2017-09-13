using UnityEngine;

/// <summary>
/// This class holds the logic for toggling the menu.
/// </summary>
public class MenuToggler : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public bool MenuActive { get; set; }

    private SteamVR_Controller.Device device;
    private GameObject menu;
    private Collider boxCollider;
    private SteamVR_TrackedObject leftController;
    private ControllerModelSwitcher controllerModelSwitcher;


    private void Start()
    {
        menu = referenceManager.mainMenu;
        boxCollider = GetComponent<Collider>();
        leftController = referenceManager.leftController;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;

        // The menu should be turned off when the program starts
        menu.SetActive(false);
        MenuActive = false;



    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)leftController.index);
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            MenuActive = !MenuActive;
            menu.SetActive(MenuActive);
            boxCollider.enabled = MenuActive;
            controllerModelSwitcher.SwitchToDesiredModel();
        }
    }

}
