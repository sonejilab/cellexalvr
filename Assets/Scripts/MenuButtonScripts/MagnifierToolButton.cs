/// <summary>
/// Represents the button that toggles the magnifier tool.
/// </summary>
class MagnifierToolButton : StationaryButton
{
    private ControllerModelSwitcher controllerModelSwitcher;

    protected override string Description
    {
        get { return "Toggle magnifier tool"; }
    }

    private void Start()
    {
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        SetButtonActivated(false);
        CellExAlEvents.GraphsLoaded.AddListener(TurnOn);
        CellExAlEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    private void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            bool magnifierToolActivated = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.Magnifier;
            if (magnifierToolActivated)
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
                //controllerModelSwitcher.SwitchToDesiredModel();
                //controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Normal;
            }
            else
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Magnifier;
                controllerModelSwitcher.ActivateDesiredTool();
            }
        }
    }

    private void TurnOn()
    {
        SetButtonActivated(true);
    }

    private void TurnOff()
    {
        SetButtonActivated(false);
    }
}

