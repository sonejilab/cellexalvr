/// <summary>
/// Represents the button that toggles all graphpoints that have an expression equal to zero
/// </summary>
public class RemoveNonExpressedCellsButton : CellexalButton
{
    private CellManager cellManager;

    protected override string Description
    {
        get { return "Toggle cells with no expression"; }
    }

    private void Start()
    {
        cellManager = referenceManager.cellManager;
        SetButtonActivated(false);
        CellexalEvents.GraphsColoredByGene.AddListener(TurnOn);
        CellexalEvents.GraphsReset.AddListener(TurnOff);
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            cellManager.ToggleNonExpressedCells();
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
