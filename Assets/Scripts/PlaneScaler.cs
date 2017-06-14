using UnityEngine;

public class PlaneScaler : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}

	void ScaleSelector () {
		GameObject.Find ("PlaneSelectors").GetComponent ("PlanePicker");//planes[active];


	}
}
