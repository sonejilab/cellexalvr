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

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            selectionToolHandler.SetSelectionToolEnabled(false);
            selectionToolHandler.ConfirmSelection();
            referenceManager.gameManager.InformConfirmSelection();
            controllerModelSwitcher.TurnOffActiveTool(true);
            // ctrlMdlSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Menu);
            //ctrlMdlSwitcher.TurnOffActiveTool();
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
