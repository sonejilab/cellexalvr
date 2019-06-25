using UnityEngine;
using CellexalVR.AnalysisObjects;
namespace CellexalVR.Menu.Buttons.Networks
{
    /// <summary>
    /// Represents a button that toggles arcs between networks.
    /// </summary>
    public class ToggleArcsButton : CellexalButton
    {
        public bool toggleToState;

        [HideInInspector]
        public ToggleAllCombinedArcsButton combinedNetworksButton;
        public NetworkCenter network;
        public GameObject parentNetwork;

        protected override string Description
        {
            get { return ""; } /*Toggle all arcs connected to this network*/
        }

        public override void Click()
        {
            combinedNetworksButton.SetCombinedArcsVisible(false);
            network.SetArcsVisible(toggleToState);
            referenceManager.gameManager.InformSetArcsVisible(toggleToState, network.name);
        }

        /// <summary>
        /// Sets which network's arcs this button should toggle.
        /// </summary>
        public void SetNetwork(NetworkCenter network)
        {
            this.network = network;
            string str = this.parentNetwork.name.Split('_')[0];
            this.network.name = str;
        }
    }
}