using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.Extensions;
using CellexalVR.Filters;
using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Menu.Buttons;
using CellexalVR.Menu.SubMenus;
using CellexalVR.Multiuser;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{

    /// <summary>
    /// Represents the selection tool that can be used to select multiple GraphPoints.
    /// </summary>
    public class SelectionManager : MonoBehaviour
    {
        private GraphManager graphManager;
        private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        // the cells selected so far, maps a cell to its index in the selection. not all indices are always present, there will be gaps if cells have changed group.
        private List<Graph.GraphPoint> selectedCells = new List<Graph.GraphPoint>();
        private Queue<Graph.GraphPoint> cellsToMove = new Queue<Graph.GraphPoint>();
        private int previousMoveIndex = -1;
        private int movesCapacity = 10000000;
        private int[] groupChanges;
        private bool selectionMade = false;
        private SelectionToolCollider selectionToolCollider;

        [HideInInspector]
        public string DataDir { get; set; }
        public List<HistoryListInfo> selectionHistory = new List<HistoryListInfo>();
        // the number of steps we have taken back in the history.
        private int historyIndexOffset;
        private MultiuserMessageSender multiuserMessageSender;
        private FilterManager filterManager;
        private CellexalButton confirmSelectionButton;
        private List<Selection> selections = new List<Selection>();
        private Dictionary<string, Selection> selectionStringMapping = new Dictionary<string, Selection>();
        private Queue<Selection> loadedSelections = new Queue<Selection>();
        private int maxLoadedSelections = 5;

        public bool RObjectUpdating { get; private set; }

        /// <summary>
        /// Helper struct for remembering history when selecting graphpoints.
        /// </summary>
        public struct HistoryListInfo
        {
            // the graphpoint this affects
            public Graph.GraphPoint graphPoint;

            // the group it was given, -1 means no group
            public int toGroup;

            // the group it had before, -1 means no group
            public int fromGroup;

            // true if this graphpoint was previously not in the list of selected graphpoints
            public bool newNode;

            public HistoryListInfo(Graph.GraphPoint graphPoint, int toGroup, int fromGroup, bool newNode)
            {
                this.graphPoint = graphPoint;
                this.toGroup = toGroup;
                this.fromGroup = fromGroup;
                this.newNode = newNode;
            }
        }

        private void Awake()
        {
            CellexalEvents.ConfigLoaded.AddListener(OnConfigLoaded);
        }

        private void Start()
        {
            graphManager = ReferenceManager.instance.graphManager;
            rightController = ReferenceManager.instance.rightController;
            multiuserMessageSender = ReferenceManager.instance.multiuserMessageSender;
            selectionToolCollider = ReferenceManager.instance.selectionToolCollider;
            filterManager = ReferenceManager.instance.filterManager;
            confirmSelectionButton = ReferenceManager.instance.selectionMenu.transform.Find("Confirm Selection Button").GetComponent<CellexalButton>();
            //CellexalEvents.GraphsColoredByGene.AddListener(Clear);
            //CellexalEvents.GraphsColoredByIndex.AddListener(Clear);
            CellexalEvents.GraphsReset.AddListener(Clear);
            CellexalEvents.GraphCreated.AddListener(AssertGroupMasksExist);
        }

        private void Update()
        {
            if (cellsToMove.Count > 0)
            {
                if (previousMoveIndex < 0 || previousMoveIndex >= selectedCells.Count)
                {
                    previousMoveIndex = selectedCells.Count / 2;
                }

                int moveCost = 0;
                while (moveCost < movesCapacity && cellsToMove.Count > 0)
                {
                    Graph.GraphPoint point = cellsToMove.Dequeue();
                    int index = PingPongSearch(selectedCells, point, previousMoveIndex);
                    selectedCells.RemoveAt(index);
                    selectedCells.Add(point);
                    moveCost += selectedCells.Count;
                }

                if (cellsToMove.Count > 0)
                {
                    confirmSelectionButton.SetButtonActivated(false);
                    ReferenceManager.instance.cellsEvaluatingText.text = cellsToMove.Count.ToString();
                }
                else
                {
                    confirmSelectionButton.SetButtonActivated(true);
                    ReferenceManager.instance.cellsEvaluatingText.text = "";
                }
            }
        }

        /// <summary>
        /// Searches a list for a graphpoint by starting at an index and searching forwards and backwards from that index.
        /// </summary>
        /// <param name="list">The list of graphpoints to search.</param>
        /// <param name="point">The graphpoint to search for.</param>
        /// <param name="startIndex">An index to guess where to start.</param>
        /// <returns>The index of <paramref name="point"/>, if it is present in the list. -1 otherwise.</returns>
        private int PingPongSearch(List<Graph.GraphPoint> list, Graph.GraphPoint point, int startIndex)
        {
            if (list[startIndex] == point)
            {
                return startIndex;
            }

            int range = 1;
            int maxRange = list.Count / 2;
            int forwardIndex = startIndex + range;
            int backwardIndex = startIndex - range;
            while (range <= maxRange)
            {
                if (forwardIndex >= list.Count)
                {
                    // loop around if we hit the end of the list
                    forwardIndex -= list.Count;
                }

                if (list[forwardIndex] == point)
                {
                    return forwardIndex;
                }

                if (backwardIndex < 0)
                {
                    // loop around if we hit the start of the list
                    backwardIndex += list.Count;
                }

                if (list[backwardIndex] == point)
                {
                    return backwardIndex;
                }

                forwardIndex++;
                backwardIndex--;
                range++;
            }

            return -1;
        }

        private void OnConfigLoaded()
        {
            groupChanges = new int[CellexalConfig.Config.SelectionToolColors.Length];
        }

        /// <summary>
        /// Attempts to find a selection by a name or id.
        /// </summary>
        /// <param name="nameOrId">The name or id to look for.</param>
        /// <returns>A selection matching the name or id, or null if no suitable selection was found.</returns>
        public Selection FindSelectionByNameOrId(string nameOrId)
        {
            if (int.TryParse(nameOrId, out int id))
            {
                if (id >= 0 && id < selections.Count)
                {
                    return selections[id];
                }
            }
            if (selectionStringMapping.ContainsKey(nameOrId))
            {
                return selectionStringMapping[nameOrId];
            }
            return null;
        }

        public void RemoveGraphpointFromSelection(Graph.GraphPoint graphPoint)
        {

            ReferenceManager.instance.multiuserMessageSender.SendMessageSelectedRemove(graphPoint.parent.GraphName, graphPoint.Label);
            RemoveGraphpointFromSelection(graphPoint.parent.GraphName, graphPoint.Label);
        }

        public void RemoveGraphpointFromSelection(string graphName, string graphpointLabel)
        {
            Graph parentGraph = graphManager.FindGraph(graphName);
            Graph.GraphPoint graphPoint = parentGraph.FindGraphPoint(graphpointLabel);

            if (graphPoint == null || graphPoint.Group == -1)
            {
                return;
            }


            if (parentGraph.tag.Equals("Untagged"))
            {
                GraphBetweenGraphs ctcGraph = parentGraph.GetComponent<GraphBetweenGraphs>();
                graphPoint = ctcGraph.graph1.FindGraphPoint(graphPoint.Label);
            }

            int oldGroup = graphPoint.Group;

            foreach (Graph graph in graphManager.Graphs)
            {
                graph.FindGraphPoint(graphPoint.Label)?.ResetColor();
            }

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

            selectionHistory.Add(new HistoryListInfo(graphPoint, -1, oldGroup, newNode));

            LegendManager legendManager = ReferenceManager.instance.legendManager;
            legendManager.desiredLegend = LegendManager.Legend.SelectionLegend;
            if (legendManager.currentLegend != legendManager.desiredLegend)
            {
                legendManager.ActivateLegend(legendManager.desiredLegend);
            }

            if (!newNode)
            {
                legendManager.selectionLegend.AddOrUpdateEntry(oldGroup.ToString(), -1, Color.white);
            }

            selectedCells.Remove(graphPoint);

            bool hapticFeedback = true;
            if (hapticFeedback && selectionToolCollider.hapticFeedbackThisFrame)
            {
                selectionToolCollider.hapticFeedbackThisFrame = false;
                // SteamVR_Controller.Input((int) rightController.index).TriggerHapticPulse(hapticIntensity);
            }

            if (selectedCells.Count == 0)
            {
                CellexalEvents.GraphsReset.Invoke();
            }
        }

        /// <summary>
        /// Adds a graphpoint to the current selection, and changes its color.
        /// </summary>
        public void AddGraphpointToSelection(Graph.GraphPoint graphPoint, int newGroup, bool hapticFeedback)
        {
            if (filterManager.currentFilter != null)
            {
                ReferenceManager.instance.filterManager.AddCellToEval(graphPoint, newGroup);
            }

            else
            {
                AddGraphpoint(graphPoint, newGroup, hapticFeedback, selectionToolCollider.Colors[newGroup]);
                multiuserMessageSender.SendMessageSelectedAdd(graphPoint.parent.GraphName, graphPoint.Label, newGroup, selectionToolCollider.Colors[newGroup]);

            }
        }

        /// <summary>
        /// Adds a graphpoint to the current selection, and changes its color.
        /// </summary>
        public void AddGraphpointToSelection(Graph.GraphPoint graphPoint, int newGroup, bool hapticFeedback, Color color)
        {
            AddGraphpoint(graphPoint, newGroup, hapticFeedback, color);
            multiuserMessageSender.SendMessageSelectedAdd(graphPoint.parent.GraphName, graphPoint.Label, newGroup, color);
        }

        private void AddGraphpoint(Graph.GraphPoint graphPoint, int newGroup, bool hapticFeedback, Color color)
        {
            if (graphPoint == null)
            {
                return;
            }

            Graph parentGraph = graphPoint.parent;
            if (parentGraph.tag.Equals("Untagged"))
            {
                GraphBetweenGraphs ctcGraph = parentGraph.GetComponent<GraphBetweenGraphs>();
                graphPoint = ctcGraph.graph1.FindGraphPoint(graphPoint.Label);
            }

            int oldGroup = graphPoint.Group;
            foreach (Graph graph in graphManager.Graphs)
            {
                Graph.GraphPoint gp = graphManager.FindGraphPoint(graph.GraphName, graphPoint.Label);
                if (gp != null)
                {
                    gp.ColorSelectionColor(newGroup, true);
                }
            }

            graphPoint.Group = newGroup;

            bool newNode = !graphPoint.unconfirmedInSelection;
            graphPoint.unconfirmedInSelection = true;
            selectedCells.Add(graphPoint);
            CullExcessHistory();

            if (!selectionMade)
            {
                // if this is a new selection we should reset some stuff
                selectionMade = true;
                // turn on the undo buttons
                CellexalEvents.BeginningOfHistoryLeft.Invoke();
            }

            // The user might select cells that already have that color

            selectionHistory.Add(new HistoryListInfo(graphPoint, newGroup, oldGroup, newNode));
            LegendManager legendManager = ReferenceManager.instance.legendManager;
            legendManager.desiredLegend = LegendManager.Legend.SelectionLegend;
            if (legendManager.currentLegend != legendManager.desiredLegend)
            {
                legendManager.ActivateLegend(legendManager.desiredLegend);
            }

            legendManager.selectionLegend.AddOrUpdateEntry(newGroup.ToString(), 1, color);
            if (!newNode)
            {
                legendManager.selectionLegend.AddOrUpdateEntry(oldGroup.ToString(), -1, Color.white);
            }

            if (newNode)
            {
                if (selectedCells.Count == 0)
                {
                    CellexalEvents.SelectionStarted.Invoke();
                }
            }
            else
            {
                cellsToMove.Enqueue(graphPoint);
            }

            if (hapticFeedback && selectionToolCollider.hapticFeedbackThisFrame)
            {
                selectionToolCollider.hapticFeedbackThisFrame = false;
                rightController.SendHapticImpulse(0.2f, 0.05f);
            }
        }

        public void AddGraphPointsToSelection(IEnumerable<Graph.GraphPoint> points, int group)
        {
            AddGraphPointsToSelection(points, group, CellexalConfig.Config.SelectionToolColors[group]);
        }

        private void AddGraphPointsToSelection(IEnumerable<Graph.GraphPoint> points, int newGroup, Color color)
        {
            if (points.Count() == 0)
            {
                return;
            }
            Graph parentGraph = points.First().parent;
            List<Graph.GraphPoint> pointsInOtherGraph = new List<Graph.GraphPoint>();
            foreach (Graph graph in graphManager.Graphs)
            {
                if (graph != parentGraph)
                {
                    pointsInOtherGraph.Clear();
                    pointsInOtherGraph.AddRange(points.Select((p) => graph.FindGraphPoint(p.Label)));
                    graph.ColorGraphPointsSelectionColor(pointsInOtherGraph, newGroup, true);
                }
                else
                {
                    graph.ColorGraphPointsSelectionColor(points, newGroup, true);
                }
            }

            CullExcessHistory();

            if (!selectionMade)
            {
                // if this is a new selection we should reset some stuff
                selectionMade = true;
                // turn on the undo buttons
                CellexalEvents.BeginningOfHistoryLeft.Invoke();
            }
            LegendManager legendManager = ReferenceManager.instance.legendManager;
            if (legendManager.currentLegend != legendManager.desiredLegend)
            {
                legendManager.ActivateLegend(legendManager.desiredLegend);
            }

            for (int i = 0; i < groupChanges.Length; ++i)
            {
                groupChanges[i] = 0;
            }

            foreach (Graph.GraphPoint point in points)
            {
                int oldGroup = point.Group;
                point.Group = newGroup;
                bool newNode = !point.unconfirmedInSelection;
                point.unconfirmedInSelection = true;
                selectionHistory.Add(new HistoryListInfo(point, newGroup, oldGroup, newNode));
                groupChanges[point.Group]++;
                if (!newNode)
                {
                    groupChanges[oldGroup]--;
                    cellsToMove.Enqueue(point);
                }
                else
                {
                    selectedCells.Add(point);
                }
            }

            for (int i = 0; i < groupChanges.Length; ++i)
            {
                if (groupChanges[i] != 0)
                {
                    legendManager.selectionLegend.AddOrUpdateEntry(i.ToString(), groupChanges[i], CellexalConfig.Config.SelectionToolColors[i]);
                }
            }
        }

        private void CullExcessHistory()
        {
            if (historyIndexOffset != 0)
            {
                // if we have undone some selected graphpoints, then they should be removed from the history
                selectionHistory.RemoveRange(selectionHistory.Count - historyIndexOffset, historyIndexOffset);
                historyIndexOffset = 0;
                // turn off the redo buttons
                CellexalEvents.EndOfHistoryReached.Invoke();
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
            Graph.GraphPoint gp = ReferenceManager.instance.graphManager.FindGraphPoint(graphName, label);
            AddGraphpoint(gp, newGroup, true, color);
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
            Graph.GraphPoint gp = ReferenceManager.instance.graphManager.FindGraphPoint(graphName, label);
            AddGraphpoint(gp, newGroup, true, color);
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
            }

            else if (indexToMoveTo < 0)
            {
                // no more history
                return;
            }

            HistoryListInfo info = selectionHistory[indexToMoveTo];
            foreach (Graph graph in graphManager.Graphs)
            {
                Graph.GraphPoint gp = graphManager.FindGraphPoint(graph.GraphName, info.graphPoint.Label);
                if (gp is not null)
                {
                    gp.ColorSelectionColor(info.fromGroup, !info.newNode);
                }
            }
            ReferenceManager.instance.legendManager.selectionLegend.AddOrUpdateEntry(info.toGroup.ToString(), -1, selectionToolCollider.Colors[info.toGroup]);

            if (info.newNode)
            {
                selectedCells.Remove(info.graphPoint);
                info.graphPoint.unconfirmedInSelection = false;
            }
            else
            {
                ReferenceManager.instance.legendManager.selectionLegend.AddOrUpdateEntry(info.fromGroup.ToString(), 1, selectionToolCollider.Colors[info.fromGroup]);
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
            foreach (Graph graph in graphManager.Graphs)
            {
                Graph.GraphPoint gp = graphManager.FindGraphPoint(graph.GraphName, info.graphPoint.Label);
                gp.ColorSelectionColor(info.toGroup, info.toGroup != -1);
            }
            ReferenceManager.instance.legendManager.selectionLegend.AddOrUpdateEntry(info.toGroup.ToString(), 1, selectionToolCollider.Colors[info.toGroup]);

            if (info.newNode)
            {
                selectedCells.Add(info.graphPoint);
                info.graphPoint.unconfirmedInSelection = true;
            }

            else
            {
                ReferenceManager.instance.legendManager.selectionLegend.AddOrUpdateEntry(info.fromGroup.ToString(), -1, selectionToolCollider.Colors[info.fromGroup]);
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
            Color color = group != -1 ? selectionToolCollider.Colors[group] : Color.white;

            Color nextColor;
            do

            {
                GoBackOneStepInHistory();
                indexToMoveTo--;
                if (indexToMoveTo >= 0)
                {
                    nextColor = selectionToolCollider.Colors[selectionHistory[indexToMoveTo].toGroup];
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
            Color color = group != -1 ? selectionToolCollider.Colors[group] : Color.white;

            Color nextColor;
            do

            {
                GoForwardOneStepInHistory();
                indexToMoveTo++;
                if (indexToMoveTo < selectionHistory.Count)
                {
                    nextColor = selectionToolCollider.Colors[selectionHistory[indexToMoveTo].toGroup];
                }
                else
                {
                    break;
                }
            } while (color.Equals(nextColor));
        }

        public void ConfirmSelectionForBigFolder()
        {
            TextureHandler.instance.sps.Clear();

            CellexalEvents.SelectionConfirmed.Invoke();
            CellexalEvents.CommandFinished.Invoke(true);
        }

        /// <summary>
        /// Confirms a selection and dumps the relevant data to a .txt file.
        /// </summary>
        [ConsoleCommand("selectionManager", aliases: new string[] { "confirmselection", "confirm" })]
        public void ConfirmSelectionConsole()
        {
            if (PointCloudGenerator.instance.pointClouds.Count > 0)
            {
                ConfirmSelectionForBigFolder();
            }
            else
            {
                ConfirmSelection();
            }
        }

        public void ConfirmSelection()
        {
            foreach (Graph graph in graphManager.Graphs)
            {
                graph.octreeRoot.Group = -1;
            }

            if (selectedCells.Count == 0)
            {
                print("empty selection confirmed");
                return;
            }

            Selection newSelection = new Selection(selectedCells);
            selections.Add(newSelection);
            selectionStringMapping[newSelection.ToString()] = newSelection;

            foreach (Graph.GraphPoint gp in selectedCells)
            {
                foreach (Graph graph in graphManager.Graphs)
                {
                    Graph.GraphPoint graphPoint = graph.FindGraphPoint(gp.Label);
                    if (graphPoint != null)
                    {
                        graphPoint.ColorSelectionColor(gp.Group, false);
                    }
                }

                gp.unconfirmedInSelection = false;
            }

            selectionHistory.Clear();
            historyIndexOffset = 0;
            selectedCells.Clear();
            CellexalEvents.SelectionConfirmed.Invoke();
            selectionMade = false;

            StartCoroutine(UpdateRObjectGroupingCoroutine(newSelection));
            CellexalEvents.CommandFinished.Invoke(true);
        }

        public void AddSelection(Selection selection)
        {
            selections.Add(selection);
            selectionStringMapping[selection.ToString()] = selection;
        }


        public void UpdateRObjectGrouping(Selection selection)
        {
            StartCoroutine(UpdateRObjectGroupingCoroutine(selection));
        }

        private IEnumerator UpdateRObjectGroupingCoroutine(Selection selection)
        {
            RObjectUpdating = true;

            // wait one frame to let ConfirmSelection finish.
            yield return null;

            //string function = "userGrouping";
            string latestSelection = selection.savedSelectionFilePath.MakeDoubleBackslash();

            string args = CellexalUser.UserSpecificFolder.MakeDoubleBackslash() + " " + latestSelection;
            string rScriptFilePath = Path.Combine(Application.streamingAssetsPath, "R", "update_grouping.R");

            // Wait for server to start up and not be busy
            bool rServerReady = File.Exists(Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.pid")) &&
                                !File.Exists(Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.R")) &&
                                !File.Exists(Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.lock"));
            while (!rServerReady || !RScriptRunner.serverIdle)
            {
                rServerReady = File.Exists(Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.pid")) &&
                               !File.Exists(Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.R")) &&
                               !File.Exists(Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.lock"));
                yield return null;
            }

            Thread t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
            CellexalLog.Log("Updating R Object grouping at " + CellexalUser.UserSpecificFolder);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            t.Start();
            while (t.IsAlive || File.Exists(Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.R")))
            {
                yield return null;
            }

            stopwatch.Stop();
            CellexalLog.Log("Updating grouping R script finished in " + stopwatch.Elapsed.ToString());
            RObjectUpdating = false;
        }

        /// <summary>
        /// Selects all points in to current group. If filter is active it queues all points for evaluation. 
        /// </summary>
        public void SelectAll()
        {
            Graph g = graphManager.originalGraphs.Find(x => !x.GraphName.Contains("Slice"));
            foreach (Cell c in ReferenceManager.instance.cellManager.GetCells())
            {
                Graph.GraphPoint gp = g.FindGraphPoint(c.Label);
                if (filterManager.currentFilter != null)
                {
                    ReferenceManager.instance.filterManager.AddCellToEval(gp, selectionToolCollider.CurrentColorIndex);
                }
                else
                {
                    AddGraphpointToSelection(gp, selectionToolCollider.CurrentColorIndex, false);
                }
            }
        }

        /// <summary>
        /// Gets the last selection that was confirmed.
        /// </summary>
        /// <returns>An object of class <see cref="Selection"/>. Or null if no selections have been made this session.</returns>
        public Selection GetLastSelection()
        {
            if (selections.Count == 0)
            {
                return null;
            }
            return selections[^1];
        }

        /// <summary>
        /// Removes all points from the current selection, and clears the selection history.
        /// </summary>
        public void Clear()
        {
            selectionHistory.Clear();
            selectedCells.Clear();
            historyIndexOffset = 0;
            CellexalEvents.SelectionCanceled.Invoke();
        }

        /// <summary>
        /// Get the current (not yet confirmed) selection.
        /// </summary>
        /// <returns> A List of all graphpoints currently selected. </returns>
        public int GetCurrentSelectionSize()
        {
            return selectedCells.Count;
        }

        /// <summary>
        /// Unselects anything selected.
        /// </summary>
        [ConsoleCommand("selectionManager", aliases: new string[] { "cancelselection", "cs" })]
        public void CancelSelection()
        {
            int stepsToGoBack = selectionHistory.Count - historyIndexOffset;
            for (int i = 0;
                i <= stepsToGoBack;
                i++)
            {
                GoBackOneStepInHistory();
            }

            //foreach (Graph.GraphPoint other in selectedCells)
            //{
            //    other.ResetColor();
            //}
            historyIndexOffset = selectionHistory.Count;
            CellexalEvents.BeginningOfHistoryReached.Invoke();

            //selectedCells.Clear();
            selectionMade = false;
            CellexalEvents.CommandFinished.Invoke(true);
            CellexalEvents.SelectionCanceled.Invoke();
        }

        /// <summary>
        /// Loads a <see cref="Selection"/> from the disk, if the <see cref="Selection"/> is already loaded, the loaded object is returned.
        /// </summary>
        /// <param name="path">The path to the <see cref="Selection"/>. May point to either the <c>selection.txt</c> file or the selection's directory.</param>
        /// <returns>A <see cref="Selection"/> generated from the given path, or null if the path does not point to a valid <see cref="Selection"/>.</returns>
        public Selection LoadSelectionFromDisk(string path)
        {
            foreach (Selection selection in selections)
            {
                if (selection.savedSelectionDirectory.Equals(path) || selection.savedSelectionFilePath.Equals(path))
                {
                    return selection;
                }
            }

            Selection newSelection = new Selection(path);
            AddSelection(newSelection);
            return newSelection;
        }

        public void UnloadOldestSelection(Selection newestSelection)
        {
            loadedSelections.Enqueue(newestSelection);

            if (loadedSelections.Count <= maxLoadedSelections)
            {
                return;
            }

            loadedSelections.Dequeue().UnloadSelection();
        }

        public void AssertGroupMasksExist(Graph graph)
        {
            if (selections.Count == 0)
            {
                return;
            }

            if (!selections[0].allGroupsCombinedMask.ContainsKey(graph))
            {
                foreach (Selection selection in selections)
                {
                    selection.SaveGroupMasksToDisk(graph);
                }
            }
        }

        /// <summary>
        /// Helper function to recolour only the points that are in the current selection.
        /// </summary>
        public void RecolorSelectionPoints()
        {
            foreach (Graph.GraphPoint gp in selectedCells)
            {
                foreach (Graph.GraphPoint g in ReferenceManager.instance.cellManager.GetCell(gp.Label).GraphPoints)
                {
                    g.ColorSelectionColor(gp.Group, true);
                }
            }
        }


        /// <summary>
        /// Gets a selection colors.
        /// </summary>
        /// <param name="index">The index of the color. Indices outside the range of the array colors will be corrected with remainder division.</param>
        public Color GetColor(int index)
        {
            return CellexalConfig.Config.SelectionToolColors[index % CellexalConfig.Config.SelectionToolColors.Length];
        }
    }
}