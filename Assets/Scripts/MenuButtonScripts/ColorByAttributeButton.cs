using UnityEngine;

/// <summary>
/// Represents a button that colors all graphs according to an attribute.
/// </summary>
public class ColorByAttributeButton : CellexalButton
{
    public TextMesh description;

    private CellManager cellManager;
    private string attribute;
    private bool colored = false;

    protected override string Description
    {
        get { return "Color graphs according to this attribute"; }
    }

    protected void Start()
    {
        cellManager = referenceManager.cellManager;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            cellManager.ColorByAttribute(attribute, !colored);
            colored = !colored;
        }
    }

    /// <summary>
    /// Sets which attribute this button should show when pressed.
    /// </summary>
    /// <param name="attribute"> the name of the attribute. </param>
    /// <param name="color"> The color that the cells in possesion of the attribute should get. </param>
    public void SetAttribute(string attribute, Color color)
    {
        string[] shorter = { attribute.Substring(0, 8), attribute.Substring(8) };

        description.text = shorter[0] + "\n" + shorter[1];
        this.attribute = attribute;
        // sometimes this is done before Awake() it seems, so we use GetComponent() here
        GetComponent<Renderer>().material.color = color;
        meshStandardColor = color;
    }
}
