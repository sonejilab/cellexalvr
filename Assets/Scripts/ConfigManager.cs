using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Static class that represents the config file and its contents.
/// </summary>
public static class CellexalConfig
{
    public static string ConfigDir { get; set; }
    public static string RscriptexePath { get; set; }
    public static int GraphLoadingCellsPerFrameStartCount { get; set; }
    public static int GraphLoadingCellsPerFrameIncrement { get; set; }
    public static float GraphGrabbableCollidersExtensionThresehold { get; set; }
    public static Color[] SelectionToolColors { get; set; }
    public static int NumberOfExpressionColors { get; set; }
    public static Color LowExpressionColor { get; set; }
    public static Color MidExpressionColor { get; set; }
    public static Color HighExpressionColor { get; set; }
    public static Color[] AttributeColors { get; set; }
    public static int NumberOfHeatmapColors { get; set; }
    public static Color HeatmapLowExpressionColor { get; set; }
    public static Color HeatmapMidExpressionColor { get; set; }
    public static Color HeatmapHighExpressionColor { get; set; }
    public static Color HeatmapHighlightMarkerColor { get; set; }
    public static Color HeatmapConfirmMarkerColor { get; set; }
    public static int HeatmapNumberOfGenes { get; set; }
    public static float NetworkLineSmallWidth { get; set; }
    public static float NetworkLineLargeWidth { get; set; }
    public static int NetworkLineColoringMethod { get; set; }
    public static int NumberOfNetworkLineColors { get; set; }
    public static Color NetworkLineColorPositiveHigh { get; set; }
    public static Color NetworkLineColorPositiveLow { get; set; }
    public static Color NetworkLineColorNegativeLow { get; set; }
    public static Color NetworkLineColorNegativeHigh { get; set; }
}

/// <summary>
/// This class is a helper class that reads the config file and sets the properties in <see cref="CellexalConfig"/>.
/// </summary>
public class ConfigManager : MonoBehaviour
{

    public ReferenceManager referenceManager;
    private string configDir;
    private string configPath;
    private string sampleConfigPath;

    private void Start()
    {
        configDir = Directory.GetCurrentDirectory() + @"\Config";
        configPath = configDir + @"\config.txt";
        sampleConfigPath = Application.streamingAssetsPath + @"\sample_config.txt";
        ReadConfigFile();

        // set up a filesystemwatcher that notifies us if the file is changed and we should reload it
        FileSystemWatcher watcher = new FileSystemWatcher(configDir);
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Filter = "config.txt";
        watcher.Changed += new FileSystemEventHandler(OnChanged);
        watcher.EnableRaisingEvents = true;

    }

    private void OnChanged(object source, FileSystemEventArgs e)
    {
        // Make the ReadConfigFile execute in the main thread
        SQLiter.LoomManager.Loom.QueueOnMainThread(() => ReadConfigFile());

    }



