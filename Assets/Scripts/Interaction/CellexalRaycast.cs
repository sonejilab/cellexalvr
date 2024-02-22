using CellexalVR.General;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles raycasting from the controller to, heatmaps, graphs, networks, legends, et.c.
    /// This will call <see cref="CellexalRaycastable.OnRaycastEnter"/> and <see cref="CellexalRaycastable.OnRaycastHit"/> and so on.
    /// </summary>
    public class CellexalRaycast : MonoBehaviour
    {
        public enum ClickType { None, RightTrigger, LeftTrigger }

        /// <summary>
        /// Defines how this raycastable can be activated.
        /// </summary>
        public ClickType activatedBy = ClickType.RightTrigger;

        /* 
        raycast mask should be:
        layerMask = 1 << LayerMask.NameToLayer("MenuLayer") |
        1 << LayerMask.NameToLayer("GraphLayer") |
        1 << LayerMask.NameToLayer("KeyboardLayer") | 
        1 << LayerMask.NameToLayer("NetworkLayer") | 
        1 << LayerMask.NameToLayer("EnvironmentButtonLayer");
        */
        [Tooltip("This raycaster will collide with *all* colliders on this layer, even if they do not have the CellexalRaycastable component.")]
        public LayerMask layerMask;
        public XRRayInteractor rayInteractor;
        public XRInteractorLineVisual interactorLineVisual;
        [HideInInspector]
        public CellexalRaycastable lastRaycastableHit = null;

        private void OnEnable()
        {
            if (activatedBy == ClickType.RightTrigger)
            {

                CellexalEvents.RightTriggerClick.AddListener(OnRightTriggerClick);
            }

            if (activatedBy == ClickType.LeftTrigger)
            {

                CellexalEvents.LeftTriggerClick.AddListener(OnLeftTriggerClick);
            }
        }

        private void OnDisable()
        {
            CellexalEvents.RightTriggerClick.RemoveListener(OnRightTriggerClick);
            CellexalEvents.LeftTriggerClick.RemoveListener(OnLeftTriggerClick);
        }

        private void Update()
        {
            bool hit = Physics.Raycast(transform.position, transform.forward, out RaycastHit raycastInfo, 100f, layerMask, QueryTriggerInteraction.Collide);
            if (hit)
            {
                // we hit something
                CellexalRaycastable raycastable = raycastInfo.collider.transform.GetComponent<CellexalRaycastable>();
                if (raycastable is not null)
                {
                    // we hit a CellexalRaycastable
                    if (lastRaycastableHit != raycastable)
                    {
                        if (lastRaycastableHit is not null)
                        {
                            // we hit a different CellexalRaycastable than last frame, call OnRaycastExit
                            lastRaycastableHit.OnRaycastExit();
                        }
                        else
                        {
                            // we hit something new, and did not hit anything last frame, call OnRaycastStart
                            OnRaycastStart();
                        }
                        // we hit something new, call OnRaycastEnter
                        raycastable.OnRaycastEnter();
                    }
                    // call OnRaycastHit and update lastRaycastableHit for the next frame
                    raycastable.OnRaycastHit(raycastInfo, this);
                    lastRaycastableHit = raycastable;
                }
                else if (lastRaycastableHit is not null)
                {
                    // we hit something but it was not a CellexalRaycastable, call OnRaycastExit if we hit something last frame
                    lastRaycastableHit.OnRaycastExit();
                    lastRaycastableHit = null;
                    OnRaycastStop();
                }
            }
            else if (lastRaycastableHit is not null)
            {
                // we didn't hit anything, call OnRaycastExit if we hit something last frame
                lastRaycastableHit.OnRaycastExit();
                lastRaycastableHit = null;
                OnRaycastStop();
            }
        }

        private void OnRaycastStart()
        {
            rayInteractor.enabled = true;
            interactorLineVisual.enabled = true;
            interactorLineVisual.reticle.SetActive(true);
        }

        private void OnRaycastStop()
        {
            rayInteractor.enabled = false;
            interactorLineVisual.enabled = false;
            interactorLineVisual.reticle.SetActive(false);
        }

        private void OnRightTriggerClick()
        {
            if (lastRaycastableHit is not null)
            {
                lastRaycastableHit.OnActivate.Invoke();
            }
        }

        private void OnLeftTriggerClick()
        {
            if (lastRaycastableHit is not null)
            {
                lastRaycastableHit.OnActivate.Invoke();
            }
        }
    }
}
