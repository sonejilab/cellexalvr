using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GenerateHeatmapThread {
    public void generateHeatmap()
    {
        string home = Directory.GetCurrentDirectory();
        string rPath;
        using (StreamReader r = new StreamReader(home + "/Assets/Config/config.txt"))
        {
            string input = r.ReadToEnd();
            rPath = input;
            Debug.Log("R result: " + RScriptRunner.RunFromCmd(home + "/Assets/Scripts/Make_heatmap_from_CellIS_list.R", rPath, ""));
        }
    }
}
