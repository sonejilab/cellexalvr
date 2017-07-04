///<summary>
/// This class represents a button used for undoing the current selection.
///</summary>

public class UndoSelectionButton : RotatableButton
{

    protected override string description
    {
        get { return "Cancel selection"; }
    }
    public SelectionToolHandler selectionToolHandler;

    void Update()
    {
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && !isRotating)
        {
            selectionToolHandler.CancelSelection();
        }
    }

}
