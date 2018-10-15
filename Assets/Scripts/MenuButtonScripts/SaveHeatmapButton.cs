using UnityEngine;
/// <summary>
/// Represents the button that saves the heatmap it is attached to the disk.
/// </summary>
class SaveHeatmapButton : CellexalButton
{
    protected override string Description
    {
        get { return "Save heatmap\nimage to disk"; }
    }

    protected override void Awake()
    {
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        base.Awake();
    }

    protected override void Click()
    {
        gameObject.GetComponentInParent<Heatmap>().SaveImage();
        device.TriggerHapticPulse(2000);
    }
}
