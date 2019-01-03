using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a menu that has tabs. Tabs are meant to function much like tabs in a web browser.
/// </summary>
public class MenuWithTabs : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public Tab tabPrefab;

    protected MenuToggler menuToggler;
    protected List<Tab> tabs = new List<Tab>();
    protected Vector3 tabButtonPos = new Vector3(-0.367f, 1f, 0.35f);
    protected Vector3 tabButtonPosOriginal = new Vector3(-0.367f, 1f, 0.35f);
    protected Vector3 tabButtonPosInc = new Vector3(0.25f, 0, 0);

    protected virtual void Start()
    {
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        menuToggler = referenceManager.menuToggler;
        CellexalEvents.GraphsUnloaded.AddListener(DestroyTabs);
    }

    /// <summary>
    /// Adds a tab to this menu.
    /// </summary>
    /// <typeparam name="T"> The type of the tab. This type must derive from <see cref="Tab"/>. </typeparam>
    /// <param name="tabPrefab"> The prefab used as template. </param>
    /// <returns> A reference to the created tab. The created tab will have the type T. </returns>
    public virtual T AddTab<T>(T tabPrefab) where T : Tab
    {
        var newTab = Instantiate(tabPrefab, transform);
        newTab.gameObject.SetActive(true);
        //newTab.SetTabActive(false);
        //newTab.transform.parent = transform;
        newTab.TabButton.gameObject.transform.localPosition = tabButtonPos;
        newTab.TabButton.Menu = this;
        tabButtonPos += tabButtonPosInc;
        tabs.Add(newTab);
        if (!menuToggler)
            menuToggler = referenceManager.menuToggler;
        // tell the menu toggler to activate the tab button later if the menu is not active
        menuToggler.AddGameObjectToActivate(newTab.TabButton.gameObject);
        return newTab;
    }

    /// <summary>
    /// Destroys one tab.
    /// </summary>
    /// <param name=""> The name of the tab to be destroyed (same as the network name corresponding to the tab). </param>
    public virtual void DestroyTab(string networkName)
    {
        Tab t = tabs.Find(tab => tab.gameObject.name.Split('_')[1].Equals(networkName));
        tabs.Remove(t);
        Destroy(t.gameObject, 0.1f);
        //foreach (Tab t in tabs)
        //{
        //    if (t.gameObject.name.Split('_')[1] == networkName)
        //    {
        //        tabs.Remove(t);
        //        Destroy(t.gameObject, 0.1f);

        //    }
        //}
        //ResetTabButtonPosition();
        //tabs.Clear();
    }

    /// <summary>
    /// Destroys all tabs.
    /// </summary>
    public virtual void DestroyTabs()
    {
        foreach (Tab t in tabs)
        {
            Destroy(t.gameObject, 0.1f);
        }
        ResetTabButtonPosition();
        tabs.Clear();
    }

    /// <summary>
    /// Reset the position of where the next tab button should be created.
    /// </summary>
    public virtual void ResetTabButtonPosition()
    {
        tabButtonPos = tabButtonPosOriginal;
    }

    /// <summary>
    /// Turns off all tabs.
    /// </summary>
    public void TurnOffAllTabs()
    {
        foreach (Tab tab in tabs)
        {
            tab.SetTabActive(false);
        }
    }
}
