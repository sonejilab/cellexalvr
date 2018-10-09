using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RefreshTabsButton : CellexalButton
{

    public SubMenuButton arcsMenuButton;
    public ToggleArcsSubMenu toggleArcsMenu;

    protected override string Description
    {
        get { return "Refresh Tabs"; }
    }

    protected void Start()
    {
        SetButtonActivated(true);
    }

    protected override void Click()
    {
        referenceManager.arcsSubMenu.RefreshTabs();
        foreach (Renderer r in toggleArcsMenu.GetComponentsInChildren<Renderer>())
            r.enabled = false;
        foreach (Collider c in toggleArcsMenu.GetComponentsInChildren<Collider>())
            c.enabled = false;

        CellexalEvents.MenuClosed.Invoke();

        arcsMenuButton.SetMenuActivated(true);
    }

    private void TurnOn()
    {
        SetButtonActivated(true);
    }

    private void TurnOff()
    {
        SetButtonActivated(false);
    }
}