using System;
using UnityEngine;

public class NewFilterRuleButton : CellexalButton
{
    protected override string Description
    {
        get { return "Create a new filter rule"; }
    }
    public GameObject filterRulePrefab;
    public Filter.FilterRule.RuleType ruleType;

    protected override void Click()
    {
        Filter.FilterRule newFilterRule = new Filter.FilterRule();
        newFilterRule.ruleType = ruleType;
        var newGamobject = Instantiate(filterRulePrefab, transform.position, filterRulePrefab.transform.rotation, transform.parent);
        newGamobject.SetActive(true);

        newGamobject.GetComponentInChildren<NewFilterRuleTypeButton>().newRule = newFilterRule;
        newGamobject.GetComponentInChildren<NewFilterRuleItemButton>().newRule = newFilterRule;
        newGamobject.GetComponentInChildren<NewFilterRuleConditionButton>().newRule = newFilterRule;
        newGamobject.GetComponentInChildren<NewFilterRuleValueButton>().newRule = newFilterRule;

        transform.Translate(0f, 0f, -0.6f);
    }
}
