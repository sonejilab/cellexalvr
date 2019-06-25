using UnityEngine;
using System.Collections;
using CellexalVR.AnalysisObjects;

namespace CellexalVR.Menu.Buttons
{
    public class ClearColoursButton : CellexalButton
    {
        private GraphManager graphManager;

        protected override string Description
        {
            get
            {
                return "Clear colours but keep selections";
            }
        }

        public override void Click()
        {
            graphManager.ClearExpressionColours();
        }

        // Use this for initialization
        void Start()
        {
            graphManager = referenceManager.graphManager;
        }

    }

}