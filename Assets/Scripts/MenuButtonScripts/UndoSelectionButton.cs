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
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && !isRotating)
        {
            selectionToolHandler.CancelSelection();
        }
    }

}
