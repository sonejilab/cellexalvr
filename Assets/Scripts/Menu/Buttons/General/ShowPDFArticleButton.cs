using CellexalVR.General;

namespace CellexalVR.Menu.Buttons.General
{
    /// <summary>
    /// Resets player and play area position.
    /// </summary>
    public class ShowPDFArticleButton : CellexalButton
    {
        private bool toggle;

        protected override string Description => "Toggle PDF article";

        private void Start()
        {
            SetButtonActivated(false);
            CellexalEvents.PDFArticleRead.AddListener(() => SetButtonActivated(true));
            CellexalEvents.GraphsUnloaded.AddListener(() => SetButtonActivated(false));
        }

        public override void Click()
        {
            toggle = !toggle;
            referenceManager.pdfMesh.TogglePDF(toggle);
        }
    }
}