///<summary>
/// Represents a button used for undoing the current selection.
///</summary>
public class UndoSelectionButton : CellexalButton
{
    private SelectionToolHandler selectionToolHandler;

    protected override string Description
    {
        get { return "Cancel selection"; }
    }

    protected void Start()
    {

        selectionToolHandler = referenceManager.selectionToolHandler;
        SetButtonActivated(false);
        CellexalEvents.SelectionStarted.AddListener(TurnOn);
        CellexalEvents.SelectionCanceled.AddListener(TurnOff);
        CellexalEvents.SelectionConfirmed.AddListener(TurnOff);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    protected override void Click()
    {
        referenceManager.gameManager.InformCancelSelection();
        selectionToolHandler.CancelSelection();
        SetButtonActivated(false);
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
