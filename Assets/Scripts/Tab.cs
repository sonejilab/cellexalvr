using UnityEngine;

/// <summary>
/// A base class that can be used with <see cref="MenuWithTabs"/> to create menus with tabs.
/// </summary>
public class Tab : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public TabButton TabButton;

    private MenuToggler menuToggler;

    protected virtual void Awake()
    {
        menuToggler = referenceManager.menuToggler;
    }

    /// <summary>
    /// Show or hides all buttons that this tab contains.
    /// </summary>
    /// <param name="active"> True if this tab should be shown, false if hidden. </param>
    public virtual void SetTabActive(bool active)
    {
        foreach (Transform child in transform)
        {
            // We don't want to change the state of the tab buttons, they should always be turned on. 
            if (ReferenceEquals(child.gameObject.GetComponent<TabButton>(), null))
            {
                if (menuToggler.MenuActive)
                {
                    // if the menu is turned on
                    ToggleGameObject(child.gameObject, active);
                    // Toggle all children to the child as well.
                    foreach (Transform t in child.GetComponentsInChildren<Transform>())
                    {
                        ToggleGameObject(t.gameObject, active);
                    }

                }
                else
                {
                    // if the menu is turned off
                    menuToggler.AddGameObjectToActivateNoChildren(child.gameObject, active);
                    foreach (Transform t in child.GetComponentsInChildren<Transform>())
                    {
                        menuToggler.AddGameObjectToActivateNoChildren(t.gameObject, active);
                    }
                }
            }
            else if (!menuToggler.MenuActive)
            {
                // set the tab button to become visible when the menu is turned back on if the submenu it is attached to is turned on
                menuToggler.AddGameObjectToActivate(child.gameObject, child.GetComponent<TabButton>().Menu.gameObject);
            }
        }
    }

    private void ToggleGameObject(GameObject obj, bool active)
    {
        Renderer r = obj.GetComponent<Renderer>();
        if (r)
        {
            r.enabled = active;
        }

        Collider c = obj.GetComponent<Collider>();
        if (c)
        {
            c.enabled = active;
        }
    }
}
