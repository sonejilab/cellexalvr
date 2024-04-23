using UnityEngine;
using CellexalVR.General;
using CellexalVR.AnalysisObjects;
using CellexalVR.Spatial;
using UnityEngine.InputSystem;
using System.Linq;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Use to push/pull object away/towards user. 
    /// </summary>
    public class PushBack : MonoBehaviour
    {
        public CellexalRaycast cellexalRaycast;
        [SerializeField] private InputActionReference touchPadPos;
        [SerializeField] private InputActionReference touchPadClick;

        public float distanceMultiplier = 0.1f;
        public ReferenceManager referenceManager;

        private bool performMove;

        /// <summary>
        /// The Unity InputSystem invokes different actions depending on which VR hardware is used.
        /// Vive-like controllers with a touchpad invokes <see cref="touchPadClick"/> when clicked (which is when we want to call <see cref="Move(GameObject, Vector3)"/>, but controllers with joysticks never invoke this action.
        /// Joystick controllers instead invoke only <see cref="touchPadPos"/> (which the Vive-like touchpads also invoke, but when touched, not clicked).
        /// The user must set which control scheme to use in the settings menu and this property will add listeners to the correct actions.
        /// </summary>
        private bool _requireToggleToClick;
        private bool RequireToggleToClick
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
                    touchPadClick.action.started += OnTouchPadPress;
                    touchPadClick.action.canceled += OnTouchPadRelease;
                    touchPadPos.action.started -= OnTouchPadPress;
                    touchPadPos.action.canceled -= OnTouchPadRelease;
                }
                else
                {
                    touchPadClick.action.started -= OnTouchPadPress;
                    touchPadClick.action.canceled -= OnTouchPadRelease;
                    touchPadPos.action.started += OnTouchPadPress;
                    touchPadPos.action.canceled += OnTouchPadRelease;
                }
            }
        }

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
                cellexalRaycast = gameObject.GetComponent<CellexalRaycast>();
            }
        }

        private void Awake()
        {
            CellexalEvents.ConfigLoaded.AddListener(() => RequireToggleToClick = CellexalConfig.Config.RequireTouchpadClickToInteract);
        }

        private void Start()
        {
            if (CrossSceneInformation.Ghost || CrossSceneInformation.Spectator)
            {
                Destroy(this);
            }
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }

        private void Update()
        {
            if (performMove)
            {
                OnTouchPadHold();
            }
        }

        /// <summary>
        /// Called when the touchpad/joystick is actuated.
        /// </summary>
        private void OnTouchPadPress(InputAction.CallbackContext context)
        {
            performMove = true;
        }

        /// <summary>
        /// Called when the touchpad/joystick is no longer actuated.
        /// </summary>
        private void OnTouchPadRelease(InputAction.CallbackContext context)
        {
            performMove = false;
        }

        /// <summary>
        /// Called every frame that the touchpad/joystick is actuated, performs the push/pull actions.
        /// </summary>
        private void OnTouchPadHold()
        {
            CellexalRaycastable raycastable = cellexalRaycast.lastRaycastableHit;
            if (raycastable is null)
            {
                return;
            }

            if (!raycastable.canBePushedAndPulled)
            {
                CellexalRaycastable pushableParent = raycastable.GetComponentsInParent<CellexalRaycastable>().FirstOrDefault((r) => r.canBePushedAndPulled);
                if (pushableParent is null)
                {
                    return;
                }
                raycastable = pushableParent;
            }

            GameObject gameObjectToMove = raycastable.gameObject;
            if (gameObjectToMove.transform.gameObject.name.Contains("Slice"))
            {
                if (!gameObjectToMove.transform.parent.GetComponent<SpatialGraph>().slicesActive)
                {
                    return;
                }
            }

            var position = gameObjectToMove.transform.position;
            Vector3 dir = position - cellexalRaycast.transform.position;

            Vector2 pos = touchPadPos.action.ReadValue<Vector2>();

            float dist = dir.sqrMagnitude;
            if (pos.y > 0.5f)
            {
                if (dist > 100f)
                {
                    return;
                }
                dir = dir.normalized;
            }
            else if (pos.y < -0.5f)
            {
                if (dist < 0.5f)
                {
                    return;
                }
                dir = -dir.normalized;
            }

            position += dir * distanceMultiplier;
            Move(gameObjectToMove, position);
        }

        /// <summary>
        /// Moves an object further from the user or closer to the user.
        /// </summary>
        public void Move(GameObject gameObjectToMove, Vector3 position)
        {
            gameObjectToMove.transform.position = position;
            if (gameObjectToMove.transform.GetComponent<Graph>())
            {
                referenceManager.multiuserMessageSender.SendMessageMoveGraph(gameObjectToMove.transform.gameObject.name, gameObjectToMove.transform.localPosition, gameObjectToMove.transform.localRotation, gameObjectToMove.transform.localScale);
            }
            else if (gameObjectToMove.transform.GetComponent<NetworkHandler>())
            {
                referenceManager.multiuserMessageSender.SendMessageMoveNetwork(gameObjectToMove.transform.gameObject.name, gameObjectToMove.transform.localPosition, gameObjectToMove.transform.localRotation, gameObjectToMove.transform.localScale);
            }
            else if (gameObjectToMove.transform.GetComponent<NetworkCenter>())
            {
                NetworkHandler handler = gameObjectToMove.transform.GetComponent<NetworkCenter>().Handler;
                gameObjectToMove.transform.LookAt(referenceManager.headset.transform);
                gameObjectToMove.transform.Rotate(0, 0, 180);
                referenceManager.multiuserMessageSender.SendMessageMoveNetworkCenter(handler.gameObject.name, gameObjectToMove.transform.gameObject.name,
                    gameObjectToMove.transform.localPosition, gameObjectToMove.transform.localRotation, gameObjectToMove.transform.localScale);
            }
            else if (gameObjectToMove.transform.GetComponent<LegendManager>())
            {
                gameObjectToMove.transform.LookAt(cellexalRaycast.transform.transform);
                gameObjectToMove.transform.Rotate(0, 180, 0);
                referenceManager.multiuserMessageSender.SendMessageMoveLegend(gameObjectToMove.transform.position, gameObjectToMove.transform.rotation, gameObjectToMove.transform.localScale);
            }
            else if (gameObjectToMove.transform.GetComponent<Heatmap>())
            {
                gameObjectToMove.transform.LookAt(referenceManager.headset.transform);
                gameObjectToMove.transform.Rotate(0, 180, 0);
                referenceManager.multiuserMessageSender.SendMessageMoveHeatmap(gameObjectToMove.transform.gameObject.name, gameObjectToMove.transform.localPosition, gameObjectToMove.transform.localRotation, gameObjectToMove.transform.localScale);
            }
        }

    }
}