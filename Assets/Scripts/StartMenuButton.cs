using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuButton : MonoBehaviour
{
	public TextMesh descriptionText;
	public SteamVR_TrackedObject rightController;
	private SteamVR_Controller.Device device;
	private bool controllerInside;
	private SpriteRenderer spriteRenderer;
	public SceneLoader sceneLoader;

	void Start()
	{
		device = SteamVR_Controller.Input((int)rightController.index);
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	}

	void Update()
	{
		if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
		{
			sceneLoader.LoadScene ("SceneLoaderTest");
		}
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
	}

}
