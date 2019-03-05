using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Select 3 markers and create new graph with these as axes.
/// </summary>
public class NewGraphFromMarkers : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public List<string> markers;

    private string filePath;

    private void Start()
    {
        markers = new List<string>();
    }

    private void Update()
    {
        
    }

    [ConsoleCommand("newGraphFromMarkers", "newGraphFromMarkers", "ngfm")]
    public void CreateMarkerGraph()
    {
        DumpSelectionToTextFile(referenceManager.selectionToolHandler.GetLastSelection(), markers[0], markers[1], markers[2]);
        string[] files = new string[1];
        files[0] = filePath;
        referenceManager.inputReader.ReadCoordinates(CellexalUser.UserSpecificFolder, files);
        markers.Clear();
        foreach(AddMarkerButton b in GetComponentsInChildren<AddMarkerButton>())
        {
            b.activeOutline.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="selection"></param>
    public void DumpSelectionToTextFile(List<CombinedGraph.CombinedGraphPoint> selection, string first_marker,
                                        string second_marker, string third_marker)
    {
        // print(new System.Diagnostics.StackTrace());
        this.filePath = CellexalUser.UserSpecificFolder + "\\" + first_marker + "_" + second_marker + "_" + third_marker + ".txt";
        HashSet<string> previousLines = new HashSet<string>();
        using (StreamWriter file = new StreamWriter(filePath))
        {
            CellexalLog.Log("Dumping selection data to " + CellexalLog.FixFilePath(filePath));
            CellexalLog.Log("\tSelection consists of  " + selection.Count + " points");
            string header = "\t" + first_marker + "\t" + second_marker + "\t" + third_marker;
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

