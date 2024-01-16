﻿using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.Extensions;
using CellexalVR.General;
using CellexalVR.Menu.Buttons.Facs;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{

    /// <summary>
    /// Select 3 markers and create new graph with these as axes.
    /// </summary>
    public class NewGraphFromMarkers : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public List<string> markers;

        private string filePath;
        private bool useSelection;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            markers = new List<string>();
            CellexalEvents.SelectionConfirmed.AddListener(SwitchMode);
            CellexalEvents.GraphsReset.AddListener(SwitchMode);
        }

        private void Update()
        {

        }

        private void SwitchMode()
        {
            useSelection = !useSelection;
        }

        [ConsoleCommand("newGraphFromMarkers", aliases: new string[] { "newGraphFromMarkers", "ngfm" })]
        public void CreateMarkerGraphConsole(string marker1, string marker2, string marker3)
        {
            markers = new List<string>() { marker1, marker2, marker3 };
            CreateMarkerGraph();
        }

        public void CreateMarkerGraph(bool selection = false)
        {
            Selection lastSelection = referenceManager.selectionManager.GetLastSelection();
            if (lastSelection is not null)
            {
                DumpSelectionToTextFile(referenceManager.selectionManager.GetLastSelection().Points, markers[0], markers[1], markers[2]);
            }
            else
            {
                // TODO: make a new graph from a selection object, or an entire graph instead of saving things to a file and using inputreader.ReadGraphFromMarkerFile()
                DumpSelectionToTextFile(new List<Graph.GraphPoint>(referenceManager.graphManager.Graphs[0].points.Values), markers[0], markers[1], markers[2]);
            }
            referenceManager.inputReader.ReadGraphFromMarkerFile(CellexalUser.UserSpecificFolder, filePath);
            markers.Clear();
            foreach (AddMarkerButton b in GetComponentsInChildren<AddMarkerButton>())
            {
                b.ToggleOutline(false);
            }

            if (!referenceManager.sessionHistoryList.Contains(filePath, Definitions.HistoryEvent.FACSGRAPH))
            {
                referenceManager.sessionHistoryList.AddEntry(filePath, Definitions.HistoryEvent.FACSGRAPH);
            }
        }

        /// <summary>
        /// Dumping the selection with ids and facs values on the same format as the mds files so it can be read in by the inputreader.
        /// </summary>
        /// <param name="selection">The points to save to the file. Either a selection or all the points in the graph.</param>
        public void DumpSelectionToTextFile(List<Graph.GraphPoint> selection, string first_marker,
                                            string second_marker, string third_marker)
        {
            this.filePath = CellexalUser.UserSpecificFolder + "\\" + first_marker + "_" + second_marker + "_" + third_marker + ".txt";
            HashSet<string> previousLines = new HashSet<string>();
            using (StreamWriter file = new StreamWriter(filePath))
            {
                CellexalLog.Log("Dumping selection data to " + CellexalLog.FixFilePath(filePath));
                CellexalLog.Log("\tSelection consists of  " + selection.Count + " points");
                string header = "CellID\t" + first_marker + "\t" + second_marker + "\t" + third_marker;
                file.WriteLine(header);
                for (int i = 0; i < selection.Count; i++)
                {
                    string label = selection[i].Label;
                    Cell cell = referenceManager.cellManager.GetCell(label);
                    // Add returns true if it was actually added,
                    // false if it was already there
                    // Duplicate lines (coordinates) causes trouble when creating the meshes...
                    string currentLine = cell.FacsValue[first_marker.ToLower()] + "\t" + cell.FacsValue[second_marker.ToLower()]
                                            + "\t" + cell.FacsValue[third_marker.ToLower()];
                    if (previousLines.Add(currentLine))
                    {
                        file.Write(label);
                        file.Write("\t");
                        file.WriteLine(currentLine);
                    }
                }
                file.Flush();
                file.Close();
            }
        }
    }
}
