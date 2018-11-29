///<summary>
/// Represents a button used for removing the current cell selection.
///</summary>
public class RemoveSelectionButton : CellexalButton
{
    private SelectionToolHandler selectionToolHandler;

    protected override string Description
    {
        get { return "Remove selection"; }
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
        referenceManager.gameManager.InformRemoveCells();
        SetButtonActivated(false);
        // more_cells selectionToolHandler.ConfirmRemove();
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
