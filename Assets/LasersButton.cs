/// <summary>
/// This class is repsonsible for turning on and off the laser pointers.
/// </summary>
public class LasersButton : StationaryButton
{

    public VRTK.VRTK_StraightPointerRenderer laserPointerLeft;
    public VRTK.VRTK_StraightPointerRenderer laserPointerRight;

    protected override string Description
    {
        get { return "Toggle Lasers"; }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            // turn both off only if both are on, otherwise turn both on.
            bool enable = !(laserPointerRight.enabled && laserPointerLeft.enabled);
            laserPointerRight.enabled = enable;
            laserPointerLeft.enabled = enable;
        }

    }
}
