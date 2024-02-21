using CellexalVR.General;
using UnityEngine;
namespace CellexalVR.Menu.Buttons.Report
{
    /// <summary>
    /// Represents the GO analysis-button. When clicked calls the Rscript doing a GO analysis of the genes on the heatmap.
    /// </summary>
    public class GOanalysisButton : CellexalButton
    {
        public Sprite doneTex;

        protected override string Description
        {
            get { return "Do GO Analysis"; }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Click()
        {
            referenceManager.reportManager.GOanalysis(gameObject.GetComponentInParent<AnalysisObjects.Heatmap>());
            ReferenceManager.instance.rightController.SendHapticImpulse(0.8f, 0.3f);
        }


        public void FinishedButton()
        {
            spriteRenderer.sprite = doneTex;
        }
    }
}