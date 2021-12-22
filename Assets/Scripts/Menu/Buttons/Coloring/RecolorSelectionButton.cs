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
            referenceManager.multiuserMessageSender.SendMessageRecolorSelectionPoints();
        }

        // Use this for initialization
        void Start()
        {
            CellexalEvents.GraphsLoaded.AddListener(TurnOn);
            CellexalEvents.ScarfObjectLoaded.AddListener(TurnOn);
            CellexalEvents.SelectionStarted.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
            CellexalEvents.SelectionCanceled.AddListener(TurnOff);

            selectionManager = referenceManager.selectionManager;
        }

        void TurnOn()
        {
            SetButtonActivated(true);
        }

        void TurnOff()
        {
            SetButtonActivated(false);
        }

    }

}