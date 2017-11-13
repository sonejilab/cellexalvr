using UnityEngine;

class SubMenuButton : StationaryButton
{
    public string description;
    public GameObject buttonsToDeactivate;
    public GameObject menu;

    protected override string Description
    {
        get { return description; }
    }


    private void Start()
    {
        // The gameobject should be active but the renderers and colliders should be disabled.
        // This makes the buttons in the menu able to receive events while not being shown.
        menu.SetActive(true);
        SetMenuActivated(false);
    }


    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            foreach (StationaryButton b in buttonsToDeactivate.GetComponentsInChildren<StationaryButton>())
            {
                b.SetButtonActivated(false);
            }
            SetMenuActivated(true);
        }
    }

    /// <summary>
    /// Show or hides the submenu
    /// </summary>
    /// <param name="activate"> True for showing the submenu, false for hiding. </param>
    private void SetMenuActivated(bool activate)
    {
        // Turn on or off the menu it self
        Renderer menuRenderer = menu.GetComponent<Renderer>();
        if (menuRenderer)
            menuRenderer.enabled = activate;
        Collider menuCollider = menu.GetComponent<Collider>();
        if (menuCollider)
            menuCollider.enabled = activate;

        // Go through all the objects in the menu
        foreach (Transform t in menu.transform)
        {
            // For everything that is not a tab, just deal with it normally
            if (!t.GetComponent<Tab>())
            {
                foreach (Renderer r in t.GetComponentsInChildren<Renderer>())
                {
                    r.enabled = activate;
                }
                foreach (Collider c in t.GetComponentsInChildren<Collider>())
                {
                    c.enabled = activate;
                }
            }
            else
            {
                // For everything that is a tab, just show the tab button.
                TabButton tabButton = t.GetComponentInChildren<TabButton>();
                if (tabButton)
                {
                    Renderer tabButtonRenderer = tabButton.GetComponent<Renderer>();
                    if (tabButtonRenderer)
                        tabButtonRenderer.enabled = activate;
                    Collider tabButtonCollider = tabButton.GetComponent<Collider>();
                    if (tabButtonCollider)
                        tabButtonCollider.enabled = activate;
                }
            }
        }
    }
}
