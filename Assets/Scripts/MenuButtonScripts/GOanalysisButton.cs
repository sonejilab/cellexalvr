using UnityEngine;
/// <summary>
/// Represents the button that calls the Rscript doing a GO analysis of the genes on the heatmap.
/// </summary>
class GOanalysisButton : CellexalButton
{
    protected override string Description
    {
        get { return "Do GO analysis"; }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Click()
    {
        gameObject.GetComponentInParent<Heatmap>().GOanalysis();
        device.TriggerHapticPulse(2000);
    }
}
