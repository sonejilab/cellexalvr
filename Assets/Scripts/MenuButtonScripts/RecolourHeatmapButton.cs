using System;
using UnityEngine;

///<summary>
/// This class represents a button used the graphs from the cell selection used for this particular heatmap.
///</summary>
public class RecolourHeatmapButton : StationaryButton
{
    protected override string Description
    {
        get
        {
            return "Recoulour heatmap";
        }
    }

    protected override void Start()
    {
        GameObject.Find("Controller (right)");
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            gameObject.GetComponentInParent<Heatmap>().ColorCells();
        }
    }
}
