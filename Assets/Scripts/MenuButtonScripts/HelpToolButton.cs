using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HelpToolButton : StationaryButton
{
    public GameObject helpTool;
    public ControllerModelSwitcher controllerModelSwitcher;
    protected override string Description
    {
        get
        {
            return "Toggles the help tool";
        }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            helpTool.SetActive(!helpTool.activeSelf);
            ControllerModelSwitcher.Model newModel = helpTool.activeSelf ? ControllerModelSwitcher.Model.HelpTool : ControllerModelSwitcher.Model.Normal;
            controllerModelSwitcher.DesiredModel = newModel;
            controllerModelSwitcher.SwitchToDesiredModel();
        }
    }
}
