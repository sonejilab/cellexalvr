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
        public ScaleGrab scaleGrab;

        private bool ungrabbedThisFrame = false;
        private IXRSelectInteractor savedInteractor;
        //private Coroutine runningCoroutine;

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

        private void RegisterColliders()
        {
            enabled = false;
            colliders.AddRange(gameObject.GetComponents<BoxCollider>());
            enabled = true;
        }

        private void Update()
        {
            ungrabbedThisFrame = false;
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);
            referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, false);
            if (base.interactorsSelecting.Count == 2)
            {
                base.trackPosition = false;
                base.trackRotation = false;
                scaleGrab.doScale = true;
                scaleGrab.firstInteractor = savedInteractor;
                scaleGrab.secondInteractor = args.interactorObject;
                scaleGrab.InitialisePositions();
            }
            else
            {
                savedInteractor = args.interactorObject;
            }
        }

        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            referenceManager.multiuserMessageSender.SendMessageGraphUngrabbed(gameObject.name, transform.position, transform.rotation, rigidbody.velocity, rigidbody.angularVelocity);
            if (base.interactorsSelecting.Count == 2)
            {
                base.trackPosition = true;
                base.trackRotation = true;
                scaleGrab.doScale = false;
                if (scaleGrab.firstInteractor == args.interactorObject)
                {
                    savedInteractor = scaleGrab.secondInteractor;
                }
                else
                {
                    savedInteractor = scaleGrab.firstInteractor;
                }
                // positions and rotations have not been tracked since scaling begun, update them manually once.
                scaleGrab.firstInteractor.GetAttachTransform(args.interactableObject).transform.position = transform.position;
                scaleGrab.firstInteractor.GetAttachTransform(args.interactableObject).transform.rotation = transform.rotation;
                scaleGrab.secondInteractor.GetAttachTransform(args.interactableObject).transform.position = transform.position;
                scaleGrab.secondInteractor.GetAttachTransform(args.interactableObject).transform.rotation = transform.rotation;
            }

            base.OnSelectExiting(args);
        }
    }
}
