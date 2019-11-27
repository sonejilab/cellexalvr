using CellexalVR.AnalysisLogic;
using CellexalVR.General;

namespace CellexalVR.Menu.Buttons.Selection
{
    /// <summary>
    /// Represents a button that draws lines between all graphpoints that share labels.
    /// </summary>
    class ToggleBundlesButton : CellexalButton
    {

        protected override string Description
        {
            get { return "Toggle bundle lines on/off"; }
        }

        private void Start()
        {
            SetButtonActivated(false);
            CellexalEvents.SelectionConfirmed.AddListener(TurnOn);
            CellexalEvents.LinesBetweenGraphsCleared.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
            //CellexalEvents.LinesBetweenGraphsDrawn.AddListener(TurnOff);
        }

        public override void Click()
        {
            //StartCoroutine(cellManager.DrawLinesBetweenGraphPoints(selectionManager.GetLastSelection(), !toggle));
            referenceManager.cellManager.BundleAllLines();
            referenceManager.multiuserMessageSender.SendMessageBundleAllLines();
            //TurnOff();
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
