using UnityEngine;
using System.Collections;

namespace CellexalVR.Menu.Buttons
{

    public class FilterBoardToggleButton : CellexalVR.Menu.Buttons.CellexalButton
    {
        protected override string Description => "Toggle filter creator board";

        public override void Click()
        {
            referenceManager.filterBlockBoard.SetActive(!referenceManager.filterBlockBoard.activeSelf);
        }

    }
}
