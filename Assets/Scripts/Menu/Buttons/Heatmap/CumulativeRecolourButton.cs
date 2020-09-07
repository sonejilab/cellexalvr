using UnityEngine;
using System.Collections;
using CellexalVR.AnalysisObjects;

namespace CellexalVR.Menu.Buttons.Heatmap
{
    public class CumulativeRecolourButton : CellexalButton
    {
        protected override string Description => "Recolours graphs by their cumulative expression of the selected genes";

        public override void Click()
        {
            gameObject.GetComponentInParent<Interaction.HeatmapRaycast>().CumulativeRecolourFromSelection();
        }
    }
}
