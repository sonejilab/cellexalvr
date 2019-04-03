using CellexalVR.Interaction;
using UnityEngine;
namespace CellexalVR.Menu.Buttons.General
{
    ///<summary>
    /// Represents a button used for resetting the input data folders. 
    /// It is spawned on a confirm submenu (which is spawned by the "Loading Menu Sub button" and if it is pressed closes this submenu.
    ///</summary>
    public class ResetFolderButton : CloseMenuButton
    {
        //public GameObject subMenu;
        //public GameObject buttonsToActivate;
        //public GameObject menuToClose;
        //public TextMesh textMeshToUndarken;

        //public bool deactivateMenu = false;
        public bool deleteSceneObjs; // If total reset is to be done. If false don't delete anything, only bring back folders.


        private ControllerModelSwitcher controllerModelSwitcher;

        protected override string Description
        {
            get
            {
                return "Go back to loading a folder";
            }
        }

        private void Start()
        {
            controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        }

        // Reset everything without clicking the button.
        public void Reset()
        {
            Click();
        }

        public override void Click()
        {
            base.Click();
            //if (deleteSceneObjs)
            //{
            //    CloseSubMenu();
            //}
            controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Normal;
            controllerModelSwitcher.ActivateDesiredTool();
            referenceManager.loaderController.ResetFolders(deleteSceneObjs);
            referenceManager.gameManager.InformLoadingMenu(deleteSceneObjs);
        }

        //void CloseSubMenu()
        //{
        //    spriteRenderer.sprite = standardTexture;
        //    controllerInside = false;
        //    descriptionText.text = "";
        //    if (deactivateMenu)
        //    {
        //        menuToClose.SetActive(false);
        //    }
        //    else
        //    {
        //        foreach (Renderer r in menuToClose.GetComponentsInChildren<Renderer>())
        //            r.enabled = false;
        //        foreach (Collider c in menuToClose.GetComponentsInChildren<Collider>())
        //            c.enabled = false;
        //    }
        //    //textMeshToUndarken.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        //    textMeshToUndarken.GetComponent<MeshRenderer>().enabled = true;
        //    buttonsToActivate.GetComponent<CellexalButton>().SetButtonActivated(true);
        //    //foreach (CellexalButton b in buttonsToActivate.GetComponentsInChildren<CellexalButton>())
        //    //{
        //    //    if (b.gameObject.name == "Help Tool Button" || b.gameObject.name == "Web Browser Button")
        //    //    {
        //    //        b.SetButtonActivated(true);
        //    //    }
        //    //}
        //}
    }
}