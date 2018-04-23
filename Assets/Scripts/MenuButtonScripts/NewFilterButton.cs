using UnityEngine;

/// <summary>
/// The button for creating a new filter.
/// </summary>
public class NewFilterButton : CellexalButton
{
    private FilterMenu filterMenu;

    protected override string Description
    {
        get { return "Create a new filter"; }
    }

    protected void Start()
    {
        filterMenu = referenceManager.filterMenu;
    }

    protected override void Click()
    {
    }
}

