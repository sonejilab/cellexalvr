using System.Collections.Generic;
using UnityEngine;

public class MenuWithTabs : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public Tab tabPrefab;

    protected List<Tab> tabs = new List<Tab>();


    private void Start()
    {
        tabPrefab.gameObject.SetActive(false);
    }

    /// <summary>
    /// Turns off all tabs.
    /// </summary>
    public void TurnOffTab()
    {
        foreach (Tab tab in tabs)
        {
            tab.SetTabActive(false);
        }
    }
}
