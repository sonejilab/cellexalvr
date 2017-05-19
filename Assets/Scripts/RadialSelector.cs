using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialSelector : MonoBehaviour {

	public Graph graph;
	public GameObject keyboard;
	public GameObject toolTipsRight;
	public GameObject toolTipsLeft;
	public GraphManager manager;

	VRTK.VRTK_StraightPointerRenderer singleSelect;


	// Use this for initialization
	void Start () {
		singleSelect = transform.parent.GetComponent<VRTK.VRTK_StraightPointerRenderer> ();

		singleSelect.enabled = false;

	}

	// Update is called once per frame
	void Update () {

	}

	public void ToggleSingleSelect(){
		GetComponent<AudioSource> ().Play ();
		singleSelect.enabled = !singleSelect.enabled;
		/* if (singleSelect.enabled) {
			manager.destroyRigidbodies ();
		} else {
			manager.createRigidbodies ();
		}*/
	}

	public void ToggleColoring(){
		singleSelect.enabled = false;
		keyboard.SetActive(!keyboard.activeSelf);
	}

	public void ToggleToolTips() {
		toolTipsRight.SetActive(!toolTipsRight.activeSelf);
		toolTipsLeft.SetActive(!toolTipsLeft.activeSelf);

	}

	public void ResetGraph(){
		manager.resetGraph ();
	}
}
