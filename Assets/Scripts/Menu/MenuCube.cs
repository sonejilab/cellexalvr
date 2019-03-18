using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents small cube to indicate menu controller.
/// </summary>
public class MenuCube : MonoBehaviour {
    public GameObject mainMenu;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (mainMenu.GetComponent<MeshRenderer>().enabled)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
        if (!mainMenu.GetComponent<MeshRenderer>().enabled)
        {
            GetComponent<MeshRenderer>().enabled = true;
            transform.Rotate(0, 0, 1);
        }
	}
}
