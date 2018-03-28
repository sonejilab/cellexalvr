using UnityEngine;
using System;
using CellexalExtensions;

/// <summary>
/// Represents one node in the list of the 10 previous searches.
/// </summary>
public class PreviousSearchesListNode : ClickableTextPanel
{

    public PreviousSearchesListNode nextNode;
    private bool locked;
    public bool Locked
    {
        get
        { return locked; }
        set
        { locked = value; }
    }

    public override void SetText(string name, Definitions.Measurement type)
    {
        base.SetText(name, type);
        if (NameOfThing != "")
        {
            Text += " " + ColoringMethod.ToString();
            textMesh.text = Text;
        }
    }

    /// <summary>
    /// Checks if the list already contains an entry.
    /// </summary>
    /// <param name="name">THe name of the thing in the entry.</param>
    /// <param name="type">The type of the entry.</param>
    /// <param name="coloringMethod">The coloring method that was used.</param>
    /// <returns>True if an entry of this kind was already in the list, false otherwise.</returns>
    [Obsolete("Use PreviousSearchesList.Contains()")]
    public bool Contains(string name, Definitions.Measurement type, GraphManager.GeneExpressionColoringMethods coloringMethod)
    {
        if (nextNode == null)
            return name == NameOfThing && Type == type && ColoringMethod == coloringMethod;
        else if (name == NameOfThing && Type == type && ColoringMethod == coloringMethod)
            return true;
        else
            return nextNode.Contains(name, type, coloringMethod);
    }

    /// <summary>
    /// Updates the list with a new gene name, removing the bottom gene name in the list if it is full.
    /// </summary>
    /// <param name="newGeneName"> The gene name to add to the list. </param>
    /// <returns> The gene name that was removed.</returns>
    [Obsolete("Use PreviousSearchesList.AddEntry()")]
    public string UpdateList(string newGeneName, Definitions.Measurement type, GraphManager.GeneExpressionColoringMethods coloringMethod)
    {
        if (nextNode != null)
        {
            if (!Locked)
            {
                var returnGeneName = nextNode.UpdateList(NameOfThing, Type, coloringMethod);
                this.ColoringMethod = coloringMethod;
                SetText(newGeneName, type);
                return returnGeneName;
            }
            else
            {
                return nextNode.UpdateList(newGeneName, type, coloringMethod);
            }
        }
        else
        {
            if (!Locked)
            {
                var oldGeneName = NameOfThing;
                this.ColoringMethod = coloringMethod;
                SetText(newGeneName, type);
                return oldGeneName;
            }
            else
            {
                return newGeneName;
            }
        }
    }
}
