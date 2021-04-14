using System.Collections.Generic;
using System.Linq;
using CellexalVR.General;
using CellexalVR.Interaction;
using DefaultNamespace;
using Unity.Entities;

namespace CellexalVR.Menu.Buttons.Selection
{
    ///<summary>
    /// Represents a button used for confirming a cell selection.
    ///</summary>
    public class ConfirmSelection : CellexalButton
    {
        //private SelectionToolHandler selectionToolHandler;
        private SelectionManager selectionManager;
        private ControllerModelSwitcher controllerModelSwitcher;

        protected override string Description => "Confirm selection";

        protected void Start()
        {
            selectionManager = referenceManager.selectionManager;
            controllerModelSwitcher = referenceManager.controllerModelSwitcher;
            SetButtonActivated(false);
            CellexalEvents.SelectionStarted.AddListener(TurnOn);
            CellexalEvents.SelectionConfirmed.AddListener(TurnOff);
            CellexalEvents.SelectionCanceled.AddListener(TurnOff);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
            CellexalEvents.BeginningOfHistoryReached.AddListener(TurnOff);
            CellexalEvents.BeginningOfHistoryLeft.AddListener(TurnOn);
        }

        public override void Click()
        {
            referenceManager.selectionToolCollider.SetSelectionToolEnabled(false);
            EntityQuery query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(PointCloudComponent));
            int c = query.CalculateEntityCount();
            if (c > 0)
            {
                selectionManager.ConfirmPCSelection();
            }
            else
            {
                selectionManager.ConfirmSelection();
            }

            referenceManager.multiuserMessageSender.SendMessageConfirmSelection();
            //controllerModelSwitcher.TurnOffActiveTool(true);
            // ctrlMdlSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Menu);
            //ctrlMdlSwitcher.TurnOffActiveTool();
        }

        private void TurnOn()
        {
            SetButtonActivated(true);
            StoreState();
        }

        private void TurnOff()
        {
            SetButtonActivated(false);
            // spriteRenderer.sprite = deactivatedTexture;
        }
    }
}