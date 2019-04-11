using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// This class represents a button that can choose between a <see cref="GraphManager.GeneExpressionColoringMethods"/>
    /// </summary>
    public class ColoringOptionsPanel : ClickablePanel
    {
        public GraphManager.GeneExpressionColoringMethods modeToSwitchTo;

        private GraphManager graphManager;

        protected override void Start()
        {
            base.Start();
            graphManager = referenceManager.graphManager;
        }

        private void OnValidate()
        {
            if (gameObject.activeInHierarchy)
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }

        /// <summary>
        /// Click this panel, changing the mode of coloring used.
        /// </summary>
        public override void Click()
        {
            graphManager.GeneExpressionColoringMethod = modeToSwitchTo;
            // set all other texts to white and ours to green
            foreach (TextMesh textMesh in transform.parent.gameObject.GetComponentsInChildren<TextMesh>())
            {
                textMesh.color = Color.white;
            }
            transform.parent.GetComponentInChildren<TextMesh>().color = Color.green;
        }
    }
}