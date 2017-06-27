public class ConfirmSelectionButton : RotatableButton {
public SelectionToolHandler selectionToolHandler;
protected override string description {
	get { return "Confirm selection";}
}

void Update() {
	if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && !isRotating) {
		selectionToolHandler.ConfirmSelection();
		// print("confirm");
	}
}
}
