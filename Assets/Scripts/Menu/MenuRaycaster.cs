using UnityEngine;
using CellexalVR.Menu.Buttons;
using CellexalVR.Interaction;
using CellexalVR.General;

public class MenuRaycaster : MonoBehaviour
{
    public ReferenceManager referenceManager;

    private ControllerModelSwitcher modelSwitcher;
    private SteamVR_TrackedObject rightController;
    private LaserPointerController laser;
    private CellexalButton lastHit = null;
    private LayerMask layersToIgnore;

    private void OnValidate()
    {
        if (gameObject.scene.IsValid())
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }
    }

    private void Start()
    {
        rightController = referenceManager.rightController;
        modelSwitcher = referenceManager.controllerModelSwitcher;
        laser = referenceManager.rightLaser;
        layersToIgnore = (1 << LayerMask.NameToLayer("Controller") + 1 << LayerMask.NameToLayer("Ignore Raycast"));
        //print((int)layersToIgnore);
    }

    private void Update()
    {
        var raycastingSource = rightController.transform;
        var device = SteamVR_Controller.Input((int)rightController.index);
        var ray = new Ray(raycastingSource.position, raycastingSource.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 2f, layersToIgnore, QueryTriggerInteraction.Collide))
        {
            // if we hit something this frame.
            var hitButton = hit.transform.gameObject.GetComponent<CellexalButton>();
            if (hitButton != null)
            {
                if (lastHit != null && lastHit != hitButton)
                {
                    TryHitButton(lastHit, false);
                }
                SetLaserActivated(true);
                TryHitButton(hitButton, true);

                lastHit = hitButton;
            }
            else if (lastHit != null)
            {
                // if we hit something this frame but it was not a button and we hit a button last frame.
                TryHitButton(lastHit, false);
                lastHit = null;
                SetLaserActivated(false);
            }
        }
        else if (lastHit != null)
        {
            // if we hit nothing this frame, but hit something last frame.
            TryHitButton(lastHit, false);
            lastHit = null;
            SetLaserActivated(false);
        }
    }

    private void TryHitButton(CellexalButton button, bool hit)
    {
        if (button.buttonActivated)
        {
            button.SetHighlighted(hit);
            button.controllerInside = hit;
        }
    }

    private void SetLaserActivated(bool active)
    {
        if (modelSwitcher.ActualModel != ControllerModelSwitcher.Model.Keyboard
            && modelSwitcher.ActualModel != ControllerModelSwitcher.Model.TwoLasers)
        {
            laser.enabled = active;
            //laser.Toggle(active, active);
            if (active)
            {
                modelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Keyboard);
            }
            else
            {
                modelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Normal);
            }
        }
    }
}
