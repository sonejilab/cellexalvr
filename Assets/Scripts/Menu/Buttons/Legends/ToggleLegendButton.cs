using CellexalVR.AnalysisObjects;
using CellexalVR.Menu.Buttons;
using UnityEngine;

public class ToggleLegendButton : CellexalButton
{
    protected override string Description => "Toggle the legend on/off";

    public override void Click()
    {
        LegendManager legendManager = referenceManager.legendManager;
        GameObject mainMenu = referenceManager.mainMenu;
        referenceManager.multiuserMessageSender.SendMessageToggleLegend();
        if (!referenceManager.legendManager.legendActive)
        {
            legendManager.ActivateLegend();
        }
        else
        {
            legendManager.DeactivateLegend();
            legendManager.DetachLegendFromCube();
        }
    }
}
