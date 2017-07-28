using UnityEngine;

///<summary>
/// This class represents a button used for toggling the burning heatmap tool.
///</summary>
public class BurnHeatmapToolButton : MonoBehaviour
{
    public TextMesh descriptionText;
    public GameObject fire;
    public SteamVR_TrackedObject rightController;
    public Sprite standardTexture;
    public Sprite highlightedTexture;
    public ControllerModelSwitcher menuController;
    private SteamVR_Controller.Device device;
    private SpriteRenderer spriteRenderer;
    private bool controllerInside = false;
    private bool fireActivated = false;

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
            fireActivated = !fireActivated;
            fire.SetActive(fireActivated);
            menuController.ToolSwitched();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //print(other.gameObject.name);
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
        {
            descriptionText.text = "Burn heatmap tool";
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
