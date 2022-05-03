using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Menu.SubMenus;
using CellexalVR.Multiuser;
using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.VFX;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// This class sole purpose is to forward collision events to the selection tool handler
    /// </summary>
    public class SelectionToolCollider : MonoBehaviour
    {
        public static SelectionToolCollider instance;

        [SerializeField] private InputActionAsset inputActionAsset;
        [SerializeField] private InputActionReference touchPadClick;
        //[SerializeField] private InputActionReference thumbStickClick;
        [SerializeField] private InputActionReference touchPadPos;
        [SerializeField] private VisualEffect vfx;

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

        public ReferenceManager referenceManager;
        public SelectionManager selectionManager;
        public Material selectionToolMaterial;
        public Collider[] selectionToolColliders;
        public Color[] Colors;
        public bool hapticFeedbackThisFrame = true;
        public bool annotate;
        public bool selActive = false;

        private int currentMeshIndex;
        private int tempColorIndex;
        private Color selectedColor;

        /// <summary>
        /// 0: paddle, 1: bludgeon, 2: smaller bludgeon, 4: stick, 5-8: same shapes but removes selected points instead (white color).
        /// </summary>
        public int CurrentMeshIndex
        {
            get => currentMeshIndex;
            set
            {
                currentMeshIndex = value;
                if (currentMeshIndex >= selectionToolColliders.Length)
                {
                    currentMeshIndex = 0;
                    if (CurrentColorIndex > Colors.Length) CurrentColorIndex = tempColorIndex;
                }
                // Change to remove selection tool: Store old color index and change to white color.
                else if (currentMeshIndex > (selectionToolColliders.Length / 2) - 1 &&
                         CurrentColorIndex <= Colors.Length)
                {
                    tempColorIndex = CurrentColorIndex;
                    CurrentColorIndex = Colors.Length + 1;
                    vfx.SetVector3("ParticleColor", new Vector3(1, 1, 1));
                }
                // Change to remove selection tool: Store color index so when we switch back to normal selection tool we get the correct color.
                else if (currentMeshIndex < 0)
                {
                    currentMeshIndex = selectionToolColliders.Length - 1;
                    if (CurrentColorIndex > Colors.Length)
                    {
                        CurrentColorIndex = tempColorIndex;
                    }

                    else
                    {
                        tempColorIndex = CurrentColorIndex;
                        CurrentColorIndex = Colors.Length + 1;
                    }
                }
                // Get back to normal mesh and switch back to color we had before.
                else if ((currentMeshIndex >= 0 && currentMeshIndex <= selectionToolColliders.Length / 2 - 1)
                         && (CurrentColorIndex > Colors.Length))
                {
                    CurrentColorIndex = tempColorIndex;
                }

                UpdateShapeIcons();
            }
        }

        private int currentColorIndex;

        public int CurrentColorIndex
        {
            get => currentColorIndex;
            set
            {
                currentColorIndex = value;
                if (currentColorIndex > Colors.Length)
                {

                    vfx.SetVector3("ParticleColor", new Vector3(1, 1, 1));
                    for (int i = selectionToolColliders.Length / 2; i < selectionToolColliders.Length; i++)
                    {
                        Collider collider = selectionToolColliders[i];
                        Color removalColor = Color.white;
                        removalColor.a = 0.1f;
                        collider.GetComponent<Renderer>().material.color = removalColor;
                    }

                    return;
                }

                Color col = Colors[currentColorIndex];
                col.a = 0.1f;
                for (int i = 0; i < selectionToolColliders.Length / 2; i++)
                {
                    Collider collider = selectionToolColliders[i];
                    collider.GetComponent<Renderer>().material.color = col;
                }
                
                vfx.SetVector3("ParticleColor", new Vector3(col.r, col.g, col.b));
            }
        }

        private ControllerModelSwitcher controllerModelSwitcher;
        private GraphManager graphManager;
        private MultiuserMessageSender multiuserMessageSender;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Awake()
        {
            instance = this;
            SetSelectionToolEnabled(false);

            if (CellexalConfig.Config != null)
            {
                UpdateColors();
            }
            CellexalEvents.ConfigLoaded.AddListener(() => RequireToggleToClick = CellexalConfig.Config.RequireTouchpadClickToInteract);

            CellexalEvents.ConfigLoaded.AddListener(UpdateColors);
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
            CellexalEvents.RightTriggerPressed.AddListener(OnTriggerDown);
            CellexalEvents.RightTriggerUp.AddListener(OnTriggerUp);
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            multiuserMessageSender = ReferenceManager.instance.multiuserMessageSender;
            selectionManager = ReferenceManager.instance.selectionManager;
            graphManager = ReferenceManager.instance.graphManager;
            UpdateShapeIcons();
            CellexalEvents.ConfigLoaded.AddListener(() => RequireToggleToClick = CellexalConfig.Config.RequireTouchpadClickToInteract);

            CurrentColorIndex = 0;
        }

        private void OnTriggerClick()
        {
            if (controllerModelSwitcher == null)
                controllerModelSwitcher = ReferenceManager.instance.controllerModelSwitcher;
            if (controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.SelectionTool)
            {
                vfx.gameObject.SetActive(true);
                //particles.gameObject.SetActive(true);
                selectionToolMaterial.SetFloat("_SelectionActive", 1);
                selActive = true;
            }

        }

        private void OnTriggerDown()
        {
            if (PointCloudGenerator.instance.pointClouds.Count > 0) return;
            if (ReferenceManager.instance.controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.SelectionTool)
            {
                hapticFeedbackThisFrame = true;
                var activeCollider = selectionToolColliders[CurrentMeshIndex];
                Vector3 boundsCenter = activeCollider.bounds.center;
                Vector3 boundsExtents = activeCollider.bounds.extents;
                foreach (var graph in graphManager.Graphs)
                {
                    var closestPoints = graph.MinkowskiDetection(activeCollider.transform.position, boundsCenter,
                        boundsExtents, currentColorIndex);
                    foreach (var point in closestPoints)
                    {
                        if (CurrentMeshIndex > 3)
                        {
                            selectionManager.RemoveGraphpointFromSelection(point);
                        }

                        else
                        {
                            selectionManager.AddGraphpointToSelection(point, currentColorIndex, true);
                        }
                    }
                }

            }


        }

        private void OnTriggerUp()
        {
            if (ReferenceManager.instance.controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.SelectionTool)
            {
                selActive = false;
                vfx.gameObject.SetActive(false);
                //particles.gameObject.SetActive(false);
                selectionToolMaterial.SetFloat("_SelectionActive", 0);
            }
        }

        private void OnTouchPadClick(InputAction.CallbackContext context)
        {
            if (ReferenceManager.instance.controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.SelectionTool)
            {
                Vector2 pos = touchPadPos.action.ReadValue<Vector2>();
                float absX = System.Math.Abs(pos.x);
                float absY = System.Math.Abs(pos.y);
                if (absX > absY)
                {
                    if (pos.x > 0)
                    {
                        // right quadrant
                        ChangeColor(true);
                    }
                    else
                    {
                        // left quadrant
                        ChangeColor(false);
                    }
                }
                else
                {
                    if (pos.y > 0)
                    {
                        // upper quadrant
                        controllerModelSwitcher.SwitchSelectionToolMesh(true);
                    }
                    else
                    {
                        // lower quadrant
                        controllerModelSwitcher.SwitchSelectionToolMesh(false);
                    }

                }
            }
        }


        private void Update()
        {
            if (ReferenceManager.instance.controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.SelectionTool)
            {


            }

            // Sometimes a bug occurs where particles stays active even when selection tool is off. This ensures particles is off 
            // if selection tool is inactive.
            //if (particles && !IsSelectionToolEnabled() && particles.gameObject.activeSelf)
            //{
            //    particles.gameObject.SetActive(false);
            //}
        }


        /// <summary>
        /// Updates <see cref="Colors"/> to <see cref="CellexalConfig.Config.SelectionToolColors"/>.
        /// </summary>
        public void UpdateColors()
        {
            currentColorIndex = 0;
            Colors = CellexalConfig.Config.SelectionToolColors;
            for (int i = 0; i < Colors.Length; i++)
            {
                Colors[i].a = 1;
            }

            if (!CrossSceneInformation.Ghost)
            {
                // radialMenu = referenceManager.rightControllerScriptAlias.GetComponentInChildren<VRTK_RadialMenu>();
                try
                {
                    if (Colors.Length > 1)
                    {
                        //radialMenu.RegenerateButtons();
                        // radialMenu.menuButtons[1].GetComponentInChildren<Image>().color = Colors[Colors.Length - 1];
                        // radialMenu.menuButtons[3].GetComponentInChildren<Image>().color = Colors[1];
                    }
                    else if (Colors.Length > 0)
                    {
                        // radialMenu.menuButtons[1].GetComponentInChildren<Image>().color = Colors[0];
                        // radialMenu.menuButtons[3].GetComponentInChildren<Image>().color = Colors[0];
                    }
                }
                catch (NullReferenceException)
                {
                    CellexalLog.Log(
                        "Could not recreate buttons on controller. Could be that controllers were inactive at the time");
                    return;
                }

            }
        }

        /// <summary>
        /// Changes the color of the selection tool.
        /// </summary>
        /// <param name="dir"> The direction to move in the array of colors. true for increment, false for decrement </param>
        public void ChangeColor(bool dir)
        {
            if (CurrentColorIndex >= Colors.Length) return;
            if (CurrentColorIndex == Colors.Length - 1 && dir)
            {
                CurrentColorIndex = 0;
            }
            else if (CurrentColorIndex == 0 && !dir)
            {
                CurrentColorIndex = Colors.Length - 1;
            }
            else if (dir)
            {
                CurrentColorIndex++;
            }
            else
            {
                CurrentColorIndex--;
            }
            //int buttonIndexLeft = currentColorIndex == 0 ? Colors.Length - 1 : currentColorIndex - 1;
            //int buttonIndexRight = currentColorIndex == Colors.Length - 1 ? 0 : currentColorIndex + 1;
            // VRTK 3.3
            //radialMenu.RegenerateButtons();
            // OpenXR Replace VRTK radial menu with something else.
            //radialMenu.menuButtons[1].GetComponentInChildren<Image>().color = Colors[buttonIndexLeft];
            //radialMenu.menuButtons[3].GetComponentInChildren<Image>().color = Colors[buttonIndexRight];
            //radialMenu.buttons[3].color = Colors[buttonIndexRight];
            selectedColor = Colors[currentColorIndex];
            //selectionToolColliders[currentMeshIndex].GetComponent<Renderer>().material.color = Colors[currentColorIndex];
            ReferenceManager.instance.controllerModelSwitcher.SwitchControllerModelColor(Colors[currentColorIndex]);

        }

        /// <summary>
        /// Activates or deactivates the selection tool.
        /// </summary>
        /// <param name="enabled"> True if the selection tool should be activated, false if it should be deactivated. </param>
        public void SetSelectionToolEnabled(bool enabled)
        {
            gameObject.SetActive(enabled);
            if (enabled)
            {
                // controllerModelSwitcher.SwitchControllerModelColor(Colors[currentColorIndex]);
                if (CurrentColorIndex > Colors.Length)
                {
                    ReferenceManager.instance.controllerModelSwitcher.SwitchControllerModelColor(Color.white);
                }
                else
                {
                    ReferenceManager.instance.controllerModelSwitcher.SwitchControllerModelColor(Colors[CurrentColorIndex]);
                }
            }
            //particles.gameObject.SetActive(enabled && particles != null);

            for (int i = 0; i < selectionToolColliders.Length; ++i)
            {
                // if we are turning on the selection tool, enable the gameobject with the right index and disable the other ones
                selectionToolColliders[i].gameObject.SetActive(enabled && CurrentMeshIndex == i);
            }
        }

        void ActivateSelection(bool sel)
        {
            selActive = sel;
            SetSelectionToolEnabled(true);
        }

        public bool IsSelectionToolEnabled()
        {
            return GetComponentsInChildren<Collider>().Any(x => x.enabled);
        }

        public Color GetCurrentColor()
        {
            if (currentColorIndex > Colors.Length) return Color.white;
            return Colors[currentColorIndex];
        }

        public Collider GetCurrentCollider()
        {
            return selectionToolColliders[currentMeshIndex];
        }

        private void UpdateShapeIcons()
        {
            // OpenXR Replace VRTK radial menu with something else.
            //if (radialMenu)
            //{
            //    //radialMenu.RegenerateButtons();
            //    //print(radialMenu.menuButtons[0] + " " + radialMenu.menuButtons[0].GetComponentInChildren<Image>());
            //    if (radialMenu.menuButtons[0] && radialMenu.menuButtons[0].GetComponentInChildren<Image>())
            //    {
            //        int buttonIndexUp = currentMeshIndex == selectionToolColliders.Length - 1 ? 0 : currentMeshIndex + 1;
            //        int buttonIndexDown = currentMeshIndex == 0 ? selectionToolColliders.Length - 1 : currentMeshIndex - 1;
            //        radialMenu.menuButtons[0].GetComponentInChildren<Image>().sprite = selectionToolShapeButtons[buttonIndexUp];
            //        radialMenu.menuButtons[2].GetComponentInChildren<Image>().sprite = selectionToolShapeButtons[buttonIndexDown];
            //    }
            //}
        }

        public int GetColorIndex(string colorString)
        {
            Color[] colorArray; // = new Color[CellexalConfig.Config.SelectionToolColors.Length];
            // if using spectator mode or for other reasons the selectionToolCollider hasnt been activated
            // return the index from the config.
            colorArray = Colors.Length == 0
                ? CellexalConfig.Config.SelectionToolColors
                : referenceManager.selectionToolCollider.Colors;

            for (int i = 0; i < colorArray.Length; i++)
            {
                Color selectionColor = colorArray[i];
                string selectionColorString = "#" + ColorUtility.ToHtmlStringRGB(selectionColor);
                if (selectionColorString.Equals(colorString)) return i;
            }

            return -1;
        }

        public int GetColorIndex(Color color)
        {
            Color[] colorArray; // = new Color[CellexalConfig.Config.SelectionToolColors.Length];
            // if using spectator mode or for other reasons the selectionToolCollider hasnt been activated
            // return the index from the config.
            colorArray = CellexalConfig.Config.SelectionToolColors;

            for (int i = 0; i < colorArray.Length; i++)
            {
                Color selectionColor = colorArray[i];
                if (InputReader.CompareColor(selectionColor, color)) return i;
            }

            return -1;
        }
    }
}