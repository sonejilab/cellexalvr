using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CellexalExtensions;
using UnityEngine;
using VRTK;

/// <summary>
/// Represents the selection tool that can be used to select multiple GraphPoints.
/// </summary>
public class SelectionToolHandler : MonoBehaviour
{
    public ReferenceManager referenceManager;

    public ushort hapticIntensity = 2000;
    public RadialMenu radialMenu;
    public Sprite[] buttonIcons;
    public GroupInfoDisplay groupInfoDisplay;
    public GroupInfoDisplay HUDGroupInfoDisplay;
    public GroupInfoDisplay FarGroupInfoDisplay;
    [HideInInspector]
    public bool selectionConfirmed = false;
    [HideInInspector]
    public bool heatmapGrabbed = false;
    [HideInInspector]
    public int fileCreationCtr = 0;
    public Color[] Colors;
    public Collider[] selectionToolColliders;

    private CreateSelectionFromPreviousSelectionMenu previousSelectionMenu;
    private ControllerModelSwitcher controllerModelSwitcher;
    private SteamVR_TrackedObject rightController;
    private List<GraphPoint> selectedCells = new List<GraphPoint>();
    private List<GraphPoint> lastSelectedCells = new List<GraphPoint>();
    private Color selectedColor;
    private PlanePicker planePicker;
    private bool selectionMade = false;
    private GameObject grabbedObject;
    private bool heatmapCreated = true;

    [HideInInspector]
    public int[] groups = new int[10];
    public int currentColorIndex = 0;
    public string DataDir { get; set; }
    private List<HistoryListInfo> selectionHistory = new List<HistoryListInfo>();
    // the number of steps we have taken back in the history.
    private int historyIndexOffset;
    private GameManager gameManager;

    public Filter CurrentFilter { get; set; }
    public bool RObjectUpdating { get; private set; }

    /// <summary>
    /// Helper struct for remembering history when selecting graphpoints.
    /// </summary>
    private struct HistoryListInfo
    {
        // the graphpoint this affects
        public GraphPoint graphPoint;
        // the group it was given, -1 means no group
        public int toGroup;
        // the group it had before, -1 means no group
        public int fromGroup;
        // true if this graphpoint was previously not in the list of selected graphpoints
        public bool newNode;

        public HistoryListInfo(GraphPoint graphPoint, int toGroup, int fromGroup, bool newNode)
        {
            this.graphPoint = graphPoint;
            this.toGroup = toGroup;
            this.fromGroup = fromGroup;
            this.newNode = newNode;
        }
    }

    void Awake()
    {
        radialMenu.buttons[1].ButtonIcon = buttonIcons[buttonIcons.Length - 1];
        radialMenu.buttons[3].ButtonIcon = buttonIcons[1];
        radialMenu.RegenerateButtons();
        previousSelectionMenu = referenceManager.createSelectionFromPreviousSelectionMenu;
        SetSelectionToolEnabled(false, 0);
        CellexalEvents.ConfigLoaded.AddListener(UpdateColors);
    }

    private void Start()
    {
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        rightController = referenceManager.rightController;
        gameManager = referenceManager.gameManager;
    }

    /// <summary>
    /// Updates <see cref="Colors"/> to <see cref="CellexalConfig.SelectionToolColors"/>.
    /// </summary>
    public void UpdateColors()
    {
        Colors = CellexalConfig.SelectionToolColors;
        selectedColor = Colors[currentColorIndex];
        groupInfoDisplay.SetColors(Colors);
        HUDGroupInfoDisplay.SetColors(Colors);
        FarGroupInfoDisplay.SetColors(Colors);
    }

    /// <summary>
    /// Adds a graphpoint to the current selection, and changes its color to the current color of the selection tool.
    /// This method is called by a child object that holds the collider.
    /// </summary>
    public void AddGraphpointToSelection(GraphPoint graphPoint)
    {
        AddGraphpointToSelection(graphPoint, currentColorIndex, true, Colors[currentColorIndex]);
    }

