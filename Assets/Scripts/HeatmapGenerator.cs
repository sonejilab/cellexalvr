using System.Collections;
using System.Threading;
using UnityEngine;

public class HeatmapGenerator : MonoBehaviour {

public SteamVR_TrackedObject trackedObject;
public SelectionToolHandler selectionToolHandler;
public GameObject heatmapPrefab;
public ErrorMessageController errorMessageController;
public GraphManager graphManager;
private ArrayList data;
private GenerateHeatmapThread ght;
private Thread t;
private bool running;
private SteamVR_Controller.Device device;
private GameObject hourglass;
private GameObject heatBoard;
private int heatmapID = 1;
private string filePath;
private Vector3 heatmapPosition;
private ArrayList heatmapList;

// Use this for initialization
void Start () {
	device = SteamVR_Controller.Input ((int)trackedObject.index);
	t = null;
	running = false;
	hourglass = GameObject.Find ("WaitingForHeatboardHourglass");
	hourglass.SetActive (false);
	ght = new GenerateHeatmapThread(selectionToolHandler);
	heatmapList = new ArrayList ();
	heatmapPosition = heatmapPrefab.transform.position;
}

// Update is called once per frame
void Update() {
	if (!running && selectionToolHandler.selectionConfirmed && !selectionToolHandler.GetHeatmapCreated()) {
		if (device.GetPressDown (SteamVR_Controller.ButtonMask.Trigger)) {
			ArrayList selection = selectionToolHandler.GetLastSelection();

			// Check if more than one color is selected
			if (selection.Count < 2) {
				return;
			}
			Color c1 = ((GraphPoint) selection[0]).GetComponent<Renderer>().material.color;
			bool colorFound = false;
			for (int i = 1; i < selection.Count; ++i) {
				Color c2 = ((GraphPoint)selection[i]).GetComponent<Renderer>().material.color;
				if (!((c1.r == c2.r) && (c1.g == c2.g) && (c1.b == c2.b))) {
					colorFound = true;
					break;
				}
			}
			if (!colorFound) {
				// Generate error message if less than two colors are selected
				errorMessageController.DisplayErrorMessage(3);
				return;
			}

			// Start generation of new heatmap in R
			t = new Thread (new ThreadStart (ght.GenerateHeatmap));
			t.Start ();
			running = true;

			// Show hourglass
			hourglass.SetActive(true);
			hourglass.GetComponent<AudioSource> ().Play ();
		}
	}

	if (running && !t.IsAlive) {
		heatmapPosition = heatmapPosition + new Vector3 (-0.2f, 0, 0);
		heatBoard = Instantiate(heatmapPrefab, heatmapPosition, heatmapPrefab.transform.rotation);
		Heatmap heatmap = heatBoard.GetComponent<Heatmap>();
		heatBoard.transform.parent = transform;
		heatmap.SetVars(graphManager, selectionToolHandler);
		heatmapList.Add (heatBoard);

		hourglass.SetActive (false);
		hourglass.GetComponent<AudioSource> ().Stop ();

		heatmap.UpdateImage("Assets/Images/heatmap.png");
		heatBoard.GetComponent<AudioSource>().Play();

		running = false;
		heatmapID++;
	}
}
}
