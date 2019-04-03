using CellexalVR.General;
using UnityEngine;
namespace CellexalVR.Menu.Buttons.Drawing
{
    /// <summary>
    /// Represents the button that undoes all the last selected graphpoints of the same color.
    /// </summary>
    public class UndoLastColorButton : CellexalButton
    {
        public Sprite grayScaleTexture;

        //private SelectionToolHandler selectionToolHandler;
        private SelectionManager selectionManager;
        protected override string Description
        {
            get { return "Undo last color"; }
        }

        protected override void Awake()
        {
            base.Awake();
            SetButtonActivated(false);
            CellexalEvents.SelectionStarted.AddListener(TurnOn);
            CellexalEvents.SelectionConfirmed.AddListener(TurnOff);
            CellexalEvents.SelectionCanceled.AddListener(TurnOff);
            CellexalEvents.BeginningOfHistoryReached.AddListener(TurnOff);
            CellexalEvents.BeginningOfHistoryLeft.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
        }

        private void Start()
        {
            selectionManager = referenceManager.selectionManager;
        }

        public override void Click()
        {
            referenceManager.gameManager.InformGoBackOneColor();
            selectionManager.GoBackOneColorInHistory();
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