using UnityEngine;

/// <summary>
/// Represents the button that toggles the help tool.
/// </summary>
public class HelpToolButton : CellexalButton
{
    private ControllerModelSwitcher controllerModelSwitcher;
    private HelperTool helpTool;

    protected override string Description
    {
        get { return "Toggles the help tool"; }
    }

    protected override void Awake()
    {
        base.Awake();
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        helpTool = referenceManager.helpTool;
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            bool helpToolActivated = helpTool.gameObject.activeSelf;
            if (helpToolActivated)
            {
                if (referenceManager.keyboard.activeSelf)
                    controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Keyboard;
                else
                    controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Normal;
                controllerModelSwitcher.HelpToolShouldStayActivated = false;
                // we can't use controllerModelSwitcher.TurnOffActiveTool(true); here because the tool can be
                // changed while the actual helptool is still activated.
                helpTool.SetToolActivated(false);

            }
            else
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.HelpTool;
                controllerModelSwitcher.HelpToolShouldStayActivated = true;
                controllerModelSwitcher.ActivateDesiredTool();
            }
        }
    }
}
