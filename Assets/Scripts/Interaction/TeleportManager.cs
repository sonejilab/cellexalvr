using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Assets.Scripts.Interaction
{
    public class TeleportManager : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actionAsset;
        [SerializeField] private XRRayInteractor rayInteractor;
        [SerializeField] private TeleportationProvider provider;

        private InputAction _thumbstick;

        private void Start()
        {
            rayInteractor.enabled = false;

            var activate = actionAsset.FindActionMap("XRI RightHand").FindAction("Teleport Mode Activate");
            activate.Enable();
            activate.performed += OnTeleportActivate;

            activate.canceled += OnTeleportDeactivate;

            //var deactivate = actionAsset.FindActionMap("XRI LeftHand").FindAction("Teleport Mode Activate");
            //activate.Enable();
            //activate.performed += OnTeleportActivate;

        }
        private void Update()
        {


        }

        private void OnTeleportActivate(InputAction.CallbackContext context)
        {
            rayInteractor.enabled = true;
        }

        private void OnTeleportDeactivate(InputAction.CallbackContext context)
        {
            Teleport();
            rayInteractor.enabled = false;
        }

        private void Teleport()
        {
            rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
            if (!hit.collider)
            {
                rayInteractor.enabled = false;
                return;
            }

            TeleportRequest teleportRequest = new TeleportRequest()
            {
                destinationPosition = hit.point
            };

            provider.QueueTeleportRequest(teleportRequest);
        }

    }
}