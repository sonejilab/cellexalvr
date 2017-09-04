using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBack : MonoBehaviour {
    public SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
    // Use this for initialization
    void Start () {
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)rightController.index);
    }
	
	// Update is called once per frame
	void Update () {
        if (rightController == null)
        {
            //Debug.Log("Find right controller");
            rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();

        }
        if (device == null)
        {
            device = SteamVR_Controller.Input((int)rightController.index);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            Debug.Log("PUSH BACK");
            Vector3 dir = this.transform.position - device.transform.pos;
            dir = -dir.normalized;
            GetComponent<Rigidbody>().AddForce(dir * 3);
        }
    }
}
