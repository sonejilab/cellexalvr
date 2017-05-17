using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using VRTK;

public class SelectionToolHandler : MonoBehaviour 
{
	public GraphManager manager;
	public Graph graph;
	public Material selectorMaterial;

	public RadialMenu menu;
	public Sprite noButton;
	public Sprite toolButton;
	public Sprite confirmButton;
	public Sprite confirmButton_w;
	public Sprite cancelButton;
	public Sprite cancelButton_w;
	public Sprite blowupButton;
	public Sprite blowupButton_w;
	public Sprite colorButton;

	public SteamVR_TrackedController right;
	public SteamVR_TrackedController left;

	ArrayList selectedCells = new ArrayList();

	int fileCreationCtr = 0;

	Color[] colors;
	int currentColorIndex = 0;
	Color selectedColor;

	PlanePicker planePicker;

	List<RadialMenuButton> buttons;
	bool inSelectionState = false;
	bool selectionMade = false;
	public bool selectionConfirmed = false;
	public ushort hapticIntensity = 2000;

	//private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
	//private SteamVR_TrackedObject trackedObj;

	void Awake () {
		colors = new Color[10];
		colors [0] = new Color (1, 0, 0); // red
		colors [1] = new Color (0, 0, 1); // blue
		colors [2] = new Color (0, 1, 1); // cyan
		colors [3] = new Color (1, 0, 1); // magenta
		colors [4] = new Color (1f, 153f/255f, 204f/255f); // pink
		colors [5] = new Color (1, 1, 0); // yellow
		colors [6] = new Color (0, 0, 1); // green
		colors [7] = new Color (.6f, 1, .6f); // lime green
		colors [8] = new Color (.4f, .2f, 1); // brown
		colors [9] = new Color (1, .6f, .2f); // orange
		selectorMaterial.color = colors [0];
		selectedColor = colors [0];

		planePicker = GameObject.Find ("PlaneSelectors").GetComponent<PlanePicker> ();
		buttons = menu.buttons;
		UpdateButtonIcons();
		//trackedObj = GetComponent<SteamVR_TrackedObject>();
	}
		
	void OnTriggerEnter(Collider other) {
		//print(other.gameObject.name);
		if (other.GetComponentInChildren<Renderer> ().material.color != null) {
			other.GetComponentInChildren<Renderer> ().material.color = new Color (selectedColor.r, selectedColor.g, selectedColor.b, .1f);
		}
		if (!selectedCells.Contains (other)) {
			selectedCells.Add (other);
			//controller.TriggerHapticPulse(500);
			SteamVR_Controller.Input((int)left.controllerIndex).TriggerHapticPulse(hapticIntensity);
		}
		if(!selectionMade) {
			selectionMade = true;
			UpdateButtonIcons ();
		}
	}

	public void ConfirmRemove() {
		foreach (Collider other in selectedCells) {
			other.transform.parent = null;
			other.gameObject.AddComponent<Rigidbody>();
			other.attachedRigidbody.useGravity = true;
			other.attachedRigidbody.isKinematic = false;
			other.isTrigger = false;
			GetComponent<AudioSource> ().Play (); // pop
		}

		selectedCells.Clear ();
		selectionMade = false;
	}

	public void ConfirmSelection () {
		Graph newGraph = Instantiate (graph);
		newGraph.gameObject.SetActive (true);
		newGraph.transform.parent = manager.transform;
		foreach(Collider cell in selectedCells) {
			GameObject graphpoint = cell.gameObject;
			graphpoint.transform.parent = newGraph.transform;
			Color cellColor = cell.GetComponentInChildren<Renderer> ().material.color; // breaks if no boom before
			Color nonTransparentColor = new Color (cellColor.r, cellColor.g, cellColor.b);
			cell.GetComponentInChildren<Renderer> ().material.color = nonTransparentColor;
		}
		// create .txt file with latest selection
		DumpData();
		// clear the list since we are done with it
		// ?
		selectedCells.Clear ();
		selectionMade = false;
		selectionConfirmed = true;
	}

