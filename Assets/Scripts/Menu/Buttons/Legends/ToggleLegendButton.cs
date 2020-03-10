using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Menu.Buttons;
using UnityEngine;

public class ToggleLegendButton : CellexalButton
{
    protected override string Description => "Toggle the legend on/off";

    private void Start()
    {
        TurnOff();
        CellexalEvents.GraphsLoaded.AddListener(TurnOn);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
    }

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
        referenceManager.multiuserMessageSender.SendMessageMoveLegend(legendManager.transform.position,
            legendManager.transform.rotation, legendManager.transform.localScale);
    }

    void TurnOn()
    {
        SetButtonActivated(true);
    }

    void TurnOff()
    {
        SetButtonActivated(false);
    }

}
