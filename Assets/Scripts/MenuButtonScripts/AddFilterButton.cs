
public class AddFilterButton : CellexalButton
{
    protected override string Description
    {
        get { return "Add the above filter"; }
    }

    private NewFilterMenu newFilterMenu;

    private void Start()
    {
        newFilterMenu = referenceManager.newFilterMenu;
    }


    private void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            newFilterMenu.AddFilter();
        }
    }
}
