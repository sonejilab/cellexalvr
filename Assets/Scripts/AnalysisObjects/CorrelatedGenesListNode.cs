using CellexalVR.Interaction;
using UnityEngine;

namespace CellexalVR.AnalysisObjects
{

    /// <summary>
    /// Rrepresents a node in the list of correlated genes.
    /// </summary>
    public class CorrelatedGenesListNode : ClickableTextPanel
    {
        private string label;
        public string GeneName
        {
            get { return label; }
            set { label = value; textMesh.text = value; }
        }

        /// <summary>
        /// Sets the list node's material to a new material.
        /// </summary>
        /// <param name="newMaterial"> The new material. </param>
        public void SetMaterial(Material newMaterial)
        {
            renderer.material = newMaterial;
        }

    }
}