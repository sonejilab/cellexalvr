using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// This class represents a heatmap.
/// </summary>
public class Heatmap : MonoBehaviour
{

    public Texture texture;
    public TextMesh infoText;
    private Dictionary<Cell, Color> containedCells;
    private SteamVR_Controller.Device device;
    private GraphManager graphManager;
    private SelectionToolHandler selectionToolHandler;
    private bool controllerInside = false;
    private GameObject fire;
    private SteamVR_TrackedObject rightController;

    // Use this for initialization
    void Start()
    {
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Controller")
        {
            controllerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Controller")
        {
            controllerInside = false;
        }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && fire.activeSelf)
        {
            gameObject.GetComponent<HeatmapBurner>().BurnHeatmap();
        }
    }

    /// <summary>
    /// Updates this heatmap's image.
    /// </summary>
    public void UpdateImage(string filepath)
    {
        byte[] fileData = File.ReadAllBytes(filepath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
    }

    /// <summary>
    /// Recolours all graphs with the colors that the cells had when this heatmap was created.
    /// Graph points that are not part of this heatmap are not recoloured and will keep their colour.
    /// </summary>
    public void ColorCells()
    {
        // print("color cells");
        Graph[] graphs = graphManager.GetComponentsInChildren<Graph>();
        foreach (Graph g in graphs)
        {
            GraphPoint[] points = g.GetComponentsInChildren<GraphPoint>();
            foreach (GraphPoint gp in points)
            {
                if (containedCells.ContainsKey(gp.GetCell()))
                {
                    gp.GetComponent<Renderer>().material.color = containedCells[gp.GetCell()];
                }

            }
        }
    }

    /// <summary>
    /// Sets some variables. Should be called after a heatmap is instantiated.
    /// </summary>
    public void SetVars(GraphManager graphManager, SelectionToolHandler selectionToolHandler, ArrayList cells, GameObject fire)
    {
        containedCells = new Dictionary<Cell, Color>();
        this.graphManager = graphManager;
        this.selectionToolHandler = selectionToolHandler;
        this.fire = fire;
        int numberOfColours = 0;
        List<Color> checkedColors = new List<Color>();
        foreach (GraphPoint g in cells)
        {
            Color color = g.GetMaterial().color;
            containedCells[g.GetCell()] = color;
            if (!checkedColors.Contains(color))
            {
                numberOfColours++;
                checkedColors.Add(color);
            }
        }
        infoText.text = "Total number of cells: " + cells.Count;
        infoText.text += "\nNumber of colours: " + numberOfColours;
    }
}
