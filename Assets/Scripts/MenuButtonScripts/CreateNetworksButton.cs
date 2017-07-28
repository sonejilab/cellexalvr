using System;
using System.Threading;
using UnityEngine;

public class CreateNetworksButton : RotatableButton
{

    public NetworkGenerator networkGenerator;

    protected override string Description
    {
        get
        {
            return "Create Networks";
        }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            SetButtonState(false);
            networkGenerator.GenerateNetworks();
        }
    }
}


