using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Tools;
using JetBrains.Annotations;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Represents an object that temporarily holds another object while it is minimized.
    /// </summary>
    public class MinimizedObjectContainer : MonoBehaviour
    {
        public Renderer rendererToHighlight;
        public Material normalMaterial;
        public Material highlightMaterial;
        public SteamVR_Action_Boolean action = SteamVR_Input.GetBooleanAction("TriggerClick");
        public SteamVR_Input_Sources inputSource = SteamVR_Input_Sources.RightHand;

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

        private int frameCount;
        private int layerMask;


        private void Start()
        {
            if (!CrossSceneInformation.Spectator)
            {
                minimizeTool = Handler.referenceManager.minimizeTool;
            }
            
            Handler.referenceManager.laserPointerController.rightLaser.PointerIn += OnPointerIn;
            Handler.referenceManager.laserPointerController.rightLaser.PointerOut += OnPointerOut;

            name = "Jail_" + MinimizedObject.name;
            //orgColor = GetComponent<Renderer>().material.color;
            frameCount = 0;
            layerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
        }

        private void Update()
        {
            if (CrossSceneInformation.Spectator)
            {
                return;
            }

            if (controllerInside && action.GetStateDown(inputSource))
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

            //frameCount++;
            // When laser is deactivated on trigger exit is not called so we check if the box is colliding with the laser.
            // To deactivate button again check every 10th frame if laser pointer collider is colliding.
            //if (frameCount % 10 != 0) return;
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

        //private void OnDrawGizmos()
        //{
        //    Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size / 2);
        //    Gizmos.DrawSphere(transform.position, (transform.localScale / 3).x);
        //}


        private void OnPointerIn(object sender, PointerEventArgs e)
        {
            if (e.target != transform) return;
            controllerInside = true;
            rendererToHighlight.sharedMaterial = highlightMaterial;
            // Handler.UpdateHighlight(this);
        }

        private void OnPointerOut(object sender, PointerEventArgs e)
        {
            if (e.target != transform) return;
            controllerInside = false;
            rendererToHighlight.sharedMaterial = normalMaterial;
        }
    }
}
