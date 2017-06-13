using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GenerateHeatmapThread {
    //public static int latestSelection = 0;

    public void GenerateHeatmap()
    {
        string home = Directory.GetCurrentDirectory();
		using (StreamReader r = new StreamReader(home + "/Assets/Config/config.txt"))
        {
			//Debug.Log ("R start");
            string rawInput = r.ReadToEnd();
            string[] input = rawInput.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string rPath = input[0];
            //Debug.Log(home);
            //Debug.Log (rPath);
            RScriptRunner.RunFromCmd(home + @"\Assets\Scripts\R\Make_heatmap_from_CellID_list_wColourBars_ANOVA.R", rPath, home + " " + (SelectionToolHandler.fileCreationCtr - 1));
            //latestSelection++;
			//Debug.Log ("R done ");
        }
    }
}
