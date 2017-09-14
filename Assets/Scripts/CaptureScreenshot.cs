using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CaptureScreenshot : MonoBehaviour
{


    public SteamVR_TrackedObject rightController;
    public GameObject fadeScreen;
    private SteamVR_Controller.Device device;
    private float fadeTime = 0.7f;
    private float elapsedTime = 0.0f;
    private float colorAlpha;
    private int scrnNr;
    private string directory = Directory.GetCurrentDirectory() + "/Screenshots";

    // Use this for initialization
    void Start()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        //fadeScreen.GetComponent<Image> ().color = new Color (0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            Vector2 touchpad = (device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
            if (touchpad.y > 0.7f)
            {
                //Touchpad 
                if (!Directory.Exists(directory))
                {
                    CellExAlLog.Log("Creating directory " + directory);
                    Directory.CreateDirectory(directory);
                }
                Application.CaptureScreenshot(directory + "/Screenshot" + scrnNr.ToString() + ".png");
                Debug.Log("Screenshot taken!");
                elapsedTime = 0.0f;
                scrnNr++;
            }
        }

        if (elapsedTime < fadeTime / 2.0f)
        {
            elapsedTime += Time.deltaTime;
            colorAlpha += 0.05f;
            fadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, colorAlpha);
        }
        else if (elapsedTime < fadeTime && elapsedTime > (fadeTime / 2.0f))
        {
            elapsedTime += Time.deltaTime;
            colorAlpha -= 0.05f;
            fadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, colorAlpha);
        }
    }

}
