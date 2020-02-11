using CellexalVR.General;
using CellexalVR.Menu.SubMenus;
using UnityEngine;
using VRTK;

namespace CellexalVR.Interaction
{

    /// <summary>
    /// Informs the flyby menu when spheres are grabbed/ungrabbed along the fly path.
    /// </summary>
    public class PreviewSphereInteract : MonoBehaviour
    {
        public int Index { get; set; }

        public VRTK.VRTK_InteractableObject vrtkInteractableObject;
        private ReferenceManager referenceManager;
        private FlybyMenu flybyMenu;

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            flybyMenu = referenceManager.flybyMenu;
            vrtkInteractableObject.InteractableObjectGrabbed += SphereGrabbed;
            vrtkInteractableObject.InteractableObjectUngrabbed += SphereUngrabbed;
        }

        private void Update()
        {
            if (vrtkInteractableObject.IsGrabbed())
            {
                UpdatePosition();
            }
        }

        /// <summary>
        /// Updates the position of this point of the path in the flyby to this gameobject's position.
        /// </summary>
        private void UpdatePosition()
        {
            flybyMenu.UpdatePosition(Index, transform.position, transform.rotation);

        }


        /// <summary>
        /// Called through the <see cref="VRTK.VRTK_InteractableObject.InteractableObjectGrabbed"/> event.
        /// </summary>
        private void SphereGrabbed(object sender, InteractableObjectEventArgs e)
        {
            flybyMenu.SetSphereGrabbed(true, gameObject);
        }

        /// <summary>
        /// Called through the <see cref="VRTK.VRTK_InteractableObject.InteractableObjectUngrabbed"/> event.
        /// </summary>
        private void SphereUngrabbed(object sender, InteractableObjectEventArgs e)
        {
            UpdatePosition();
            flybyMenu.SetSphereGrabbed(false, gameObject);
        }
    }
}
