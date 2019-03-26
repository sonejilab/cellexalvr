namespace CellexalVR.Menu.Buttons.Facs
{
    /// <summary>
    /// Create a new graph where the axes are the selected markers in the FACS menu.
    /// </summary>
    public class NewGraphFromMarkersButton : CellexalButton
    {
        private string indexName;

        protected override string Description
        {
            get { return "Create new Graph"; }
        }

        protected void Start()
        {
        }

        public override void Click()
        {
            referenceManager.newGraphFromMarkers.CreateMarkerGraph();
            referenceManager.gameManager.InformCreateMarkerGraph();
        }

    }

}