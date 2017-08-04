using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenshotButton : MonoBehaviour
{
	public TextMesh descriptionText;
	public SteamVR_TrackedObject rightController;
	private SteamVR_Controller.Device device;
	private bool controllerInside;
	private SpriteRenderer spriteRenderer;
	public Camera camera;
	public GameObject canvas;
	public SpriteRenderer spriteRend;
	public Sprite gray;
	public Sprite black;

		
	void Start()
	{
		device = SteamVR_Controller.Input((int)rightController.index);
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	}

	void Update()
	{
		if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
		{
			if (canvas.activeSelf) {
				canvas.SetActive (false);
				spriteRend.sprite = black;
				camera.gameObject.SetActive (false);
			} else {
				canvas.SetActive (true);
				spriteRend.sprite = gray;
				camera.gameObject.SetActive (true);
			}

			camera.gameObject.GetComponent<CaptureScreenshot> ().enabled = !camera.gameObject.GetComponent<CaptureScreenshot> ().enabled;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Controller")
		{
			descriptionText.text = "Take Screenshots";
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
