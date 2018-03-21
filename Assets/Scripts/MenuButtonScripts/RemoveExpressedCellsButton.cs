/// <summary>
/// Represents the button that toggles all graphpoints that have an expression greater than 0
/// </summary>

public class RemoveExpressedCellsButton : CellexalButton
{
    private CellManager cellManager;

    protected override string Description
    {
        get { return "Toggle cells with some expression"; }
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
            cellManager.ToggleExpressedCells();
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
