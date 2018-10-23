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

    protected override void Awake()
    {
        base.Awake();
        CellexalEvents.NetworkEnlarged.AddListener(TurnOn);
        CellexalEvents.NetworkUnEnlarged.AddListener(TurnOff);
        TurnOff();
    }

    protected override void Click()
    {
        center.SwitchLayout(layout);
        referenceManager.gameManager.InformSwitchNetworkLayout((int)layout, center.name, center.Handler.name);
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
