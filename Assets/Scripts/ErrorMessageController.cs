using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorMessageController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		gameObject.SetActive(false);
	}

	public void DisplayErrorMessage(int displayTime) {
		gameObject.SetActive(true);
   		StartCoroutine(HideErrorMessage(displayTime));
   	}

	private IEnumerator HideErrorMessage(int waiForSeconds) {
		yield return new WaitForSeconds(waiForSeconds);
		gameObject.SetActive(false);
	}


}
