using CellexalVR.AnalysisLogic;
using CellexalVR.General;

namespace CellexalVR.Menu.Buttons.Networks
{
    /// <summary>
    /// Represents the butotn that creates networks from a selection.
    /// </summary>
    public class CreateNetworksButton : CellexalButton
    {
        private NetworkGenerator networkGenerator;
        private GameManager gameManager;

        protected override string Description
        {
            get { return "Create Networks"; }
        }

        protected void Start()
        {

            networkGenerator = referenceManager.networkGenerator;
            gameManager = referenceManager.gameManager;
            SetButtonActivated(false);
            CellexalEvents.SelectionConfirmed.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
            CellexalEvents.CreatingNetworks.AddListener(TurnOff);
        }

        public override void Click()
        {
            var rand = new System.Random();
            var layoutSeed = rand.Next();
            networkGenerator.GenerateNetworks(layoutSeed);
            gameManager.InformGenerateNetworks(layoutSeed);
            referenceManager.controllerModelSwitcher.TurnOffActiveTool(true);
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