using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SendToSky : MonoBehaviour {
    private VRTK_InteractableObject interact;
    private SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
    private bool controllerInside = false;
    private NetworkGenerator networkGenerator;

    // Use this for initialization
    void Start () {
        interact = GetComponent<VRTK_InteractableObject>();
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        networkGenerator = GameObject.Find("NetworkGenerator").GetComponent<NetworkGenerator>();
    }
	
	// Update is called once per frame
	void Update () {
        if (interact == null)
        {
            interact = GetComponent<VRTK_InteractableObject>();
        }
        if (device == null)
        {
            device = SteamVR_Controller.Input((int)rightController.index);
        }
        if (this.name == "Enlarged Network")
        {
	        if (interact.enabled)
            {
                Debug.Log("INTERACTING");
                // handle input
                if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    controllerInside = false;
                    Debug.Log("DO SEND TO SKY");
                    DoSendToSky(networkGenerator.objectsInSky);
                }
            }
        }
	}
    void DoSendToSky(int objInSky)
    {
        Debug.Log(objInSky);
        int caseSwitch = objInSky;
        switch (caseSwitch)
        {
            case 0:
            
                transform.localScale = new Vector3(8, 8, 8);
                transform.position = new Vector3(4, 4, 4);
                transform.LookAt(new Vector3(0, 0, 0));
                networkGenerator.objectsInSky++;
                break;
            case 1:
                transform.localScale = new Vector3(8, 8, 8);
                transform.position = new Vector3(4, 4, -4);
                transform.LookAt(new Vector3(0, 0, 0));
                networkGenerator.objectsInSky++;
                break;
            
            case 2:
                transform.localScale = new Vector3(8, 8, 8);
                transform.position = new Vector3(-4, 4, -4);
                transform.LookAt(new Vector3(0, 0, 0));
                networkGenerator.objectsInSky++;
                break;


        }
    }

}
