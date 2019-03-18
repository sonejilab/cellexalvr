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
            get { return "Do GO analysis"; }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Click()
        {
            gameObject.GetComponentInParent<AnalysisObjects.Heatmap>().GOanalysis();
            device.TriggerHapticPulse(2000);
        }


        public void FinishedButton()
        {
            spriteRenderer.sprite = doneTex;
        }
    }
}