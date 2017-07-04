using UnityEngine;
using System.Collections;

public class CloseAttributeMenuButton : MonoBehaviour
{
    public TextMesh descriptionText;
    public GameObject attributeMenu;
    public SteamVR_TrackedObject rightController;
    public Sprite standardTexture;
    public Sprite highlightedTexture;
    private SteamVR_Controller.Device device;
    private SpriteRenderer spriteRenderer;
    private bool controllerInside = false;

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
            attributeMenu.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Controller")
        {
            descriptionText.text = "Close menu";
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

