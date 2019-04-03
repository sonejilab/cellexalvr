using UnityEngine;
using CellexalVR.General;
namespace CellexalVR.Menu.Buttons.Selection
{
    /// <summary>
    /// Represents the button that redoes the last undone graphpoint.
    /// </summary>
    public class RedoOneStepButton : CellexalButton
    {
        public Sprite grayScaleTexture;
        
        private SelectionManager selectionManager;
        protected override string Description
        {
            get { return "Redo one step"; }
        }

        protected override void Awake()
        {
            base.Awake();
            SetButtonActivated(false);
            CellexalEvents.SelectionConfirmed.AddListener(TurnOff);
            CellexalEvents.SelectionCanceled.AddListener(TurnOff);
            CellexalEvents.EndOfHistoryReached.AddListener(TurnOff);
            CellexalEvents.EndOfHistoryLeft.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);

        }

        private void Start()
        {
            selectionManager = referenceManager.selectionManager;
        }

        public override void Click()
        {
            referenceManager.gameManager.InformRedoSteps(1);
            selectionManager.GoForwardOneStepInHistory();
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