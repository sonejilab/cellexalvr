using UnityEngine;

public class PlaneScaler : MonoBehaviour {

void ScaleSelector () {
	GameObject.Find("PlaneSelectors").GetComponent("PlanePicker");
}

}
