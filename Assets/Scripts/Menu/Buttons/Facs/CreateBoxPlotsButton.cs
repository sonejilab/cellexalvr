using CellexalVR.AnalysisLogic;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Facs
{
    public class CreateBoxPlotsButton : CellexalButton
    {
        protected override string Description => "Create Boxplots from selection";
        public GameObject boxPlotPrefab;

        public override void Click()
        {
            GameObject newBoxPlots = Instantiate(boxPlotPrefab);
            newBoxPlots.GetComponent<BoxPlotGrid>().referenceManager = referenceManager;
            newBoxPlots.GetComponent<BoxPlotGrid>().GenerateBoxPlots(referenceManager.selectionManager.GetLastSelection());
            newBoxPlots.transform.position = referenceManager.headset.transform.position + referenceManager.headset.transform.forward * 1.5f;
            newBoxPlots.transform.LookAt(referenceManager.headset.transform);
            newBoxPlots.transform.Rotate(0f, 180f, 0f);
        }
    }
}
