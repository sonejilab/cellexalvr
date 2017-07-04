using UnityEngine;
using System.Collections;

public class ColorByAttributeButton : MonoBehaviour
{
    public GraphManager graphManager;
    public SteamVR_TrackedObject rightController;
    public Sprite standardTexture;
    public Sprite highlightedTexture;
    private SteamVR_Controller.Device device;
    private SpriteRenderer spriteRenderer;
    private bool controllerInside = false;
    private TextMesh description;
    private string attribute;

    void Start()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = standardTexture;
        description = GetComponentInChildren<TextMesh>();
    }

    void Update()
    {
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            //graphManager.ColorAllGraphsByAttribute(attribute);
        }
    }

    public void SetAttribute(string attribute)
    {
        description.text = attribute;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Controller")
        {
            spriteRenderer.sprite = highlightedTexture;
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Controller")
        {
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
        }
    }
}

