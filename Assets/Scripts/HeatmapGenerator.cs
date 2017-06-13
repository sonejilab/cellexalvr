using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class HeatmapGenerator : MonoBehaviour {

	public SelectionToolHandler selectionToolHandler; // use to be: public CellSelector cellselector;
   public GameObject heatmapImageBoard;
	public ErrorMessageController errorMessageController;

	private ArrayList data;
   private GenerateHeatmapThread ght;
   private Thread t;
   private bool running;
	private SteamVR_TrackedObject trackedObject;
	private SteamVR_Controller.Device device;
	// private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
	// private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
	private GameObject hourglass;

	private GameObject heatBoard;
	private int heatmapID = 1;
	private string filePath;
	private Vector3 heatmapPosition;
	// private Quaternion heatmapRotation;
	private float heatmapRotationAngle = 0.0f;
	private ArrayList heatmapList;
	// public AudioSource heatmapCreated;

	// Use this for initialization
	void Start () {
		trackedObject = GetComponent<SteamVR_TrackedObject> ();
   	t = null;
      running = false;
		hourglass = GameObject.Find ("WaitingForHeatboardHourglass");
		hourglass.SetActive (false);
      ght = new GenerateHeatmapThread(selectionToolHandler);
		heatmapList = new ArrayList ();
		heatmapPosition = heatmapImageBoard.transform.position;
      //heatmapRotation = heatmapImageBoard.transform.rotation;
   }

   // Update is called once per frame
   void Update() {
		device = SteamVR_Controller.Input ((int)trackedObject.index);
		if (!running && selectionToolHandler.selectionConfirmed && !selectionToolHandler.GetHeatmapCreated()) {
         //if (device.GetPress (SteamVR_Controller.ButtonMask.Grip)) {
	         if (device.GetPressDown (SteamVR_Controller.ButtonMask.Trigger)) {
	            ArrayList selection = selectionToolHandler.GetLastSelection();
	            //print(selection.Count);
					if (selection.Count < 2) {
	               return;
	            }
					Color c1 = ((GraphPoint) selection[0]).GetComponent<Renderer>().material.color;
	            bool colorFound = false;
					for (int i = 1; i < selection.Count; ++i) {
	               //print(((GraphPoint) selection[i]).GetComponent<Renderer>().material.color);
						Color c2 = ((GraphPoint)selection[i]).GetComponent<Renderer>().material.color;
						if (!((c1.r == c2.r) && (c1.g == c2.g) && (c1.b == c2.b))) {
	                  colorFound = true;
	                  break;
	               }
	            }
					if (!colorFound) {
						//print("you must select more than one color!");
	               errorMessageController.DisplayErrorMessage(3);
						return;
	            }
					hourglass.SetActive(true);
	            //heatmapPosition = new Vector3(heatmapPosition.x, heatmapPosition.y, heatmapPosition.z + 3.0f);
	            //heatmapPosition = heatmapPosition + new Vector3 (0, 0, 3.0f);
	            //print (heatmapPosition);
	         	heatmapRotationAngle += 90.0f;
					//print(heatmapRotattionAngle);
					heatBoard = Instantiate (heatmapImageBoard, heatmapPosition, heatmapImageBoard.transform.rotation);
					heatmapList.Add (heatBoard);
					heatBoard.SetActive(true);
					heatmapPosition = heatmapPosition + new Vector3 (-0.2f, 0, 0);
					//heatBoard.transform.Rotate (new Vector3 (0, 0, heatmapRotattionAngle));
					//print (heatBoard.activeSelf.ToString());
					//hourglass = Instantiate (hourglass, heatmapPosition, new Quaternion(0, 0, 0, 0));
					//heatmapImageBoard.SetActive (false);
					//heatBoard.SetActive (false);
					hourglass.GetComponent<AudioSource> ().Play ();
					t = new Thread (new ThreadStart (ght.GenerateHeatmap));
					t.Start ();
					running = true;
				}
         //}
      }
      if (running && !t.IsAlive) {
         heatBoard.SetActive(true);
			hourglass.SetActive (false);
			hourglass.GetComponent<AudioSource> ().Stop ();
			//filePath = "Assets/Images/heatmap" + heatmapID + ".png";
			//print (filePath);
			heatBoard.GetComponent<ChangeImage>().UpdateImage("Assets/Images/heatmap.png");
         //heatBoard.SetActive (true);
			heatBoard.GetComponent<AudioSource>().Play();
			//heatmapCreated.Play();
			//heatmapImageBoard.GetComponent<ChangeImage>().updateImage("Assets/Images/heatmap.png");
			//heatmapImageBoard.SetActive (true);
			//heatmapImageBoard.GetComponent<AudioSource>().Play();
			running = false;
			heatmapID++;
      }
	}
}
