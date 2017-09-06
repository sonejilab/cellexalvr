using System;
using System.IO;
using UnityEngine;

/// <summary>
/// This class is the thread that runs the R script that generates the heatmap image
/// </summary>
public class GenerateHeatmapThread
{

    private SelectionToolHandler selectionToolHandler;

    public GenerateHeatmapThread(SelectionToolHandler sth)
    {
        selectionToolHandler = sth;
    }
    [Obsolete]
    public void GenerateHeatmap()
    {
        string home = Directory.GetCurrentDirectory();
        using (StreamReader r = new StreamReader(home + "/Assets/Config/config.txt"))
        {
            string rawInput = r.ReadToEnd();
            string[] input = rawInput.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string rPath = input[0];
            //Debug.Log("R Out: " + RScriptRunner.RunFromCmd(home + @"\Assets\Scripts\R\make_heatmap.R", rPath, home + " " + selectionToolHandler.DataDir + " " + (selectionToolHandler.fileCreationCtr - 1)));
        }
    }
}
