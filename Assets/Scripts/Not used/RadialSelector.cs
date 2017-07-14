using UnityEngine;

public class RadialSelector : MonoBehaviour {

public Graph graph;
public GameObject keyboard;
public GameObject toolTipsRight;
public GameObject toolTipsLeft;
public GraphManager manager;
public AudioSource laser;
private VRTK.VRTK_StraightPointerRenderer singleSelect;
private bool pointerActive = false;

// Use this for initialization
void Start () {
	singleSelect = transform.parent.GetComponent<VRTK.VRTK_StraightPointerRenderer> ();
}

public void ToggleSingleSelect() {
	laser.Play ();
	pointerActive = !pointerActive;
	singleSelect.enabled = pointerActive;
}

public void ToggleColoring() {
	singleSelect.enabled = true;
	keyboard.SetActive(!keyboard.activeSelf);
}

public void ToggleToolTips() {
	keyboard.SetActive (false);
	singleSelect.enabled = false;
	toolTipsRight.SetActive(!toolTipsRight.activeSelf);
	toolTipsLeft.SetActive(!toolTipsLeft.activeSelf);
}

public void ResetGraph() {
	manager.ResetGraphs ();
}

}
