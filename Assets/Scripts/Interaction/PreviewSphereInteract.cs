using CellexalVR.General;
using CellexalVR.Menu.SubMenus;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{

    /// <summary>
    /// Informs the flyby menu when spheres are grabbed/ungrabbed along the fly path.
    /// </summary>
    public class PreviewSphereInteract : MonoBehaviour
    {
        public int Index { get; set; }
        public bool checkpoint;

        private XRGrabInteractable interactableObject;
        private ReferenceManager referenceManager;
        private FlybyMenu flybyMenu;

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            flybyMenu = referenceManager.flybyMenu;
            interactableObject.selectEntered.AddListener(SphereGrabbed);
            interactableObject.selectExited.AddListener(SphereUnGrabbed);
        }

        private void Update()
        {
            if (interactableObject.isSelected)
            {
                UpdatePosition();
            }
        }

        /// <summary>
        /// Updates the position of this point of the path in the flyby to this gameobject's position.
        /// </summary>
        private void UpdatePosition()
        {
            flybyMenu.UpdatePosition(Index, transform.position, transform.rotation, checkpoint);
        }


        private void SphereGrabbed(SelectEnterEventArgs args)
        {
            flybyMenu.SetSphereGrabbed(true, gameObject, Index, checkpoint);
        }

        private void SphereUnGrabbed(SelectExitEventArgs args)
        {
            UpdatePosition();
            flybyMenu.SetSphereGrabbed(false, gameObject, Index, checkpoint);
        }

        // OpenXR

        /// <summary>
        /// Called through the <see cref="VRTK.VRTK_InteractableObject.InteractableObjectGrabbed"/> event.
        /// </summary>
        //private void SphereGrabbed(object sender, InteractableObjectEventArgs e)
        //{
        //    flybyMenu.SetSphereGrabbed(true, gameObject, Index, checkpoint);
        //}

        /// <summary>
        /// Called through the <see cref="VRTK.VRTK_InteractableObject.InteractableObjectUngrabbed"/> event.
        /// </summary>
        //private void SphereUngrabbed(object sender, InteractableObjectEventArgs e)
        //{
        //    UpdatePosition();
        //    flybyMenu.SetSphereGrabbed(false, gameObject, Index, checkpoint);
        //}
    }
}
