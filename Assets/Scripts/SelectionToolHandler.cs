using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using VRTK;

/// <summary>
/// This class represents the selection tool that can be used to select multiple GraphPoints.
/// </summary>
public class SelectionToolHandler : MonoBehaviour
{
    public ReferenceManager referenceManager;

    public ushort hapticIntensity = 2000;
    public RadialMenu radialMenu;
    public Sprite[] buttonIcons;
    public GroupInfoDisplay groupInfoDisplay;
    [HideInInspector]
    public bool selectionConfirmed = false;
    [HideInInspector]
    public bool heatmapGrabbed = false;
    [HideInInspector]
    public int fileCreationCtr = 0;

    private SelectionToolMenu selectionToolMenu;
    private ControllerModelSwitcher controllerModelSwitcher;
    private UndoButtonsHandler undoButtonsHandler;
    private SteamVR_TrackedObject rightController;
    private ArrayList selectedCells = new ArrayList();
    private ArrayList lastSelectedCells = new ArrayList();
    private Color[] colors;
    private Color selectedColor;
    private PlanePicker planePicker;
    private bool selectionMade = false;
    private GameObject grabbedObject;
    private bool heatmapCreated = true;

    [HideInInspector]
    public int[] groups = new int[10];
    private int currentColorIndex = 0;
    public string DataDir { get; set; }
    private List<HistoryListInfo> selectionHistory = new List<HistoryListInfo>();
    // the number of steps we have taken back in the history.
    private int historyIndexOffset;
    private GameManager gameManager;

    /// <summary>
    /// Helper struct for remembering history when selecting graphpoints.
    /// </summary>
    private struct HistoryListInfo
    {
        // the graphpoint this affects
        public GraphPoint graphPoint;
        // the color it was given
        public Color toColor;
        // the color it had before
        public Color fromColor;
        // true if this graphpoint was previously not in the list of selected graphpoints
        public bool newNode;

        public HistoryListInfo(GraphPoint graphPoint, Color toColor, Color fromColor, bool newNode)
        {
            this.graphPoint = graphPoint;
            this.toColor = toColor;
            this.fromColor = fromColor;
            this.newNode = newNode;
        }
    }

    void Awake()
    {
        // TODO: create more colors.
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
        groupInfoDisplay.SetColors(colors);

        selectedColor = colors[currentColorIndex];
        SetSelectionToolEnabled(false);
        //UpdateButtonIcons();
    }

    private void Start()
    {
        selectionToolMenu = referenceManager.selectionToolMenu;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        undoButtonsHandler = referenceManager.undoButtonsHandler;
        rightController = referenceManager.rightController;
        gameManager = referenceManager.gameManager;
    }

    /// <summary>
    /// Adds a graphpoint to the current selection, and changes its color to the current color of the selection tool.
    /// This method is called by a child object that holds the collider.
    /// </summary>
    public void AddGraphpointToSelection(GraphPoint graphPoint)
    {
        Color newColor = new Color(selectedColor.r, selectedColor.g, selectedColor.b);
        AddGraphpointToSelection(graphPoint, newColor, true);
    }

    /// <summary>
    /// Adds a graphpoint to the current selection, and changes its color.
    /// </summary>
    public void AddGraphpointToSelection(GraphPoint graphPoint, Color newColor, bool hapticFeedback)
    {
        // print(other.gameObject.name);
        if (graphPoint == null)
        {
            return;
        }
        Renderer renderer = graphPoint.gameObject.GetComponent<Renderer>();

        Color oldColor = renderer.material.color;
        renderer.material.color = newColor;
        gameManager.InformGraphPointChangedColor(graphPoint.GraphName, graphPoint.Label, newColor);

        bool newNode = !selectedCells.Contains(graphPoint);
        if (historyIndexOffset != 0)
        {
            // if we have undone some selected graphpoints, then they should be removed from the history
            selectionHistory.RemoveRange(selectionHistory.Count - historyIndexOffset, historyIndexOffset);
            historyIndexOffset = 0;
            // turn off the redo buttons
            undoButtonsHandler.EndOfHistoryReached();
        }
        if (!selectionMade)
        {
            // if this is a new selection we should reset some stuff
            selectionMade = true;
            selectionToolMenu.SelectionStarted();
            groupInfoDisplay.ResetGroupsInfo();
            // turn on the undo buttons
            undoButtonsHandler.BeginningOfHistoryLeft();
        }
        // The user might select cells that already have that color
        if (!Equals(newColor, oldColor))
        {
            selectionHistory.Add(new HistoryListInfo(graphPoint, newColor, oldColor, newNode));

            if (hapticFeedback)
                SteamVR_Controller.Input((int)rightController.index).TriggerHapticPulse(hapticIntensity);

            groupInfoDisplay.ChangeGroupsInfo(newColor, 1);
            if (newNode)
            {
                gameManager.InformSelectedAdd(graphPoint.GraphName, graphPoint.Label);
                selectedCells.Add(graphPoint);
            }
            else
            {
                groupInfoDisplay.ChangeGroupsInfo(oldColor, -1);
            }
        }
    }

