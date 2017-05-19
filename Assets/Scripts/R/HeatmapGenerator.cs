using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class HeatmapGenerator : MonoBehaviour {

	public SelectionToolHandler selectionToolHandler; // use to be: public CellSelector cellselector;
    public GameObject HeatmapImageBoard;
    private ArrayList data;
    private GenerateHeatmapThread ght;
    private Thread t;
    private bool running;
	private SteamVR_TrackedObject trackedObject;
	private SteamVR_Controller.Device device;
	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
	private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
	private GameObject hourglass;

	// Use this for initialization
	void Start () {
		trackedObject = GetComponent<SteamVR_TrackedObject> ();
        ght = new GenerateHeatmapThread();
        t = null;
        running = false;
		hourglass = GameObject.Find ("WaitingForHeatboardHourglass");
		hourglass.SetActive (false);
    }

    // Update is called once per frame
    void Update()
    {
		device = SteamVR_Controller.Input ((int)trackedObject.index);
		if (selectionToolHandler.selectionConfirmed) {
			if (device.GetPress (SteamVR_Controller.ButtonMask.Grip)) {
				if (device.GetPressDown (SteamVR_Controller.ButtonMask.Trigger)) { 
					hourglass.SetActive (true);
					HeatmapImageBoard.SetActive (false);
					hourglass.GetComponent<AudioSource> ().Play ();
					t = new Thread (new ThreadStart (ght.generateHeatmap));
					t.Start ();
					running = true;
				}
			}
		}
        if(running && !t.IsAlive) {
			hourglass.SetActive (false);
			hourglass.GetComponent<AudioSource> ().Stop ();
            HeatmapImageBoard.GetComponent<ChangeImage>().updateImage("Assets/Images/heatmap.png");
			HeatmapImageBoard.SetActive (true);
			HeatmapImageBoard.GetComponent<AudioSource>().Play();
            running = false;
        }
    }
}