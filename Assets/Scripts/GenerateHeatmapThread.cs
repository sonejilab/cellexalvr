using System;
using System.IO;
using UnityEngine;

public class GenerateHeatmapThread {
    //public static int latestSelection = 0;
    private SelectionToolHandler selectionToolHandler;

    public GenerateHeatmapThread (SelectionToolHandler sth)
    {
        selectionToolHandler = sth;
    }
    public void GenerateHeatmap()
    {
        string home = Directory.GetCurrentDirectory();
		using (StreamReader r = new StreamReader(home + "/Assets/Config/config.txt"))
        {
	        // Debug.Log ("R start");
            string rawInput = r.ReadToEnd();
            string[] input = rawInput.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string rPath = input[0];
            //Debug.Log(home);
            // Debug.Log (home + @"\Assets\Scripts\R\Make_heatmap_from_CellID_list_wColourBars_ANOVA.R " + rPath + " "+ home + " " + (selectionToolHandler.fileCreationCtr - 1));
            Debug.Log("R Out: " + RScriptRunner.RunFromCmd(home + @"\Assets\Scripts\R\Make_heatmap_from_CellID_list_wColourBars_ANOVA.R", rPath, home + " " + (selectionToolHandler.fileCreationCtr - 1)));
            //latestSelection++;
			// Debug.Log ("R done ");
        }
    }
}
