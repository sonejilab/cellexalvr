using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePointSelection : MonoBehaviour {

	VRTK.VRTK_SimplePointer singleSelect;


	// Use this for initialization
	void Start () {
		singleSelect = transform.parent.GetComponent<VRTK.VRTK_SimplePointer> ();

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ToggleSingleSelect(){
		singleSelect.enabled = !singleSelect.enabled;
	}
}
