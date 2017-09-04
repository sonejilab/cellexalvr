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

    private Transform oldTransform;
    private Vector3 oldScale;
    private Vector3 oldPosition;
    private Quaternion oldRotation;
    // Use this for initialization
    void Start () {
        oldRotation = new Quaternion(0, 0, 0, 0);
        oldPosition = new Vector3(0, 0, 0);
        oldScale = new Vector3(0, 0, 0);
        networkGenerator = GameObject.Find("NetworkGenerator").GetComponent<NetworkGenerator>();
    }
	
	// Update is called once per frame
	void Update () {


	}
    // Sends object to sky and places it dependant on how many objects are in the sky already and dependant of what object it is (0 for network and 1 for heatmap)
    public void DoSendToSky(int objInSky, int objType)
    {
        int caseSwitch = objInSky;
        oldPosition = transform.position;
        oldScale = transform.localScale;
        oldRotation = transform.rotation;
        switch (caseSwitch)
        {
            case 0:

                transform.position = new Vector3(4, 5, 4);
                if (objType == 0)
                {
                    transform.localScale = new Vector3(8, 8, 8);
                    transform.LookAt(new Vector3(0, 0, 0));
                }
                else
                {
                    transform.localScale = new Vector3(1, 1, 1);
                    //transform.LookAt(new Vector3(0, 0, 0));
                    transform.rotation.Set(125, 210, 0, 0);
                }
                networkGenerator.objectsInSky++;
                break;
            case 1:
                transform.position = new Vector3(4, 5, -4);
                if (objType == 0)
                {
                    transform.localScale = new Vector3(8, 8, 8);
                    transform.LookAt(new Vector3(0, 0, 0));
                }
                else
                {
                    transform.localScale = new Vector3(1, 1, 1);
                    transform.LookAt(new Vector3(0, 0, 0));
                    transform.Rotate(100, 0, 0);
                }
                networkGenerator.objectsInSky++;
                break;
            
            case 2:
                transform.position = new Vector3(-4, 5, -4);
                if (objType == 0)
                {
                    transform.localScale = new Vector3(8, 8, 8);
                    transform.LookAt(new Vector3(0, 0, 0));
                }
                else
                {
                    transform.localScale = new Vector3(1, 1, 1);
                    transform.LookAt(new Vector3(0, 0, 0));
                    transform.Rotate(100, 0, 0);
                }
                networkGenerator.objectsInSky++;
                break;


        }
    }

    public void GetBackFromSky()
    {
        networkGenerator.objectsInSky--;
        transform.position = oldPosition;
        transform.localScale = oldScale;
        transform.rotation = oldRotation;
    }
}
