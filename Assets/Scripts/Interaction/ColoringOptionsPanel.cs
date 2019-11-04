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
        public TMPro.TextMeshPro text;
        public ColoringOptionsList coloringOptionsList;

        private GraphManager graphManager;

        protected override void Start()
        {
            base.Start();
            graphManager = referenceManager.graphManager;
        }


        /// <summary>
        /// Click this panel, changing the mode of coloring used.
        /// </summary>
        public override void Click()
        {
            referenceManager.multiuserMessageSender.SendMessageColoringMethodChanged((int)modeToSwitchTo);
            coloringOptionsList.SwitchMode(modeToSwitchTo);
        }
    }
}