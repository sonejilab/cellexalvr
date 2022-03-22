using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles what happens when a graph is interacted with.
    /// </summary>
    class GraphInteract : OffsetGrab
    {
        public ReferenceManager referenceManager;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            CellexalEvents.GraphsLoaded.AddListener(RegisterColliders);
        }

        public void RegisterColliders()
        {
            enabled = false;
            colliders.AddRange(gameObject.GetComponents<BoxCollider>());
            enabled = true;
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);
            referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, false);

        }

        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            referenceManager.multiuserMessageSender.SendMessageGraphUngrabbed(gameObject.name, transform.position, transform.rotation, rigidbody.velocity, rigidbody.angularVelocity);
            base.OnSelectExiting(args);
        }
    }
}
