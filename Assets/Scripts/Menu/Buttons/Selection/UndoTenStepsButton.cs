using UnityEngine;
using CellexalVR.General;
namespace CellexalVR.Menu.Buttons.Selection
{
    /// <summary>
    /// Represents the button that undoes the 10 last selected graphpoints.
    /// </summary>
    public class UndoTenStepsButton : CellexalButton
    {
        public Sprite grayScaleTexture;

        //private SelectionToolHandler selectionToolHandler;
        private SelectionManager selectionManager;
        protected override string Description
        {
            get { return "Undo ten steps"; }
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
            referenceManager.multiuserMessageSender.SendMessageGoBackSteps(10);
            for (int i = 0; i < 10; i++)
            {
                selectionManager.GoBackOneStepInHistory();
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