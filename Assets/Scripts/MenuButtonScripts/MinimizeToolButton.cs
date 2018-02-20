/// <summary>
/// Represents the buttont that minimizes things
/// </summary>

class MinimizeToolButton : StationaryButton
{

    private ControllerModelSwitcher controllerModelSwitcher;
    private bool changeSprite;

    protected override string Description
    {
        get { return "Toggle minimizer tool"; }
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
        bool deleteToolActived = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.Minimizer;
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (deleteToolActived)
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
                spriteRenderer.sprite = standardTexture;
            }
            else
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Minimizer;
                controllerModelSwitcher.ActivateDesiredTool();
                spriteRenderer.sprite = deactivatedTexture;
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
