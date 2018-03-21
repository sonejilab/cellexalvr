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

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            SetButtonActivated(false);
            selectionToolHandler.ConfirmRemove();
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
