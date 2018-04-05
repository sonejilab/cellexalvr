using UnityEngine;

public class NewFilterRuleValueButton : CellexalButton
{
    protected override string Description
    {
        get { return "Choose a value"; }
    }

    public Filter.FilterRule newRule;
    public TextMesh textMesh;
    private KeyboardOutput keyboardOutput;

    private void Start()
    {
        keyboardOutput = referenceManager.keyboardOutput;
    }



    private void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            keyboardOutput.SetNextTarget(KeyboardOutput.OutputType.FILTER_VALUE, newRule, textMesh);
        }
    }
}
