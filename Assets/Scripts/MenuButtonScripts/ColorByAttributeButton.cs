using UnityEngine;
using System.Collections;

public class ColorByAttributeButton : MonoBehaviour
{
    public CellManager cellManager;
    public SteamVR_TrackedObject rightController;
    public Sprite standardTexture;
    public Sprite highlightedTexture;
    public TextMesh description;
    private SteamVR_Controller.Device device;
    private new Renderer renderer;
    private bool controllerInside = false;
    private string attribute;
    private Color color;
    private bool colored = false;

    void Awake()
    {
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (!colored)
            {
                cellManager.ColorByAttribute(attribute, color);
            }
            else
            {
                cellManager.ColorByAttribute(attribute, Color.white);
            }
            colored = !colored;
        }
    }

    public void SetAttribute(string attribute, Color color)
    {
        description.text = attribute;
        this.attribute = attribute;
        // sometimes this is done before Awake() it seems, so we use GetComponent() here
        GetComponent<Renderer>().material.color = color;
        this.color = color;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
        {
            renderer.material.color = Color.white;
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Smaller Controller Collider"))
        {
            renderer.material.color = color;
            controllerInside = false;
        }
    }
}

