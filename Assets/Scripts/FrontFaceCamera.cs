using UnityEngine;
using Valve.VR;

public class SwitchVideoMode : MonoBehaviour
{
    [SerializeField]
    SteamVR_TrackedController rightCtrl;
    [SerializeField]
    SteamVR_TrackedController leftCtrl;

    [SerializeField]
    [Tooltip("Quad that is parented to controller and renders video")]
    GameObject videoQuad;

    SteamVR_TestTrackedCamera trackedCamera;

    enum VideoMode
    {
        NONE,
        TRON_AND_VIDEO,
        TRON,
        VIDEO,
    }

    VideoMode vMode;

    void Start()
    {
        trackedCamera = GetComponent<SteamVR_TestTrackedCamera>();


        // we assume on Start that TrackedCamera game object and video quad
        // are disabled in the object hierarchy, so only test for tron mode.
        EVRSettingsError e = EVRSettingsError.None;
        var tron_enabled = OpenVR.Settings.GetBool(
            OpenVR.k_pch_Camera_Section,
            OpenVR.k_pch_Camera_EnableCameraForCollisionBounds_Bool,
            ref e);
        vMode = tron_enabled ? VideoMode.TRON : VideoMode.NONE;
        if (e != EVRSettingsError.None)
        {
            Debug.LogError(e);
        }

        rightCtrl.MenuButtonClicked += CtrlClicked;
        leftCtrl.MenuButtonClicked += CtrlClicked;
    }

    bool menuPressed;
    void CtrlClicked(object sender, ClickedEventArgs e)
    {
        menuPressed = true;
    }

    void Update()
    {
        if (menuPressed)
        {
            vMode = (VideoMode)(((int)vMode + 1) % 4);
            SetVideoMode(vMode);
            menuPressed = false;
        }
    }

    void SetVideoMode(VideoMode m)
    {
        EVRSettingsError e = EVRSettingsError.None;

        switch (m)
        {
            case VideoMode.NONE:
                SetCameraVideo(false);
                e = SetTronMode(false);
                break;
            case VideoMode.TRON:
                SetCameraVideo(false);
                e = SetTronMode(true);
                break;
            case VideoMode.VIDEO:
                SetCameraVideo(true);
                e = SetTronMode(false);
                break;
            case VideoMode.TRON_AND_VIDEO:
                SetCameraVideo(true);
                e = SetTronMode(true);
                break;
        }

        if (e != EVRSettingsError.None)
        {
            Debug.LogError(e);
        }
    }


    EVRSettingsError SetTronMode(bool enable)
    {
        EVRSettingsError e = EVRSettingsError.None;
        OpenVR.Settings.SetBool(OpenVR.k_pch_Camera_Section,
                                OpenVR.k_pch_Camera_EnableCameraForCollisionBounds_Bool,
                                enable,
                                ref e);
        OpenVR.Settings.Sync(true, ref e);
        return e;
    }

    void SetCameraVideo(bool enable)
    {
        videoQuad.SetActive(enable);
        trackedCamera.enabled = enable;
    }
}