	public void CancelSelection() {
		foreach (Collider other in selectedCells) {
			other.GetComponentInChildren<Renderer> ().material.color = Color.white;
		}
		selectedCells.Clear ();
		selectionMade = false;
	}

	public void ChangeColor() {
		if (currentColorIndex == colors.Length - 1) {
			currentColorIndex = 0;
		} else {
			currentColorIndex++;
		}
		selectedColor = colors [currentColorIndex];
		selectorMaterial.color = selectedColor;
		this.gameObject.GetComponent<Renderer> ().material.color = new Color (selectedColor.r, selectedColor.g, selectedColor.b);
	}

	/*public void ctrlZ() //currently not used
	{
		selectedCells.RemoveAt(selectedCells.Count - 1);
	}*/

	public void DumpData()
	{
		using (System.IO.StreamWriter file =
			new System.IO.StreamWriter(Directory.GetCurrentDirectory() + "\\Assets\\Data\\runtimeGroups\\selection" + fileCreationCtr++ + ".txt")) {
			// new System.IO.StreamWriter(Directory.GetCurrentDirectory() + "\\Assets\\Data\\runtimeGroups\\" + DateTime.Now.ToShortTimeString() + ".txt"))
			// print ("dumping data");
			foreach (Collider cell in selectedCells)
			{
				file.Write(cell.GetComponent<GraphPoint>().getLabel());
				file.Write ("\t");
				Color c = cell.GetComponentInChildren<Renderer> ().material.color;
				int r = (int)(c.r * 255);
				int g = (int)(c.g * 255);
				int b = (int)(c.b * 255);
				file.Write(string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b));
				file.WriteLine ();
				// print ("wrote " + cell.GetComponent<GraphPoint> ().getLabel () + "\t" + string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b));
			}
			file.Flush ();
			file.Close ();
		}
	}
		
	public void Up() {
		print ("up");
		print ("In selection state: " + inSelectionState.ToString());
		print ("Selection made: " + selectionMade.ToString());
		if (inSelectionState && selectionMade) {
			ConfirmSelection ();
			planePicker.cyclePlanes ();
			inSelectionState = false;
			UpdateButtonIcons ();
		}
	}

	public void Left() {
		print ("left");
		print ("In selection state: " + inSelectionState.ToString());
		print ("Selection made: " + selectionMade.ToString());
		if (inSelectionState && selectionMade) {
			CancelSelection ();
		} else if (inSelectionState) {
			planePicker.cyclePlanes ();
			inSelectionState = false;
		} else {
			planePicker.cyclePlanes ();
			inSelectionState = true;
		}
		UpdateButtonIcons ();
	}

	public void Down() {
		print ("down");
		print ("In selection state: " + inSelectionState.ToString());
		print ("Selection made: " + selectionMade.ToString());
		if (inSelectionState && selectionMade) {
			ConfirmRemove ();
			UpdateButtonIcons ();
		}
	}

	public void Right() {
		print ("right");
		print ("In selection state: " + inSelectionState.ToString());
		print ("Selection made: " + selectionMade.ToString());
		if (inSelectionState) {
			ChangeColor ();
		}
	}

	private void UpdateButtonIcons() {
		print ("UpdateButtonIcons");
		// in selection state - selection made
		if (inSelectionState && selectionMade) {
			buttons [0].ButtonIcon = confirmButton;
			buttons [1].ButtonIcon = cancelButton;
			buttons [2].ButtonIcon = blowupButton;
			buttons [3].ButtonIcon = colorButton;
		// in selection state - no selection made
		} else if (inSelectionState) {
			buttons [0].ButtonIcon = confirmButton_w;
			buttons [1].ButtonIcon = cancelButton_w;
			buttons [2].ButtonIcon = blowupButton_w;
			buttons [3].ButtonIcon = colorButton;
		// in default state
		} else {
			buttons [0].ButtonIcon = noButton;
			buttons [1].ButtonIcon = toolButton;
			buttons [2].ButtonIcon = noButton;
			buttons [3].ButtonIcon = noButton;
		}
		menu.RegenerateButtons();
	}
		
}

