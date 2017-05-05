using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class HeatmapGenerator : MonoBehaviour {

	public GameObject graph;
	private CellSelector cellselector;
    public GameObject HeatmapImageBoard;
    private ArrayList data;
    private GenerateHeatmapThread ght;
    private Thread t;
    private bool running;
	private SteamVR_TrackedObject trackedObject;
	private SteamVR_Controller.Device device;
	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

	// Use this for initialization
	void Start () {
		cellselector = graph.GetComponent<CellSelector>();
		trackedObject = GetComponent<SteamVR_TrackedObject> ();
        ght = new GenerateHeatmapThread();
        t = null;
        running = false;
    }

    // Update is called once per frame
    void Update()
    {
		device = SteamVR_Controller.Input ((int)trackedObject.index);
		if (device.GetPressDown (triggerButton)) {
			cellselector.dumpData();
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
