using System;
using System.Collections;
using System.Collections.Generic;
using static Filter.FilterRule;

/// <summary>
/// A filter that can be used to filter out some graphpoints based on some rules.
/// </summary>
public class Filter
{
    private ReferenceManager referenceManager;
    private SelectionToolHandler selectionToolHandler;
    protected SQLiter.SQLite database;
    private List<FilterRule> rulesAll;
    private List<FilterRule> rulesAny;

    public Filter(ReferenceManager referenceManager)
    {
        rulesAll = new List<FilterRule>();
        rulesAny = new List<FilterRule>();
        database = referenceManager.database;
        selectionToolHandler = referenceManager.selectionToolHandler;
    }

    /// <summary>
    /// Adds a rule to this filter.
    /// </summary>
    /// <param name="rule">The rule to add</param>
    public void AddRule(FilterRule rule)
    {
        if (rule.ruleType == RuleType.All)
        {
            rulesAll.Add(rule);
        }
        else if (rule.ruleType == RuleType.Any)
        {
            rulesAny.Add(rule);
        }
    }


    /// <summary>
    /// Checks if a <see cref="GraphPoint"/> passes this filter.
    /// </summary>
    /// <param name="point">the graphpoint to check.</param>
    /// <returns>True if the graphpoint passed the filter. False if it did not pass.</returns>
    public bool Pass(GraphPoint point)
    {
        // graphpoint must pass all the rules in rulesAll
        foreach (FilterRule rule in rulesAll)
        {
            if (!rule.Pass(point))
                return false;
        }
        if (rulesAny.Count == 0)
            return true;

        // graphpoint must pass at least one of the rules in rulesAny
        foreach (FilterRule rule in rulesAny)
        {
            if (rule.Pass(point))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Loads this filter. This may use up much memory depending on the rules this filter uses.
    /// </summary>
    public void Load()
    {
        selectionToolHandler.CurrentFilter = this;
        foreach (FilterRule rule in rulesAll)
        {
            rule.Load(database);
        }
        foreach (FilterRule rule in rulesAny)
        {
            rule.Load(database);
        }
    }

    /// <summary>
    /// Unloads this filter, clearing up the memory used by the filter rules.
    /// </summary>
    public void Unload()
    {
        if (selectionToolHandler.CurrentFilter == this)
            selectionToolHandler.CurrentFilter = null;

        foreach (FilterRule rule in rulesAll)
        {
            rule.Unload();
        }
        foreach (FilterRule rule in rulesAny)
        {
            rule.Unload();
        }
    }

    public override string ToString()
    {
        string result = "";
        foreach (FilterRule rule in rulesAll)
        {
            result += (" ALL " + rule.filterType + " " + rule.item + " " + rule.condition + " " + rule.value + " " + rule.attributeValue);
        }
        foreach (FilterRule rule in rulesAny)
        {
            result += (" ANY " + rule.filterType + " " + rule.item + " " + rule.condition + " " + rule.value + " " + rule.attributeValue);
        }
        return result;
    }

    /// <summary>
    /// Helper class to represent a rule in a filter.
    /// </summary>
    public class FilterRule
    {
        public string item;
        public float value;
        public bool attributeValue;
        public Condition condition;
        public enum Condition { Invalid, Equals, NotEquals, LessEqualThan, LessThan, GreaterEqualsThan, GreaterThan }

        public FilterType filterType;
        public enum FilterType { Invalid, Gene, Attribute, Facs }

        public RuleType ruleType;
        public enum RuleType { Invalid, All, Any }

        private Dictionary<string, float> values;

        /// <summary>
        /// Use for non-attribute filterrules
        /// </summary>
        public FilterRule(FilterType type, string name, Condition cond, float value)
        {
            filterType = type;
            item = name;
            condition = cond;
            this.value = value;
            values = new Dictionary<string, float>();
        }

        /// <summary>
        /// Use for attribute filter rules
        /// </summary>
        public FilterRule(FilterType type, string name, bool value)
        {
            filterType = type;
            item = name;
            attributeValue = value;
        }

        /// <summary>
        /// Check if a graphpoint passes this filter.
        /// </summary>
        /// <param name="graphPoint">The graphpoint to check.</param>
        /// <returns>True if the graphpoint passes the filter, false otherwise.</returns>
        public bool Pass(GraphPoint graphPoint)
        {
            float graphPointValue;
            switch (filterType)
            {
                case FilterType.Gene:
                    if (values.ContainsKey(graphPoint.label))
                    {
                        graphPointValue = values[graphPoint.label];
                    }
                    else
                    {
                        graphPointValue = 0f;
                    }
                    break;
                case FilterType.Facs:
                    graphPointValue = graphPoint.Cell.Facs[item];
                    break;
                case FilterType.Attribute:
                    return graphPoint.Cell.Attributes.ContainsKey(item) == attributeValue;
                default:
                    return false;
            }
            switch (condition)
            {
                case Condition.Equals:
                    return graphPointValue == value;
                case Condition.NotEquals:
                    return graphPointValue != value;
                case Condition.LessEqualThan:
                    return graphPointValue <= value;
                case Condition.LessThan:
                    return graphPointValue < value;
                case Condition.GreaterEqualsThan:
                    return graphPointValue >= value;
                case Condition.GreaterThan:
                    return graphPointValue > value;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Loads this filter rule. Only needed if <see cref="filterType"/> = <see cref="FilterType.Gene"/>,
        /// </summary>
        /// <param name="database">The database containing the gene expression values</param>
        public void Load(SQLiter.SQLite database)
        {
            if (filterType == FilterType.Gene)
            {
                database.QueryGene(item, ReadResults);
            }
        }

        /// <summary>
        /// Called after load when the database has finished its query.
        /// </summary>
        private void ReadResults(SQLiter.SQLite database)
        {
            ArrayList result = database._result;
            for (int i = 0; i < result.Count; ++i)
            {
                var tuple = (Tuple<string, float>)result[i];
                values[tuple.Item1] = tuple.Item2;
            }
        }

        /// <summary>
        /// Unloads this filter rule.
        /// </summary>
        public void Unload()
        {
            if (filterType == FilterType.Gene)
            {
                values.Clear();
            }
        }
    }

    /// <summary>
    /// Returns the length of a condition as a string.
    /// </summary>
    /// <param name="c">The condition.</param>
    /// <returns>The length of the condition as a string, or -1 if <paramref name="c"/> was <see cref="Condition.Invalid"/> or some other invalid value.</returns>
    public static int ConditionStringLength(Condition c)
    {
        switch (c)
        {
            case Condition.Equals:
            case Condition.GreaterThan:
            case Condition.LessThan:
                return 1;
            case Condition.NotEquals:
            case Condition.GreaterEqualsThan:
            case Condition.LessEqualThan:
                return 2;
            case Condition.Invalid:
            default:
                return -1;
        }
    }

    /// <summary>
    /// Converts a string to a <see cref="FilterRule.Condition"/>.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <returns>A <see cref="FilterRule.Condition"/> if the string was acceptable. -1 otherwise.</returns>
    public static Condition StringToFilterRule(string s)
    {
        switch (s)
        {
            case "=":
                return Condition.Equals;
            case "!=":
                return Condition.NotEquals;
            case "<=":
                return Condition.LessEqualThan;
            case "<":
                return Condition.LessThan;
            case ">=":
                return Condition.GreaterEqualsThan;
            case ">":
                return Condition.GreaterThan;
            default:
                return Condition.Invalid;
        }
    }

    /// <summary>
    /// Converts a string to a <see cref="FilterType"/>.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <returns>A <see cref="FilterType"/> if the string was acceptable.</returns>
    public static FilterType StringToFilterType(string s)
    {
        switch (s.ToUpper())
        {
            case "FACS":
                return FilterType.Facs;
            case "GENE":
                return FilterType.Gene;
            case "ATTRIBUTE":
                return FilterType.Attribute;
            default:
                return FilterType.Invalid;
        }
    }

    public static RuleType StringToRuleType(string s)
    {
        switch (s.ToUpper())
        {
            case "ALL":
                return RuleType.All;
            case "ANY":
                return RuleType.Any;
            default:
                return RuleType.Invalid;
        }
    }

    public static bool StringToAttributeValue(string s)
    {
        switch (s.ToUpper())
        {
            case "YES":
            case "1":
                return true;
            case "NO":
            case "0":
                return false;
            default:
                throw new ArgumentException();
        }
    }
}
