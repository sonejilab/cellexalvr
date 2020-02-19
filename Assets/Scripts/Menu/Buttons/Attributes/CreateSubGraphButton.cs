using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Multiuser;

namespace CellexalVR.Menu.Buttons.Attributes
{
    /// <summary>
    /// Create a new graph from a selection of attributes. 
    /// The subgraph will contain only the points that are coloured by the attributes selected.
    /// </summary>
    public class CreateSubGraphButton : CellexalButton
    {
        private GraphGenerator graphGenerator;
        private MultiuserMessageSender MultiuserMessageSender;
        private bool toggle;


        protected override string Description
        {
            get { return "Create Sub Graph"; }
        }

        protected void Start()
        {

            graphGenerator = referenceManager.graphGenerator;
            MultiuserMessageSender = referenceManager.multiuserMessageSender;
        }

        public override void Click()
        {
            graphGenerator.CreateSubGraphs(referenceManager.attributeSubMenu.attributes);
            referenceManager.multiuserMessageSender.SendMessageCreateAttributeGraph();
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