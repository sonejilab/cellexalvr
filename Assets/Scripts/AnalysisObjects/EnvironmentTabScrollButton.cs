using CellexalVR.Menu.Buttons;
using System.Collections;
using UnityEngine;

namespace CellexalVR.AnalysisObjects
{
    public class EnvironmentTabScrollButton : CellexalButton
    {
        public EnvironmentMenuWithTabs parentMenu;
        [Tooltip("Scrolls through the list of tabs by the specified number.\n" +
            "For example: 1 would scroll 1 index \"forward\" (towards higher indices) through the list, -3 would scroll 3 indices \"backwards\".")]
        public int direction = 1;

        protected override string Description => "More tabs";

        public override void Click()
        {
            parentMenu.ScrollTabs(direction);
        }
    }
}
