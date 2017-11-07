/// <summary>
/// This class represents the button that colors the other graphs after the current celection.
/// </summary>
class RecolorGraphsAfterLastSelectionButton : StationaryButton
{
    private SelectionToolHandler selectionToolHandler;
    private GraphManager graphManager;

    protected override string Description
    {
        get { return "Colors the selected cells in all other graphs"; }
    }

    private void Start()
    {
        selectionToolHandler = referenceManager.selectionToolHandler;
        graphManager = referenceManager.graphManager;
        SetButtonActivated(false);
        CellExAlEvents.SelectionStarted.AddListener(TurnOn);
        CellExAlEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            graphManager.RecolorAllGraphsAfterSelection();
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
