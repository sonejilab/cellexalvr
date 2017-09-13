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
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && !isRotating)
        {
            selectionToolHandler.ConfirmRemove();
        }
    }

}
