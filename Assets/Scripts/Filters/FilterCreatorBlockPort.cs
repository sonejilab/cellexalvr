using System;
using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using CellexalVR.SceneObjects;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Filters
{
    public class FilterCreatorBlockPort : MonoBehaviour
    {
        /// <summary>
        /// Represents a input or ouput port on a <see cref="FilterCreatorBlock"/>. Two ports can be connected to connect an output to an input on two different <see cref="FilterCreatorBlock"/>.
        /// </summary>
        public ReferenceManager referenceManager;
        public FilterManager filterManager;
        public bool IsInput;
        public FilterCreatorBlock parent;
        public FilterCreatorBlockPort connectedTo;
        public GameObject wire;

        private bool controllerInside = false;
        private MeshRenderer meshRenderer;

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
                filterManager = referenceManager.filterManager;
                parent = transform.parent?.GetComponent<FilterCreatorBlock>();
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Controller"))
            {
                parent.UnhighlightAllPorts();
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

        private void Update()
        {
            if (controllerInside && Player.instance.rightHand.grabPinchAction.GetStateDown(Player.instance.rightHand.handType))
            {
                filterManager.PortClicked(this);
            }
        }

        public void SetHighlighted(bool highlight)
        {
            controllerInside = highlight;
            meshRenderer.material.mainTextureOffset = new Vector2(highlight ? 0.5f : 0, 0);
        }

        /// <summary>
        /// Connects this port to a port on another block.
        /// </summary>
        /// <param name="other">The port to connect to.</param>
        /// <returns>True if a successful connection was made.</returns>
        public bool ConnectTo(FilterCreatorBlockPort other)
        {
            // make sure that:
            // blocks are not the same block,
            // one is input and one is ouput
            // we are not already connected to this block
            if (other.parent == this.parent || IsInput == other.IsInput || connectedTo == other)
            {
                return false;
            }

            // disconnect the other port from what ever it is connected to
            if (connectedTo != null)
            {
                connectedTo.Disconnect();
            }


            GameObject newWire = null;
            // check if the other block has a wire we can use
            if (other.wire != null)
            {
                newWire = other.wire;
            }

            // check if we already have wire we can use
            if (wire != null)
            {
                if (newWire != null)
                {
                    // if we have a wire but we have already fetched a wire, destroy ours
                    Destroy(wire.gameObject);
                    wire = null;
                }
                else
                {
                    // if we have a wire but we did not fetch one before, use ours
                    newWire = wire;
                }
            }
            // if there was no wire we could use, create one
            if (newWire == null)
            {
                newWire = Instantiate(filterManager.wirePrefab);
            }

            LineRendererFollowTransforms line = newWire.GetComponent<LineRendererFollowTransforms>();
            line.transform1 = this.transform;
            line.transform2 = other.transform;
            wire = newWire;
            other.wire = newWire;
            connectedTo = other;
            other.connectedTo = this;
            filterManager.UpdateFilterFromFilterCreator();
            return true;
        }

        public void Disconnect()
        {
            if (connectedTo == null)
            {
                // nothing to disconnect from
                return;
            }

            // disconnect both us and our connected partner
            connectedTo.connectedTo = null;
            connectedTo.wire = null;
            connectedTo = null;
            Destroy(wire.gameObject);
            wire = null;
        }

        public BooleanExpression.Expr ToExpr()
        {
            return connectedTo.parent.ToExpr();
        }
    }
}