﻿using CellexalVR.AnalysisObjects;
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

        //private Material correlatedGenesNormalMaterial;
        //private Material correlatedGenesHighlightMaterial;
        //private Material correlatedGenesPressedMaterial;


        protected override void Start()
        {
            base.Start();
            correlatedGenesList = referenceManager.correlatedGenesList;
            this.tag = "Keyboard";
            CellexalEvents.CorrelatedGenesCalculated.AddListener(Reset);
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
            base.Click();
            // the gene name is followed by some other text
            SetPressed(true);
            correlatedGenesList.CalculateCorrelatedGenes(listNode, listNode.Type);
            referenceManager.multiuserMessageSender.SendMessageCalculateCorrelatedGenes(listNode.NameOfThing);
        }

        private void Reset()
        {
            SetPressed(false);
        }
    }
}
