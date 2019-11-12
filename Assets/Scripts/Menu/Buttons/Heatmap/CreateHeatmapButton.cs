using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using CellexalVR.Multiuser;

namespace CellexalVR.Menu.Buttons.Heatmap
{
    ///<summary>
    /// Represents a button used for creating a heatmap from a cell selection.
    ///</summary>
    public class CreateHeatmapButton : CellexalButton
    {
        //public GeneDistance gd;
        private HeatmapGenerator heatmapGenerator;
        private MultiuserMessageSender MultiuserMessageSender;

        public string statsMethod;

        protected override string Description
        {
            get { return "Create heatmap"; }
        }

        protected void Start()
        {
            heatmapGenerator = referenceManager.heatmapGenerator;
            MultiuserMessageSender = referenceManager.multiuserMessageSender;
            SetButtonActivated(false);
            CellexalEvents.SelectionConfirmed.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
            CellexalEvents.CreatingHeatmap.AddListener(TurnOff);
        }

        public override void Click()
        {
            heatmapGenerator.CreateHeatmap("");
            //TurnOff();
            //referenceManager.controllerModelSwitcher.TurnOffActiveTool(true);
            //gd.CreateManyPlots();
        }

        private void TurnOn()
        {
            SetButtonActivated(true);
        }

        private void TurnOff()
        {
            SetButtonActivated(false);
            spriteRenderer.sprite = deactivatedTexture;
        }
    }
}