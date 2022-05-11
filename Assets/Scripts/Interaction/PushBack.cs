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
        [SerializeField] private InputActionAsset actionAsset;
        [SerializeField] private InputActionReference touchPadPos;
        [SerializeField] private InputActionReference touchPadClick;

        public float distanceMultiplier = 0.1f;
        public float scaleMultiplier = 0.4f;
        public float maxScale;
        public float minScale;
        public ReferenceManager referenceManager;

        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.InputDevice device;
        private Ray ray;
        private RaycastHit hit;
        private Transform raycastingSource;
        private bool push;
        private bool pull;
        private int maxDist = 10;
        private int layerMask;
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
            //_requireToggleToClick = false;
            //touchPadPos.action.performed += OnTouchPadClick;
            CellexalEvents.ConfigLoaded.AddListener(() => RequireToggleToClick = CellexalConfig.Config.RequireTouchpadClickToInteract);
        }

        private void Start()
        {
            if (CrossSceneInformation.Ghost || CrossSceneInformation.Spectator)
            {
                Destroy(this);
            }
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            layerMask = ReferenceManager.instance.rightLaser.interactionLayers;
            //layerMask = 1 << LayerMask.NameToLayer("GraphLayer") | 1 << LayerMask.NameToLayer("NetworkLayer")
            //    | 1 << LayerMask.NameToLayer("EnvironmentButtonLayer") | 1 << LayerMask.NameToLayer("Ignore Raycast");
        }

        private void Update()
        {
            if (!_requireToggleToClick || touchPadClick.action.ReadValue<float>() > 0)
            {
                if (!ReferenceManager.instance.rightLaser.enabled)
                    return;
                Vector2 pos = touchPadPos.action.ReadValue<Vector2>();
                if (pos.y > 0.5f)
                {
                    Push();
                }
                else if (pos.y < -0.5f)
                {
                    Pull();
                }
            }
        }

        private void OnTouchPadClick(InputAction.CallbackContext context)
        {
            if (!ReferenceManager.instance.rightLaser.enabled)
                return;
            Vector2 pos = touchPadPos.action.ReadValue<Vector2>();
            if (pos.y > 0.5f)
            {
                Push();
            }
            else if (pos.y < -0.5f)
            {
                Pull();
            }
        }

        /// <summary>
        /// Pulls an object closer to the user.
        /// </summary>
        public void Pull()
        {
            raycastingSource = transform;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward), out hit, maxDist + 5, layerMask);
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
            if (hit.transform.GetComponent<Graph>())
            {
                hit.transform.position = position;
                referenceManager.multiuserMessageSender.SendMessageMoveGraph(hit.transform.gameObject.name, hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<NetworkHandler>())
            {
                // hit.transform.LookAt(referenceManager.headset.transform);
                // hit.transform.Rotate(0, 0, 180);
                hit.transform.position = position;
                referenceManager.multiuserMessageSender.SendMessageMoveNetwork(hit.transform.gameObject.name, hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<NetworkCenter>())
            {
                hit.transform.position = position;
                NetworkHandler handler = hit.transform.GetComponent<NetworkCenter>().Handler;
                hit.transform.LookAt(referenceManager.headset.transform);
                hit.transform.Rotate(0, 0, 180);
                referenceManager.multiuserMessageSender.SendMessageMoveNetworkCenter(handler.gameObject.name, hit.transform.gameObject.name,
                    hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<LegendManager>())
            {
                hit.transform.position = position;
                hit.transform.LookAt(raycastingSource.transform);
                hit.transform.Rotate(0, 180, 0);
                referenceManager.multiuserMessageSender.SendMessageMoveLegend(hit.transform.position, hit.transform.rotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<Heatmap>())
            {
                hit.transform.position = position;
                hit.transform.LookAt(referenceManager.headset.transform);
                hit.transform.Rotate(0, 180, 0);
                referenceManager.multiuserMessageSender.SendMessageMoveHeatmap(hit.transform.gameObject.name, hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
        }

        /// <summary>
        /// Pushes an object further from the user.
        /// </summary>
        public void Push()
        {
            raycastingSource = transform;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward), out hit, maxDist, layerMask);
            if (!hit.collider) return;
            if (hit.transform.gameObject.name.Contains("Slice"))
            {
                if (!hit.transform.parent.GetComponent<SpatialGraph>().slicesActive) return;
            }
            Vector3 position = hit.transform.position;
            Vector3 dir = position - raycastingSource.position;
            dir = dir.normalized;
            position += dir * distanceMultiplier;
            if (hit.transform.GetComponent<Graph>())
            {
                hit.transform.position = position;
                referenceManager.multiuserMessageSender.SendMessageMoveGraph(hit.transform.gameObject.name, hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<NetworkHandler>())
            {
                // hit.transform.LookAt(referenceManager.headset.transform);
                // hit.transform.Rotate(0, 0, 180);
                hit.transform.position = position;
                referenceManager.multiuserMessageSender.SendMessageMoveNetwork(hit.transform.gameObject.name, hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<NetworkCenter>())
            {
                hit.transform.position = position;
                NetworkHandler handler = hit.transform.GetComponent<NetworkCenter>().Handler;
                hit.transform.LookAt(referenceManager.headset.transform);
                hit.transform.Rotate(0, 0, 180);
                referenceManager.multiuserMessageSender.SendMessageMoveNetworkCenter(handler.gameObject.name, hit.transform.gameObject.name,
                    hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<LegendManager>())
            {
                hit.transform.position = position;
                hit.transform.LookAt(raycastingSource.transform);
                hit.transform.Rotate(0, 180, 0);
                referenceManager.multiuserMessageSender.SendMessageMoveLegend(hit.transform.position, hit.transform.rotation, hit.transform.localScale);
            }
            else if (hit.transform.GetComponent<Heatmap>())
            {
                hit.transform.position = position;
                hit.transform.LookAt(referenceManager.headset.transform);
                hit.transform.Rotate(0, 180, 0);
                referenceManager.multiuserMessageSender.SendMessageMoveHeatmap(hit.transform.gameObject.name, hit.transform.localPosition, hit.transform.localRotation, hit.transform.localScale);
            }
        }

    }
}