using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCanvas : MonoBehaviour {
	public LoaderController loaderController;
	public InputReader inputReader;
	// Use this for initialization
	void Start () {
		if (inputReader.doLoad) {
			transform.gameObject.SetActive (true);
		} else {
			transform.gameObject.SetActive (false);
		}
	}

	// Update is called once per frame
	void Update () {
		if (!loaderController.loadingComplete && inputReader.doLoad) {
			
		} else if (loaderController.loadingComplete) {
			transform.gameObject.SetActive (false);
			return;
		}
	}
}
