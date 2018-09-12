using System;
using System.IO;
using UnityEngine;
using static Filter.FilterRule;

public class FilterManager : MonoBehaviour
{
    public ReferenceManager referenceManager;

    private FilterMenu filterMenu;
    private FileSystemWatcher watcher;

    private void Start()
    {
        FilterParser.referenceManager = referenceManager;
        CellexalEvents.GraphsLoaded.AddListener(ReadFilters);
        filterMenu = referenceManager.filterMenu;


    }

    private void ReadFilters()
    {
        ReadFilters(null, null);
    }

    private void ReadFilters(object source, FileSystemEventArgs e)
    {
        string filterDir = referenceManager.selectionToolHandler.DataDir;
        if (watcher != null)
            watcher.Dispose();
        watcher = new FileSystemWatcher(filterDir);
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Filter = "*.fil";
        watcher.Changed += new FileSystemEventHandler(ReadFilters);
        watcher.EnableRaisingEvents = true;

        filterMenu.RemoveButtons();
        foreach (string file in Directory.GetFiles(filterDir, "*.fil"))
        {
            var filter = FilterParser.ParseFilterFromFile(file);
            //filter.Load();
            // find the last slash
            int lastForwardSlash = file.LastIndexOf("/");
            int lastBackwardSlash = file.LastIndexOf("\\");
            int lastSlash = lastBackwardSlash > lastForwardSlash ? lastBackwardSlash : lastForwardSlash;
            string filterName = file.Substring(lastSlash);
            // remove the .fil
            filterName = filterName.Substring(0, filterName.Length - 4);
            filterMenu.AddFilterButton(filter, filterName);
            //print(filter.ToString());
        }
    }
}

public static class FilterParser
{
    public static ReferenceManager referenceManager;
    private static string[] conditions = { "=", "!=", "<=", "<", ">=", ">" };

    /// <summary>
    /// Parses a file and creates a filter from the contents. See the manual for the filter file syntax.
    /// </summary>
    /// <param name="filepath">The path to the file.</param>
    /// <returns>A <see cref="Filter"/> created from the file.</returns>
    public static Filter ParseFilterFromFile(string filepath)
    {

        StreamReader streamReader = new StreamReader(filepath);
        int lineNbr = 0;
        var filter = new Filter(referenceManager);
        var ruleType = RuleType.Invalid;
        while (!streamReader.EndOfStream)
        {
            string line = streamReader.ReadLine();
            lineNbr++;
            if (line.Length == 0) continue;
            if (line[0] == '#') continue;
            line = line.Trim();
            int firstSpace = line.IndexOf(' ');

            if (firstSpace == -1)
            {
                var newRuleType = Filter.StringToRuleType(line);
                if (newRuleType == RuleType.Invalid)
                {
                    LogError("Ignoring line because \"" + line + "\" was not a valid rule type", filepath, lineNbr, line);
                    continue;
                }
                else
                {
                    ruleType = newRuleType;
                    continue;
                }
            }

            string filterTypeString = line.Substring(0, firstSpace);
            var filterType = Filter.StringToFilterType(filterTypeString);
            if (filterType == FilterType.Invalid)
            {
                LogError("Ignoring line because \"" + filterTypeString + "\" was not a valid filter type", filepath, lineNbr, line);
                continue;
            }
            if (filterType == FilterType.Attribute)
            {
                // attributes are a bit special, they have "YES" or "NO" following them, not a condition
                string attribute = line.Substring(firstSpace);
                if (attribute == string.Empty)
                {
                    LogError("Ignoring line because no attribute to filter was found", filepath, lineNbr, line);
                    continue;
                }
                int lastSpace = line.LastIndexOf(' ');
                attribute = line.Substring(firstSpace, lastSpace - firstSpace).Trim();
                string attributeValueString = line.Substring(lastSpace).Trim();
                try
                {
                    bool attributeValue = Filter.StringToAttributeValue(attributeValueString);
                    var attributeFilterRule = new Filter.FilterRule(filterType, attribute, attributeValue);
                    attributeFilterRule.ruleType = ruleType;
                    filter.AddRule(attributeFilterRule);
                }
                catch (ArgumentException e)
                {
                    LogError("Ignoring line because \"" + attributeValueString + "\" was not a valid attribute value", filepath, lineNbr, line);
                }
                continue;
            }

            Condition cond;
            int conditionIndex = IndexOfAny(line, conditions, out cond);
            if (conditionIndex == -1)
            {
                LogError("Ignoring line because no condition was found", filepath, lineNbr, line);
                continue;
            }

            string filterItem = line.Substring(firstSpace, conditionIndex - firstSpace).Trim();
            if (filterItem == string.Empty)
            {
                LogError("Ignoring line because no item to filter was found", filepath, lineNbr, line);
                continue;
            }

            int conditionLength = Filter.ConditionStringLength(cond);
            string valueString = line.Substring(conditionIndex + conditionLength).Trim();
            float value = 0f;
            try
            {
                value = float.Parse(valueString);
            }
            catch (FormatException e)
            {
                LogError("Ignoring line becase the value \"" + valueString + "\"  was not a valid floating point number", filepath, lineNbr, line);
                continue;
            }
            //var filterRule
            var filterRule = new Filter.FilterRule(filterType, filterItem, cond, value);
            filterRule.ruleType = ruleType;
            filter.AddRule(filterRule);

        }
        streamReader.Close();
        return filter;
    }

    /// <summary>
    /// Logs a error message. Use <see cref="CellexalError.SpawnError(string, string)"/> to show the user an error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="filepath">The file the error occured in.</param>
    /// <param name="lineNbr">The line number the error occured at.</param>
    /// <param name="line">The line that caused the error.</param>
    private static void LogError(string message, string filepath, int lineNbr, string line)
    {
        CellexalLog.Log("WARNING: " + message + ". In file " + filepath + " on line " + lineNbr + ": " + line + ".");
    }

    /// <summary>
    /// Finds the first occurance of any string in an array. Used for finding where and which condition is in a line in a filter file.
    /// </summary>
    /// <param name="s">The string to look in.</param>
    /// <param name="stringsToLookFor">An array with strings to look for in <paramref name="s"/>.</param>
    /// <param name="cond">The <see cref="FilterRule.Condition"/> that was found</param>
    /// <returns>The index that the condition was found on. Or -1 if no occurance was found.</returns>
    private static int IndexOfAny(string s, string[] stringsToLookFor, out Condition cond)
    {
        // loop to go through the string
        for (int i = 0; i < s.Length; ++i)
        {
            // loop to check all different strings to look for
            for (int j = 0; j < stringsToLookFor.Length; ++j)
            {
                // loop to check one thing to look for
                for (int k = 0; k < stringsToLookFor[j].Length; ++k)
                {
                    if (s[i + k] != stringsToLookFor[j][k])
                    {
                        // if it does not match, try the next one
                        break;
                    }
                    else if (k == stringsToLookFor[j].Length - 1)
                    {
                        // if it matched and this was the last character of the thing to look for, return the start index
                        cond = Filter.StringToFilterRule(stringsToLookFor[j]);
                        return i;
                    }
                }
            }

        }
        // if nothing was found
        cond = Condition.Invalid;
        return -1;
    }
}

