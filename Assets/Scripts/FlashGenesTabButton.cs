/// <summary>
/// This class represent the tab buttons on top of the flash genes menu.
/// Each tab represents one file of genes that can be flashed.
/// </summary>
class FlashGenesTabButton : TabButton
{
    private CellManager cellManager;

    protected override void Start()
    {
        base.Start();
        cellManager = referenceManager.cellManager;
    }

    protected override void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            cellManager.CurrentFlashGenesMode = CellManager.FlashGenesMode.DoNotFlash;
            Menu.TurnOffAllTabs();
            tab.SetTabActive(true);
        }
    }
}
