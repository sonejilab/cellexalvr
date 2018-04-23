
using UnityEngine;
/// <summary>
/// Represents the button that opens the flashing genes menus.
/// </summary>
class FlashGenesMenuButton : CellexalButton
{
    protected override string Description
    {
        get { return "Show menu for flashing genes"; }
    }

    public GameObject menu;


    protected override void Click()
    {
        menu.SetActive(true);
    }
}
