using UnityEngine;
/// <summary>
/// Represents the button that saves the heatmap as an image. If user wants to create a report
/// the image is included in it.
/// </summary>
public class SaveHeatmapButton : CellexalButton
{
    public Sprite doneTex;
    protected override string Description
    {
        get { return "Save heatmap\nimage to disk"; }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Click()
    {
        gameObject.GetComponentInParent<Heatmap>().SaveImage();
        device.TriggerHapticPulse(2000);
    }
    public void FinishedButton()
    {
        spriteRenderer.sprite = doneTex;
    }
}
