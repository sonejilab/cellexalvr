using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HeatmapTest : MonoBehaviour {
    
    public GameObject HeatmapImageBoard;
    private ArrayList data;

	// Use this for initialization
	void Start () {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("n"))
        {
            runHeatmap();
            HeatmapImageBoard.GetComponent<ChangeImage>().updateImage("Assets/Images/heatmap.png");
        }
    }

    public void runHeatmap()
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