    public void DoClientSelectAdd(string graphName, string label)
    {
        GraphPoint gp = referenceManager.graphManager.FindGraphPoint(graphName, label);
        selectedCells.Add(gp);
    }

    /// <summary>
    /// Helper method to see if two colors are equal.
    /// </summary>
    /// <param name="c1"> The first color. </param>
    /// <param name="c2"> The second color. </param>
    /// <returns> True if the two colors have the same rgb values, false otherwise. </returns>
    private bool Equals(Color c1, Color c2)
    {
        return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b;
    }

    /// <summary>
    /// Goes back one step in the history of selecting cells.
    /// </summary>
    public void GoBackOneStepInHistory()
    {
        if (historyIndexOffset == 0)
        {
            undoButtonsHandler.EndOfHistoryLeft();
        }

        int indexToMoveTo = selectionHistory.Count - historyIndexOffset - 1;
        if (indexToMoveTo == 0)
        {
            // beginning of history reached
            undoButtonsHandler.BeginningOfHistoryReached();
            selectionToolMenu.UndoSelection();
        }
        else if (indexToMoveTo < 0)
        {
            // no more history
            return;
        }
        HistoryListInfo info = selectionHistory[indexToMoveTo];
        info.graphPoint.GetComponent<Renderer>().material.color = info.fromColor;
        groupInfoDisplay.ChangeGroupsInfo(info.toColor, -1);
        if (info.newNode)
        {
            selectedCells.Remove(info.graphPoint);
        }
        else
        {
            groupInfoDisplay.ChangeGroupsInfo(info.fromColor, 1);
        }
        historyIndexOffset++;
        selectionMade = false;
    }

    /// <summary>
    /// Go forward one step in the history of selecting cells.
    /// </summary>
    public void GoForwardOneStepInHistory()
    {
        if (historyIndexOffset == selectionHistory.Count)
        {
            undoButtonsHandler.BeginningOfHistoryLeft();
            selectionToolMenu.SelectionStarted();
        }

        int indexToMoveTo = selectionHistory.Count - historyIndexOffset;
        if (indexToMoveTo == selectionHistory.Count - 1)
        {
            // end of history reached
            undoButtonsHandler.EndOfHistoryReached();
        }
        else if (indexToMoveTo >= selectionHistory.Count)
        {
            // no more history
            return;
        }

        HistoryListInfo info = selectionHistory[indexToMoveTo];
        info.graphPoint.GetComponent<Renderer>().material.color = info.toColor;
        groupInfoDisplay.ChangeGroupsInfo(info.toColor, 1);
        if (info.newNode)
        {
            selectedCells.Add(info.graphPoint);
        }
        else
        {
            groupInfoDisplay.ChangeGroupsInfo(info.fromColor, -1);
        }
        historyIndexOffset--;
        selectionMade = false;
    }

    /// <summary>
    /// Go back in history until the color changes. This unselects all the last cells that have the same color.
    /// </summary>
    /// <example>
    /// If the user selects 2 cells as red then 3 cells as blue and then 4 cells as red, in that order, the 4 last red cells would be unselected when calling this method. 
    /// </example>
    public void GoBackOneColorInHistory()
    {
        int indexToMoveTo = selectionHistory.Count - historyIndexOffset - 1;
        Color color = selectionHistory[indexToMoveTo].toColor;
        Color nextColor;
        do
        {
            GoBackOneStepInHistory();
            indexToMoveTo--;
            if (indexToMoveTo >= 0)
            {
                nextColor = selectionHistory[indexToMoveTo].toColor;
            }
            else
            {
                break;
            }
        } while (color.Equals(nextColor));
    }

    /// <summary>
    /// Go forward in history until the color changes. This re-selects all the last cells that have the same color.
    /// </summary>
    public void GoForwardOneColorInHistory()
    {
        int indexToMoveTo = selectionHistory.Count - historyIndexOffset;
        Color color = selectionHistory[indexToMoveTo].toColor;
        Color nextColor;
        do
        {
            GoForwardOneStepInHistory();
            indexToMoveTo++;
            if (indexToMoveTo < selectionHistory.Count)
            {
                nextColor = selectionHistory[indexToMoveTo].toColor;
            }
            else
            {
                break;
            }
        } while (color.Equals(nextColor));
    }

