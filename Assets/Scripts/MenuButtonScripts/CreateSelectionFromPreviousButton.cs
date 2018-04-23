using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the buttons that are used to create new selections from old ones.
/// </summary>
class CreateSelectionFromPreviousButton : CellexalButton
{

    public TextMesh buttonDescription;
    private string graphName;
    private string[] selectionCellNames;
    private int[] selectionGroups;
    private Dictionary<int, Color> groupingColors;

    protected override string Description
    {
        get { return "Create a selection from a previous selection"; }
    }


    private void Start()
    {
        rightController = referenceManager.rightController;
    }

    protected override void Click()
    {
        referenceManager.cellManager.CreateNewSelection(graphName, selectionCellNames, selectionGroups, groupingColors);
    }

    /// <summary>
    /// Set which selection this button represents.
    /// </summary>
    /// <param name="graphName"> Which graph the selection originated from. </param>
    /// <param name="selectionName"> The name of this selection. </param>
    /// <param name="selectionCellNames"> An array containing the cell names. </param>
    /// <param name="selectionGroups"> An array containing which groups the cells belonged to. </param>
    public void SetSelection(string graphName, string selectionName, string[] selectionCellNames, int[] selectionGroups, Dictionary<int, Color> groupingColors)
    {
        buttonDescription.text = selectionName;
        this.graphName = graphName;
        this.selectionCellNames = selectionCellNames;
        this.selectionGroups = selectionGroups;
        this.groupingColors = groupingColors;
    }
}
