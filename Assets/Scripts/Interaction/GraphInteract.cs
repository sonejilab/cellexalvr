using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System.Collections;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles what happens when a graph is interacted with.
    /// </summary>
    public class GraphInteract : InteractableObjectBasic //VRTK_InteractableObject
    {
        public ReferenceManager referenceManager;

        private Graph graph;

        protected override void Awake()
        {
            base.Awake();
            InteractableObjectGrabbed += Grabbed;
            InteractableObjectUnGrabbed += UnGrabbed;

        }
        
        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            graph = GetComponent<Graph>();
        }

        protected override void HandAttachedUpdate(Hand hand)
        {
            base.HandAttachedUpdate(hand);
            referenceManager.multiuserMessageSender.SendMessageMoveGraph(graph.GraphName, graph.transform.position, graph.transform.rotation, graph.transform.localScale);
        }

        private void Grabbed(object sender, Hand hand)
        {
            base.OnInteractableObjectGrabbed(hand);
            referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, false);
            //StopPositionSync();
        }

        private void UnGrabbed(object sender, Hand hand)
        {
            base.OnInteractableObjectUnGrabbed(hand);
            referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, true);
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            referenceManager.multiuserMessageSender.SendMessageGraphUngrabbed(gameObject.name, transform.position, transform.rotation, rigidbody.velocity, rigidbody.angularVelocity);
            //referenceManager.rightLaser.enabled = true;
            //referenceManager.controllerModelSwitcher.ActivateDesiredTool();
            //runningCoroutine = StartCoroutine(KeepGraphPositionSynched());
        }

    }
}