///<summary>
/// This class represents a button used for undoing the current selection.
///</summary>
public class UndoSelectionButton : RotatableButton
{
    private SelectionToolHandler selectionToolHandler;

    protected override string Description
    {
        get { return "Cancel selection"; }
    }

    protected override void Start()
    {
        base.Start();
        selectionToolHandler = referenceManager.selectionToolHandler;
        SetButtonState(false);
        ButtonEvents.SelectionStarted.AddListener(TurnOn);
        ButtonEvents.SelectionCanceled.AddListener(TurnOff);
        ButtonEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && !isRotating)
        {
            selectionToolHandler.CancelSelection();
            SetButtonState(false);
        }
    }

    private void TurnOn()
    {
        SetButtonState(true);
    }

    private void TurnOff()
    {
        SetButtonState(false);
    }
}
