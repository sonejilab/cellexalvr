/// <summary>
/// Represents the button that colors the other graphs after the current selection.
/// </summary>
class RecolorGraphsAfterLastSelectionButton : CellexalButton
{
    private GraphManager graphManager;

    protected override string Description
    {
        get { return "Colors the selected cells in all other graphs"; }
    }

    private void Start()
    {
        graphManager = referenceManager.graphManager;
        SetButtonActivated(false);
        CellexalEvents.SelectionStarted.AddListener(TurnOn);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    protected override void Click()
    {
        graphManager.RecolorAllGraphsAfterSelection();
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
