using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

	public string SceneToLoad;
	public Loading load;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame

	public void LoadScene(string scene)
	{
		Destroy (GameObject.Find ("Load"));
		SceneManager.LoadScene (scene);
	}
	public void LoadPreviousScene(string scene)
	{
		load.doLoad = true;
		SceneManager.LoadScene (scene);
	}
}
