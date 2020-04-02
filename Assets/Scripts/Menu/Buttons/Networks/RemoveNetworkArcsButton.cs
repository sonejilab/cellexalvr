using System.Collections.Generic;
using UnityEngine;
using CellexalVR.AnalysisObjects;
using CellexalVR.SceneObjects;

namespace CellexalVR.Menu.Buttons.Networks
{
    /// <summary>
    /// Represents a button that toggles arcs between networks.
    /// </summary>
    public class ToggleArcsButton : CellexalButton
    {
        public bool toggleToState;
        public List<GameObject> wires;
        public ToggleArcsButton connectedTo;
        // public Color color;

        [HideInInspector] public ToggleAllCombinedArcsButton combinedNetworksButton;
        public NetworkCenter network;
        public GameObject parentNetwork;

        private Material material;

        public Color ButtonColor
        {
            get => color;
            set
            {
                color = value;
                meshStandardColor = color;
            }
        }

        protected override string Description => "" /*Toggle all arcs connected to this network*/;
        private NetworkHandler networkHandler;
        private Color color;

        private void Start()
        {
            networkHandler = network.Handler;
            material = GetComponent<Material>();
        }

        public override void Click()
        {
            // combinedNetworksButton.SetCombinedArcsVisible(false);
            // network.SetArcsVisible(toggleToState);
            // referenceManager.multiuserMessageSender.SendMessageSetArcsVisible(toggleToState, network.name);
            referenceManager.arcsSubMenu.NetworkArcsButtonClicked(this);
        }

        public bool ConnectTo(ToggleArcsButton other)
        {
            if (other.network == this.network) return false;

            GameObject newWire = null;
            // if (other.wire != null)
            // {
            //     newWire = other.wire;
            // }
            //
            // if (newWire != null)
            // {
            //     // if we have a wire but we have already fetched a wire, destroy ours
            //     Destroy(wire.gameObject);
            //     wire = null;
            // }
            // else
            // {
            //     // if we have a wire but we did not fetch one before, use ours
            //     newWire = wire;
            // }

            // if there was no wire we could use, create one
            if (newWire == null)
            {
                newWire = Instantiate(referenceManager.arcsSubMenu.wirePrefab);
            }

            LineRendererFollowTransforms line = newWire.GetComponent<LineRendererFollowTransforms>();
            line.bendLine = true;
            line.transform1 = this.transform;
            line.transform2 = other.transform;
            wires.Add(newWire);
            other.wires.Add(newWire);
            connectedTo = other;
            other.connectedTo = this;
            network.SetArcsVisible(true, other.network);
            newWire.SetActive(true);
            return true;
        }

        public void ClearArcs()
        {
            foreach (GameObject wire in wires)
            {
                Destroy(wire.gameObject);
            }
            wires.Clear();
            network.SetArcsVisible(false);
        }
        

        public bool ConnectTo(bool toggle)
        {
            return false;
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

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Controller"))
            {
                // parent.UnhighlightAllPorts();
                SetHighlighted(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Controller"))
            {
                SetHighlighted(false);
            }
        }
    }
}