using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRTK;

/// <summary>
/// Rotates the menu 90 degrees to the left.
/// </summary>
public class SelectionToolHandler_old : MonoBehaviour
{

    public SelectionToolMenu selectionToolMenu;
    public GraphManager manager;
    public Graph graph;
    public Material selectorMaterial;
    public RadialMenu menu;
    public Sprite noButton;
    public Sprite toolButton;
    public Sprite confirmButton;
    public Sprite confirmButton_w;
    public Sprite cancelButton;
    public Sprite cancelButton_w;
    public Sprite blowupButton;
    public Sprite blowupButton_w;
    public Sprite colorButton;
    public Sprite recolorButton;
    public Sprite ddrtreeButton;
    public Sprite tsneButton;
    public SteamVR_TrackedController right;
    public SteamVR_TrackedController left;
    public bool selectionConfirmed = false;
    public ushort hapticIntensity = 2000;
    public bool heatmapGrabbed = false;
    public int fileCreationCtr = 0;
    public SteamVR_RenderModel renderModel;
    private ArrayList selectedCells = new ArrayList();
    private ArrayList lastSelectedCells = new ArrayList();
    private Color[] colors;
    private int currentColorIndex = 0;
    private Color selectedColor;
    private PlanePicker planePicker;
    private List<RadialMenuButton> buttons;
    private bool inSelectionState = false;
    private bool selectionMade = false;
    private GameObject leftController;
    private GameObject grabbedObject;
    private bool heatmapCreated = true;

    // int i = 0;

    void Start()
    {
        colors = new Color[10];
        colors[0] = new Color(1, 0, 0);         // red
        colors[1] = new Color(0, 0, 1);         // blue
        colors[2] = new Color(0, 1, 1);         // cyan
        colors[3] = new Color(1, 0, 1);         // magenta
        colors[4] = new Color(1f, 153f / 255f, 204f / 255f);         // pink
        colors[5] = new Color(1, 1, 0);         // yellow
        colors[6] = new Color(0, 1, 0);         // green
        colors[7] = new Color(.6f, 1, .6f);         // lime green
        colors[8] = new Color(.4f, .2f, 1);         // brown
        colors[9] = new Color(1, .6f, .2f);         // orange
        selectorMaterial.color = colors[0];
        selectedColor = colors[0];
        foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
        {
            r.material.color = colors[0];
        }
        SetSelectionToolEnabled(false);
        buttons = menu.buttons;
        //UpdateButtonIcons();
        leftController = GameObject.Find("LeftController");
    }

