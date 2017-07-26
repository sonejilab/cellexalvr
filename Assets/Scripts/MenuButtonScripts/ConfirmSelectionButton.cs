///<summary>
/// This class represents a button used for confirming a cell selection.
///</summary>
public class ConfirmSelectionButton : RotatableButton
{
    public SelectionToolHandler selectionToolHandler;
    protected override string Description
    {
        get { return "Confirm selection"; }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && !isRotating)
        {
            selectionToolHandler.ConfirmSelection();
            // print("confirm");
        }
    }
}
