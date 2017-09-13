using UnityEngine;

/// <summary>
/// This class represents the buttont that minimizes things
/// </summary>

class MinimizeToolButton : StationaryButton
{
    public ControllerModelSwitcher controllerModelSwitcher;
    public MinimizeTool deleteTool;
    public Sprite original;
    public Sprite gray;
    private bool changeSprite;

    protected override string Description
    {
        get { return "Toggle minimizer tool"; }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        bool deleteToolActived = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.Minimizer;
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (deleteToolActived)
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
                spriteRenderer.sprite = original;
            }
            else
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Minimizer;
                controllerModelSwitcher.ActivateDesiredTool();
                spriteRenderer.sprite = gray;
            }
        }
    }
}
