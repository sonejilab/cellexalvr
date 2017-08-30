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
        get { return "Toggle delete tool"; }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        bool deleteToolActived = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.Minimizer;
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            //deleteTool.gameObject.SetActive(!deleteToolActive);
            if (deleteToolActived)
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
                changeSprite = true;
                //controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Normal;
                //controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Normal);
            }
            else
            {

                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Minimizer;
                controllerModelSwitcher.ActivateDesiredTool();
                changeSprite = true;
            }
        }
        if (changeSprite)
        {
            if (deleteToolActived)
            {
                standardTexture = original;
            }
            if (!deleteToolActived)
            {
                standardTexture = gray;
            }
            spriteRenderer.sprite = standardTexture;
            changeSprite = false;
        }


    }
}
