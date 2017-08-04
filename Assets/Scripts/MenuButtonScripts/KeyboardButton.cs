using UnityEngine;

///<summary>
/// This class represents a button used for toggling the keyboard.
///</summary>
public class KeyboardButton : StationaryButton
{
    public GameObject keyboard;
    public VRTK.VRTK_StraightPointerRenderer laserPointer;
    private bool keyboardActivated = false;

    protected override string Description
    {
        get { return "Toggle keyboard"; }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
			keyboardActivated = !laserPointer.enabled;
            laserPointer.enabled = keyboardActivated;
            keyboard.SetActive(keyboardActivated);
        }
    }

}
