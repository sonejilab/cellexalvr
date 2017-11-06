using System.Collections.Generic;
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
    // These dictionaries holds the things that were turned off when the menu was deactivated
    private Dictionary<Renderer, bool> renderers = new Dictionary<Renderer, bool>();
    private Dictionary<Collider, bool> colliders = new Dictionary<Collider, bool>();
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
        // menu.SetActive(false);
        MenuActive = false;
        SetMenuVisible(false);
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)leftController.index);
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            MenuActive = !MenuActive;
            SetMenuVisible(MenuActive);
            boxCollider.enabled = MenuActive;
            controllerModelSwitcher.SwitchToDesiredModel();
        }
    }

    /// <summary>
    /// Adds a gameobject to the list of gameobjects to show when the menu is turned back on. 
    /// The gameobject will only be hidden if the whole menu is hidden or if the submenu it is attached to is hidden.
    /// This should be used when adding a gameobject to a submenu.
    /// </summary>
    /// <param name="item"> The gameobject turn back on later. </param>
    /// <param name="subMenu"> The menu this gameobject is part of. </param>
    public void AddGameObjectToActivate(GameObject item, GameObject subMenu)
    {
        Renderer submenuRenderer = subMenu.GetComponent<Renderer>();
        // if the menu is not shown now but the submenu was shown when the menu was hidden
        if (!MenuActive)
        {
            bool submenuActive = renderers.ContainsKey(submenuRenderer) && renderers[submenuRenderer];
            Renderer r = item.GetComponent<Renderer>();
            if (r)
            {
                renderers[r] = submenuActive;
                r.enabled = false;
            }

            Collider c = item.GetComponent<Collider>();
            if (c)
            {
                colliders[c] = submenuActive;
                c.enabled = false;
            }
        }
        // if the menu is active but the submenu is not, then we should not show the new gameobject when the menu is turned back on.
        else if (MenuActive && subMenu.GetComponent<Renderer>() && !subMenu.GetComponent<Renderer>().enabled)
        {
            Renderer r = item.GetComponent<Renderer>();
            if (r)
            {
                renderers[r] = false;
                r.enabled = false;
            }

            Collider c = item.GetComponent<Collider>();
            if (c)
            {
                colliders[c] = false;
                c.enabled = false;
            }
        }
    }

    /// <summary>
    /// Adds a gameobject to the list of gameobjects to show when to menu is turned back on.
    /// </summary>
    /// <param name="item"> The gameobject to turn back on. </param>
    public void AddGameObjectToActivate(GameObject item)
    {
        if (MenuActive) return;

        Renderer r = item.GetComponent<Renderer>();
        if (r)
        {
            renderers[r] = true;
            r.enabled = false;
        }

        Collider c = item.GetComponent<Collider>();
        if (c)
        {
            colliders[c] = true;
            c.enabled = false;
        }
    }

    /// <summary>
    /// Shows or hides the menu.
    /// </summary>
    /// <param name="visible"> True for showing the menu, false otherwise. </param>
    private void SetMenuVisible(bool visible)
    {
        if (visible)
        {
            // we are turning on the menu
            // set everything back the way it was
            foreach (KeyValuePair<Renderer, bool> pair in renderers)
            {
                if (pair.Key)
                {
                    pair.Key.enabled = pair.Value;
                }
            }
            foreach (KeyValuePair<Collider, bool> pair in colliders)
            {
                if (pair.Key)
                {
                    pair.Key.enabled = pair.Value;
                }
            }
        }
        else
        {
            // we are turning off the menu
            // clear the old saved values
            renderers.Clear();
            colliders.Clear();
            // save whether each renderer and collider is enabled
            foreach (Renderer r in menu.GetComponentsInChildren<Renderer>())
            {
                if (r)
                {
                    renderers[r] = r.enabled;
                    r.enabled = false;
                }
            }
            foreach (Collider c in menu.GetComponentsInChildren<Collider>())
            {
                if (c)
                {
                    colliders[c] = c.enabled;
                    c.enabled = false;
                }
            }
            foreach (StationaryButton b in menu.GetComponentsInChildren<StationaryButton>())
            {
                b.MenuTurnedOff();
            }
        }
    }
}
