using UnityEngine;


///<summary>
/// This class represents a button used for toggling the selection tool.
///</summary>
public class SelectionToolButton : MonoBehaviour
{
    public TextMesh descriptionText;
    public SelectionToolHandler selectionToolHandler;
    public SteamVR_TrackedObject rightController;
    public Sprite standardTexture;
    public Sprite highlightedTexture;
    public MenuRotator rotator;
    public SelectionToolMenu selectionToolMenu;

    private SteamVR_Controller.Device device;
    private SpriteRenderer spriteRenderer;
    private bool controllerInside = false;
    private bool menuActive = false;
    private bool buttonsInitialized = false;

    void Start()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = standardTexture;
    }

    void Update()
    {
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            menuActive = !menuActive;
            selectionToolMenu.gameObject.SetActive(menuActive);
            selectionToolHandler.SetSelectionToolEnabled(menuActive);

            if (menuActive && rotator.rotation == 0)
            {
                rotator.RotateLeft();
            }
            if (!buttonsInitialized)
            {
                selectionToolMenu.InitializeButtons();
                buttonsInitialized = true;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Controller")
        {
            descriptionText.text = "Toggle selection tool";
            spriteRenderer.sprite = highlightedTexture;
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Controller")
        {
            descriptionText.text = "";
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
        }
    }
}
