using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSwitcher : MonoBehaviour {

    public GameObject[] canvases;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void switchCanvas(int id)
    {
        for (int i = 0; i < canvases.Length; i++)
        {
            bool b = i == id;
            canvases[i].SetActive(b);
        }
        //if (tutorialCanvas.gameObject.activeSelf)
        //{
        //    tutorialCanvas.SetActive(false);
        //    obj.SetActive(true);
        //}
        //else
        //{
        //    mainCanvas.SetActive(false);
        //    tutorialCanvas.SetActive(true);
        //}
    }
}
