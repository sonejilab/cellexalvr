using UnityEngine;

///<summary>
/// Represents a button used the graphs from the cell selection used for this particular heatmap.
///</summary>
public class RecolourHeatmapButton : StationaryButton
{
    protected override string Description
    {
        get
        {
            return "";
        }
    }

    protected override void Awake()
    {
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        base.Awake();
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            gameObject.GetComponentInParent<Heatmap>().ColorCells();
        }
    }
}
