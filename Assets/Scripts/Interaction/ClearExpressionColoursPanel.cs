using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// 
    /// </summary>
    public class ClearExpressionColoursPanel : ClickablePanel
    {
        public TMPro.TextMeshPro text;

        private GraphManager graphManager;

        protected override void Start()
        {
            base.Start();
            graphManager = referenceManager.graphManager;
        }


        /// <summary>
        /// Click this panel, reset graphs but keep selection groups.
        /// </summary>
        public override void Click()
        {
            base.Click();
            referenceManager.multiuserMessageSender.SendMessageClearExpressionColours();
            graphManager.ClearExpressionColours();
        }
    }
}