    void Update()
    {
        // if (selectionToolMenu == null) {
        //  print(i + ": null");
        //
        // }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "HeatBoard")
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "HeatBoard")
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = true;
            }
        }
    }

    void FindGrabbedHeatMap()
    {
        grabbedObject = leftController.GetComponent<VRTK_InteractGrab>().GetGrabbedObject();
        if (grabbedObject != null)
        {
            if (grabbedObject.tag == "HeatBoard")
            {
                if (!heatmapGrabbed)
                {
                    heatmapGrabbed = true;
                    //UpdateButtonIcons ();
                }
            }
        }
        else if (grabbedObject == null && heatmapGrabbed)
        {
            heatmapGrabbed = false;
            //UpdateButtonIcons ();
        }
    }

    public void Trigger(Collider other)
    {
        // print(other.gameObject.name);
        GraphPoint graphPoint = other.gameObject.GetComponent<GraphPoint>();
        if (graphPoint == null)
        {
            return;
        }
        Color transparentColor = new Color(selectedColor.r, selectedColor.g, selectedColor.b, .1f);
        graphPoint.gameObject.GetComponent<Renderer>().material.color = transparentColor;
        graphPoint.SetSelected(true);

        if (!selectedCells.Contains(other))
        {
            selectedCells.Add(other);
            SteamVR_Controller.Input((int)right.controllerIndex).TriggerHapticPulse(hapticIntensity);
        }
        if (!selectionMade)
        {
            selectionMade = true;
            selectionToolMenu.SelectionStarted();
            //UpdateButtonIcons ();
        }
    }

    public void SingleSelect(Collider other)
    {
        Color transparentColor = new Color(selectedColor.r, selectedColor.g, selectedColor.b, .1f);
        other.gameObject.GetComponent<Renderer>().material.color = transparentColor;
        if (!selectedCells.Contains(other))
        {
            selectedCells.Add(other);
        }
        if (!selectionMade)
        {
            selectionMade = true;
            //UpdateButtonIcons();
        }
    }

    public void ConfirmRemove()
    {
        GetComponent<AudioSource>().Play();
        foreach (Collider other in selectedCells)
        {
            other.transform.parent = null;
            other.gameObject.AddComponent<Rigidbody>();
            other.attachedRigidbody.useGravity = true;
            other.attachedRigidbody.isKinematic = false;
            other.isTrigger = false;
        }
        selectedCells.Clear();
        selectionMade = false;
        selectionToolMenu.RemoveSelection();
    }

    public void ConfirmSelection()
    {
        foreach (Collider cell in selectedCells)
        {
            GameObject graphpoint = cell.gameObject;
            //		graphpoint.transform.parent = newGraph.transform;
            Color cellColor = cell.gameObject.GetComponent<Renderer>().material.color;
            Color nonTransparentColor = new Color(cellColor.r, cellColor.g, cellColor.b);
            cell.gameObject.GetComponent<Renderer>().material.color = nonTransparentColor;
        }
        // create .txt file with latest selection
        DumpData();
        // clear the list since we are done with it
        lastSelectedCells.Clear();

        foreach (Collider c in selectedCells)
        {
            lastSelectedCells.Add(c.gameObject.GetComponent<GraphPoint>());
        }
        selectedCells.Clear();
        heatmapCreated = false;
        selectionMade = false;
        selectionConfirmed = true;
        selectionToolMenu.ConfirmSelection();
    }

    public ArrayList GetLastSelection()
    {
        return lastSelectedCells;
    }

    public void CancelSelection()
    {
        if (selectionToolMenu == null)
        {
            print("null");
        }
        foreach (Collider other in selectedCells)
        {
            other.GetComponentInChildren<Renderer>().material.color = Color.white;
        }
        selectedCells.Clear();
        selectionMade = false;
        selectionToolMenu.UndoSelection();
    }

    public void ChangeColor()
    {
        if (currentColorIndex == colors.Length - 1)
        {
            currentColorIndex = 0;
        }
        else
        {
            currentColorIndex++;
        }
        selectedColor = colors[currentColorIndex];
        selectorMaterial.color = selectedColor;
        foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
        {
            r.material.color = new Color(selectedColor.r, selectedColor.g, selectedColor.b);
        }
    }

    public void HeatmapCreated()
    {
        heatmapCreated = true;
    }

    public bool GetHeatmapCreated()
    {
        return heatmapCreated;
    }

    public void DumpData()
    {
        // print(new System.Diagnostics.StackTrace());
        using (System.IO.StreamWriter file =
                   new System.IO.StreamWriter(Directory.GetCurrentDirectory() + "\\Assets\\Data\\runtimeGroups\\selection" + (fileCreationCtr++) + ".txt"))
        {

            foreach (Collider cell in selectedCells)
            {
                file.Write(cell.GetComponent<GraphPoint>().GetLabel());
                file.Write("\t");
                Color c = cell.GetComponentInChildren<Renderer>().material.color;
                int r = (int)(c.r * 255);
                int g = (int)(c.g * 255);
                int b = (int)(c.b * 255);
                file.Write(string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b));
                file.WriteLine();
            }
            file.Flush();
            file.Close();
        }
    }

    public void SetSelectionToolEnabled(bool enabled)
    {
        foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
        {
            // print("renderer: " + enabled);
            r.enabled = enabled;
        }
        foreach (Collider c in gameObject.GetComponentsInChildren<Collider>())
        {
            c.enabled = enabled;
        }
        // renderModel.gameObject.SetActive(!enabled);
        // if (!enabled) {
        //  renderModel.UpdateModel();
        // }
    }

    public bool IsSelectionToolEnabled()
    {
        return gameObject.GetComponentInChildren<Renderer>().enabled;
    }

    public void Up()
    {
        if (inSelectionState && selectionMade)
        {
            // ConfirmSelection ();
            // HideSelectionTool();
            inSelectionState = false;
            //UpdateButtonIcons ();
        }
    }

    public void Left()
    {
        if (inSelectionState && selectionMade)
        {
            CancelSelection();
        }
        else if (inSelectionState)
        {
            // HideSelectionTool ();
            inSelectionState = false;
        }
        else
        {
            // ShowSelectionTool();
            inSelectionState = true;
        }
        //UpdateButtonIcons ();
    }

    public void Down()
    {
        if (heatmapGrabbed)
        {
            //grabbedObject.GetComponent<HeatmapBurner>().BurnHeatmap ();
        }
        else if (inSelectionState && selectionMade)
        {
            ConfirmRemove();
            //UpdateButtonIcons ();
        }
        else
        {
            //manager.HideDDRGraph();
        }
    }

    public void Right()
    {
        if (heatmapGrabbed)
        {
            grabbedObject.GetComponentInChildren<Heatmap>().ColorCells();
        }
        else if (inSelectionState)
        {
            ChangeColor();
        }
        else
        {
            //manager.HideTSNEGraph();
        }
    }

    public void UpdateButtonIcons()
    {
        // in selection state - selection made
        if (heatmapGrabbed)
        {
            buttons[0].ButtonIcon = noButton;
            buttons[1].ButtonIcon = noButton;
            buttons[2].ButtonIcon = blowupButton;
            buttons[3].ButtonIcon = recolorButton;
            // in selection state - no selection made
        }
        else if (inSelectionState && selectionMade)
        {
            buttons[0].ButtonIcon = confirmButton;
            buttons[1].ButtonIcon = cancelButton;
            buttons[2].ButtonIcon = blowupButton;
            buttons[3].ButtonIcon = colorButton;
            // in selection state - no selection made
        }
        else if (inSelectionState)
        {
            buttons[0].ButtonIcon = confirmButton_w;
            buttons[1].ButtonIcon = cancelButton_w;
            buttons[2].ButtonIcon = blowupButton_w;
            buttons[3].ButtonIcon = colorButton;
            // in default state
        }
        else
        {
            buttons[0].ButtonIcon = noButton;
            buttons[1].ButtonIcon = toolButton;
            buttons[2].ButtonIcon = ddrtreeButton;
            buttons[3].ButtonIcon = tsneButton;
        }
        //menu.RegenerateButtons();
    }

}