    public void SingleSelect(Collider other)
    {
        Color transparentColor = new Color(selectedColor.r, selectedColor.g, selectedColor.b);
        other.gameObject.GetComponent<Renderer>().material.color = transparentColor;
        GraphPoint gp = other.GetComponent<GraphPoint>();
        if (!selectedCells.Contains(gp))
        {
            selectedCells.Add(gp);
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
        //GetComponent<AudioSource>().Play();
        foreach (GraphPoint other in selectedCells)
        {
            other.transform.parent = null;
            other.gameObject.AddComponent<Rigidbody>();
            other.GetComponent<Rigidbody>().useGravity = true;
            other.GetComponent<Rigidbody>().isKinematic = false;
            other.GetComponent<Collider>().isTrigger = false;
        }
        selectionHistory.Clear();
        undoButtonsHandler.TurnAllButtonsOff();
        selectedCells.Clear();
        selectionMade = false;
        selectionToolMenu.RemoveSelection();
    }

    /// <summary>
    /// Confirms a selection and dumps the relevant data to a .txt file.
    /// </summary>
    public void ConfirmSelection()
    {
        // create .txt file with latest selection
        DumpData();
        lastSelectedCells.Clear();
        StartCoroutine(UpdateRObjectGrouping());
        foreach (GraphPoint c in selectedCells)
        {
            lastSelectedCells.Add(c.gameObject.GetComponent<GraphPoint>());
        }
        // clear the list since we are done with it
        selectedCells.Clear();
        selectionHistory.Clear();
        undoButtonsHandler.TurnAllButtonsOff();
        heatmapCreated = false;
        selectionMade = false;
        selectionConfirmed = true;
        selectionToolMenu.ConfirmSelection();
    }

    private IEnumerator UpdateRObjectGrouping()
    {
        string rScriptFilePath = Application.streamingAssetsPath + @"\R\update_grouping.R";
        string args = CellExAlUser.UserSpecificFolder + "/selection" + (fileCreationCtr - 1) + ".txt " + CellExAlUser.UserSpecificFolder + " " + DataDir;
        CellExAlLog.Log("Updating R Object grouping at " + CellExAlUser.UserSpecificFolder);
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Thread t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
        t.Start();
        while (t.IsAlive)
        {
            yield return null;
        }
        stopwatch.Stop();
        CellExAlLog.Log("Updating R Object finished in " + stopwatch.Elapsed.ToString());
    }

    public ArrayList GetLastSelection()
    {
        return lastSelectedCells;
    }

    public ArrayList GetCurrentSelection()
    {
        return selectedCells;
    }

    /// <summary>
    /// Unselects anything selected.
    /// </summary>
    public void CancelSelection()
    {
        foreach (GraphPoint other in selectedCells)
        {
            other.GetComponentInChildren<Renderer>().material.color = Color.white;
        }
        undoButtonsHandler.BeginningOfHistoryReached();
        undoButtonsHandler.EndOfHistoryLeft();
        historyIndexOffset = selectionHistory.Count;
        selectedCells.Clear();
        selectionMade = false;
        selectionToolMenu.UndoSelection();
    }

    /// <summary>
    /// Changes the color of the selection tool.
    /// </summary>
    /// <param name="dir"> The direction to move in the array of colors. true for increment, false for decrement </param>
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
        string filePath = CellExAlUser.UserSpecificFolder + "/selection" + (fileCreationCtr++) + ".txt";
        using (StreamWriter file =
                   new StreamWriter(filePath))
        {
            CellExAlLog.Log("Dumping selection data to " + filePath);
            CellExAlLog.Log("\tSelection consists of  " + selectedCells.Count + " points");
            if (selectionHistory != null)
                CellExAlLog.Log("\tThere are " + selectionHistory.Count + " entries in the history");

            foreach (GraphPoint gp in selectedCells)
            {

                file.Write(gp.Label);
                file.Write("\t");
                Color c = gp.GetComponentInChildren<Renderer>().material.color;
                int r = (int)(c.r * 255);
                int g = (int)(c.g * 255);
                int b = (int)(c.b * 255);
                // writes the color as #RRGGBB where RR, GG and BB are hexadecimal values
                file.Write(string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b));
                file.Write("\t");
                file.Write(gp.GraphName);
                file.WriteLine();
            }
            file.Flush();
            file.Close();
        }
    }

    /// <summary>
    /// Activates or deactivates all colliders on the selectiontool.
    /// </summary>
    /// <param name="enabled"> True if the selection tool should be activated, false if it should be deactivated. </param>
    public void SetSelectionToolEnabled(bool enabled)
    {
        if (enabled)
        {
            controllerModelSwitcher.SwitchControllerModelColor(colors[currentColorIndex]);
        }
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = enabled;
        }
    }

    public bool IsSelectionToolEnabled()
    {
        return GetComponentInChildren<Collider>().enabled;
    }

    public Color GetColor(int index)
    {
        return colors[index];
    }

}
