using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    private SelectionFromPreviousMenu previousSelectionMenu;
    private ControllerModelSwitcher controllerModelSwitcher;
    private GraphManager graphManager;
    private SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
    private List<CombinedGraph.CombinedGraphPoint> selectedCells = new List<CombinedGraph.CombinedGraphPoint>();
    private List<CombinedGraph.CombinedGraphPoint> lastSelectedCells = new List<CombinedGraph.CombinedGraphPoint>();
    private Color selectedColor;
    private PlanePicker planePicker;
    private bool selectionMade = false;
    private GameObject grabbedObject;
    private bool heatmapCreated = true;
    private bool selActive = false;
    private int currentMeshIndex;
    public ParticleSystem particles;
    private bool hapticFeedbackThisFrame = true;
    private Stopwatch minkowskiTimeoutStopwatch = new Stopwatch();

    [HideInInspector]
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
        public CombinedGraph.CombinedGraphPoint graphPoint;
        // the group it was given, -1 means no group
        public int toGroup;
        // the group it had before, -1 means no group
        public int fromGroup;
        // true if this graphpoint was previously not in the list of selected graphpoints
        public bool newNode;

        public HistoryListInfo(CombinedGraph.CombinedGraphPoint graphPoint, int toGroup, int fromGroup, bool newNode)
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
        previousSelectionMenu = referenceManager.selectionFromPreviousMenu;
        graphManager = referenceManager.graphManager;
        SetSelectionToolEnabled(false, 0);

        CellexalEvents.ConfigLoaded.AddListener(UpdateColors);
    }

    private void Start()
    {
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        rightController = referenceManager.rightController;
        gameManager = referenceManager.gameManager;
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        // Only activate selection if trigger is pressed.
        //device = SteamVR_Controller.Input((int)rightController.index);
        // more_cells device = SteamVR_Controller.Input((int)rightController.index);
        //device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.SelectionTool)
        {
            if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                particles.gameObject.SetActive(true);
                ActivateSelection(true);
            }
            if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger))
            {
                hapticFeedbackThisFrame = true;

                Vector3 boundsCenter = selectionToolColliders[currentMeshIndex].bounds.center;
                Vector3 boundsExtents = selectionToolColliders[currentMeshIndex].bounds.extents;
                minkowskiTimeoutStopwatch.Stop();
                minkowskiTimeoutStopwatch.Start();
                float millisecond = Time.realtimeSinceStartup;
                foreach (var graph in graphManager.graphs)
                {
                    var closestPoints = graph.MinkowskiDetection(transform.position, boundsCenter, boundsExtents, currentColorIndex, millisecond);
                    foreach (var point in closestPoints)
                    {
                        AddGraphpointToSelection(point, currentColorIndex, true);
                    }
                }
            }
            else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                particles.gameObject.SetActive(false);
                ActivateSelection(false);
            }
        }
        // Sometimes the a bug occurs where particles stays active even when selection tool is off... Keep this line 
        // until you know why.
        particles.gameObject.SetActive(IsSelectionToolEnabled());

    }

    //private void FixedUpdate()
    //{
    //    if (controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.SelectionTool)
    //    {
    //        if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger))
    //        {
    //            hapticFeedbackThisFrame = true;

    //            Vector3 boundsCenter = selectionToolColliders[currentMeshIndex].bounds.center;
    //            Vector3 boundsExtents = selectionToolColliders[currentMeshIndex].bounds.extents;
    //            minkowskiTimeoutStopwatch.Stop();
    //            minkowskiTimeoutStopwatch.Start();
    //            int millisecond = Environment.TickCount;
    //            foreach (var graph in graphManager.graphs)
    //            {
    //                var closestPoints = graph.MinkowskiDetection(transform.position, boundsCenter, boundsExtents, currentColorIndex, millisecond);
    //                foreach (var point in closestPoints)
    //                {
    //                    AddGraphpointToSelection(point, currentColorIndex, true);
    //                }
    //            }
    //        }
    //    }
    //}

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
    public void AddGraphpointToSelection(CombinedGraph.CombinedGraphPoint graphPoint)
    {
        AddGraphpointToSelection(graphPoint, currentColorIndex, true, Colors[currentColorIndex]);
        // more_cells gameManager.InformSelectedAdd(graphPoint.GraphName, graphPoint.label, currentColorIndex, Colors[currentColorIndex]);
    }

    /// <summary>
    /// Adds a graphpoint to the current selection, and changes its color.
    /// </summary>
    public void AddGraphpointToSelection(CombinedGraph.CombinedGraphPoint graphPoint, int newGroup, bool hapticFeedback)
    {
        AddGraphpointToSelection(graphPoint, currentColorIndex, true, Colors[newGroup]);
        //Debug.Log("Adding gp to sel. Inform clients.");
        //gameManager.InformSelectedAdd(graphPoint.GraphName, graphPoint.label, newGroup, Colors[newGroup]);
    }



    /// <summary>
    /// Adds a graphpoint to the current selection, and changes its color.
    /// </summary>
    public void AddGraphpointToSelection(CombinedGraph.CombinedGraphPoint graphPoint, int newGroup, bool hapticFeedback, Color color)
    {
        // print(other.gameObject.name);
        if (graphPoint == null)
        {
            return;
        }
        // more_cells if (CurrentFilter != null && !CurrentFilter.Pass(graphPoint)) return;

        int oldGroup = graphPoint.group;

        if (newGroup < Colors.Length && color.Equals(Colors[newGroup]))
        {
            foreach (CombinedGraph graph in graphManager.graphs)
            {
                CombinedGraph.CombinedGraphPoint gp = graphManager.FindGraphPoint(graph.GraphName, graphPoint.Label);
                graphPoint.RecolorSelectionColor(newGroup, true);
            }
            //graphPoint.Recolor(Colors[newGroup], newGroup);
        }
        else
        {
            graphPoint.RecolorSelectionColor(newGroup, true);
        }
        graphPoint.group = newGroup;
        // renderer.material.color = Colors[newGroup];
        //gameManager.InformGraphPointChangedColor(graphPoint.GraphName, graphPoint.Label, color);

        bool newNode = !graphPoint.unconfirmedInSelection;
        graphPoint.unconfirmedInSelection = true;
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

            if (hapticFeedback && hapticFeedbackThisFrame)
            {
                hapticFeedbackThisFrame = false;
                SteamVR_Controller.Input((int)rightController.index).TriggerHapticPulse(hapticIntensity);
            }

            groupInfoDisplay.ChangeGroupsInfo(newGroup, 1);
            HUDGroupInfoDisplay.ChangeGroupsInfo(newGroup, 1);
            FarGroupInfoDisplay.ChangeGroupsInfo(newGroup, 1);
            if (newNode)
            {
                //gameManager.InformSelectedAdd(graphPoint.GraphName, graphPoint.Label);
                if (selectedCells.Count == 0)
                {
                    CellexalEvents.SelectionStarted.Invoke();
                }
                selectedCells.Add(graphPoint);
            }
            else
            {
                // If graphPoint was reselected. Remove it and add again so it is moved to the end of the list.
                //selectedCells.Remove(graphPoint);
                selectedCells.Add(graphPoint);
                groupInfoDisplay.ChangeGroupsInfo(oldGroup, -1);
                HUDGroupInfoDisplay.ChangeGroupsInfo(oldGroup, -1);
                FarGroupInfoDisplay.ChangeGroupsInfo(oldGroup, -1);
            }
        }
    }
    /// <summary>
    /// If selecting from client then graphpoint to be added needs to be found by searching since it has not collided with selection tool.
    /// </summary>
    /// <param name="graphName">The name of the graph that the point is in.</param>
    /// <param name="label">The label of the cell (graphpoint)</param>
    /// <param name="newGroup">The group which the cell is to be added to. </param>
    /// <param name="color">Colour that the graphpoint should be coloured by.</param>
    public void DoClientSelectAdd(string graphName, string label, int newGroup, Color color)
    {
        // more_cells    GraphPoint gp = referenceManager.graphManager.FindGraphPoint(graphName, label);
        // more_cells    AddGraphpointToSelection(gp, newGroup, true, color);
    }

    /// <summary>
    /// Same as above but this function is called when coluring the cubes between same cells in different graphs.
    /// </summary>
    /// <param name="graphName"></param>
    /// <param name="label"></param>
    /// <param name="newGroup"></param>
    /// <param name="color"></param>
    /// <param name="cube">If a cube should be coloured as well as the graphpoint.</param>
    public void DoClientSelectAdd(string graphName, string label, int newGroup, Color color, bool cube)
    {
        // more_cells   GraphPoint gp = referenceManager.graphManager.FindGraphPoint(graphName, label);
        // more_cells   AddGraphpointToSelection(gp, newGroup, true, color);
        // more_cells   foreach (Selectable sel in gp.lineBetweenCellsCubes)
        // more_cells   {
        // more_cells       sel.GetComponent<Renderer>().material.color = color;
        // more_cells   }
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
        info.graphPoint.group = info.fromGroup;

        // if info.fromGroup != 1 then the outline should be drawn
        // more_cells info.graphPoint.SetOutLined(info.fromGroup != -1, info.fromGroup);

        groupInfoDisplay.ChangeGroupsInfo(info.toGroup, -1);
        HUDGroupInfoDisplay.ChangeGroupsInfo(info.toGroup, -1);
        FarGroupInfoDisplay.ChangeGroupsInfo(info.toGroup, -1);
        if (info.newNode)
        {
            selectedCells.Remove(info.graphPoint);
            info.graphPoint.ResetColor();
        }
        else
        {
            groupInfoDisplay.ChangeGroupsInfo(info.fromGroup, 1);
            HUDGroupInfoDisplay.ChangeGroupsInfo(info.fromGroup, 1);
            FarGroupInfoDisplay.ChangeGroupsInfo(info.fromGroup, 1);
            info.graphPoint.ResetColor();
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
        info.graphPoint.group = info.toGroup;
        info.graphPoint.RecolorSelectionColor(info.toGroup, info.toGroup != -1);
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

    // more_cells   public void SingleSelect(Collider other)
    // more_cells   {
    // more_cells       Color transparentColor = new Color(selectedColor.r, selectedColor.g, selectedColor.b);
    // more_cells       other.gameObject.GetComponent<Renderer>().material.color = transparentColor;
    // more_cells       GraphPoint gp = other.GetComponent<CombinedGraph.CombinedGraphPoint>();
    // more_cells       if (!selectedCells.Contains(gp))
    // more_cells       {
    // more_cells           selectedCells.Add(gp);
    // more_cells       }
    // more_cells       if (!selectionMade)
    // more_cells       {
    // more_cells           selectionMade = true;
    // more_cells           //UpdateButtonIcons();
    // more_cells       }
    // more_cells   }

    /// <summary>
    /// Adds rigidbody to all selected cells, making them fall to the ground.
    /// </summary>
    // more_cells   public void ConfirmRemove()
    // more_cells   {
    // more_cells       //GetComponent<AudioSource>().Play();
    // more_cells       foreach (GraphPoint other in selectedCells)
    // more_cells       {
    // more_cells           other.transform.parent = null;
    // more_cells           other.gameObject.AddComponent<Rigidbody>();
    // more_cells           other.GetComponent<Rigidbody>().useGravity = true;
    // more_cells           other.GetComponent<Rigidbody>().isKinematic = false;
    // more_cells           other.GetComponent<Collider>().isTrigger = false;
    // more_cells           other.SetOutLined(false, -1);
    // more_cells       }
    // more_cells       selectionHistory.Clear();
    // more_cells       CellexalEvents.SelectionCanceled.Invoke();
    // more_cells       selectedCells.Clear();
    // more_cells       selectionMade = false;
    // more_cells       //selectionToolMenu.RemoveSelection();
    // more_cells   }

    /// <summary>
    /// Confirms a selection and dumps the relevant data to a .txt file.
    /// </summary>
    [ConsoleCommand("selectionToolHandler", "confirmselection", "confirm")]
    public void ConfirmSelection()
    {
        if (selectedCells.Count == 0)
        {
            print("empty selection confirmed");
        }
        // create .txt file with latest selection
        DumpSelectionToTextFile();
        lastSelectedCells.Clear();
        IEnumerable<CombinedGraph.CombinedGraphPoint> uniqueCells = selectedCells.Reverse<CombinedGraph.CombinedGraphPoint>().Distinct().Reverse() ;
        foreach (CombinedGraph.CombinedGraphPoint gp in uniqueCells)
        {
            // more_cells   if (gp.CustomColor)
            // more_cells       gp.SetOutLined(false, gp.Material.color);
            // more_cells   else
            // more_cells       gp.SetOutLined(false, gp.CurrentGroup);
            lastSelectedCells.Add(gp);
            gp.unconfirmedInSelection = false;
        }
        //previousSelectionMenu.CreateButton(selectedCells);
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
        string rScriptFilePath = (Application.streamingAssetsPath + @"\R\update_grouping.R").FixFilePath();
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
    public List<CombinedGraph.CombinedGraphPoint> GetLastSelection()
    {
        return lastSelectedCells;
    }

    /// <summary>
    /// Get the current (not yet confirmed) selection.
    /// </summary>
    /// <returns> A List of all graphpoints currently selected. </returns>
    public List<CombinedGraph.CombinedGraphPoint> GetCurrentSelection()
    {
        return selectedCells;
    }

    /// <summary>
    /// Unselects anything selected.
    /// </summary>
    [ConsoleCommand("selectionToolHandler", "cancelselection", "cs")]
    public void CancelSelection()
    {
        foreach (CombinedGraph.CombinedGraphPoint other in selectedCells)
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
    public void DumpSelectionToTextFile()
    {
        DumpSelectionToTextFile(selectedCells);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="selection"></param>
    public void DumpSelectionToTextFile(List<CombinedGraph.CombinedGraphPoint> selection)
    {
        // print(new System.Diagnostics.StackTrace());
        string filePath = CellexalUser.UserSpecificFolder + "\\selection" + (fileCreationCtr++) + ".txt";
        using (StreamWriter file =
                    new StreamWriter(filePath))
        {
            CellexalLog.Log("Dumping selection data to " + CellexalLog.FixFilePath(filePath));
            CellexalLog.Log("\tSelection consists of  " + selection.Count + " points");
            if (selectionHistory != null)
                CellexalLog.Log("\tThere are " + selectionHistory.Count + " entries in the history");
            foreach (CombinedGraph.CombinedGraphPoint gp in selection)
            {
                file.Write(gp.Label);
                file.Write("\t");
                Color c = gp.GetColor();
                int r = (int)(c.r * 255);
                int g = (int)(c.g * 255);
                int b = (int)(c.b * 255);
                // writes the color as #RRGGBB where RR, GG and BB are hexadecimal values
                file.Write(string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b));
                file.Write("\t");
                file.Write(gp.parent.GraphName);
                file.Write("\t");
                file.Write(gp.group);
                file.WriteLine();
            }
            file.Flush();
            file.Close();
        }
        StartCoroutine(UpdateRObjectGrouping());
    }

    /// <summary>
    /// Activates or deactivates all colliders on the selectiontool.
    /// </summary>
    /// <param name="enabled"> True if the selection tool should be activated, false if it should be deactivated. </param>
    /// <param name="meshIndex">The index of the collider that should be activated, if <paramref name="enabled"/> is <code>true</code>.</param>
    public void SetSelectionToolEnabled(bool enabled, int meshIndex)
    {
        currentMeshIndex = meshIndex;
        if (enabled)
        {
            controllerModelSwitcher.SwitchControllerModelColor(Colors[currentColorIndex]);
        }
        if (!enabled)
        {
            particles.gameObject.SetActive(false);
            //controllerModelSwitcher.rightControllerBody.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
        }
        if (selActive)
        {
            particles.gameObject.SetActive(true);
            var main = particles.main;
            main.startColor = Colors[currentColorIndex];
            //controllerModelSwitcher.rightControllerBody.GetComponent<Renderer>().material.SetColor("_EmissionColor", Colors[currentColorIndex]);
            //controllerModelSwitcher.rightControllerBody.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        }
        for (int i = 0; i < selectionToolColliders.Length; ++i)
        {
            // if we are turning on the selection tool, enable the collider with the corresponding index as the mesh and disable the other colliders.
            selectionToolColliders[i].enabled = enabled && selActive && meshIndex == i;
        }

    }

    void ActivateSelection(bool sel)
    {
        selActive = sel;
        SetSelectionToolEnabled(true, currentMeshIndex);
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
