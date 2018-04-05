using System;
using UnityEngine;

public class NewFilterRuleConditionButton : CellexalButton
{
    protected override string Description
    {
        get { return "Choose a condition"; }
    }

    public TextMesh chosonOptionTextMesh;
    public Filter.FilterRule newRule;
    public Filter.FilterRule.Condition type;
    private int index = 0;

    private void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            var enumValues = (Filter.FilterRule.Condition[])Enum.GetValues(type.GetType());
            index++;
            if (index >= enumValues.Length)
                index = 0;
            if (enumValues[index] == Filter.FilterRule.Condition.Invalid)
                index++;
            type = enumValues[index];
            chosonOptionTextMesh.text = type.ToString();
            newRule.condition = type;
        }
    }
}
