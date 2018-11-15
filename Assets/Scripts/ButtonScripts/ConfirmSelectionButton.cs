///<summary>
/// Represents a button used for confirming a cell selection.
///</summary>
public class ConfirmSelectionButton : CellexalButton
{

    private SelectionToolHandler selectionToolHandler;
    private ControllerModelSwitcher controllerModelSwitcher;

    protected override string Description
    {
        get { return "Confirm selection"; }
    }

    protected void Start()
    {

        selectionToolHandler = referenceManager.selectionToolHandler;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        SetButtonActivated(false);
        CellexalEvents.SelectionStarted.AddListener(TurnOn);
        CellexalEvents.SelectionConfirmed.AddListener(TurnOff);
        CellexalEvents.SelectionCanceled.AddListener(TurnOff);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    protected override void Click()
    {
        selectionToolHandler.SetSelectionToolEnabled(false, 0);
        selectionToolHandler.ConfirmSelection();
        referenceManager.gameManager.InformConfirmSelection();
        controllerModelSwitcher.TurnOffActiveTool(true);
        // ctrlMdlSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Menu);
        //ctrlMdlSwitcher.TurnOffActiveTool();
    }

    private void TurnOn()
    {
        SetButtonActivated(true);
    }

    private void TurnOff()
    {
        SetButtonActivated(false);
        spriteRenderer.sprite = deactivatedTexture;
    }
}
