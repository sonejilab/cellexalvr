using UnityEngine;
using VRTK;
using CellexalVR.General;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Responsible for changing the controller model and the activated tool.
    /// </summary>
    public class ControllerModelSwitcher : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        public SteamVR_RenderModel renderModel;
        public GameObject rightControllerBody;
        public GameObject leftControllerBody;
        public Mesh normalControllerMesh;
        //public Texture normalControllerTexture;
        public Mesh menuControllerMesh;
        //public Texture menuControllerTexture;
        public Mesh[] selectionToolMeshes;
        public Mesh minimizeMesh;
        public Material normalMaterial;
        public Material selectionToolHandlerMaterial;
        public Material leftControllerMaterial;

        public enum Model { Normal, SelectionTool, Menu, Minimizer, DeleteTool, HelpTool, Keyboard, TwoLasers, DrawTool, WebBrowser };
        // what model we actually want
        public Model DesiredModel { get; set; }
        // what model is actually displayed, useful for when we want to change the model temporarily
        // for example: the user has activated the selection tool, so DesiredModel = SelectionTool and actualModel = SelectionTool
        // the user then moves the controller into the menu. DesiredModel is still SelectionTool, but actualModel will now be Menu
        public Model ActualModel;

        private SelectionToolHandler selectionToolHandler;
        private GameObject deleteTool;
        private GameObject minimizer;
        private GameObject drawTool;
        private KeyboardSwitch keyboard;
        private GameObject webBrowser;
        private MeshFilter rightControllerBodyMeshFilter;
        private Renderer rightControllerBodyRenderer;
        private MeshFilter leftControllerBodyMeshFilter;
        private Renderer leftControllerBodyRenderer;
        private Color desiredColor;
        private LaserPointerController rightLaser;
        private VRTK_StraightPointerRenderer leftLaser;
        private int selectionToolMeshIndex = 0;
        // The help tool is a bit of an exception, it can be active while another tool is also active, like the keyboard.
        // Otherwise you can't point the helptool towards the keyboard.
        public bool HelpToolShouldStayActivated { get; set; }

        void Awake()
        {
            selectionToolHandler = referenceManager.selectionToolHandler;
            deleteTool = referenceManager.deleteTool;
            minimizer = referenceManager.minimizeTool.gameObject;
            keyboard = referenceManager.keyboardSwitch;
            drawTool = referenceManager.drawTool.gameObject;
            rightLaser = referenceManager.rightLaser;
            leftLaser = referenceManager.leftLaser;
            webBrowser = referenceManager.webBrowser;
            DesiredModel = Model.Normal;
            //if (rightControllerBody.activeSelf)
            //{
            //SetMeshes();
            //}
            //else
            //{
            //SteamVR_Events.RenderModelLoaded.Listen(OnControllerLoaded);
            //}
        }

        // Used when starting the program to know when steamvr has loaded the model and applied a meshfilter and meshrenderer for us to use.
        void OnControllerLoaded(SteamVR_RenderModel renderModel, bool success)
        {
            if (!success) return;
            SetMeshes();
        }

        public void SetMeshes()
        {
            rightControllerBodyMeshFilter = rightControllerBody.GetComponent<MeshFilter>();
            rightControllerBodyMeshFilter.mesh = normalControllerMesh;
            rightControllerBodyRenderer = rightControllerBody.GetComponent<Renderer>();
            rightControllerBodyRenderer.material = normalMaterial;
            leftControllerBodyMeshFilter = leftControllerBody.GetComponent<MeshFilter>();
            leftControllerBodyMeshFilter.mesh = normalControllerMesh;
            leftControllerBodyRenderer = leftControllerBody.GetComponent<Renderer>();
            leftControllerBodyRenderer.material = normalMaterial;
            //var leftBody = leftControllerBody.GetComponent<Renderer>();
            //leftBody.material = leftControllerMaterial;

        }

        // Used when starting the program.
        // It takes some time for steamvr and vrtk to set everything up, and for some time
        // these variables will be null because the components are not yet added to the gameobjects.
        internal bool Ready()
        {
            return rightControllerBody.GetComponent<MeshFilter>() != null && rightControllerBody.GetComponent<Renderer>() != null && leftControllerBody.GetComponent<Renderer>() != null;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Equals("Menu Selecter Collider"))
            {
                if (rightControllerBodyMeshFilter == null) return;
                //SwitchToModel(Model.Menu);
                deleteTool.SetActive(false);
                minimizer.SetActive(false);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.Equals("Menu Selecter Collider"))
            {
                if (rightControllerBodyMeshFilter == null) return;
                ActivateDesiredTool();
            }
        }

        /// <summary>
        /// Switches the right controller's model.
        /// </summary>
        public void SwitchToModel(Model model)
        {
            ActualModel = model;
            switch (model)
            {
                case Model.Normal:
                case Model.HelpTool:
                case Model.DrawTool:
                case Model.DeleteTool:
                    rightControllerBodyMeshFilter.mesh = normalControllerMesh;
                    rightControllerBodyRenderer.material = normalMaterial;
                    break;

                case Model.Keyboard:
                    //keyboard.SetKeyboardVisible(true);
                    rightLaser.GetComponent<VRTK_StraightPointerRenderer>().enabled = true;
                    rightLaser.transform.localRotation = Quaternion.identity;
                    break;

                case Model.WebBrowser:
                    webBrowser.SetActive(true);
                    rightLaser.GetComponent<VRTK_StraightPointerRenderer>().enabled = true;
                    rightLaser.transform.localRotation = Quaternion.identity;
                    break;

                case Model.TwoLasers:
                    rightLaser.GetComponent<VRTK_StraightPointerRenderer>().enabled = true;
                    rightLaser.transform.localRotation = Quaternion.identity;
                    break;

                case Model.Menu:
                    //print("switched to menu");
                    drawTool.SetActive(false);
                    rightLaser.GetComponent<VRTK_StraightPointerRenderer>().enabled = true;
                    rightControllerBodyMeshFilter.mesh = normalControllerMesh;
                    rightControllerBodyRenderer.material = normalMaterial;
                    break;

                case Model.SelectionTool:
                    rightControllerBodyMeshFilter.mesh = selectionToolMeshes[selectionToolMeshIndex];
                    rightControllerBodyRenderer.material = selectionToolHandlerMaterial;
                    rightControllerBodyRenderer.material.color = desiredColor;
                    break;

                case Model.Minimizer:
                    rightControllerBodyMeshFilter.mesh = minimizeMesh;
                    break;

            }
        }

        /// <summary>
        /// Activates the current tool and changes the controller's model to that tool and deactivates previously active tool.
        /// </summary>
        public void ActivateDesiredTool()
        {
            // These models can have the help tool activated, so if we are not switching to one of them, the help tool should go away
            if (DesiredModel != Model.HelpTool && DesiredModel != Model.Keyboard)
            {
                HelpToolShouldStayActivated = false;
            }
            // Deactivate all tools that should not be active.
            if (DesiredModel != Model.SelectionTool)
            {
                selectionToolHandler.SetSelectionToolEnabled(false, 0);
            }
            if (DesiredModel != Model.DeleteTool)
            {
                deleteTool.SetActive(false);
            }
            if (DesiredModel != Model.Minimizer)
            {
                minimizer.SetActive(false);
            }
            if (DesiredModel != Model.HelpTool && !HelpToolShouldStayActivated)
            {
                //helpTool.SetToolActivated(false);
            }
            // if we are switching from the keyboard to the help tool, the keyboard should stay activated.
            if (DesiredModel != Model.Keyboard && DesiredModel != Model.HelpTool)
            {
                //rightLaser.ToggleLaser(false);
                //leftLaser.enabled = false;
                keyboard.SetKeyboardVisible(false);
                //rightLaser.enabled = false;
                //referenceManager.gameManager.InformActivateKeyboard(false);
            }
            if (DesiredModel != Model.TwoLasers)
            {
                rightLaser.ToggleLaser(false);
                leftLaser.enabled = false;
            }
            if (DesiredModel != Model.DrawTool)
            {
                drawTool.SetActive(false);
            }
            switch (DesiredModel)
            {
                case Model.SelectionTool:
                    selectionToolHandler.SetSelectionToolEnabled(true, selectionToolMeshIndex);
                    break;
                case Model.DeleteTool:
                    deleteTool.SetActive(true);
                    break;
                case Model.Minimizer:
                    minimizer.SetActive(true);
                    break;
                //case Model.HelpTool:
                //    helpTool.SetToolActivated(true);
                //    rightLaser.enabled = true;
                //    break;
                case Model.Keyboard:
                    keyboard.SetKeyboardVisible(true);
                    //rightLaser.enabled = true;
                    //rightLaser.ToggleLaser(true);
                    break;
                case Model.WebBrowser:
                    webBrowser.SetActive(true);
                    rightLaser.ToggleLaser(true);
                    break;
                case Model.TwoLasers:
                    rightLaser.ToggleLaser(true);
                    leftLaser.enabled = true;
                    break;
                case Model.DrawTool:
                    drawTool.SetActive(true);
                    break;
            }
            SwitchToDesiredModel();
            CellexalEvents.ModelChanged.Invoke();
        }

        /// <summary>
        /// Turns off the active tool and sets our desired model to the normal model.
        /// </summary>
        /// <param name="inMenu"> True if the controller is in the menu and we should temporarily change into the menu model, false otherwise. </param>
        public void TurnOffActiveTool(bool inMenu)
        {
            selectionToolHandler.SetSelectionToolEnabled(false, 0);
            deleteTool.SetActive(false);
            minimizer.SetActive(false);
            //if (!HelpToolShouldStayActivated)
            //{
            //helpTool.SetToolActivated(false);
            DesiredModel = Model.Normal;
            //}
            //else
            //{
            //    DesiredModel = Model.HelpTool;
            //}
            rightLaser.ToggleLaser(false);
            leftLaser.enabled = false;
            keyboard.SetKeyboardVisible(false);
            //referenceManager.gameManager.InformActivateKeyboard(false);
            drawTool.SetActive(false);
            webBrowser.SetActive(false);
            if (inMenu)
            {
                SwitchToModel(Model.Menu);
            }
            else
            {
                SwitchToModel(Model.Normal);
            }
        }

        /// <summary>
        /// Switches to the desired model. Does not activate or deactivate any tool.
        /// </summary>
        public void SwitchToDesiredModel()
        {
            SwitchToModel(DesiredModel);
        }

        /// <summary>
        /// Used by the selectiontoolhandler. Changes the current model's color.
        /// </summary>
        /// <param name="color"> The new color. </param>
        public void SwitchControllerModelColor(Color color)
        {
            desiredColor = color;

            if (ActualModel == Model.SelectionTool)
            {
                rightControllerBodyRenderer.material.color = desiredColor;
            }
        }

        public void SwitchSelectionToolMesh(bool dir)
        {
            if (ActualModel == Model.SelectionTool)
            {
                if (dir)
                    selectionToolMeshIndex++;
                else
                    selectionToolMeshIndex--;

                if (selectionToolMeshIndex == -1)
                    selectionToolMeshIndex = selectionToolMeshes.Length - 1;
                else if (selectionToolMeshIndex == selectionToolMeshes.Length)
                    selectionToolMeshIndex = 0;
                ActivateDesiredTool();
            }
        }
    }
}