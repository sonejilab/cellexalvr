using System;
using UnityEngine;

public class RecalculateNetworkLayoutButton : CellexalButton
{
    protected override string Description
    {
        get { return "Recalculate layout"; }
    }
    public NetworkCenter center;
    public NetworkCenter.Layout layout;

    protected override void Click()
    {
        center.CalculateLayout(layout);
    }
}
