using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Selection
{
    /// <summary>
    /// Represents the button that redoes all the last undone graphpoints of the same color.
    /// </summary>
    public class RedoLastColorButton : CellexalButton
    {
        public Sprite grayScaleTexture;

        //private SelectionToolHandler selectionToolHandler;
        private SelectionManager selectionManager;

        protected override string Description
        {
            get { return "Redo last color"; }
        }

        private void Start()
        {
            selectionManager = referenceManager.selectionManager;
            SetButtonActivated(false);
            CellexalEvents.SelectionConfirmed.AddListener(TurnOff);
            CellexalEvents.SelectionCanceled.AddListener(TurnOff);
            CellexalEvents.EndOfHistoryReached.AddListener(TurnOff);
            CellexalEvents.EndOfHistoryLeft.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
        }

        public override void Click()
        {
            referenceManager.multiuserMessageSender.SendMessageRedoOneColor();
            selectionManager.GoForwardOneColorInHistory();
        }

        private void TurnOff()
        {
            SetButtonActivated(false);
        }

        private void TurnOn()
        {
            SetButtonActivated(true);
        }
    }
}