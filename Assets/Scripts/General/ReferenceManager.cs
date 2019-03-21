using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.Interaction;
using CellexalVR.Menu.SubMenus;
using CellexalVR.Tools;
using CellexalVR.SceneObjects;
using CurvedVRKeyboard;
using TMPro;
using UnityEngine;
using VRTK;
using SQLiter;

namespace CellexalVR.General
{
    /// <summary>
    /// This class just holds a lot of references to other scripts and gameobjects so they won't clutter the inspector so much.
    /// </summary>
    public class ReferenceManager : MonoBehaviour
    {
        #region Controller things
        [Header("Controller things")]
        public SteamVR_TrackedObject rightController;
        public SteamVR_TrackedObject leftController;
        public ControllerModelSwitcher controllerModelSwitcher;
        //public GroupInfoDisplay groupInfoDisplay;
        //public StatusDisplay statusDisplay;
        //public StatusDisplay statusDisplayHUD;
        //public StatusDisplay statusDisplayFar;
        //public GameObject HUD;
        //public GameObject FarDisplay;
        public TextMeshProUGUI HUDFlashInfo;
        public TextMeshProUGUI HUDGroupInfo;
        public TextMeshProUGUI FarFlashInfo;
        public TextMeshProUGUI FarGroupInfo;
        public GameObject headset;
        public BoxCollider controllerMenuCollider;
        public LaserPointerController rightLaser;
        public VRTK_StraightPointerRenderer leftLaser;
        public LaserPointerController laserPointerController;

        #endregion

        #region Tools
        [Header("Tools")]
        public SelectionToolHandler selectionToolHandler;
        public GameObject deleteTool;
        public MinimizeTool minimizeTool;
        public GameObject helpMenu;
        public DrawTool drawTool;
        public GameObject webBrowser;

        #endregion

        #region Menu
        [Header("Menu")]
        public GameObject mainMenu;
        public ToggleArcsSubMenu arcsSubMenu;
        public AttributeSubMenu attributeSubMenu;
        public ColorByIndexMenu indexMenu;
        public GraphFromMarkersMenu createFromMarkerMenu;
        public SelectionFromPreviousMenu selectionFromPreviousMenu;
        public ColorByGeneMenu colorByGeneMenu;
        public FilterMenu filterMenu;
        public GameObject selectionMenu;
        public TextMesh currentFlashedGeneText;
        public GameObject frontButtons;
        public GameObject rightButtons;
        public GameObject backButtons;
        public GameObject leftButtons;
        public TextMesh frontDescription;
        public TextMesh rightDescription;
        public TextMesh backDescription;
        public TextMesh leftDescription;
        public MenuRotator menuRotator;
        public MinimizedObjectHandler minimizedObjectHandler;
        public MenuToggler menuToggler;

        #endregion

        #region Managers, generators and things
        [Header("Managers, generators and things")]
        public GraphManager graphManager;
        public CellManager cellManager;
        public HeatmapGenerator heatmapGenerator;
        public NetworkGenerator networkGenerator;
        public GraphGenerator graphGenerator;
        public InputFolderGenerator inputFolderGenerator;
        public LoaderController loaderController;
        public ConfigManager configManager;
        public GameObject helperCylinder;
        public InputReader inputReader;
        public SQLite database;
        public LogManager logManager;
        public GameManager gameManager;
        public GameObject calculatorCluster;
        public ConsoleManager consoleManager;
        public TurnOffThoseLights turnOffThoseLights;
        public GameObject fpsCounter;
        public DemoManager demoManager;
        public NewGraphFromMarkers newGraphFromMarkers;
        #endregion

        #region GeneKeyboard
        [Header("Keyboards")]
        public KeyboardHandler keyboardHandler;
        public KeyboardSwitch keyboardSwitch;
        public CorrelatedGenesList correlatedGenesList;
        public PreviousSearchesList previousSearchesList;
        public AutoCompleteList autoCompleteList;
        public KeyboardHandler folderKeyboard;

        #endregion

        #region SettingsMenu
        [Header("Settings Menu")]
        public SettingsMenu settingsMenu;
        public ColorPicker colorPicker;
        #endregion

    }
}