using UnityEngine;
using System.Collections;

/// <summary>
/// This class is responsible for changing the controller model.
/// </summary>
public class ControllerModelSwitcher : MonoBehaviour
{
    public SteamVR_RenderModel renderModel;
    public GameObject controllerBody;
    public Mesh normalControllerMesh;
    public Texture normalControllerTexture;
    public Mesh menuControllerMesh;
    public Texture menuControllerTexture;
    public SelectionToolHandler selectionToolHandler;
    public Mesh selectionToolMesh;
    public Mesh deleteToolMesh;
    public Material normalMaterial;
    public Material selectionToolHandlerMaterial;
    public GameObject fire;
    public GameObject minimizer;
    public GameObject magnifier;
    public SelectionToolButton selectionToolButton;
    public enum Model { Normal, SelectionTool, Menu, Minimizer, Magnifier, HeatmapDeleteTool };
    public Model DesiredModel { get; set; }
    private Model actualModel;
    private MeshFilter controllerBodyMeshFilter;
    private Renderer controllerBodyRenderer;
    private Color desiredColor;

    void Awake()
    {
        DesiredModel = Model.Normal;
        if (controllerBody.activeSelf == false)
            SteamVR_Events.RenderModelLoaded.Listen(OnControllerLoaded);
        else
        {
            controllerBodyMeshFilter = controllerBody.GetComponent<MeshFilter>();
            controllerBodyRenderer = controllerBody.GetComponent<Renderer>();
        }
    }

    void OnControllerLoaded(SteamVR_RenderModel renderModel, bool success)
    {
        if (!success) return;
        controllerBodyMeshFilter = controllerBody.GetComponent<MeshFilter>();
        controllerBodyRenderer = controllerBody.GetComponent<Renderer>();
    }

    internal bool Ready()
    {
        return controllerBodyMeshFilter != null && controllerBodyRenderer != null;
    }

    void OnTriggerEnter(Collider other)
    {
        //print("ontriggerenter " + other.gameObject.name);
        if (other.gameObject.CompareTag("Controller"))
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
        if (other.gameObject.CompareTag("Controller"))
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
        actualModel = model;
        switch (model)
        {
            case Model.Normal:
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

            case Model.Magnifier:
                controllerBodyMeshFilter.mesh = normalControllerMesh;
                break;
        }
    }

    public void ActivateDesiredTool()
    {
        switch (DesiredModel)
        {
            case Model.SelectionTool:
                selectionToolHandler.SetSelectionToolEnabled(true, true);
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
        }
    }

    public void TurnOffActiveTool()
    {

        selectionToolHandler.SetSelectionToolEnabled(false, true);
        fire.SetActive(false);
        minimizer.SetActive(false);
        DesiredModel = Model.Normal;
        SwitchToModel(Model.Normal);
    }

    public void SwitchToDesiredModel()
    {
        SwitchToModel(DesiredModel);
    }

    public void SwitchControllerModelColor(Color color)
    {
        desiredColor = color;

        if (actualModel == Model.SelectionTool)
            controllerBodyRenderer.material.color = desiredColor;
    }
}
