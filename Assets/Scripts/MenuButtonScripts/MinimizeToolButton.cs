/// <summary>
/// Represents the buttont that minimizes things
/// </summary>

class MinimizeToolButton : CellexalButton
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
        CellexalEvents.GraphsLoaded.AddListener(TurnOn);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    protected override void Click()
    {
        bool deleteToolActived = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.Minimizer;
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

    private void TurnOn()
    {
        SetButtonActivated(true);
    }

    private void TurnOff()
    {
        SetButtonActivated(false);
    }
}
