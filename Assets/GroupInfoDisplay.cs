using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupInfoDisplay : MonoBehaviour {
    public SelectionToolHandler selectionToolHandler;
    public TextMesh status;
    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void UpdateStatus()
    {
        status.text = " Red: " + selectionToolHandler.groups[0] + "\n Blue: " + selectionToolHandler.groups[1] + "\n Cyan: " + selectionToolHandler.groups[2]
            + "\n Magneta: " + selectionToolHandler.groups[3] + "\n Pink: " + selectionToolHandler.groups[4] + "\n Yellow: " + selectionToolHandler.groups[5]
            + "\n Green: " + selectionToolHandler.groups[6] + "\n Yellow: " + selectionToolHandler.groups[7] + "\n Purple: " + selectionToolHandler.groups[8]
            + "\n Orange: " + selectionToolHandler.groups[9];
    }
}
