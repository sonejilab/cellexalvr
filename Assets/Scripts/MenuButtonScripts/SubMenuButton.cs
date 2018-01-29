using UnityEngine;

class SubMenuButton : StationaryButton
{
    public string description;
    public GameObject buttonsToDeactivate;
    public GameObject menu;
    public TextMesh textMeshToDarken;
    private Tab activeTab;

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
            textMeshToDarken.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
            SetMenuActivated(true);
            descriptionText.text = "";
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
            Tab tab = t.GetComponent<Tab>();
            if (!tab)
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
                // for everything that is a tab
                if (!activate)
                {
                    // if we are turning off the menu
                    // save the active tab
                    if (tab.Active)
                        activeTab = tab;
                    tab.SetTabActive(false);
                    tab.TabButton.GetComponent<Renderer>().enabled = false;
                    tab.TabButton.GetComponent<Collider>().enabled = false;
                }
                else
                {
                    // if we are turning on the menu
                    // skip the tab prefabs
                    if (menu.GetComponent<ToggleArcsSubMenu>() && tab == menu.GetComponent<ToggleArcsSubMenu>().tabPrefab) continue;
                    if (menu.GetComponent<FlashGenesMenu>() && tab == menu.GetComponent<FlashGenesMenu>().tabPrefab) continue;
                    // if we have a saved tab that should be active, turn on that one and turn off the other ones.
                    // if there is no saved active tab, turn all tabs off
                    if (activeTab != null)
                        tab.SetTabActive(tab == activeTab);
                    else
                        tab.SetTabActive(false);
                    tab.TabButton.GetComponent<Renderer>().enabled = true;
                    tab.TabButton.GetComponent<Collider>().enabled = true;
                }
            }
        }
    }

}
