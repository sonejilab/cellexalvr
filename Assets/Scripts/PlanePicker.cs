using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanePicker : MonoBehaviour {
	public GameObject[] planes;
	public int active = 0;

	public void Start() {
		planes [0].SetActive (true);
		for (int i = 1; i < planes.Length; ++i) {
			planes [i].SetActive (false);
		}
	}

	public void cyclePlanes() {
		print ("cyclePlanes");
		if (active == planes.Length - 1) {
			active = 0;
		} else {
			active++;
		}
		for (int i = 0; i < planes.Length; ++i) {
			if (i == active) {
				planes [i].SetActive (true);
			} else {
				planes [i].SetActive (false);
			} 
		}
	}




}
