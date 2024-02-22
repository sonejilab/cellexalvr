using UnityEngine;
using CellexalVR.General;
using CellexalVR.AnalysisObjects;
using CellexalVR.Spatial;
using UnityEngine.InputSystem;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Use to push/pull object away/towards user. 
    /// </summary>
    public class PushBack : MonoBehaviour
    {
        //// Open XR 
        //public SteamVR_TrackedObject rightController;;
        public CellexalRaycast cellexalRaycast;
        [SerializeField] private InputActionAsset actionAsset;
        [SerializeField] private InputActionReference touchPadPos;
        [SerializeField] private InputActionReference touchPadClick;

        public float distanceMultiplier = 0.1f;
        public ReferenceManager referenceManager;

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
                    touchPadClick.action.performed += OnTouchPadClick;
                    touchPadPos.action.performed -= OnTouchPadClick;
                }
                else
                {
                    touchPadClick.action.performed -= OnTouchPadClick;
                    touchPadPos.action.performed += OnTouchPadClick;
                }
            }
        }

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
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

        private void OnTouchPadClick(InputAction.CallbackContext context)
        {
            if (cellexalRaycast.lastRaycastableHit is null)
            {
                return;
            }

            GameObject gameObjectToMove = cellexalRaycast.lastRaycastableHit.gameObject;
            if (gameObjectToMove.transform.gameObject.name.Contains("Slice"))
            {
                if (!gameObjectToMove.transform.parent.GetComponent<SpatialGraph>().slicesActive)
                {
                    return;
                }
            }

            float dist = Vector3.Distance(gameObjectToMove.transform.position, cellexalRaycast.transform.position);
            if (dist < 0.5f)
            {
                return;
            }

            var position = gameObjectToMove.transform.position;
            Vector3 dir = position - cellexalRaycast.transform.position;

            Vector2 pos = touchPadPos.action.ReadValue<Vector2>();

            if (pos.y > 0.5f)
            {
                dir = -dir.normalized;
            }
            else if (pos.y < -0.5f)
            {
                dir = dir.normalized;
            }

            position += dir * distanceMultiplier;
            Move(gameObjectToMove, position);
        }

        /// <summary>
        /// Pushes an object further from the user.
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