using UnityEngine;
using System.Collections;

public class ColorByAttributeButton : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public Sprite standardTexture;
    public Sprite highlightedTexture;

    private CellManager cellManager;
    private SteamVR_TrackedObject rightController;
    private TextMesh description;
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

    private void Start()
    {
        cellManager = referenceManager.cellManager;
        rightController = referenceManager.rightController;
        description = referenceManager.leftDescription;
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
        if (description == null)
        {
            description = referenceManager.leftDescription;
        }
        description.text = attribute;
        this.attribute = attribute;
        // sometimes this is done before Awake() it seems, so we use GetComponent() here
        GetComponent<Renderer>().material.color = color;
        this.color = color;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
        {
            renderer.material.color = Color.white;
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
        {
            renderer.material.color = color;
            controllerInside = false;
        }
    }
}

