using System;
using UnityEngine;

///<summary>
/// This class represents a button used for resetting the color and position of the graphs.
///</summary>
public class ResetGraphButton : StationaryButton
{

    private GraphManager graphManager;

    protected override string Description
    {
        get
        {
            return "Reset position and colors of all graphs";
        }
    }

    private void Start()
    {
        graphManager = referenceManager.graphManager;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            graphManager.ResetGraphsColor();
        }
    }

}
