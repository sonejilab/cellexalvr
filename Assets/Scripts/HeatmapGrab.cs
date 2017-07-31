namespace VRTK.GrabAttachMechanics
{

    using UnityEngine;

    /// <summary>
    /// This class is used with VRTK's interaction system when a heatmap is grabbed.
    /// It hides the menu and any tool active while the heatmap is grabbed.
    /// </summary>
    public class HeatmapGrab : VRTK_BaseJointGrabAttach
    {

        public float breakForce = 1500f;
        GameObject menu;
        private ControllerModelSwitcher menuController;
        private bool menuTurnedOff = false;

        void Start()
        {

        }

        protected override void CreateJoint(GameObject obj)
        {
            givenJoint = obj.AddComponent<FixedJoint>();
            givenJoint.breakForce = (grabbedObjectScript.IsDroppable() ? breakForce : Mathf.Infinity);
            base.CreateJoint(obj);
            if (menu.activeSelf)
            {
                menuTurnedOff = true;
                menu.SetActive(false);
                menuController.SwitchToModel(ControllerModelSwitcher.Model.Normal);
            }
            else
            {
                menuTurnedOff = false;
            }
        }

        protected override void DestroyJoint(bool withDestroyImmediate, bool applyGrabbingObjectVelocity)
        {
            base.DestroyJoint(withDestroyImmediate, applyGrabbingObjectVelocity);
            menu.SetActive(true);
            if (!menuTurnedOff)
            {
                menu.SetActive(false);
            }
            else
            {
                menuController.SwitchToDesiredModel();
            }
        }

    }

}
