/// <summary>
/// Represents the button that clears lines drawn between graphs.
/// </summary>
class ClearLinesBetweenGraphPointsButton : CellexalButton
{

    private CellManager cellManager;

    protected override string Description
    {
        get { return "Draw lines between all cells with the same label in other graphs"; }
    }

    private void Start()
    {
        cellManager = referenceManager.cellManager;
        SetButtonActivated(false);
        CellexalEvents.LinesBetweenGraphsDrawn.AddListener(TurnOn);
    }

    protected override void Click()
    {
        cellManager.ClearLinesBetweenGraphPoints();
        CellexalEvents.LinesBetweenGraphsCleared.Invoke();
        SetButtonActivated(false);
        referenceManager.gameManager.InformClearLinesBetweenGps();
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
