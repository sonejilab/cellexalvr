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
        private SteamVR_TrackedObject rightController;
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

        private bool controllerInside = false;

        private string laserColliderName =
            "[VRTK][AUTOGEN][RightControllerScriptAlias][StraightPointerRenderer_Tracer]";

        private int frameCount;
        private int layerMask;


        private void Start()
        {
            if (!CrossSceneInformation.Spectator)
            {
                rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
                minimizeTool = Handler.referenceManager.minimizeTool;
            }

            this.name = "Jail_" + MinimizedObject.name;
            orgColor = GetComponent<Renderer>().material.color;
            frameCount = 0;
            layerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
        }

        private void Update()
        {
            if (CrossSceneInformation.Spectator)
            {
                return;
            }

            var device = SteamVR_Controller.Input((int) rightController.index);
            if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
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

            frameCount++;
            // When laser is deactivated on trigger exit is not called so we check if the box is colliding with the laser.
            // To deactivate button again check every 10th frame if laser pointer collider is colliding.
            if (frameCount % 10 != 0) return;
            Transform transform1 = transform;
            Collider[] collidesWith = Physics.OverlapBox(transform1.position, transform1.localScale / 5f,
                Quaternion.identity, layerMask);

            if (collidesWith.Length == 0)
            {
                controllerInside = false;
                return;
            }

            for (int i = 0; i < collidesWith.Length; i++)
            {
                Collider col = collidesWith[i];
                if (col.gameObject.name != laserColliderName) continue;
                if (i == 0) return;
                MinimizedObjectContainer moc = col.GetComponent<MinimizedObjectContainer>();
                if (moc)
                {
                    moc.controllerInside = false;
                    GetComponent<Renderer>().material.color = orgColor;
                }
            }
        }

        private void OnDrawGizmos()
        {
            //Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size / 2);
            Gizmos.DrawSphere(transform.position, (transform.localScale / 3).x);
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == laserColliderName)
            {
                controllerInside = true;
                GetComponent<Renderer>().material.color = Color.cyan;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == laserColliderName && !controllerInside)
            {
                controllerInside = false;
                GetComponent<Renderer>().material.color = orgColor;
            }
        }
    }
}