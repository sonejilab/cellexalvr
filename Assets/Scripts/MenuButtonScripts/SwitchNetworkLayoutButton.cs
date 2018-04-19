using System;
using UnityEngine;

public class SwitchNetworkLayoutButton : CellexalButton
{
    protected override string Description
    {
        get { return "Switch layout"; }
    }
    public NetworkCenter center;
    public NetworkCenter.Layout layout;

    private void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            center.SwitchLayout(layout);
        }
    }
}
