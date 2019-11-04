using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using CellexalVR.Multiuser;

namespace CellexalVR.Menu.Buttons.Networks
{
    /// <summary>
    /// Represents the butotn that creates networks from a selection.
    /// </summary>
    public class CreateNetworksButton : CellexalButton
    {
        private NetworkGenerator networkGenerator;
        private MultiuserMessageSender MultiuserMessageSender;

        protected override string Description
        {
            get { return "Create Networks"; }
        }

        protected void Start()
        {

            networkGenerator = referenceManager.networkGenerator;
            MultiuserMessageSender = referenceManager.multiuserMessageSender;
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
            MultiuserMessageSender.SendMessageGenerateNetworks(layoutSeed);
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