using UnityEngine;
using CellexalVR.General;
using VRTK;
using CellexalVR.Menu;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Make events for click on radial menu object since prefab does not keep references to other objects when building the scene.
    /// Depending on the object this script is attached to the events differ. The method names should be explanatory.
    /// </summary>
    public class EventSetter : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        private PushBack pushBack;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            pushBack = GetComponentInParent<PushBack>();
        }

        public void LeftMenuLeftClickEvent()
        {
            referenceManager.menuRotator.RotateRight(1);
        }

        public void LeftMenuRightClickEvent()
        {
            referenceManager.menuRotator.RotateLeft(1);
        }

        public void LeftMenuUpClickEvent()
        {
            referenceManager.teleportLaser.SetActive(!referenceManager.teleportLaser.activeSelf);
            //if (referenceManager.leftController.GetComponent<MenuToggler>().MenuActive)
            //{
            //    referenceManager.leftController.GetComponent<MenuToggler>().ToggleMenu();
            //}
        }

        public void LeftMenuDownClickEvent()
        {
            referenceManager.controllerModelSwitcher.SwitchSelectionToolMesh(false);
        }


        public void RightMenuLeftClickEvent()
        {
            referenceManager.selectionToolCollider.ChangeColor(false);
        }

        public void RightMenuRightClickEvent()
        {
            referenceManager.selectionToolCollider.ChangeColor(true);

        }

        public void RightMenuUpClickEvent()
        {
            if (referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.SelectionTool)
            {
                referenceManager.controllerModelSwitcher.SwitchSelectionToolMesh(true);
            }
        }

        public void RightMenuUpHoldEvent()
        {
            if (referenceManager.rightLaser.isActiveAndEnabled)
            {
                pushBack.Push();
            }
        }

        public void RightMenuDownClickEvent()
        {
            if (referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.SelectionTool)
            {
                referenceManager.controllerModelSwitcher.SwitchSelectionToolMesh(false);
            }
        }

        public void RightMenuDownHoldEvent()
        {
            if (referenceManager.rightLaser.isActiveAndEnabled)
            {
                pushBack.Pull();
            }
        }

        public void GeneKeyboardEnterEvent(string geneName)
        {
            referenceManager.cellManager.ColorGraphsByGene(geneName);
            referenceManager.gameManager.InformColorGraphsByGene(geneName);
        }

        public void GeneKeyboardEditEvent(string s)
        {
            referenceManager.gameManager.InformKeyClicked(s);
        }


        public void BrowserKeyboardEditEvent(string s)
        {
            referenceManager.gameManager.InformBrowserKeyClicked(s);
        }

        public void FolderKeyboardEditEvent(string filter)
        {
            referenceManager.inputFolderGenerator.GenerateFolders(filter);
        }

    }
}




