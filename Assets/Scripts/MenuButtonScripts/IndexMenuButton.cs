using UnityEngine;
using System.Collections;

public class IndexMenuButton : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public Sprite standardTexture;
    public Sprite highlightedTexture;

    private TextMesh descriptionText;
    private GameObject indexMenu;
    private SteamVR_TrackedObject rightController;
    private GameObject buttons;
    private SteamVR_Controller.Device device;
    private SpriteRenderer spriteRenderer;
    private bool controllerInside = false;

    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = standardTexture;
        descriptionText = referenceManager.leftDescription;
        indexMenu = referenceManager.indexMenu.gameObject;
        rightController = referenceManager.rightController;
        buttons = referenceManager.leftButtons;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            spriteRenderer.sprite = standardTexture;
            descriptionText.text = "";
            controllerInside = false;
            indexMenu.SetActive(true);
            buttons.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            descriptionText.text = "Color by index";
            spriteRenderer.sprite = highlightedTexture;
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            descriptionText.text = "";
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
        }
    }
}

