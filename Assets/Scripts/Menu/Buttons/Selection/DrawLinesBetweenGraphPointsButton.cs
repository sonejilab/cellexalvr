using CellexalVR.AnalysisLogic;
using CellexalVR.General;

namespace CellexalVR.Menu.Buttons.Selection
{
    /// <summary>
    /// Represents a button that draws lines between all graphpoints that share labels.
    /// </summary>
    class DrawLinesBetweenGraphPointsButton : CellexalButton
    {

        //private SelectionToolHandler selectionToolHandler;
        private SelectionManager selectionManager;

        protected override string Description
        {
            get { return "Draw lines between all cells with the same label in other graphs"; }
        }

        private void Start()
        {
            selectionManager = referenceManager.selectionManager;
            SetButtonActivated(false);
            CellexalEvents.SelectionConfirmed.AddListener(TurnOn);
            CellexalEvents.LinesBetweenGraphsCleared.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
            CellexalEvents.LinesBetweenGraphsDrawn.AddListener(TurnOff);
            CellexalEvents.GraphsReset.AddListener(TurnOff);
        }

        public override void Click()
        {
            StartCoroutine(referenceManager.lineBundler.DrawLinesBetweenGraphPoints(selectionManager.GetLastSelection()));
            referenceManager.multiuserMessageSender.SendMessageDrawLinesBetweenGps();
            TurnOff();
        }

        private void TurnOn()
        {
            SetButtonActivated(true);
        }

        private void TurnOff()
        {
            SetButtonActivated(false);
        }
    }
}