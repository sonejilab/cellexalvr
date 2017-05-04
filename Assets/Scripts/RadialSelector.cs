using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialSelector : MonoBehaviour {

	public Graph graph;
	public GameObject keyboard;

	VRTK.VRTK_SimplePointer singleSelect;


	// Use this for initialization
	void Start () {
		singleSelect = transform.parent.GetComponent<VRTK.VRTK_SimplePointer> ();

		singleSelect.enabled = false;

	}

	// Update is called once per frame
	void Update () {

	}

	public void ToggleSingleSelect(){
		singleSelect.enabled = !singleSelect.enabled;
		if (singleSelect.enabled == false) {
			graph.getGroups ();
		}
	}

	public void ToggleColoring(){
		singleSelect.enabled = false;
		keyboard.SetActive(!keyboard.activeSelf);
	}
}
