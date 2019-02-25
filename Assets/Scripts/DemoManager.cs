using UnityEngine;

public class DemoManager : MonoBehaviour
{

    public ReferenceManager referenceManager;
    public GameObject demoPanel;
    public enum SelectionState { INACTIVE, SELECTING, CONFIRMED }
    private SelectionState currentSelectionState;

    void Start()
    {
        currentSelectionState = SelectionState.INACTIVE;
    }

    [ConsoleCommand("demoManager", "demo")]
    public void SetDemoPanelActive(bool active)
    {
        demoPanel.SetActive(active);
    }

    public void AdvanceSelection()
    {
        if (currentSelectionState == SelectionState.INACTIVE || currentSelectionState == SelectionState.CONFIRMED)
        {
            // start selection tool and advance to selecting
            currentSelectionState = SelectionState.SELECTING;
            referenceManager.controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.SelectionTool;
            referenceManager.controllerModelSwitcher.SwitchToDesiredModel();
            referenceManager.selectionToolHandler.SetSelectionToolEnabled(true, 0);

        }
        else if (currentSelectionState == SelectionState.SELECTING)
        {
            // confirm and advance to confirmed
            currentSelectionState = SelectionState.CONFIRMED;
            referenceManager.selectionToolHandler.ConfirmSelection();
            referenceManager.selectionToolHandler.SetSelectionToolEnabled(false, 0);
            referenceManager.controllerModelSwitcher.TurnOffActiveTool(false);
        }

    }

}
