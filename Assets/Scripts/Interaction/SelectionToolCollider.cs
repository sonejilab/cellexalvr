using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using VRTK;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// This class sole purpose is to forward collision events to the selection tool handler
    /// </summary>
    public class SelectionToolCollider : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public SelectionManager selectionManager;
        public RadialMenu radialMenu;
        public Sprite buttonIcons;
        public ParticleSystem particles;
        public Collider[] selectionToolColliders;
        public Color[] Colors;
        public int currentColorIndex = 0;
        public bool hapticFeedbackThisFrame = true;

        private SelectionFromPreviousMenu previousSelectionMenu;
        private ControllerModelSwitcher controllerModelSwitcher;
        private GraphManager graphManager;
        private SteamVR_TrackedObject rightController;
        private SteamVR_Controller.Device device;
        private GameManager gameManager;
        private Stopwatch minkowskiTimeoutStopwatch = new Stopwatch();
        private bool selActive = false;
        private int currentMeshIndex;
        private Color selectedColor;


        void Awake()
        {
            previousSelectionMenu = referenceManager.selectionFromPreviousMenu;
            graphManager = referenceManager.graphManager;
            SetSelectionToolEnabled(false, 0);

            UpdateColors();
            CellexalEvents.ConfigLoaded.AddListener(UpdateColors);
        }

        private void Start()
        {
            controllerModelSwitcher = referenceManager.controllerModelSwitcher;
            rightController = referenceManager.rightController;
            gameManager = referenceManager.gameManager;
            selectionManager = referenceManager.selectionManager;
        }


        private void Update()
        {
            device = SteamVR_Controller.Input((int)rightController.index);
            // Only activate selection if trigger is pressed.
            //device = SteamVR_Controller.Input((int)rightController.index);
            // more_cells device = SteamVR_Controller.Input((int)rightController.index);
            //device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.SelectionTool)
            {
                if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    particles.gameObject.SetActive(true);
                    ActivateSelection(true);
                }
                if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger))
                {
                    hapticFeedbackThisFrame = true;
                    Vector3 boundsCenter = selectionToolColliders[currentMeshIndex].bounds.center;
                    Vector3 boundsExtents = selectionToolColliders[currentMeshIndex].bounds.extents;
                    minkowskiTimeoutStopwatch.Stop();
                    minkowskiTimeoutStopwatch.Start();
                    float millisecond = Time.realtimeSinceStartup;
                    foreach (var graph in graphManager.Graphs)
                    {
                        var closestPoints = graph.MinkowskiDetection(transform.position, boundsCenter, boundsExtents, currentColorIndex, millisecond);
                        foreach (var point in closestPoints)
                        {
                            selectionManager.AddGraphpointToSelection(point, currentColorIndex, true);

                        }
                    }
                }

                else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
                {
                    ActivateSelection(false);
                }
            }
            // Sometimes the a bug occurs where particles stays active even when selection tool is off. This ensures particles is off 
            // if selection tool is inactive.
            if (particles && !IsSelectionToolEnabled() && particles.gameObject.activeSelf)
            {
                particles.gameObject.SetActive(false);
            }

        }

        //void OnTriggerEnter(Collider other)
        //{

        //    var cubeOnLine = other.gameObject.GetComponent<Selectable>();
        //    if (cubeOnLine != null && !cubeOnLine.selected)
        //    {
        //        cubeOnLine.selected = true;
        //        selectionManager.AddGraphpointToSelection(cubeOnLine.graphPoint);
        //        foreach (Selectable sel in cubeOnLine.graphPoint.lineBetweenCellsCubes)
        //        {
        //            sel.GetComponent<Renderer>().material.color = Colors[currentColorIndex];
        //        }
        //        referenceManager.gameManager.InformCubeColoured(cubeOnLine.graphPoint.parent.name, cubeOnLine.graphPoint.Label,
        //                                                        currentColorIndex, Colors[currentColorIndex]);
        //    }
        //}


        /// <summary>
        /// Updates <see cref="Colors"/> to <see cref="CellexalConfig.Config.SelectionToolColors"/>.
        /// </summary>
        public void UpdateColors()
        {
            currentColorIndex = 0;
            radialMenu.buttons[1].ButtonIcon = buttonIcons;
            radialMenu.buttons[3].ButtonIcon = buttonIcons;
            Colors = CellexalConfig.Config.SelectionToolColors;
            for (int i = 0; i < Colors.Length; i++)
            {
                Colors[i].a = 1;
            }
            radialMenu.buttons[1].color = Colors[Colors.Length - 1];
            radialMenu.buttons[3].color = Colors[1];
            radialMenu.RegenerateButtons();
            selectedColor = Colors[currentColorIndex];
        }

        /// <summary>
        /// Changes the color of the selection tool.
        /// </summary>
        /// <param name="dir"> The direction to move in the array of colors. true for increment, false for decrement </param>
        public void ChangeColor(bool dir)
        {
            if (currentColorIndex == Colors.Length - 1 && dir)
            {
                currentColorIndex = 0;
            }
            else if (currentColorIndex == 0 && !dir)
            {
                currentColorIndex = Colors.Length - 1;
            }
            else if (dir)
            {
                currentColorIndex++;
            }
            else
            {
                currentColorIndex--;
            }
            int buttonIndexLeft = currentColorIndex == 0 ? Colors.Length - 1 : currentColorIndex - 1;
            int buttonIndexRight = currentColorIndex == Colors.Length - 1 ? 0 : currentColorIndex + 1;
            radialMenu.buttons[1].color = Colors[buttonIndexLeft];
            radialMenu.buttons[3].color = Colors[buttonIndexRight];
            radialMenu.RegenerateButtons();
            selectedColor = Colors[currentColorIndex];
            controllerModelSwitcher.SwitchControllerModelColor(Colors[currentColorIndex]);
        }

        /// <summary>
        /// Activates or deactivates all colliders on the selectiontool.
        /// </summary>
        /// <param name="enabled"> True if the selection tool should be activated, false if it should be deactivated. </param>
        /// <param name="meshIndex">The index of the collider that should be activated, if <paramref name="enabled"/> is <code>true</code>.</param>
        public void SetSelectionToolEnabled(bool enabled, int meshIndex)
        {
            currentMeshIndex = meshIndex;
            if (enabled)
            {
                controllerModelSwitcher.SwitchControllerModelColor(Colors[currentColorIndex]);
            }
            if (!enabled && particles != null)
            {
                particles.gameObject.SetActive(false);
            }
            if (selActive)
            {
                particles.gameObject.SetActive(true);
                var main = particles.main;
                main.startColor = Colors[currentColorIndex];
            }
            for (int i = 0; i < selectionToolColliders.Length; ++i)
            {
                // if we are turning on the selection tool, enable the collider with the corresponding index as the mesh and disable the other colliders.
                selectionToolColliders[i].enabled = enabled && selActive && meshIndex == i;
            }

        }

        void ActivateSelection(bool sel)
        {
            selActive = sel;
            SetSelectionToolEnabled(true, currentMeshIndex);
        }

        public bool IsSelectionToolEnabled()
        {
            return GetComponentsInChildren<Collider>().Any(x => x.enabled);
        }

    }
}