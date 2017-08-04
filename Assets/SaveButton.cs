using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Examples;

public class SaveButton : MonoBehaviour {
	
	public TextMesh descriptionText;
	public SteamVR_TrackedObject rightController;
	private SteamVR_Controller.Device device;
	private bool controllerInside;
	private SpriteRenderer spriteRenderer;
	public SaveScene saveScene;
	public Sprite green;
	public Sprite black;
	private float elapsedTime;
	private float time = 1.0f;

	// Use this for initialization

	void Start ()
	{
		device = SteamVR_Controller.Input((int)rightController.index);
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (controllerInside && device.GetPressDown (SteamVR_Controller.ButtonMask.Trigger)) {
			Debug.Log ("Do Save");
			saveScene.Save ();
			elapsedTime = 0.0f;
			spriteRenderer.sprite = green;
		}
		if (elapsedTime < time) {
			elapsedTime += Time.deltaTime;
		} else {
			spriteRenderer.sprite = black;
		}
	}

	void ChangeSprite() 
	{
		spriteRenderer.sprite = green;
		float elapsedTime = 0.0f;
		if (elapsedTime > time) 
		{
			spriteRenderer.sprite = black;
		} else {
			elapsedTime += Time.deltaTime;
		}
	}
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Controller")
		{
			descriptionText.text = "Save Session";
			controllerInside = true;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Controller")
		{
			descriptionText.text = "";
			controllerInside = false;
		}
	}
}
