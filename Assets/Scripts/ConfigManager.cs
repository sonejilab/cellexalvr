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
}

/// <summary>
/// This class is a helper class that reads the config file and sets the properties in <see cref="CellExAlConfig"/>.
/// </summary>
public class ConfigManager : MonoBehaviour
{
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
            return;
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
            // ignore empty lines
            if (line.Length == 0) continue;
            // comments start with #
            if (line[0] == '#') continue;

            // everything else is assumed to be a line on the format
            // [KEY] = [VALUE]
            int equalIndex = line.IndexOf("=", System.StringComparison.Ordinal);

            // if a '=' is not found
            if (equalIndex == -1)
            {
                CellExAlLog.Log("WARNING: Misformatted line in the config file. No \"=\" found. Line " + lineNbr + ": " + line);
                continue;
            }
            string key = line.Substring(0, equalIndex).Trim();
            string value = line.Substring(equalIndex + 1).Trim();

            if (key.Length == 0)
            {
                CellExAlLog.Log("WARNING: Misformatted line in the config file. No key found. Line " + lineNbr + ": " + line);
                continue;
            }

            if (value.Length == 0)
            {
                CellExAlLog.Log("WARNING: Misformatted line in the config file. No value found. Line " + lineNbr + ": " + line);
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
                default:
                    CellExAlLog.Log("WARNING: Unknown option in the config file. At line " + lineNbr + ": " + line);
                    break;

            }
        }
        streamReader.Close();
        fileStream.Close();

        CellExAlLog.Log("Successfully read the config file");
    }
}
