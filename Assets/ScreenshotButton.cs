using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenshotButton : StationaryButton
{

	public Camera camera;
	public GameObject canvas;
	public SpriteRenderer spriteRend;
	public Sprite gray;
	public Sprite black;

	protected override string Description
	{
		get { return "Take Snapshots"; }
	}

	void Update()
	{

		device = SteamVR_Controller.Input((int)rightController.index);
		if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
		{
			if (canvas.activeSelf) {
				canvas.SetActive (false);
				standardTexture = black;
				camera.gameObject.SetActive (false);
			} else {
				canvas.SetActive (true);
				standardTexture = gray;
				camera.gameObject.SetActive (true);
			}

			camera.gameObject.GetComponent<CaptureScreenshot> ().enabled = !camera.gameObject.GetComponent<CaptureScreenshot> ().enabled;
		}
	}

	/*
	protected virtual override void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Controller"))
		{
			descriptionText.text = "";
			spriteRenderer.sprite = standardTexture;
			controllerInside = false;
		}
	}*/


}
