using CellexalVR.General;
using CellexalVR.Menu.SubMenus;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Interaction
{

    /// <summary>
    /// Informs the flyby menu when spheres are grabbed/ungrabbed along the fly path.
    /// </summary>
    public class PreviewSphereInteract : MonoBehaviour
    {
        public int Index { get; set; }
        public bool checkpoint;

        private InteractableObjectBasic interactableObject;
        private ReferenceManager referenceManager;
        private FlybyMenu flybyMenu;

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            flybyMenu = referenceManager.flybyMenu;
            interactableObject.InteractableObjectGrabbed += SphereGrabbed;
            interactableObject.InteractableObjectUnGrabbed += SphereUngrabbed;
        }

        private void Update()
        {
            if (interactableObject.isGrabbed)
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

        private void SphereGrabbed(object sender, Hand hand)
        {
            flybyMenu.SetSphereGrabbed(true, gameObject, Index, checkpoint);
        }

        private void SphereUngrabbed(object sender, Hand hand)
        {
            UpdatePosition();
            flybyMenu.SetSphereGrabbed(false, gameObject, Index, checkpoint);
        }
    }
}
