using UnityEngine;
using CellexalVR.DesktopUI;
using CellexalVR.Interaction;

namespace CellexalVR.General
{
    public class DemoManager : MonoBehaviour
    {
        /// <summary>
        /// Controls the panel that is used when Demo mode is activated.
        /// </summary>
        public ReferenceManager referenceManager;
        public GameObject demoPanel;
        public enum SelectionState { INACTIVE, SELECTING, CONFIRMED }
        private SelectionState currentSelectionState;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

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
                referenceManager.selectionToolCollider.SetSelectionToolEnabled(true, 0);

            }
            else if (currentSelectionState == SelectionState.SELECTING)
            {
                // confirm and advance to confirmed
                currentSelectionState = SelectionState.CONFIRMED;
                referenceManager.selectionManager.ConfirmSelection();
                referenceManager.selectionToolCollider.SetSelectionToolEnabled(false, 0);
                referenceManager.controllerModelSwitcher.TurnOffActiveTool(false);
            }

        }

    }
}