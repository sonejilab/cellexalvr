using System;
using UnityEngine;
using UnityEngine.Events;

///<summary>
/// This class represents a button used for resetting the color and position of the graphs.
///</summary>
public class ResetGraphAllButton : StationaryButton
{

    private GraphManager graphManager;

    protected override string Description
    {
        get
        {
            return "Reset the position and\ncolor of all graphs";
        }
    }

    private void Start()
    {
        graphManager = referenceManager.graphManager;
        SetButtonActivated(false);
        CellExAlEvents.GraphsLoaded.AddListener(OnGraphsLoaded);
        CellExAlEvents.GraphsUnloaded.AddListener(OnGraphsUnloaded);
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            graphManager.ResetGraphs();
        }
    }

    private void OnGraphsLoaded()
    {
        SetButtonActivated(true);
    }

    private void OnGraphsUnloaded()
    {
        SetButtonActivated(false);
    }
}
