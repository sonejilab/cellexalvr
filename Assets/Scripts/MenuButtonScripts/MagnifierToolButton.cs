
using UnityEngine;
/// <summary>
/// This class represents the button that toggles the magnifier tool.
/// </summary>
class MagnifierToolButton : StationaryButton
{
    public ControllerModelSwitcher controllerModelSwitcher;
    public MagnifierTool magnifier;
    public Sprite gray;
    public Sprite original;

    protected override string Description
    {
        get { return "Toggle magnifier tool"; }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            bool magnifierActive = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.Magnifier;
            if (magnifierActive)
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Normal;
                standardTexture = original;
            }
            else
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Magnifier;
                controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Magnifier);
                controllerModelSwitcher.ActivateDesiredTool();
                standardTexture = gray;
            }
        }
    }
}

