using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingController : MonoBehaviour {

	private SteamVR_TrackedObject trackedObject;
	private SteamVR_Controller.Device device;
	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

	// Use this for initialization
	void Start () {
		trackedObject = GetComponent<SteamVR_TrackedObject> ();
	}

	void Trigger (object sender, ClickedEventArgs e)
	{
		Debug.Log ("Trigger has been pressed");
	}
	
	// Update is called once per frame
	void Update () {
		device = SteamVR_Controller.Input ((int)trackedObject.index);


		if (device.GetPressDown (triggerButton)) {
			device.TriggerHapticPulse (3999);
			if (device.GetAxis().x != 0 || device.GetAxis().y != 0) {
				Debug.Log(device.GetAxis().x + " " + device.GetAxis().y);
			}
		}		
	}
}
