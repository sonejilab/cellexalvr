///<summary>
/// This class represents a button used for confirming a cell selection.
///</summary>
public class ConfirmSelectionButton : StationaryButton
{

    private SelectionToolHandler selectionToolHandler;

    protected override string Description
    {
        get { return "Confirm selection"; }
    }

    protected void Start()
    {

        selectionToolHandler = referenceManager.selectionToolHandler;
        SetButtonActivated(false);
        ButtonEvents.SelectionStarted.AddListener(TurnOn);
        ButtonEvents.SelectionConfirmed.AddListener(TurnOff);
        ButtonEvents.SelectionCanceled.AddListener(TurnOff);
        ButtonEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            selectionToolHandler.SetSelectionToolEnabled(false);
            selectionToolHandler.ConfirmSelection();
            referenceManager.gameManager.InformConfirmSelection();
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
