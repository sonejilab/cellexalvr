using UnityEngine;

public class RemoveExpressedCellsButton : MonoBehaviour
{
    public TextMesh descriptionText;
    public SteamVR_TrackedObject rightController;
    public Sprite standardTexture;
    public Sprite highlightedTexture;
    public CellManager cellManager;
    private SteamVR_Controller.Device device;
    private bool controllerInside;
    private SpriteRenderer spriteRenderer;

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
            cellManager.RemoveExpressedCells();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Controller")
        {
            descriptionText.text = "Toggle the cells with some expression";
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
