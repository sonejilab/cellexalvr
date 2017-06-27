using UnityEngine;

public class SelectionToolMenu : MonoBehaviour {
public ConfirmSelectionButton confirmSelectionButton;
public UndoSelectionButton undoSelectionButton;
public RemoveSelectionButton removeSelectionButton;
public CreateHeatmapButton createHeatmapButton;

void Start() {
	SetEnabledState(false);
}

/// <summary>
/// Enables or disables every renderer and collider in this menu
/// </summary>

public void SetEnabledState(bool enabled) {
	foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
		r.enabled = enabled;
	}
	foreach (Collider c in GetComponentsInChildren<Collider>()) {
		c.enabled = enabled;
	}
}

public void InitializeButtons() {
	confirmSelectionButton.SetButtonState(false);
	undoSelectionButton.SetButtonState(false);
	removeSelectionButton.SetButtonState(false);
	createHeatmapButton.SetButtonState(false);
}

public void SelectionStarted() {
	createHeatmapButton.SetButtonState(false);
	confirmSelectionButton.SetButtonState(true);
	removeSelectionButton.SetButtonState(true);
	undoSelectionButton.SetButtonState(true);
}

public void ConfirmSelection() {
	createHeatmapButton.SetButtonState(true);
	confirmSelectionButton.SetButtonState(false);
	removeSelectionButton.SetButtonState(false);
	undoSelectionButton.SetButtonState(false);
}

public void RemoveSelection() {
	confirmSelectionButton.SetButtonState(false);
	removeSelectionButton.SetButtonState(false);
	createHeatmapButton.SetButtonState(false);
	undoSelectionButton.SetButtonState(false);
}

public void UndoSelection() {
	confirmSelectionButton.SetButtonState(false);
	removeSelectionButton.SetButtonState(false);
	createHeatmapButton.SetButtonState(false);
	undoSelectionButton.SetButtonState(false);
}

}
