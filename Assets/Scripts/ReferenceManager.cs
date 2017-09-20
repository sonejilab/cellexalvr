using SQLiter;
using UnityEngine;

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
    public GameObject headset;
    public BoxCollider controllerMenuCollider;

    #endregion

    #region Tools
    [Header("Tools")]
    public SelectionToolHandler selectionToolHandler;
    public GameObject fire;
    public MinimizeTool minimizeTool;
    public HelperTool helpTool;
    public GameObject helpMenu;
    public MagnifierTool magnifierTool;
    public DrawTool drawTool;

    #endregion

    #region Menu
    [Header("Menu")]
    public GameObject mainMenu;
    public ToggleArcsSubMenu arcsSubMenu;
    public AttributeSubMenu attributeSubMenu;
    public ColorByIndexMenu indexMenu;
    public SelectionToolMenu selectionToolMenu;
    public UndoButtonsHandler undoButtonsHandler;
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

    #endregion

    #region Managers, generators and things
    [Header("Managers, generators and things")]
    public GraphManager graphManager;
    public CellManager cellManager;
    public HeatmapGenerator heatmapGenerator;
    public NetworkGenerator networkGenerator;
    public InputFolderGenerator inputFolderGenerator;
    public LoaderController loaderController;
    public GameObject helperCylinder;
    public InputReader inputReader;
    public SQLite database;
    public LogManager logManager;
    public GameManager gameManager;

    #endregion

    #region Keyboard
    [Header("Keyboard")]
    public GameObject keyboard;
    public CorrelatedGenesList correlatedGenesList;
    public PreviousSearchesList previousSearchesList;
    public PreviousSearchesListNode topListNode;

    #endregion

}
