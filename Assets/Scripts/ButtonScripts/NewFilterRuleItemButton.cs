using System;
using UnityEngine;

public class NewFilterRuleItemButton : CellexalButton
{
    protected override string Description
    {
        get { return "Choose an item to filter upon"; }
    }

    public Filter.FilterRule newRule;
    public TextMesh textMesh;
    private KeyboardOutput keyboardOutput;

    private void Start()
    {
        keyboardOutput = referenceManager.keyboardOutput;
    }

    protected override void Click()
    {
        keyboardOutput.SetNextTarget(KeyboardOutput.OutputType.FILTER_ITEM_NAME, newRule, textMesh);
    }
}
