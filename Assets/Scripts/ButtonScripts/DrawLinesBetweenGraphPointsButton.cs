/// <summary>
/// Represents a button that draws lines between all graphpoints that share labels.
/// </summary>
class DrawLinesBetweenGraphPointsButton : CellexalButton
{

    private CellManager cellManager;
    private SelectionToolHandler selectionToolHandler;

    protected override string Description
    {
        get { return "Draw lines between all cells with the same label in other graphs"; }
    }

    private void Start()
    {
        cellManager = referenceManager.cellManager;
        selectionToolHandler = referenceManager.selectionToolHandler;
        SetButtonActivated(false);
        CellexalEvents.SelectionConfirmed.AddListener(TurnOn);
        CellexalEvents.LinesBetweenGraphsCleared.AddListener(TurnOn);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
        CellexalEvents.LinesBetweenGraphsDrawn.AddListener(TurnOff);
    }

    public override void Click()
    {
        cellManager.DrawLinesBetweenGraphPoints(selectionToolHandler.GetLastSelection());
        referenceManager.gameManager.InformDrawLinesBetweenGps();
        TurnOff();
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
