using System.IO;
using UnityEngine;

/// <summary>
/// This static class represents the config file and its contents. Not very useful yet, but might be when the config file contains more info.
/// </summary>
public static class CellExAlConfig
{
    public static string ConfigDir { get; set; }
    public static string RScriptexePath { get; set; }
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

        // make sure the folder and the file exists.
        if (!Directory.Exists("Config"))
        {
            CellExAlLog.Log("Created directory " + CellExAlLog.FixFilePath(configPath));
        }

        if (!File.Exists(configPath))
        {
            CellExAlLog.Log("ERROR: No config file found at " + configPath + ". Cellexal can not run any R scripts without this file. Read the readme for more information.");
            return;
        }
        CellExAlLog.Log("Started reading the config file");

        // start reading the contents.
        FileStream fileStream = new FileStream(configPath, FileMode.Open);
        StreamReader streamReader = new StreamReader(fileStream);

        string rscriptfilepath = streamReader.ReadLine();

        streamReader.Close();
        fileStream.Close();

        CellExAlLog.Log("Successfully read the config file");

        CellExAlConfig.ConfigDir = configPath;
        CellExAlConfig.RScriptexePath = rscriptfilepath;


    }
}
