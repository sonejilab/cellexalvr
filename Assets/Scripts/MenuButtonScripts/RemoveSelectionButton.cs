///<summary>
/// This class represents a button used for removing the current cell selection.
///</summary>
public class RemoveSelectionButton : RotatableButton
{
    private SelectionToolHandler selectionToolHandler;

    protected override string Description
    {
        get { return "Remove selection"; }
    }

    protected override void Start()
    {
        base.Start();
        selectionToolHandler = referenceManager.selectionToolHandler;
        SetButtonState(false);
        ButtonEvents.SelectionStarted.AddListener(TurnOn);
        ButtonEvents.SelectionCanceled.AddListener(TurnOff);
        ButtonEvents.SelectionConfirmed.AddListener(TurnOff);
        ButtonEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && !isRotating)
        {
            SetButtonState(false);
            selectionToolHandler.ConfirmRemove();
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
