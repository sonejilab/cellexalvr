using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

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
    public Material normalMaterial;
    public Material selectionToolHandlerMaterial;
    public GameObject fire;
    public SelectionToolButton selectionToolButton;
    //public List<GameObject> activatedInMenu;
    //public List<GameObject> deactivatedInMenu;
    public enum Model { Normal, SelectionTool, Menu };
    public Model DesiredModel { get; set; }
    private Model actualModel;
    private bool selectionToolEnabled = false;
    private bool fireEnabled = false;
    private MeshFilter controllerBodyMeshFilter;
    private Renderer controllerBodyRenderer;
    private Color desiredColor;

    void Awake()
    {
        SteamVR_Events.RenderModelLoaded.Listen(OnControllerLoaded);
    }

    void OnControllerLoaded(SteamVR_RenderModel renderModel, bool success)
    {
        if (!success) return;
        controllerBodyMeshFilter = controllerBody.GetComponent<MeshFilter>();
        controllerBodyRenderer = controllerBody.GetComponent<Renderer>();
        StartCoroutine(ChangeModelOnStart());
    }

    IEnumerator ChangeModelOnStart()
    {
        if (controllerBodyMeshFilter != null && controllerBodyRenderer != null)
        {
            SwitchToModel(Model.Normal);
        }
        else
        {
            yield return null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //print("ontriggerenter " + other.gameObject.name);
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
        {
            if (controllerBodyMeshFilter == null) return;
            SwitchToModel(Model.Menu);
            fireEnabled = fire.activeSelf;
            fire.SetActive(false);

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
        {
            if (controllerBodyMeshFilter == null) return;
            SwitchToModel(DesiredModel);
        }
    }

    /// <summary>
    /// Should be called when a button that changes the tool is pressed.
    /// </summary>
    public void ToolSwitched()
    {
        selectionToolEnabled = false;
        fireEnabled = false;
    }

    /// <summary>
    /// Switches the right controller's model.
    /// </summary>
    public void SwitchToModel(Model model)
    {
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
        }
    }

    public void TurnOffActiveTool()
    {
        
        selectionToolEnabled = false;
        fireEnabled = false;
        selectionToolHandler.SetSelectionToolEnabled(false);
        fire.SetActive(false);
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

    /// <summary>
    /// Check if the contoller is inside the menu. If, switch to the menu model. If not, switch to the normal model.
    /// </summary>
    public void SwitchControllerModel()
    {
        BoxCollider collider = gameObject.GetComponent<BoxCollider>();
        Vector3 center = collider.transform.position + collider.center;
        Vector3 halfExtents = Vector3.Scale(collider.size / 2, collider.transform.localScale);
        int layerMask = 0;
        layerMask = ~layerMask;
        Quaternion rotation = collider.transform.rotation;
        Collider[] allOverlappingColliders = Physics.OverlapBox(center, halfExtents, rotation, layerMask, QueryTriggerInteraction.Collide);
        bool controllerInside = false;
        foreach (Collider c in allOverlappingColliders)
        {
            if (c.gameObject.CompareTag("Smaller Controller Collider"))
            {
                SwitchToModel(Model.Menu);
                controllerInside = true;
                break;
            }
        }
        if (!controllerInside)
        {
            SwitchToModel(Model.Normal);
        }

    }

}
