using CellexalVR.AnalysisObjects;
using CellexalVR.General;

namespace CellexalVR.Menu.Buttons.Attributes
{
    /// <summary>
    /// Create a new graph from a selection of attributes. 
    /// The subgraph will contain only the points that are coloured by the attributes selected.
    /// </summary>
    public class CreateSubGraphButton : CellexalButton
    {

        private GraphGenerator graphGenerator;
        private GameManager gameManager;

        protected override string Description
        {
            get { return "Create Sub Graph"; }
        }

        protected void Start()
        {

            graphGenerator = referenceManager.graphGenerator;
            gameManager = referenceManager.gameManager;
        }

        public override void Click()
        {
            if (referenceManager.attributeSubMenu.attributes.Count > 0)
            {
                graphGenerator.CreateSubGraphs(referenceManager.attributeSubMenu.attributes);
                referenceManager.gameManager.InformCreateAttributeGraph();
            }
            else
            {
                print(referenceManager.attributeSubMenu.attributes.Count);
            }
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