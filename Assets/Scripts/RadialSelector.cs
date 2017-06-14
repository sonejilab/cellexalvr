using UnityEngine;

public class RadialSelector : MonoBehaviour {

	public Graph graph;
	public GameObject keyboard;
	public GameObject toolTipsRight;
	public GameObject toolTipsLeft;
	public GraphManager manager;
	public AudioSource laser;

	VRTK.VRTK_StraightPointerRenderer singleSelect;
    private bool pointerActive = false;


	// Use this for initialization
	void Start () {
		singleSelect = transform.parent.GetComponent<VRTK.VRTK_StraightPointerRenderer> ();

		// singleSelect.Toggle(pointerActive);

	}

	// Update is called once per frame
	void Update () {

	}

	public void ToggleSingleSelect(){
		laser.Play ();
        // keyboard.SetActive (false);
        pointerActive = !pointerActive;
        singleSelect.enabled = pointerActive;

        // singleSelect.Toggle(pointerActive, pointerActive);
		/* if (singleSelect.enabled) {
			manager.destroyRigidbodies ();
		} else {
			manager.createRigidbodies ();
		}*/
	}

	public void ToggleColoring(){
		singleSelect.enabled = true;
		keyboard.SetActive(!keyboard.activeSelf);
	}

	public void ToggleToolTips() {
		keyboard.SetActive (false);
		singleSelect.enabled = false;
		toolTipsRight.SetActive(!toolTipsRight.activeSelf);
		toolTipsLeft.SetActive(!toolTipsLeft.activeSelf);

	}

	public void ResetGraph(){
		manager.ResetGraph ();
		manager.ResetGraph ();//en andra reset löser en del problem. Fult men who cares?
	}
}
