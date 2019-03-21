using UnityEngine;
namespace CellexalVR.Interaction
{
    /// <summary>
    /// Represents the locks next to the previous searches.
    /// Pressing one of the locks makes the gene name next to it not move when a new gene is searched for.
    /// </summary>
    public class PreviousSearchesLock : ClickablePanel
    {

        public ClickableTextPanel searchListNode;
        private bool locked;
        public bool Locked
        {
            get { return locked; }

            set
            {
                locked = value;
                if (!renderer)
                    renderer = gameObject.GetComponent<Renderer>();
                renderer.sharedMaterial = (locked ? lockedNormalMaterial : unlockedNormalMaterial);
            }
        }

        private Material unlockedNormalMaterial;
        private Material unlockedHighlightMaterial;
        private Material unlockedPressedMaterial;
        private Material lockedNormalMaterial;
        private Material lockedHighlightMaterial;
        private Material lockedPressedMaterial;

        public override void Click()
        {
            Locked = !Locked;
            if (Locked)
            {
                renderer.sharedMaterial = lockedNormalMaterial;
            }
            else
            {
                renderer.sharedMaterial = unlockedNormalMaterial;
            }

        }

        /// <summary>
        /// Do not use. Use <see cref="SetMaterials(Material, Material, Material, Material, Material, Material)"/>
        /// </summary>
        public override void SetMaterials(Material keyNormalMaterial, Material keyHighlightMaterial, Material keyPressedMaterial)
        {
            throw new System.InvalidOperationException("Use the other SetMaterial method");
        }

        /// <summary>
        /// Set the materials used by this lock.
        /// </summary>
        public void SetMaterials(Material unlockedNormalMaterial, Material unlockedHighlightMaterial, Material unlockedPressedMaterial, Material lockedNormalMaterial, Material lockedHighlightMaterial, Material lockedPressedMaterial)
        {
            this.unlockedNormalMaterial = unlockedNormalMaterial;
            this.unlockedHighlightMaterial = unlockedHighlightMaterial;
            this.unlockedPressedMaterial = unlockedPressedMaterial;
            this.lockedNormalMaterial = lockedNormalMaterial;
            this.lockedHighlightMaterial = lockedHighlightMaterial;
            this.lockedPressedMaterial = lockedPressedMaterial;
        }

        public override void SetHighlighted(bool highlight)
        {
            if (Locked)
            {
                if (highlight)
                    renderer.sharedMaterial = lockedHighlightMaterial;
                else
                    renderer.sharedMaterial = lockedNormalMaterial;
            }
            else
            {
                if (highlight)
                    renderer.sharedMaterial = unlockedHighlightMaterial;
                else
                    renderer.sharedMaterial = unlockedNormalMaterial;
            }
        }
    }
}