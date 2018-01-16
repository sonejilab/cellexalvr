﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// This static class represents the config file and its contents.
/// </summary>
public static class CellExAlConfig
{
    public static string ConfigDir { get; set; }
    public static string RscriptexePath { get; set; }
    public static int GraphLoadingCellsPerFrameStartCount { get; set; }
    public static int GraphLoadingCellsPerFrameIncrement { get; set; }
    public static Color[] SelectionToolColors { get; set; }
    public static int NumberOfExpressionColors { get; set; }
    public static Color LowExpressionColor { get; set; }
    public static Color MidExpressionColor { get; set; }
    public static Color HighExpressionColor { get; set; }
    public static Color[] AttributeColors { get; set; }
    public static float NetworkLineSmallWidth { get; set; }
    public static float NetworkLineLargeWidth { get; set; }
    public static int NumberOfNetworkLineColors { get; set; }
}

/// <summary>
/// This class is a helper class that reads the config file and sets the properties in <see cref="CellExAlConfig"/>.
/// </summary>
public class ConfigManager : MonoBehaviour
{

    public ReferenceManager referenceManager;

    private void Start()
    {
        string workingDir = Directory.GetCurrentDirectory();
        string configPath = workingDir + @"\Config\config.txt";
        string sampleConfigPath = Application.streamingAssetsPath + @"\sample_config.txt";

        // make sure the folder and the file exists.
        if (!Directory.Exists("Config"))
        {
            Directory.CreateDirectory("Config");
            CellExAlLog.Log("Created directory " + CellExAlLog.FixFilePath(workingDir + @"\Config"));
        }

        if (!File.Exists(configPath))
        {
            File.Copy(sampleConfigPath, configPath);
            CellExAlLog.Log("WARNING: No config file found at " + configPath + ". A sample config file has been created.");
        }
        CellExAlLog.Log("Started reading the config file");

        // start reading the contents.
        FileStream fileStream = new FileStream(configPath, FileMode.Open);
        StreamReader streamReader = new StreamReader(fileStream);

        CellExAlConfig.ConfigDir = configPath;
        int lineNbr = 0;
        while (!streamReader.EndOfStream)
        {
            lineNbr++;
            string line = streamReader.ReadLine();
            // ignore empty lines and line with only whitespace
            if (line.Trim() == "") continue;
            // comments start with #
            if (line[0] == '#') continue;

            // everything else is assumed to be a line on the format
            // [KEY] = [VALUE]
            int equalIndex = line.IndexOf("=", System.StringComparison.Ordinal);

            // if a '=' is not found
            if (equalIndex == -1)
            {
                CellExAlLog.Log("WARNING: Bad line in the config file. No \"=\" found. Line " + lineNbr + ": " + line);
                continue;
            }
            string key = line.Substring(0, equalIndex).Trim();
            string value = line.Substring(equalIndex + 1).Trim();

            if (key.Length == 0)
            {
                CellExAlLog.Log("WARNING: Bad line in the config file. No key found. Line " + lineNbr + ": " + line);
                continue;
            }

            if (value.Length == 0)
            {
                CellExAlLog.Log("WARNING: Bad line in the config file. No value found. Line " + lineNbr + ": " + line);
                continue;
            }

            switch (key)
            {
                case "RscriptFilePath":
                    CellExAlConfig.RscriptexePath = value;
                    break;

                case "GraphLoadingCellsPerFrameStartCount":
                    CellExAlConfig.GraphLoadingCellsPerFrameStartCount = int.Parse(value);
                    break;

                case "GraphLoadingCellsPerFrameIncrement":
                    CellExAlConfig.GraphLoadingCellsPerFrameIncrement = int.Parse(value);
                    break;

                case "SelectionColors":
                    List<Color> selectionColors = new List<Color>();
                    while (true)
                    {
                        Color newColor = ReadColor(value, lineNbr);
                        newColor.a = 0.5f;
                        selectionColors.Add(newColor);

                        // a '}' denotes the end of the list
                        if (!value.Contains("}"))
                        {
                            if (streamReader.EndOfStream)
                            {
                                CellExAlLog.Log("WARNING: Unexpected end of file when parsing list of selection colors from the config file.");
                                break;
                            }
                            lineNbr++;
                            value = streamReader.ReadLine();
                        }
                        else
                        {
                            break;
                        }
                    }
                    Color[] selectionColorsArray = selectionColors.ToArray();
                    // the selection tool handler is not active when the config is being read
                    SelectionToolHandler selectionToolHandler = referenceManager.selectionToolHandler;
                    CellExAlConfig.SelectionToolColors = selectionColorsArray;
                    selectionToolHandler.UpdateColors();
                    break;

                case "AttributeColors":
                    List<Color> attributeColors = new List<Color>();
                    while (true)
                    {
                        Color newColor = ReadColor(value, lineNbr);
                        newColor.a = 0.5f;
                        attributeColors.Add(newColor);

                        // a '}' denotes the end of the list
                        if (!value.Contains("}"))
                        {
                            if (streamReader.EndOfStream)
                            {
                                CellExAlLog.Log("WARNING: Unexpected end of file when parsing list of attribute colors from the config file.");
                                break;
                            }
                            lineNbr++;
                            value = streamReader.ReadLine();
                        }
                        else
                        {
                            break;
                        }
                    }
                    CellExAlConfig.AttributeColors = attributeColors.ToArray();
                    break;

                case "NumberOfExpressionColors":
                    int nColors = int.Parse(value);
                    if (nColors < 3)
                    {
                        CellExAlLog.Log("WARNING: Number of gene expression colors is less than 3, changing it to 3.");
                        nColors = 3;
                    }
                    CellExAlConfig.NumberOfExpressionColors = nColors;
                    break;

                case "LowExpressionColor":
                    CellExAlConfig.LowExpressionColor = ReadColor(value, lineNbr);
                    break;

                case "MidExpressionColor":
                    CellExAlConfig.MidExpressionColor = ReadColor(value, lineNbr);
                    break;

                case "HighExpressionColor":
                    CellExAlConfig.HighExpressionColor = ReadColor(value, lineNbr);
                    break;

                case "NetworkLineSmallWidth":
                    CellExAlConfig.NetworkLineSmallWidth = float.Parse(value);
                    break;

                case "NetworkLineLargeWidth":
                    CellExAlConfig.NetworkLineLargeWidth = float.Parse(value);
                    break;
                case "NumberOfNetworkLineColors":
                    CellExAlConfig.NumberOfNetworkLineColors = int.Parse(value);
                    break;

                default:
                    CellExAlLog.Log("WARNING: Unknown option in the config file. At line " + lineNbr + ": " + line);
                    break;

            }
        }
        streamReader.Close();
        fileStream.Close();

        CellExAlEvents.ConfigLoaded.Invoke();
        CellExAlLog.Log("Finished reading the config file");
    }

    /// <summary>
    /// Helper method to extract a hexadecimal value from a string
    /// </summary>
    /// <param name="value"> The string containing the value</param>
    /// <param name="lineNbr"> The line number that this string was found on, used for error messages. </param>
    /// <returns> A <see cref="Color"/> that the hexadecimal values represented. </returns>
    private Color ReadColor(string value, int lineNbr)
    {
        int hashtagIndex = value.IndexOf('#');
        if (hashtagIndex == -1)
        {
            CellExAlLog.Log("WARNING: Bad line in the config file. Expected \'#\' but did not find it at line " + lineNbr + ": " + value);
            return Color.white;
        }
        string hexcolorValue = value.Substring(hashtagIndex, 7);
        Color newColor = new Color();
        ColorUtility.TryParseHtmlString(hexcolorValue, out newColor);
        return newColor;
    }
}