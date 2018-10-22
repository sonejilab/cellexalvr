using System;
using UnityEngine;

public class SwitchNetworkLayoutButton : CellexalButton
{
    protected override string Description
    {
        get { return "Switch layout - " + layout.ToString(); }
    }
    public NetworkCenter center;
    public NetworkCenter.Layout layout;

    protected override void Click()
    {
        center.SwitchLayout(layout);
        referenceManager.gameManager.InformSwitchNetworkLayout((int)layout, center.name, center.Handler.name);
    }
}
