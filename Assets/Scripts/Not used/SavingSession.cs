using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingSession : MonoBehaviour {

	public string directory;

	// Use this for initialization
	void Awake () {
		DontDestroyOnLoad (gameObject.transform);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void SetDirectory(string name)
	{
		directory = name;
		Debug.Log ("Dir: " + directory);
	}
}
