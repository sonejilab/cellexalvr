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



    public override void Click()
    {
        keyboardOutput.SetNextTarget(KeyboardOutput.OutputType.FILTER_VALUE, newRule, textMesh);
    }
}
