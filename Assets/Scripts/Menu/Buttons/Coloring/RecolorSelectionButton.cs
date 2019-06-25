using UnityEngine;
using System.Collections;
using CellexalVR.General;

namespace CellexalVR.Menu.Buttons
{
    public class RecolorSelectionButton : CellexalButton
    {
        private SelectionManager selectionManager;

        protected override string Description
        {
            get
            {
                return "Recolour current selection";
            }
        }

        public override void Click()
        {
            selectionManager.RecolorSelectionPoints();
        }

        // Use this for initialization
        void Start()
        {
            selectionManager = referenceManager.selectionManager;
        }

    }

}