using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Tools;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Represents an object that temporarily holds another object while it is minimized.
    /// </summary>
    public class MinimizedObjectContainer : MonoBehaviour
    {
        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.InputDevice device;
        // Open XR 
        private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        private MinimizeTool minimizeTool;
        private Color orgColor;
        public GameObject MinimizedObject { get; set; }
        public MinimizedObjectHandler Handler { get; set; }

        /// <summary>
        /// The x-coordinate in the grid that this container is in.
        /// Has a range of [0, 4]
        /// </summary>
        public int SpaceX { get; set; }

        /// <summary>
        /// The y-coordinate in the grid that this container is in.
        /// Has a range of [0, 4]
        /// </summary>
        public int SpaceY { get; set; }

        public bool controllerInside = false;

        private string laserColliderName =
            "[VRTK][AUTOGEN][RightControllerScriptAlias][StraightPointerRenderer_Tracer]";

        private int frameCount;
        private int layerMask;


        private void Start()
        {
            if (!CrossSceneInformation.Spectator)
            {
                rightController = Handler.referenceManager.rightController;
                minimizeTool = Handler.referenceManager.minimizeTool;
            }

            this.name = "Jail_" + MinimizedObject.name;
            orgColor = GetComponent<Renderer>().material.color;
            frameCount = 0;
            layerMask = 1 << LayerMask.NameToLayer("MenuLayer");
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
        }

        private void Update()
        {
            frameCount++;
            // When laser is deactivated on trigger exit is not called so we check if the box is colliding with the laser.
            // To deactivate button again check every 10th frame if laser pointer collider is colliding.
            if (frameCount % 10 != 0) return;
            // frameCount = 0;
            // Transform transform1 = transform;
            // Collider[] collidesWith = Physics.OverlapBox(transform1.position, transform1.localScale / 2f,
            //     transform1.rotation, layerMask);
            //
            // if (collidesWith.Length == 0)
            // {
            //     controllerInside = false;
            //     return;
            // }
            //
            // foreach (Collider col in collidesWith)
            // {
            //     if (col.gameObject.name == laserColliderName)
            //     {
            //         controllerInside = true;
            //         return;
            //     }
            // }
            // controllerInside = false;
            // GetComponent<Renderer>().material.color = orgColor;
        }

        // Open XR
        private void OnTriggerClick()
        {
            if (CrossSceneInformation.Spectator)
            {
                return;
            }
            RaycastHit hit;
            Transform raycastingSource = Handler.referenceManager.laserPointerController.origin;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward), out hit, 10, layerMask);
            if (!hit.collider)
            {
                return;
            }
            if (MinimizedObject.CompareTag("Graph"))
            {
                MinimizedObject.GetComponent<Graph>().ShowGraph();
                Handler.referenceManager.multiuserMessageSender.SendMessageShowGraph(MinimizedObject.name,
                    this.name);
            }

            if (MinimizedObject.CompareTag("SubGraph"))
            {
                MinimizedObject.GetComponent<Graph>().ShowGraph();
                //minimizeTool.MaximizeObject(MinimizedObject, this, "Network");
                Handler.referenceManager.multiuserMessageSender.SendMessageShowGraph(MinimizedObject.name,
                    this.name);
            }

            if (MinimizedObject.CompareTag("FacsGraph"))
            {
                MinimizedObject.GetComponent<Graph>().ShowGraph();
                //minimizeTool.MaximizeObject(MinimizedObject, this, "Network");
                Handler.referenceManager.multiuserMessageSender.SendMessageShowGraph(MinimizedObject.name,
                    this.name);
            }

            if (MinimizedObject.CompareTag("Network"))
            {
                MinimizedObject.GetComponent<NetworkHandler>().ShowNetworks();
                //minimizeTool.MaximizeObject(MinimizedObject, this, "Network");
                Handler.referenceManager.multiuserMessageSender.SendMessageShowNetwork(MinimizedObject.name,
                    this.name);
            }

            if (MinimizedObject.CompareTag("HeatBoard"))
            {
                MinimizedObject.GetComponent<Heatmap>().ShowHeatmap();
                //minimizeTool.MaximizeObject(MinimizedObject, this, "Network");
                Handler.referenceManager.multiuserMessageSender.SendMessageShowHeatmap(MinimizedObject.name,
                    this.name);
            }

            Handler.ContainerRemoved(this);
            Destroy(gameObject);
        }



        private void OnDrawGizmos()
        {
            //Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size / 2);
            Gizmos.DrawSphere(transform.position, (transform.localScale / 3).x);
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name != laserColliderName) return;
            controllerInside = true;
            GetComponent<Renderer>().material.color = Color.cyan;
            // Handler.UpdateHighlight(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name != laserColliderName) return;
            controllerInside = false;
            GetComponent<Renderer>().material.color = orgColor;
        }
    }
}