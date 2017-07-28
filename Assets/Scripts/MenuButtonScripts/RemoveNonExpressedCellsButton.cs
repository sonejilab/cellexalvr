using System;
using UnityEngine;

public class RemoveNonExpressedCellsButton : StationaryButton
{
    public CellManager cellManager;

    protected override string Description
    {
        get
        {
            return "Toggle cells with no expression";
        }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            cellManager.ToggleNonExpressedCells();
        }
    }


}
