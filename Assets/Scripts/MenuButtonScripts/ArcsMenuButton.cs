using UnityEngine;
using System.Collections;

public class ArcsMenuButton : MonoBehaviour
{

    public ReferenceManager referenceManager;

    public Sprite standardTexture;
    public Sprite highlightedTexture;
    private TextMesh descriptionText;
    private GameObject buttons;
    private GameObject arcsMenu;
    private SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
    private SpriteRenderer spriteRenderer;
    private bool controllerInside = false;

    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = standardTexture;
        descriptionText = referenceManager.backDescription;
        buttons = referenceManager.backButtons;
        arcsMenu = referenceManager.arcsSubMenu.gameObject;
        rightController = referenceManager.rightController;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
            arcsMenu.SetActive(true);
            buttons.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Controller")
        {
            descriptionText.text = "Toggle arcs";
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

