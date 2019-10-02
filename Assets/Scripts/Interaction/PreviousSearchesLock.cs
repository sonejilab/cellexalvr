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
                keyNormalMaterial = lockedNormalMaterial;
                keyHighlightMaterial = lockedHighlightMaterial;
                keyPressedMaterial = lockedPressedMaterial;

                renderer.sharedMaterial = lockedNormalMaterial;
            }
            else
            {
                keyNormalMaterial = unlockedNormalMaterial;
                keyHighlightMaterial = unlockedHighlightMaterial;
                keyPressedMaterial = unlockedPressedMaterial;

                renderer.sharedMaterial = unlockedNormalMaterial;
            }

        }

        /// <summary>
        /// Do not use. Use <see cref="SetMaterials(Material, Material, Material, Material, Material, Material)"/>
        /// </summary>
        public override void SetMaterials(Material keyNormalMaterial, Material keyHighlightMaterial, Material keyPressedMaterial, Vector4 scaleCorrection)
        {
            //throw new System.InvalidOperationException("Use the other SetMaterial method");
        }

        /// <summary>
        /// Set the materials used by this lock.
        /// </summary>
        public void SetMaterials(Material unlockedNormalMaterial, Material unlockedHighlightMaterial, Material unlockedPressedMaterial,
            Material lockedNormalMaterial, Material lockedHighlightMaterial, Material lockedPressedMaterial,
            Vector4 scaleCorrection)
        {
            this.unlockedNormalMaterial = unlockedNormalMaterial;
            this.unlockedHighlightMaterial = unlockedHighlightMaterial;
            this.unlockedPressedMaterial = unlockedPressedMaterial;
            this.lockedNormalMaterial = lockedNormalMaterial;
            this.lockedHighlightMaterial = lockedHighlightMaterial;
            this.lockedPressedMaterial = lockedPressedMaterial;
            unlockedNormalMaterial.SetVector("_ScaleCorrection", scaleCorrection);
            unlockedHighlightMaterial.SetVector("_ScaleCorrection", scaleCorrection);
            unlockedPressedMaterial.SetVector("_ScaleCorrection", scaleCorrection);
            lockedNormalMaterial.SetVector("_ScaleCorrection", scaleCorrection);
            lockedHighlightMaterial.SetVector("_ScaleCorrection", scaleCorrection);
            lockedPressedMaterial.SetVector("_ScaleCorrection", scaleCorrection);


            keyNormalMaterial = unlockedNormalMaterial;
            keyHighlightMaterial = unlockedHighlightMaterial;
            keyPressedMaterial = unlockedPressedMaterial;

            if (renderer)
            {
                renderer.sharedMaterial = unlockedNormalMaterial;
            }
        }

    }
}
