using CellexalVR.Menu.Buttons;
using CellexalVR.Menu.Buttons.Facs;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.Menu.SubMenus
{
    /// <summary>
    /// Represents the sub menu that pops up when the <see cref="NewGraphFromMarkersButton"/> is pressed.
    /// </summary>
    public class GraphFromMarkersMenu : MenuWithTabs
    {
        public override void CreateButtons(string[] categoriesAndNames)
        {
            base.CreateButtons(categoriesAndNames);
            for (int i = 0; i < cellexalButtons.Count; ++i)
            {
                var b = cellexalButtons[i].GetComponent<AddMarkerButton>();
                b.referenceManager = referenceManager;
                //int colorIndex = i % Colors.Length;
                b.SetIndex(names[i]);
                b.parentMenu = this;
                b.gameObject.name = names[i];
            }
        }

        public override CellexalButton FindButton(string name)
        {
            var button = cellexalButtons.Find(x => x.GetComponent<AddMarkerButton>().indexName == name);
            return button;
        }

    }
}