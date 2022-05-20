using Assets.Scripts.General;
using CellexalVR.General;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Responsible for changing the controller model and the activated tool.
    /// </summary>
    public class ControllerModelSwitcher : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        // supported controllers
        public ActionBasedController leftControllerScript;
        public ActionBasedController rightControllerScript;
        public GameObject viveControllerPrefab;
        public GameObject valveIndexControllerPrefab;
        public GameObject hpReverbControllerPrefab;
        public enum ControllerBrand { Set_Automatically, HTC_Vive, Valve_Index, HP_Reverb };
        public ControllerBrand BaseModel { get; set; }
        public enum Model { Normal, SelectionTool, Menu, Minimizer, DeleteTool, Keyboard, TwoLasers, DrawTool, WebBrowser };
        // what model we actually want
        public Model DesiredModel { get; set; }
        // what model is actually displayed, useful for when we want to change the model temporarily
        // for example: the user has activated the selection tool, so DesiredModel = SelectionTool and actualModel = SelectionTool
        // the user then moves the controller into the menu. DesiredModel is still SelectionTool, but actualModel will now be Menu
        public Model ActualModel;
        public enum ControllerColorMode { StaticColors, AnimatedPulse }
        public ControllerColorMode colorMode;
        public Material defaultStaticMaterial;
        public Material animatedPulseMaterial;

        //private SelectionToolHandler selectionToolHandler;
        [SerializeField] private GameObject toolsParent;
        private SelectionToolCollider selectionToolCollider;
        private GameObject deleteTool;
        private GameObject minimizeTool;
        private GameObject drawTool;
        private KeyboardSwitch keyboard;
        private GameObject webBrowser;
        private MeshFilter rightControllerBodyMeshFilter;
        private LaserPointerController laserPointerController;
        private XRRayInteractor rightLaser;
        private XRRayInteractor leftLaser;
        private Dictionary<Color, Material> staticMaterialsCache;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Awake()
        {
            //selectionToolHandler = referenceManager.selectionToolHandler;
            selectionToolCollider = referenceManager.selectionToolCollider;
            drawTool = referenceManager.drawTool.gameObject;
            keyboard = referenceManager.keyboardSwitch;
            webBrowser = referenceManager.webBrowser;
            deleteTool = referenceManager.deleteTool;
            minimizeTool = referenceManager.minimizeTool.gameObject;
            DesiredModel = Model.Normal;
            laserPointerController = referenceManager.laserPointerController;
            rightLaser = referenceManager.rightLaser;
            leftLaser = referenceManager.leftLaser;
            staticMaterialsCache = new Dictionary<Color, Material>();
            staticMaterialsCache[animatedPulseMaterial.color] = animatedPulseMaterial;

            InputDevices.deviceConnected += OnDeviceConnected;
            InputDevices.deviceDisconnected += OnDeviceDisconnected;
        }

        /// <summary>
        /// Switches the controller's models.
        /// </summary>
        /// <param name="brand">The new model to apply.</param>
        /// <param name="left">True if the left controller's model should be switched.</param>
        /// <param name="right">True if the right controller's model should be switched.</param>
        public void SwitchControllerBaseModel(ControllerBrand brand, bool left = true, bool right = true)
        {
            GameObject baseModelToSwitchTo;
            switch (brand)
            {
                case ControllerBrand.HTC_Vive:
                    baseModelToSwitchTo = viveControllerPrefab;
                    break;

                case ControllerBrand.Valve_Index:
                    baseModelToSwitchTo = valveIndexControllerPrefab;
                    break;

                case ControllerBrand.HP_Reverb:
                    baseModelToSwitchTo = hpReverbControllerPrefab;
                    break;

                case ControllerBrand.Set_Automatically:
                default:
                    var allDevices = new List<UnityEngine.XR.InputDevice>();
                    InputDevices.GetDevices(allDevices);
                    var firstController = allDevices.Find((device) => (device.characteristics & InputDeviceCharacteristics.Controller) != 0);
                    baseModelToSwitchTo = TryGuessControllerModel(firstController);
                    break;
            }
            SwitchControllerBaseModel(baseModelToSwitchTo, left, right);
        }

        /// <summary>
        /// Switches the controllers' models.
        /// </summary>
        /// <param name="newModel">The new model to apply.</param>
        /// <param name="left">True if the left controller's model should be switched.</param>
        /// <param name="right">True if the right controller's model should be switched.</param>
        public void SwitchControllerBaseModel(GameObject newModel, bool left = true, bool right = true)
        {
            if (left)
            {
                GameObject leftModel = newModel.transform.Find("Left").gameObject;
                leftControllerScript.enabled = false;
                if (leftControllerScript.model)
                {
                    Destroy(leftControllerScript.model.gameObject);
                }

                leftControllerScript.modelPrefab = leftModel.transform;
                Transform model = Instantiate(leftControllerScript.modelPrefab, leftControllerScript.modelParent);
                leftControllerScript.model = model;
                leftControllerScript.enabled = true;
                BoxCollider collider = leftControllerScript.GetComponent<BoxCollider>();
                collider.center = model.InverseTransformPoint(model.transform.position) - model.InverseTransformDirection(model.transform.forward) * 0.1f;
            }

            if (right)
            {
                GameObject rightModel = newModel.transform.Find("Right").gameObject;
                rightControllerScript.enabled = false;
                if (rightControllerScript.model)
                {
                    Destroy(rightControllerScript.model.gameObject);
                }

                rightControllerScript.modelPrefab = rightModel.transform;
                Transform model = Instantiate(rightControllerScript.modelPrefab, rightControllerScript.modelParent);
                rightControllerScript.model = model;
                rightControllerScript.enabled = true;
                BoxCollider collider = rightControllerScript.GetComponent<BoxCollider>();
                collider.center = model.InverseTransformPoint(model.transform.position) - model.InverseTransformDirection(model.transform.forward) * 0.1f;
            }

            StartCoroutine(WaitOneFrameAndRun(() => SwitchControllerModelColor(colorMode)));
        }

        private System.Collections.IEnumerator WaitOneFrameAndRun(Action func)
        {
            yield return null;
            func();
        }

        /// <summary>
        /// Attempts the guess what controller is currently connected through OpenXR. Returns the appopriate gameobject prefab to apply to the device.
        /// </summary>
        /// <param name="device">The device to </param>
        /// <returns>The gameobject to use as a model for this controller.</returns>
        private GameObject TryGuessControllerModel(UnityEngine.XR.InputDevice device)
        {
            switch (device.name)
            {
                case "HTC Vive Controller OpenXR":
                    return viveControllerPrefab;
                case "Index Controller OpenXR":
                    return valveIndexControllerPrefab;
                case "Oculus Touch Controller OpenXR":
                    return hpReverbControllerPrefab;
                default:
                    // couldn't guess the model, default to the vive controller model
                    return viveControllerPrefab;
            }
        }

        /// <summary>
        /// Called when a new device connects, sets the controller model according to what the user has defined in the settings menu, if it is a controller.
        /// </summary>
        /// <param name="device">The device that connected.</param>
        private void OnDeviceConnected(UnityEngine.XR.InputDevice device)
        {
            CellexalLog.Log($"Device connected. Name: {device.name}, role: {device.characteristics}, manufacturer: {device.manufacturer}");

            if ((device.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                ControllerBrand desiredBrand = CellexalConfig.Config.ControllerModel.ToBrand();

                if ((device.characteristics & InputDeviceCharacteristics.Left) != 0)
                {
                    SwitchControllerBaseModel(desiredBrand, true, false);
                }
                else if ((device.characteristics & InputDeviceCharacteristics.Right) != 0)
                {
                    SwitchControllerBaseModel(desiredBrand, false, true);
                }
            }
        }

        /// <summary>
        /// Called when a device disconnects.
        /// </summary>
        /// <param name="device">The device that disconnected.</param>
        private void OnDeviceDisconnected(UnityEngine.XR.InputDevice device)
        {
            CellexalLog.Log($"Device disconnected. Name: {device.name}, role: {device.characteristics}, manufacturer: {device.manufacturer}");
        }

        /// <summary>
        /// Used when starting the program.
        /// It takes some time for steamvr and vrtk to set everything up, and for some time
        /// these variables will be null because the components are not yet added to the gameobjects.
        /// </summary>
        /// <returns></returns>
        internal bool Ready()
        {
            // Open XR
            return true;
            if (CrossSceneInformation.Spectator)
                return true;
            //return rightControllerBody.GetComponent<MeshFilter>() != null && rightControllerBody.GetComponent<Renderer>() != null
            //     && leftControllerBody.GetComponent<MeshFilter>() != null && leftControllerBody.GetComponent<Renderer>() != null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Equals("Menu Selecter Collider"))
            {
                if (rightControllerBodyMeshFilter == null) return;
                //SwitchToModel(Model.Menu);
                deleteTool.SetActive(false);
                minimizeTool.SetActive(false);
            }
        }

        private void OnTriggerExit(Collider other)
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
                case Model.DrawTool:
                case Model.DeleteTool:
                    break;

                case Model.Keyboard:
                    laserPointerController.origin.localRotation = Quaternion.identity;
                    break;

                case Model.WebBrowser:
                    webBrowser.GetComponent<WebManager>().SetBrowserActive(true);
                    laserPointerController.ToggleLaser(true);
                    laserPointerController.origin.localRotation = Quaternion.identity;
                    break;

                case Model.TwoLasers:
                    laserPointerController.ToggleLaser(true);
                    laserPointerController.origin.localRotation = Quaternion.identity;
                    break;

                case Model.Menu:
                    drawTool.SetActive(false);
                    deleteTool.SetActive(false);
                    minimizeTool.SetActive(false);
                    minimizeTool.SetActive(false);
                    selectionToolCollider.SetSelectionToolEnabled(false);
                    laserPointerController.ToggleLaser(true);
                    break;

                case Model.SelectionTool:
                    break;

                case Model.Minimizer:
                    minimizeTool.SetActive(true);
                    break;

            }
        }

        /// <summary>
        /// Activates the current tool and changes the controller's model to that tool and deactivates previously active tool.
        /// </summary>
        public void ActivateDesiredTool()
        {
            // Deactivate all tools that should not be active.
            if (DesiredModel != Model.SelectionTool && selectionToolCollider != null)
            {
                selectionToolCollider.SetSelectionToolEnabled(false);
            }
            if (DesiredModel != Model.DeleteTool && deleteTool != null)
            {
                deleteTool.SetActive(false);
            }
            if (DesiredModel != Model.Minimizer && minimizeTool != null)
            {
                minimizeTool.SetActive(false);
            }
            if (DesiredModel != Model.TwoLasers)
            {
                laserPointerController.ToggleLaser(false);
                leftLaser.enabled = false;
            }
            if (DesiredModel != Model.DrawTool)
            {
                drawTool.SetActive(false);
            }
            switch (DesiredModel)
            {
                case Model.SelectionTool:
                    selectionToolCollider.SetSelectionToolEnabled(true);
                    break;
                case Model.DeleteTool:
                    deleteTool.SetActive(true);
                    break;
                case Model.Minimizer:
                    minimizeTool.SetActive(true);
                    break;
                case Model.Keyboard:
                    keyboard.SetKeyboardVisible(true);
                    laserPointerController.ToggleLaser(true);
                    break;
                case Model.WebBrowser:
                    webBrowser.GetComponent<WebManager>().SetBrowserActive(true);
                    laserPointerController.ToggleLaser(true);
                    break;
                case Model.TwoLasers:
                    laserPointerController.ToggleLaser(true);
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
            selectionToolCollider.SetSelectionToolEnabled(false);
            deleteTool.SetActive(false);
            minimizeTool.SetActive(false);
            DesiredModel = Model.Normal;
            laserPointerController.ToggleLaser(false);
            keyboard.SetKeyboardVisible(false);
            drawTool.SetActive(false);
            webBrowser.GetComponent<WebManager>().SetVisible(false);
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
        public void SwitchSelectionToolColor(Color color)
        {

        }

        public void SwitchSelectionToolMesh(bool dir)
        {
            if (ActualModel == Model.SelectionTool)
            {
                selectionToolCollider.CurrentMeshIndex += dir ? 1 : -1;
                ActivateDesiredTool();
            }
        }

        /// <summary>
        /// Changes the colors of the decals on the controllers.
        /// </summary>
        /// <param name="mode">The new mode </param>
        /// <exception cref="ArgumentException"></exception>
        public void SwitchControllerModelColor(ControllerColorMode mode)
        {
            colorMode = mode;
            // find all decal gameobjects
            Transform leftDecals = leftControllerScript.modelParent.Find("Left(Clone)/Decals");
            Transform rightDecals = rightControllerScript.modelParent.Find("Right(Clone)/Decals");
            // concatenate the lists and remove the parents, they don't have renderers
            if (!leftDecals)
            {
                leftDecals = leftControllerScript.modelParent;
            }

            if (!rightDecals)
            {
                rightDecals = rightControllerScript.modelParent;
            }

            IEnumerable<Transform> allDecals = leftDecals.GetComponentsInChildren<Transform>()
                    .Concat(rightDecals.GetComponentsInChildren<Transform>())
                    .Where((Transform t) => t != rightDecals && t != leftDecals);

            switch (mode)
            {
                case ControllerColorMode.StaticColors:
                    foreach (Transform t in allDecals)
                    {
                        ColorPreset colorPreset = t.GetComponent<ColorPreset>();
                        if (colorPreset)
                        {
                            if (staticMaterialsCache.ContainsKey(colorPreset.color))
                            {
                                t.GetComponent<Renderer>().material = staticMaterialsCache[colorPreset.color];
                            }
                            else
                            {
                                Material newMaterial = new Material(defaultStaticMaterial)
                                {
                                    color = colorPreset.color
                                };
                                staticMaterialsCache[colorPreset.color] = newMaterial;
                                t.GetComponent<Renderer>().material = newMaterial;
                            }
                        }
                        else
                        {
                            t.GetComponent<Renderer>().material = defaultStaticMaterial;
                        }
                    }
                    break;
                case ControllerColorMode.AnimatedPulse:
                    foreach (Transform t in allDecals)
                    {
                        t.GetComponent<Renderer>().material = animatedPulseMaterial;
                    }
                    break;

                default:
                    throw new ArgumentException($"Unknown color mode: {mode}");
            }
        }
    }
    /// <summary>
    /// Extensions methods for <see cref="ControllerModelSwitcher.ControllerBrand"/>.
    /// </summary>
    public static class ControllerBrandExtensions
    {
        /// <summary>
        /// Converts a <see cref="ControllerModelSwitcher.ControllerBrand"/> into a human friendly string.
        /// </summary>
        /// <param name="brand">The <see cref="ControllerModelSwitcher.ControllerBrand"/> to convert.</param>
        /// <returns>The resulting string.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <see cref="ControllerModelSwitcher.ControllerBrand"/> is not in the range of defined values.</exception>
        public static string ToFriendlyString(this ControllerModelSwitcher.ControllerBrand brand)
        {
            switch (brand)
            {
                case ControllerModelSwitcher.ControllerBrand.Set_Automatically:
                    return "Set Automatically";
                case ControllerModelSwitcher.ControllerBrand.HP_Reverb:
                    return "HP Reverb";
                case ControllerModelSwitcher.ControllerBrand.HTC_Vive:
                    return "HTC Vive";
                case ControllerModelSwitcher.ControllerBrand.Valve_Index:
                    return "Valve Index";
                default:
                    throw new System.ArgumentException("Undefined controller brand.");
            }
        }

        /// <summary>
        /// Converts a string into a <see cref="ControllerModelSwitcher.ControllerBrand"/>.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>The corresponding <see cref="ControllerModelSwitcher.ControllerBrand"/>, if conversion was possible.</returns>
        /// <exception cref="ArgumentException">Thrown if the string could not be recognised as a <see cref="ControllerModelSwitcher.ControllerBrand"/>.</exception>
        public static ControllerModelSwitcher.ControllerBrand ToBrand(this string s)
        {
            switch (s)
            {
                case "Set Automatically":
                    return ControllerModelSwitcher.ControllerBrand.Set_Automatically;
                case "HP Reverb":
                    return ControllerModelSwitcher.ControllerBrand.HP_Reverb;
                case "HTC Vive":
                    return ControllerModelSwitcher.ControllerBrand.HTC_Vive;
                case "Valve Index":
                    return ControllerModelSwitcher.ControllerBrand.Valve_Index;
                default:
                    throw new ArgumentException($"{s} is not a valid controller brand.");
            }
        }
    }

    /// <summary>
    /// Extensions methods for <see cref="ControllerModelSwitcher.ControllerColorMode"/>.
    /// </summary>
    public static class ControllerColorModeExtensions
    {
        /// <summary>
        /// Converts a string into a <see cref="ControllerModelSwitcher.ControllerColorMode"/>.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>The corresponding <see cref="ControllerModelSwitcher.ControllerColorMode"/>, if conversion was possible.</returns>
        /// <exception cref="ArgumentException">Thrown if the string could not be recognised as a <see cref="ControllerModelSwitcher.ControllerColorMode"/>.</exception>
        public static ControllerModelSwitcher.ControllerColorMode ToControllerColorMode(this string s)
        {
            switch (s)
            {
                case "Static Colors":
                    return ControllerModelSwitcher.ControllerColorMode.StaticColors;
                case "Animated Pulse":
                    return ControllerModelSwitcher.ControllerColorMode.AnimatedPulse;
                default:
                    throw new ArgumentException($"{s} is not a valid controller color mode");
            }
        }
    }
}