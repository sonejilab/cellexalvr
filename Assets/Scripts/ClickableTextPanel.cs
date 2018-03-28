using CellexalExtensions;
using UnityEngine;

/// <summary>
/// A clickable panel that contains some text, typically a gene name.
/// </summary>
public class ClickableTextPanel : ClickablePanel
{
    [HideInInspector]
    public TextMesh textMesh;

    public string Text { get; protected set; }
    public Definitions.Measurement TextType { get; protected set; }

    public string NameOfThing { get; protected set; }

    protected override void Start()
    {
        base.Start();
        textMesh = GetComponentInChildren<TextMesh>();
    }

    /// <summary>
    /// Sets the text of this panel.
    /// </summary>
    /// <param name="name">The new text that should be displayed.</param>
    /// <param name="type">The type (gene, attribute or facs) of the text.</param>
    public virtual void SetText(string name, Definitions.Measurement type)
    {
        TextType = type;
        if (type != Definitions.Measurement.INVALID)
        {
            Text = type.ToString() + " " + name;
            textMesh.text = Text;
            NameOfThing = name;
        }
        else
        {
            Text = "";
            textMesh.text = Text;
            NameOfThing = "";
        }
    }

    public override void Click()
    {
        if (TextType == Definitions.Measurement.GENE)
        {
            referenceManager.cellManager.ColorGraphsByGene(NameOfThing);
        }
        else if (TextType == Definitions.Measurement.ATTRIBUTE)
        {
            referenceManager.cellManager.ColorByAttribute(NameOfThing, true);
        }
        else if (TextType == Definitions.Measurement.FACS)
        {
            referenceManager.cellManager.ColorByIndex(NameOfThing);
        }
    }
}
