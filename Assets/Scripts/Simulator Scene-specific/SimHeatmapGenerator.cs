using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class SimHeatmapGenerator : MonoBehaviour {

	public GameObject graph;
	// private SelectionToolHandler selectionToolHandler; // used to be: private CellSelector cellselector;
    public GameObject HeatmapImageBoard;
    private ArrayList data;
    private GenerateHeatmapThread ght;
    private Thread t;
    private bool running;

	// Use this for initialization
	void Start () {
		// selectionToolHandler = graph.GetComponent<SelectionToolHandler>();
        ght = new GenerateHeatmapThread(null);
        t = null;
        running = false;
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown("n")) {
            HeatmapImageBoard.SetActive(true);
            t = new Thread(new ThreadStart(ght.GenerateHeatmap));
            t.Start();
            running = true;
        }
        if(running && !t.IsAlive) {
            HeatmapImageBoard.GetComponent<ChangeImage>().UpdateImage("Assets/Images/heatmap.png");
            running = false;
        }
    }
}
