using UnityEngine;

/// <summary>
/// This class represents the button that toggles the help tool.
/// </summary>
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
            if (helpToolActivated)
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
            }
            else
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.HelpTool;
                controllerModelSwitcher.ActivateDesiredTool();
            }
        }
    }
}
