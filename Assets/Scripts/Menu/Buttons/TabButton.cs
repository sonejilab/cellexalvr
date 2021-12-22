using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Menu.SubMenus;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Menu.Buttons
{
    /// <summary>
    /// Represents the tab buttons on top of a tab.
    /// </summary>
    public class TabButton : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public Tab tab;
        public MenuWithTabs Menu;

        // Open XR
        //protected SteamVR_TrackedObject rightController;
        //protected SteamVR_Controller.Device device;
        private ActionBasedController rightController;
        private InputDevice device;
        protected bool controllerInside = false;
        private MeshRenderer meshRenderer;
        private Color standardColor = Color.grey;
        private Color highlightColor = Color.blue;
        private readonly string laserCollider = "[VRTK][AUTOGEN][RightControllerScriptAlias][StraightPointerRenderer_Tracer]";
        private bool highlight;
        private int frameCount;
        private bool laserInside;
        private Transform raycastingSource;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        protected virtual void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material.color = standardColor;
            if (!CrossSceneInformation.Spectator)
            {
                rightController = referenceManager.rightController;
            }
            this.tag = "Menu Controller Collider";

            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
        }

        protected virtual void Update()
        {
            if (!CrossSceneInformation.Normal) return;

            //if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            if (!tab.Active && highlight && !controllerInside)
            {
                highlight = false;
                SetHighlighted(highlight);
            }
            if (highlight && meshRenderer.material.color != highlightColor)
            {
                SetHighlighted(highlight);
            }
            CheckForHit();
        }

        private void OnTriggerClick()
        {
            if (controllerInside)
            {
                Menu.TurnOffAllTabs();
                tab.SetTabActive(true);
                highlight = true;
                SetHighlighted(highlight);
            }
        }

        /// <summary>
        /// Button sometimes stays active even though ontriggerexit should have been called.
        /// To deactivate button again check every 10th frame if laser pointer collider is colliding.
        /// </summary>
        private void CheckForHit()
        {
            if (tab.Active) return;

            if (frameCount % 10 == 0)
            {
                laserInside = false;
                RaycastHit hit;
                raycastingSource = referenceManager.laserPointerController.origin;
                Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward), out hit, 10);
                // Open XR
                //if (hit.collider && hit.collider.transform == transform && referenceManager.rightLaser.IsTracerVisible())
                if (hit.collider && hit.collider.transform == transform && referenceManager.rightLaser.enabled)
                {
                    laserInside = true;
                    frameCount = 0;
                    controllerInside = laserInside;
                    SetHighlighted(laserInside);
                    return;
                }
                if (!(hit.collider || hit.transform == transform))
                {
                    laserInside = false;
                    controllerInside = laserInside;
                    SetHighlighted(laserInside);
                }
                controllerInside = laserInside;
                SetHighlighted(laserInside);
                frameCount = 0;
            }
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == laserCollider)
            {
                highlight = true;
                controllerInside = true;
                SetHighlighted(highlight);
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == laserCollider)
            {
                controllerInside = false;
                if (!tab.Active)
                {
                    highlight = false;
                    SetHighlighted(highlight);
                }
            }
        }

        /// <summary>
        /// Changes the color of the button to either its highlighted color or standard color.
        /// </summary>
        /// <param name="highlight"> True if the button should be highlighted, false otherwise. </param>
        public virtual void SetHighlighted(bool h)
        {
            highlight = h;
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            meshRenderer.material.color = h ? highlightColor : standardColor;
        }
    }
}