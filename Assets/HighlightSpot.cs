using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightSpot : MonoBehaviour {

    public TutorialManager tutorialManager;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Graph")
        {
            tutorialManager.NextStep();
        }
    }
}
