using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the sub menu that pops up when the <see cref="ColorByIndexButton"/> is pressed.
/// </summary>
public class GraphFromMarkersMenu : MenuWithTabs
{
    public override void CreateButtons(string[] categoriesAndNames)
    {
        base.CreateButtons(categoriesAndNames);
        for (int i = 0; i < buttons.Count; ++i)
        {
            var b = buttons[i].GetComponent<AddMarkerButton>();
            b.referenceManager = referenceManager;
            //int colorIndex = i % Colors.Length;
            b.SetIndex(names[i]);
            b.parentMenu = this;
        }
    }

}