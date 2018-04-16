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
        CellexalEvents.GraphsColoredByGene.AddListener(ResetVars);
        CellexalEvents.GraphsReset.AddListener(ResetVars);
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
    /// <param name="attribute"> The name of the attribute. </param>
    /// <param name="color"> The color that the cells in possesion of the attribute should get. </param>
    public void SetAttribute(string attribute, Color color)
    {
        SetAttribute(attribute, attribute, color);
    }

    /// <summary>
    /// Sets which attribute this button should show when pressed.
    /// </summary>
    /// <param name="attribute">The name of the attribute.</param>
    /// <param name="displayedName">The text that should be displayed on the button.</param>
    /// <param name="color">The color that the cells in possesion of the attribute should get.</param>
    public void SetAttribute(string attribute, string displayedName, Color color)
    {
        if (displayedName.Length > 8)
        {
            string[] shorter = { displayedName.Substring(0, displayedName.Length / 2), displayedName.Substring(displayedName.Length / 2) };
            description.text = shorter[0] + "\n" + shorter[1];
        }
        else
        {
            description.text = displayedName;
        }
        this.attribute = attribute;
        // sometimes this is done before Awake() it seems, so we use GetComponent() here
        GetComponent<Renderer>().material.color = color;
        meshStandardColor = color;
    }

    private void ResetVars()
    {
        colored = false;
    }
}
