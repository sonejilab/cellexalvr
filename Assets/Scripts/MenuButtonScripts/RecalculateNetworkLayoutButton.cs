using System;
using UnityEngine;

public class RecalculateNetworkLayoutButton : CellexalButton
{
    protected override string Description
    {
        get { return "Recalculate layout - " + layout.ToString(); }
    }
    public NetworkCenter center;
    public NetworkCenter.Layout layout;

    protected override void Awake()
    {
        base.Awake();
        //CellexalEvents.NetworkEnlarged.AddListener(TurnOn);
        //CellexalEvents.NetworkUnEnlarged.AddListener(TurnOff);
        TurnOff();
    }

    protected override void Click()
    {
        center.CalculateLayout(layout);
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
