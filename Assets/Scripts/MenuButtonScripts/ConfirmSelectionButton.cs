///<summary>
/// This class represents a button used for confirming a cell selection.
///</summary>
public class ConfirmSelectionButton : RotatableButton
{

    private SelectionToolHandler selectionToolHandler;

    protected override string Description
    {
        get { return "Confirm selection"; }
    }

    protected override void Start()
    {
        base.Start();
        selectionToolHandler = referenceManager.selectionToolHandler;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && !isRotating)
        {
            selectionToolHandler.SetSelectionToolEnabled(false);
            selectionToolHandler.ConfirmSelection();
            referenceManager.gameManager.InformConfirmSelection();
            // ctrlMdlSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Menu);
            //ctrlMdlSwitcher.TurnOffActiveTool();
        }
    }
}
