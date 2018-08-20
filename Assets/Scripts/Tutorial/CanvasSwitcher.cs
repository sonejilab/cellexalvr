using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSwitcher : MonoBehaviour {

    public GameObject tutorialCanvas;
    public GameObject mainCanvas;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void switchCanvas()
    {
        if (tutorialCanvas.gameObject.activeSelf)
        {
            tutorialCanvas.SetActive(false);
            mainCanvas.SetActive(true);
        }
        else
        {
            mainCanvas.SetActive(false);
            tutorialCanvas.SetActive(true);
        }
    }
}
