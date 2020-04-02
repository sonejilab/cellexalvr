using System.Collections.Generic;
using UnityEngine;
using CellexalVR.AnalysisObjects;
using CellexalVR.Menu.SubMenus;
using CellexalVR.SceneObjects;
using JetBrains.Annotations;
using UnityEngine.Experimental.PlayerLoop;

namespace CellexalVR.Menu.Buttons.Networks
{
    /// <summary>
    /// Represents a button that toggles arcs between networks.
    /// </summary>
    public class ToggleArcsButton : CellexalButton
    {
        public bool toggleToState;
        public List<GameObject> wires;

        // public Color color;

        [HideInInspector] public ToggleAllCombinedArcsButton combinedNetworksButton;
        public NetworkCenter network;
        // public GameObject parentNetwork;

        private List<ToggleArcsButton> connectedButtons = new List<ToggleArcsButton>();
        private ToggleArcsSubMenu arcsSubMenu;

        public Color ButtonColor
        {
            get => color;
            set
            {
                color = value / 1.5f;
                meshStandardColor = color;
                foreach (Renderer rend in GetComponentsInChildren<Renderer>())
                {
                    rend.material.color = color;
                }
            }
        }

        protected override string Description => "" /*Toggle all arcs connected to this network*/;
        // private NetworkHandler networkHandler;
        private Color color;
        public bool selected;


        private void Start()
        {
            // networkHandler = network.Handler;
            arcsSubMenu = referenceManager.arcsSubMenu;
        }


        public override void Click()
        {
            // combinedNetworksButton.SetCombinedArcsVisible(false);
            // network.SetArcsVisible(toggleToState);
            // referenceManager.multiuserMessageSender.SendMessageSetArcsVisible(toggleToState, network.name);
            arcsSubMenu.NetworkArcsButtonClicked(this);
        }


        public bool ConnectTo(ToggleArcsButton other)
        {
            if (other.network == network || connectedButtons.Contains(other)) return false;

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
                newWire = Instantiate(arcsSubMenu.wirePrefab, arcsSubMenu.transform);
            }

            LineRendererFollowTransforms line = newWire.GetComponent<LineRendererFollowTransforms>();
            line.bendLine = true;
            line.transform1 = transform;
            line.transform2 = other.transform;
            wires.Add(newWire);
            other.wires.Add(newWire);
            network.SetArcsVisible(true, other.network);
            connectedButtons.Add(other);
            other.connectedButtons.Add(this);
            newWire.SetActive(true);
            selected = false;
            return true;
        }

        public void ClearArcs()
        {
            foreach (GameObject wire in wires)
            {
                Destroy(wire.gameObject);
            }

            foreach (ToggleArcsButton button in connectedButtons)
            {
                button.connectedButtons.Remove(this);
            }

            wires.Clear();
            connectedButtons.Clear();
            network.SetArcsVisible(false);
        }

        /// <summary>
        /// Sets which network's arcs this button should toggle.
        /// </summary>
        public void SetNetwork(NetworkCenter network)
        {
            this.network = network;

            string str = network.name.Split('_')[0];
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