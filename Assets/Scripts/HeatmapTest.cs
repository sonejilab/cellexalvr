using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class HeatmapTest : MonoBehaviour {
    
    public GameObject HeatmapImageBoard;
    private ArrayList data;
    private GenerateHeatmapThread ght;
    private Thread t;
    private bool running;

	// Use this for initialization
	void Start () {
        ght = new GenerateHeatmapThread();
        t = null;
        running = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("n"))
        {
            t = new Thread(new ThreadStart(ght.generateHeatmap));
            t.Start();
            running = true;
        }
        if(running && !t.IsAlive) {
            HeatmapImageBoard.GetComponent<ChangeImage>().updateImage("Assets/Images/heatmap.png");
            running = false;
        }
    }
}
