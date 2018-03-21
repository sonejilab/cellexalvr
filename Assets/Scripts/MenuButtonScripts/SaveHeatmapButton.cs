using UnityEngine;
/// <summary>
/// Represents the button that saves the heatmap it is attached to the disk.
/// </summary>
class SaveHeatmapButton : CellexalButton
{
    protected override string Description
    {
        get { return "Save heatma\nimage to disk"; }
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
            gameObject.GetComponentInParent<Heatmap>().SaveImage();
        }
    }
}
