using CellexalExtensions;
using UnityEngine;

/// <summary>
/// Represents a button that colors all graphs according to an attribute.
/// </summary>
public class ColorByAttributeButton : CellexalButton
{
    public TextMesh description;

    private CellManager cellManager;
    public Color booleanNotIncludedColor;
    public Color booleanAndColor;
    public Color booleanNotColor;
    public string Attribute { get; set; }
    private bool colored = false;

    public enum Mode { SINGLE, BOOLEAN_EXPR }
    public Mode CurrentMode { get; set; } = Mode.SINGLE;
    public BooleanLogic CurrentBooleanExpressionState { get; private set; } = BooleanLogic.NOT_INCLUDED;
    public AttributeSubMenu parentMenu;

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

    protected override void Click()
    {
        if (CurrentMode == Mode.SINGLE)
        {
            cellManager.ColorByAttribute(Attribute, !colored);
            colored = !colored;
        }
        else if (CurrentMode == Mode.BOOLEAN_EXPR)
        {
            if (CurrentBooleanExpressionState == BooleanLogic.NOT_INCLUDED)
            {
                CurrentBooleanExpressionState = BooleanLogic.AND;
                meshRenderer.material.color = booleanAndColor;
            }
            else if (CurrentBooleanExpressionState == BooleanLogic.AND)
            {
                CurrentBooleanExpressionState = BooleanLogic.NOT;
                meshRenderer.material.color = booleanNotColor;
            }
            else if (CurrentBooleanExpressionState == BooleanLogic.NOT)
            {
                CurrentBooleanExpressionState = BooleanLogic.NOT_INCLUDED;
                meshRenderer.material.color = booleanNotIncludedColor;
            }

            parentMenu.EvaluateExpression();
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
        this.Attribute = attribute;
        // sometimes this is done before Awake() it seems, so we use GetComponent() here
        GetComponent<Renderer>().material.color = color;
        meshStandardColor = color;
    }

    public void SwitchMode()
    {
        if (CurrentMode == Mode.BOOLEAN_EXPR)
        {
            CurrentMode = Mode.SINGLE;
            meshRenderer.material.color = meshStandardColor;
        }
        else if (CurrentMode == Mode.SINGLE)
        {
            CurrentMode = Mode.BOOLEAN_EXPR;
            ColorButtonBooleanExpression();
        }
    }

    public override void SetHighlighted(bool highlight)
    {
        if (highlight)
        {
            meshRenderer.material.color = meshHighlightColor;
        }
        else
        {
            if (CurrentMode == Mode.BOOLEAN_EXPR)
            {
                ColorButtonBooleanExpression();
            }
            else if (CurrentMode == Mode.SINGLE)
            {
                meshRenderer.material.color = meshStandardColor;
            }
        }
    }

    private void ColorButtonBooleanExpression()
    {
        if (CurrentBooleanExpressionState == BooleanLogic.AND)
        {
            meshRenderer.material.color = booleanAndColor;
        }
        else if (CurrentBooleanExpressionState == BooleanLogic.NOT)
        {
            meshRenderer.material.color = booleanNotColor;
        }
        else if (CurrentBooleanExpressionState == BooleanLogic.NOT_INCLUDED)
        {
            meshRenderer.material.color = booleanNotIncludedColor;
        }
    }

    private void ResetVars()
    {
        colored = false;
    }
}
