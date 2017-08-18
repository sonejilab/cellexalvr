using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LasersButton : StationaryButton {

    public VRTK.VRTK_StraightPointerRenderer laserPointerLeft;
    public VRTK.VRTK_StraightPointerRenderer laserPointerRight;

    protected override string Description
    {
        get { return "Toggle Lasers"; }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            laserPointerRight.enabled = !laserPointerRight.enabled;
            laserPointerLeft.enabled = !laserPointerLeft.enabled;
        }

    }
}
