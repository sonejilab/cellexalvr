using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRTK;

/// <summary>
/// This class represents the selection tool that can be used to select multiple GraphPoints.
/// </summary>
public class SelectionToolHandler : MonoBehaviour
{

    public SelectionToolMenu selectionToolMenu;
    public GraphManager manager;
    public ControllerModelSwitcher controllerModelSwitcher;
    public SteamVR_TrackedController right;
    public SteamVR_TrackedController left;
    public ushort hapticIntensity = 2000;
    public RadialMenu radialMenu;
    public Sprite[] buttonIcons;
    [HideInInspector]
    public bool selectionConfirmed = false;
    [HideInInspector]
    public bool heatmapGrabbed = false;
    [HideInInspector]
    public int fileCreationCtr = 0;
    private ArrayList selectedCells = new ArrayList();
    private ArrayList lastSelectedCells = new ArrayList();
    private Color[] colors;
    private int currentColorIndex = 0;
    private Color selectedColor;
    private PlanePicker planePicker;
    private bool inSelectionState = false;
    private bool selectionMade = false;
    private GameObject leftController;
    private GameObject grabbedObject;
    private bool heatmapCreated = true;

    public string DataDir { get; set; }

    void Awake()
    {
        colors = new Color[10];
        colors[0] = new Color(1, 0, 0, .5f);     // red
        colors[1] = new Color(0, 0, 1, .5f);     // blue
        colors[2] = new Color(0, 1, 1, .5f);     // cyan
        colors[3] = new Color(1, 0, 1, .5f);     // magenta
        colors[4] = new Color(1f, 153f / 255f, 204f / 255f);     // pink
        colors[5] = new Color(1, 1, 0, .5f);     // yellow
        colors[6] = new Color(0, 1, 0, .5f);     // green
        colors[7] = new Color(.5f, 0, .5f, .5f);     // purple
        colors[8] = new Color(.4f, .2f, 1, .5f);     // brown
        colors[9] = new Color(1, .6f, .2f, .5f);     // orange
                                                     //selectorMaterial.color = colors[0];
        radialMenu.buttons[1].ButtonIcon = buttonIcons[buttonIcons.Length - 1];
        radialMenu.buttons[3].ButtonIcon = buttonIcons[1];
        radialMenu.RegenerateButtons();

        selectedColor = colors[0];
        SetSelectionToolEnabled(false);
        //UpdateButtonIcons();
        leftController = GameObject.Find("LeftController");
    }

    /// <summary>
    /// This method is called a child object that holds the collider.
    /// </summary>
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

    /// <summary>
    /// Adds rigidbody to all selected cells, making them fall to the ground.
    /// </summary>
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

    /// <summary>
    /// Confirms a selection and dumps the relevant data to a .txt file.
    /// </summary>
    public void ConfirmSelection()
    {
        foreach (Collider cell in selectedCells)
        {
            GameObject graphpoint = cell.gameObject;
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

    /// <summary>
    /// Unselects anything selected.
    /// </summary>
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

    /// <summary>
    /// Changes the color of the selection tool.
    /// </summary>
    /// <param name="dir"> The direction ot move in the array colors. </param>
    public void ChangeColor(bool dir)
    {
        if (currentColorIndex == colors.Length - 1 && dir)
        {
            currentColorIndex = 0;
        }
        else if (currentColorIndex == 0 && !dir)
        {
            currentColorIndex = colors.Length - 1;
        }
        else if (dir)
        {
            currentColorIndex++;
        }
        else
        {
            currentColorIndex--;
        }
        int buttonIndexLeft = currentColorIndex == 0 ? colors.Length - 1 : currentColorIndex - 1;
        int buttonIndexRight = currentColorIndex == colors.Length - 1 ? 0 : currentColorIndex + 1;
        radialMenu.buttons[1].ButtonIcon = buttonIcons[buttonIndexLeft];
        radialMenu.buttons[3].ButtonIcon = buttonIcons[buttonIndexRight];
        radialMenu.RegenerateButtons();
        selectedColor = colors[currentColorIndex];
        controllerModelSwitcher.SwitchControllerModelColor(colors[currentColorIndex]);

        //selectorMaterial.color = selectedColor;
        // this.gameObject.GetComponent<Renderer>().material.color = new Color(selectedColor.r, selectedColor.g, selectedColor.b);
    }

    public void HeatmapCreated()
    {
        heatmapCreated = true;
    }

    public bool GetHeatmapCreated()
    {
        return heatmapCreated;
    }

    /// <summary>
    /// Dumps the current selection to a .txt file.
    /// </summary>
    private void DumpData()
    {
        // print(new System.Diagnostics.StackTrace());
        using (System.IO.StreamWriter file =
                   new System.IO.StreamWriter(Directory.GetCurrentDirectory() + "\\Assets\\Data\\runtimeGroups\\selection" + (fileCreationCtr++) + ".txt"))
        {

            foreach (Collider cell in selectedCells)
            {
                GraphPoint graphPoint = cell.GetComponent<GraphPoint>();
                file.Write(graphPoint.GetLabel());
                file.Write("\t");
                Color c = cell.GetComponentInChildren<Renderer>().material.color;
                int r = (int)(c.r * 255);
                int g = (int)(c.g * 255);
                int b = (int)(c.b * 255);
                file.Write(string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b));
                file.Write("\t");
                file.Write(graphPoint.GraphName);
                file.WriteLine();
            }
            file.Flush();
            file.Close();
        }
    }

    public void SetSelectionToolEnabled(bool enabled)
    {
        controllerModelSwitcher.DesiredModel = enabled ? ControllerModelSwitcher.Model.SelectionTool : ControllerModelSwitcher.Model.Normal;
        controllerModelSwitcher.SwitchControllerModelColor(colors[currentColorIndex]);
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = enabled;
        }
    }

    public bool IsSelectionToolEnabled()
    {
        return GetComponentInChildren<Renderer>().enabled;
    }

}
