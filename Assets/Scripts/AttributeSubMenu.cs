using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the sub menu that pops up when the <see cref="AttributeMenuButton"/> is pressed.
/// </summary>
public class AttributeSubMenu : DynamicButtonMenu
{
    protected override Color[] Colors
    {
        get { return CellexalConfig.AttributeColors; }
    }

    /// <summary>
    /// Fill the menu with buttons that will color graphs according to attributes when pressed.
    /// </summary>
    /// <param name="names">The names of the attributes.</param>
    public override void CreateButtons(string[] names)
    {
        base.CreateButtons(names);

        // set the names of the attributes after the buttons have been created.
        for (int i = 0; i < buttons.Count; ++i)
        {
            var b = buttons[i].GetComponent<ColorByAttributeButton>();
            b.referenceManager = referenceManager;
            b.SetAttribute(names[i], Colors[i]);
        }
    }
}
