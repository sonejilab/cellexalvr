using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using UnityEngine;
namespace CellexalVR.Interaction
{
    /// <summary>
    /// Represents the button that calculates the correlated genes.
    /// </summary>
    public class CorrelatedGenesPanel : ClickablePanel
    {

        public ClickableTextPanel listNode;

        private CorrelatedGenesList correlatedGenesList;

        private Material correlatedGenesNormalMaterial;
        private Material correlatedGenesHighlightMaterial;
        private Material correlatedGenesPressedMaterial;


        protected override void Start()
        {
            correlatedGenesList = referenceManager.correlatedGenesList;
            this.tag = "Keyboard";
        }

        private void OnValidate()
        {
            if (gameObject.activeInHierarchy)
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }

        /// <summary>
        /// Set the materials used by this panel.
        /// </summary>
        //public void SetMaterials(Material correlatedGenesNormalMaterial, Material correlatedGenesHighlightMaterial, Material correlatedGenesPressedMaterial)
        //{
        //    this.correlatedGenesNormalMaterial = correlatedGenesNormalMaterial;
        //    this.correlatedGenesHighlightMaterial = correlatedGenesHighlightMaterial;
        //    this.correlatedGenesPressedMaterial = correlatedGenesPressedMaterial;
        //}

        /// <summary>
        /// Click this panel, calculating the genes correlated to another gene.
        /// </summary>
        public override void Click()
        {
            // the gene name is followed by some other text
            //SetPressed(true);
            correlatedGenesList.CalculateCorrelatedGenes(listNode, listNode.Type);
            referenceManager.gameManager.InformCalculateCorrelatedGenes(listNode.NameOfThing);
        }
    }
}