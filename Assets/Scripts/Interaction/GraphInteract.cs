using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
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

        /// <summary>
        /// Overwrites the current list of colliders, to keep the list synced with colliders created at runtime.
        /// </summary>
        public void RegisterColliders()
        {
            enabled = false;
            colliders.Clear();
            colliders.AddRange(gameObject.GetComponents<BoxCollider>());
            enabled = true;
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);
            ReferenceManager.instance.multiuserMessageSender.SendMessageDisableColliders(gameObject.name);

        }

        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            ReferenceManager.instance.multiuserMessageSender.SendMessageEnableColliders(gameObject.name);
            base.OnSelectExiting(args);
        }
    }
}
