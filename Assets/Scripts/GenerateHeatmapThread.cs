using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GenerateHeatmapThread {

    public void generateHeatmap()
    {
        string home = Directory.GetCurrentDirectory();
		using (StreamReader r = new StreamReader(home + "/Assets/Config/config.txt"))
        {
            string rawInput = r.ReadToEnd();
            string[] input = rawInput.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            string rPath = input[0];
			Debug.Log (rPath);
			Debug.Log("R result: " + RScriptRunner.RunFromCmd(home + "/Assets/Scripts/R/Make_heatmap_from_CellID_list_wColourBars_ANOVA.R", rPath, home));
        }
    }
}
