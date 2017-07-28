using UnityEngine;

///<summary>
/// This class represents a button used for resetting the input data folders.
///</summary>
public class ResetFolderButton : MonoBehaviour
{

    public TextMesh descriptionText;
    public GraphManager graphManager;
    public InputFolderGenerator inputFolderGenerator;
    public LoaderController loader;
    public SteamVR_TrackedObject rightController;
    public Sprite standardTexture;
    public Sprite highlightedTexture;
    public PreviousSearchesList previousSearchesList;
    private SteamVR_Controller.Device device;
    private SpriteRenderer spriteRenderer;
    private bool controllerInside = false;
    private bool menuActive = false;
    private bool buttonsInitialized = false;

    void Start()
    {

        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = standardTexture;

    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            graphManager.DeleteGraphs();
            previousSearchesList.ClearList();
            // must reset loader before generating new folders
            loader.ResetLoaderBooleans();
            inputFolderGenerator.GenerateFolders();
            if (loader.loaderMovedDown)
            {
                loader.loaderMovedDown = false;
                loader.MoveLoader(new Vector3(0f, 2f, 0f), 2f);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
        {
            descriptionText.text = "Reset folder";
            spriteRenderer.sprite = highlightedTexture;
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
        {
            descriptionText.text = "";
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
        }
    }

}
