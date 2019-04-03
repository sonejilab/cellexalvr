using UnityEngine;
using CellexalVR.General;
namespace CellexalVR.Menu.Buttons.Selection
{
    /// <summary>
    /// Represents the button that redoes the 10 last undone graphpoints.
    /// </summary>
    public class RedoTenStepsButton : CellexalButton
    {
        public Sprite grayScaleTexture;

        //private SelectionToolHandler selectionToolHandler;
        private SelectionManager selectionManager;

        protected override string Description
        {
            get { return "Redo ten steps"; }
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
            referenceManager.gameManager.InformRedoSteps(10);
            for (int i = 0; i < 10; i++)
            {
                selectionManager.GoForwardOneStepInHistory();
            }
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