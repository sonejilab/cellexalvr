using System;

public class DrawToolButton : StationaryButton
{
    protected override string Description
    {
        get { return "Toggles the draw tool"; }
    }

    private DrawTool drawTool;
    private ControllerModelSwitcher controllerModelSwitcher;

    private void Start()
    {
        drawTool = referenceManager.drawTool;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
    }

    private void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            bool toolEnabled = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.DrawTool;
            if (toolEnabled)
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
            }
            else
            {

                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.DrawTool;
                controllerModelSwitcher.ActivateDesiredTool();
                // Tell the draw tool to skip its next draw because we used the trigger to press the button
                drawTool.SkipNextDraw();
            }
        }
    }
}

