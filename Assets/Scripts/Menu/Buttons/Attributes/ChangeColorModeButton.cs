using CellexalVR.AnalysisLogic;
using CellexalVR.DesktopUI;
using CellexalVR.General;
using CellexalVR.Menu.SubMenus;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Attributes
{
    /// <summary>
    /// Adds the points coloured according to the attributes to a new selection.
    /// </summary>
    public class ChangeColorModeButton : CellexalButton
    {
        public SubMenuButton menuButton;
        public Tab firstTab;

        private bool rainbowColors;
        private SettingsMenu settingsMenu;

        protected override string Description
        {
            get { return "Change colors"; }
        }

        protected void Start()
        {
            settingsMenu = referenceManager.settingsMenu;
            SetButtonActivated(true);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
            CellexalEvents.GraphsLoaded.AddListener(TurnOn);
            CellexalEvents.ScarfObjectLoaded.AddListener(TurnOn);
        }

        public override void Click()
        {
            if (rainbowColors)
            {
                settingsMenu.ResetSettingsToBefore();
                settingsMenu.UpdateSelectionToolColors();

            }
            else
            {
                settingsMenu.GetComponent<DesktopUI.ColorMapManager>().GenerateRainbowColors(CellexalConfig.Config.SelectionToolColors.Length);
                settingsMenu.unsavedChanges = false;
            }
            menuButton.SetMenuActivated(true);
            GetComponentInParent<MenuWithTabs>().TurnOffAllTabs();
            firstTab.SetTabActive(true);
            firstTab.tabButton.SetHighlighted(true);
            
            rainbowColors = !rainbowColors;
            //TurnOff();
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

