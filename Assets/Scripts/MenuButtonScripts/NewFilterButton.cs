using UnityEngine;

/// <summary>
/// The button for creating a new filter.
/// </summary>
public class NewFilterButton : SolidButton
{
    private FilterMenu filterMenu;

    protected override void Start()
    {
        base.Start();
        filterMenu = referenceManager.filterMenu;
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
        }
    }
}

