using System;
using UnityEngine;

public class RemoveNonExpressedCellsButton : StationaryButton
{
    private CellManager cellManager;

    protected override string Description
    {
        get
        {
            return "Toggle cells with no expression";
        }
    }

    private void Start()
    {
        cellManager = referenceManager.cellManager;
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