    /// <summary>
    /// Adds a graphpoint to the current selection, and changes its color.
    /// </summary>
    public void AddGraphpointToSelection(GraphPoint graphPoint, int newGroup, bool hapticFeedback)
    {
        AddGraphpointToSelection(graphPoint, currentColorIndex, true, Colors[newGroup]);
    }

    /// <summary>
    /// Adds a graphpoint to the current selection, and changes its color.
    /// </summary>
    public void AddGraphpointToSelection(GraphPoint graphPoint, int newGroup, bool hapticFeedback, Color color)
    {
        // print(other.gameObject.name);
        if (graphPoint == null)
        {
            return;
        }
        if (CurrentFilter != null && !CurrentFilter.Pass(graphPoint)) return;

        int oldGroup = graphPoint.CurrentGroup;
        if (newGroup < Colors.Length && color.Equals(Colors[newGroup]))
        {

            graphPoint.SetOutLined(true, newGroup);

        }
        else
        {

            graphPoint.SetOutLined(true, color);

        }
        graphPoint.CurrentGroup = newGroup;
        // renderer.material.color = Colors[newGroup];
        gameManager.InformGraphPointChangedColor(graphPoint.GraphName, graphPoint.Label, color);

        bool newNode = !selectedCells.Contains(graphPoint);
        if (historyIndexOffset != 0)
        {
            // if we have undone some selected graphpoints, then they should be removed from the history
            selectionHistory.RemoveRange(selectionHistory.Count - historyIndexOffset, historyIndexOffset);
            historyIndexOffset = 0;
            // turn off the redo buttons
            CellexalEvents.EndOfHistoryReached.Invoke();
        }
        if (!selectionMade)
        {
            // if this is a new selection we should reset some stuff
            selectionMade = true;
            //selectionToolMenu.SelectionStarted();
            groupInfoDisplay.ResetGroupsInfo();
            HUDGroupInfoDisplay.ResetGroupsInfo();
            FarGroupInfoDisplay.ResetGroupsInfo();
            // turn on the undo buttons
            CellexalEvents.BeginningOfHistoryLeft.Invoke();
        }
        // The user might select cells that already have that color
        if (newGroup != oldGroup || newNode)
        {
            selectionHistory.Add(new HistoryListInfo(graphPoint, newGroup, oldGroup, newNode));

            if (hapticFeedback)
                SteamVR_Controller.Input((int)rightController.index).TriggerHapticPulse(hapticIntensity);

            groupInfoDisplay.ChangeGroupsInfo(newGroup, 1);
            HUDGroupInfoDisplay.ChangeGroupsInfo(newGroup, 1);
            FarGroupInfoDisplay.ChangeGroupsInfo(newGroup, 1);
            if (newNode)
            {
                gameManager.InformSelectedAdd(graphPoint.GraphName, graphPoint.Label);
                if (selectedCells.Count == 0)
                {
                    CellexalEvents.SelectionStarted.Invoke();
                }
                selectedCells.Add(graphPoint);
            }
            else
            {
                groupInfoDisplay.ChangeGroupsInfo(oldGroup, -1);
                HUDGroupInfoDisplay.ChangeGroupsInfo(oldGroup, -1);
                FarGroupInfoDisplay.ChangeGroupsInfo(oldGroup, -1);
            }
        }
    }

    public void DoClientSelectAdd(string graphName, string label)
    {
        GraphPoint gp = referenceManager.graphManager.FindGraphPoint(graphName, label);
        selectedCells.Add(gp);
    }

