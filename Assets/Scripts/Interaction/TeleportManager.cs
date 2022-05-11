using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using CellexalVR.General;

namespace Assets.Scripts.Interaction
{
    public class TeleportManager : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actionAsset;
        [SerializeField] private XRRayInteractor rayInteractor;
        [SerializeField] private TeleportationProvider provider;
        [SerializeField] private GameObject endPoint;
        [SerializeField] private InputActionReference touchPadPos;
        [SerializeField] private InputActionReference touchPadClick;

        private InputAction _thumbstick;

        private bool _requireToggleToClick;
        public bool RequireToggleToClick
        {
            get { return _requireToggleToClick; }
            set
            {
                if (_requireToggleToClick == value)
                {
                    return;
                }
                _requireToggleToClick = value;
                if (value)
                {
                    touchPadClick.action.performed += OnTeleportActivate;
                    touchPadClick.action.canceled += OnTeleportDeactivate;
                    touchPadPos.action.performed -= OnTeleportActivate;
                    touchPadPos.action.canceled -= OnTeleportDeactivate;
                }
                else
                {
                    touchPadClick.action.performed -= OnTeleportActivate;
                    touchPadPos.action.performed += OnTeleportActivate;
                    touchPadPos.action.canceled += OnTeleportDeactivate;
                }
            }
        }

        private void Awake()
        {
            rayInteractor.enabled = false;
            _requireToggleToClick = false;
            touchPadPos.action.performed += OnTeleportActivate;
            touchPadPos.action.canceled += OnTeleportDeactivate;
            CellexalEvents.ConfigLoaded.AddListener(() => RequireToggleToClick = CellexalConfig.Config.RequireTouchpadClickToInteract);
        }

        private void Update()
        {
            if (!_requireToggleToClick)
            {
                Vector2 pos = touchPadPos.action.ReadValue<Vector2>();
                if (pos.y < 0.65f)
                    return;
                rayInteractor.enabled = true;
                endPoint.SetActive(true);
            }
        }

        private void OnTeleportActivate(InputAction.CallbackContext context)
        {
            Vector2 pos = touchPadPos.action.ReadValue<Vector2>();
            if (pos.y < 0.65f)
                return;
            rayInteractor.enabled = true;
            endPoint.SetActive(true);
        }

        private void OnTeleportDeactivate(InputAction.CallbackContext context)
        {
            Teleport();
            endPoint.SetActive(false);
            rayInteractor.enabled = false;
        }

        private void Teleport()
        {
            rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
            if (!hit.collider)
            {
                rayInteractor.enabled = false;
                endPoint.SetActive(false);
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