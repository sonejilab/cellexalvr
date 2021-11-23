using UnityEngine;

namespace CellexalVR.Menu.Buttons.Heatmap
{
    /// <summary>
    /// Represents the button that saves the heatmap as an image. If user wants to create a report
    /// the image is included in it.
    /// </summary>
    public class SaveHeatmapButton : CellexalButton
    {
        public Sprite doneTex;
        protected override string Description
        {
            get { return "Save Heatmap Image To Disk"; }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Click()
        {
            GetComponentInParent<CellexalVR.AnalysisObjects.Heatmap>().SaveImage();
            rightController.SendHapticImpulse(0.8f, 0.3f);
        }

        public void FinishedButton()
        {
            spriteRenderer.sprite = doneTex;
        }
    }
}