    /// <summary>
    /// Goes back one step in the history of selecting cells.
    /// </summary>
    public void GoBackOneStepInHistory()
    {
        if (historyIndexOffset == 0)
        {
            CellexalEvents.EndOfHistoryLeft.Invoke();
        }

        int indexToMoveTo = selectionHistory.Count - historyIndexOffset - 1;
        if (indexToMoveTo == 0)
        {
            // beginning of history reached
            CellexalEvents.BeginningOfHistoryReached.Invoke();
            //selectionToolMenu.UndoSelection();
        }
        else if (indexToMoveTo < 0)
        {
            // no more history
            return;
        }
        HistoryListInfo info = selectionHistory[indexToMoveTo];
        info.graphPoint.CurrentGroup = info.fromGroup;

        // if info.fromGroup != 1 then the outline should be drawn
        info.graphPoint.SetOutLined(info.fromGroup != -1, info.fromGroup);

        groupInfoDisplay.ChangeGroupsInfo(info.toGroup, -1);
        HUDGroupInfoDisplay.ChangeGroupsInfo(info.toGroup, -1);
        FarGroupInfoDisplay.ChangeGroupsInfo(info.toGroup, -1);
        if (info.newNode)
        {
            selectedCells.Remove(info.graphPoint);
            info.graphPoint.SetOutLined(false, -1);
        }
        else
        {
            groupInfoDisplay.ChangeGroupsInfo(info.fromGroup, 1);
            HUDGroupInfoDisplay.ChangeGroupsInfo(info.fromGroup, 1);
            FarGroupInfoDisplay.ChangeGroupsInfo(info.fromGroup, 1);
            info.graphPoint.SetOutLined(true, info.fromGroup);
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
            CellexalEvents.BeginningOfHistoryLeft.Invoke();
            //selectionToolMenu.SelectionStarted();
        }

        int indexToMoveTo = selectionHistory.Count - historyIndexOffset;
        if (indexToMoveTo == selectionHistory.Count - 1)
        {
            // end of history reached
            CellexalEvents.EndOfHistoryReached.Invoke();
        }
        else if (indexToMoveTo >= selectionHistory.Count)
        {
            // no more history
            return;
        }

        HistoryListInfo info = selectionHistory[indexToMoveTo];
        info.graphPoint.CurrentGroup = info.toGroup;
        info.graphPoint.SetOutLined(info.toGroup != -1, info.toGroup);
        groupInfoDisplay.ChangeGroupsInfo(info.toGroup, 1);
        HUDGroupInfoDisplay.ChangeGroupsInfo(info.toGroup, 1);
        FarGroupInfoDisplay.ChangeGroupsInfo(info.toGroup, 1);
        if (info.newNode)
        {
            selectedCells.Add(info.graphPoint);
        }
        else
        {
            groupInfoDisplay.ChangeGroupsInfo(info.fromGroup, -1);
            HUDGroupInfoDisplay.ChangeGroupsInfo(info.fromGroup, -1);
            FarGroupInfoDisplay.ChangeGroupsInfo(info.fromGroup, -1);
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
        int group = selectionHistory[indexToMoveTo].toGroup;
        Color color = group != -1 ? Colors[group] : Color.white;
        Color nextColor;
        do
        {
            GoBackOneStepInHistory();
            indexToMoveTo--;
            if (indexToMoveTo >= 0)
            {
                nextColor = Colors[selectionHistory[indexToMoveTo].toGroup];
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
        int group = selectionHistory[indexToMoveTo].toGroup;
        Color color = group != -1 ? Colors[group] : Color.white;
        Color nextColor;
        do
        {
            GoForwardOneStepInHistory();
            indexToMoveTo++;
            if (indexToMoveTo < selectionHistory.Count)
            {
                nextColor = Colors[selectionHistory[indexToMoveTo].toGroup];
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
            other.SetOutLined(false, -1);
        }
        selectionHistory.Clear();
        CellexalEvents.SelectionCanceled.Invoke();
        selectedCells.Clear();
        selectionMade = false;
        //selectionToolMenu.RemoveSelection();
    }

    /// <summary>
    /// Confirms a selection and dumps the relevant data to a .txt file.
    /// </summary>
    public void ConfirmSelection()
    {
        if (selectedCells.Count == 0)
        {
            print("empty selection confirmed");
        }
        // create .txt file with latest selection
        DumpData();
        lastSelectedCells.Clear();
        StartCoroutine(UpdateRObjectGrouping());
        foreach (GraphPoint gp in selectedCells)
        {
            if (gp.CustomColor)
                gp.SetOutLined(false, gp.Material.color);
            else
                gp.SetOutLined(false, gp.CurrentGroup);
            lastSelectedCells.Add(gp);
        }
        previousSelectionMenu.CreateButton(selectedCells);
        // clear the list since we are done with it
        selectedCells.Clear();
        selectionHistory.Clear();
        CellexalEvents.SelectionConfirmed.Invoke();
        heatmapCreated = false;
        selectionMade = false;
        selectionConfirmed = true;
        //selectionToolMenu.ConfirmSelection();
    }

    private IEnumerator UpdateRObjectGrouping()
    {
        RObjectUpdating = true;
        // wait one frame to let ConfirmSelection finish.
        yield return null;
        string rScriptFilePath = Application.streamingAssetsPath + @"\R\update_grouping.R";
        string args = CellexalUser.UserSpecificFolder + "\\selection" + (fileCreationCtr - 1) + ".txt " + CellexalUser.UserSpecificFolder + " " + DataDir;
        CellexalLog.Log("Updating R Object grouping at " + CellexalUser.UserSpecificFolder);
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Thread t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
        t.Start();
        while (t.IsAlive)
        {
            yield return null;
        }
        stopwatch.Stop();
        CellexalLog.Log("Updating R Object finished in " + stopwatch.Elapsed.ToString());
        RObjectUpdating = false;
    }

    /// <summary>
    /// Gets the last selection that was confirmed.
    /// </summary>
    /// <returns> A List of all graphpoints that were selected. </returns>
    public List<GraphPoint> GetLastSelection()
    {
        return lastSelectedCells;
    }

    /// <summary>
    /// Get the current (not yet confirmed) selection.
    /// </summary>
    /// <returns> A List of all graphpoints currently selected. </returns>
    public List<GraphPoint> GetCurrentSelection()
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
            other.ResetColor();
        }
        CellexalEvents.SelectionCanceled.Invoke();
        historyIndexOffset = selectionHistory.Count;
        selectedCells.Clear();
        selectionMade = false;
        //selectionToolMenu.UndoSelection();
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
        radialMenu.buttons[1].ButtonIcon = buttonIcons[buttonIndexLeft];
        radialMenu.buttons[3].ButtonIcon = buttonIcons[buttonIndexRight];
        radialMenu.RegenerateButtons();
        selectedColor = Colors[currentColorIndex];
        controllerModelSwitcher.SwitchControllerModelColor(Colors[currentColorIndex]);
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
        string filePath = CellexalUser.UserSpecificFolder + "\\selection" + (fileCreationCtr++) + ".txt";
        using (StreamWriter file =
                   new StreamWriter(filePath))
        {
            CellexalLog.Log("Dumping selection data to " + CellexalLog.FixFilePath(filePath));
            CellexalLog.Log("\tSelection consists of  " + selectedCells.Count + " points");
            if (selectionHistory != null)
                CellexalLog.Log("\tThere are " + selectionHistory.Count + " entries in the history");
            foreach (GraphPoint gp in selectedCells)
            {
                file.Write(gp.Label);
                file.Write("\t");
                Color c = gp.Material.color;
                int r = (int)(c.r * 255);
                int g = (int)(c.g * 255);
                int b = (int)(c.b * 255);
                // writes the color as #RRGGBB where RR, GG and BB are hexadecimal values
                file.Write(string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b));
                file.Write("\t");
                file.Write(gp.GraphName);
                file.Write("\t");
                file.Write(gp.CurrentGroup);
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
    /// <param name="meshIndex">The index of the collider that should be activated, if <paramref name="enabled"/> is <code>true</code>.</param>
    public void SetSelectionToolEnabled(bool enabled, int meshIndex)
    {
        if (enabled)
        {
            controllerModelSwitcher.SwitchControllerModelColor(Colors[currentColorIndex]);
        }
        for (int i = 0; i < selectionToolColliders.Length; ++i)
        {
            // if we are turning on the selection tool, enable the collider with the corresponding index as the mesh and disable the other colliders.
            selectionToolColliders[i].enabled = enabled && meshIndex == i;
        }
    }

    public bool IsSelectionToolEnabled()
    {
        return GetComponentInChildren<Collider>().enabled;
    }

    public Color GetColor(int index)
    {
        return Colors[index];
    }


}
