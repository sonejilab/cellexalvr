using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents a menu that has tabs.
/// </summary>
public class MenuWithTabs : MonoBehaviour
{
    public ReferenceManager referenceManager;

    protected MenuToggler menuToggler;
    protected List<Tab> tabs = new List<Tab>();
    protected Vector3 tabButtonPos = new Vector3(-0.433f, 0, 0.517f);
    protected Vector3 tabButtonPosInc = new Vector3(0.1f, 0, 0);

    protected virtual void Start()
    {
        menuToggler = referenceManager.menuToggler;
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
        //newTab.transform.parent = transform;
        newTab.TabButton.gameObject.transform.localPosition = tabButtonPos;
        newTab.TabButton.Menu = this;
        tabButtonPos += tabButtonPosInc;
        tabs.Add(newTab);
        if (!menuToggler)
            menuToggler = referenceManager.menuToggler;
        // hide the tab buttons if the tab is hidden
        foreach (Transform child in newTab.GetComponentsInChildren<Transform>())
        {
            menuToggler.AddGameObjectToActivate(child.gameObject, gameObject);
        }
        return newTab;
    }

    public virtual void ResetTabButtonPosition()
    {
        tabButtonPos = new Vector3(-0.433f, 0, 0.517f);
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
