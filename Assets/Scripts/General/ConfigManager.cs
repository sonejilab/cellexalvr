﻿using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace CellexalVR.General
{
    /// <summary>
    /// Static class that represents the config file and its contents. To properly get a value from the config when the program is started, subscribe to the <see cref="CellexalEvents.ConfigLoaded"/> event in Awake.
    /// </summary>
    public static class CellexalConfig
    {
        public static Config Config { get; set; }

        public static Dictionary<string, Config> savedConfigs { get; set; }

        public static bool configLoaded = false;


    }

    /// <summary>
    /// Represents the configurable options in the settings menu.
    /// </summary>
    public class Config
    {
        public string ConfigDir { get; set; }
        public string RscriptexePath { get; set; }
        public string ScarfscriptPath { get; set; }
        public bool RequireTouchpadClickToInteract { get; set; }
        public string ControllerModel { get; set; }
        public int GraphLoadingCellsPerFrameStartCount { get; set; }
        public int GraphLoadingCellsPerFrameIncrement { get; set; }
        public int GraphClustersPerFrameStartCount { get; set; }
        public int GraphClustersPerFrameIncrement { get; set; }
        public float GraphGrabbableCollidersExtensionThresehold { get; set; }
        public Color[] SelectionToolColors { get; set; }
        public Color GraphDefaultColor { get; set; }
        public Color GraphZeroExpressionColor { get; set; }
        public int GraphNumberOfExpressionColors { get; set; }
        public Color GraphLowExpressionColor { get; set; }
        public Color GraphMidExpressionColor { get; set; }
        public Color GraphHighExpressionColor { get; set; }
        public bool GraphMostExpressedMarker { get; set; }
        public Color[] AttributeColors { get; set; }
        public int NumberOfHeatmapColors { get; set; }
        public Color HeatmapLowExpressionColor { get; set; }
        public Color HeatmapMidExpressionColor { get; set; }
        public Color HeatmapHighExpressionColor { get; set; }
        public string HeatmapAlgorithm { get; set; }
        public string NetworkAlgorithm { get; set; }
        public int HeatmapNumberOfGenes { get; set; }
        public int NetworkLineColoringMethod { get; set; }
        public Color NetworkLineColorPositiveHigh { get; set; }
        public Color NetworkLineColorPositiveLow { get; set; }
        public Color NetworkLineColorNegativeLow { get; set; }
        public Color NetworkLineColorNegativeHigh { get; set; }
        public int NumberOfNetworkLineColors { get; set; }
        public float NetworkLineWidth { get; set; }
        public Color VelocityParticlesLowColor { get; set; }
        public Color VelocityParticlesHighColor { get; set; }
        public int ConsoleMaxBufferLines { get; set; }
        public bool ShowNotifications { get; set; }
        public string GraphPointQuality { get; set; }
        public string GraphPointSize { get; set; }
        public Color SkyboxTintColor { get; set; }


        public Config() { }

        public Config(Config c)
        {
            ConfigDir = c.ConfigDir;
            if (CrossSceneInformation.RScriptPath != null)
            {
                RscriptexePath = CrossSceneInformation.RScriptPath;
                c.RscriptexePath = RscriptexePath;
            }
            else
            {
                RscriptexePath = c.RscriptexePath;
            }
            ScarfscriptPath = c.ScarfscriptPath;
            RequireTouchpadClickToInteract = c.RequireTouchpadClickToInteract;
            ControllerModel = c.ControllerModel;
            GraphLoadingCellsPerFrameStartCount = c.GraphLoadingCellsPerFrameStartCount;
            GraphLoadingCellsPerFrameIncrement = c.GraphLoadingCellsPerFrameIncrement;
            GraphClustersPerFrameStartCount = c.GraphClustersPerFrameStartCount;
            GraphClustersPerFrameIncrement = c.GraphClustersPerFrameIncrement;
            GraphGrabbableCollidersExtensionThresehold = c.GraphGrabbableCollidersExtensionThresehold;
            SelectionToolColors = c.SelectionToolColors;
            GraphDefaultColor = c.GraphDefaultColor;
            GraphZeroExpressionColor = c.GraphZeroExpressionColor;
            GraphNumberOfExpressionColors = c.GraphNumberOfExpressionColors;
            GraphLowExpressionColor = c.GraphLowExpressionColor;
            GraphMidExpressionColor = c.GraphMidExpressionColor;
            GraphHighExpressionColor = c.GraphHighExpressionColor;
            GraphMostExpressedMarker = c.GraphMostExpressedMarker;
            AttributeColors = c.AttributeColors;
            NumberOfHeatmapColors = c.NumberOfHeatmapColors;
            HeatmapLowExpressionColor = c.HeatmapLowExpressionColor;
            HeatmapMidExpressionColor = c.HeatmapMidExpressionColor;
            HeatmapHighExpressionColor = c.HeatmapHighExpressionColor;
            HeatmapAlgorithm = c.HeatmapAlgorithm;
            NetworkAlgorithm = c.NetworkAlgorithm;
            HeatmapNumberOfGenes = c.HeatmapNumberOfGenes;
            NetworkLineColoringMethod = c.NetworkLineColoringMethod;
            NetworkLineColorPositiveHigh = c.NetworkLineColorPositiveHigh;
            NetworkLineColorPositiveLow = c.NetworkLineColorPositiveLow;
            NetworkLineColorNegativeLow = c.NetworkLineColorNegativeLow;
            NetworkLineColorNegativeHigh = c.NetworkLineColorNegativeHigh;
            NumberOfNetworkLineColors = c.NumberOfNetworkLineColors;
            NetworkLineWidth = c.NetworkLineWidth;
            VelocityParticlesLowColor = c.VelocityParticlesLowColor;
            VelocityParticlesHighColor = c.VelocityParticlesHighColor;
            ConsoleMaxBufferLines = c.ConsoleMaxBufferLines;
            ShowNotifications = c.ShowNotifications;
            GraphPointQuality = c.GraphPointQuality;
            GraphPointSize = c.GraphPointSize;
            SkyboxTintColor = c.SkyboxTintColor;
        }
    }

    /// <summary>
    /// This class is a helper class that reads the config file and sets the properties in <see cref="CellexalConfig.Config"/>.
    /// </summary>
    public class ConfigManager : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public string currentProfileFullPath;

        private string configDir;
        private string sharedConfigDir;
        private string defaultConfigPath;
        private List<string> configPaths;
        private string sampleConfigPath;
        private bool multiUserSynchronise;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            configDir = Path.Combine(Directory.GetCurrentDirectory(), "Config");
            if (!Directory.Exists(configDir))
            {
                CellexalLog.Log("Config directory not found, creating " + configDir);
                Directory.CreateDirectory(configDir);
            }

            sharedConfigDir = Path.Combine(configDir, "Shared");
            if (!Directory.Exists(sharedConfigDir))
            {

                CellexalLog.Log("Shared config directory not found, creating " + sharedConfigDir);
                Directory.CreateDirectory(sharedConfigDir);
            }

            defaultConfigPath = Path.Combine(configDir, "default_config.xml");
            currentProfileFullPath = defaultConfigPath;
            sampleConfigPath = Path.Combine(Application.streamingAssetsPath, "sample_config.xml");
            if (!File.Exists(defaultConfigPath))
            {
                File.Copy(sampleConfigPath, defaultConfigPath);
                CellexalLog.Log("WARNING: No default config file found at " + defaultConfigPath + ". A sample config file has been created.");
            }

            if (CellexalLog.consoleManager == null)
            {
                CellexalLog.consoleManager = referenceManager.consoleManager;
            }
            configPaths = new List<string>();
            ReadConfigFiles(configDir);
            CellexalConfig.Config = CellexalConfig.savedConfigs["default"];
            CellexalConfig.configLoaded = true;
            CellexalEvents.ConfigLoaded.Invoke();
        }

        public void ReadConfigFiles(string folderPath)
        {
            // read all configs in this folder, add only the ones that are not already added
            string[] newConfigs = Directory.GetFiles(folderPath, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (string configPath in newConfigs)
            {
                if (!configPaths.Contains(configPath))
                {
                    configPaths.Add(configPath);
                }
            }
            CellexalConfig.savedConfigs = new Dictionary<string, Config>();
            foreach (string path in configPaths)
            {
                ReadConfigFile(path, switchTo: false);
            }
            //SaveConfigFile();
            // set up a filesystemwatcher that notifies us if the file is changed and we should reload it
            FileSystemWatcher watcher = new FileSystemWatcher(configDir);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.xml";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;

        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // If config is being synchronised with other user. Do not re-read local config.
            if (!multiUserSynchronise)
            {
                // Make the ReadConfigFile execute in the main thread
                SQLiter.LoomManager.Loom.QueueOnMainThread(() => ReadConfigFile(e.Name));
            }
        }

        /// <summary>
        /// Copies the deafult config file and overwrites the current one.
        /// </summary>
        public void ResetToDefault()
        {
            File.Copy(sampleConfigPath, defaultConfigPath, true);
            ReadConfigFile(defaultConfigPath);
        }

        /// <summary>
        /// Reads a config file.
        /// </summary>
        /// <param name="configPath">The full path to the config file.</param>
        /// <param name="switchTo">True if this config should become the new config to use right now, setting this to false disables invoking the <see cref="CellexalEvents.ConfigLoaded"/> event.</param>
        public void ReadConfigFile(string configPath, bool switchTo = true)
        {
            // make sure the folder and the file exists.
            if (!Directory.Exists("Config"))
            {
                Directory.CreateDirectory("Config");
                CellexalLog.Log("Created directory " + CellexalLog.FixFilePath(configDir));
            }

            if (!File.Exists(configPath))
            {
                CellexalLog.Log("WARNING: No config file found at " + configPath + ". Reading default config instead.");
                if (!File.Exists(defaultConfigPath))
                {
                    File.Copy(sampleConfigPath, defaultConfigPath);
                    CellexalLog.Log("WARNING: No default config file found at " + defaultConfigPath + ". A sample config file has been created.");
                }
                configPath = defaultConfigPath;
            }
            if (!configPaths.Contains(configPath))
            {
                configPaths.Add(configPath);
            }
            CellexalLog.Log($"Started reading the config file at {configPath}");
            int indexOfLastSlash = Mathf.Max(configPath.LastIndexOf('/'), configPath.LastIndexOf('\\'));
            int indexOfLastUnderscore = configPath.LastIndexOf('_');
            string profileName = configPath.Substring(indexOfLastSlash + 1, indexOfLastUnderscore - indexOfLastSlash - 1);
            FileStream fileStream = new FileStream(configPath, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            CellexalConfig.savedConfigs[profileName] = (Config)serializer.Deserialize(streamReader);
            streamReader.Close();
            fileStream.Close();
            CellexalLog.Log($"Finished reading the config file at {configPath}");
            if (switchTo)
            {
                CellexalConfig.Config = CellexalConfig.savedConfigs[profileName];
                currentProfileFullPath = configPath;
                CellexalEvents.ConfigLoaded.Invoke();
            }
        }

        /// <summary>
        /// Saves the current config as a file. If a file already exists at the specified path, it will be overwritten.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        public void SaveConfigFile(string path)
        {
            if (!Directory.Exists("Config"))
            {
                Directory.CreateDirectory("Config");
                CellexalLog.Log("Created directory " + CellexalLog.FixFilePath(configDir));
            }

            CellexalLog.Log("Started saving the config file");

            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            XmlSerializer ser = new XmlSerializer(typeof(Config));
            ser.Serialize(streamWriter, CellexalConfig.Config);
            streamWriter.Close();
            fileStream.Close();

        }

        public string ProfileNameToConfigPath(string profileName)
        {
            if (!CellexalConfig.savedConfigs.ContainsKey(profileName))
            {
                return "";
            }
            return Path.Combine(Directory.GetCurrentDirectory(), CellexalConfig.savedConfigs[profileName].ConfigDir, profileName + "_config.xml");
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
                catch (System.FormatException)
                {
                    // we found something that seemed like an alpha value, but wasn't
                    return new Color(unityR, unityG, unityB);
                }
            }
            return new Color(unityR, unityG, unityB);
        }

        /// <summary>
        /// Synchronizes our config with a clients.
        /// </summary>
        public void MultiUserSynchronise()
        {
            byte[] data = SerializeConfig(CellexalConfig.Config);

            referenceManager.multiuserMessageSender.SendMessageSynchConfig(data);


            string sharedConfigPath = Path.Combine(sharedConfigDir, "shared_config.xml");
            //if (!File.Exists(sharedConfigPath))
            //{
            //    File.Create(sharedConfigPath);
            //}

            defaultConfigPath = sharedConfigPath;
            SaveConfigFile(sharedConfigPath);
            ReadConfigFile(sharedConfigPath);
        }

        /// <summary>
        /// Serializes the config.
        /// </summary>
        /// <param name="serializableConfig">The config to serialize</param>
        /// <returns>The raw byte data representing the config.</returns>
        public byte[] SerializeConfig(Config serializableConfig)
        {
            Config config = serializableConfig;
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer ser = new XmlSerializer(typeof(Config));
                ser.Serialize(stream, config);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes the config.
        /// </summary>
        /// <param name="serializedBytes">The raw byte data, preferably coming from <see cref="SerializeConfig(Config)"/>.</param>
        /// <returns>The deserialized config.</returns>
        public Config DeserializeConfig(byte[] serializedBytes)
        {
            XmlSerializer ser = new XmlSerializer(typeof(Config));
            using (MemoryStream stream = new MemoryStream(serializedBytes))
            {
                return (Config)ser.Deserialize(stream);
            }
        }

        /// <summary>
        /// Synchronized our config with the serialized data from another clients config.
        /// </summary>
        /// <param name="data"></param>
        public void SynchroniseConfig(byte[] data)
        {
            Config config = DeserializeConfig(data);
            // Only change the parts that need to be.
            CellexalConfig.Config.SelectionToolColors = config.SelectionToolColors;
            CellexalConfig.Config.GraphDefaultColor = config.GraphDefaultColor;
            CellexalConfig.Config.GraphZeroExpressionColor = config.GraphZeroExpressionColor;
            CellexalConfig.Config.GraphNumberOfExpressionColors = config.GraphNumberOfExpressionColors;
            CellexalConfig.Config.GraphLowExpressionColor = config.GraphLowExpressionColor;
            CellexalConfig.Config.GraphMidExpressionColor = config.GraphMidExpressionColor;
            CellexalConfig.Config.GraphHighExpressionColor = config.GraphHighExpressionColor;
            CellexalConfig.Config.GraphMostExpressedMarker = config.GraphMostExpressedMarker;
            CellexalConfig.Config.AttributeColors = config.AttributeColors;
            CellexalConfig.Config.NumberOfHeatmapColors = config.NumberOfHeatmapColors;
            CellexalConfig.Config.HeatmapLowExpressionColor = config.HeatmapLowExpressionColor;
            CellexalConfig.Config.HeatmapMidExpressionColor = config.HeatmapMidExpressionColor;
            CellexalConfig.Config.HeatmapHighExpressionColor = config.HeatmapHighExpressionColor;
            CellexalConfig.Config.HeatmapAlgorithm = config.HeatmapAlgorithm;
            CellexalConfig.Config.NetworkAlgorithm = config.NetworkAlgorithm;
            CellexalConfig.Config.HeatmapNumberOfGenes = config.HeatmapNumberOfGenes;
            CellexalConfig.Config.NetworkLineColoringMethod = config.NetworkLineColoringMethod;
            CellexalConfig.Config.NetworkLineColorPositiveHigh = config.NetworkLineColorPositiveHigh;
            CellexalConfig.Config.NetworkLineColorPositiveLow = config.NetworkLineColorPositiveLow;
            CellexalConfig.Config.NetworkLineColorNegativeLow = config.NetworkLineColorNegativeLow;
            CellexalConfig.Config.NetworkLineColorNegativeHigh = config.NetworkLineColorNegativeHigh;
            CellexalConfig.Config.NumberOfNetworkLineColors = config.NumberOfNetworkLineColors;
            CellexalConfig.Config.NetworkLineWidth = config.NetworkLineWidth;
            CellexalConfig.Config.VelocityParticlesLowColor = config.VelocityParticlesLowColor;
            CellexalConfig.Config.VelocityParticlesHighColor = config.VelocityParticlesHighColor;

            string sharedConfigPath = Path.Combine(sharedConfigDir, "shared_config.xml");
            defaultConfigPath = sharedConfigPath;
            SaveConfigFile(sharedConfigPath);
            ReadConfigFile(sharedConfigPath);
        }

    }
}
