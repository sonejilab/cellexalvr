using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles raycasting from the controller to, heatmaps, graphs, networks, legends, et.c.
    /// This will call <see cref="CellexalRaycastable.OnRaycastEnter"/> and <see cref="CellexalRaycastable.OnRaycastHit"/> and so on.
    /// </summary>
    public class CellexalRaycast : MonoBehaviour
    {
        /* 
        raycast mask should be:
        layerMask = 1 << LayerMask.NameToLayer("MenuLayer") |
        1 << LayerMask.NameToLayer("KeyboardLayer") | 
        1 << LayerMask.NameToLayer("NetworkLayer") | 
        1 << LayerMask.NameToLayer("EnvironmentButtonLayer");
        */
        public LayerMask layerMask;

        private CellexalRaycastable lastRaycastableHit = null;

        private void OnEnable()
        {
            CellexalEvents.RightTriggerClick.AddListener(OnRightTriggerClick);
            CellexalEvents.LeftTriggerClick.AddListener(OnLeftTriggerClick);
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
                }
            }
            else if (lastRaycastableHit is not null)
            {
                // we didn't hit anything, call OnRaycastExit if we hit something last frame
                lastRaycastableHit.OnRaycastExit();
                lastRaycastableHit = null;
            }
        }

        private void OnRightTriggerClick()
        {
            if (lastRaycastableHit is not null &&
                (lastRaycastableHit.activatedBy == CellexalRaycastable.ClickType.RightTrigger ||
                lastRaycastableHit.activatedBy == CellexalRaycastable.ClickType.AnyTrigger))
            {
                lastRaycastableHit.OnActivate.Invoke();
            }
        }

        private void OnLeftTriggerClick()
        {
            if (lastRaycastableHit is not null &&
                (lastRaycastableHit.activatedBy == CellexalRaycastable.ClickType.LeftTrigger ||
                lastRaycastableHit.activatedBy == CellexalRaycastable.ClickType.AnyTrigger))
            {
                lastRaycastableHit.OnActivate.Invoke();
            }
        }
    }
}
