using UnityEngine;
/// <summary>
/// Represents a button that opens a pop up menu.
/// </summary>
public class SubMenuButton : CellexalButton
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

    public override void Click()
    {
        OpenMenu();
    }

    public void OpenMenu()
    {
        foreach (CellexalButton b in buttonsToDeactivate.GetComponentsInChildren<CellexalButton>())
        {
            DeactivateButtonsRecursive(buttonsToDeactivate);
        }
        //textMeshToDarken.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
        textMeshToDarken.GetComponent<MeshRenderer>().enabled = false;
        SetMenuActivated(true);
        descriptionText.text = "";
    }

    private void DeactivateButtonsRecursive(GameObject buttonsToDeactivate)
    {
        foreach (Transform t in buttonsToDeactivate.transform)
        {
            if (infoMenu != null)
            {
                infoMenu.SetActive(false);
            }
            // skip all nested menues
            if (t.GetComponent<DynamicButtonMenu>() || t.GetComponent<NewFilterMenu>()) continue;
            // if this is a button, deactivate it
            t.GetComponent<CellexalButton>()?.SetButtonActivated(false);
            // recursive call to include all children of children
            DeactivateButtonsRecursive(t.gameObject);
        }
    }

    /// <summary>
    /// Show or hides the submenu
    /// </summary>
    /// <param name="activate"> True for showing the submenu, false for hiding. </param>
    public void SetMenuActivated(bool activate)
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
            // skip nested menues
            if (t.GetComponent<DynamicButtonMenu>() || t.GetComponent<NewFilterMenu>()) continue;
            // For everything that is not a tab, just deal with it normally
            Tab tab = t.GetComponent<Tab>();
            if (!tab)
            {
                SetGameObjectAndChildrenEnabled(t.gameObject, activate);
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
                    SetGameObjectAndChildrenEnabled(tab.TabButton.gameObject, false);
                }
                else
                {
                    // if we are turning on the menu
                    // skip the tab prefabs
                    var menuWithTabs = menu.GetComponent<MenuWithTabs>();
                    if (menuWithTabs && tab == menuWithTabs.tabPrefab) continue;

                    // if we have a saved tab that should be active, turn on that one and turn off the other ones.
                    // if there is no saved active tab, turn all tabs off
                    if (activeTab != null)
                        tab.SetTabActive(tab == activeTab);
                    else
                        tab.SetTabActive(false);
                    SetGameObjectAndChildrenEnabled(tab.TabButton.gameObject, true);
                }
            }
        }
    }

    private void SetGameObjectAndChildrenEnabled(GameObject obj, bool active)
    {
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
            r.enabled = active;
        foreach (var c in obj.GetComponentsInChildren<Collider>())
            c.enabled = active;
    }
}
