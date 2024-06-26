﻿using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using CellexalVR.Menu.SubMenus;

namespace CellexalVR.Menu.Buttons.Attributes
{
    /// <summary>
    /// Toggles all the attributes. Called as a couroutine since if you have a big dataset with many attributes it takes too long.
    /// </summary>
    public class SelectAllAttributesButton : CellexalButton
    {
        public AttributeSubMenu attributeSubMenu;
        public CloseMenuButton closeMenuButton;

        private bool toggle = true;

        protected override string Description
        {
            get { return "Toggle All"; }
        }

        protected void Start()
        {
            SetButtonActivated(true);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
            CellexalEvents.GraphsLoaded.AddListener(TurnOn);
        }

        public override void Click()
        {
            if (ScarfManager.instance.scarfActive)
            {
                StartCoroutine(ScarfManager.instance.GetCellValues("RNA_leiden_cluster"));
            }
            else
            {
                StartCoroutine(attributeSubMenu.SelectAllAttributesCoroutine(toggle));
                toggle = !toggle;
            }
        }

        private void TurnOn()
        {
            SetButtonActivated(true);
        }

        private void TurnOff()
        {
            SetButtonActivated(false);
            spriteRenderer.sprite = deactivatedTexture;
        }
    }
}