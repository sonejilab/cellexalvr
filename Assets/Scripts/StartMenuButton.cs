using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuButton : StationaryButton
{
	public SceneLoader sceneLoader;

	protected override string Description
	{
		get
		{
			return "Back to Start Menu";
		}
	}



	void Update()
	{
		if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
		{
			sceneLoader.LoadScene ("SceneLoaderTest");
		}
	}
	/*
	void Start()
	{
		device = SteamVR_Controller.Input((int)rightController.index);
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	}
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Controller")
		{
			descriptionText.text = "Back to Start Menu";
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
	}*/

}