    private static IEnumerable<String> FindAccessableFiles(string path, string file_pattern, bool recurse)
    {
        Console.WriteLine(path);
        var list = new List<string>();
        var required_extension = "mp4";

        if (File.Exists(path))
        {
            yield return path;
            yield break;
        }

        if (!Directory.Exists(path))
        {
            yield break;
        }

        if (null == file_pattern)
            file_pattern = "*." + required_extension;

        var top_directory = new DirectoryInfo(path);

        // Enumerate the files just in the top directory.
        IEnumerator<FileInfo> files;
        try
        {
            files = top_directory.EnumerateFiles(file_pattern).GetEnumerator();
        }
        catch (Exception ex)
        {
            files = null;
        }

        while (true)
        {
            FileInfo file = null;
            try
            {
                if (files != null && files.MoveNext())
                    file = files.Current;
                else
                    break;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (PathTooLongException)
            {
                continue;
            }

            yield return file.FullName;
        }

        if (!recurse)
            yield break;

        IEnumerator<DirectoryInfo> dirs;
        try
        {
            dirs = top_directory.EnumerateDirectories("*").GetEnumerator();
        }
        catch (Exception ex)
        {
            dirs = null;
        }


        while (true)
        {
            DirectoryInfo dir = null;
            try
            {
                if (dirs != null && dirs.MoveNext())
                    dir = dirs.Current;
                else
                    break;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (PathTooLongException)
            {
                continue;
            }

            foreach (var subpath in FindAccessableFiles(dir.FullName, file_pattern, recurse))
                yield return subpath;
        }
    }

    private void ReadConfigFile()
    {
        // make sure the folder and the file exists.
        if (!Directory.Exists("Config"))
        {
            Directory.CreateDirectory("Config");
            CellexalLog.Log("Created directory " + CellexalLog.FixFilePath(configDir));
        }

        if (!File.Exists(configPath))
        {
            File.Copy(sampleConfigPath, configPath);
            CellexalLog.Log("WARNING: No config file found at " + configPath + ". A sample config file has been created.");
        }
        CellexalLog.Log("Started reading the config file");


        // start reading the contents.
        FileStream fileStream = new FileStream(configPath, FileMode.Open);
        StreamReader streamReader = new StreamReader(fileStream);

        CellexalConfig.ConfigDir = configPath;
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
                CellexalLog.Log("WARNING: Bad line in the config file. No \"=\" found. Line " + lineNbr + ": " + line);
                continue;
            }
            string key = line.Substring(0, equalIndex).Trim();
            string value = line.Substring(equalIndex + 1).Trim();

            if (key.Length == 0)
            {
                CellexalLog.Log("WARNING: Bad line in the config file. No key found. Line " + lineNbr + ": " + line);
                continue;
            }

            if (value.Length == 0)
            {
                CellexalLog.Log("WARNING: Bad line in the config file. No value found. Line " + lineNbr + ": " + line);
                continue;
            }

            switch (key)
            {
                case "RscriptFilePath":
                    CellexalConfig.RscriptexePath = value;
                    break;

                case "GraphLoadingCellsPerFrameStartCount":
                    CellexalConfig.GraphLoadingCellsPerFrameStartCount = int.Parse(value);
                    break;

                case "GraphLoadingCellsPerFrameIncrement":
                    CellexalConfig.GraphLoadingCellsPerFrameIncrement = int.Parse(value);
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
                                CellexalError.SpawnError("Error in config file", "Unexpected end of file when parsing list of selection colors from the config file. File: " + configPath + " at line " + lineNbr);
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
                    CellexalConfig.SelectionToolColors = selectionColorsArray;
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
                                CellexalError.SpawnError("Error in config file", "Unexpected end of file when parsing list of attribute colors from the config file. File: " + configPath + " at line " + lineNbr);
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
                    CellexalConfig.AttributeColors = attributeColors.ToArray();
                    break;

                case "NumberOfExpressionColors":
                    int nColors = int.Parse(value);
                    if (nColors < 3)
                    {
                        CellexalLog.Log("WARNING: Number of gene expression colors is less than 3, changing it to 3.");
                        nColors = 3;
                    }
                    CellexalConfig.NumberOfExpressionColors = nColors;
                    break;

                case "LowExpressionColor":
                    CellexalConfig.LowExpressionColor = ReadColor(value, lineNbr);
                    break;
                case "MidExpressionColor":
                    CellexalConfig.MidExpressionColor = ReadColor(value, lineNbr);
                    break;
                case "HighExpressionColor":
                    CellexalConfig.HighExpressionColor = ReadColor(value, lineNbr);
                    break;

                case "NetworkLineSmallWidth":
                    CellexalConfig.NetworkLineSmallWidth = float.Parse(value);
                    break;
                case "NetworkLineLargeWidth":
                    CellexalConfig.NetworkLineLargeWidth = float.Parse(value);
                    break;
                case "NetworkLineColoringMethod":
                    CellexalConfig.NetworkLineColoringMethod = int.Parse(value);
                    break;
                case "NetworkLineColorPositiveHigh":
                    CellexalConfig.NetworkLineColorPositiveHigh = ReadColor(value, lineNbr);
                    break;
                case "NetworkLineColorPositiveLow":
                    CellexalConfig.NetworkLineColorPositiveLow = ReadColor(value, lineNbr);
                    break;
                case "NetworkLineColorNegativeLow":
                    CellexalConfig.NetworkLineColorNegativeLow = ReadColor(value, lineNbr);
                    break;
                case "NetworkLineColorNegativeHigh":
                    CellexalConfig.NetworkLineColorNegativeHigh = ReadColor(value, lineNbr);
                    break;
                case "NumberOfNetworkLineColors":
                    CellexalConfig.NumberOfNetworkLineColors = int.Parse(value);
                    break;


                case "NumberOfHeatmapColors":
                    int numberOfHeatmapColors = int.Parse(value);
                    if (numberOfHeatmapColors < 3)
                    {
                        CellexalLog.Log("WARNING: Number of heatmap colors is less than 3, changing it to 3.");
                        numberOfHeatmapColors = 3;
                    }
                    CellexalConfig.NumberOfHeatmapColors = numberOfHeatmapColors;
                    break;
                case "HeatmapLowExpressionColor":
                    CellexalConfig.HeatmapLowExpressionColor = ReadColor(value, lineNbr);
                    break;
                case "HeatmapMidExpressionColor":
                    CellexalConfig.HeatmapMidExpressionColor = ReadColor(value, lineNbr);
                    break;
                case "HeatmapHighExpressionColor":
                    CellexalConfig.HeatmapHighExpressionColor = ReadColor(value, lineNbr);
                    break;
                case "HeatmapNumberOfGenes":
                    CellexalConfig.HeatmapNumberOfGenes = int.Parse(value);
                    break;
                case "HeatmapHighlightMarkerColor":
                    CellexalConfig.HeatmapHighlightMarkerColor = ReadColor(value, lineNbr);
                    break;
                case "HeatmapConfirmMarkerColor":
                    CellexalConfig.HeatmapConfirmMarkerColor = ReadColor(value, lineNbr);
                    break;
                case "GraphGrabbableCollidersExtensionThresehold":
                    CellexalConfig.GraphGrabbableCollidersExtensionThresehold = float.Parse(value);
                    break;

                default:
                    CellexalError.SpawnError("Error in config file", "Unknown option " + key + " in file " + configPath + " at line " + lineNbr);
                    break;

            }
        }
        streamReader.Close();
        fileStream.Close();

        CellexalEvents.ConfigLoaded.Invoke();
        CellexalLog.Log("Finished reading the config file");
    }
    /// <summary>
    /// Method to search through the drivers on the computer for the Rscript.
    /// Assigns the path to the config rscriptpath.
    /// </summary>
    private void SearchForRscript()
    {
        string mask = "Rscript.exe";
        string[] drives = Directory.GetLogicalDrives();
        foreach (string dr in drives)
        {
            var files = FindAccessableFiles(dr, mask, true);
            foreach (string file in files)
            {
                CellexalConfig.RscriptexePath = file;
                break;
            }
        }
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
            CellexalLog.Log("WARNING: Bad line in the config file. Expected \'#\' but did not find it at line " + lineNbr + ": " + value);
            return Color.white;
        }
        string hexcolorValue = value.Substring(hashtagIndex);
        string r = hexcolorValue.Substring(1, 2);
        string g = hexcolorValue.Substring(3, 2);
        string b = hexcolorValue.Substring(5, 2);
        float unityR = byte.Parse(r, System.Globalization.NumberStyles.HexNumber) / 255f;
        float unityG = byte.Parse(g, System.Globalization.NumberStyles.HexNumber) / 255f;
        float unityB = byte.Parse(b, System.Globalization.NumberStyles.HexNumber) / 255f;
        if (hexcolorValue.Length == 9)
        {
            // if there is an alpha value as well
            string a = hexcolorValue.Substring(7, 2);
            try
            {
                float unityA = byte.Parse(a, System.Globalization.NumberStyles.HexNumber) / 255f;
                return new Color(unityR, unityG, unityB, unityA);
            }
            catch (System.FormatException e)
            {
                // we found something that seemed like an alpha value, but wasn't
                return new Color(unityR, unityG, unityB);
            }
        }
        return new Color(unityR, unityG, unityB);
    }
}
