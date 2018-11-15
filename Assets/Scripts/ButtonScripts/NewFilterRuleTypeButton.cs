using System;
using UnityEngine;

class NewFilterRuleTypeButton : CellexalButton
{
    protected override string Description
    {
        get { return "Choose a condition"; }
    }

    public TextMesh chosonOptionTextMesh;
    public Filter.FilterRule newRule;
    public Filter.FilterRule.FilterType type;
    private int index = 0;

    protected override void Click()
    {
        var enumValues = (Filter.FilterRule.FilterType[])Enum.GetValues(type.GetType());
        index++;
        if (index >= enumValues.Length)
            index = 0;
        if (enumValues[index] == Filter.FilterRule.FilterType.Invalid)
            index++;
        type = enumValues[index];
        chosonOptionTextMesh.text = type.ToString();
        newRule.filterType = type;
    }
}
