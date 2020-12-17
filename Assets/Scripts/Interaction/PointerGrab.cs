using System;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Spatial;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Interaction
{
    [RequireComponent(typeof(SteamVR_LaserPointer))]
    public class PointerGrab : MonoBehaviour
    {
        public float distanceMultiplier = 0.1f;
        public SteamVR_Action_Boolean controllerAction = SteamVR_Input.GetBooleanAction("TouchpadPress");
        public Vector2 touchpadPosition;

        private ReferenceManager referenceManager;
        private SteamVR_LaserPointer laserPointer;
        private Hand hand;
        private Transform previousParent;
        private int layerMask;
        private Transform raycastingSource;
        private int maxDist = 10;
        private bool pull;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            laserPointer = GetComponent<SteamVR_LaserPointer>();
            hand = GetComponent<Hand>();
            laserPointer.PointerIn += OnPointerIn;
            laserPointer.PointerOut += OnPointerOut;
            laserPointer.PointerGrab += OnPointerGrab;
            laserPointer.PointerUnGrab += OnPointerUnGrab;
            layerMask = 1 << LayerMask.NameToLayer("GraphLayer") | 1 << LayerMask.NameToLayer("NetworkLayer")
                                                                 | 1 << LayerMask.NameToLayer("EnvironmentButtonLayer") | 1 << LayerMask.NameToLayer("Ignore Raycast");
        }

        private void Update()
        {
            if (!laserPointer.pointer.activeSelf) return;
            if (controllerAction.GetState(hand.handType))
            {
                touchpadPosition = SteamVR_Input.GetVector2("TouchpadPosition", hand.handType);
                if (touchpadPosition.y < -0.5f) 
                {
                    Pull();
                }
                if (touchpadPosition.y > 0.5f)
                {
                    Push();
                }
            }
        }

        private void AttachObjectToPointer(Transform t)
        {
            if (!laserPointer.pointer.activeSelf) return;
            t.GetComponent<InteractableObjectBasic>().HandleGrabInput(hand, hand.GetGrabStarting(), hand.GetGrabEnding(), hand.IsGrabEnding(t.gameObject));
        }

        private void DetachObjectFromPointer(Transform t)
        {
            InteractableObjectBasic interactable = t.GetComponent<InteractableObjectBasic>();
            if (interactable == null) return;
            interactable.HandleGrabInput(hand, hand.GetGrabStarting(), hand.GetGrabEnding(), hand.IsGrabEnding(t.gameObject));
        }

        private void OnPointerGrab(object sender, PointerEventArgs e)
        {
            if (!laserPointer.enabled || e.target == null || e.target.gameObject.name.Equals("Main Menu")) return;
            InteractableObjectBasic interactable = e.target.GetComponent<InteractableObjectBasic>();
            if (interactable == null) return;
            AttachObjectToPointer(e.target);
            interactable.isGrabbed = true;
        }

        private void OnPointerUnGrab(object sender, PointerEventArgs e)
        {
            if (!laserPointer.enabled || e.target == null || e.target.gameObject.name.Equals("Main Menu")) return;
            InteractableObjectBasic interactable = e.target.GetComponent<InteractableObjectBasic>();
            if (interactable == null) return;
            DetachObjectFromPointer(e.target);
            interactable.isGrabbed = false;
        }

        private void OnPointerOut(object sender, PointerEventArgs e)
        {
        }

        private void OnPointerIn(object sender, PointerEventArgs e)
        {
        }

        /// <summary>
        /// Pulls an object closer to the user.
        /// </summary>
        private void Pull()
        {
            raycastingSource = this.transform;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward), out RaycastHit hit, maxDist + 5, layerMask);
            if (!hit.collider) return;
            if (hit.transform.gameObject.name.Contains("Slice"))
            {
                if (!hit.transform.parent.GetComponent<SpatialGraph>().slicesActive) return;
            }

            // don't let the thing come too close
            if (Vector3.Distance(hit.transform.position, raycastingSource.position) < 0.5f)
            {
                pull = false;
                return;
            }

            var position = hit.transform.position;
            Vector3 dir = position - raycastingSource.position;
            dir = -dir.normalized;
            position += dir * distanceMultiplier;
            hit.transform.position = position;
            if (hit.transform.GetComponent<Graph>())
            {
                referenceManager.multiuserMessageSender.SendMessageMoveGraph(hit.transform.gameObject.name, hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<NetworkHandler>())
            {
                referenceManager.multiuserMessageSender.SendMessageMoveNetwork(hit.transform.gameObject.name, hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<NetworkCenter>())
            {
                NetworkHandler handler = hit.transform.GetComponent<NetworkCenter>().Handler;
                hit.transform.LookAt(referenceManager.headset.transform);
                hit.transform.Rotate(0, 0, 180);
                referenceManager.multiuserMessageSender.SendMessageMoveNetworkCenter(handler.gameObject.name, hit.transform.gameObject.name,
                    hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<LegendManager>())
            {
                hit.transform.LookAt(raycastingSource.transform);
                hit.transform.Rotate(0, 180, 0);
                referenceManager.multiuserMessageSender.SendMessageMoveLegend(hit.transform.position, hit.transform.rotation, hit.transform.localScale);
            }
        }

        /// <summary>
        /// Pushes an object further from the user.
        /// </summary>
        private void Push()
        {
            raycastingSource = this.transform;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward), out RaycastHit hit, maxDist, layerMask);
            if (!hit.collider) return;
            if (hit.transform.gameObject.name.Contains("Slice"))
            {
                if (!hit.transform.parent.GetComponent<SpatialGraph>().slicesActive) return;
            }

            Vector3 position = hit.transform.position;
            Vector3 dir = position - raycastingSource.position;
            dir = dir.normalized;
            position += dir * distanceMultiplier;
            hit.transform.position = position;
            if (hit.transform.GetComponent<Graph>())
            {
                referenceManager.multiuserMessageSender.SendMessageMoveGraph(hit.transform.gameObject.name, hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<NetworkHandler>())
            {
                referenceManager.multiuserMessageSender.SendMessageMoveNetwork(hit.transform.gameObject.name, hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<NetworkCenter>())
            {
                NetworkHandler handler = hit.transform.GetComponent<NetworkCenter>().Handler;
                hit.transform.LookAt(referenceManager.headset.transform);
                hit.transform.Rotate(0, 0, 180);
                referenceManager.multiuserMessageSender.SendMessageMoveNetworkCenter(handler.gameObject.name, hit.transform.gameObject.name,
                    hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<LegendManager>())
            {
                hit.transform.LookAt(raycastingSource.transform);
                hit.transform.Rotate(0, 180, 0);
                referenceManager.multiuserMessageSender.SendMessageMoveLegend(hit.transform.position, hit.transform.rotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<Heatmap>())
            {
                hit.transform.LookAt(referenceManager.headset.transform);
                hit.transform.Rotate(0, 180, 0);
                referenceManager.multiuserMessageSender.SendMessageMoveHeatmap(hit.transform.gameObject.name, hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
        }
    }
}