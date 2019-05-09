using System.IO;
using UnityEngine;
using System.Xml.Serialization;
namespace CellexalVR.General
{
    /// <summary>
    /// Static class that represents the config file and its contents. To properly get a value from the config when the program is started, subscribe to the <see cref="CellexalEvents.ConfigLoaded"/> event in Awake.
    /// </summary>
    public static class CellexalConfig
    {
        public static Config Config { get; set; }
    }

    /// <summary>
    /// Represents the configurable options in the settings menu.
    /// </summary>
    public class Config
    {
        public string ConfigDir { get; set; }
        public string RscriptexePath { get; set; }
        public int GraphLoadingCellsPerFrameStartCount { get; set; }
        public int GraphLoadingCellsPerFrameIncrement { get; set; }
        public int GraphClustersPerFrameStartCount { get; set; }
        public int GraphClustersPerFrameIncrement { get; set; }
        public float GraphGrabbableCollidersExtensionThresehold { get; set; }
        public Color[] SelectionToolColors { get; set; }
        public Color GraphDefaultColor { get; set; }
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
        public Color HeatmapHighlightMarkerColor { get; set; }
        public Color HeatmapConfirmMarkerColor { get; set; }
        public string HeatmapAlgorithm { get; set; }
        public int HeatmapNumberOfGenes { get; set; }
        public int NetworkLineColoringMethod { get; set; }
        public Color NetworkLineColorPositiveHigh { get; set; }
        public Color NetworkLineColorPositiveLow { get; set; }
        public Color NetworkLineColorNegativeLow { get; set; }
        public Color NetworkLineColorNegativeHigh { get; set; }
        public int NumberOfNetworkLineColors { get; set; }
        public float NetworkLineWidth { get; set; }
        public int ConsoleMaxBufferLines { get; set; }
        public bool ShowNotifications { get; set; }

        public Config() { }

        public Config(Config c)
        {
            ConfigDir = c.ConfigDir;
            RscriptexePath = c.RscriptexePath;
            GraphLoadingCellsPerFrameStartCount = c.GraphLoadingCellsPerFrameStartCount;
            GraphLoadingCellsPerFrameIncrement = c.GraphLoadingCellsPerFrameIncrement;
            GraphClustersPerFrameStartCount = c.GraphClustersPerFrameStartCount;
            GraphClustersPerFrameIncrement = c.GraphClustersPerFrameIncrement;
            GraphGrabbableCollidersExtensionThresehold = c.GraphGrabbableCollidersExtensionThresehold;
            SelectionToolColors = c.SelectionToolColors;
            GraphDefaultColor = c.GraphDefaultColor;
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
            HeatmapHighlightMarkerColor = c.HeatmapHighlightMarkerColor;
            HeatmapConfirmMarkerColor = c.HeatmapConfirmMarkerColor;
            HeatmapAlgorithm = c.HeatmapAlgorithm;
            HeatmapNumberOfGenes = c.HeatmapNumberOfGenes;
            NetworkLineColoringMethod = c.NetworkLineColoringMethod;
            NetworkLineColorPositiveHigh = c.NetworkLineColorPositiveHigh;
            NetworkLineColorPositiveLow = c.NetworkLineColorPositiveLow;
            NetworkLineColorNegativeLow = c.NetworkLineColorNegativeLow;
            NetworkLineColorNegativeHigh = c.NetworkLineColorNegativeHigh;
            NumberOfNetworkLineColors = c.NumberOfNetworkLineColors;
            NetworkLineWidth = c.NetworkLineWidth;
            ConsoleMaxBufferLines = c.ConsoleMaxBufferLines;
            ShowNotifications = c.ShowNotifications;

        }
    }

    /// <summary>
    /// This class is a helper class that reads the config file and sets the properties in <see cref="CellexalConfig.Config"/>.
    /// </summary>
    public class ConfigManager : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        private string configDir;
        private string configPath;
        private string sampleConfigPath;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            configDir = Directory.GetCurrentDirectory() + @"\Config";
            configPath = configDir + @"\config.xml";
            sampleConfigPath = Application.streamingAssetsPath + @"\sample_config.xml";
            if (CellexalLog.consoleManager == null)
            {
                CellexalLog.consoleManager = referenceManager.consoleManager;
            }
            ReadConfigFile();
            //SaveConfigFile();
            // set up a filesystemwatcher that notifies us if the file is changed and we should reload it
            FileSystemWatcher watcher = new FileSystemWatcher(configDir);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "config.xml";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Make the ReadConfigFile execute in the main thread
            SQLiter.LoomManager.Loom.QueueOnMainThread(() => ReadConfigFile());
        }

        /// <summary>
        /// Copies the deafult config file and overwrites the current one.
        /// </summary>
        public void ResetToDefault()
        {
            File.Copy(sampleConfigPath, configPath, true);
            ReadConfigFile();
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

            FileStream fileStream = new FileStream(configPath, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            CellexalConfig.Config = (Config)serializer.Deserialize(streamReader);
            streamReader.Close();
            fileStream.Close();
            CellexalEvents.ConfigLoaded.Invoke();

        }

        public void SaveConfigFile()
        {
            if (!Directory.Exists("Config"))
            {
                Directory.CreateDirectory("Config");
                CellexalLog.Log("Created directory " + CellexalLog.FixFilePath(configDir));
            }

            if (!File.Exists(configPath))
            {
                File.Create(configPath);
            }
            CellexalLog.Log("Started saving the config file");

            FileStream fileStream = new FileStream(configPath, FileMode.Create, FileAccess.Write, FileShare.None);
            StreamWriter streamWriter = new StreamWriter(fileStream);

            XmlSerializer ser = new XmlSerializer(typeof(Config));
            ser.Serialize(streamWriter, CellexalConfig.Config);
            streamWriter.Close();
            fileStream.Close();

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
}