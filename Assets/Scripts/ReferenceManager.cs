using CurvedVRKeyboard;
using SQLiter;
using TMPro;
using UnityEngine;
using VRTK;

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
    public GroupInfoDisplay groupInfoDisplay;
    public StatusDisplay statusDisplay;
    public GameObject HUD;
    public GameObject FarDisplay;
    public TextMeshProUGUI HUDFlashInfo;
    public TextMeshProUGUI HUDGroupInfo;
    public StatusDisplay statusDisplayHUD;
    public TextMeshProUGUI FarFlashInfo;
    public TextMeshProUGUI FarGroupInfo;
    public StatusDisplay statusDisplayFar;
    public GameObject headset;
    public BoxCollider controllerMenuCollider;
    public LaserPointerController rightLaser;
    public VRTK_StraightPointerRenderer leftLaser;

    #endregion

    #region Tools
    [Header("Tools")]
    public SelectionToolHandler selectionToolHandler;
    public GameObject deleteTool;
    public MinimizeTool minimizeTool;
    public HelperTool helpTool;
    public GameObject helpMenu;
    public MagnifierTool magnifierTool;
    public DrawTool drawTool;
    public GameObject webBrowser;

    #endregion

    #region Menu
    [Header("Menu")]
    public GameObject mainMenu;
    public FlashGenesMenu flashGenesMenu;
    public ToggleArcsSubMenu arcsSubMenu;
    public AttributeSubMenu attributeSubMenu;
    public ColorByIndexMenu indexMenu;
    public SelectionFromPreviousMenu selectionFromPreviousMenu;
    public ColorByGeneMenu colorByGeneMenu;
    public FilterMenu filterMenu;
    public NewFilterMenu newFilterMenu;
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
    public HeatmapBurner heatmapBurner;
    public NetworkGenerator networkGenerator;
    public InputFolderGenerator inputFolderGenerator;
    public LoaderController loaderController;
    public GameObject helperCylinder;
    public InputReader inputReader;
    public SQLite database;
    public LogManager logManager;
    public GameManager gameManager;
    public GameObject calculatorCluster;
    public ConsoleManager consoleManager;
    #endregion

    #region GeneKeyboard
    [Header("Gene Keyboard")]
    public KeyboardSwitch keyboard;
    public KeyboardStatus keyboardStatus;
    public KeyboardOutput keyboardOutput;
    public CorrelatedGenesList correlatedGenesList;
    public PreviousSearchesList previousSearchesList;
    public PreviousSearchesListNode topListNode;
    public AutoCompleteList autoCompleteList;

    #endregion

    #region FolderKeyboard
    [Header("Folder Keyboard")]
    public KeyboardStatus keyboardStatusFolder;
    public KeyboardOutput keyboardOutputFolder;

    #endregion

}
