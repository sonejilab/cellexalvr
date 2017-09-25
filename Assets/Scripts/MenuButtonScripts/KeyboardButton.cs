using UnityEngine;

///<summary>
/// This class represents a button used for toggling the keyboard.
///</summary>
public class KeyboardButton : StationaryButton
{
    public Sprite gray;
    public Sprite original;

    private GameObject keyboard;
    private ControllerModelSwitcher controllerModelSwitcher;
    private bool activateKeyboard = false;

    protected override string Description
    {
        get { return "Toggle keyboard"; }
    }

    private void Start()
    {
        keyboard = referenceManager.keyboard.gameObject;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            activateKeyboard = !keyboard.activeSelf;
            if (activateKeyboard)
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Keyboard;
                controllerModelSwitcher.ActivateDesiredTool();
            }
            else
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
            }
            keyboard.SetActive(activateKeyboard);
        }
        if (activateKeyboard)
        {
            standardTexture = gray;
        }
        if (!activateKeyboard)
        {
            standardTexture = original;
        }
    }

}
