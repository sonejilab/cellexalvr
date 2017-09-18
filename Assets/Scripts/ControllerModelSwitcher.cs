using UnityEngine;
using VRTK;
using System.Collections;

/// <summary>
/// This class is responsible for changing the controller model and the activated tool.
/// </summary>
public class ControllerModelSwitcher : MonoBehaviour
{
    public ReferenceManager referenceManager;

    public SteamVR_RenderModel renderModel;
    public GameObject controllerBody;
    public Mesh normalControllerMesh;
    //public Texture normalControllerTexture;
    public Mesh menuControllerMesh;
    //public Texture menuControllerTexture;
    public Mesh selectionToolMesh;
    public Mesh deleteToolMesh;
    public Material normalMaterial;
    public Material selectionToolHandlerMaterial;
    public VRTK_StraightPointerRenderer rightLaser;
    public VRTK_StraightPointerRenderer leftLaser;

    public enum Model { Normal, SelectionTool, Menu, Minimizer, Magnifier, HeatmapDeleteTool, HelpTool, OneLaser, TwoLasers };
    // what model we actually want
    public Model DesiredModel { get; set; }
    // what model is actually displayed, useful for when we want to change the model temporarily
    // for example: the user has activated the selection tool, so DesiredModel = SelectionTool and actualModel = SelectionTool
    // the user then moves the controller into the menu. DesiredModel is still SelectionTool, but actualModel will now be Menu
    public Model ActualModel;

    private SelectionToolHandler selectionToolHandler;
    private GameObject fire;
    private GameObject minimizer;
    private GameObject magnifier;
    private HelperTool helpTool;
    private MeshFilter controllerBodyMeshFilter;
    private Renderer controllerBodyRenderer;
    private Color desiredColor;

    void Awake()
    {
        helpTool = referenceManager.helpTool;
        helpTool.SaveLayersToIgnore();
        DesiredModel = Model.Normal;
        if (controllerBody.activeSelf == false)
            SteamVR_Events.RenderModelLoaded.Listen(OnControllerLoaded);
        else
        {
            controllerBodyMeshFilter = controllerBody.GetComponent<MeshFilter>();
            controllerBodyRenderer = controllerBody.GetComponent<Renderer>();
        }
    }

    private void Start()
    {
        selectionToolHandler = referenceManager.selectionToolHandler;
        fire = referenceManager.fire;
        minimizer = referenceManager.minimizeTool.gameObject;
        magnifier = referenceManager.magnifierTool.gameObject;
    }

    // Used when starting the program to know when steamvr has loaded the model and applied a meshfilter and meshrenderer for us to use.
    void OnControllerLoaded(SteamVR_RenderModel renderModel, bool success)
    {
        if (!success) return;
        controllerBodyMeshFilter = controllerBody.GetComponent<MeshFilter>();
        controllerBodyRenderer = controllerBody.GetComponent<Renderer>();
    }

    // Used when starting the program.
    // It takes some time for steamvr and vrtk to set everything up, and for some time
    // these variables will be null because the components are not yet added to the gameobjects.
    internal bool Ready()
    {
        return controllerBodyMeshFilter != null && controllerBodyRenderer != null;
    }

    void OnTriggerEnter(Collider other)
    {
        //print("ontriggerenter " + other.gameObject.name);
        if (other.gameObject.name.Equals("Menu Selecter Collider"))
        {
            //print ("ontriggerenter " + other.gameObject.name);
            if (controllerBodyMeshFilter == null) return;
            SwitchToModel(Model.Menu);
            fire.SetActive(false);
            minimizer.SetActive(false);
            magnifier.SetActive(false);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Equals("Menu Selecter Collider"))
        {
            if (controllerBodyMeshFilter == null) return;
            SwitchToModel(DesiredModel);
            ActivateDesiredTool();
        }
    }

    /// <summary>
    /// Switches the right controller's model.
    /// </summary>
    public void SwitchToModel(Model model)
    {
        //print ("switching to " + model);
        ActualModel = model;
        switch (model)
        {
            case Model.Normal:
            case Model.Magnifier:
            case Model.HelpTool:
            case Model.OneLaser:
            case Model.TwoLasers:
                controllerBodyMeshFilter.mesh = normalControllerMesh;
                controllerBodyRenderer.material = normalMaterial;
                break;

            case Model.Menu:
                controllerBodyMeshFilter.mesh = menuControllerMesh;
                controllerBodyRenderer.material = normalMaterial;
                break;

            case Model.SelectionTool:
                controllerBodyMeshFilter.mesh = selectionToolMesh;
                controllerBodyRenderer.material = selectionToolHandlerMaterial;
                controllerBodyRenderer.material.color = desiredColor;
                break;

            case Model.Minimizer:
                controllerBodyMeshFilter.mesh = deleteToolMesh;
                break;
        }
    }

    /// <summary>
    /// Activates the current tool and changes the controller's model to that tool and deactivates previously active tool.
    /// </summary>
    public void ActivateDesiredTool()
    {
        selectionToolHandler.SetSelectionToolEnabled(false);
        fire.SetActive(false);
        magnifier.SetActive(false);
        minimizer.SetActive(false);
        helpTool.SetToolActivated(false);
        rightLaser.enabled = false;
        leftLaser.enabled = false;
        switch (DesiredModel)
        {
            case Model.SelectionTool:
                selectionToolHandler.SetSelectionToolEnabled(true);
                break;
            case Model.Magnifier:
                magnifier.SetActive(true);
                break;
            case Model.HeatmapDeleteTool:
                fire.SetActive(true);
                break;
            case Model.Minimizer:
                minimizer.SetActive(true);
                break;
            case Model.HelpTool:
                helpTool.SetToolActivated(true);
                rightLaser.enabled = true;
                break;
            case Model.OneLaser:
                rightLaser.enabled = true;
                break;
            case Model.TwoLasers:
                rightLaser.enabled = true;
                leftLaser.enabled = true;
                break;
        }
        SwitchToDesiredModel();
    }

    /// <summary>
    /// Turns off the active tool and sets our desired model to the normal model.
    /// </summary>
    /// <param name="inMenu"> True if the controller is in the menu and we should temporarily change into the menu model, false otherwise. </param>
    public void TurnOffActiveTool(bool inMenu)
    {
        selectionToolHandler.SetSelectionToolEnabled(false);
        fire.SetActive(false);
        magnifier.SetActive(false);
        minimizer.SetActive(false);
        helpTool.SetToolActivated(false);
        rightLaser.enabled = false;
        leftLaser.enabled = false;
        DesiredModel = Model.Normal;
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
            controllerBodyRenderer.material.color = desiredColor;
    }
}
