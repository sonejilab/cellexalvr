using System;
using System.IO;
using UnityEngine;

public class NewFilterMenu : MonoBehaviour
{

    public ReferenceManager referenceManager;

    private FilterMenu filterMenu;
    private Filter newFilter;

    private void Start()
    {
        filterMenu = referenceManager.filterMenu;
    }

    public void AddFilter()
    {
        filterMenu.AddFilterButton(newFilter, "new filter");
        var filterFile = new StreamWriter(File.Create("test.txt"));
        filterFile.Write(newFilter.ToString());
    }
}
