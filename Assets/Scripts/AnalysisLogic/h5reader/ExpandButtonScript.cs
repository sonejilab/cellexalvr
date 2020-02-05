using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CellexalVR.General;

public class ExpandButtonScript : MonoBehaviour
{
    private ReferenceManager referenceManager;
    private H5ReaderAnnotatorTextBoxScript parentScript;
    private SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
    private bool controllerInside;
    public Image image;

    // Start is called before the first frame update
    void Start()
    {
        if (!referenceManager)
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }
        rightController = referenceManager.rightController;
        parentScript = GetComponentInParent<H5ReaderAnnotatorTextBoxScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
        {
            controllerInside = true;
            image.color = Color.red;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
        {
            controllerInside = false;
            image.color = Color.white;
        }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            foreach (H5ReaderAnnotatorTextBoxScript key in parentScript.subkeys.Values)
            {
                key.gameObject.SetActive(!key.gameObject.activeSelf);
            }
            H5ReaderAnnotatorTextBoxScript parent = parentScript;
            while (!parent.isTop)
            {
                parent = parent.transform.parent.GetComponent<H5ReaderAnnotatorTextBoxScript>();
            }
            parent.updatePosition(10f);
        }
    }
}
