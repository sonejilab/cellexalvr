using UnityEngine;

/// <summary>
/// This class represents the button that toggles the help tool.
/// </summary>
public class HelpToolButton : StationaryButton
{
    private ControllerModelSwitcher controllerModelSwitcher;

    protected override string Description
    {
        get { return "Toggles the help tool"; }
    }

    protected override void Awake()
    {
        base.Awake();
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
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
