using System;
using UnityEngine;

///<summary>
/// This class represents a button used for toggling the burning heatmap tool.
///</summary>
public class BurnHeatmapToolButton : StationaryButton
{

    public GameObject fire;
    public ControllerModelSwitcher controllerModelSwitcher;
    private bool fireActivated = false;

    protected override string Description
    {
        get
        {
            return "Burn heatmaps tool";
        }
    }
    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.HeatmapDeleteTool)
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
            }
            else
            {
                //controllerModelSwitcher.TurnOffActiveTool();
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.HeatmapDeleteTool;
                controllerModelSwitcher.ActivateDesiredTool();
            }
        }
    }
}
