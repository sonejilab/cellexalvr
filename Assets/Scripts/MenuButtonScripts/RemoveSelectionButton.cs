public class RemoveSelectionButton : RotatableButton
{
protected override string description {
	get { return "Remove selection";}
}
public SelectionToolHandler selectionToolHandler;

void Update() {
	if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && !isRotating) {
		selectionToolHandler.ConfirmRemove();
	}
}
}
