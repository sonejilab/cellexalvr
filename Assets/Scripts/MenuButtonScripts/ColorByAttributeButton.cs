using UnityEngine;

/// <summary>
/// This class represents a button that colors all graphs according to an attribute.
/// </summary>
public class ColorByAttributeButton : SolidButton
{
    public TextMesh description;

    private CellManager cellManager;
    private string attribute;
    private bool colored = false;

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
}
