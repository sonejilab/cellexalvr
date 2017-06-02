using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class HeatmapGenerator : MonoBehaviour {

	public SelectionToolHandler selectionToolHandler; // use to be: public CellSelector cellselector;
    public GameObject heatmapImageBoard;
    private ArrayList data;
    private GenerateHeatmapThread ght;
    private Thread t;
    private bool running;
	private SteamVR_TrackedObject trackedObject;
	private SteamVR_Controller.Device device;
	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
	private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
	private GameObject hourglass;

	private GameObject heatBoard;
	private int heatmapID = 1;
	private string filePath;
	private Vector3 heatmapPosition;
	private Quaternion heatmapRotation;
	private float heatmapRotattionAngle = 0.0f;
	private ArrayList heatmapList;


	// Use this for initialization
	void Start () {
		trackedObject = GetComponent<SteamVR_TrackedObject> ();
        ght = new GenerateHeatmapThread();
        t = null;
        running = false;
		hourglass = GameObject.Find ("WaitingForHeatboardHourglass");
		hourglass.SetActive (false);

		heatmapList = new ArrayList ();

		heatmapPosition = heatmapImageBoard.transform.position;
		heatmapRotation = heatmapImageBoard.transform.rotation;



    }

    // Update is called once per frame
    void Update()
    {
		device = SteamVR_Controller.Input ((int)trackedObject.index);
		if (selectionToolHandler.selectionConfirmed) {
			//if (device.GetPress (SteamVR_Controller.ButtonMask.Grip)) {
				if (device.GetPressDown (SteamVR_Controller.ButtonMask.Trigger)) { 
					hourglass.SetActive (true);

					//heatmapPosition = new Vector3(heatmapPosition.x, heatmapPosition.y, heatmapPosition.z + 3.0f);
					//heatmapPosition = heatmapPosition + new Vector3 (0, 0, 3.0f);
					print (heatmapPosition);


					
					heatmapRotattionAngle += 90.0f;
					print(heatmapRotattionAngle);
					heatBoard = Instantiate (heatmapImageBoard, heatmapPosition, heatmapImageBoard.transform.rotation);
				heatmapList.Add (heatBoard);	
				heatBoard.active = true;
				heatmapPosition = heatmapPosition + new Vector3 (-0.2f, 0, 0);
					//heatBoard.transform.Rotate (new Vector3 (0, 0, heatmapRotattionAngle));
	
					print ("hallåhallå");
					print (heatBoard.activeSelf.ToString());
					//hourglass = Instantiate (hourglass, heatmapPosition, new Quaternion(0, 0, 0, 0));

					//heatmapImageBoard.SetActive (false);
					heatBoard.SetActive (false);
					

				hourglass.GetComponent<AudioSource> ().Play ();
					t = new Thread (new ThreadStart (ght.generateHeatmap));
					t.Start ();
					running = true;
				}
			//}
		}
        if(running && !t.IsAlive) {
			print ("hej");

			hourglass.SetActive (false);
			hourglass.GetComponent<AudioSource> ().Stop ();

			filePath = "Assets/Images/heatmap" + heatmapID + ".png";
			print (filePath);

			heatBoard.GetComponent<ChangeImage>().updateImage("Assets/Images/heatmap.png");
			heatBoard.SetActive (true);
			heatBoard.GetComponent<AudioSource>().Play();

			//heatmapImageBoard.GetComponent<ChangeImage>().updateImage("Assets/Images/heatmap.png");
			//heatmapImageBoard.SetActive (true);
			//heatmapImageBoard.GetComponent<AudioSource>().Play();

        
			running = false;
			heatmapID++;
        }
    }
}