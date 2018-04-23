using UnityEngine;

/// <summary>
/// Represents a button that colors all graphs according to an index.
/// </summary>
public class ColorByIndexButton : CellexalButton
{
    public TextMesh descriptionOnButton;

    private CellManager cellManager;
    private string indexName;

    protected override string Description
    {
        get { return "Color graphs according to this facs measurement"; }
    }

    protected void Start()
    {
        cellManager = referenceManager.cellManager;
    }

    protected override void Click()
    {
        cellManager.ColorByIndex(indexName);
    }

    /// <summary>
    /// Sets which index this button should show when pressed.
    /// </summary>
    /// <param name="indexName"> The name of the index. </param>
    public void SetIndex(string indexName)
    {
        //color = network.GetComponent<Renderer>().material.color;
        //GetComponent<Renderer>().material.color = color;
        meshStandardColor = GetComponent<Renderer>().material.color;
        this.indexName = indexName;
        descriptionOnButton.text = indexName;
    }
}
