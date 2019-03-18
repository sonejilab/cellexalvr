using UnityEngine;
using CellexalVR.General;
namespace CellexalVR.Menu.Buttons.Selection
{
    /// <summary>
    /// Represents the button that opens the menu for coloring graphs based on a previous selection.
    /// </summary>
    class SelectionFromPreviousMenuButton : CellexalButton
    {
        public GameObject menu;
        public GameObject selectionMenu;

        protected override string Description
        {
            get { return "Show menu for selection a previous selection"; }
        }

        void Start()
        {
            menu = referenceManager.selectionFromPreviousMenu.gameObject;
            selectionMenu = referenceManager.selectionMenu;
            SetButtonActivated(false);
            CellexalEvents.GraphsLoaded.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);

        }

        public override void Click()
        {
            foreach (var button in selectionMenu.GetComponentsInChildren<CellexalButton>())
            {
                button.SetButtonActivated(false);
            }
            menu.SetActive(true);
            controllerInside = false;
        }

        private void TurnOn()
        {
            SetButtonActivated(true);
        }

        private void TurnOff()
        {
            SetButtonActivated(false);
        }
    }
}