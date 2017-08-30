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
        get { return "Toggles the help tool"; }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            bool helpToolActivated = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.HelpTool;
            //ControllerModelSwitcher.Model newModel = helpTool.activeSelf ? ControllerModelSwitcher.Model.HelpTool : ControllerModelSwitcher.Model.Normal;
            if (helpToolActivated)
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
                //controllerModelSwitcher.SwitchToDesiredModel();
                //controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Normal;
            }
            else
            {
                helpTool.SetActive(true);
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.HelpTool;
                controllerModelSwitcher.ActivateDesiredTool();
            }
        }
    }
}
